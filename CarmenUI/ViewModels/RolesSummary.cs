using Microsoft.EntityFrameworkCore;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Carmen.ShowModel.Structure.Role;
using System.Threading;

namespace CarmenUI.ViewModels
{
    public class RolesSummary : Summary
    {
        public override async Task LoadAsync(ShowContext c, CancellationToken cancel)
        {
            StartLoad();
            var alternative_casts = await c.AlternativeCasts.ToArrayAsync();
            // check role statuses
            var roles = await c.Roles.Include(r => r.Cast).ToArrayAsync();
            var counts = roles.GroupBy(r => r.CastingStatus(alternative_casts))
                .ToDictionary(g => g.Key, g => g.Count());
            if (counts.TryGetValue(RoleStatus.FullyCast, out var roles_cast))
                Rows.Add(new Row { Success = $"{roles_cast.Plural("Role")} cast" });
            if (counts.TryGetValue(RoleStatus.NotCast, out var roles_blank))
                Rows.Add(new Row { Success = $"{roles_blank.Plural("Role")} not cast" });
            if (counts.TryGetValue(RoleStatus.UnderCast, out var roles_undercast))
                Rows.Add(new Row { Fail = $"{roles_undercast.Plural("Role")} partially cast" });
            if (counts.TryGetValue(RoleStatus.OverCast, out var roles_overcast))
                Rows.Add(new Row { Fail = $"{roles_overcast.Plural("Role")} with too many cast" });
            // check showroot consecutive items
            var consecutive_item_failures = new HashSet<ConsecutiveItemCast>();
            if (!c.ShowRoot.VerifyConsecutiveItems(out var show_root_failures))
                foreach (var failure in show_root_failures)
                    consecutive_item_failures.Add(failure);
            // verify section type rules
            var cast_groups = await c.CastGroups.Include(cg => cg.Members).ToArrayAsync();
            var total_cast = (uint)cast_groups.Sum(cg => cg.FullTimeEquivalentMembers(alternative_casts.Length));
            var section_types = await c.SectionTypes.Include(st => st.Sections).ToArrayAsync();
            foreach (var section_type in section_types)
            {
                if (section_type.AllowNoRoles && section_type.AllowMultipleRoles && section_type.AllowConsecutiveItems)
                    continue; // nothing to check
                foreach (var section in section_type.Sections)
                {
                    if (!section.CastingMeetsSectionTypeRules(total_cast, out var no_roles, out var multi_roles))
                    {
                        if (no_roles > 0)
                            Rows.Add(new Row { Fail = $"{no_roles.Plural("Applicant has", "Applicants have")} no role in {section.Name}" });
                        if (multi_roles > 0)
                            Rows.Add(new Row { Fail = $"{multi_roles.Plural("Applicant has", "Applicants have")} multiple roles in {section.Name}" });
                    }
                    if (!section.VerifyConsecutiveItems(out var section_failures))
                        foreach (var failure in section_failures)
                            consecutive_item_failures.Add(failure);
                }
            }
            // append combined showroot & section consecutive item failures
            foreach (var failure in consecutive_item_failures)
                Rows.Add(new Row { Fail = $"{failure.Cast.Count.Plural("Applicant is", "Applicants are")} in {failure.Item1.Name} and {failure.Item2.Name}" });
            FinishLoad(cancel, roles_cast == 0 || roles_blank != 0);
        }
    }
}
