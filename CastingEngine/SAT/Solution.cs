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

        public IEnumerable<bool[]> Enumerate()
        {
            //TODO
            throw new NotImplementedException();
        }

        public override string ToString()
            => Assignments == null ? "Unsolvable" : "Solution: " + string.Join("", Assignments.Select(a => a.HasValue ? a.Value ? "1" : "0" : "X"));
    }
}
