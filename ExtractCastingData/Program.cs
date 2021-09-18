using Carmen.CastingEngine;
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
            var temp_db = Path.GetTempFileName();
            File.Copy(input_db, temp_db, true);
            using var user_chosen = new ShowContext(OptionsFor(input_db));
            using var temp = new ShowContext(OptionsFor(temp_db));
            using var f = new StreamWriter(output_csv);
            ClearCasting(temp);
            temp.SaveChanges();
            RecastPreviousCasting(user_chosen, temp, f, pairwise);
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
                if (input.Key == ConsoleKey.Enter)
                    return default_value;
                Console.WriteLine("");
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
            var roles = context.Nodes.OfType<Item>().SelectMany(i => i.Roles).Distinct().ToArray();
            foreach (var role in roles)
                role.Cast.Clear();
        }

        private static void RecastPreviousCasting(ShowContext previously_cast, ShowContext context, StreamWriter f, bool pairwise)
        {
            var criterias = context.Criterias.InOrder().ToArray();
            if (pairwise)
                PairwiseHeader(f, criterias);
            else
                PointwiseHeader(f, criterias);
            var alternative_casts = context.AlternativeCasts.ToArray();
            var ap_engine = new WeightedSumEngine(criterias);
            var al_engine = new HeuristicAllocationEngine(ap_engine, alternative_casts, criterias);
            var casting_order = al_engine.IdealCastingOrder(context.ShowRoot.ItemsInOrder()).SelectMany(roles => roles).Distinct().ToArray();
            if (casting_order.Distinct().Count() != casting_order.Length)
                throw new ApplicationException("IdealCastingOrder returned duplicate roles.");
            var applicants_in_cast_by_id = context.Applicants.Where(a => a.IsAccepted).ToDictionary(a => a.ApplicantId);
            foreach (var role in casting_order)
            {
                //TODO process separately by cast_group and alternative cast
                var picked_applicants = previously_cast // in the database containing the original user choices
                    .Nodes.OfType<Item>().Where(i => i.NodeId == role.Items.First().NodeId).Single() // find the item this role is in
                    .Roles.Where(r => r.RoleId == role.RoleId).Single() // find this role
                    .Cast.Select(a => a.ApplicantId) // see who was previous cast
                    .Select(id => applicants_in_cast_by_id[id]) // find them in this database
                    .ToHashSet();
                var not_picked_applicants = applicants_in_cast_by_id.Values // applicants in this database
                    .Where(a => al_engine.AvailabilityOf(a, role).IsAvailable) // which were (probably) available at the time the user cast this role
                    .Where(a => !picked_applicants.Contains(a)) // which weren't picked
                    .ToArray();
                if (pairwise)
                    PairwiseExtract(f, picked_applicants, not_picked_applicants, role, criterias, ap_engine, al_engine);
                else
                    PointwiseExtract(f, picked_applicants, not_picked_applicants, role, criterias, ap_engine, al_engine);
                foreach (var applicant in picked_applicants)
                {
                    applicant.Roles.Add(role);
                    role.Cast.Add(applicant);
                }
            }
        }

        private static void PointwiseHeader(StreamWriter f, Criteria[] criterias)
        {
            f.WriteLine($"Picked,CastGroup,RequiredCount,OverallAbility,{string.Join(",", criterias.Select(c => $"Requires_{c.Name},Mark_{c.Name},ExistingCount_{c.Name}"))}");
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
            f.WriteLine($"{(picked ? 1 : 0)},{cast_group.Name},{r.CountFor(cast_group)},{ap_engine.OverallAbility(a)}," +
                $"{string.Join(",", criterias.Select(c => $"{(r.Requirements.OfType<AbilityRangeRequirement>().Where(arr => arr.Criteria == c).Any() ? 1 : 0)},{a.MarkFor(c)},{al_engine.CountRoles(a, c, r)}"))}");
        }

        private static void PairwiseHeader(StreamWriter f, Criteria[] criterias)
        {
            f.WriteLine($"BestCandidate,CastGroup,RequiredCount,{string.Join(",",criterias.Select(c => $"Requires_{c.Name}"))}," +
                $"A_OverallAbility,{string.Join(",", criterias.Select(c => $"A_Mark_{c.Name},A_ExistingCount_{c.Name}"))}" +
                $"B_OverallAbility,{string.Join(",", criterias.Select(c => $"B_Mark_{c.Name},B_ExistingCount_{c.Name}"))}");
        }

        private static void PairwiseExtract(StreamWriter f, IEnumerable<Applicant> picked, IEnumerable<Applicant> not_picked, Role role, Criteria[] criterias, IApplicantEngine ap_engine, IAllocationEngine al_engine)
        {
            foreach (var picked_applicant in picked)
                foreach (var not_picked_applicant in not_picked)
                {
                    PairwiseRow(f, "A", picked_applicant, not_picked_applicant, role, criterias, ap_engine, al_engine);
                    PairwiseRow(f, "B", not_picked_applicant, picked_applicant, role, criterias, ap_engine, al_engine);
                }
        }

        private static void PairwiseRow(StreamWriter f, string best_candidate, Applicant a, Applicant b, Role r, Criteria[] criterias, IApplicantEngine ap_engine, IAllocationEngine al_engine)
        {
            var cast_group = a.CastGroup;
            f.WriteLine($"{best_candidate},{cast_group.Name},{r.CountFor(cast_group)},{string.Join(",", criterias.Select(c => r.Requirements.OfType<AbilityRangeRequirement>().Where(arr => arr.Criteria == c).Any() ? 1 : 0))}," +
                $"{ap_engine.OverallAbility(a)},{string.Join(",", criterias.Select(c => $"{a.MarkFor(c)},{al_engine.CountRoles(a, c, r)}"))}" +
                $"{ap_engine.OverallAbility(b)},{string.Join(",", criterias.Select(c => $"{b.MarkFor(c)},{al_engine.CountRoles(b, c, r)}"))}");
        }
    }
}
