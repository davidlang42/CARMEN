using CarmenUI.Converters;
using CarmenUI.ViewModels;
using CarmenUI.Windows;
using Microsoft.EntityFrameworkCore;
using ShowModel;
using ShowModel.Applicants;
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
        private readonly CollectionViewSource rootNodesViewSource;
        private CastGroupAndCast[]? _castGroupsByCast;
        private Applicant[]? _applicantsInCast;

        private CastGroupAndCast[] castGroupsByCast => _castGroupsByCast
            ?? throw new ApplicationException($"Tried to used {nameof(castGroupsByCast)} before it was loaded.");

        private Applicant[] applicantsInCast => _applicantsInCast
            ?? throw new ApplicationException($"Tried to used {nameof(applicantsInCast)} before it was loaded.");

        public AllocateRoles(DbContextOptions<ShowContext> context_options) : base(context_options)
        {
            InitializeComponent();
            rootNodesViewSource = (CollectionViewSource)FindResource(nameof(rootNodesViewSource));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            using var loading = new LoadingOverlay(this);
            loading.Progress = 0;
            await context.Criterias.LoadAsync();//LATER have a user setting which enables/disables preloading on page open for all pages, because if the connection is fast (or local) it might actually be a nicer UX to not do this all up front.
            loading.Progress = 10;
            await context.CastGroups.LoadAsync();
            loading.Progress = 20;
            await context.AlternativeCasts.LoadAsync();
            _castGroupsByCast = CastGroupAndCast.Enumerate(context.CastGroups.Local, context.AlternativeCasts.Local).ToArray();
            loading.Progress = 30;
            _applicantsInCast = await context.Applicants.Where(a => a.CastGroup != null).ToArrayAsync();
            loading.Progress = 60;
            await context.Nodes.LoadAsync();
            loading.Progress = 80;
            await context.Nodes.OfType<Item>().Include(i => i.Roles).ThenInclude(r => r.Cast).LoadAsync();
            rootNodesViewSource.Source = context.Nodes.Local.ToObservableCollection();
            rootNodesViewSource.View.Filter = n => ((Node)n).Parent == null;
            rootNodesViewSource.View.SortDescriptions.Add(StandardSort.For<Node>()); // sorts top level only, other levels sorted by SortIOrdered converter
            loading.Progress = 100;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (CancelChanges())
                OnReturn(null);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveChanges())
                OnReturn(DataObjects.Applicants | DataObjects.Nodes);
        }

        private void itemsTreeView_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void itemsTreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void itemsTreeView_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void itemsTreeView_DragOver(object sender, DragEventArgs e)
        {

        }

        private void itemsTreeView_Drop(object sender, DragEventArgs e)
        {

        }

        private void rolesTreeView_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void rolesTreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void rolesTreeView_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void rolesTreeView_DragOver(object sender, DragEventArgs e)
        {

        }

        private void rolesTreeView_Drop(object sender, DragEventArgs e)
        {

        }

        private void AutoCastButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ListView_Initialized(object sender, EventArgs e)
        {
            //TODO
            //<GridViewColumn Header="Singing"/>
            //<GridViewColumn Header="Roles"/>
            //<GridViewColumn Header="Acting"/>
            //<GridViewColumn Header="Roles"/>
            //<GridViewColumn Header="Dancing"/>
            //<GridViewColumn Header="Roles"/>
            
            // Insert columns for primary Criteria (which can't be done in XAML
            // because they are dynamic and GridView is not a panel)
            var listview = (ListView)sender;
            int column_index = 4;
            int array_index = 0;
            //foreach (var cast_group in context.CastGroups.Local)
            //{
            //    datagrid.Columns.Insert(column_index++, new DataGridTextColumn
            //    {
            //        Header = cast_group.Abbreviation,
            //        Binding = new Binding($"{nameof(RoleOnlyView.CountByGroups)}[{array_index++}].{nameof(CountByGroup.Count)}")
            //        {
            //            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            //        }
            //    });
            //}
        }

        private void rolesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //TODO this probably needs to be constructed higher up, in a NodeView VM, so that each node can know its progress/icon etc
            //TODO if any changes have been made, prompt for lose changes like cancel click
            applicantsPanel.DataContext = rolesTreeView.SelectedItem switch
            {
                Role role => new RoleWithApplicantsView(role,//TODO this needs disposing
                    castGroupsByCast,
                    context.Criterias.Local.ToArray(),
                    applicantsInCast),//TODO loadingoverlay while this is created (if needed)
                _ => null//TODO make this not show the UI when null, probably needs to be a content control
            };
        }
    }
}
