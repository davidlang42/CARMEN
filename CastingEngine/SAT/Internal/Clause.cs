using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT.Internal
{
    /// <summary>
    /// Internally a boolean clause is represented by an array of Variables, which has an array of Literals.
    /// Fundamentally this 2 dimensional array of booleans identifies which variable literals are contained in this clause. That is,
    /// - this clause contains the negative literal of variable V if Variables[V].Literals[0] == true,
    /// - this clause contains the positive literal of variable V if Variables[V].Literals[1] == true
    /// </summary>
    public struct Clause
    {
        public const string DISJUNCTION = "∨";

        public Variable[] Variables { get; set; }

        public override string ToString()
        {
            var variable_names = new List<string>();
            for (var v=0; v < Variables.Length; v++)
            {
                if (Variables[v].Literals[0])
                    variable_names.Add($"neg(X{v+1})");
                if (Variables[v].Literals[1])
                    variable_names.Add($"X{v+1}");
            }
            return string.Join(DISJUNCTION, variable_names);
        }

        /// <summary>Create a copy of this clause with one variable (both literals) set to false</summary>
        public Clause Without(int variable)
        {
            var variables = (Variable[])Variables.Clone();
            variables[variable] = new() { Literals = new[] { false, false } };
            return new Clause { Variables = variables };
        }

        public bool Evaluate(Solution solution)
            => Variables.Zip(solution.Assignments).Any(p => p.First.Evaluate(p.Second ?? throw new ApplicationException("Tried to evaluate with a non-fully assigned solution")));

        /// <summary>Create a new clause containing only one literal</summary>
        public static Clause Unit(int n_variables, int n_literals, int variable, int literal)
        {
            var variables = new Variable[n_variables];
            for (var v = 0; v < n_variables; v++)
                variables[v].Literals = new bool[n_literals];
            for (var l = 0; l < n_literals; l++)
                variables[variable].Literals[l] = l == literal;
            return new() { Variables = variables };
        }
    }
}
