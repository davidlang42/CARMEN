using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    /// <summary>
    /// A positive or negative literal of a boolean variable.
    /// </summary>
    public struct Literal
    {
        public Variable Variable { get; set; }
        public bool Inverse { get; set; }

        public Literal InverseLiteral() => new Literal { Variable = Variable, Inverse = !Inverse };

        public Assignment AssignmentForTrue() => new Assignment { Variable = Variable, Value = !Inverse };

        public bool Evaluate(Dictionary<Variable, bool> assignments)
        {
            if (!assignments.TryGetValue(Variable, out var assigned_value))
                throw new ArgumentException($"{nameof(assignments)} did not contain variable {Variable}");
            return assigned_value != Inverse;
        }

        public override string ToString() => Inverse ? $"neg({Variable})" : Variable.ToString();

        public override bool Equals(object? obj)
            => obj is Literal other && other.Inverse == Inverse && other.Variable == Variable;

        public override int GetHashCode() => Variable.GetHashCode() ^ Inverse.GetHashCode();
    }
}
