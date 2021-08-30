using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.SAT
{
    public class Solver
    {
        public HashSet<Variable> Variables { get; init; }

        public Solver(HashSet<Variable> variables)
        {
            Variables = variables;
        }

        public Solver(IEnumerable<Variable> variables)
            : this(variables.ToHashSet())
        { }

        public Solver()
            : this(new())
        { }

        /// <summary>Solves a boolean expression, or returns null if it is found to be unsolveable</summary>
        public Solution? Solve(Expression expression)
        {
            throw new NotImplementedException();
            //TODO
        }

        /// <summary>Checks than a boolean expression is valid. That is:
        /// - it only contains Variables known to the solver
        /// - it contains at least one Clause
        /// - it does not contain duplicate Clauses
        /// - each Clause contains at least one Literal
        /// - no Clause contains duplicate Literals</summary>
        public bool Check(Expression expression)
        {
            if (expression.Clauses.Length == 0)
                return false;
            HashSet<Clause> valid_clauses = new();
            foreach (var clause in expression.Clauses)
            {
                if (clause.Literals.Length == 0)
                    return false;
                if (!valid_clauses.Add(clause))
                    return false;
                HashSet<Literal> valid_literals = new();
                foreach (var literal in clause.Literals)
                {
                    if (!Variables.Contains(literal.Variable))
                        return false;
                    if (!valid_literals.Add(literal))
                        return false;
                }
            }
            return true;
        }

        /// <summary>Introduces the solver to all variables in the given expression</summary>
        public void Introduce(Expression expression)
        {
            foreach (var clause in expression.Clauses)
                foreach (var literal in clause.Literals)
                    Variables.Add(literal.Variable);
        }
    }
}
