using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    public struct Solution
    {
        public Assignment[] Assignments { get; set; }
        public Variable[] FreeVariables { get; set; }

        public override string ToString()
        {
            var values = Assignments.ToDictionary(a => a.Variable, a => (bool?)a.Value);
            foreach (var variable in FreeVariables)
                if (!values.TryAdd(variable, null))
                    return $"Invalid: Free variable {variable} also has an assignment of {values[variable]}";
            return "Solution: " + string.Join("", values.OrderBy(p => p.Key.ToString()).Select(p => p.Value.HasValue ? p.Value.Value ? "1" : "0" : "X"));
        }
    }
}
