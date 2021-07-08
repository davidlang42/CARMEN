﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    /// <summary>
    /// A requirement which can be satisfied by an Applicant
    /// </summary>
    public abstract class Requirement : IOrdered
    {
        #region Database fields
        public int RequirementId { get; set; }
        public string Name { get; set; } = "";
        public int Order { get; set; }
        public string? Reason { get; set; }
        #endregion

        /// <summary>Calculates the suitability of an Applicant.
        /// Value returned will be between 0 and 1 (inclusive).</summary>
        public virtual double SuitabilityOf(Applicant applicant)
            => IsSatisfiedBy(applicant) ? 1 : 0;

        /// <summary>Checks if an Applicant satisfies this requirement.</summary>
        public abstract bool IsSatisfiedBy(Applicant applicant);
    }
}
