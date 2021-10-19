using Carmen.CastingEngine.Audition;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System.ComponentModel;

namespace Carmen.CastingEngine.Selection
{
    /// <summary>
    /// A concrete approach for balancing alternative casts with a combination of the TopPairs and ChunkedPairs approaches.
    /// Initially the full list of aplicants are paired off in order, reducing the number of pairs each iteration until success.
    /// The remaining applicants are then chunked, increasing the chunk size until it succeeds.
    /// </summary>
    public class HybridPairsSatEngine : PairsSatEngine
    {
        public HybridPairsSatEngine(IAuditionEngine audition_engine, AlternativeCast[] alternative_casts, Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction, Criteria[] criterias)
            : base(audition_engine, alternative_casts, cast_number_order_by, cast_number_order_direction, criterias)
        { }

        protected override void Initialise(out int chunk_size, out int max_chunks)
        {
            chunk_size = 2;
            max_chunks = int.MaxValue;
        }

        protected override bool Simplify(ref int chunk_size, ref int max_chunks)
        {
            max_chunks -= 1;
            return max_chunks >= 0; // allow zero in case the basic clauses are solvable on their own
        }

        protected override bool Improve(ref int chunk_size, ref int max_chunks)
        {
            chunk_size += 2;
            max_chunks = int.MaxValue;
            return true; // eventually terminates when no additional chunk clauses are added
        }  
    }
}
