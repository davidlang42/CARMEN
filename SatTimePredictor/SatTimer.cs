using Carmen.CastingEngine.SAT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatTimePredictor
{
    class SatTimer
    {
        int nStart, nEnd, jStart, jEnd, kStart, kEnd;

        public int TotalCombinations => (nEnd - nStart + 1) * (jEnd - jStart + 1) * (kEnd - kStart + 1);

        public SatTimer(int start_n_vars, int end_n_vars, int start_j_clauses, int end_j_clauses, int start_k_literals, int end_k_literals)
        {
            this.nStart = start_n_vars;
            this.nEnd = end_n_vars;
            this.jStart = start_j_clauses;
            this.jEnd = end_j_clauses;
            this.kStart = start_k_literals;
            this.kEnd = end_k_literals;
        }

        public IEnumerable<SatResult> Run(int random_seed)
        {
            for (var n = nStart; n <= nEnd; n++)
                for (var j = jStart; j <= jEnd; j++)
                    for (var k = kStart; k <= kEnd; k++)
                        yield return TimeToSolve(random_seed, n, j, k);
        }

        public IEnumerable<SatResult> RunReverse(int random_seed)
        {
            for (var n = nEnd; n >= nStart; n--)
                for (var j = jEnd; j >= jStart; j--)
                    for (var k = kEnd; k >= kStart; k--)
                        yield return TimeToSolve(random_seed, n, j, k);
        }

        private SatResult TimeToSolve(int random_seed, int n_variables, int j_clauses, int k_literals_per_clause)
        {
            var result = new SatResult
            {
                RandomSeed = random_seed,
                NVariables = n_variables,
                JClauses = j_clauses,
                KLiterals = k_literals_per_clause,
            };
            var vars = Enumerable.Range(1, n_variables).ToArray();
            var sat = new DpllAllSolver<int>(vars);
            var expression = GenerateExpression(random_seed, vars, j_clauses, k_literals_per_clause);
            var start = DateTime.Now;
            var solutions = sat.Solve(expression).ToArray();
            var duration = DateTime.Now - start;
            result.SecondsTaken = duration.TotalSeconds;
            result.Solutions = solutions.Length;
            return result;
        }

        public static Expression<T> GenerateExpression<T>(int random_seed, T[] variables, int j_clauses, int k_literals_per_clause)
            where T : notnull
        {
            var random = new Random(random_seed);
            var all_literals = variables.SelectMany(v => new[] { Literal<T>.Positive(v), Literal<T>.Negative(v) }).ToArray();
            var clauses = new List<Clause<T>>();
            for (var j = 0; j < j_clauses; j++)
            {
                var literals = new List<Literal<T>>();
                for (var k = 0; k < k_literals_per_clause; k++)
                {
                    literals.Add(all_literals[random.Next(all_literals.Length)]);
                }
                clauses.Add(new(literals.ToHashSet()));
            }
            return new(clauses.ToHashSet());
        }
    }

    public struct SatResult
    {
        public int RandomSeed;
        public int NVariables;
        public int JClauses;
        public int KLiterals;
        public double SecondsTaken;
        public int Solutions;

        public static string ToHeader()
            => $"{nameof(RandomSeed)},{nameof(NVariables)},{nameof(JClauses)},{nameof(KLiterals)},{nameof(SecondsTaken)},{nameof(Solutions)}";

        public override string ToString()
            => $"{RandomSeed},{NVariables},{JClauses},{KLiterals},{SecondsTaken},{Solutions}";
    }
}
