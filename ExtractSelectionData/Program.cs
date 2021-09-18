using Carmen.CastingEngine.Heuristic;
using Carmen.ShowModel;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
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
            var connection = new SqliteConnectionStringBuilder { DataSource = input_db };
            var options = new DbContextOptionsBuilder<ShowContext>().UseSqlite(connection.ConnectionString).Options;
            using var context = new ShowContext(options);
            using var f = new StreamWriter(output_csv);
            var criterias = context.Criterias.InOrder().ToArray();
            var cast_groups = context.CastGroups.ToArray();
            var engine = new HeuristicSelectionEngine(new WeightedSumEngine(criterias), context.AlternativeCasts.ToArray(), context.ShowRoot.CastNumberOrderBy, context.ShowRoot.CastNumberOrderDirection);
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
    }
}
