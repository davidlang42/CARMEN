using CarmenUI.Converters;
using CarmenUI.ViewModels;
using CarmenUI.Windows;
using Carmen.CastingEngine;
using Microsoft.EntityFrameworkCore;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using CarmenUI.Bindings;
using Carmen.CastingEngine.Heuristic;

namespace CarmenUI.Pages
{
    /// <summary>
    /// Interaction logic for AllocateRoles.xaml
    /// </summary>
    public partial class AllocateRoles : SubPage
    {
        private CastGroupAndCast[]? _castGroupsByCast;
        private AlternativeCast[]? _alternativeCasts;
        private Applicant[]? _applicantsInCast;
        private Criteria[]? _primaryCriterias;
        private Criteria[]? _criterias;
        private NodeView? _rootNodeView;
        private IAllocationEngine? _engine;

        private object defaultPanelContent;

        private CastGroupAndCast[] castGroupsByCast => _castGroupsByCast
            ?? throw new ApplicationException($"Tried to used {nameof(castGroupsByCast)} before it was loaded.");

        private AlternativeCast[] alternativeCasts => _alternativeCasts
            ?? throw new ApplicationException($"Tried to used {nameof(alternativeCasts)} before it was loaded.");

        private Applicant[] applicantsInCast => _applicantsInCast
            ?? throw new ApplicationException($"Tried to used {nameof(applicantsInCast)} before it was loaded.");

        private Criteria[] criterias => _criterias
            ?? throw new ApplicationException($"Tried to used {nameof(criterias)} before it was loaded.");

        private Criteria[] primaryCriterias => _primaryCriterias
            ?? throw new ApplicationException($"Tried to used {nameof(primaryCriterias)} before it was loaded.");

        private NodeView rootNodeView => _rootNodeView
            ?? throw new ApplicationException($"Tried to used {nameof(rootNodeView)} before it was loaded.");

        private IAllocationEngine engine => _engine
            ?? throw new ApplicationException($"Tried to used {nameof(engine)} before it was loaded.");

        public AllocateRoles(DbContextOptions<ShowContext> context_options) : base(context_options)
        {
            InitializeComponent();
            defaultPanelContent = applicantsPanel.Content;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            using (var loading = new LoadingOverlay(this).AsSegment(nameof(AllocateRoles)))
            {
                using (loading.Segment(nameof(ShowContext.Criterias), "Criteria"))
                    _criterias = await context.Criterias.InOrder().ToArrayAsync();
                _primaryCriterias = _criterias.Where(c => c.Primary).ToArray();
                using (loading.Segment(nameof(ShowContext.CastGroups), "Cast groups"))
                    await context.CastGroups.LoadAsync();
                using (loading.Segment(nameof(ShowContext.AlternativeCasts), "Alternative casts"))
                    _alternativeCasts = await context.AlternativeCasts.InNameOrder().ToArrayAsync();
                _castGroupsByCast = CastGroupAndCast.Enumerate(context.CastGroups.Local.InOrder(), _alternativeCasts).ToArray();
                using (loading.Segment(nameof(ShowContext.Applicants) + nameof(Applicant.Roles) + nameof(Role.Items), "Applicants"))
                    _applicantsInCast = await context.Applicants.Where(a => a.CastGroup != null).Include(a => a.Roles).ThenInclude(r => r.Items).ToArrayAsync();
                using (loading.Segment(nameof(ShowContext.Nodes), "Nodes"))
                    await context.Nodes.LoadAsync();
                using (loading.Segment(nameof(ShowContext.Requirements) + nameof(AbilityExactRequirement) + nameof(AbilityExactRequirement.Criteria), "Requirements"))
                    await context.Requirements.OfType<AbilityExactRequirement>().Include(cr => cr.Criteria).LoadAsync();
                using (loading.Segment(nameof(ShowContext.Requirements) + nameof(AbilityRangeRequirement) + nameof(AbilityRangeRequirement.Criteria), "Requirements"))
                    await context.Requirements.OfType<AbilityRangeRequirement>().Include(cr => cr.Criteria).LoadAsync();
                using (loading.Segment(nameof(ShowContext.Nodes) + nameof(Item) + nameof(Item.Roles) + nameof(Role.Cast), "Items"))
                    await context.Nodes.OfType<Item>().Include(i => i.Roles).ThenInclude(r => r.Cast).LoadAsync();
                using (loading.Segment(nameof(ShowContext.Nodes) + nameof(Item) + nameof(Item.Roles) + nameof(Role.Requirements), "Items"))
                    await context.Nodes.OfType<Item>().Include(i => i.Roles).ThenInclude(r => r.Requirements).LoadAsync();
                uint total_cast;
                using (loading.Segment(nameof(CastGroup.FullTimeEquivalentMembers), "Cast members"))
                    total_cast = (uint)context.CastGroups.Local.Sum(cg => cg.FullTimeEquivalentMembers(context.AlternativeCasts.Local.Count));
                using (loading.Segment(nameof(IAllocationEngine), "Allocation engine"))
                {
                    //TODO have a mechanism to choose which engine
                    //_engine = new DummyEngine();
                    _engine = new OriginalHeuristicEngine(criterias, null, ListSortDirection.Descending);
                }
                _rootNodeView = new ShowRootNodeView(context.ShowRoot, total_cast, context.AlternativeCasts.Local.ToArray());
                showCompleted.IsChecked = true; // must be set after creating ShowRootNodeView because it triggers Checked event
                rolesTreeView.ItemsSource = rootNodeView.ChildrenInOrder;
                castingProgress.DataContext = rootNodeView;
            }
            await rootNodeView.UpdateAllAsync();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => ChangeToViewMode();

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveChanges())
            {
                if (applicantsPanel.Content is EditableRoleWithApplicantsView editable_view)
                    rootNodeView.FindRoleView(editable_view.Role)?.UpdateAsync();
                ChangeToViewMode();
            }
        }

        private void AutoCastButton_Click(object sender, RoutedEventArgs e)
        {
            if (applicantsPanel.Content is not EditableRoleWithApplicantsView current_view)
                return;
            var new_applicants = engine.PickCast(applicantsInCast, current_view.Role, context.AlternativeCasts.Local);
            current_view.SelectApplicants(new_applicants);
        }

        private void ListView_Initialized(object sender, EventArgs e)
        {
            // Insert columns for primary Criteria (which can't be done in XAML
            // because they are dynamic and GridView is not a panel)
            var listview = (ListView)sender;
            var gridview = (GridView)listview.View;
            int column_index = 4;
            int array_index = 0;
            foreach (var criteria in primaryCriterias)
            {
                var mark_column = new GridViewColumn
                {
                    Header = criteria.Name,
                    DisplayMemberBinding = new Binding($"{nameof(ApplicantForRole.Marks)}[{array_index}]"),
                };
                BindingOperations.SetBinding(mark_column, GridViewColumn.WidthProperty, new WidthBinding($"{nameof(Properties.Widths.AllocateRolesGrid)}[{criteria.Name}_Mark]"));
                gridview.Columns.Insert(column_index++, mark_column);
                var count_column = new GridViewColumn
                {
                    Header = "Roles",
                    DisplayMemberBinding = new Binding($"{nameof(ApplicantForRole.ExistingRoles)}[{array_index}]")
                };
                BindingOperations.SetBinding(count_column, GridViewColumn.WidthProperty, new WidthBinding($"{nameof(Properties.Widths.AllocateRolesGrid)}[{criteria.Name}_Count]"));
                gridview.Columns.Insert(column_index++, count_column);
                array_index++;
            }
        }

        private bool ChangeToViewMode()
        {
            if (!CancelChanges())
                return false;
            if (RevertChanges())
                Page_Loaded(this, new RoutedEventArgs());
            if (applicantsPanel.Content is IDisposable existing_view)
                existing_view.Dispose();
            applicantsPanel.Content = rolesTreeView.SelectedItem switch
            {
                RoleNodeView role_node_view => new RoleWithApplicantsView(role_node_view.Role, castGroupsByCast),
                ItemNodeView item_node_view => new NodeRolesOverview(item_node_view.Item, alternativeCasts),
                SectionNodeView section_node_view => new NodeRolesOverview(section_node_view.Section, alternativeCasts),
                _ => defaultPanelContent
            };
            return true;
        }

        private bool ChangeToEditMode()
        {
            //LATER loadingoverlay while this is created (if needed) -- due to computational time rather than db time
            if (!CancelChanges())
                return false;
            if (RevertChanges())
                Page_Loaded(this, new RoutedEventArgs());
            if (applicantsPanel.Content is IDisposable existing_view)
                existing_view.Dispose();
            applicantsPanel.Content = rolesTreeView.SelectedItem switch
            {
                RoleNodeView role_node_view => new EditableRoleWithApplicantsView(engine, role_node_view.Role, castGroupsByCast, primaryCriterias, applicantsInCast,
                    Properties.Settings.Default.ShowUnavailableApplicants, Properties.Settings.Default.ShowIneligibleApplicants),
                _ => defaultPanelContent
            };
            if (applicantsPanel.VisualDescendants<CheckBox>().FirstOrDefault(chk => chk.Name == "showUnavailableApplicants") is CheckBox check_box)
                check_box.IsChecked = false; //LATER it would be much better if this was a property of the view itself, but for some reason I couldn't get the binding to work properly
            return true;
        }

        protected override void DisposeInternal()
        {
            if (applicantsPanel.Content is IDisposable existing_view)
                existing_view.Dispose();
            base.DisposeInternal();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var current_role = (rolesTreeView.SelectedItem as RoleNodeView)?.Role;
            var next_role = engine.NextUncastRole(context.ShowRoot.ItemsInOrder(), context.AlternativeCasts.Local.ToArray(), current_role);
            if (next_role != null)
                rootNodeView.SelectRole(next_role);
            else
                rootNodeView.ClearSelection();
        }

        private void MainMenuButton_Click(object sender, RoutedEventArgs e)
            => CancelChangesAndReturn();

        private void showApplicants_Changed(object sender, RoutedEventArgs e)
        {
            if (applicantsPanel.Content is EditableRoleWithApplicantsView current_view)
                current_view.ConfigureFiltering(Properties.Settings.Default.ShowUnavailableApplicants, Properties.Settings.Default.ShowIneligibleApplicants);
        }

        private void ClearCastButton_Click(object sender, RoutedEventArgs e)
        {
            if (applicantsPanel.Content is EditableRoleWithApplicantsView current_view)
                current_view.ClearSelectedApplicants();
        }

        private void EditCastButton_Click(object sender, RoutedEventArgs e)
            => ChangeToEditMode();

        private void ViewOnlyApplicantsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => ChangeToEditMode();

        private void rolesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!ChangeToViewMode())
            { } //LATER ideally change the selection back, but this causes the event to get called again and infinitely loop asking if you want to cancel, until you do -- a better way to handle this might be to disable the items tree when in edit mode
        }

        private void rolesTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => ChangeToEditMode();

        private void ExpandAll_Click(object sender, RoutedEventArgs e)
            => rootNodeView.ExpandAll();

        private void CollapseAll_Click(object sender, RoutedEventArgs e)
            => rootNodeView.CollapseAll();

        private void showCompleted_Checked(object sender, RoutedEventArgs e)
            => rootNodeView.SetShowCompleted(true);

        private void showCompleted_Unchecked(object sender, RoutedEventArgs e)
            => rootNodeView.SetShowCompleted(false);

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var list_view_item = (ListViewItem)sender;
            if (list_view_item.DataContext is ApplicantForRole afr)
                afr.IsSelected = !afr.IsSelected;
        }

        private void ListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.T) // Toggle
            {
                e.Handled = true;
                PickSelectedApplicants((ListView)sender, null);
            }
            else if (e.Key == Key.S) // Select
            {
                e.Handled = true;
                PickSelectedApplicants((ListView)sender, true);
            }
            else if (e.Key == Key.U) // Unselect
            {
                e.Handled = true;
                PickSelectedApplicants((ListView)sender, false);
            }
        }

        private void ListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) // Space can only be handed in PreviewKeyDown, not KeyDown
            {
                e.Handled = true;
                PickSelectedApplicants((ListView)sender, null);
            }
        }

        private void PickSelectedApplicants(ListView list_view, bool? value_or_toggle)
        {
            var selected_items = list_view.SelectedItems.OfType<ApplicantForRole>().ToArray();
            var value = value_or_toggle ?? !(selected_items.Where(afr => afr.IsSelected).Count() > selected_items.Length / 2);
            foreach (var afr in selected_items)
                afr.IsSelected = value;
        }

        private void BalanceCastButton_Click(object sender, RoutedEventArgs e)
        {
            if (applicantsPanel.Content is not NodeRolesOverview current_view)
                return;
            if (current_view.IncompleteRoles.Count == 0)
            {
                MessageBox.Show("There are no incomplete roles to cast.");
                return;
            }
            if (ParseSelectedRoles(current_view.IncompleteRoles) is not List<Role> selected_roles)
                return;
            if (selected_roles.Count == 0)
            {
                if (MessageBox.Show("No roles are selected. Would you like to select them all?", WindowTitle, MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
                foreach (var ir in current_view.IncompleteRoles)
                    ir.IsSelected = true;
                if (ParseSelectedRoles(current_view.IncompleteRoles) is not List<Role> all_selected_roles)
                    return;
                if (all_selected_roles.Count == 0)
                {
                    MessageBox.Show("No roles are selected.", WindowTitle);
                    return;
                }
                selected_roles = all_selected_roles;
            }
            if (selected_roles.Count == 1 && MessageBox.Show("Only 1 role is selected, so no balancing will occur."
                + "\nDo you still want to automatically cast this role?", WindowTitle, MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;
            var new_role_applicants = engine.BalanceCast(applicantsInCast, selected_roles, context.AlternativeCasts.Local);
            foreach (var pair in new_role_applicants)
                foreach (var applicant in pair.Value)
                    pair.Key.Cast.Add(applicant);
            applicantsPanel.Content = new NodeRolesOverview(current_view.Node, alternativeCasts);
            //TODO balance- should we allow IdealCastingOrder() to return sets of roles to be cast together? how will the UI handle this?
        }

        private List<Role>? ParseSelectedRoles(IEnumerable<IncompleteRole> incomplete_roles)
        {
            var selected_roles = new List<Role>();
            foreach (var incomplete_role in incomplete_roles)
            {
                if (incomplete_role.IsSelected && incomplete_role.Role.CastingStatus(alternativeCasts) == Role.RoleStatus.OverCast)
                {
                    var msg = $"Selected role '{incomplete_role.Role.Name}' is currently over cast."
                        + "\nWould you like to clear the currently selected cast before balancing cast?"
                        + "\n(Choosing No will remove this role from the selection)";
                    var result = MessageBox.Show(msg, WindowTitle, MessageBoxButton.YesNoCancel);
                    if (result == MessageBoxResult.Yes)
                        incomplete_role.Role.Cast.Clear();
                    else if (result == MessageBoxResult.No)
                        incomplete_role.IsSelected = false;
                    else
                        return null;
                }
                if (incomplete_role.IsSelected)
                    selected_roles.Add(incomplete_role.Role);
            }
            return selected_roles;
        }
    }
}
