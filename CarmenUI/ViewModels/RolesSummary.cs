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
            var items = c.ShowRoot.ItemsInOrder().ToList();
            var roles = items.SelectMany(i => i.Roles).Distinct();
            var counts = roles.GroupBy(r => r.Status).ToDictionary(g => g.Key, g => g.Count()); //LATER does this need await?
            if (counts.TryGetValue(RoleStatus.FullyCast, out var roles_cast))
                Rows.Add(new Row { Success = $"{roles_cast} Roles cast" });
            if (counts.TryGetValue(RoleStatus.NotCast, out var roles_blank))
                Rows.Add(new Row { Success = $"{roles_blank} Roles not cast" });
            if (counts.TryGetValue(RoleStatus.UnderCast, out var roles_undercast))
                Rows.Add(new Row { Fail = $"{roles_undercast} Roles partially cast" });
            if (counts.TryGetValue(RoleStatus.OverCast, out var roles_overcast))
                Rows.Add(new Row { Fail = $"{roles_overcast} Roles with too many cast" });            
            await c.CastGroups.Include(cg => cg.Members).LoadAsync();
            var total_cast = c.CastGroups.Local.Sum(cg => cg.Members.Count);
            await c.SectionTypes.Include(st => st.Sections).LoadAsync();
            foreach (var section_type in c.SectionTypes.Local)
            {
                if (section_type.AllowNoRoles && section_type.AllowMultipleRoles)
                    continue; // nothing to check
                foreach (var section in section_type.Sections)
                {
                    var roles_in_section = section.ItemsInOrder().SelectMany(i => i.Roles).ToHashSet(); //LATER does this need await?
                    if (section_type.AllowNoRoles == false)
                    {
                        var no_roles = total_cast - roles_in_section.SelectMany(r => r.Cast).Distinct().Count(); //LATER does this need await?
                        if (no_roles > 0)
                            Rows.Add(new Row { Fail = $"{no_roles.Plural("Applicant has", "Applicants have")} no role in {section.Name}" });
                    }
                    if (section_type.AllowMultipleRoles == false)
                    {
                        var multi_roles = roles_in_section.SelectMany(r => r.Cast).GroupBy(a => a).Where(g => g.Count() > 1).Count(); //LATER does this need await?
                        if (multi_roles > 0)
                            Rows.Add(new Row { Fail = $"{multi_roles.Plural("Applicant has", "Applicants have")} multiple roles in {section.Name}" });
                    }
                }
            }
            var cast_per_item = items.Select(i => i.Roles.SelectMany(r => r.Cast).ToHashSet()).ToList(); //LATER does this need await? also parallelise
            for (var i = 1; i < items.Count; i++)
            {
                var cast_in_consecutive_items = cast_per_item[i].Intersect(cast_per_item[i-1]).Count(); //LATER does this need await? also confirm that in-built Intersect() isn't slower than hashset.select(i => other_hashset.contains(i)).count()
                if (cast_in_consecutive_items != 0)
                    Rows.Add(new Row { Fail = $"{cast_in_consecutive_items.Plural("Applicant is", "Applicants are")} in {items[i-1].Name} and {items[i].Name}" });
            }
            FinishLoad(roles_blank == 0);
        }
    }
}
