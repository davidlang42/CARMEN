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
using Carmen.CastingEngine.Dummy;
using Carmen.CastingEngine.Neural;

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
        private Requirement[]? _requirements;
        private NodeView? _rootNodeView;
        private uint? _totalCast;
        private IAllocationEngine? _engine;

        private object defaultPanelContent;
        private IEnumerator<Role[]>? recommendedCastingOrder;

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

        private Requirement[] requirements => _requirements
            ?? throw new ApplicationException($"Tried to used {nameof(requirements)} before it was loaded.");

        private NodeView rootNodeView => _rootNodeView
            ?? throw new ApplicationException($"Tried to used {nameof(rootNodeView)} before it was loaded.");

        private uint totalCast => _totalCast
            ?? throw new ApplicationException($"Tried to used {nameof(totalCast)} before it was loaded.");

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
                using (loading.Segment(nameof(ShowContext.Requirements), "Requirements"))
                    _requirements = await context.Requirements.ToArrayAsync();
                using (loading.Segment(nameof(ShowContext.Requirements) + nameof(AbilityExactRequirement) + nameof(AbilityExactRequirement.Criteria), "Requirements"))
                    await context.Requirements.OfType<AbilityExactRequirement>().Include(cr => cr.Criteria).LoadAsync();
                using (loading.Segment(nameof(ShowContext.Requirements) + nameof(AbilityRangeRequirement) + nameof(AbilityRangeRequirement.Criteria), "Requirements"))
                    await context.Requirements.OfType<AbilityRangeRequirement>().Include(cr => cr.Criteria).LoadAsync();
                using (loading.Segment(nameof(ShowContext.Nodes) + nameof(Item) + nameof(Item.Roles) + nameof(Role.Cast), "Items"))
                    await context.Nodes.OfType<Item>().Include(i => i.Roles).ThenInclude(r => r.Cast).LoadAsync();
                using (loading.Segment(nameof(ShowContext.Nodes) + nameof(Item) + nameof(Item.Roles) + nameof(Role.Requirements), "Items"))
                    await context.Nodes.OfType<Item>().Include(i => i.Roles).ThenInclude(r => r.Requirements).LoadAsync();
                using (loading.Segment(nameof(CastGroup.FullTimeEquivalentMembers), "Cast members"))
                    _totalCast = (uint)context.CastGroups.Local.Sum(cg => cg.FullTimeEquivalentMembers(context.AlternativeCasts.Local.Count));
                using (loading.Segment(nameof(IAllocationEngine), "Allocation engine"))
                {
                    IApplicantEngine applicant_engine = ParseApplicantEngine() switch
                    {
                        nameof(DummyApplicantEngine) => new DummyApplicantEngine(),
                        nameof(NeuralApplicantEngine) => new NeuralApplicantEngine(criterias, NeuralEngineConfirm),
                        nameof(WeightedSumEngine) => new WeightedSumEngine(criterias),
                        _ => throw new ArgumentException($"Applicant engine not handled: {ParseApplicantEngine()}")
                    };
                    _engine = ParseAllocationEngine() switch
                    {
                        nameof(DummyAllocationEngine) => new DummyAllocationEngine(applicant_engine, alternativeCasts),
                        nameof(HeuristicAllocationEngine) => new HeuristicAllocationEngine(applicant_engine, alternativeCasts, criterias),
                        nameof(WeightedAverageEngine) => new WeightedAverageEngine(applicant_engine, alternativeCasts, context.ShowRoot),
                        nameof(SessionLearningAllocationEngine) => new SessionLearningAllocationEngine(applicant_engine, alternativeCasts, context.ShowRoot, requirements, NeuralEngineConfirm),
                        nameof(RoleLearningAllocationEngine) => new RoleLearningAllocationEngine(applicant_engine, alternativeCasts, context.ShowRoot, requirements, NeuralEngineConfirm),
                        _ => throw new ArgumentException($"Allocation engine not handled: {ParseAllocationEngine()}")
                    };
                }
                _rootNodeView = new ShowRootNodeView(context.ShowRoot, totalCast, context.AlternativeCasts.Local.ToArray());
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
                {
                    rootNodeView.FindRoleView(editable_view.Role)?.UpdateAsync();
                    var applicants_not_picked = editable_view.Applicants
                        .Where(afr => afr.Eligibility.IsEligible && afr.Availability.IsAvailable)
                        .Where(afr => !afr.IsSelected)
                        .Select(afr => afr.Applicant);
                    if (engine.UserPickedCast(editable_view.Role.Cast, applicants_not_picked, editable_view.Role))
                        SaveChanges(false);
                }
                ChangeToViewMode();
            }
        }

        protected override void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.S && Properties.Settings.Default.SaveOnCtrlS)
                {
                    if (applicantsPanel.Content is EditableRoleWithApplicantsView)
                        SaveButton_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.Key == Key.Enter && Properties.Settings.Default.SaveAndExitOnCtrlEnter)
                {
                    SaveButton_Click(sender, e);
                    MainMenuButton_Click(sender, e);
                    e.Handled = true;
                }
            }
            if (!e.Handled)
                base.Window_KeyDown(sender, e);
        }

        private void AutoCastButton_Click(object sender, RoutedEventArgs e)
        {
            if (applicantsPanel.Content is not EditableRoleWithApplicantsView current_view)
                return;
            var new_applicants = engine.PickCast(applicantsInCast, current_view.Role);
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
                ItemNodeView item_node_view => new NodeRolesOverview(item_node_view.Item, alternativeCasts, totalCast),
                SectionNodeView section_node_view => new NodeRolesOverview(section_node_view.Section, alternativeCasts, totalCast),
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

        bool selectingRoleProgrammatically = false;
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectingRoleProgrammatically)
                throw new ApplicationException("Called next role while already selecting a role programatically.");
            selectingRoleProgrammatically = true;
            if (recommendedCastingOrder == null)
                recommendedCastingOrder = engine.IdealCastingOrder(context.ShowRoot, applicantsInCast).GetEnumerator();
            //LATER add a loading overlay (if all roles are cast, this can take up to 10 seconds to process through the entire list)
            while (recommendedCastingOrder.MoveNext())
                if (recommendedCastingOrder.Current.Any(r => r.CastingStatus(alternativeCasts) != Role.RoleStatus.FullyCast))
                {
                    rootNodeView.ClearSelection();
                    rootNodeView.CollapseAll();
                    if (recommendedCastingOrder.Current.Length == 1)
                    {
                        if (!rootNodeView.SelectRole(recommendedCastingOrder.Current[0])) // select a single role
                            break; // if role fails to select, exit process
                    }
                    else
                    {
                        var node = Role.CommonItemsAndSections(recommendedCastingOrder.Current) // find nodes which contain ALL of these roles
                            .OrderBy(n => n.ItemsInOrder().Count()) // find the node containing the minimum number of items (possibly only 1)
                            .ThenBy(n => n.ItemsInOrder().SelectMany(i => i.Roles).Distinct().Count()) // if tied, find the node with the minimum descendent roles
                            .FirstOrDefault(); // ShowRoot was excluded by Role.ItemsAndSections(), which is fine because its not selectable
                        if (node == null)
                            break; // not selectable
                        if (!rootNodeView.SelectNode(node)) // select the node containing these roles
                            break; // if node fails to select, exit process
                        if (applicantsPanel.Content is not NodeRolesOverview overview)
                            break; // if node overview not shown, exit process
                        overview.SelectRoles(recommendedCastingOrder.Current); // pre-select the recommended roles in the incomplete roles list
                    }
                    selectingRoleProgrammatically = false;
                    return; // success
                }
            rootNodeView.ClearSelection(); // clear when no more roles, or another reason to stop the process
            recommendedCastingOrder = null;
            selectingRoleProgrammatically = false;
        }

        private void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (engine.ExportChanges())
                SaveChangesAndReturn();
            else
                CancelChangesAndReturn();
        } 

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
            if (!selectingRoleProgrammatically)
            {
                // clear recommender state if user manually initiates editing a role
                recommendedCastingOrder = null;
            }
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
            if (Keyboard.Modifiers == ModifierKeys.None)
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
            engine.BalanceCast(applicantsInCast, selected_roles);
            applicantsPanel.Content = new NodeRolesOverview(current_view.Node, alternativeCasts, totalCast);
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
