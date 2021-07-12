using System;

namespace Model.Criterias
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
            maxMark = 1;
        }
    }
}
