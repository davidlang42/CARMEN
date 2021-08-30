using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    public struct Expression
    {
        public const string CONJUNCTION = "∧";

        public Clause[] Clauses { get; set; }

        public override string ToString() => string.Join(CONJUNCTION, Clauses.Select(c => $"({c})"));
    }
}
