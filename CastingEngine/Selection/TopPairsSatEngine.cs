using Carmen.CastingEngine.Audition;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System.ComponentModel;

namespace Carmen.CastingEngine.Selection
{
    /// <summary>
    /// A concrete approach for balancing alternative casts by initially pairing off the full list of applicants in order,
    /// then reducing the number of pairs each iteration until success.
    /// NOTE: this approach may cause uneven alternative cast counts
    /// </summary>
    public class TopPairsSatEngine : PairsSatEngine
    {
        public TopPairsSatEngine(IAuditionEngine audition_engine, AlternativeCast[] alternative_casts, Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction, Criteria[] criterias)
            : base(audition_engine, alternative_casts, cast_number_order_by, cast_number_order_direction, criterias)
        { }

        protected override bool Simplify(ref int chunk_size, ref int max_chunks)
        {
            max_chunks -= 1;
            return max_chunks >= 0; // allow zero in case the basic clauses are solvable on their own
        }
    }
}
