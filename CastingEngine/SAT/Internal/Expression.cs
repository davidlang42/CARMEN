using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT.Internal
{
    /// <summary>
    /// Internally a boolean expression is expressed as an array of Clauses, which has an array of Variables, which has an array of Literals.
    /// Fundamentally this 3 dimensional array of booleans identifies which variable literals are contained in each clause. That is,
    /// - a clause C contains the negative literal of variable V if Expression.Clauses[C].Variables[V].Literals[0] == true,
    /// - a clause C contains the positive literal of variable V if Expression.Clauses[C].Variables[V].Literals[1] == true
    /// </summary>
    public struct Expression
    {
        public const string CONJUNCTION = "∧";

        public Clause[] Clauses { get; set; }

        public bool Evaluate(Solution solution) => Clauses.All(c => c.Evaluate(solution));

        public override string ToString() => string.Join(CONJUNCTION, Clauses.Select(c => $"({c})"));

        /// <summary>Create a copy of this expression with one additional clause</summary>
        public Expression With(Clause clause)
        {
            var clauses = new Clause[Clauses.Length + 1];
            for (var c = 0; c < Clauses.Length; c++)
                clauses[c] = Clauses[c];
            clauses[Clauses.Length] = clause;
            return new() { Clauses = clauses };
        }
    }
}
