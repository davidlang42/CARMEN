using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public abstract class Requirement<T> //TODO how will generics save to the database?
    {
        /// <summary>Calculates a suitability value between 0 and 1 (inclusive)</summary>
        public abstract double Suitability(Applicant applicant);
    }
}
