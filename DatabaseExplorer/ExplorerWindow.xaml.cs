﻿using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using ShowModel;
using ShowModel.Structure;
using ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ShowModel.Requirements;
using System.IO;
using System.Windows.Media.Imaging;

namespace DatabaseExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ExplorerWindow : Window
    {
        private ShowContext? _context;

        public ShowContext Context => _context ?? throw new Exception("Attempted to use Context before loading.");

        /// <summary>Create a new database context, disposing the old one if set</summary>
        private void CreateContext(string connection_string)
        {
            _context?.Dispose();
            var context_options = new DbContextOptionsBuilder<ShowContext>().UseSqlite(connection_string).Options;
            _context = new ShowContext(context_options);
        }

        private readonly CollectionViewSource applicantsViewSource;
        private readonly CollectionViewSource castGroupsViewSource;
        private readonly CollectionViewSource alternativeCastsViewSource;
        private readonly CollectionViewSource tagsViewSource;
        private readonly CollectionViewSource rootNodesViewSource;
        private readonly CollectionViewSource itemsViewSource;
        private readonly CollectionViewSource criteriaViewSource;
        private readonly CollectionViewSource imagesViewSource;
        private readonly CollectionViewSource sectionTypesViewSource;
        private readonly CollectionViewSource requirementsViewSource;
        private readonly CollectionViewSource requirementsSelectionSource;
        private readonly CallbackCommand addNodeCommand;
        private readonly CallbackCommand addCriteriaCommand;
        private readonly CallbackCommand addRequirementCommand;
        private readonly CallbackCommand uploadImageCommand;
        private readonly CallbackCommand pasteImageCommand;

        public ExplorerWindow()
        {
            InitializeComponent();
            applicantsViewSource = (CollectionViewSource)FindResource(nameof(applicantsViewSource));
            castGroupsViewSource = (CollectionViewSource)FindResource(nameof(castGroupsViewSource));
            alternativeCastsViewSource = (CollectionViewSource)FindResource(nameof(alternativeCastsViewSource));
            tagsViewSource = (CollectionViewSource)FindResource(nameof(tagsViewSource));
            rootNodesViewSource = (CollectionViewSource)FindResource(nameof(rootNodesViewSource));
            criteriaViewSource = (CollectionViewSource)FindResource(nameof(criteriaViewSource));
            imagesViewSource = (CollectionViewSource)FindResource(nameof(imagesViewSource));
            sectionTypesViewSource = (CollectionViewSource)FindResource(nameof(sectionTypesViewSource));
            itemsViewSource = (CollectionViewSource)FindResource(nameof(itemsViewSource));
            requirementsViewSource = (CollectionViewSource)FindResource(nameof(requirementsViewSource));
            requirementsSelectionSource = (CollectionViewSource)FindResource(nameof(requirementsSelectionSource));
            addNodeCommand = (CallbackCommand)FindResource(nameof(addNodeCommand));
            addNodeCommand.Callback = AddNode;
            addCriteriaCommand = (CallbackCommand)FindResource(nameof(addCriteriaCommand));
            addCriteriaCommand.Callback = AddCriteria;
            addRequirementCommand = (CallbackCommand)FindResource(nameof(addRequirementCommand));
            addRequirementCommand.Callback = AddRequirement;
            uploadImageCommand = (CallbackCommand)FindResource(nameof(uploadImageCommand));
            uploadImageCommand.Callback = UploadImage;
            pasteImageCommand = (CallbackCommand)FindResource(nameof(pasteImageCommand));
            pasteImageCommand.Callback = PasteImage;
        }

        private void CreateMenu_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Create New Database",
                Filter = "Sqlite Database (*.db)|*.db"
            };
            if (dialog.ShowDialog() == true)
            {
                OpenDatabase(dialog.FileName);
                Context.Database.EnsureDeleted();
                Context.Database.EnsureCreated();
                PopulateViews();
            }
        }

        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Open Existing Database",
                Filter = "Sqlite Database (*.db)|*.db|All Files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                OpenDatabase(dialog.FileName);
                PopulateViews();
            }
        }

        private void OpenDatabase(string filename)
        {
            var connection = new SqliteConnectionStringBuilder { DataSource = filename };
            CreateContext(connection.ToString());
            foreach (var ui_element in this.AllControls<UIElement>())
                ui_element.IsEnabled = true;
        }

        private void PopulateViews()
        {
            // Load the entities into EF Core
            Context.Applicants.Load();
            Context.AlternativeCasts.Load();
            Context.Tags.Load();
            Context.CastGroups.Load();
            Context.Nodes.Load();
            Context.Criterias.Load();
            Context.Images.Load();
            Context.SectionTypes.Load();
            Context.Requirements.Load();
            // Put collections into view source
            applicantsViewSource.Source = Context.Applicants.Local.ToObservableCollection();
            castGroupsViewSource.Source = Context.CastGroups.Local.ToObservableCollection();
            alternativeCastsViewSource.Source = Context.AlternativeCasts.Local.ToObservableCollection();
            tagsViewSource.Source = Context.Tags.Local.ToObservableCollection();
            rootNodesViewSource.Source = Context.Nodes.Local.ToObservableCollection();
            rootNodesViewSource.View.Filter = n => ((Node)n).Parent == null;
            rootNodesViewSource.View.SortDescriptions.Add(new SortDescription(nameof(IOrdered.Order), ListSortDirection.Ascending)); // sorts top level only, other levels sorted by SortIOrdered converter
            itemsViewSource.Source = Context.Nodes.Local.ToObservableCollection(); //TODO try Context.RootNode.ItemsInOrder(), but it wouldn't update automatically
            itemsViewSource.View.Filter = n => n is Item;
            criteriaViewSource.Source = Context.Criterias.Local.ToObservableCollection();
            imagesViewSource.Source = Context.Images.Local.ToObservableCollection();
            sectionTypesViewSource.Source = Context.SectionTypes.Local.ToObservableCollection();
            requirementsViewSource.Source = Context.Requirements.Local.ToObservableCollection();
            requirementsSelectionSource.Source = Context.Requirements.Local.ToObservableCollection(); // separate collection for combo boxes
        }

        private void SaveMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Context.SaveChanges();
                RefreshViews();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show("Could not save changes: \n" + ex.Message);
                RevertChanges();
            }
        }

        /// <summary>Updates all ItemControls with data generated by DB commit, eg. auto-ids, cascaded-deletes</summary>
        private void RefreshViews()
        {
            foreach (var items_control in this.AllControls<ItemsControl>())
                items_control.Items.Refresh();
        }

        private void RevertMenu_Click(object sender, RoutedEventArgs e) => RevertChanges();

        private void RevertChanges()
        {
            CreateContext(Context.Database.GetConnectionString());
            PopulateViews();
        }

        private void ExitMenu_Click(object sender, RoutedEventArgs e) => Close();

        private void TestDataMenu_Click(object sender, RoutedEventArgs e)
        {
            if (!TryInputNumber("How many ITEMS would you like to add?", "Test Data", out var items, 30))
                return;
            if (!TryInputNumber("How many SECTIONS would you like to add?", "Test Data", out var sections, 6))
                return;
            uint depth;
            bool every_depth;
            if (sections == 0)
            {
                depth = 0;
                every_depth = true;
            }
            else
            {
                if (!TryInputNumber("How DEEP should the structure be?", "Test Data", out depth, 1))
                    return;
                var result = MessageBox.Show("Should items be included at EVERY depth?", "Test Data", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No);
                if (result == MessageBoxResult.Cancel)
                    return;
                every_depth = result == MessageBoxResult.Yes;
            }
            if (!TryInputNumber("How many CAST GROUPS would you like to add?", "Test Data", out var cast_groups, 4))
                return;
            if (!TryInputNumber("How many TAGS would you like to add?", "Test Data", out var tags, 1))
                return;
            if (!TryInputNumber("How many IDENTIFIERS would you like to add?", "Test Data", out var identifiers, 1))
                return;
            if (!TryInputNumber("How many APPLICANTS would you like to add?", "Test Data", out var applicants, 100))
                return;
            if (!TryInputNumber("How many ROLES PER ITEM would you like to add?", "Test Data", out var roles, 5))
                return;
            using var test_data = new TestDataGenerator(Context);
            test_data.AddAlternativeCasts();
            test_data.AddCastGroups(cast_groups);
            Context.SaveChanges();
            test_data.AddShowStructure(items, sections, depth, include_items_at_every_depth: every_depth);
            Context.SaveChanges();
            test_data.AddCriteriaAndRequirements(); // after cast groups committed
            Context.SaveChanges();
            test_data.AddTags(tags); // after requirements, cast groups committed
            Context.SaveChanges();
            test_data.AddApplicants(applicants); // after criteria, tags, alternative casts committed
            Context.SaveChanges();
            test_data.AddRoles(roles); // after items, cast groups, requirements committed
            test_data.AddImages(); // after applicants, cast groups, tags committed
            Context.SaveChanges();
        }

        private bool TryInputNumber(string message, string title, out uint value, uint? default_response= null)
        {
            var response = default_response?.ToString() ?? "";
            while (true) {
                response = Microsoft.VisualBasic.Interaction.InputBox(message, title, response); // I'm sorry
                if (string.IsNullOrEmpty(response))
                {
                    value = default;
                    return false;
                }
                else if (uint.TryParse(response, out value))
                    return true;
            }
        }

        private void ClearDataMenu_Click(object sender, RoutedEventArgs e)
        {
            Context.Applicants.RemoveRange(Context.Applicants);
            Context.CastGroups.RemoveRange(Context.CastGroups);
            Context.Nodes.RemoveRange(Context.Nodes);
            Context.Criterias.RemoveRange(Context.Criterias);
            Context.Images.RemoveRange(Context.Images);
            Context.SectionTypes.RemoveRange(Context.SectionTypes);
            Context.Requirements.RemoveRange(Context.Requirements);
            Context.AlternativeCasts.RemoveRange(Context.AlternativeCasts);
            Context.Tags.RemoveRange(Context.Tags);
            SaveMenu_Click(sender, e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Context.Dispose();
            base.OnClosing(e);
        }

        private void AddNode(object? parameter)
        {
            (var parent, var after) = itemsTreeView.SelectedItem switch
            {
                Item item => (item.Parent ?? throw new Exception("Item did not have a parent."), item),
                Section section => (section, null),
                ShowRoot show => (show, null),
                _ => (Context.ShowRoot, null)
            };
            Node new_node = parameter switch
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

        private void itemsTreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && itemsTreeView.SelectedItem is Node node && node is not ShowRoot)
            {
                var parent = node.Parent ?? throw new ApplicationException("Non-ShowRoot must have a parent.");
                parent.Children.Remove(node);
                Context.Remove(node); //TODO children should be cascade deleted, but are not
            }
        }

        private void AddCriteria(object? parameter)
        {
            Criteria criteria = parameter switch
            {
                nameof(NumericCriteria) => new NumericCriteria(),
                nameof(SelectCriteria) => new SelectCriteria(),
                nameof(BooleanCriteria) => new BooleanCriteria(),
                _ => throw new ArgumentException($"Criteria type '{parameter}' does not exist.")
            };
            Context.Criterias.InsertInOrder(criteria); //TODO this re-allocates the last order value if more than one criteria is added without saving
        }

        private void AddRequirement(object? parameter)
        {
            Requirement requirement = parameter switch
            {
                nameof(AbilityExactRequirement) => new AbilityExactRequirement(),
                nameof(AbilityRangeRequirement) => new AbilityRangeRequirement(),
                nameof(AgeRequirement) => new AgeRequirement(),
                nameof(TagRequirement) => new TagRequirement(),
                nameof(GenderRequirement) => new GenderRequirement(),
                nameof(AndRequirement) => new AndRequirement(),
                nameof(OrRequirement) => new OrRequirement(),
                nameof(XorRequirement) => new XorRequirement(),
                nameof(NotRequirement) => new NotRequirement(),
                _ => throw new ArgumentException($"Requirement type '{parameter}' does not exist.")
            };
            Context.Requirements.InsertInOrder(requirement); //TODO this re-allocates the last order value if more than one requirement is added without saving
        }

        private void UploadImage(object? _)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Upload Image",
                Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp|All Files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                var image = new ShowModel.Image
                {
                    Name = Path.GetFileName(dialog.FileName),
                    ImageData = File.ReadAllBytes(dialog.FileName)
                };
                Context.Images.Add(image);
            }
        }

        private void PasteImage(object? _)
        {
            if (Clipboard.GetImage() is BitmapSource source)
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                using var stream = new MemoryStream();
                encoder.Save(stream);
                var image = new ShowModel.Image
                {
                    Name = $"Pasted at {DateTime.Now:yyyy-MM-dd HH:mm}",
                    ImageData = stream.ToArray()
                };
                Context.Images.Add(image);
            }
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
    }
}