using CarmenUI.Converters;
using CarmenUI.ViewModels;
using CarmenUI.Windows;
using CastingEngine;
using Microsoft.EntityFrameworkCore;
using ShowModel;
using ShowModel.Applicants;
using ShowModel.Criterias;
using ShowModel.Requirements;
using ShowModel.Structure;
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

namespace CarmenUI.Pages
{
    /// <summary>
    /// Interaction logic for AllocateRoles.xaml
    /// </summary>
    public partial class AllocateRoles : SubPage
    {
        //LATER make sure that ViewOnlyApplicantsList scrolls if too long
        //LATER handle double click on ViewOnlyApplicantsList even when no applicants
        private CastGroupAndCast[]? _castGroupsByCast;
        private Applicant[]? _applicantsInCast;
        private Criteria[]? _primaryCriterias;
        private NodeView? _rootNodeView;
        private ICastingEngine engine;

        private CastGroupAndCast[] castGroupsByCast => _castGroupsByCast
            ?? throw new ApplicationException($"Tried to used {nameof(castGroupsByCast)} before it was loaded.");

        private Applicant[] applicantsInCast => _applicantsInCast
            ?? throw new ApplicationException($"Tried to used {nameof(applicantsInCast)} before it was loaded.");

        private Criteria[] primaryCriterias => _primaryCriterias
            ?? throw new ApplicationException($"Tried to used {nameof(primaryCriterias)} before it was loaded.");
        
        private NodeView rootNodeView => _rootNodeView
            ?? throw new ApplicationException($"Tried to used {nameof(rootNodeView)} before it was loaded.");

        public AllocateRoles(DbContextOptions<ShowContext> context_options) : base(context_options)
        {
            InitializeComponent();
            engine = new DummyEngine(); //LATER use real engine, maybe have it supplied by constructor
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //LATER have a user setting which enables/disables preloading on page open for all pages, because if the connection is fast (or local) it might actually be a nicer UX to not do this all up front.
            using (var loading = new LoadingOverlay(this))
            {
                loading.Progress = 0;
                _primaryCriterias = await context.Criterias.Where(c => c.Primary).ToArrayAsync();
                loading.Progress = 10;
                await context.CastGroups.LoadAsync();
                loading.Progress = 20;
                await context.AlternativeCasts.LoadAsync();
                _castGroupsByCast = CastGroupAndCast.Enumerate(context.CastGroups.Local, context.AlternativeCasts.Local).ToArray();
                loading.Progress = 30;
                _applicantsInCast = await context.Applicants.Where(a => a.CastGroup != null).Include(a => a.Roles).ToArrayAsync();
                loading.Progress = 60;
                await context.Nodes.LoadAsync();
                loading.Progress = 80;
                await context.Requirements.OfType<AbilityExactRequirement>().Include(cr => cr.Criteria).LoadAsync();
                loading.Progress = 85;
                await context.Requirements.OfType<AbilityRangeRequirement>().Include(cr => cr.Criteria).LoadAsync();
                loading.Progress = 90;
                await context.Nodes.OfType<Item>().Include(i => i.Roles).ThenInclude(r => r.Cast).LoadAsync();
                loading.Progress = 95;
                await context.Nodes.OfType<Item>().Include(i => i.Roles).ThenInclude(r => r.Requirements).LoadAsync();
                _rootNodeView = new ShowRootNodeView(context.ShowRoot, applicantsInCast.Length, context.AlternativeCasts.ToArray());
                rolesTreeView.ItemsSource = rootNodeView.Yield();
                castingProgress.DataContext = rootNodeView;
                loading.Progress = 100;
            }
            await rootNodeView.UpdateAsync();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => ChangeToViewMode();

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveChanges())
            {
                if (applicantsPanel.Content is EditableRoleWithApplicantsView editable_view)
                    rootNodeView.FindRoleView(editable_view.Role)?.UpdateAsync(); //LATER this should propogate upwards, but it will need a rewrite because it currently propogates downwards
                ChangeToViewMode();
            }
        }

        private void AutoCastButton_Click(object sender, RoutedEventArgs e)
        {
            if (applicantsPanel.Content is not EditableRoleWithApplicantsView current_view)
                return;
            current_view.ClearSelectedApplicants();
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
                gridview.Columns.Insert(column_index++, new GridViewColumn
                {
                    Header = criteria.Name,
                    DisplayMemberBinding = new Binding($"{nameof(ApplicantForRole.Marks)}[{array_index}]")
                });
                gridview.Columns.Insert(column_index++, new GridViewColumn
                {
                    Header = "Roles",
                    DisplayMemberBinding = new Binding($"{nameof(ApplicantForRole.ExistingRoles)}[{array_index}]")
                });
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
                _ => null
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
                RoleNodeView role_node_view => new EditableRoleWithApplicantsView(engine, role_node_view.Role, castGroupsByCast, primaryCriterias, applicantsInCast),
                _ => null
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
            var next_role = engine.NextRoleToCast(context.ShowRoot.ItemsInOrder(), current_role);
            if (next_role == null)
                rolesTreeView.ClearSelectedItem();
            else if (rootNodeView.FindRoleView(next_role) is RoleNodeView next_role_view)
                rolesTreeView.SetSelectedItem(next_role_view);
        }

        private void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (CancelChanges())
                OnReturn(DataObjects.Applicants | DataObjects.Nodes);//LATER only trigger changes if something was actually changed
        }

        private void showUnavailableApplicants_Checked(object sender, RoutedEventArgs e)
        {
            if (applicantsPanel.Content is EditableRoleWithApplicantsView current_view)
                current_view.ConfigureFiltering(true);
        }

        private void showUnavailableApplicants_Unchecked(object sender, RoutedEventArgs e)
        {
            if (applicantsPanel.Content is EditableRoleWithApplicantsView current_view)
                current_view.ConfigureFiltering(false);
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
                rolesTreeView.SetSelectedItem(e.OldValue);
        }

        private void rolesTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => ChangeToEditMode();
    }
}
