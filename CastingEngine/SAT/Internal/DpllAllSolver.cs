using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT.Internal
{
    /// <summary>
    /// An ALL-SAT solver based on the DPLL algorithm, but without pure literal propogation
    /// </summary>
    public class DpllAllSolver<T> : DpllSolver<T>
        where T : notnull
    {
        public DpllAllSolver(IEnumerable<T>? variables = null)
            : base(variables)
        {
            propogatePureLiterals = false; //LATER if I want to keep pure literals and have all solutions, could possibly back-check the inverse pure literal when returning solutions
        }
    }
}
