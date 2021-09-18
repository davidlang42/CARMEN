using Carmen.CastingEngine.Heuristic;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExtractSelectionData
{
    class Program
    {
        static void Main(string[] args)
        {
            var input_db = PromptFile("CARMEN database file to extract from?", args.FirstOrDefault() ?? "", true);
            var output_csv = PromptFile("File to save extracted data?", args.Skip(1).FirstOrDefault() ?? Path.GetFileNameWithoutExtension(input_db) + ".csv", false);
            var pairwise = PromptBool("Should each data point be a comparison of 2 applicants?", false);
            var connection = new SqliteConnectionStringBuilder { DataSource = input_db };
            var options = new DbContextOptionsBuilder<ShowContext>().UseSqlite(connection.ConnectionString).Options;
            using var context = new ShowContext(options);
            using var f = new StreamWriter(output_csv);
            if (pairwise)
                PairwiseExtract(context, f);
            else
                PointwiseExtract(context, f);
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

        private static void PointwiseExtract(ShowContext context, StreamWriter f)
        {
            var criterias = context.Criterias.InOrder().ToArray();
            var cast_groups = context.CastGroups.ToArray();
            f.WriteLine($"Accepted,CastGroup,{string.Join(",", criterias.Select(c => c.Name))}");
            var extracted = new int[2];
            foreach (var applicant in context.Applicants)
            {
                if (applicant.HasAuditioned(criterias))
                {
                    var cast_group = applicant.CastGroup ?? cast_groups.FirstOrDefault(cg => cg.Requirements.All(r => r.IsSatisfiedBy(applicant)));
                    if (cast_group != null)
                    {
                        var accepted = applicant.IsAccepted ? 1 : 0;
                        var marks = new uint[criterias.Length];
                        var valid = true;
                        for (var i = 0; i < marks.Length; i++)
                        {
                            marks[i] = applicant.MarkFor(criterias[i]);
                            if (criterias[i].MaxMark == 100 && marks[i] < 10)
                            {
                                Console.WriteLine($"Skipped \"{applicant.FirstName} {applicant.LastName}\" because they had a {criterias[i].Name} mark less than 10");
                                valid = false;
                                break;
                            }
                        }
                        if (valid)
                        {
                            f.WriteLine($"{accepted},{cast_group.Name},{string.Join(",", marks)}");
                            extracted[accepted]++;
                        }
                    }
                    else
                        Console.WriteLine($"Skipped \"{applicant.FirstName} {applicant.LastName}\" because they weren't eligible for any cast group");
                }
                else
                    Console.WriteLine($"Skipped \"{applicant.FirstName} {applicant.LastName}\" because they haven't auditioned");
            }
            Console.WriteLine($"Extracted {extracted[1]} accepted applicants and {extracted[0]} rejected applicants");
        }

        private static void PairwiseExtract(ShowContext context, StreamWriter f)
        {
            var criterias = context.Criterias.InOrder().ToArray();
            var cast_groups = context.CastGroups.ToArray();
            var accepted_applicants = new HashSet<Applicant>[cast_groups.Length];
            var rejected_applicants = new HashSet<Applicant>[cast_groups.Length];
            var cast_group_lookup = new Dictionary<CastGroup, int>();
            for (var i = 0; i < cast_groups.Length; i++)
            {
                accepted_applicants[i] = new HashSet<Applicant>();
                rejected_applicants[i] = new HashSet<Applicant>();
                cast_group_lookup.Add(cast_groups[i], i);
            }
            var extracted_applicants = 0;
            foreach (var applicant in context.Applicants)
            {
                if (applicant.HasAuditioned(criterias))
                {
                    var cast_group = applicant.CastGroup ?? cast_groups.FirstOrDefault(cg => cg.Requirements.All(r => r.IsSatisfiedBy(applicant)));
                    if (cast_group != null)
                    {
                        var valid = true;
                        for (var i = 0; i < criterias.Length; i++)
                        {
                            if (criterias[i].MaxMark == 100 && applicant.MarkFor(criterias[i]) < 10)
                            {
                                Console.WriteLine($"Skipped \"{applicant.FirstName} {applicant.LastName}\" because they had a {criterias[i].Name} mark less than 10");
                                valid = false;
                                break;
                            }
                        }
                        if (valid)
                        {
                            var cast_group_index = cast_group_lookup[cast_group];
                            if (applicant.IsAccepted)
                                accepted_applicants[cast_group_index].Add(applicant);
                            else
                                rejected_applicants[cast_group_index].Add(applicant);
                            extracted_applicants++;
                        }
                    }
                    else
                        Console.WriteLine($"Skipped \"{applicant.FirstName} {applicant.LastName}\" because they weren't eligible for any cast group");
                }
                else
                    Console.WriteLine($"Skipped \"{applicant.FirstName} {applicant.LastName}\" because they haven't auditioned");
            }
            f.WriteLine($"BestCandidate,CastGroup,{string.Join(",", criterias.Select(c => "A_" + c.Name))},{string.Join(",", criterias.Select(c => "B_" + c.Name))}");
            var extracted_pairs = 0;
            for (var i = 0; i < cast_groups.Length; i++)
            {
                Console.WriteLine($"Found {accepted_applicants[i].Count} accepted applicants and {rejected_applicants[i].Count} rejected applicants in '{cast_groups[i].Name}' cast group");
                foreach (var accepted_applicant in accepted_applicants[i])
                    foreach (var rejected_applicant in rejected_applicants[i])
                    {
                        f.WriteLine(ComparisonRow("A", cast_groups[i], accepted_applicant, rejected_applicant, criterias));
                        f.WriteLine(ComparisonRow("B", cast_groups[i], rejected_applicant, accepted_applicant, criterias));
                        extracted_pairs++;
                    }
            }
            Console.WriteLine($"Extracted {extracted_applicants} applicants forming {extracted_pairs} unique pairs ({extracted_pairs*2} data rows)");
        }

        private static string ComparisonRow(string best_candidate, CastGroup cast_group, Applicant a, Applicant b, Criteria[] criterias)
        {
            return $"{best_candidate},{cast_group.Name},{string.Join(",", criterias.Select(c => a.MarkFor(c)))},{string.Join(",", criterias.Select(c => b.MarkFor(c)))}";
        }
    }
}
