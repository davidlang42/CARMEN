using System;

namespace ShowModel.Criterias
{
    /// <summary>
    /// A criteria which is marked as True or False.
    /// </summary>
    public class BooleanCriteria : Criteria
    {
        public override uint MaxMark
        {
            set => throw new NotImplementedException("BooleanCriteria.MaxMark cannot be set.");
        }

        public BooleanCriteria()
        {
            base.MaxMark = 1;
        }
    }
}
