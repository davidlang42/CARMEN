using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// A set of assignments and remaining free variables which solves a boolean expression.
    /// </summary>
    public struct Solution
    {
        public Assignment[] Assignments { get; set; }
        public Variable[] FreeVariables { get; set; }

        public bool HasAssignments() => Assignments != null && Assignments.Length != 0;

        public bool HasFreeVariables() => FreeVariables != null && FreeVariables.Length != 0;

        public IEnumerable<Assignment> FullyAssigned(bool value_to_assign_free_variables = false)
        {
            if (HasAssignments())
                foreach (var assignment in Assignments)
                    yield return assignment;
            if (HasFreeVariables())
                foreach (var free_variable in FreeVariables)
                    yield return new Assignment { Variable = free_variable, Value = value_to_assign_free_variables };
        }

        public override string ToString()
        {
            Dictionary<Variable, bool?> values = new();
            if (HasAssignments())
                foreach (var assignment in Assignments)
                    values.Add(assignment.Variable, assignment.Value);
            if (HasFreeVariables())
                foreach (var variable in FreeVariables)
                    if (!values.TryAdd(variable, null))
                        return $"Invalid: Free variable {variable} also has an assignment of {values[variable]}";
            return "Solution: " + string.Join("", values.OrderBy(p => p.Key.ToString()).Select(p => p.Value.HasValue ? p.Value.Value ? "1" : "0" : "X"));
        }
    }
}
