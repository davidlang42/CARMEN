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

        [Test]
        public void Random()
        {
            Console.WriteLine(SummaryRow.ToHeader());
            foreach (var file_name in TestFiles("random"))
            {
                using var context = new ShowContext(OptionsFor(file_name));
                RunTestCase(context, Path.GetFileNameWithoutExtension(file_name), false);
            }
        }

        [Test]
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

        [Test]
        public void Converted()
        {
            Console.WriteLine(SummaryRow.ToHeader());
            foreach (var file_name in TestFiles("converted"))
            {
                using var context = new ShowContext(OptionsFor(file_name));
                RunTestCase(context, Path.GetFileNameWithoutExtension(file_name), true);
            }
        }

        [Test]
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
                DisplaySummary(test_case, "Existing", cast_groups, criterias);
            foreach (var engine in CreateEngines(context))
            {
                if (engine is HeuristicSelectionEngine)
                    continue; //TODO fix heuristic selection engine
                ClearAlternativeCasts(applicants);
                engine.BalanceAlternativeCasts(applicants, same_cast_sets); //TODO benchmark how long each engine takes as well
                DisplaySummary(test_case, engine.GetType().Name, cast_groups, criterias);
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
            public CastGroup CastGroup;
            public Criteria Criteria;
            public AlternativeCast AlternativeCast;
            public uint[] Marks;
            public MarkDistribution Distribution;

            public static string ToHeader()
                => $"Test case\tEngine\tCast group\tCriteria\tAlternative cast\t"
                + $"Min\tMax\tMean\tMedian\tStd-Dev\t"
                + $"Sorted marks";

            public override string ToString()
                => $"{TestCase}\t{Engine}\t{CastGroup.Abbreviation}\t{Criteria.Name}\t{AlternativeCast.Initial}\t"
                + $"{Distribution.Min}\t{Distribution.Max}\t{Distribution.Mean:#.#}\t{Distribution.Median:#.#}\t{Distribution.StandardDeviation:#.#}\t"
                + $"{string.Join(",", Marks.OrderByDescending(m => m))}";
        }

        private void DisplaySummary(string test_case, string engine_name, IEnumerable<CastGroup> cast_groups, IEnumerable<Criteria> criterias)
        {
            var summary = new SummaryRow
            {
                TestCase = test_case,
                Engine = engine_name
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
                    foreach (var group in cg.Members.GroupBy(a => a.AlternativeCast))
                    {
                        if (group.Key == null)
                            throw new Exception($"Missing alternative cast for {group.Count().Plural("applicant")}");
                        summary.AlternativeCast = group.Key;
                        summary.Marks = group.Select(a => a.MarkFor(c)).ToArray();
                        summary.Distribution = MarkDistribution.Analyse(summary.Marks);
                        Console.WriteLine(summary.ToString());
                        //TODO make each row a comparison between 2 casts (but keep check for null ac), including:
                        //- mean difference
                        //- median difference
                        //- std dev difference
                        //- ranking difference
                        //- all of those for top10
                        //- min threshold for top10 difference
                        //- count of marks above 70 difference
                    }
                }
            }
        }
    }
}
