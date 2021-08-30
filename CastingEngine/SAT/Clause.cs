using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// A disjunction (OR) of boolean literals.
    /// </summary>
    public struct Clause
    {
        public const string DISJUNCTION = "∨";

        public Literal[] Literals { get; set; }

        public bool IsEmpty() => Literals == null || Literals.Length == 0;

        public bool IsUnitClause(out Literal? single_literal)
        {
            if (Literals == null || Literals.Length == 0 || Literals.Length > 1)
            {
                single_literal = null;
                return false;
            }
            else
            {
                single_literal = Literals[0];
                return true;
            }
        }

        public bool Evaluate(Dictionary<Variable, bool> assignments) => Literals.Any(l => l.Evaluate(assignments));

        public override string ToString() => string.Join(DISJUNCTION, Literals.Select(l => l.ToString()));
    }
}
