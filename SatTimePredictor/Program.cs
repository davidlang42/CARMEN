using Carmen.CastingEngine.SAT.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SatTimePredictor
{
    class Program
    {
        const int MAX_VARS = 200;
        const int MAX_CLAUSES = 1000;
        const int MAX_LITERALS = 100;

        static void Main(string[] args)
        {
            var filename = PromptFile("Filename to store SAT times?", $"sat_times_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv", false);
            var start_seed = PromptInt("Starting at random seed?", 1);
            var end_seed = PromptInt("Ending at random seed?", start_seed, start_seed);
            var start_n_vars = PromptInt("Starting at N variables?", 1, 1, MAX_VARS);
            var end_n_vars = PromptInt("Ending at N variables?", Math.Min(start_n_vars + 99, MAX_VARS), start_n_vars, MAX_VARS);
            var start_j_clauses = PromptInt("Starting at J clauses?", 1, 1, MAX_CLAUSES);
            var end_j_clauses = PromptInt("Ending at J clauses?", Math.Min(start_j_clauses + 99, MAX_CLAUSES), start_j_clauses, MAX_CLAUSES);
            var start_k_literals = PromptInt("Starting at K literals per clause?", 1, 1, MAX_LITERALS);
            var end_k_literals = PromptInt("Ending at K literals per clause?", Math.Min(start_k_literals + 9, MAX_LITERALS), start_k_literals, MAX_LITERALS);
            var timer = new SatTimer(start_n_vars, end_n_vars, start_j_clauses, end_j_clauses, start_k_literals, end_k_literals);
            var percent_delta = 100.0 / timer.TotalCombinations;
            using var f = new StreamWriter(filename);
            f.WriteLine(SatResult.ToHeader());
            for (var seed = start_seed; seed <= end_seed; seed++)
            {
                var percent = 0.0;
                Console.WriteLine($"########## Starting pass for seed: {seed}/{end_seed} ##########");
                foreach (var result in timer.Run(seed))
                {
                    f.WriteLine(result.ToString());
                    percent += percent_delta;
                    Console.WriteLine($"[{percent:0.00}%] #{seed} ({result.NVariables}n, {result.JClauses}j, {result.KLiterals}k) => {result.SecondsTaken:0.000}s ({result.Solutions} solutions)");
                }
            }
            Console.WriteLine("########## COMPLETE ##########");
        }

        private static string PromptFile(string prompt, string default_value, bool existing_file)
        {
            while (true)
            {
                Console.Write($"{prompt} [{default_value}] ");
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                    return default_value;
                else if (!existing_file && File.Exists(input))
                    Console.WriteLine("File already exists, choose a new file.");
                else if (existing_file && !File.Exists(input))
                    Console.WriteLine("File does not exist, choose an existing file.");
                else
                    return input;
            }
        }

        private static int PromptInt(string prompt, int default_value, int? min = null, int? max = null)
        {
            while (true)
            {
                Console.Write($"{prompt} [{default_value}] ");
                var input = Console.ReadLine();
                if (string.IsNullOrEmpty(input))
                    return default_value;
                else if (!int.TryParse(input, out var result))
                    Console.WriteLine("Input must be an integer.");
                else if (min.HasValue && result < min.Value)
                    Console.WriteLine($"Input must be at least {min}.");
                else if (max.HasValue && result > max.Value)
                    Console.WriteLine($"Input cannot be more than {max}.");
                else
                    return result;
            }
        }
    }
}
