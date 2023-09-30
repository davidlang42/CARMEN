using Carmen.ShowModel.Structure;
using Carmen.ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Models
{
    internal class ItemDetail : IBasicListItem
    {
        public Item Item { get; init; }

        public string MainText => Item.Name;

        public string DetailText => string.Join(" > ", Item.Parents().Reverse().Skip(1).Select(i => i.Name)); // skip the ShowRoot
    }
}
