using CarmenUI.Converters;
using CarmenUI.ViewModels;
using CarmenUI.Windows;
using Microsoft.EntityFrameworkCore;
using ShowModel;
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

        public AllocateRoles(DbContextOptions<ShowContext> context_options) : base(context_options)
        {
            InitializeComponent();
            rootNodesViewSource = (CollectionViewSource)FindResource(nameof(rootNodesViewSource));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PopulateViews();
        }

        private async void PopulateViews()
        {
            using var loading = new LoadingOverlay(this);
            await context.Criterias.LoadAsync();
            await context.CastGroups.LoadAsync();
            await context.Nodes.LoadAsync();
            await context.Nodes.OfType<Item>().Include(i => i.Roles).ThenInclude(r => r.Cast).LoadAsync();
            rootNodesViewSource.Source = context.Nodes.Local.ToObservableCollection();
            rootNodesViewSource.View.Filter = n => ((Node)n).Parent == null;
            rootNodesViewSource.View.SortDescriptions.Add(StandardSort.For<Node>()); // sorts top level only, other levels sorted by SortIOrdered converter
            await context.Applicants.LoadAsync();
            loading.Progress = 100;//TODO update percentages
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
            int column_index = 4;
        }

        private void rolesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //TODO if any changes have been made, prompt for lose changes like cancel click
            applicantsPanel.DataContext = rolesTreeView.SelectedItem switch
            {
                Role role => new RoleWithApplicantsView(role, context.CastGroups.Local.ToArray(), context.Criterias.Local.ToArray(), context.Applicants.Local.ToObservableCollection()),
                _ => null
            };
        }
    }
}
