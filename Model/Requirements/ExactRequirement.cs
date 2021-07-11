using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Requirements
{
    public abstract class ExactRequirement : Requirement
    {
        public uint ExactValue { get; set; }
    }
}
