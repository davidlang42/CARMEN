using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// A really bad SAT solver
    /// </summary>
    public class BruteForceSolver : Solver
    {
        public BruteForceSolver(IEnumerable<Variable> variables)
            : base(variables.ToHashSet())
        { }

        public override Solution? Solve(Expression expression)
            => Solve(expression, new Assignment[0], Variables.ToArray());

        private static Solution? Solve(Expression expression, Assignment[] assignments, Variable[] unassigned_variables)
        {
            if (unassigned_variables.Length == 0)
            {
                if (expression.Evaluate(assignments))
                    return new Solution { Assignments = assignments };
                else
                    return null;
            }
            else
            {
                var first_var = unassigned_variables[0];
                var remaining_vars = unassigned_variables.Skip(1).ToArray();
                return Solve(expression, assignments.Concat(new Assignment { Variable = first_var, Value = false }.Yield()).ToArray(), remaining_vars)
                    ?? Solve(expression, assignments.Concat(new Assignment { Variable = first_var, Value = true }.Yield()).ToArray(), remaining_vars);
            }
        }
    }
}
