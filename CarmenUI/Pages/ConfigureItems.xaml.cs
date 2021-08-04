using CarmenUI.Converters;
using CarmenUI.ViewModels;
using CarmenUI.Windows;
using Microsoft.EntityFrameworkCore;
using ShowModel;
using ShowModel.Applicants;
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
    /// Interaction logic for ConfigureItems.xaml
    /// </summary>
    public partial class ConfigureItems : SubPage
    {
        private readonly CollectionViewSource rootNodesViewSource;
        private readonly CollectionViewSource castGroupsViewSource;
        private readonly CollectionViewSource requirementsViewSource;

        public ConfigureItems(DbContextOptions<ShowContext> context_options) : base(context_options)
        {
            InitializeComponent();
            rootNodesViewSource = (CollectionViewSource)FindResource(nameof(rootNodesViewSource));
            castGroupsViewSource = (CollectionViewSource)FindResource(nameof(castGroupsViewSource));
            requirementsViewSource = (CollectionViewSource)FindResource(nameof(requirementsViewSource));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            using var loading = new LoadingOverlay(this);
            loading.Progress = 0;
            await context.CastGroups.LoadAsync();
            castGroupsViewSource.Source = context.CastGroups.Local.ToObservableCollection();
            loading.Progress = 25;
            await context.Requirements.LoadAsync();
            requirementsViewSource.Source = context.Requirements.Local.ToObservableCollection();
            loading.Progress = 50;
            await context.Nodes.LoadAsync();
            loading.Progress = 75;
            await context.Nodes.OfType<Item>().Include(i => i.Roles).ThenInclude(r => r.Requirements).LoadAsync();
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
                OnReturn(DataObjects.Nodes);
        }

        private void itemsTreeView_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void itemsTreeView_MouseMove(object sender, MouseEventArgs e)
        {

        }

        private void itemsTreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void itemsTreeView_DragOver(object sender, DragEventArgs e)
        {

        }

        private void itemsTreeView_Drop(object sender, DragEventArgs e)
        {

        }

        private void RolesDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            // Insert columns for CountByGroups (which can't be done in XAML
            // because they are dynamic and DataGridColumns is not a panel)
            var columns = ((DataGrid)sender).Columns;
            int index = 1;
            foreach (var cast_group in context.CastGroups.Local)
            {
                // build view template
                var viewFactory = new FrameworkElementFactory(typeof(TextBlock));
                viewFactory.SetBinding(TextBlock.TextProperty, new Binding(nameof(NullableCountByGroup.Count)));
                // build edit template
                var editFactory = new FrameworkElementFactory(typeof(TextBox));
                editFactory.SetBinding(TextBox.TextProperty, new Binding(nameof(NullableCountByGroup.Count)));
                // add the column
                columns.Insert(index++, new DataGridTemplateColumn
                {
                    Header = cast_group.Abbreviation,
                    CellTemplate = WrapWithNullableCountByGroupContext(cast_group, viewFactory),
                    CellEditingTemplate = WrapWithNullableCountByGroupContext(cast_group, editFactory)
                });
            }
        }

        private DataTemplate WrapWithNullableCountByGroupContext(CastGroup cast_group, FrameworkElementFactory child_element)
        {
            var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
            stackPanelFactory.SetValue(StackPanel.DataContextProperty, new Binding(nameof(Role.CountByGroups))
            {
                Converter = new CountByGroupSelector(),
                ConverterParameter = cast_group
            });
            stackPanelFactory.AppendChild(child_element);
            return new DataTemplate { VisualTree = stackPanelFactory };
        }

    }
}
