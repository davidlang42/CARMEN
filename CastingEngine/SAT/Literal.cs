using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    public struct Literal
    {
        public Variable Variable { get; set; }
        public bool Inverse { get; set; }

        public override string ToString() => Inverse ? $"neg({Variable})" : Variable.ToString();
    }
}
