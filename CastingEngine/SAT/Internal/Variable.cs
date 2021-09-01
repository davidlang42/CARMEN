using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT.Internal
{
    /// <summary>
    /// Internally a boolean variable is represented by an array of Literals.
    /// Fundamentally this boolean array identifies which variable literals are referenced. That is,
    /// - the negative literal of variable V is referenced if Literals[0] == true,
    /// - the positive literal of variable V is referenced if Literals[1] == true
    /// </summary>
    public struct Variable
    {
        public bool[] Literals { get; set; }

        public bool Evaluate(bool value) => Literals[value ? 1 : 0];

        public static Variable FromReferences<T>(T variable, IEnumerable<Literal<T>> referenced_literals)
            where T : notnull
        {
            var variable_references = referenced_literals.Where(l => variable.Equals(l.Variable)).ToArray();
            return new Variable
            {
                Literals = new[] {
                    variable_references.Any(r => !r.Polarity),
                    variable_references.Any(r => r.Polarity)
                }
            };
        }
    }
}
