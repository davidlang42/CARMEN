﻿using Carmen.CastingEngine;
using Carmen.CastingEngine.Allocation;
using Carmen.CastingEngine.Heuristic;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExtractCastingData
{
    class Program
    {
        static void Main(string[] args)
        {
            var input_db = PromptFile("CARMEN database file to extract from?", args.FirstOrDefault() ?? "", true);
            var output_csv = PromptFile("File to save extracted data?", args.Skip(1).FirstOrDefault() ?? Path.GetFileNameWithoutExtension(input_db) + ".csv", false);
            var pairwise = PromptBool("Should each data point be a comparison of 2 applicants?", false);
            var zeroed = pairwise && PromptBool("Should irrelevant fields be excluded/zeroed?", false);
            var polarised = pairwise && PromptBool("Should A_BetterThan_B be polarised?", false);
            var temp_db = Path.GetTempFileName();
            Console.WriteLine($"Copying database to {temp_db}");
            File.Copy(input_db, temp_db, true);
            using var user_chosen = new ShowContext(OptionsFor(input_db));
            using var temp = new ShowContext(OptionsFor(temp_db));
            Console.WriteLine($"Clearing casting in copied database");
            ClearCasting(temp);
            temp.SaveChanges();
            Console.WriteLine($"Re-casting to record previously made decisions");
            using var f = new StreamWriter(output_csv);
            RecastPreviousCasting(user_chosen, temp, f, pairwise, polarised, zeroed);
            Console.WriteLine("########## COMPLETE ##########");
        }

        private static DbContextOptions<ShowContext> OptionsFor(string filename)
        {
            var connection = new SqliteConnectionStringBuilder { DataSource = filename };
            return new DbContextOptionsBuilder<ShowContext>().UseSqlite(connection.ConnectionString).Options;
        }

        private static string PromptFile(string prompt, string default_value, bool existing_file)
        {
            while (true)
            {
                Console.Write($"{prompt} [{default_value}] ");
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                    input = default_value;
                if (!existing_file && File.Exists(input))
                    Console.WriteLine("File already exists, choose a new file.");
                else if (existing_file && !File.Exists(input))
                    Console.WriteLine("File does not exist, choose an existing file.");
                else
                    return input;
            }
        }

        private static bool PromptBool(string prompt, bool default_value)
        {
            while (true)
            {
                Console.Write($"{prompt} [{(default_value ? "Y" : "N")}] ");
                var input = Console.ReadKey();
                Console.WriteLine("");
                if (input.Key == ConsoleKey.Enter)
                    return default_value;
                bool? result = input.Key switch
                {
                    ConsoleKey.Y => true,
                    ConsoleKey.N => false,
                    ConsoleKey.T => true,
                    ConsoleKey.F => false,
                    _ => null
                };
                if (result.HasValue)
                    return result.Value;
                else
                    Console.WriteLine("Please press Y or T for yes/true, or N or F for no/false.");
            }
        }

        private static void ClearCasting(ShowContext context)
        {
            var roles = context.Nodes.OfType<Item>().SelectMany(i => i.Roles).ToHashSet();
            foreach (var role in roles)
                role.Cast.Clear();
            var applicants = context.Applicants.ToArray();
            foreach (var applicant in applicants)
                applicant.Roles.Clear();
        }

        private static void RecastPreviousCasting(ShowContext previously_cast, ShowContext context, StreamWriter f, bool pairwise, bool polarised, bool zeroed)
        {
            var criterias = context.Criterias.InOrder().ToArray();
            if (pairwise)
                PairwiseHeader(f, criterias, zeroed);
            else
                PointwiseHeader(f, criterias);
            var alternative_casts = context.AlternativeCasts.ToArray();
            var ap_engine = new WeightedSumEngine(criterias);
            var al_engine = new HeuristicAllocationEngine(ap_engine, alternative_casts, criterias);
            var casting_order = al_engine.SimpleCastingOrder(context.ShowRoot).SelectMany(roles => roles).Distinct().ToArray();
            if (casting_order.Distinct().Count() != casting_order.Length)
                throw new ApplicationException("IdealCastingOrder returned duplicate roles.");
            var applicants_in_cast_by_id = new Dictionary<int, Applicant>();
            foreach (var applicant in context.Applicants.Where(a => a.CastGroup != null))
            {
                if (applicant.HasAuditioned(criterias))
                {
                    var valid = true;
                    for (var i = 0; i < criterias.Length; i++)
                    {
                        if (criterias[i].MaxMark == 100 && applicant.MarkFor(criterias[i]) == 1)
                        {
                            Console.WriteLine($"Skipped \"{applicant.FirstName} {applicant.LastName}\" because they had a {criterias[i].Name} mark of 1");
                            valid = false;
                            break;
                        }
                    }
                    if (valid)
                        applicants_in_cast_by_id.Add(applicant.ApplicantId, applicant);
                }
                else
                    Console.WriteLine($"Skipped \"{applicant.FirstName} {applicant.LastName}\" because they haven't auditioned");
            }
            Console.WriteLine($"Found {applicants_in_cast_by_id.Count} valid applicants");
            foreach (var role in casting_order)
            {
                var item = role.Items.First();
                var role_description = $"{item.Name}/{role.Name}";
                if (!role.Requirements.OfType<AbilityRangeRequirement>().Any())
                {
                    Console.WriteLine($"Skipping {role_description} because it has no criteria requirements");
                    continue;
                }
                var picked_applicants_by_castgroup_and_cast = previously_cast // in the database containing the original user choices
                    .Nodes.OfType<Item>().Where(i => i.NodeId == item.NodeId).Single() // find the item this role is in
                    .Roles.Where(r => r.RoleId == role.RoleId).Single() // find this role
                    .Cast.Select(a => a.ApplicantId) // see who was previous cast
                    .Select(id => applicants_in_cast_by_id.TryGetValue(id, out var a) ? a : null) // find them in this database
                    .OfType<Applicant>() // skip if not found (ie. they were invalid for some reason)
                    .GroupBy(a => (a.CastGroup, a.AlternativeCast)) // grouped by cast group & cast
                    .ToDictionary(g => g.Key, g => g.ToHashSet());
                foreach (var ((cast_group, alternative_cast), picked_applicants) in picked_applicants_by_castgroup_and_cast)
                {
                    var not_picked_applicants = applicants_in_cast_by_id.Values // applicants in this database
                        .Where(a => a.CastGroup == cast_group && a.AlternativeCast == alternative_cast) // of the same cast group & cast
                        .Where(a => al_engine.AvailabilityOf(a, role).IsAvailable) // which were (probably) available at the time the user cast this role (assuming the show was cast in the expected order)
                        .Where(a => !picked_applicants.Contains(a)) // which weren't picked
                        .ToArray();
                    var group_description = cast_group.Abbreviation;
                    if (alternative_cast != null)
                        group_description += "/" + alternative_cast.Initial;
                    if (pairwise)
                    {
                        PairwiseExtract(f, picked_applicants, not_picked_applicants, role, criterias, ap_engine, al_engine, polarised, zeroed);
                        Console.WriteLine($"Extracted {picked_applicants.Count*not_picked_applicants.Length} unique pairs of {group_description} for {role_description}");
                    }
                    else
                    {
                        PointwiseExtract(f, picked_applicants, not_picked_applicants, role, criterias, ap_engine, al_engine);
                        Console.WriteLine($"Extracted {picked_applicants.Count} picked and {not_picked_applicants.Length} not picked {group_description} for {role_description}");
                    }
                    foreach (var applicant in picked_applicants)
                    {
                        applicant.Roles.Add(role);
                        role.Cast.Add(applicant);
                    }
                }
            }
        }

        private static void PointwiseHeader(StreamWriter f, Criteria[] criterias)
        {
            f.WriteLine($"Picked,CastGroup,AlternativeCast,RequiredCount,OverallAbility,{string.Join(",", criterias.Select(c => $"Requires_{c.Name},Mark_{c.Name},ExistingCount_{c.Name}"))}");
        }

        private static void PointwiseExtract(StreamWriter f, IEnumerable<Applicant> picked, IEnumerable<Applicant> not_picked, Role role, Criteria[] criterias, IApplicantEngine ap_engine, IAllocationEngine al_engine)
        {
            foreach (var applicant in picked)
                PointwiseRow(f, true, applicant, role, criterias, ap_engine, al_engine);
            foreach (var applicant in not_picked)
                PointwiseRow(f, false, applicant, role, criterias, ap_engine, al_engine);
        }

        private static void PointwiseRow(StreamWriter f, bool picked, Applicant a, Role r, Criteria[] criterias, IApplicantEngine ap_engine, IAllocationEngine al_engine)
        {
            var cast_group = a.CastGroup;
            f.WriteLine($"{(picked ? 1 : 0)},{cast_group.Name},{a.AlternativeCast?.Name},{r.CountFor(cast_group)},{ap_engine.OverallAbility(a)}," +
                $"{string.Join(",", criterias.Select(c => $"{(r.Requirements.OfType<AbilityRangeRequirement>().Where(arr => arr.Criteria == c).Any() ? 1 : 0)},{a.MarkFor(c)},{al_engine.CountRoles(a, c, r)}"))}");
        }

        private static void PairwiseHeader(StreamWriter f, Criteria[] criterias, bool zeroed)
        {
            if (zeroed)
            {
                var primary_criterias = criterias.Where(c => c.Primary).ToArray();
                f.WriteLine($"A_BetterThan_B,RequiredCount," +
                    $"A_OverallAbility,{string.Join(",", primary_criterias.Select(c => $"A_Mark_{c.Name},A_ExistingCount_{c.Name}"))}," +
                    $"B_OverallAbility,{string.Join(",", primary_criterias.Select(c => $"B_Mark_{c.Name},B_ExistingCount_{c.Name}"))}");
            }
            else
                f.WriteLine($"A_BetterThan_B,CastGroup,AlternativeCast,RequiredCount,{string.Join(",",criterias.Select(c => $"Requires_{c.Name}"))}," +
                    $"A_OverallAbility,{string.Join(",", criterias.Select(c => $"A_Mark_{c.Name},A_ExistingCount_{c.Name}"))}," +
                    $"B_OverallAbility,{string.Join(",", criterias.Select(c => $"B_Mark_{c.Name},B_ExistingCount_{c.Name}"))}");
        }

        private static void PairwiseExtract(StreamWriter f, IEnumerable<Applicant> picked, IEnumerable<Applicant> not_picked, Role role, Criteria[] criterias, IApplicantEngine ap_engine, IAllocationEngine al_engine, bool polarised, bool zeroed)
        {
            foreach (var picked_applicant in picked)
                foreach (var not_picked_applicant in not_picked)
                {
                    PairwiseRow(f, 1, picked_applicant, not_picked_applicant, role, criterias, ap_engine, al_engine, zeroed);
                    PairwiseRow(f, polarised ? -1 : 0, not_picked_applicant, picked_applicant, role, criterias, ap_engine, al_engine, zeroed);
                }
        }

        private static void PairwiseRow(StreamWriter f, int a_better_than_b, Applicant a, Applicant b, Role r, Criteria[] criterias, IApplicantEngine ap_engine, IAllocationEngine al_engine, bool zeroed)
        {
            var cast_group = a.CastGroup;
            if (zeroed)
            {
                var primary_criterias = criterias.Where(c => c.Primary).ToArray();
                var weights = primary_criterias.Select(c => r.Requirements.OfType<AbilityRangeRequirement>().Where(arr => arr.Criteria == c).Any() ? 1 : 0).ToArray();
                f.WriteLine($"{a_better_than_b},{r.CountFor(cast_group)}," +
                    $"{ap_engine.OverallAbility(a)},{string.Join(",", primary_criterias.Zip(weights).Select(p => $"{a.MarkFor(p.First) * p.Second},{al_engine.CountRoles(a, p.First, r) * p.Second}"))}," +
                    $"{ap_engine.OverallAbility(b)},{string.Join(",", primary_criterias.Zip(weights).Select(p => $"{b.MarkFor(p.First) * p.Second},{al_engine.CountRoles(b, p.First, r) * p.Second}"))}");
            }
            else
                f.WriteLine($"{a_better_than_b},{cast_group.Name},{a.AlternativeCast?.Name},{r.CountFor(cast_group)},{string.Join(",", criterias.Select(c => r.Requirements.OfType<AbilityRangeRequirement>().Where(arr => arr.Criteria == c).Any() ? 1 : 0))}," +
                    $"{ap_engine.OverallAbility(a)},{string.Join(",", criterias.Select(c => $"{a.MarkFor(c)},{al_engine.CountRoles(a, c, r)}"))}," +
                    $"{ap_engine.OverallAbility(b)},{string.Join(",", criterias.Select(c => $"{b.MarkFor(c)},{al_engine.CountRoles(b, c, r)}"))}");
        }
    }
}