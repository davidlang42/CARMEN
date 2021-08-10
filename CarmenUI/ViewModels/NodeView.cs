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

        public bool IsSelected //LATER try to use isseleted/isexpanded here rather than hacky visual tree stuff to change selection
        {
            get => (bool)GetValue(IsSelectedProperty);
            set
            {
                SetValue(IsSelectedProperty, value);
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

        /// <summary>Searches recursively for the RoleNodeView for the given Role.
        /// This will throw an exception if the Role is not found.</summary>
        public RoleNodeView? FindRoleView(Role role)
        {
            if (this is RoleNodeView rv && rv.Role == role)
                return rv;
            foreach (var child in ChildrenInOrder)
                if (child.FindRoleView(role) is RoleNodeView child_result)
                    return child_result;
            return null;
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

        public static NodeView CreateView(Node node, int total_cast, AlternativeCast[] alternative_casts)
            => node switch
            {
                ShowRoot show_root => new ShowRootNodeView(show_root, total_cast, alternative_casts),
                Section section => new SectionNodeView(section, total_cast, alternative_casts),
                Item item => new ItemNodeView(item, alternative_casts),
                _ => throw new NotImplementedException($"Node type not handled: {typeof(Node)}")
            };
    }
}
