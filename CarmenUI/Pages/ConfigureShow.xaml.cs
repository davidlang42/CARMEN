using CarmenUI.Converters;
using CarmenUI.ViewModels;
using CarmenUI.Windows;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using A = Carmen.ShowModel.Applicants;

namespace CarmenUI.Pages
{
    /// <summary>
    /// Interaction logic for ConfigureShow.xaml
    /// </summary>
    public partial class ConfigureShow : SubPage
    {
        private readonly CollectionViewSource criteriasViewSource = new()
        {
            IsLiveSortingRequested = true,
            SortDescriptions = { StandardSort.For<Criteria>() }
        };
        private readonly CollectionViewSource criteriasSelectionSource; // xaml resource loaded in constructor
        private readonly CollectionViewSource castGroupsViewSource; // xaml resource loaded in constructor
        private readonly CollectionViewSource alternativeCastsViewSource; // xaml resource loaded in constructor
        private readonly CollectionViewSource tagsViewSource = new() { SortDescriptions = { StandardSort.For<Tag>() } };
        private readonly CollectionViewSource tagsSelectionSource; // xaml resource loaded in constructor
        private readonly CollectionViewSource sectionTypesViewSource = new() { SortDescriptions = { StandardSort.For<SectionType>() } };
        private readonly CollectionViewSource requirementsViewSource = new()
        {
            IsLiveSortingRequested = true,
            SortDescriptions = { StandardSort.For<Requirement>() }
        };
        private readonly CollectionViewSource requirementsSelectionSource; // xaml resource loaded in constructor
        private readonly CollectionViewSource listSortDirectionEnumSource; // xaml resource loaded in constructor
        private readonly CollectionViewSource showRootSource; // xaml resource loaded in constructor

        private CollectionViewSource? currentViewSource;

        public ConfigureShow(RecentShow connection) : base(connection)
        {
            InitializeComponent();
            alternativeCastsViewSource = (CollectionViewSource)FindResource(nameof(alternativeCastsViewSource));
            alternativeCastsViewSource.SortDescriptions.Add(StandardSort.For<AlternativeCast>());
            criteriasSelectionSource = (CollectionViewSource)FindResource(nameof(criteriasSelectionSource));
            criteriasSelectionSource.SortDescriptions.Add(StandardSort.For<Criteria>());
            castGroupsViewSource = (CollectionViewSource)FindResource(nameof(castGroupsViewSource));
            castGroupsViewSource.SortDescriptions.Add(StandardSort.For<CastGroup>());
            tagsSelectionSource = (CollectionViewSource)FindResource(nameof(tagsSelectionSource));
            tagsSelectionSource.SortDescriptions.Add(StandardSort.For<Tag>());
            showRootSource = (CollectionViewSource)FindResource(nameof(showRootSource));
            requirementsSelectionSource = (CollectionViewSource)FindResource(nameof(requirementsSelectionSource));
            requirementsSelectionSource.SortDescriptions.Add(StandardSort.For<Requirement>());
            listSortDirectionEnumSource = (CollectionViewSource)FindResource(nameof(listSortDirectionEnumSource));
            listSortDirectionEnumSource.Source = Enum.GetValues<ListSortDirection>();
            configList.SelectedIndex = 0; // must be set after InitializeComponent() because it triggers Selected event below
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            using var loading = new LoadingOverlay(this).AsSegment(nameof(ConfigureShow));
            using (loading.Segment(nameof(ShowContext.Requirements), "Requirements"))
                await context.Requirements.LoadAsync();
            requirementsViewSource.Source = requirementsSelectionSource.Source = context.Requirements.Local.ToObservableCollection();
            using (loading.Segment(nameof(ShowContext.Criterias), "Criteria"))
                await context.Criterias.LoadAsync();
            criteriasViewSource.Source = criteriasSelectionSource.Source = context.Criterias.Local.ToObservableCollection();
            using (loading.Segment(nameof(ShowContext.CastGroups) + nameof(CastGroup.Requirements), "Cast groups"))
                await context.CastGroups.Include(cg => cg.Requirements).LoadAsync();
            castGroupsViewSource.Source = context.CastGroups.Local.ToObservableCollection();
            using (loading.Segment(nameof(ShowContext.AlternativeCasts), "Alternative casts"))
                await context.AlternativeCasts.LoadAsync();
            alternativeCastsViewSource.Source = context.AlternativeCasts.Local.ToObservableCollection();
            using (loading.Segment(nameof(ShowContext.Tags) + nameof(A.Tag.Requirements), "Tags"))
                await context.Tags.Include(t => t.Requirements).LoadAsync();
            tagsViewSource.Source = tagsSelectionSource.Source = context.Tags.Local.ToObservableCollection();
            using (loading.Segment(nameof(ShowContext.SectionTypes), "Section types"))
                await context.SectionTypes.LoadAsync();
            sectionTypesViewSource.Source = context.SectionTypes.Local.ToObservableCollection();
            showRootSource.Source = context.ShowRoot.Yield();
        }

        private void BindObjectList(string header, bool show_move_buttons, CollectionViewSource view_source, AddableObject[][] add_buttons, bool hide_panel = false)
        {
            objectPanelColumn.Width = hide_panel ? new(0) : new(1, GridUnitType.Star);
            moveButtons.Visibility = show_move_buttons ? Visibility.Visible : Visibility.Collapsed;
            objectHeading.Text = header;
            objectList.ToolTip = show_move_buttons ? null : "Sorted by name";
            objectList.SetBinding(ItemsControl.ItemsSourceProperty, new Binding { Source = view_source });
            currentViewSource = view_source;
            objectAddButtons.ItemsSource = add_buttons;
        }

        private void ShowRoot_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("", false, showRootSource, new AddableObject[0][], true);

        readonly AddableObject[][] criteriasButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Numeric Criteria", () => new NumericCriteria { Name = "New Numeric Criteria" }),
                new AddableObject("Add Tick Criteria", () => new BooleanCriteria { Name = "New Tick Criteria" }),
                new AddableObject("Add List Criteria", () => new SelectCriteria { Name = "New List Criteria" })
            }
        };

        private void Criteria_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Audition Criteria", true, criteriasViewSource, criteriasButtons);

        readonly AddableObject[][] castGroupsButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Cast Group", () => new CastGroup { Name = "New Cast Group" })
            }
        };

        private void CastGroups_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Cast Groups", true, castGroupsViewSource, castGroupsButtons);


        readonly AddableObject[][] alternativeCastsButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Alternative Cast", () => new AlternativeCast { Name = "New Alternative Cast" })
            }
        };

        private void AlternativeCasts_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Alternative Casts", false, alternativeCastsViewSource, alternativeCastsButtons);

        readonly AddableObject[][] tagsButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Tag", () => new Tag { Name = "New Tag" })
            }
        };

        private void Tags_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Tags", false, tagsViewSource, tagsButtons);

        readonly AddableObject[][] sectionTypesButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Section Type", () => new SectionType { Name = "New Section Type" })
            }
        };

        private void SectionTypes_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Section Types", false, sectionTypesViewSource, sectionTypesButtons);

        readonly AddableObject[][] requirementsButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Age", () => new AgeRequirement { Name = "New Age Requirement" }),
                new AddableObject("Add Gender", () => new GenderRequirement { Name = "New Gender Requirement" }),
                new AddableObject("Add Tag", () => new TagRequirement { Name = "New Tag Requirement" }),
                new AddableObject("Add Exact Ability", () => new AbilityExactRequirement { Name = "New Ability Requirement (Exact)" }),
                new AddableObject("Add Ability Range", () => new AbilityRangeRequirement { Name = "New Ability Requirement (Range)" })
            },
            new[]
            {
                new AddableObject("Add 'AND'", () => new AndRequirement { Name = "New AND Requirement" }),
                new AddableObject("Add 'OR'", () => new OrRequirement { Name = "New OR Requirement" }),
                new AddableObject("Add 'XOR'", () => new XorRequirement { Name = "New XOR Requirement" }),
                new AddableObject("Add 'NOT'", () => new NotRequirement { Name = "New NOT Requirement" })
            }
        };

        private void Requirements_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Requirements", true, requirementsViewSource, requirementsButtons);

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => CancelChangesAndReturn();

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
            => await SaveChangesAndReturn();

        private void AddObjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentViewSource?.Source is IList list && !list.IsFixedSize)
            {
                var button = (Button)sender;
                var addable = (AddableObject)button.DataContext;
                var new_object = addable.CreateObject();
                if (new_object is IOrdered ordered)
                    ordered.Order = list.OfType<IOrdered>().NextOrder();
                list.Add(new_object);
                objectList.SelectedItem = new_object;
                editPanel.VisualDescendants<TextBox>().FirstOrDefault()?.Focus();
            }
        }

        private void DeleteObject_Click(object sender, RoutedEventArgs e)
        {
            if (objectList.ItemsSource is ListCollectionView list && objectList.SelectedItem is not ShowRoot)
                DeleteObject(list);
        }

        private void Name_LostFocus(object sender, RoutedEventArgs e)
        {
            currentViewSource?.View.Refresh();
        }

        private void objectList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Properties.Settings.Default.MoveOnCtrlArrow
                && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Up)
                {
                    moveUpButton_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.Key == Key.Down)
                {
                    moveDownButton_Click(sender, e);
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Delete && objectList.ItemsSource is ListCollectionView list)
            {
                e.Handled = true;
                DeleteObject(list);
            }
        }

        private void DeleteObject(ListCollectionView list)
        {
            switch (objectList.SelectedItem)
            {
                case Criteria criteria:
                    if (!criteria.Abilities.Any() || Confirm($"Are you sure you want to delete the '{criteria.Name}' criteria?\nThis will delete the {criteria.Name} mark of {criteria.Abilities.Count.Plural("applicant")}."))
                        list.Remove(criteria);
                    break;
                case CastGroup cast_group:
                    if (context.CastGroups.Local.Count == 1)
                    {
                        MessageBox.Show("You must have at least one Cast Group.", WindowTitle);
                        return;
                    }
                    if (!cast_group.Members.Any() || Confirm($"Are you sure you want to delete the '{cast_group.Name}' cast group?\nThis will remove the {cast_group.Members.Count.Plural($"currently selected {cast_group.Name}")} from the cast."))
                        list.Remove(cast_group);
                    break;
                case AlternativeCast alternative_cast:
                    bool any_members = alternative_cast.Members.Any();
                    int cast_groups_alternating = context.CastGroups.Local.Count(cg => cg.AlternateCasts);
                    bool needed_to_alternate = context.AlternativeCasts.Local.Count <= 2 && cast_groups_alternating > 0;
                    var allow_delete = (any_members, needed_to_alternate) switch
                    {
                        (true, true) => Confirm($"Are you sure you want to delete the '{alternative_cast.Name}' cast?\nThis will remove the {alternative_cast.Members.Count.Plural($"currently selected {alternative_cast.Name}")} from this alternative cast, and disable alternating casts on the {cast_groups_alternating.Plural("cast group")} for which it is enabled."),
                        (true, false) => Confirm($"Are you sure you want to delete the '{alternative_cast.Name}' cast?\nThis will remove the {alternative_cast.Members.Count.Plural($"currently selected {alternative_cast.Name}")} from this alternative cast."),
                        (false, true) => Confirm($"Are you sure you want to delete the '{alternative_cast.Name}' cast?\nThis will disable alternating casts on the {cast_groups_alternating.Plural("cast group")} for which it is enabled."),
                        _ => true // (false, false)
                    };
                    if (allow_delete)
                        context.DeleteAlternativeCast(alternative_cast);
                    break;
                case Tag tag:
                    if (!tag.Members.Any() || Confirm($"Are you sure you want to delete the '{tag.Name}' tag?\nThis will remove the tag from {tag.Members.Count.Plural($"currently tagged applicant")}."))
                        list.Remove(tag);
                    break;
                case SectionType section_type:
                    if (context.SectionTypes.Local.Count == 1)
                    {
                        MessageBox.Show("You must have at least one Section Type.", WindowTitle);
                        return;
                    }
                    if (!section_type.Sections.Any() || Confirm($"Are you sure you want to delete the '{section_type.Name}' section type?\nThis will remove the {section_type.Sections.Count.Plural(section_type.Name)} and any sections, items and roles within them."))
                        context.DeleteSectionType(section_type);
                    break;
                case Requirement requirement:
                    var used_count = requirement.UsedByCastGroups.Count + requirement.UsedByCombinedRequirements.Count + requirement.UsedByRoles.Count + requirement.UsedByTags.Count;//TODO is this missing NotRequirements?
                    if (used_count == 0 || Confirm($"Are you sure you want to delete the '{requirement.Name}' reqirement?\nThis will also delete any NOT requirements which refer to it, and is currently in used {used_count.Plural("time")}."))
                        context.DeleteRequirement(requirement);
                    break;
            }
        }

        private void ImportShowConfiguration_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Importing show configuration is not currently supported.\nWould you like to reset to the default configuration instead?", WindowTitle, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                context.SetDefaultShowSettings(connection.DefaultShowName, false);
        }

        private void moveUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentViewSource?.Source is IList list 
                && objectList.SelectedItem is IOrdered selected
                && objectList.SelectedIndex > 0)
            {
                var index_above = objectList.SelectedIndex - 1;
                var item_above = (IOrdered)objectList.Items[index_above];
                list.OfType<IOrdered>().ToArray().MoveInOrder(selected, item_above.Order);
                EnableMoveButtons(index_above);
            }
        }

        private void moveDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentViewSource?.Source is IList list
                && objectList.SelectedItem is IOrdered selected
                && objectList.SelectedIndex < objectList.Items.Count - 1)
            {
                var index_below = objectList.SelectedIndex + 1;
                var item_below = (IOrdered)objectList.Items[index_below];
                list.OfType<IOrdered>().ToArray().MoveInOrder(selected, item_below);
                EnableMoveButtons(index_below);
            }
        }

        private void objectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => EnableMoveButtons(objectList.SelectedIndex);

        private void EnableMoveButtons(int selected_index)
        {
            moveUpButton.IsEnabled = selected_index != -1 && selected_index > 0;
            moveDownButton.IsEnabled = selected_index != -1 && selected_index < objectList.Items.Count - 1;
        }

        protected override async Task<bool> PreSaveChecks()
        {
            List<string> validation_issues;
            using (new LoadingOverlay(this) { MainText = "Processing...", SubText = "Validating requirements" })
                validation_issues = await Task.Run(() =>
                {
                    var issues = context.Requirements.Local.ToList().SelectMany(vo => vo.Validate()).ToList();
                    if (issues.Count == 0)
                    {
                    // only validate CastGroups if Requirements/Tags are valid, otherwise there might be circular references
                    issues.AddRange(context.CastGroups.Local.SelectMany(vo => vo.Validate()));
                        issues.AddRange(context.Tags.Local.SelectMany(vo => vo.Validate()));
                    }
                    return issues;
                });
            if (validation_issues.Count == 0)
                return true;
            string msg;
            if (validation_issues.Count == 1)
                msg = validation_issues[0];
            else
                msg = $"There are {validation_issues.Count} validation issues.\n\n{string.Join("\n", validation_issues)}\n";
            msg += "\nChanges have not been saved.";
            MessageBox.Show(msg, "CARMEN");
            return false;
        }
    }
}
