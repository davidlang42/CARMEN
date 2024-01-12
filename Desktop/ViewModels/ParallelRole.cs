using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Carmen.ShowModel;

namespace Carmen.Desktop.ViewModels
{
    public class ParallelRole
    {
        public Role Role { get; }

        public string Description => Role.Name;

        public string ToolTip
        {
            get
            {
                var s = string.Join(", ", Role.CountByGroups.OrderBy(cbg => cbg.CastGroup.Order).Select(cbg => $"{cbg.Count} {cbg.CastGroup.Abbreviation}"));
                if (Role.Requirements.Count != 0)
                    s += " requires " + string.Join(", ", Role.Requirements.InOrder().Select(r => r.Name));
                return s;
            }
        }

        public ParallelRole(Role role)
        {
            Role = role;
        }
    }
}
