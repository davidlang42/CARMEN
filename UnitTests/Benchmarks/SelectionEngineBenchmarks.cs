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
    public class SelectionEngineBenchmarks
    {
        static string testDataPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            + @"\..\..\..\Benchmarks\Test data\";

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

        [TestCase("random2021.db")]
        [Test]
        public void BalanceAlternativeCasts(string file)
        {
            Console.WriteLine(SummaryRow.ToHeader());
            using var context = new ShowContext(OptionsFor(testDataPath + file));
            var applicants = context.Applicants.ToArray();
            var cast_groups = context.CastGroups.ToArray();
            var criterias = context.Criterias.ToArray();
            var same_cast_sets = context.SameCastSets.ToArray();
            foreach (var engine in CreateEngines(context))
            {
                ClearAlternativeCasts(applicants);
                engine.BalanceAlternativeCasts(applicants, same_cast_sets);
                DisplaySummary(engine, cast_groups, criterias);
            }
        }

        private void ClearAlternativeCasts(IEnumerable<Applicant> applicants)
        {
            foreach (var applicant in applicants)
                applicant.AlternativeCast = null;
        }

        private struct SummaryRow
        {
            public ISelectionEngine Engine;
            public CastGroup CastGroup;
            public Criteria Criteria;
            public AlternativeCast AlternativeCast;
            public uint[] Marks;
            public MarkDistribution Distribution;

            public static string ToHeader()
                => $"Engine\tCast group\tCriteria\tAlternative cast\t"
                + $"Min\tMax\tMean\tMedian\tStd-Dev\t"
                + $"Sorted marks";

            public override string ToString()
                => $"{Engine.GetType().Name}\t{CastGroup.Abbreviation}\t{Criteria.Name}\t{AlternativeCast.Initial}\t"
                + $"{Distribution.Min}\t{Distribution.Max}\t{Distribution.Mean}\t{Distribution.Median}\t{Distribution.StandardDeviation}\t"
                + $"{string.Join(",", Marks.OrderByDescending(m => m))}";
        }

        private void DisplaySummary(ISelectionEngine engine, IEnumerable<CastGroup> cast_groups, IEnumerable<Criteria> criterias)
        {
            var summary = new SummaryRow { Engine = engine };
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
                    }
                }
            }
        }
    }
}
