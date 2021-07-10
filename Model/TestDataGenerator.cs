using Model.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// A generator of test data for the ShowContext data model.
    /// </summary>
    public class TestDataGenerator : IDisposable
    {
        ShowContext? _context;

        internal ShowContext Context => _context ?? throw new ApplicationException("Context is not set, has this already been disposed?");

        public TestDataGenerator(ShowContext context)
        {
            this._context = context;
        }

        public void Dispose()
        {
            _context = null;
        }

        private void AddToNode(InnerNode node, ref uint next_item_number, ref uint next_section_number, uint items_in_this_node, uint sections_in_this_node, uint items_per_sub_section, uint sections_per_sub_section, uint section_depth, SectionType section_type, bool include_items_at_every_depth)
        {
            int order_in_node = node.Children.Any() ? node.Children.Select(c => c.Order).Max() + 1 : 0;
            if (section_depth != 0)
            {
                for (var s = 0; s < sections_in_this_node; s++)
                {
                    var section = new Section {
                        Name = $"Section {next_section_number++}",
                        Order = order_in_node++,
                        SectionType = section_type
                    };
                    AddToNode(section, ref next_item_number, ref next_section_number, items_per_sub_section, sections_per_sub_section, items_per_sub_section, sections_per_sub_section, section_depth - 1, section_type, include_items_at_every_depth);
                    node.Children.Add(section);
                }
            }
            if (include_items_at_every_depth || section_depth == 0)
            {
                for (var i = 0; i < items_in_this_node; i++)
                {
                    var item = new Item
                    {
                        Name = $"Item {next_item_number++}",
                        Order = order_in_node++
                    };
                    node.Children.Add(item);
                }
            }
        }

        /// <summary>Add a specified number of items and sections to the show structure.
        /// NOTE: <paramref name="section_depth"/> may not be met in all circumstances.</summary>
        public void AddShowStructure(uint total_items, uint total_sections = 0, uint section_depth = 1, SectionType? section_type = null, bool include_items_at_every_depth = true)
        {
            // Set up show
            var show = Context.ShowRoot; // adds blank ShowRoot if it doesn't exist
            if (show.Name == "")
                show.Name = "Test Show";

            // Set up section type
            if (section_type == null)
                section_type = Context.SectionTypes.FirstOrDefault() ?? Context.Add(new SectionType()).Entity;

            // Calculate sections
            uint extra_sections_in_root;
            uint sections_per_section;
            if (total_sections == 0) {
                extra_sections_in_root = 0;
                sections_per_section = 0;
            } else {
                if (section_depth == 0)
                    section_depth = 1;
                if (section_depth > total_sections)
                    section_depth = total_sections;
                sections_per_section = 1;
                while (TotalSections(sections_per_section + 1, section_depth) <= total_sections)
                    sections_per_section++;
                extra_sections_in_root = total_sections - TotalSections(sections_per_section, section_depth);
            }

            // Calculate items
            uint sections_with_items;
            if (include_items_at_every_depth)
                sections_with_items = total_sections + 1; // all new sections + show root
            else
                sections_with_items = Convert.ToUInt32(Math.Pow(sections_per_section, section_depth)) + extra_sections_in_root; // only sections with no children
            uint items_per_section = total_items / sections_with_items;
            uint extra_items_in_root = total_items - items_per_section * sections_with_items;
            
            // Add sections & items recursively
            uint next_item_number = 1;
            uint next_section_number = 1;
            AddToNode(show, ref next_item_number, ref next_section_number, items_per_section, sections_per_section, items_per_section, sections_per_section, section_depth, section_type, include_items_at_every_depth);
            AddToNode(show, ref next_item_number, ref next_section_number, extra_items_in_root, extra_sections_in_root, items_per_section, 0, 1, section_type, true);

            // Assert the correct number of things added
            if (next_item_number - 1 != total_items)
                throw new ApplicationException($"Tried to add {total_items} items, but added {next_item_number - 1}");
            if (next_section_number - 1 != total_sections)
                throw new ApplicationException($"Tried to add {total_sections} sections, but added {next_section_number - 1}");
        }

        private uint TotalSections(uint sections_per_section, uint section_depth)
            => Convert.ToUInt32(Enumerable.Range(1, (int)section_depth).Select(d => Math.Pow(sections_per_section, d)).Sum());
    }
}
