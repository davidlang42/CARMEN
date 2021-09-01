using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// A set of assignments which solves a boolean expression.
    /// </summary>
    public struct Solution
    {
        public static Solution Unsolveable => default;

        public bool?[] Assignments { get; set; }

        public bool IsUnsolvable => Assignments == null;

        public bool IsFullyAssigned
            => Assignments?.All(a => a != null) ?? throw new ApplicationException($"Cannot check full assignment of an a unsolveable solution");

        public IEnumerable<Solution> Enumerate() => Enumerate(this, 0);

        private static IEnumerable<Solution> Enumerate(Solution partial, int start_at)
        {
            partial = partial.Clone();
            var first_unassigned = -1;
            for (var i = start_at; i < partial.Assignments.Length; i++)
            {
                if (partial.Assignments[i] == null)
                {
                    first_unassigned = i;
                    break;
                }
            }
            if (first_unassigned == -1)
                yield return partial;
            else
            {
                partial.Assignments[first_unassigned] = false;
                foreach (var full_assignment in Enumerate(partial, first_unassigned + 1))
                    yield return full_assignment;
                partial.Assignments[first_unassigned] = true;
                foreach (var full_assignment in Enumerate(partial, first_unassigned + 1))
                    yield return full_assignment;
            }
        }

        public Solution Clone()
            => new Solution
            {
                Assignments = (bool?[])Assignments.Clone()
            };

        public override string ToString()
            => Assignments == null ? "Unsolvable" : "Solution: " + string.Join("", Assignments.Select(a => a.HasValue ? a.Value ? "1" : "0" : "X"));
    }
}
