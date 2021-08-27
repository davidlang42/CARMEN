using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class RoleNodeView : NodeView
    {
        public Role Role { get; init; }
        private AlternativeCast[] alternativeCasts;

        public override string Name => Role.Name;

        protected async override Task<(double progress, bool has_errors)> CalculateAsync(double _, bool __)
        {
            var status = await Task.Run(() => Role.CastingStatus(alternativeCasts));
            var is_complete = status == Role.RoleStatus.FullyCast;
            var has_errors = status == Role.RoleStatus.UnderCast || status == Role.RoleStatus.OverCast;
            return (is_complete ? 1 : 0, has_errors);
        }

        public RoleNodeView(Role role, AlternativeCast[] alternative_casts)
            : base(new NodeView[0])
        {
            this.Role = role;
            alternativeCasts = alternative_casts;
        }
    }
}
