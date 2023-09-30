using Carmen.ShowModel.Structure;
using Carmen.ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Models
{
    internal class RoleDetail : IBasicListItem
    {
        public Role Role { get; init; }

        public string MainText => Role.Name;

        public string DetailText => string.Join(", ", Role.Requirements.InOrder().Select(r => r.Name));
    }
}
