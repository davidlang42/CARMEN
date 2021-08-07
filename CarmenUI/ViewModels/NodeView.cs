using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CarmenUI.ViewModels
{
    public abstract class NodeView : DependencyObject
    {
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            nameof(IsSelected), typeof(bool), typeof(NodeView), new PropertyMetadata(false));

        public bool IsSelected //LATER try to use isseleted/isexpanded here rather than hacky visual tree stuff to change selection
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
            nameof(Status), typeof(ProcessStatus), typeof(NodeView), new PropertyMetadata(ProcessStatus.Loading));

        public ProcessStatus Status
        {
            get => (ProcessStatus)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
            nameof(Progress), typeof(double?), typeof(NodeView), new PropertyMetadata(null));

        /// <summary>Value between 0 and 1 indicating percentage complete.
        /// This will be null only while Status==Loading.</summary>
        public double? Progress
        {
            get => (double?)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        /// <summary>Update the Status and Progress of this node.
        /// Status/Progress should be Loading/null respectively while it is updating.</summary>
        public abstract Task UpdateAsync();

        public abstract ICollection<NodeView> ChildrenInOrder { get; }

        public abstract string Name { get; }

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

        protected void StartUpdate()
        {
            Status = ProcessStatus.Loading;
            Progress = null;
        }

        protected async Task<(double, bool)> UpdateChildren()
        {
            if (!ChildrenInOrder.Any())
                return (1, false);
            await Task.WhenAll(ChildrenInOrder.Select(c => c.UpdateAsync()));
            var average_progress = ChildrenInOrder.Average(c => c.Progress!.Value);
            var any_errors = ChildrenInOrder.Any(c => c.Status == ProcessStatus.Error);
            return (average_progress, any_errors);
        }

        protected void FinishUpdate(double progress, bool has_errors)
        {
            Status = (has_errors, progress) switch
            {
                (true, _) => ProcessStatus.Error,
                (false, 1) => ProcessStatus.Complete,
                _ => ProcessStatus.None
            };
            Progress = progress;
        }

        public static NodeView CreateView(Node node, int total_cast)
            => node switch
            {
                ShowRoot show_root => new ShowRootNodeView(show_root, total_cast),
                Section section => new SectionNodeView(section, total_cast),
                Item item => new ItemNodeView(item),
                _ => throw new NotImplementedException($"Node type not handled: {typeof(Node)}")
            };
    }
}
