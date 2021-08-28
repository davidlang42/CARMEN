using CarmenUI.Converters;
using CarmenUI.ViewModels;
using CarmenUI.Windows;
using Microsoft.EntityFrameworkCore;
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
        string fileName;

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
        private readonly CollectionViewSource showRootSource = new();

        private CollectionViewSource? currentViewSource;

        public ConfigureShow(DbContextOptions<ShowContext> context_options, string file_name) : base(context_options)
        {
            fileName = file_name;
            InitializeComponent();
            alternativeCastsViewSource = (CollectionViewSource)FindResource(nameof(alternativeCastsViewSource));
            alternativeCastsViewSource.SortDescriptions.Add(StandardSort.For<AlternativeCast>());
            criteriasSelectionSource = (CollectionViewSource)FindResource(nameof(criteriasSelectionSource));
            criteriasSelectionSource.SortDescriptions.Add(StandardSort.For<Criteria>());
            castGroupsViewSource = (CollectionViewSource)FindResource(nameof(castGroupsViewSource));
            castGroupsViewSource.SortDescriptions.Add(StandardSort.For<CastGroup>());
            tagsSelectionSource = (CollectionViewSource)FindResource(nameof(tagsSelectionSource));
            tagsSelectionSource.SortDescriptions.Add(StandardSort.For<Tag>());
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

        private void BindObjectList(string header, string? tool_tip, CollectionViewSource view_source, AddableObject[][] add_buttons, bool hide_panel = false)
        {
            objectPanelColumn.Width = hide_panel ? new(0) : new(1, GridUnitType.Star);
            objectHeading.Text = header;
            objectList.ToolTip = tool_tip;
            objectList.SetBinding(ItemsControl.ItemsSourceProperty, new Binding { Source = view_source });
            currentViewSource = view_source;
            objectAddButtons.ItemsSource = add_buttons;
        }

        private void ShowRoot_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("", null, showRootSource, new AddableObject[0][], true);

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
            => BindObjectList("Audition Criteria", "Drag to re-order", criteriasViewSource, criteriasButtons);

        readonly AddableObject[][] castGroupsButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Cast Group", () => new CastGroup { Name = "New Cast Group" })
            }
        };

        private void CastGroups_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Cast Groups", "Drag to re-order", castGroupsViewSource, castGroupsButtons);


        readonly AddableObject[][] alternativeCastsButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Alternative Cast", () => new AlternativeCast { Name = "New Alternative Cast" })
            }
        };

        private void AlternativeCasts_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Alternative Casts", "Sorted by name", alternativeCastsViewSource, alternativeCastsButtons);

        readonly AddableObject[][] tagsButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Tag", () => new Tag { Name = "New Tag" })
            }
        };

        private void Tags_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Tags", "Sorted by name", tagsViewSource, tagsButtons);

        readonly AddableObject[][] sectionTypesButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Section Type", () => new SectionType { Name = "New Section Type" })
            }
        };

        private void SectionTypes_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Section Types", "Sorted by name", sectionTypesViewSource, sectionTypesButtons);

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
            => BindObjectList("Role Requirements", "Drag to re-order", requirementsViewSource, requirementsButtons);

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => CancelChangesAndReturn();

        private void SaveButton_Click(object sender, RoutedEventArgs e)
            => SaveChangesAndReturn();

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
            }
        }

        private void Name_LostFocus(object sender, RoutedEventArgs e)
        {
            currentViewSource?.View.Refresh();
        }

        private void objectList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && objectList.ItemsSource is ListCollectionView list)
            {
                e.Handled = true;
                switch (objectList.SelectedItem) {
                    case Criteria criteria:
                        if (!criteria.Abilities.Any() || ConfirmDelete($"Are you sure you want to delete the '{criteria.Name}' criteria?\nThis will delete the {criteria.Name} mark of {criteria.Abilities.Count.Plural("applicant")}."))
                            list.Remove(criteria);
                        break;
                    case CastGroup cast_group:
                        if (context.CastGroups.Local.Count == 1)
                        {
                            MessageBox.Show("You must have at least one Cast Group.", WindowTitle);
                            return;
                        }
                        if (!cast_group.Members.Any() || ConfirmDelete($"Are you sure you want to delete the '{cast_group.Name}' cast group?\nThis will remove the {cast_group.Members.Count.Plural($"currently selected {cast_group.Name}")} from the cast."))
                            list.Remove(cast_group);
                        break;
                    case AlternativeCast alternative_cast:
                        bool any_members = alternative_cast.Members.Any();
                        int cast_groups_alternating = context.CastGroups.Local.Count(cg => cg.AlternateCasts);
                        bool needed_to_alternate = context.AlternativeCasts.Local.Count <= 2 && cast_groups_alternating > 0;
                        var allow_delete = (any_members, needed_to_alternate) switch
                        {
                            (true, true) => ConfirmDelete($"Are you sure you want to delete the '{alternative_cast.Name}' cast?\nThis will remove the {alternative_cast.Members.Count.Plural($"currently selected {alternative_cast.Name}")} from the cast, and disable alternating casts on the {cast_groups_alternating.Plural("cast group")} for which it is enabled."),
                            (true, false) => ConfirmDelete($"Are you sure you want to delete the '{alternative_cast.Name}' cast?\nThis will remove the {alternative_cast.Members.Count.Plural($"currently selected {alternative_cast.Name}")} from the cast."),
                            (false, true) => ConfirmDelete($"Are you sure you want to delete the '{alternative_cast.Name}' cast?\nThis will disable alternating casts on the {cast_groups_alternating.Plural("cast group")} for which it is enabled."),
                            _ => true // (false, false)
                        };
                        if (allow_delete)
                        {
                            if (any_members)
                                foreach (var member in alternative_cast.Members)
                                    member.CastGroup = null;
                            if (needed_to_alternate)
                                foreach (var cast_group in context.CastGroups.Local)
                                {
                                    foreach (var other_cast_member in cast_group.Members)
                                        other_cast_member.AlternativeCast = null;
                                    cast_group.AlternateCasts = false;
                                }
                            list.Remove(alternative_cast);
                        }
                        break;
                    case Tag tag:
                        if (!tag.Members.Any() || ConfirmDelete($"Are you sure you want to delete the '{tag.Name}' tag?\nThis will remove the tag from {tag.Members.Count.Plural($"currently tagged applicant")}."))
                            list.Remove(tag);
                        break;
                    case SectionType section_type:
                        if (context.SectionTypes.Local.Count == 1)
                        {
                            MessageBox.Show("You must have at least one Section Type.", WindowTitle);
                            return;
                        }
                        if (!section_type.Sections.Any() || ConfirmDelete($"Are you sure you want to delete the '{section_type.Name}' section type?\nThis will remove the {section_type.Sections.Count.Plural(section_type.Name)} and any sections, items and roles within them."))
                            list.Remove(section_type);
                        break;
                    case Requirement requirement:
                        var used_count = requirement.UsedByCastGroups.Count + requirement.UsedByCombinedRequirements.Count + requirement.UsedByRoles.Count + requirement.UsedByTags.Count;//LATER is this missing NotRequirements?
                        if (used_count == 0 || ConfirmDelete($"Are you sure you want to delete the '{requirement.Name}' reqirement?\nThis requirement is currently in used {used_count.Plural("time")}."))
                            list.Remove(requirement);
                        break;
                }
            }
        }

        private bool ConfirmDelete(string msg)
            => MessageBox.Show(msg, WindowTitle, MessageBoxButton.YesNo) == MessageBoxResult.Yes;

        private void ImportShowConfiguration_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Importing show configuration is not currently supported.\nWould you like to reset to the default configuration instead?", WindowTitle, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                context.SetDefaultShowSettings(fileName, false);
        }

        #region Drag & drop in objectList

        private Point lastMouseDown;
        private IOrdered? draggedItem;
        private IOrdered? draggedTarget;

        private void objectList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                lastMouseDown = e.GetPosition(objectList);
        }

        private void objectList_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if ((e.GetPosition(objectList) - lastMouseDown).Length > 1)
                {
                    draggedItem = objectList.SelectedItem as IOrdered;
                    if (draggedItem != null)
                    {
                        var drop_effect = DragDrop.DoDragDrop(objectList, objectList.SelectedValue, DragDropEffects.Move);
                        if (drop_effect == DragDropEffects.Move && draggedTarget != null && draggedItem != draggedTarget)
                        {
                            MoveObject(draggedItem, draggedTarget);
                            draggedTarget = null;
                            draggedItem = null;
                        }
                    }
                }
            }
        }

        private void objectList_DragOver(object sender, DragEventArgs e)
        {
            if ((e.GetPosition(objectList) - lastMouseDown).Length > 1)
                ProcessDragDrop(e, out _);
            e.Handled = true;
        }

        private void objectList_Drop(object sender, DragEventArgs e)
        {
            ProcessDragDrop(e, out draggedTarget);
            e.Handled = true;
        }

        private void ProcessDragDrop(DragEventArgs e, out IOrdered? target_item)
        {
            target_item = (e.OriginalSource as TextBlock)?.DataContext as IOrdered;
            e.Effects = target_item != null && draggedItem != null && target_item != draggedItem ? DragDropEffects.Move : DragDropEffects.None;
        }
        #endregion

        private void MoveObject(IOrdered dragged, IOrdered target)
        {
            if (currentViewSource?.Source is IList list)
            {
                var typed_list = list.OfType<IOrdered>().ToList();
                typed_list.MoveInOrder(dragged, target);
            }
        }
    }
}
