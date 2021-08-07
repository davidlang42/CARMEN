using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CarmenUI.ViewModels
{
    public abstract class NodeView : DependencyObject
    {
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

        protected void StartUpdate()
        {
            Status = ProcessStatus.Loading;
            Progress = null;
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

        public static NodeView CreateView(Node node)
            => node switch
            {
                ShowRoot show_root => new ShowRootNodeView(show_root),
                Section section => new SectionNodeView(section),
                Item item => new ItemNodeView(item),
                _ => throw new NotImplementedException($"Node type not handled: {typeof(Node)}")
            };
    }
}
