using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    public struct Assignment
    {
        public Variable Variable { get; set; }
        public bool Value { get; set; }

        public override string ToString() => $"{Variable} <- {Value}";
    }
}
