using CarmenUI.Converters;
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
    /// Interaction logic for ConfigureItems.xaml
    /// </summary>
    public partial class ConfigureItems : SubPage
    {
        private readonly CollectionViewSource rootNodesViewSource;
        private readonly CollectionViewSource castGroupsViewSource;

        public ConfigureItems(DbContextOptions<ShowContext> context_options) : base(context_options)
        {
            InitializeComponent();
            rootNodesViewSource = (CollectionViewSource)FindResource(nameof(rootNodesViewSource));
            castGroupsViewSource = (CollectionViewSource)FindResource(nameof(castGroupsViewSource));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            using var loading = new LoadingOverlay(this);//TODO fix percentages
            loading.Progress = 0;

            await context.CastGroups.LoadAsync();
            castGroupsViewSource.Source = context.CastGroups.Local.ToObservableCollection();

            await context.Requirements.LoadAsync();

            await context.Nodes.LoadAsync();
            loading.Progress = 40;
            await context.Nodes.OfType<Item>().Include(i => i.Roles).LoadAsync();
            loading.Progress = 90;
            rootNodesViewSource.Source = context.Nodes.Local.ToObservableCollection();
            rootNodesViewSource.View.Filter = n => ((Node)n).Parent == null;
            rootNodesViewSource.View.SortDescriptions.Add(StandardSort.For<Node>()); // sorts top level only, other levels sorted by SortIOrdered converter


            ConfigureDataGridColumns();
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

        private void ConfigureDataGridColumns()
        {
            
        }

        private void RolesDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            //<DataGrid.Columns>
            //    <DataGridTextColumn x:Name="HeaderName" Header="Name" Width="*" Binding="{Binding Name}"/>
            //    <DataGridTextColumn x:Name="HeaderJB" Header="JB"/>
            //    <DataGridTextColumn x:Name="HeaderJG" Header="JG"/>
            //    <DataGridTextColumn x:Name="HeaderSB" Header="SB"/>
            //    <DataGridTextColumn x:Name="HeaderSG" Header="SG"/>
            //    <DataGridTextColumn x:Name="HeaderTotal" Header="Total"/>
            //    <DataGridCheckBoxColumn x:Name="HeaderSinger" Header="Singer"/>
            //    <DataGridCheckBoxColumn x:Name="HeaderActor" Header="Actor"/>
            //    <DataGridCheckBoxColumn x:Name="HeaderDancer" Header="Dancer"/>
            //    <DataGridTemplateColumn x:Name="HeaderOther" Header="Other" Width="*">
            //        <DataGridTemplateColumn.CellTemplate>
            //            <DataTemplate>
            //                <TextBlock Text="Baritone, Adult, blah, etc."/>
            //            </DataTemplate>
            //        </DataGridTemplateColumn.CellTemplate>
            //        <DataGridTemplateColumn.CellEditingTemplate>
            //            <DataTemplate>
            //                <ComboBox />
            //            </DataTemplate>
            //        </DataGridTemplateColumn.CellEditingTemplate>
            //    </DataGridTemplateColumn>
            //</DataGrid.Columns>
            var columns = ((DataGrid)sender).Columns;
            columns.Add(new DataGridTextColumn
            {
                Binding = new Binding(nameof(Role.Name)),
                Header = "Name",
                Width = new(1, DataGridLengthUnitType.Star)
            });
            foreach (var cast_group in context.CastGroups.Local)
                columns.Add(new DataGridTextColumn
                {
                    Header = cast_group.Abbreviation,
                    Binding = new Binding(nameof(Role.CountByGroups))
                    {
                        Converter = new CountByGroupSelector(cast_group)
                    }
                });
            columns.Add(new DataGridTextColumn
            {
                Header = "Total",
                Binding = new Binding(nameof(Role.CountByGroups))
                {
                    Converter = new CountByGroupsTotal()
                },
                IsReadOnly = true
            });
            //TODO requirements not tested because requirements list in ConfigureShow is incorrectly showing criterias, so I can't set any as primary
            foreach (var requirement in context.Requirements.Local.Where(r => r.Primary))
                columns.Add(new DataGridCheckBoxColumn
                {
                    Header = requirement.Name,
                    Binding = new Binding
                    {
                        Converter = new CollectionContains(requirement),
                        ConverterParameter = new Binding(nameof(Role.Requirements))
                    }
                });
        }
    }
}
