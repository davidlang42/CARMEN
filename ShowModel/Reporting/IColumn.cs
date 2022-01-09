using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Reporting
{
    public interface IColumn : IOrdered
    {
        public string Name { get; }
        public string? Format { get; }
        public bool Show { get; set; }
    }
}
