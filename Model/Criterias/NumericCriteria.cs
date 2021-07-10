using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Criterias
{
    /// <summary>
    /// A criteria which is marked on a numeric scale.
    /// </summary>
    public class NumericCriteria : Criteria
    {
        const uint DEFAULT_MAX_MARK = 100;

        public NumericCriteria()
        {
            MaxMark = DEFAULT_MAX_MARK;
        }
    }
}
