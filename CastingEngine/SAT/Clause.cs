using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    public struct Clause
    {
        public const string DISJUNCTION = "∨";

        public Literal[] Literals { get; set; }

        public override string ToString() => string.Join(DISJUNCTION, Literals.Select(l => l.ToString()));
    }
}
