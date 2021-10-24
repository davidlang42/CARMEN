using System.Collections.Generic;

namespace Carmen.CastingEngine.SAT
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
            // if I want to keep pure literals and have all solutions, could possibly
            // back-check the inverse pure literal when returning solutions
            propogatePureLiterals = false;
        }
    }
}
