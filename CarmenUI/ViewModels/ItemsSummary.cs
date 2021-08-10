﻿using Microsoft.EntityFrameworkCore;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class ItemsSummary : Summary
    {
        public override async Task LoadAsync(ShowContext c)
        {
            StartLoad();
            await c.Nodes.Include(i => i.CountByGroups).ThenInclude(cbg => cbg.CastGroup).LoadAsync();
            await c.Nodes.OfType<Item>().Include(i => i.Roles).ThenInclude(r => r.CountByGroups).ThenInclude(cbg => cbg.CastGroup).LoadAsync();
            await c.Nodes.OfType<InnerNode>().Include(n => n.Children).LoadAsync();
            var items_in_order = c.ShowRoot.ItemsInOrder().ToList();
            Rows.Add(CreateItemsRow(items_in_order, out int item_count));
            await c.CastGroups.Include(cg => cg.Members).LoadAsync();
            var cast_members = c.CastGroups.Local.ToDictionary(cg => cg, cg => (uint)cg.Members.Count);
            await c.SectionTypes.Include(st => st.Sections).ThenInclude(s => s.CountByGroups).ThenInclude(cbg => cbg.CastGroup).LoadAsync();
            foreach (var section_type in c.SectionTypes.Local)
                Rows.Add(CreateSectionTypeRow(section_type, cast_members));
            var role_count = items_in_order.SelectMany(i => i.Roles).Distinct().Count();
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
            foreach (var item in items) //LATER paralleise
            {
                if (item.Roles.Count == 0)//TODO should a Role with CBG=0 be considered "without roles"? highlighting currently implemented on ConfigureItems UI does, so either change this or change that to match
                    without_roles += 1;
                else if (!item.CountMatchesSumOfRoles()) //LATER does this need await?
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
            foreach (var section in section_type.Sections) //LATER parallelise
            {//TODO highlight on ConfigureItems UI if roles in non-multi/non-zero section dont match cast members
                var role_match = section.RolesMatchCastMembers(cast_members); //LATER does this need await?
                if (role_match == Section.RolesMatchResult.TooFewRoles)
                    missing_roles += 1;
                else if (role_match == Section.RolesMatchResult.TooManyRoles)
                    too_many_roles += 1;
                else if (!section.CountMatchesSumOfRoles())//TODO highlight on ConfigureItems UI if section/showroot CBGs dont match sum of roles, also check showroot here?
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
