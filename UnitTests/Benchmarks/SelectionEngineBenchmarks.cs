using Carmen.CastingEngine;
using Carmen.CastingEngine.Base;
using Carmen.CastingEngine.Dummy;
using Carmen.CastingEngine.Heuristic;
using Carmen.CastingEngine.SAT;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Benchmarks
{
    public class SelectionEngineBenchmarks
    {
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

        [TestCase("..\\..\\Benchmarks\\Test data\\random\\random2021.db")]
        [Test]
        public void BalanceAlternativeCasts_OverallDistribution(string file_name)
        {
            var options = new DbContextOptionsBuilder<ShowContext>()
                .UseSqlite($"Filename={file_name}").Options;
            using var context = new ShowContext(options);
            foreach (var applicant in context.Applicants)
                applicant.AlternativeCast = null;
            engine.BalanceAlternativeCasts(context.Applicants, Enumerable.Empty<SameCastSet>());
        }
    }
}
