using System;

namespace Carmen.ShowModel.Criterias
{
    /// <summary>
    /// A criteria which is marked on a numeric scale.
    /// </summary>
    public class NumericCriteria : Criteria
    {
        const uint DEFAULT_MAX_MARK = 100;

        public NumericCriteria()
        {
            base.MaxMark = DEFAULT_MAX_MARK;
        }
    }
}
