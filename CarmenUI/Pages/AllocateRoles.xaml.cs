using CarmenUI.Converters;
using CarmenUI.ViewModels;
using CarmenUI.Windows;
using Carmen.CastingEngine;
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
using Carmen.CastingEngine.Audition;
using Carmen.CastingEngine.Allocation;

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

        public AllocateRoles(RecentShow connection) : base(connection)
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
                CastGroup[] cast_groups;
                using (loading.Segment(nameof(ShowContext.CastGroups) + nameof(CastGroup.Members), "Cast groups"))
                    cast_groups = await context.CastGroups.Include(cg => cg.Members).InOrder().ToArrayAsync();
                using (loading.Segment(nameof(ShowContext.AlternativeCasts), "Alternative casts"))
                    _alternativeCasts = await context.AlternativeCasts.InNameOrder().ToArrayAsync();
                _castGroupsByCast = CastGroupAndCast.Enumerate(cast_groups, _alternativeCasts).ToArray();
                using (loading.Segment(nameof(ShowContext.Applicants) + nameof(Applicant.Roles), "Applicants"))
                    _applicantsInCast = await context.Applicants.Include(a => a.Roles).ToArrayAsync();
                using (loading.Segment(nameof(ShowContext.Nodes), "Nodes"))
                    await context.Nodes.LoadAsync();
                using (loading.Segment(nameof(ShowContext.Requirements), "Requirements"))
                    _requirements = await context.Requirements.ToArrayAsync();
                using (loading.Segment(nameof(ShowContext.Roles) + nameof(Role.Cast), "Roles"))
                    await context.Roles.Include(r => r.Cast).LoadAsync();
                using (loading.Segment(nameof(ShowContext.Nodes) + nameof(Item) + nameof(Item.Roles), "Items"))
                    await context.Nodes.OfType<Item>().Include(i => i.Roles).LoadAsync();
                using (loading.Segment(nameof(ShowContext.Roles) + nameof(Role.Items), "Items"))
                    await context.Roles.Include(r => r.Items).LoadAsync();
                using (loading.Segment(nameof(CastGroup.FullTimeEquivalentMembers), "Cast members"))
                    _totalCast = (uint)cast_groups.Sum(cg => cg.Members.Count);
                using (loading.Segment(nameof(IAllocationEngine), "Allocation engine"))
                {
                    IAuditionEngine audition_engine = ParseAuditionEngine() switch
                    {
                        nameof(NeuralAuditionEngine) => new NeuralAuditionEngine(criterias, NeuralEngineConfirm),
                        nameof(WeightedSumEngine) => new WeightedSumEngine(criterias),
                        _ => throw new ArgumentException($"Audition engine not handled: {ParseAuditionEngine()}")
                    };
                    _engine = ParseAllocationEngine() switch
                    {
                        nameof(HeuristicAllocationEngine) => new HeuristicAllocationEngine(audition_engine, alternativeCasts, criterias),
                        nameof(WeightedAverageEngine) => new WeightedAverageEngine(audition_engine, alternativeCasts, context.ShowRoot),
                        nameof(SessionLearningAllocationEngine) => new SessionLearningAllocationEngine(audition_engine, alternativeCasts, context.ShowRoot, requirements, NeuralEngineConfirm),
                        nameof(RoleLearningAllocationEngine) => new RoleLearningAllocationEngine(audition_engine, alternativeCasts, context.ShowRoot, requirements, NeuralEngineConfirm),
                        nameof(ComplexNeuralAllocationEngine) => new ComplexNeuralAllocationEngine(audition_engine, alternativeCasts, context.ShowRoot, requirements, NeuralEngineConfirm, new FilePersistence("ComplexNeuralModel.xml")),
                        _ => throw new ArgumentException($"Allocation engine not handled: {ParseAllocationEngine()}")
                    };
                }
                _rootNodeView = new ShowRootNodeView(context.ShowRoot, totalCast, _alternativeCasts);
                showCompleted.IsChecked = true; // must be set after creating ShowRootNodeView because it triggers Checked event
                rolesTreeView.ItemsSource = rootNodeView.ChildrenInOrder;
                castingProgress.DataContext = rootNodeView;
            }
            await rootNodeView.UpdateAllAsync();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => ChangeToViewMode();

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
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
                    using (new LoadingOverlay(this).AsSegment(nameof(IAllocationEngine) + nameof(IAllocationEngine.UserPickedCast), "Learning...", "Roles allocated by the user"))
                        await Task.Run(() => engine.UserPickedCast(editable_view.Role.Cast, applicants_not_picked, editable_view.Role));
                    SaveChanges(false); // to save any weights updated by the engine
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

        private async void AutoCastButton_Click(object sender, RoutedEventArgs e)
        {
            if (applicantsPanel.Content is not EditableRoleWithApplicantsView current_view)
                return;
            IEnumerable<Applicant> new_applicants;
            using (new LoadingOverlay(this).AsSegment(nameof(IAllocationEngine) + nameof(IAllocationEngine.UserPickedCast), "Processing...", "Finding best applicants"))
                new_applicants = await Task.Run(() => engine.PickCast(applicantsInCast, current_view.Role));
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
                    {
                        StringFormat = "{0:0.#}"
                    },
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

        private async Task<bool> ChangeToEditMode()
        {
            if (!CancelChanges())
                return false;
            if (RevertChanges())
                Page_Loaded(this, new RoutedEventArgs());
            if (applicantsPanel.Content is IDisposable existing_view)
                existing_view.Dispose();
            using var loading = new LoadingOverlay(this) { MainText = "Processing...", SubText = "Calculating applicant suitabilities" };
            applicantsPanel.Content = rolesTreeView.SelectedItem switch
            {
                RoleNodeView role_node_view => await Task.Run(() => new EditableRoleWithApplicantsView(engine, role_node_view.Role, castGroupsByCast, primaryCriterias, applicantsInCast,
                    Properties.Settings.Default.ShowUnavailableApplicants, Properties.Settings.Default.ShowIneligibleApplicants)),
                _ => defaultPanelContent
            };
            if (applicantsPanel.VisualDescendants<CheckBox>().FirstOrDefault(chk => chk.Name == "showUnavailableApplicants") is CheckBox check_box)
                check_box.IsChecked = false;
            return true;
        }

        protected override void DisposeInternal()
        {
            if (applicantsPanel.Content is IDisposable existing_view)
                existing_view.Dispose();
            base.DisposeInternal();
        }

        bool selectingRoleProgrammatically = false;
        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectingRoleProgrammatically)
                throw new ApplicationException("Called next role while already selecting a role programatically.");
            selectingRoleProgrammatically = true;
            if (recommendedCastingOrder == null)
                recommendedCastingOrder = engine.IdealCastingOrder(context.ShowRoot, applicantsInCast).GetEnumerator();
            using (new LoadingOverlay(this) { MainText = "Processing...", SubText = "Finding next uncast role" })
                while (await Task.Run(() => recommendedCastingOrder.MoveNext()))
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

        private async void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            using (new LoadingOverlay(this).AsSegment(nameof(IAllocationEngine) + nameof(IAllocationEngine.ExportChanges), "Learning...", "Finalising user session"))
                await Task.Run(() => engine.ExportChanges());
            SaveChangesAndReturn(false); // to save any weights updated by the engine
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

        private async void EditCastButton_Click(object sender, RoutedEventArgs e)
            => await ChangeToEditMode();

        private async void ViewOnlyApplicantsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => await ChangeToEditMode();

        private void rolesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!ChangeToViewMode())
            { } // ideally we'd change the selection back, but this causes the event to get called again and infinitely loop asking if you want to cancel, until you do
            if (!selectingRoleProgrammatically)
            {
                // clear recommender state if user manually initiates editing a role
                recommendedCastingOrder = null;
            }
        }

        private async void rolesTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => await ChangeToEditMode();

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

        private async void BalanceCastButton_Click(object sender, RoutedEventArgs e)
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
            using (new LoadingOverlay(this).AsSegment(nameof(IAllocationEngine) + nameof(IAllocationEngine.BalanceCast), "Processing...", "Balancing cast between roles"))
                await Task.Run(() => engine.BalanceCast(applicantsInCast, selected_roles));
            SaveChanges(false); // immediately save any automatic casting
            foreach (var role in selected_roles)
                rootNodeView.FindRoleView(role)?.UpdateAsync();
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
