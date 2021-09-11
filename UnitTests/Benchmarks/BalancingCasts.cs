using Carmen.CastingEngine;
using Carmen.CastingEngine.Analysis;
using Carmen.CastingEngine.Base;
using Carmen.CastingEngine.Dummy;
using Carmen.CastingEngine.Heuristic;
using Carmen.CastingEngine.SAT;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Benchmarks
{
#if !BENCHMARK
    [Ignore("Used only for Benchmarking")]
#endif
    public class BalancingCasts
    {
        static string testDataPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            + @"\..\..\..\Benchmarks\Test data\";

        private static IEnumerable<string> TestFiles(string containing)
        {
            foreach (var file_name in Directory.GetFiles(testDataPath))
            {
                if (Path.GetExtension(file_name).ToLower() == ".db"
                    && Path.GetFileNameWithoutExtension(file_name).ToLower().Contains(containing.ToLower()))
                    yield return file_name;
            }
        }

        private static IEnumerable<ISelectionEngine> CreateEngines(ShowContext context)
        {
            var criterias = context.Criterias.ToArray();
            var applicant_engine = new WeightedSumEngine(criterias);
            foreach (var type in SelectionEngine.Implementations)
                yield return type.Name switch
                {
                    nameof(HeuristicSelectionEngine) => new HeuristicSelectionEngine(applicant_engine, context.AlternativeCasts.ToArray(), context.ShowRoot.CastNumberOrderBy, context.ShowRoot.CastNumberOrderDirection),
                    nameof(DummySelectionEngine) => new DummySelectionEngine(applicant_engine, context.AlternativeCasts.ToArray(), context.ShowRoot.CastNumberOrderBy, context.ShowRoot.CastNumberOrderDirection),
                    nameof(ChunkedPairsSatEngine) => new ChunkedPairsSatEngine(applicant_engine, context.AlternativeCasts.ToArray(), context.ShowRoot.CastNumberOrderBy, context.ShowRoot.CastNumberOrderDirection, criterias),
                    nameof(TopPairsSatEngine) => new TopPairsSatEngine(applicant_engine, context.AlternativeCasts.ToArray(), context.ShowRoot.CastNumberOrderBy, context.ShowRoot.CastNumberOrderDirection, criterias),
                    nameof(ThreesACrowdSatEngine) => new ThreesACrowdSatEngine(applicant_engine, context.AlternativeCasts.ToArray(), context.ShowRoot.CastNumberOrderBy, context.ShowRoot.CastNumberOrderDirection, criterias),
                    _ => throw new NotImplementedException($"Selection engine not handled: {type.Name}")
                };
        }

        private static DbContextOptions<ShowContext> OptionsFor(string file_name)
        {
            if (!File.Exists(file_name))
                throw new Exception($"File not found: {file_name}");
            return new DbContextOptionsBuilder<ShowContext>().UseSqlite($"Filename={file_name}").Options;
        }

        [Test] // ~5min
        public void Random()
        {
            Console.WriteLine(SummaryRow.ToHeader());
            foreach (var file_name in TestFiles("random"))
            {
                using var context = new ShowContext(OptionsFor(file_name));
                RunTestCase(context, Path.GetFileNameWithoutExtension(file_name), false);
            }
        }

        [Test] // ~5min
        public void Random_Quantized()
        {
            Console.WriteLine(SummaryRow.ToHeader());
            foreach (var file_name in TestFiles("random"))
            {
                using var context = new ShowContext(OptionsFor(file_name));
                QuantizePrimaryAbilities(context.Applicants, 5);
                RunTestCase(context, Path.GetFileNameWithoutExtension(file_name) + "_quantized", false);
            }
        }

        [Test] // ~1min
        public void Converted()
        {
            Console.WriteLine(SummaryRow.ToHeader());
            foreach (var file_name in TestFiles("converted"))
            {
                using var context = new ShowContext(OptionsFor(file_name));
                RunTestCase(context, Path.GetFileNameWithoutExtension(file_name), true);
            }
        }

        [Test] // ~1min
        public void Converted_Quantized()
        {
            Console.WriteLine(SummaryRow.ToHeader());
            foreach (var file_name in TestFiles("converted"))
            {
                using var context = new ShowContext(OptionsFor(file_name));
                QuantizePrimaryAbilities(context.Applicants, 5);
                RunTestCase(context, Path.GetFileNameWithoutExtension(file_name) + "_quantized", true);
            }
        }

        private void QuantizePrimaryAbilities(IEnumerable<Applicant> applicants, double quantize_to_nearest)
        {
            foreach (var applicant in applicants)
                foreach (var ability in applicant.Abilities)
                    if (ability.Criteria.Primary)
                        ability.Mark = (uint)(Math.Round(ability.Mark / quantize_to_nearest) * quantize_to_nearest);
        }

        internal void RunTestCase(ShowContext context, string test_case, bool analyse_existing)
        {
            var applicants = context.Applicants.ToArray();
            var cast_groups = context.CastGroups.ToArray();
            var criterias = context.Criterias.ToArray();
            var same_cast_sets = context.SameCastSets.ToArray();
            if (analyse_existing)
                DisplaySummary(test_case, "Existing", 0, cast_groups, criterias);
            foreach (var engine in CreateEngines(context))
            {
                if (engine is HeuristicSelectionEngine)
                    continue; //TODO fix heuristic selection engine
                ClearAlternativeCasts(applicants);
                var start_time = DateTime.Now;
                engine.BalanceAlternativeCasts(applicants, same_cast_sets);
                var duration = DateTime.Now - start_time;
                DisplaySummary(test_case, engine.GetType().Name, duration.TotalSeconds, cast_groups, criterias);
            }
        }

        private void ClearAlternativeCasts(IEnumerable<Applicant> applicants)
        {
            foreach (var applicant in applicants)
                applicant.AlternativeCast = null;
        }

        private struct SummaryRow
        {
            public string TestCase;
            public string Engine;
            public double TotalSeconds;
            public CastGroup CastGroup;
            public Criteria Criteria;
            public List<CastRow> CastRows;

            const int CAST_COUNT = 2;

            public static string ToHeader()
            {
                var h = "Test case\tEngine\tTotal seconds\tCast group\tCriteria\t";
                for(var i=0; i<CAST_COUNT; i++)
                    h += $"Cast({i})\tSorted marks({i})\t{CastRow.ToHeader(i.ToString())}";
                h += $"{CastRow.ToHeader("Δ")}Rank difference\tCast rank order\t";
                h += $"{CastRow.ToHeader("Top10-Δ")}Rank difference(Top5)\tCast rank order(Top5)\t";
                return h;
            }

            public override string ToString()
            {
                if (CastRows.Count != CAST_COUNT)
                    throw new Exception($"Expected {CAST_COUNT} cast rows but found {CastRows.Count}.");
                var s = $"{TestCase}\t{Engine}\t{TotalSeconds:0.0}\t{CastGroup.Abbreviation}\t{Criteria.Name}\t";
                foreach (var cr in CastRows)
                    s += $"{cr.AlternativeCast.Initial}\t{string.Join(", ",cr.SortedMarks.OrderByDescending(m => m))}\t{cr}";
                var a = CastRows[0];
                var b = CastRows[1];
                s += CastRow.Difference(a, b).ToString();
                CalculateRankings(a, b, out var rank_letter_order, out var rank_difference);
                s += $"{rank_difference}\t{rank_letter_order}\t";
                a = a.Top(5);
                b = b.Top(5);
                s += CastRow.Difference(a, b).ToString();
                CalculateRankings(a, b, out rank_letter_order, out rank_difference);
                s += $"{rank_difference}\t{rank_letter_order}\t";
                //TODO add to cast comparisons:
                //- min threshold for top10 difference
                //- count of marks above 70 difference
                return s;
            }

            private static void CalculateRankings(CastRow row_a, CastRow row_b, out string rank_letter_order, out int rank_difference)
            {
                rank_letter_order = "";
                rank_difference = 0;
                int a = 0, b = 0;
                int current_rank = row_a.SortedMarks.Concat(row_b.SortedMarks).Distinct().Count();
                while (a < row_a.SortedMarks.Length && b < row_b.SortedMarks.Length)
                {
                    var value_a = row_a.SortedMarks[a];
                    var value_b = row_b.SortedMarks[b];
                    if (value_a > value_b)
                    {
                        a++;
                        rank_letter_order += row_a.AlternativeCast.Initial;
                        rank_difference += current_rank--;
                    }
                    else if (value_a < value_b)
                    {
                        b++;
                        rank_letter_order += row_b.AlternativeCast.Initial;
                        rank_difference -= current_rank--;
                    }
                    else
                    {
                        // if equal, default to opposite of last
                        if (rank_letter_order.LastOrDefault() == row_a.AlternativeCast.Initial)
                        {
                            b++;
                            rank_letter_order += row_b.AlternativeCast.Initial;
                            rank_difference -= current_rank;
                        }
                        else
                        {
                            a++;
                            rank_letter_order += row_a.AlternativeCast.Initial;
                            rank_difference += current_rank;
                        }
                    }
                }
                for (; a < row_a.SortedMarks.Length; a++)
                    rank_letter_order += row_a.AlternativeCast.Initial;
                for (; b < row_b.SortedMarks.Length; b++)
                    rank_letter_order += row_b.AlternativeCast.Initial;
                rank_difference = Math.Abs(rank_difference);
            }
        }

        private struct CastRow
        {
            public AlternativeCast AlternativeCast;
            public uint[] SortedMarks; // descending
            public MarkDistribution Distribution;

            public static string ToHeader(string label)
                => $"Min({label})\tMax({label})\tMean({label})\tMedian({label})\tStd-dev({label})\t";

            public override string ToString()
                => $"{Distribution.Min}\t{Distribution.Max}\t{Distribution.Mean:0.0}\t{Distribution.Median:0.0}\t{Distribution.StandardDeviation:0.0}\t";

            public static CastRow Difference(CastRow a, CastRow b)
                => new()
                {
                    Distribution = new()
                    {
                        Max = (uint)Math.Abs(a.Distribution.Max - b.Distribution.Max),
                        Min = (uint)Math.Abs(a.Distribution.Min - b.Distribution.Min),
                        Mean = (uint)Math.Abs(a.Distribution.Mean - b.Distribution.Mean),
                        Median = (uint)Math.Abs(a.Distribution.Median - b.Distribution.Median),
                        StandardDeviation = (uint)Math.Abs(a.Distribution.StandardDeviation - b.Distribution.StandardDeviation),
                    }
                };

            public CastRow Top(int count)
            {
                var row = new CastRow
                {
                    AlternativeCast = AlternativeCast,
                    SortedMarks = SortedMarks.Take(count).ToArray()
                };
                row.Distribution = MarkDistribution.Analyse(row.SortedMarks);
                return row;
            }
        }

        private void DisplaySummary(string test_case, string engine_name, double total_seconds, IEnumerable<CastGroup> cast_groups, IEnumerable<Criteria> criterias)
        {
            var summary = new SummaryRow
            {
                TestCase = test_case,
                Engine = engine_name,
                TotalSeconds = total_seconds
            };
            foreach (var cg in cast_groups)
            {
                if (cg.Members.Count == 0)
                    throw new Exception($"No cast allocated to {cg.Abbreviation}");
                if (!cg.AlternateCasts)
                    continue;
                summary.CastGroup = cg;
                foreach (var c in criterias.Where(c => c.Primary))
                {
                    summary.Criteria = c;
                    summary.CastRows = new();
                    foreach (var group in cg.Members.GroupBy(a => a.AlternativeCast))
                    {
                        if (group.Key == null)
                            throw new Exception($"Missing alternative cast for {group.Count().Plural("applicant")}");
                        var marks = group.Select(a => a.MarkFor(c)).OrderByDescending(m => m).ToArray();
                        summary.CastRows.Add(new()
                        {
                            AlternativeCast = group.Key,
                            SortedMarks = marks,
                            Distribution = MarkDistribution.Analyse(marks),
                        });
                    }
                    Console.WriteLine(summary.ToString());
                }
            }
        }
    }
}
