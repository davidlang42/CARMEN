using ShowModel;
using ShowModel.Applicants;
using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class ItemsSummary : Summary
    {
        public override async Task LoadAsync(ShowContext context)
        {
            StartLoad();
            var nodes = await context.ColdLoadAsync(c => c.Nodes);
            Rows.Add(CreateItemsRow(nodes.OfType<Item>(), out int item_count));
            var cast_groups = await context.ColdLoadAsync(c => c.CastGroups);
            var cast_members = cast_groups.ToDictionary(cg => cg, cg => (uint)cg.Members.Count);
            var section_types = await context.ColdLoadAsync(c => c.SectionTypes);
            foreach (var section_type in section_types)
                Rows.Add(CreateSectionTypeRow(section_type, cast_members));
            var role_count = context.ShowRoot.ItemsInOrder().SelectMany(i => i.Roles).Distinct().Count();
            Rows.Add(new Row { Success = $"{role_count} Roles" });
            if (item_count == 0)
                Rows.Add(new Row { Fail = "At least one Item is required" });
            if (role_count == 0)
                Rows.Add(new Row { Fail = "At least one Role is required" });
            FinishLoad(true);
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
            var row = new Row { Success = $"{item_count} Items" };
            if (without_roles != 0 && incorrect_sum != 0)
                row.Fail = $"({without_roles} without roles, {incorrect_sum.Plural("incorrect sum")})";
            else if (without_roles != 0)
                row.Fail = $"({without_roles} without roles)";
            else if (incorrect_sum != 0)
                row.Fail = incorrect_sum.Plural("incorrect sum");
            return row;
        }

        private static Row CreateSectionTypeRow(SectionType section_type, Dictionary<CastGroup, uint> cast_members)
        {
            var section_count = 0;
            var missing_roles = 0;
            var too_many_roles = 0;
            var incorrect_sum = 0;
            foreach (var section in section_type.Sections)
            {
                var role_match = section.RolesMatchCastMembers(cast_members);
                if (role_match == Section.RolesMatchResult.TooFewRoles)
                    missing_roles += 1;
                else if (role_match == Section.RolesMatchResult.TooManyRoles)
                    too_many_roles += 1;
                else if (!section.CountMatchesSumOfRoles())
                    incorrect_sum += 1;
                section_count += 1;
            }
            var row = new Row { Success = $"{section_count} {section_type.Name}s" };
            var fails = new List<string>();
            if (missing_roles != 0)
                fails.Add($"{missing_roles} missing roles");
            if (too_many_roles != 0)
                fails.Add($"({too_many_roles} with too many roles");
            if (incorrect_sum != 0)
                fails.Add(incorrect_sum.Plural("incorrect sum"));
            if (fails.Count != 0)
                row.Fail = $"({string.Join(", ", fails)})";
            return row;
        }
    }
}
