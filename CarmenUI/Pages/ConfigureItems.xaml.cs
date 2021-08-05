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
using Section = ShowModel.Structure.Section;

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

        private void RolesDataGrid_Initialized(object sender, EventArgs e)
        {
            // Insert columns for CountByGroups (which can't be done in XAML
            // because they are dynamic and DataGridColumns is not a panel)
            var datagrid = (DataGrid)sender;
            int column_index = 1;
            int array_index = 0;
            foreach (var cast_group in context.CastGroups.Local)
            {
                datagrid.Columns.Insert(column_index++, new DataGridTextColumn
                {
                    Header = cast_group.Abbreviation,
                    Binding = new Binding($"{nameof(RoleView.CountByGroups)}[{array_index++}].{nameof(CountByGroup.Count)}")
                });
            }
            // Populate all footer cells (because doing any in XAML would require
            // knowing how many CountByGroup columns there are in the DataGrid)
            //TODO HorizontalAlignment="Center"
            //TODO TextBox for bottom text except total

            var footer = ((Grid)datagrid.Parent).Children.OfType<Grid>().Single();
            column_index = 2;
            for (var i = 0; i < context.CastGroups.Local.Count; i++)
            {
                // define column
                var column_definition = new ColumnDefinition();
                column_definition.SetBinding(ColumnDefinition.WidthProperty, new Binding
                {
                    ElementName = datagrid.Name,
                    Path = new PropertyPath($"{nameof(DataGrid.Columns)}[{column_index-1}].{nameof(DataGridColumn.ActualWidth)}")
                });
                footer.ColumnDefinitions.Add(column_definition);
                // add top text
                var top_text = new TextBlock();
                top_text.SetBinding(TextBlock.TextProperty, new Binding($"{nameof(ItemView.SumOfRolesCount)}[{i}]"));
                Grid.SetColumn(top_text, column_index);
                footer.Children.Add(top_text);
                // add bottom text
                var bottom_text = new TextBlock();
                bottom_text.SetBinding(TextBlock.TextProperty, new Binding($"{nameof(ItemView.CountByGroups)}[{i}].{nameof(NullableCountByGroup.Count)}"));
                Grid.SetColumn(bottom_text, column_index);
                Grid.SetRow(bottom_text, 1);
                footer.Children.Add(bottom_text);
                // increment column
                column_index++;
            }
            // define totals column
            var total_column = new ColumnDefinition();
            total_column.SetBinding(ColumnDefinition.WidthProperty, new Binding
            {
                ElementName = datagrid.Name,
                Path = new PropertyPath($"{nameof(DataGrid.Columns)}[{column_index - 1}].{nameof(DataGridColumn.ActualWidth)}")
            });
            footer.ColumnDefinitions.Add(total_column);
            // add top total
            var top_total = new TextBlock();
            top_total.SetBinding(TextBlock.TextProperty, new Binding(nameof(ItemView.SumOfRolesTotal)));
            Grid.SetColumn(top_total, column_index);
            footer.Children.Add(top_total);
            // add bottom total
            var bottom_total = new TextBlock();
            bottom_total.SetBinding(TextBlock.TextProperty, new Binding(nameof(ItemView.TotalCount)));
            Grid.SetColumn(bottom_total, column_index);
            Grid.SetRow(bottom_total, 1);
            footer.Children.Add(bottom_total);
        }

        private void itemsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            rolesPanel.Content = itemsTreeView.SelectedItem switch
            {
                Item item => new ItemView(item, context.CastGroups.Local.ToArray()),
                Section section => section,
                _ => null
            };
        }
    }
}
