using CarmenUI.Converters;
using CarmenUI.ViewModels;
using CarmenUI.Windows;
using Microsoft.EntityFrameworkCore;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
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
using Section = Carmen.ShowModel.Structure.Section;
using System.Collections;

namespace CarmenUI.Pages
{
    /// <summary>
    /// Interaction logic for ConfigureItems.xaml
    /// </summary>
    public partial class ConfigureItems : SubPage
    {
        private CastGroup[]? _castGroups;
        private AlternativeCast[]? _alternativeCasts;
        private Requirement[]? _primaryRequirements;

        private CastGroup[] castGroups => _castGroups
            ?? throw new ApplicationException($"Tried to used {nameof(castGroups)} before it was loaded.");

        private AlternativeCast[] alternativeCasts => _alternativeCasts
            ?? throw new ApplicationException($"Tried to used {nameof(alternativeCasts)} before it was loaded.");

        private Requirement[] primaryRequirements => _primaryRequirements
            ?? throw new ApplicationException($"Tried to used {nameof(_primaryRequirements)} before it was loaded.");

        private readonly CollectionViewSource rootNodesViewSource;
        private readonly CollectionViewSource castGroupsViewSource;
        private readonly CollectionViewSource nonPrimaryRequirementsViewSource;
        private readonly CollectionViewSource itemsViewSource;
        private readonly CollectionViewSource sectionTypesViewSource;
        private readonly CollectionViewSource castMembersDictionarySource;

        public ConfigureItems(DbContextOptions<ShowContext> context_options) : base(context_options)
        {
            InitializeComponent();
            rootNodesViewSource = (CollectionViewSource)FindResource(nameof(rootNodesViewSource));
            castGroupsViewSource = (CollectionViewSource)FindResource(nameof(castGroupsViewSource));
            castGroupsViewSource.SortDescriptions.Add(StandardSort.For<CastGroup>());
            nonPrimaryRequirementsViewSource = (CollectionViewSource)FindResource(nameof(nonPrimaryRequirementsViewSource));
            itemsViewSource = (CollectionViewSource)FindResource(nameof(itemsViewSource));
            sectionTypesViewSource = (CollectionViewSource)FindResource(nameof(sectionTypesViewSource));
            sectionTypesViewSource.SortDescriptions.Add(StandardSort.For<SectionType>());
            castMembersDictionarySource = (CollectionViewSource)FindResource(nameof(castMembersDictionarySource));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            using (var loading = new LoadingOverlay(this).AsSegment(nameof(ConfigureItems)))
            {
                using (loading.Segment(nameof(ShowContext.AlternativeCasts), "Alternative casts"))
                    _alternativeCasts = await context.AlternativeCasts.InNameOrder().ToArrayAsync();
                using (loading.Segment(nameof(ShowContext.CastGroups), "Cast groups"))
                    _castGroups = await context.CastGroups.InOrder().ToArrayAsync();
                using (loading.Segment(nameof(CastGroup.FullTimeEquivalentMembers), "Cast members"))
                    castMembersDictionarySource.Source = castGroups.ToDictionary(cg => cg, cg => cg.FullTimeEquivalentMembers(alternativeCasts.Length));
                castGroupsViewSource.Source = context.CastGroups.Local.ToObservableCollection();
                using (loading.Segment(nameof(ShowContext.Requirements), "Requirements"))
                {
                    await context.Requirements.LoadAsync();
                    nonPrimaryRequirementsViewSource.Source = context.Requirements.Local.Where(r => !r.Primary).InOrder().ToArray();
                    _primaryRequirements = context.Requirements.Local.Where(r => r.Primary).InOrder().ToArray();
                }
                using (loading.Segment(nameof(ShowContext.Nodes), "Nodes"))
                    await context.Nodes.LoadAsync();
                using (loading.Segment(nameof(ShowContext.Nodes) + nameof(Item) + nameof(Item.Roles) + nameof(Role.Requirements), "Items"))
                    await context.Nodes.OfType<Item>().Include(i => i.Roles).ThenInclude(r => r.Requirements).LoadAsync();
                using (loading.Segment(nameof(ConfigureItems) + nameof(rootNodesViewSource), "Sorting"))
                {
                    rootNodesViewSource.Source = context.Nodes.Local.ToObservableCollection();
                    rootNodesViewSource.View.Filter = n => ((Node)n).Parent == null;
                    rootNodesViewSource.View.SortDescriptions.Add(StandardSort.For<Node>()); // sorts top level only, other levels sorted by SortIOrdered converter
                }
                using (loading.Segment(nameof(ShowContext.SectionTypes), "Section types"))
                    await context.SectionTypes.LoadAsync();
                sectionTypesViewSource.Source = context.SectionTypes.Local.ToObservableCollection();
            }
            if (itemsTreeView.VisualDescendants<TreeViewItem>().FirstOrDefault() is TreeViewItem show_root_tvi)
                show_root_tvi.IsSelected = true;
        }

        private void PopulateItemsViewSource()
        {
            itemsViewSource.Source = context.ShowRoot.ItemsInOrder().ToArray();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => CancelChangesAndReturn();

        private void SaveButton_Click(object sender, RoutedEventArgs e)
            => SaveChangesAndReturn();

        private void RolesDataGrid_Initialized(object sender, EventArgs e)
        {
            const int COLUMNS_BEFORE_CBG = 1;
            const int EXTRA_FOOTER_COLUMNS = 1; // accounts for DataGrid row header
            // Insert columns for CountByGroups (which can't be done in XAML
            // because they are dynamic and DataGridColumns is not a panel)
            var datagrid = (DataGrid)sender;
            int column_index = COLUMNS_BEFORE_CBG;
            int array_index = 0;
            foreach (var cast_group in castGroups)
            {
                datagrid.Columns.Insert(column_index++, new DataGridTextColumn
                {
                    Header = cast_group.Abbreviation,
                    Binding = new Binding($"{nameof(RoleOnlyView.CountByGroups)}[{array_index++}].{nameof(NullableCountByGroup.Count)}")
                    {
                        TargetNullValue = "",
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    }
                });
            }
            // Insert columns for PrimaryRequirements (which can't be done in XAML
            // because they are dynamic and DataGridColumns is not a panel)
            column_index++; // skip the "Total count" column
            array_index = 0;
            foreach (var requirement in primaryRequirements)
            {
                datagrid.Columns.Insert(column_index++, new DataGridCheckBoxColumn
                {
                    Header = requirement.Name,
                    Binding = new Binding($"{nameof(RoleOnlyView.PrimaryRequirements)}[{array_index++}].{nameof(SelectableObject<Requirement>.IsSelected)}")
                    {
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    }
                });
            }
            // Populate all footer cells (because doing any in XAML would require knowing
            // how many CountByGroup/PrimaryRequirement columns there are in the DataGrid)
            var footer = ((Grid)datagrid.Parent).Children.OfType<Grid>().Single();
            column_index = COLUMNS_BEFORE_CBG + EXTRA_FOOTER_COLUMNS;
            for (var i = 0; i < castGroups.Length; i++)
            {
                // define column
                var column_definition = new ColumnDefinition();
                column_definition.SetBinding(ColumnDefinition.WidthProperty, new Binding
                {
                    ElementName = datagrid.Name,
                    Path = new PropertyPath($"{nameof(DataGrid.Columns)}[{column_index - EXTRA_FOOTER_COLUMNS}].{nameof(DataGridColumn.ActualWidth)}")
                });
                footer.ColumnDefinitions.Add(column_definition);
                // add top text
                var top_text = new TextBlock() { FontSize = datagrid.FontSize };
                top_text.SetBinding(TextBlock.TextProperty, new Binding($"{nameof(ItemView.SumOfRolesCount)}[{i}]"));
                Grid.SetColumn(top_text, column_index);
                footer.Children.Add(top_text);
                // add bottom text
                var bottom_text = new TextBox() { FontSize = datagrid.FontSize };
                bottom_text.SetBinding(TextBox.TextProperty, new Binding($"{nameof(ItemView.CountByGroups)}[{i}].{nameof(NullableCountByGroup.Count)}")
                {
                    TargetNullValue = "*",
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
                bottom_text.SetBinding(TextBox.BackgroundProperty, new Binding($"{nameof(ItemView.CountErrorBackgroundColors)}[{i}]"));
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
                Path = new PropertyPath($"{nameof(DataGrid.Columns)}[{column_index - EXTRA_FOOTER_COLUMNS}].{nameof(DataGridColumn.ActualWidth)}")
            });
            footer.ColumnDefinitions.Add(total_column);
            // add top total
            var top_total = new TextBlock() { FontSize = datagrid.FontSize };
            top_total.SetBinding(TextBlock.TextProperty, new Binding(nameof(ItemView.SumOfRolesTotal))
            {
                StringFormat = "={0}"
            });
            Grid.SetColumn(top_total, column_index);
            footer.Children.Add(top_total);
            // add bottom total
            var bottom_total = new TextBlock() { FontSize = datagrid.FontSize };
            bottom_total.SetBinding(TextBlock.TextProperty, new Binding(nameof(ItemView.TotalCount))
            {
                TargetNullValue = "=*",
                StringFormat = "={0}"
            });
            Grid.SetColumn(bottom_total, column_index);
            Grid.SetRow(bottom_total, 1);
            footer.Children.Add(bottom_total);
            // increment column
            column_index++;
            // define primary requirement description column
            var primary_requirements_column = new ColumnDefinition()
            {
                Width = new(1, GridUnitType.Star)
            };
            footer.ColumnDefinitions.Add(primary_requirements_column);
            // add top text
            var primary_requirements_text = new TextBlock() { FontSize = datagrid.FontSize };
            primary_requirements_text.SetBinding(TextBlock.TextProperty, new Binding(nameof(ItemView.DescriptiveSumOfPrimaryRequirements))
            {
                TargetNullValue = "",
                StringFormat = "({0})"
            });
            Grid.SetColumn(primary_requirements_text, column_index);
            footer.Children.Add(primary_requirements_text);
            // increment column
            //column_index++;
        }

        private void ChildrenDataGrid_Initialized(object sender, EventArgs e)//LATER mostly copied from RolesDataGrid_Initialized, abstract somehow
        {
            const int COLUMNS_BEFORE_CBG = 2;
            const int EXTRA_FOOTER_COLUMNS = 1; // accounts for DataGrid row header
            // Insert columns for CountByGroups (which can't be done in XAML
            // because they are dynamic and DataGridColumns is not a panel)
            var datagrid = (DataGrid)sender;
            int column_index = COLUMNS_BEFORE_CBG;
            int array_index = 0;
            foreach (var cast_group in castGroups)
            {
                datagrid.Columns.Insert(column_index++, new DataGridTextColumn
                {
                    Header = cast_group.Abbreviation,
                    Binding = new Binding($"{nameof(ChildView.CountByGroups)}[{array_index++}].{nameof(NullableCountByGroup.Count)}")
                    {
                        TargetNullValue = "*",
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    }
                });
            }
            // Populate all footer cells (because doing any in XAML would require
            // knowing how many CountByGroup columns there are in the DataGrid)
            var footer = ((Grid)datagrid.Parent).Children.OfType<Grid>().Single();
            column_index = COLUMNS_BEFORE_CBG + EXTRA_FOOTER_COLUMNS;
            for (var i = 0; i < castGroups.Length; i++)
            {
                // define column
                var column_definition = new ColumnDefinition();
                column_definition.SetBinding(ColumnDefinition.WidthProperty, new Binding
                {
                    ElementName = datagrid.Name,
                    Path = new PropertyPath($"{nameof(DataGrid.Columns)}[{column_index - EXTRA_FOOTER_COLUMNS}].{nameof(DataGridColumn.ActualWidth)}")
                });
                footer.ColumnDefinitions.Add(column_definition);
                // add top text
                var top_text = new TextBlock() { FontSize = datagrid.FontSize };
                top_text.SetBinding(TextBlock.TextProperty, new Binding($"{nameof(ShowRootOrSectionView.SumOfRolesCount)}[{i}]"));
                Grid.SetColumn(top_text, column_index);
                footer.Children.Add(top_text);
                // add bottom text
                var bottom_text = new TextBox() { FontSize = datagrid.FontSize };
                bottom_text.SetBinding(TextBox.TextProperty, new Binding($"{nameof(ShowRootOrSectionView.CountByGroups)}[{i}].{nameof(NullableCountByGroup.Count)}")
                {
                    TargetNullValue = "*",
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
                bottom_text.SetBinding(TextBox.BackgroundProperty, new Binding($"{nameof(ShowRootOrSectionView.CountErrorBackgroundColors)}[{i}]"));
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
                Path = new PropertyPath($"{nameof(DataGrid.Columns)}[{column_index - EXTRA_FOOTER_COLUMNS}].{nameof(DataGridColumn.ActualWidth)}")
            });
            footer.ColumnDefinitions.Add(total_column);
            // add top total
            var top_total = new TextBlock() { FontSize = datagrid.FontSize };
            top_total.SetBinding(TextBlock.TextProperty, new Binding(nameof(ShowRootOrSectionView.SumOfRolesTotal))
            {
                StringFormat = "={0}"
            });
            Grid.SetColumn(top_total, column_index);
            footer.Children.Add(top_total);
            // add bottom total
            var bottom_total = new TextBlock() { FontSize = datagrid.FontSize };
            bottom_total.SetBinding(TextBlock.TextProperty, new Binding(nameof(ShowRootOrSectionView.TotalCount))
            {
                TargetNullValue = "=*",
                StringFormat = "={0}"
            });
            Grid.SetColumn(bottom_total, column_index);
            Grid.SetRow(bottom_total, 1);
            footer.Children.Add(bottom_total);
            // increment column
            //column_index++;
        }

        private void itemsTreeView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Properties.Settings.Default.MoveOnCtrlArrow
                && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Up)
                {
                    moveUpButton_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.Key == Key.Down)
                {
                    moveDownButton_Click(sender, e);
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Delete && itemsTreeView.SelectedItem is Node node && node is not ShowRoot)
            {
                context.DeleteNode(node);
                e.Handled = true;
            }
        }

        private void itemsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (rolesPanel.Content is ItemView existing_view)
                existing_view.Dispose();
            PopulateItemsViewSource();
            rolesPanel.Content = itemsTreeView.SelectedItem switch
            {
                Item item => new ItemView(item, castGroups, primaryRequirements),
                Section section => new ShowRootOrSectionView(section, castGroups, alternativeCasts.Length),
                ShowRoot show_root => new ShowRootOrSectionView(show_root, castGroups, alternativeCasts.Length),
                _ => null
            };
            EnableMoveButtons(itemsTreeView.SelectedItem as Node);
        }

        private void AddRole_Click(object sender, RoutedEventArgs e)
        {
            var item_view = (ItemView)rolesPanel.Content;
            var data_grid = rolesPanel.VisualDescendants<DataGrid>().First();
            data_grid.Focus();
            var role_view = item_view.AddRole(data_grid.SelectedItems.OfType<RoleOnlyView>().SingleOrDefaultSafe());
            data_grid.SelectedItem = role_view;
            data_grid.CurrentCell = new DataGridCellInfo(role_view, data_grid.Columns.First());
        }

        private void AddNodeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
            => AddNode(e.Parameter as SectionType);

        private void AddNode(SectionType? section_type)
        {
            var section_view = (ShowRootOrSectionView)rolesPanel.Content;
            var data_grid = rolesPanel.VisualDescendants<DataGrid>().First();
            data_grid.Focus();
            Node new_node = section_type != null
                ? new Section
                {
                    Name = $"New {section_type.Name}",
                    SectionType = section_type
                }
                : new Item
                {
                    Name = "New Item"
                };
            var child_view = section_view.AddChild(new_node, data_grid.SelectedItems.OfType<ChildView>().SingleOrDefaultSafe());
            data_grid.SelectedItem = child_view;
            data_grid.CurrentCell = new DataGridCellInfo(child_view, data_grid.Columns.First());
        }

        private void MoveNode(Node dragged, Node target)//LATER remove if not needed
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

        private void RolesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is ScrollViewer) // Double clicked empty space
                AddRole_Click(sender, e);
        }

        private void ChildrenDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is ScrollViewer) // Double clicked empty space
                AddNode(null);
        }

        private void RemoveRole_Click(object sender, RoutedEventArgs e)
        {
            var item_view = (ItemView)rolesPanel.Content;
            var data_grid = rolesPanel.VisualDescendants<DataGrid>().First();
            var selected_roles = data_grid.SelectedItems.OfType<RoleOnlyView>().ToArray();
            foreach (var selected_role in selected_roles)
                item_view.RemoveRole(selected_role);
        }

        private void DeleteItemOrSection_Click(object sender, RoutedEventArgs e)
        {
            var view = (ShowRootOrSectionView)rolesPanel.Content;
            var data_grid = rolesPanel.VisualDescendants<DataGrid>().First();
            var selected_children = data_grid.SelectedItems.OfType<ChildView>().ToArray();
            foreach (var selected_child in selected_children)
                view.RemoveChild(selected_child);
        }

        private void moveUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (itemsTreeView.SelectedItem is Node n)
                MoveNodeUp(n);
        }

        private void moveDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (itemsTreeView.SelectedItem is Node n)
                MoveNodeDown(n);
        }

        private void EnableMoveButtons(Node? selected)
        {
            if (selected is not Node node || node.Parent is not InnerNode parent)
                moveUpButton.IsEnabled = moveDownButton.IsEnabled = false;
            else if (parent is not ShowRoot show_root)
                moveUpButton.IsEnabled = moveDownButton.IsEnabled = true;
            else
            {
                moveUpButton.IsEnabled = show_root.Children.InOrder().First() != node;
                moveDownButton.IsEnabled = show_root.Children.InOrder().Last() != node;
            }
        }

        private void MoveNodeUp(Node node)
        {
            if (node is ShowRoot)
                return;
            if (node.Parent is not InnerNode parent)
                throw new ApplicationException("Non-ShowRoot node must have a parent.");
            if (node.SiblingAbove() is not Node sibling_above)
            {
                if (parent is ShowRoot)
                    return; // already at top of everything, do nothing
                var grandparent = parent.Parent ?? throw new ApplicationException("Non-ShowRoot node must have a parent.");
                // at top of parent, move to sibling of parent above parent
                parent.Children.Remove(node);
                grandparent.Children.InsertInOrder(node, parent.Order);
                node.Parent = grandparent;
                itemsTreeView.SetSelectedItem(node);
            }
            else if (sibling_above is InnerNode inner_node_above)
            {
                // sibling above can have children, move to last child of sibling above
                parent.Children.Remove(node);
                inner_node_above.Children.InsertInOrder(node);
                node.Parent = inner_node_above;
                itemsTreeView.SetSelectedItem(node);
            }
            else
            {
                // sibling above cannot have children, move above sibling
                parent.Children.MoveInOrder(node, sibling_above.Order);
            }
            EnableMoveButtons(node);
        }

        private void MoveNodeDown(Node node)
        {
            if (node is ShowRoot)
                return;
            if (node.Parent is not InnerNode parent)
                throw new ApplicationException("Non-ShowRoot node must have a parent.");
            if (node.SiblingBelow() is not Node sibling_below)
            {
                if (parent is ShowRoot)
                    return; // already at bottom of everything, do nothing
                var grandparent = parent.Parent ?? throw new ApplicationException("Non-ShowRoot node must have a parent.");
                // at bottom of parent, move to sibling of parent after parent
                parent.Children.Remove(node);
                grandparent.Children.InsertInOrder(node, parent);
                node.Parent = grandparent;
                itemsTreeView.SetSelectedItem(node);
            }
            else if (sibling_below is InnerNode inner_node_below)
            {
                // sibling below can have children, move to first child of sibling below
                parent.Children.Remove(node);
                inner_node_below.Children.InsertInOrder(node, inner_node_below.Children.FirstOrder());
                node.Parent = inner_node_below;
                itemsTreeView.SetSelectedItem(node);
            }
            else
            {
                // sibling below cannot have children, move after sibling
                parent.Children.MoveInOrder(node, sibling_below);
            }
            EnableMoveButtons(node);
        }
    }
}
