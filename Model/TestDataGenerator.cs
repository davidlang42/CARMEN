﻿using Model.Criterias;
using Model.Requirements;
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
        // SOURCE: https://namecensus.com/ (16 May 2021)
        static string[] LAST_NAMES = "SMITH,JOHNSON,WILLIAMS,JONES,BROWN,DAVIS,MILLER,WILSON,MOORE,TAYLOR,ANDERSON,THOMAS,JACKSON,WHITE,HARRIS,MARTIN,THOMPSON,GARCIA,MARTINEZ,ROBINSON,CLARK,RODRIGUEZ,LEWIS,LEE,WALKER,HALL,ALLEN,YOUNG,HERNANDEZ,KING,WRIGHT,LOPEZ,HILL,SCOTT,GREEN,ADAMS,BAKER,GONZALEZ,NELSON,CARTER,MITCHELL,PEREZ,ROBERTS,TURNER,PHILLIPS,CAMPBELL,PARKER,EVANS,EDWARDS,COLLINS,STEWART,SANCHEZ,MORRIS,ROGERS,REED,COOK,MORGAN,BELL,MURPHY,BAILEY,RIVERA,COOPER,RICHARDSON,COX,HOWARD,WARD,TORRES,PETERSON,GRAY,RAMIREZ,JAMES,WATSON,BROOKS,KELLY,SANDERS,PRICE,BENNETT,WOOD,BARNES,ROSS,HENDERSON,COLEMAN,JENKINS,PERRY,POWELL,LONG,PATTERSON,HUGHES,FLORES,WASHINGTON,BUTLER,SIMMONS,FOSTER,GONZALES,BRYANT,ALEXANDER,RUSSELL,GRIFFIN,DIAZ,HAYES".Split(',');
        static string[] MALE_FIRST_NAMES = "JAMES,JOHN,ROBERT,MICHAEL,WILLIAM,DAVID,RICHARD,CHARLES,JOSEPH,THOMAS,CHRISTOPHER,DANIEL,PAUL,MARK,DONALD,GEORGE,KENNETH,STEVEN,EDWARD,BRIAN,RONALD,ANTHONY,KEVIN,JASON,MATTHEW,GARY,TIMOTHY,JOSE,LARRY,JEFFREY,FRANK,SCOTT,ERIC,STEPHEN,ANDREW,RAYMOND,GREGORY,JOSHUA,JERRY,DENNIS,WALTER,PATRICK,PETER,HAROLD,DOUGLAS,HENRY,CARL,ARTHUR,RYAN,ROGER,JOE,JUAN,JACK,ALBERT,JONATHAN,JUSTIN,TERRY,GERALD,KEITH,SAMUEL,WILLIE,RALPH,LAWRENCE,NICHOLAS,ROY,BENJAMIN,BRUCE,BRANDON,ADAM,HARRY,FRED,WAYNE,BILLY,STEVE,LOUIS,JEREMY,AARON,RANDY,HOWARD,EUGENE,CARLOS,RUSSELL,BOBBY,VICTOR,MARTIN,ERNEST,PHILLIP,TODD,JESSE,CRAIG,ALAN,SHAWN,CLARENCE,SEAN,PHILIP,CHRIS,JOHNNY,EARL,JIMMY,ANTONIO".Split(',');
        static string[] FEMALE_FIRST_NAMES = "MARY,PATRICIA,LINDA,BARBARA,ELIZABETH,JENNIFER,MARIA,SUSAN,MARGARET,DOROTHY,LISA,NANCY,KAREN,BETTY,HELEN,SANDRA,DONNA,CAROL,RUTH,SHARON,MICHELLE,LAURA,SARAH,KIMBERLY,DEBORAH,JESSICA,SHIRLEY,CYNTHIA,ANGELA,MELISSA,BRENDA,AMY,ANNA,REBECCA,VIRGINIA,KATHLEEN,PAMELA,MARTHA,DEBRA,AMANDA,STEPHANIE,CAROLYN,CHRISTINE,MARIE,JANET,CATHERINE,FRANCES,ANN,JOYCE,DIANE,ALICE,JULIE,HEATHER,TERESA,DORIS,GLORIA,EVELYN,JEAN,CHERYL,MILDRED,KATHERINE,JOAN,ASHLEY,JUDITH,ROSE,JANICE,KELLY,NICOLE,JUDY,CHRISTINA,KATHY,THERESA,BEVERLY,DENISE,TAMMY,IRENE,JANE,LORI,RACHEL,MARILYN,ANDREA,KATHRYN,LOUISE,SARA,ANNE,JACQUELINE,WANDA,BONNIE,JULIA,RUBY,LOIS,TINA,PHYLLIS,NORMA,PAULA,DIANA,ANNIE,LILLIAN,EMILY,ROBIN".Split(',');

        static DateTime MINIMUM_DOB = new DateTime(1970, 1, 1);
        static DateTime MAXIMUM_DOB = new DateTime(2011, 1, 1);

        ShowContext? _context;
        Random random;

        internal ShowContext Context => _context ?? throw new ApplicationException("Context is not set, has this already been disposed?");

        public TestDataGenerator(ShowContext context, int random_seed)
            : this(context, new Random(random_seed))
        { }

        public TestDataGenerator(ShowContext context, Random? random = null)
        {
            this._context = context;
            this.random = random ?? new Random();
        }

        public void Dispose()
        {
            _context = null;
        }

        private void AddToNode(InnerNode node, ref uint next_item_number, ref uint next_section_number, uint items_in_this_node, uint sections_in_this_node, uint items_per_sub_section, uint sections_per_sub_section, uint section_depth, SectionType section_type, bool include_items_at_every_depth)
        {
            int order_in_node = node.Children.NextOrder();
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

        /// <summary>Adds applicants in a specified gender ratio with random first and last name, optionally filling their abilities with random marks for each existing criteria.
        /// NOTE: Must be called after Criteria have been committed.</summary>
        public void AddApplicants(uint count, double gender_ratio = 0.5, bool include_abilities = true)
        {
            if (gender_ratio > 1 || gender_ratio < 0)
                throw new ArgumentException($"{nameof(gender_ratio)} must be between 0 and 1 (inclusive)");
            uint male = (uint)(count * gender_ratio);
            uint female = count - male;
            AddApplicants(male, Gender.Male, include_abilities);
            AddApplicants(female, Gender.Female, include_abilities);
        }

        /// <summary>Adds applicants of a specified gender with random first and last name, optionally filling their abilities with random marks for each existing criteria.
        /// NOTE: Must be called after Criteria have been committed.</summary>
        public void AddApplicants(uint count, Gender gender, bool include_abilities = true)
        {
            var first_names = gender == Gender.Male ? MALE_FIRST_NAMES : FEMALE_FIRST_NAMES;
            var criterias = Context.Criteria.ToArray();
            for (var i = 0; i < count; i++)
            {
                var applicant = new Applicant
                {
                    FirstName = FormatName(first_names[random.Next(first_names.Length)]),
                    LastName = FormatName(LAST_NAMES[random.Next(LAST_NAMES.Length)]),
                    Gender = gender,
                    DateOfBirth = RandomDate(random, MINIMUM_DOB, MAXIMUM_DOB)
                };
                if (include_abilities)
                {
                    foreach (var criteria in criterias)
                    {
                        var ability = new Ability
                        {
                            Criteria = criteria,
                            Mark = (uint)random.Next((int)criteria.MaxMark)
                        };
                        applicant.Abilities.Add(ability);
                    }
                }
                Context.Applicants.Add(applicant);
            }
        }

        private uint TotalSections(uint sections_per_section, uint section_depth)
            => Convert.ToUInt32(Enumerable.Range(1, (int)section_depth).Select(d => Math.Pow(sections_per_section, d)).Sum());

        private DateTime RandomDate(Random r, DateTime minimum, DateTime maximum)
        {
            var span = maximum - minimum;
            int days = Convert.ToInt32(span.TotalDays);
            return minimum.AddDays(r.Next(days));
        }

        /// <summary>Adds cast groups, mutually exclusive by default, optionally with a required count.</summary>
        public void AddCastGroups(uint count, bool mutually_exclusive = true, uint? required_count = null)
        {
            int order = Context.CastGroups.NextOrder();
            for (var i = 0; i < count; i++)
            {
                var cast_group = new CastGroup
                {
                    Name = $"Group {i + 1}",
                    Order = order++,
                    MutuallyExclusive = mutually_exclusive,
                    RequiredCount = required_count
                };
                Context.CastGroups.Add(cast_group);
            }
        }

        /// <summary>Adds one of each type of Criteria, and one of each type of Requirements.
        /// NOTE: Must be called after at least one Cast Group has been committed.</summary>
        public void AddCriteriaAndRequirements()
        {
            int order = Context.Criteria.NextOrder();
            var numeric = new NumericCriteria
            {
                Name = $"Numeric Criteria",
                Order = order++,
                MaxMark = 100,
            };
            var select = new SelectCriteria
            {
                Name = $"Select Criteria",
                Order = order++,
                Options = new[] { "Option A", "Option B", "Option C" }
            };
            var boolean = new BooleanCriteria
            {
                Name = $"Boolean Criteria",
                Order = order++,
            };
            Context.Criteria.AddRange(numeric, select, boolean);
            order = Context.Requirements.NextOrder();
            var ability_exact = new AbilityExactRequirement
            {
                Criteria = boolean,
                Name = "Boolean is True",
                Order = order++,
                RequiredValue = 1
            };
            var ability_range = new AbilityRangeRequirement
            {
                Criteria = numeric,
                Name = "Numeric > 70",
                Order = order++,
                Minimum = 70
            };
            var age = new AgeRequirement
            {
                Name = "Adult",
                Minimum = 18,
                Order = order++
            };
            var gender = new GenderRequirement
            {
                Name = "Male",
                RequiredValue = (int)Gender.Male,
                Order = order++
            };
            var not_req = new NotRequirement
            {
                Name = "Not Male",
                SubRequirement = gender,
                Order = order++
            };
            if (!Context.CastGroups.Any())
                AddCastGroups(1);
            var cast_group = new CastGroupRequirement
            {
                Name = "First Cast Group",
                CastGroup = Context.CastGroups.First(),
                Order = order++
            };
            var and_req = new AndRequirement
            {
                Name = "Adult Male",
            };
            and_req.SubRequirements.Add(age);
            and_req.SubRequirements.Add(gender);
            var or_req = new OrRequirement
            {
                Name = "Numeric > 70, or Male",
            };
            or_req.SubRequirements.Add(ability_range);
            or_req.SubRequirements.Add(gender);
            var xor_req = new XorRequirement
            {
                Name = "Bool is True, or Male, but not both",
            };
            xor_req.SubRequirements.Add(ability_exact);
            xor_req.SubRequirements.Add(gender);
            Context.Requirements.AddRange(ability_exact, ability_range, age, cast_group, gender, not_req, and_req, or_req, xor_req);
        }

        /// <summary>Adds roles to items, optionally with random counts of a single cast group and random requirements.
        /// NOTE: Must be called after Items, Cast Groups and Requirements have been committed.</summary>
        public void AddRoles(uint roles_per_item, bool include_count_by_groups = true, bool include_requirements = true)
        {
            var cast_groups = Context.CastGroups.ToArray();
            var requirements = Context.Requirements.ToArray();
            foreach (var item in Context.ShowRoot.ItemsInOrder())
            {
                for (var i = 0; i < roles_per_item; i++)
                {
                    var role = new Role { Name = $"Role {i + 1}" };
                    if (include_count_by_groups)
                    {
                        var count_by_group = new CountByGroup
                        {
                            CastGroup = cast_groups[random.Next(cast_groups.Length)],
                            Count = (uint)i
                        };
                        if (i == 0)
                            count_by_group.Everyone = true;
                        role.CountByGroups.Add(count_by_group);
                    }
                    if (include_requirements)
                        role.Requirements.Add(requirements[random.Next(requirements.Length)]);
                    item.Roles.Add(role);
                }
            }
        }

        private string FormatName(string name)
            => name == "" ? "" : name.Substring(0, 1).ToUpper() + name.Substring(1).ToLower();
    }
}
