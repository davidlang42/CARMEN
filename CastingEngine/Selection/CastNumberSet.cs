using Carmen.ShowModel.Applicants;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Selection
{
    /// <summary>
    /// A set of cast numbers stored in a dictionary-based structure, with some special handling:
    /// - A cast number allocated to null AlternativeCast cannot be allocated to anyone else
    /// - A cast number allocated to an AlternativeCast can be allocated to other AlternativeCasts, as long as they are the same CastGroup
    /// NOTE: Currently supports Add/Clear operations, but not Remove
    /// </summary>
    public class CastNumberSet
    {
        public const int FIRST_CAST_NUMBER = 1;

        /// <summary>Stores the actual cast number data</summary>
        Dictionary<int, CastNumberRecord> numbers { get; init; } = new();

        /// <summary>The lowest cast number which is not allocated for a given AlternativeCast.
        /// NOTE: denotes a starting point only, not guarenteed to be available</summary>
        Dictionary<AlternativeCast, int> lowestAvailable { get; init; } = new();

        /// <summary>The lowest cast number which is not allocated to anyone.
        /// NOTE: denotes a starting point only, not guarenteed to be available</summary>
        int lowestUnusedNumber = FIRST_CAST_NUMBER;

        public bool Add(int cast_number, AlternativeCast? alternative_cast, CastGroup cast_group)
        {
            if (numbers.TryGetValue(cast_number, out var record))
            {
                if (record.CastGroup != cast_group)
                    return false; // wrong CastGroup, so we can't share this allocation
                if (record.AlternativeCasts is not HashSet<AlternativeCast> record_alternative_casts)
                    return false; // cast number is fully allocated
                if (alternative_cast == null)
                    return false; // cast number is partially allocated, but we needed full
                // take the partial allocation we need, or fail because that partial is already taken
                return record_alternative_casts.Add(alternative_cast); 
            }
            else
            {
                // the full allocation is available, so we take it
                numbers.Add(cast_number, new CastNumberRecord
                {
                    CastGroup = cast_group,
                    AlternativeCasts = alternative_cast?.Yield().ToHashSet()
                });
                return true;
            }
        }

        /// <summary>Find the next cast number available for a given AlternativeCast and CastGroup</summary>
        public int AddNextAvailable(AlternativeCast? alternative_cast, CastGroup cast_group)
        {
            int cast_number;
            if (alternative_cast == null)
                cast_number = FastForwardToLowestFullyAvailable();
            else
                cast_number = FastForwardToLowestPartiallyAvailable(alternative_cast);
            while (!Add(cast_number, alternative_cast, cast_group))
                cast_number++;
            return cast_number;
        }

        private int FastForwardToLowestPartiallyAvailable(AlternativeCast partial_required)
        {
            int cast_number = lowestAvailable.TryGetValue(partial_required, out var previous_lowest) ? previous_lowest : FIRST_CAST_NUMBER;
            while (numbers.TryGetValue(cast_number, out var record) && // while some allocation exists
                    (record.AlternativeCasts is not HashSet<AlternativeCast> existing // and it is a full allocation
                    || existing.Contains(partial_required))) // or it contains the partial we want
                cast_number++; // fast-forward
            return lowestAvailable[partial_required] = cast_number;
        }

        private int FastForwardToLowestFullyAvailable()
        {
            int cast_number = lowestUnusedNumber;
            while (numbers.ContainsKey(cast_number)) // while any allocation exists
                cast_number++; // fast-foward
            return lowestUnusedNumber = cast_number;
        }

        public void Clear()
        {
            numbers.Clear();
            lowestAvailable.Clear();
            lowestUnusedNumber = FIRST_CAST_NUMBER;
        }
    }

    public struct CastNumberRecord
    {
        public CastGroup CastGroup { get; init; }
        public HashSet<AlternativeCast>? AlternativeCasts { get; init; }
    }
}
