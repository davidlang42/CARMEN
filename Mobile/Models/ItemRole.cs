using Carmen.ShowModel.Structure;
using Carmen.ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Models
{
    internal class ItemRole
    {
        public Role Role { get; init; }
        public Item Item { get; init; }

        public string MainText => Item.Name;
        public string DetailText => Role.Name;
    }
}
