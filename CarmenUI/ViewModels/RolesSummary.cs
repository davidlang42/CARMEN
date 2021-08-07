using Microsoft.EntityFrameworkCore;
using ShowModel;
using ShowModel.Applicants;
using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ShowModel.Structure.Role;

namespace CarmenUI.ViewModels
{
    public class RolesSummary : Summary
    {
        public override async Task LoadAsync(ShowContext c)
        {
            StartLoad();
            await c.Nodes.OfType<Item>().Include(i => i.Roles).ThenInclude(r => r.CountByGroups).ThenInclude(cbg => cbg.CastGroup).LoadAsync();
            await c.Nodes.OfType<Item>().Include(i => i.Roles).ThenInclude(r => r.Cast).LoadAsync();
            await c.Nodes.OfType<InnerNode>().Include(n => n.Children).LoadAsync();
            // check role statuses
            var roles = c.ShowRoot.ItemsInOrder().SelectMany(i => i.Roles).Distinct();
            var counts = roles.GroupBy(r => r.Status).ToDictionary(g => g.Key, g => g.Count()); //LATER does this need await?
            if (counts.TryGetValue(RoleStatus.FullyCast, out var roles_cast))
                Rows.Add(new Row { Success = $"{roles_cast} Roles cast" });
            if (counts.TryGetValue(RoleStatus.NotCast, out var roles_blank))
                Rows.Add(new Row { Success = $"{roles_blank} Roles not cast" });
            if (counts.TryGetValue(RoleStatus.UnderCast, out var roles_undercast))
                Rows.Add(new Row { Fail = $"{roles_undercast} Roles partially cast" });
            if (counts.TryGetValue(RoleStatus.OverCast, out var roles_overcast))
                Rows.Add(new Row { Fail = $"{roles_overcast} Roles with too many cast" });            
            // verify section type rules
            await c.CastGroups.Include(cg => cg.Members).LoadAsync();
            var total_cast = c.CastGroups.Local.Sum(cg => cg.Members.Count);
            await c.SectionTypes.Include(st => st.Sections).LoadAsync();
            foreach (var section_type in c.SectionTypes.Local)
            {
                if (section_type.AllowNoRoles && section_type.AllowMultipleRoles)
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
                }
            }
            // verify consecutive items
            if (!c.ShowRoot.VerifyConsecutiveItems(out var failures))
                foreach (var failure in failures)
                    Rows.Add(new Row { Fail = $"{failure.CastCount.Plural("Applicant is", "Applicants are")} in {failure.Item1.Name} and {failure.Item2.Name}" });
            FinishLoad(roles_blank == 0);
        }
    }
}
