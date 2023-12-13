using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Desktop.ViewModels
{
    public class ParallelRole
    {
        public Role Role { get; }

        public string Description => Role.Name;

        public ParallelRole(Role role)
        {
            Role = role;
        }
    }
}
