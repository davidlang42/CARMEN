using Carmen.CastingEngine.Audition;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System.ComponentModel;

namespace Carmen.CastingEngine.Selection
{
    /// <summary>
    /// A concrete approach for balancing alternative casts by initially pairing off the full list of applicants in order,
    /// then increasing from pairs to a larger chunk size each iteration until success.
    /// </summary>
    public class ChunkedPairsSatEngine : PairsSatEngine
    {
        public ChunkedPairsSatEngine(IAuditionEngine audition_engine, AlternativeCast[] alternative_casts, Criteria? cast_number_order_by, ListSortDirection cast_number_order_direction, Criteria[] criterias)
            : base(audition_engine, alternative_casts, cast_number_order_by, cast_number_order_direction, criterias)
        { }

        protected override bool Simplify(ref int chunk_size, ref int max_chunks)
        {
            chunk_size += 2;
            return chunk_size <= 16; // any more than 16 takes too long to process
        }
    }
}
