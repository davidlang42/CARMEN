using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Models
{
    internal interface IBasicListItem
    {
        string MainText { get; }
        string DetailText { get; }
    }
}
