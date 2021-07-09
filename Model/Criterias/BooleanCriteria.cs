using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Criterias
{
    /// <summary>
    /// A criteria which is marked as True or False.
    /// </summary>
    public class BooleanCriteria : Criteria
    {
        public override uint MaxMark
        {
            get => 1;
            set => throw new NotImplementedException("BooleanCriteria.MaxMark cannot be set."); //TODO confirm save/load still works
        }
    }
}
