using ShowModel;
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
        public override async Task LoadAsync(ShowContext context)
        {
            StartLoad();
            var roles = context.ShowRoot.ItemsInOrder().SelectMany(i => i.Roles).Distinct(); // TODO common with ItemsSummary
            var counts = roles.GroupBy(r => r.Status).ToDictionary(g => g.Key, g => g.Count());
            if (counts.TryGetValue(RoleStatus.FullyCast, out var roles_cast))
                Rows.Add(new Row { Success = $"{roles_cast} Roles cast" });
            if (counts.TryGetValue(RoleStatus.NotCast, out var roles_blank))
                Rows.Add(new Row { Success = $"{roles_blank} Roles not cast" });
            if (counts.TryGetValue(RoleStatus.UnderCast, out var roles_undercast))
                Rows.Add(new Row { Fail = $"{roles_undercast} Roles partially cast" });
            if (counts.TryGetValue(RoleStatus.OverCast, out var roles_overcast))
                Rows.Add(new Row { Fail = $"{roles_overcast} Roles with too many cast" });
            var section_types = await context.ColdLoadAsync(c => c.SectionTypes);
            var cast_groups = await context.ColdLoadAsync(c => c.CastGroups);
            var total_cast = cast_groups.Sum(cg => cg.Members.Count);
            foreach (var section_type in section_types)
            {
                if (section_type.AllowNoRoles && section_type.AllowMultipleRoles)
                    continue; // nothing to check
                foreach (var section in section_type.Sections)
                {
                    var roles_in_section = section.ItemsInOrder().SelectMany(i => i.Roles).ToHashSet();
                    if (section_type.AllowNoRoles == false)
                    {
                        var no_roles = total_cast - roles_in_section.SelectMany(r => r.Cast).Distinct().Count();
                        if (no_roles > 0)
                            Rows.Add(new Row { Fail = $"{no_roles.Plural("Applicant has", "Applicants have")} no role in {section.Name}" });
                    }
                    if (section_type.AllowMultipleRoles == false)
                    {
                        var multi_roles = roles_in_section.SelectMany(r => r.Cast).GroupBy(a => a).Where(g => g.Count() > 1).Count();
                        if (multi_roles > 0)
                            Rows.Add(new Row { Fail = $"{multi_roles.Plural("Applicant has", "Applicants have")} multiple roles in {section.Name}" });
                    }
                }
            }
            //TODO consecutive items check (eg. 1 Applicant is in two consecutive items)
            FinishLoad(counts[RoleStatus.NotCast] == 0);
        }
    }
}
