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

        public bool[] FullyAssigned(bool value_to_assign_free_variables = false)
        {
            if (Assignments == null)
                throw new ApplicationException($"Cannot get a full assignment of an a unsolveable solution");
            return Assignments.Select(a => a ?? value_to_assign_free_variables).ToArray();
        }

        public IEnumerable<bool[]> Enumerate() => Enumerate(Assignments, 0);

        private static IEnumerable<bool[]> Enumerate(bool?[] partial_assignments, int start_at)
        {
            var first_unassigned = -1;
            for (var i = start_at; i < partial_assignments.Length; i++)
            {
                if (partial_assignments[i] == null)
                {
                    first_unassigned = i;
                    break;
                }
            }
            if (first_unassigned == -1)
                yield return partial_assignments.Cast<bool>().ToArray();
            else
            {
                partial_assignments[first_unassigned] = false;
                foreach (var full_assignment in Enumerate(partial_assignments, first_unassigned + 1))
                    yield return full_assignment;
                partial_assignments[first_unassigned] = true;
                foreach (var full_assignment in Enumerate(partial_assignments, first_unassigned + 1))
                    yield return full_assignment;
            }
        }

        public override string ToString()
            => Assignments == null ? "Unsolvable" : "Solution: " + string.Join("", Assignments.Select(a => a.HasValue ? a.Value ? "1" : "0" : "X"));
    }
}
