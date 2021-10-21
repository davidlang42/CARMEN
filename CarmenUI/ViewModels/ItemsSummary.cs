using Microsoft.EntityFrameworkCore;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CarmenUI.ViewModels
{
    public class ItemsSummary : Summary
    {
        public override async Task LoadAsync(ShowContext c, CancellationToken cancel)
        {
            StartLoad();
            var items = await c.Nodes.OfType<Item>().Include(i => i.Roles).ToArrayAsync();
            var items_row = CreateItemsRow(items, out int item_count);
            bool anything_exists = item_count != 0;
            Rows.Add(items_row);
            var alternative_casts_count = await c.AlternativeCasts.CountAsync();
            var cast_members = await c.CastGroups.Include(cg => cg.Members).ToDictionaryAsync(cg => cg, cg => cg.FullTimeEquivalentMembers(alternative_casts_count));
            var section_types = await c.SectionTypes.Include(st => st.Sections).ThenInclude(n => n.Children).ToArrayAsync();
            if (!c.ShowRoot.CountMatchesSumOfRoles())
                Rows.Add(new Row { Fail = "Show has incorrect sum of roles" });
            foreach (var section_type in section_types)
            {
                Rows.Add(CreateSectionTypeRow(section_type, cast_members, out int section_count));
                anything_exists |= section_count != 0;
            }
            var roles = items.SelectMany(i => i.Roles).Distinct();
            Rows.Add(CreateRolesRow(roles, out int role_count));
            anything_exists |= role_count != 0;
            FinishLoad(cancel, !anything_exists);
        }

        private static Row CreateRolesRow(IEnumerable<Role> roles, out int role_count)
        {
            role_count = 0;
            var blank_name = 0;
            var zero_count = 0;
            foreach (var role in roles)
            {
                if (string.IsNullOrEmpty(role.Name))
                    blank_name += 1;
                if (role.CountByGroups.Sum(cbg => cbg.Count) == 0)
                    zero_count += 1;
                role_count += 1;
            }
            var row = new Row { Success = $"{role_count} Roles" };
            if (role_count == 0)
                row.Fail = $"(at least 1 required)";
            else if (blank_name != 0 && zero_count != 0)
                row.Fail = $"({blank_name} with blank name, {zero_count} with no cast required)";
            else if (blank_name != 0)
                row.Fail = $"({blank_name} with blank name)";
            else if (zero_count != 0)
                row.Fail = $"({zero_count} with no cast required)";
            return row;
        }

        private static Row CreateItemsRow(IEnumerable<Item> items, out int item_count)
        {
            item_count = 0;
            var without_roles = 0;
            var incorrect_sum = 0;
            foreach (var item in items)
            {
                if (item.Roles.Count == 0)
                    without_roles += 1;
                else if (!item.CountMatchesSumOfRoles())
                    incorrect_sum += 1;
                item_count += 1;
            }
            var row = new Row { Success = item_count.Plural("Item") };
            if (item_count == 0)
                row.Fail = $"(at least 1 required)";
            else if (without_roles != 0 && incorrect_sum != 0)
                row.Fail = $"({without_roles} without roles, {incorrect_sum.Plural("incorrect sum")})";
            else if (without_roles != 0)
                row.Fail = $"({without_roles} without roles)";
            else if (incorrect_sum != 0)
                row.Fail = $"({incorrect_sum.Plural("incorrect sum")})";
            return row;
        }

        private static Row CreateSectionTypeRow(SectionType section_type, Dictionary<CastGroup, uint> cast_members, out int section_count)
        {
            section_count = 0;
            var without_children = 0;
            var missing_roles = 0;
            var too_many_roles = 0;
            var incorrect_sum = 0;
            foreach (var section in section_type.Sections)
            {
                var role_match = section.RolesMatchCastMembers(cast_members);
                if (section.Children.Count == 0)
                    without_children += 1;
                else if (role_match == Section.RolesMatchResult.TooFewRoles)
                    missing_roles += 1;
                else if (role_match == Section.RolesMatchResult.TooManyRoles)
                    too_many_roles += 1;
                else if (!section.CountMatchesSumOfRoles())
                    incorrect_sum += 1;
                section_count += 1;
            }
            var row = new Row { Success = $"{section_count} {section_type.Name}s" };
            var fails = new List<string>();
            if (without_children != 0)
                fails.Add($"{without_children} without items");
            if (missing_roles != 0)
                fails.Add($"{missing_roles} missing roles");
            if (too_many_roles != 0)
                fails.Add($"{too_many_roles} with too many roles");
            if (incorrect_sum != 0)
                fails.Add(incorrect_sum.Plural("incorrect sum"));
            if (fails.Count != 0)
                row.Fail = $"({string.Join(", ", fails)})";
            return row;
        }
    }
}
