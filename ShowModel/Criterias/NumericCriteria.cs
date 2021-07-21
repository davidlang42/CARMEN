using System;

namespace ShowModel.Criterias
{
    /// <summary>
    /// A criteria which is marked on a numeric scale.
    /// </summary>
    public class NumericCriteria : Criteria
    {
        const uint DEFAULT_MAX_MARK = 100;

        public NumericCriteria()
        {
            maxMark = DEFAULT_MAX_MARK;
        }
    }
}
