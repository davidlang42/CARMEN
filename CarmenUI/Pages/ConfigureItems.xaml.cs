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
        private readonly CollectionViewSource sectionTypesViewSource;

        public ConfigureItems(DbContextOptions<ShowContext> context_options) : base(context_options)
        {
            InitializeComponent();
            rootNodesViewSource = (CollectionViewSource)FindResource(nameof(rootNodesViewSource));
            castGroupsViewSource = (CollectionViewSource)FindResource(nameof(castGroupsViewSource));
            requirementsViewSource = (CollectionViewSource)FindResource(nameof(requirementsViewSource));
            sectionTypesViewSource = (CollectionViewSource)FindResource(nameof(sectionTypesViewSource));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            using var loading = new LoadingOverlay(this);
            loading.Progress = 0;
            await context.CastGroups.LoadAsync();
            castGroupsViewSource.Source = context.CastGroups.Local.ToObservableCollection();
            loading.Progress = 20;
            await context.Requirements.LoadAsync();
            requirementsViewSource.Source = context.Requirements.Local.ToObservableCollection();
            loading.Progress = 40;
            await context.Nodes.LoadAsync();
            loading.Progress = 60;
            await context.Nodes.OfType<Item>().Include(i => i.Roles).ThenInclude(r => r.Requirements).LoadAsync();
            rootNodesViewSource.Source = context.Nodes.Local.ToObservableCollection();
            rootNodesViewSource.View.Filter = n => ((Node)n).Parent == null;
            rootNodesViewSource.View.SortDescriptions.Add(StandardSort.For<Node>()); // sorts top level only, other levels sorted by SortIOrdered converter
            loading.Progress = 80;
            await context.SectionTypes.LoadAsync();
            sectionTypesViewSource.Source = context.SectionTypes.Local.ToObservableCollection();
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

        #region Drag & drop in itemsTreeView

        private Point lastMouseDown;
        private Node? draggedItem;
        private Node? draggedTarget;

        private void itemsTreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                lastMouseDown = e.GetPosition(itemsTreeView);
        }

        private void itemsTreeView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if ((e.GetPosition(itemsTreeView) - lastMouseDown).Length > 10)
                {
                    draggedItem = (Node?)itemsTreeView.SelectedItem;
                    if (draggedItem != null)
                    {
                        var drop_effect = DragDrop.DoDragDrop(itemsTreeView, itemsTreeView.SelectedValue, DragDropEffects.Move);
                        if (drop_effect == DragDropEffects.Move && draggedTarget != null && draggedItem != draggedTarget)
                        {
                            MoveNode(draggedItem, draggedTarget);
                            draggedTarget = null;
                            draggedItem = null;
                        }
                    }
                }
            }
        }

        private void itemsTreeView_DragOver(object sender, DragEventArgs e)
        {
            if ((e.GetPosition(itemsTreeView) - lastMouseDown).Length > 10)
                ProcessDragDrop(e, out _);
            e.Handled = true;
        }

        private void itemsTreeView_Drop(object sender, DragEventArgs e)
        {
            ProcessDragDrop(e, out draggedTarget);
            e.Handled = true;
        }

        private void ProcessDragDrop(DragEventArgs e, out Node? target_item)
        {
            target_item = (e.OriginalSource as TextBlock)?.DataContext as Node;
            e.Effects = target_item != null && draggedItem != null && target_item != draggedItem ? DragDropEffects.Move : DragDropEffects.None;
        }
        #endregion

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
                    Binding = new Binding($"{nameof(RoleOnlyView.CountByGroups)}[{array_index++}].{nameof(CountByGroup.Count)}")
                    {
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    }
                });
            }
            // Populate all footer cells (because doing any in XAML would require
            // knowing how many CountByGroup columns there are in the DataGrid)
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
                var bottom_text = new TextBox();
                bottom_text.SetBinding(TextBox.TextProperty, new Binding($"{nameof(ItemView.CountByGroups)}[{i}].{nameof(NullableCountByGroup.Count)}")
                {
                    TargetNullValue = "",
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
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
            top_total.SetBinding(TextBlock.TextProperty, new Binding(nameof(ItemView.SumOfRolesTotal))
            {
                StringFormat = "={0}"
            });
            Grid.SetColumn(top_total, column_index);
            footer.Children.Add(top_total);
            // add bottom total
            var bottom_total = new TextBlock();
            bottom_total.SetBinding(TextBlock.TextProperty, new Binding(nameof(ItemView.TotalCount))
            {
                StringFormat = "={0}"
            });
            Grid.SetColumn(bottom_total, column_index);
            Grid.SetRow(bottom_total, 1);
            footer.Children.Add(bottom_total);
        }

        private void itemsTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && itemsTreeView.SelectedItem is Node node && node is not ShowRoot)
            {
                var parent = node.Parent ?? throw new ApplicationException("Non-ShowRoot must have a parent.");
                parent.Children.Remove(node);
                var collection = (ObservableCollection<Node>)rootNodesViewSource.Source;
                collection.Remove(node);
            }
        }

        private void itemsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (rolesPanel.Content is ItemView existing_view)
                existing_view.Dispose();
            rolesPanel.Content = itemsTreeView.SelectedItem switch
            {
                Item item => new ItemView(item, context.CastGroups.Local.ToArray()),
                Section section => section,
                _ => null
            };
        }

        private void AddRoleButton_Click(object sender, RoutedEventArgs e)
        {
            var item_view = (ItemView)rolesPanel.Content;
            var data_grid = rolesPanel.VisualDescendants<DataGrid>().First();
            data_grid.Focus();
            var role_view = item_view.AddRole();
            data_grid.SelectedItem = role_view;
            data_grid.CurrentCell = new DataGridCellInfo(role_view, data_grid.Columns.First());
        }

        private void AddNode(object sender, ExecutedRoutedEventArgs e)
        {
            (var parent, var after) = itemsTreeView.SelectedItem switch
            {
                Item item => (item.Parent ?? throw new Exception("Item did not have a parent."), item),
                Section section => (section, null),
                ShowRoot show => (show, null),
                _ => (context.ShowRoot, null)
            };
            Node new_node = e.Parameter switch
            {
                SectionType section_type => new Section
                {
                    Name = $"New {section_type.Name}",
                    SectionType = section_type
                },
                _ => new Item
                {
                    Name = "New Item"
                }
            };
            new_node.Parent = parent;
            parent.Children.InsertInOrder(new_node, after);
        }

        private void MoveNode(Node dragged, Node target)
        {
            if (dragged is ShowRoot)
                return;
            if (dragged.Parent == null)
                throw new ApplicationException("Non-ShowRoot node must have a parent.");
            if (dragged.Parent == target.Parent) // move within parent
            {
                dragged.Parent.Children.MoveInOrder(dragged, target);
                var parent = dragged.Parent;
                parent.Children.Remove(dragged);
                parent.Children.Add(dragged);
            }
            else if (dragged.Parent == target) // move to start of parent
            {
                dragged.Parent.Children.MoveInOrder(dragged, dragged.Parent.Children.FirstOrder());
                var parent = dragged.Parent;
                parent.Children.Remove(dragged);
                parent.Children.Add(dragged);
            }
            else if (target is InnerNode inner_node) // move into the targeted section/show (at the end of existing children)
            {
                dragged.Parent.Children.Remove(dragged);
                dragged.Parent = inner_node;
                inner_node.Children.InsertInOrder(dragged);
            }
            else // move into a new parent (after the targeted item)
            {
                var new_parent = target.Parent ?? throw new ApplicationException("Non-InnerNode node must have a parent.");
                dragged.Parent.Children.Remove(dragged);
                dragged.Parent = new_parent;
                new_parent.Children.InsertInOrder(dragged, target);
            }
        }

        protected override void DisposeInternal()
        {
            if (rolesPanel.Content is ItemView existing_view)
                existing_view.Dispose();
            base.DisposeInternal();
        }
    }
}
