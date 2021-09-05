using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CarmenUI.ViewModels
{
    public abstract class NodeView : DependencyObject, INotifyPropertyChanged, IDisposable
    {
        bool disposed = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            nameof(IsSelected), typeof(bool), typeof(NodeView), new PropertyMetadata(false));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set
            {
                SetValue(IsSelectedProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            nameof(IsExpanded), typeof(bool), typeof(NodeView), new PropertyMetadata(true));

        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set
            {
                SetValue(IsExpandedProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            nameof(Status), typeof(ProcessStatus), typeof(NodeView), new PropertyMetadata(ProcessStatus.Loading));

        public ProcessStatus Status
        {
            get => (ProcessStatus)GetValue(StatusProperty);
            private set
            {
                SetValue(StatusProperty, value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
            nameof(Progress), typeof(double?), typeof(NodeView), new PropertyMetadata(null));

        /// <summary>Value between 0 and 1 indicating percentage complete.
        /// This will be null only while Status==Loading.</summary>
        public double? Progress
        {
            get => (double?)GetValue(ProgressProperty);
            private set
            {
                SetValue(ProgressProperty, value);
                OnPropertyChanged();
            }
        }

        /// <summary>Calculate the result of this node, returning the progress (between 0 and 1)
        /// and whether there are errors.</summary>
        protected abstract Task<(double progress, bool has_errors)> CalculateAsync(double child_progress, bool child_errors);

        public NodeView[] ChildrenInOrder { get; init; }

        public bool IsVisible => ShowCompleted || Status != ProcessStatus.Complete;

        private bool showCompleted = true;
        public bool ShowCompleted
        {
            get => showCompleted;
            set
            {
                if (showCompleted == value)
                    return;
                showCompleted = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        public abstract string Name { get; }

        public NodeView(IEnumerable<NodeView> children_in_order)
            : this(children_in_order.ToArray())
        { }

        public NodeView(NodeView[] children_in_order)
        {
            ChildrenInOrder = children_in_order;
            foreach (var child in ChildrenInOrder)
                child.PropertyChanged += Child_PropertyChanged;
        }

        private async void Child_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(Status))
                await UpdateAsync();
        }

        /// <summary>Update the Status and Progress of this node.</summary>
        public async Task UpdateAsync()
        {
            StartUpdate();
            if (ChildrenInOrder.Any(c => c.Status == ProcessStatus.Loading))
                return; // leave status as loading until child statuses update
            var child_progress = ChildrenInOrder.Average(c => c.Progress) ?? 1;
            var child_errors = ChildrenInOrder.Any(c => c.Status == ProcessStatus.Error);
            var (progress, has_errors) = await CalculateAsync(child_progress, child_errors);
            if (has_errors && progress == 1)
                progress = 0.99; // don't show more than 99% complete if there are errors
            FinishUpdate(progress, has_errors);
        }

        private void StartUpdate()
        {
            Progress = null;
            Status = ProcessStatus.Loading;
        }

        private void FinishUpdate(double progress, bool has_errors)
        {
            Progress = progress;
            Status = (has_errors, progress) switch
            {
                (true, _) => ProcessStatus.Error,
                (false, 1) => ProcessStatus.Complete,
                _ => ProcessStatus.None
            };
        }

        public async Task UpdateAllAsync()
        {
            if (ChildrenInOrder.Length == 0)
                await UpdateAsync();
            else
                foreach (var child in ChildrenInOrder)
                    await child.UpdateAllAsync();
        }

        /// <summary>Recursively set ShowCompleted to the given value</summary>
        public void SetShowCompleted(bool value)
        {
            foreach (var child in ChildrenInOrder)
                child.SetShowCompleted(value);
            ShowCompleted = value;
        }

        /// <summary>Searches recursively for the RoleNodeView representing the given Role, and marks it as selected.
        /// Any nodes on the path to it will be expanded once found. Returns whether or not it was found.</summary>
        public bool SelectRole(Role role)
        {
            if (this is RoleNodeView rv && rv.Role == role)
                return IsSelected = true;
            foreach (var child in ChildrenInOrder)
                if (child.SelectRole(role))
                    return IsExpanded = true;
            return false;
        }

        /// <summary>Searches recursively for the Section/ItemNodeView representing the given Node, and marks it as selected.
        /// Any nodes on the path to it will be expanded once found. Returns whether or not it was found.</summary>
        public bool SelectNode(Node node)
        {
            if (node is Section section && this is SectionNodeView sv && sv.Section == section
                || node is Item item && this is ItemNodeView iv && iv.Item == item)
                return IsSelected = true;
            foreach (var child in ChildrenInOrder)
                if (child.SelectNode(node))
                    return IsExpanded = true;
            return false;
        }

        /// <summary>Searches recursively for the NodeView which is selected, and marks it as not selected.
        /// Returns whether or not it was found.</summary>
        public bool ClearSelection()
        {
            if (IsSelected)
                return !(IsSelected = false);
            foreach (var child in ChildrenInOrder)
                if (child.ClearSelection())
                    return true;
            return false;
        }

        /// <summary>Searches recursively for the RoleNodeView for the given Role.
        /// This will return null if the Role is not found.</summary>
        public RoleNodeView? FindRoleView(Role role)
        {
            if (this is RoleNodeView rv && rv.Role == role)
                return rv;
            foreach (var child in ChildrenInOrder)
                if (child.FindRoleView(role) is RoleNodeView child_result)
                    return child_result;
            return null;
        }

        /// <summary>Recursively expands all NodeViews</summary>
        public void ExpandAll()
        {
            IsExpanded = true;
            foreach (var child in ChildrenInOrder)
                child.ExpandAll();
        }

        /// <summary>Recursively collapses all NodeViews except the path to the current selection.
        /// Returns whether or not a selected item was found.</summary>
        public bool CollapseAll()
        {
            var containing_selection = false;
            foreach (var child in ChildrenInOrder)
                if (child.CollapseAll())
                    containing_selection = true;
            IsExpanded = containing_selection;
            return containing_selection || IsSelected;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void Dispose()
        {
            if (!disposed)
            {
                foreach (var child in ChildrenInOrder)
                {
                    child.PropertyChanged -= Child_PropertyChanged;
                    child.Dispose();
                }
                disposed = true;
            }
        }

        public static NodeView CreateView(Node node, uint total_cast, AlternativeCast[] alternative_casts)
            => node switch
            {
                ShowRoot show_root => new ShowRootNodeView(show_root, total_cast, alternative_casts),
                Section section => new SectionNodeView(section, total_cast, alternative_casts),
                Item item => new ItemNodeView(item, alternative_casts),
                _ => throw new NotImplementedException($"Node type not handled: {typeof(Node)}")
            };
    }
}
