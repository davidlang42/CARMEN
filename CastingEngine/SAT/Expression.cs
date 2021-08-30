using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// A conjunction (AND) of boolean clauses.
    /// </summary>
    public struct Expression
    {
        public const string CONJUNCTION = "∧";

        public Clause[] Clauses { get; set; }

        public bool IsEmpty() => Clauses == null || Clauses.Length == 0;

        public bool Evaluate(IEnumerable<Assignment> assignments) => Evaluate(assignments.ToDictionary(a => a.Variable, a => a.Value));

        public bool Evaluate(Dictionary<Variable, bool> assignments) => Clauses.All(c => c.Evaluate(assignments));
        
        public override string ToString() => string.Join(CONJUNCTION, Clauses.Select(c => $"({c})"));
    }
}
