using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Carmen.ShowModel.Structure.Section;

namespace CarmenUI.ViewModels
{
    public class ShowRootOrSectionView : IDisposable, INotifyPropertyChanged
    {
        bool disposed = false;

        private CastGroup[] castGroups;
        private Dictionary<CastGroup, uint> castMemberCounts;
        private Action<Node> removeNodeAction;

        public event PropertyChangedEventHandler? PropertyChanged;

        public InnerNode InnerNode { get; init; } //LATER this should be private

        public ObservableCollection<ChildView> Children { get; init; }

        public string SectionTypeName => (InnerNode as Section)?.SectionType.Name ?? "Show";

        public string? SectionDescription
        {
            get
            {
                if (InnerNode is not Section section)
                    return null;
                return (section.SectionType.AllowNoRoles ?
                    "Some cast members may not have a role in this " : "Every cast member must have a role within this ")
                    + section.SectionType.Name + "."
                    + "\n"
                    + (section.SectionType.AllowMultipleRoles ?
                    "Cast members can have more than one role in this " : "Cast members cannot have more than one role in this ")
                    + section.SectionType.Name + ".";
            }
        }

        public string? SectionError
        {
            get
            {
                if (InnerNode is not Section section)
                    return null;
                var result = section.RolesMatchCastMembers(castMemberCounts);
                return result switch
                {
                    RolesMatchResult.RolesMatch => null,
                    RolesMatchResult.TooManyRoles => $"There are currently too many roles in this {SectionTypeName} to meet those requirements.",
                    RolesMatchResult.TooFewRoles => $"There are currently too few roles in this {SectionTypeName} to meet those requirements.",
                    _ => throw new NotImplementedException($"RolesMatchResult not handled: {result}")
                };
            }
        }

        public uint[] SumOfRolesCount
        {
            get
            {
                var sum = new uint[castGroups.Length];
                var descendent_roles = InnerNode.Children
                    .SelectMany(n => n.ItemsInOrder())
                    .SelectMany(i => i.Roles)
                    .Distinct();
                foreach (var role in descendent_roles)
                    for (var i = 0; i < sum.Length; i++)
                        sum[i] += role.CountFor(castGroups[i]);
                return sum;
            }
        }

        public uint SumOfRolesTotal => SumOfRolesCount.Sum();

        public NullableCountByGroup[] CountByGroups { get; init; }

        /// <summary>The sum of CountByGroups.Count, if they are all set, otherwise null</summary>
        public uint? TotalCount
        {
            get
            {
                uint sum = 0;
                foreach (var cbg in CountByGroups)
                    if (cbg.Count == null)
                        return null;
                    else
                        sum += cbg.Count.Value;
                return sum;
            }
        }

        public string[] CountErrorBackgroundColors//LATER when these arrays change, _get is called once for each castGroup, therefore re-calcing it ~4 times instead of 1. this should probably be a stored value, which is updated once when the current propertychanged is called, similar for all other arrays like this in ShowRootOrSectionView/ChildView / ItemView/RoleOnlyView/RoleView
        {
            get
            {
                var colors = new string[castGroups.Length];
                for (var i = 0; i < colors.Length; i++)
                    if (CountByGroups[i].Count is uint required_count
                        && SumOfRolesCount[i] != required_count)
                        colors[i] = "LightCoral";//LATER use constants, also convert to brush
                    else
                        colors[i] = "White";//LATER use constants, also convert to brush
                return colors;
            }
        }

        public string NoChildrenErrorBackgroundColor => InnerNode.Children.Count == 0 ? "LightCoral" : "WhiteSmoke";//LATER use constants, also convert to brush

        public ShowRootOrSectionView(InnerNode inner_node, CastGroup[] cast_groups, int alternative_casts_count, Action<Node> remove_node_action)
        {
            InnerNode = inner_node;
            Children = new ObservableCollection<ChildView>(inner_node.Children.InOrder().Select(c =>
            {
                var cv = new ChildView(c, cast_groups);
                cv.PropertyChanged += ChildView_PropertyChanged;
                return cv;
            }));
            Children.CollectionChanged += Children_CollectionChanged;
            CountByGroups = cast_groups.Select(cg =>
            {
                var ncbg = new NullableCountByGroup(inner_node.CountByGroups, cg);
                ncbg.PropertyChanged += NullableCountByGroup_PropertyChanged;
                return ncbg;
            }).ToArray();
            this.castGroups = cast_groups;
            this.castMemberCounts = castGroups.ToDictionary(cg => cg, cg => cg.FullTimeEquivalentMembers(alternative_casts_count));
            this.removeNodeAction = remove_node_action;
        }

        private void NullableCountByGroup_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(CountErrorBackgroundColors));
        }

        private void Children_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null)
                        foreach (ChildView cv in e.OldItems)
                        {
                            cv.PropertyChanged -= ChildView_PropertyChanged;
                            removeNodeAction(cv.Node);
                            if (InnerNode.Children.Contains(cv.Node))
                                throw new ApplicationException("Called removeNodeAction but Node is still attached.");
                            cv.Dispose();
                        }
                    if (e.NewItems != null)
                        foreach (ChildView cv in e.NewItems)
                        {
                            InnerNode.Children.Add(cv.Node);
                            cv.PropertyChanged += ChildView_PropertyChanged;
                        }
                    ChildView_PropertyChanged(this, new PropertyChangedEventArgs(""));
                    break;
                case NotifyCollectionChangedAction.Reset://LATER is this implementation correct? it probably isn't used
                    foreach (var cv in Children)
                    {
                        cv.PropertyChanged -= ChildView_PropertyChanged;
                        cv.Dispose();
                    }
                    InnerNode.Children.Clear();
                    foreach (var cv in Children)
                    {
                        InnerNode.Children.Add(cv.Node);
                        cv.PropertyChanged += ChildView_PropertyChanged;
                    }
                    ChildView_PropertyChanged(this, new PropertyChangedEventArgs(""));
                    break;
                case NotifyCollectionChangedAction.Move:
                    // do nothing
                    break;
                default:
                    throw new NotImplementedException($"Action not handled: {e.Action}");
            }
        }

        private void ChildView_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(ChildView.CountByGroups))
            {
                OnPropertyChanged(nameof(SumOfRolesCount));
                OnPropertyChanged(nameof(SumOfRolesTotal));
                OnPropertyChanged(nameof(CountErrorBackgroundColors));
                OnPropertyChanged(nameof(NoChildrenErrorBackgroundColor));
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ChildView AddChild(Node new_child, ChildView? insert_after = null)
        {
            new_child.Parent = InnerNode;
            new_child.Order = InnerNode.Children.NextOrder();
            var child_view = new ChildView(new_child, castGroups);
            if (insert_after != null)
            {
                for (var i = 0; i < Children.Count; i++)
                    if (Children[i] == insert_after)
                    {
                        Children.Insert(i + 1, child_view);
                        break;
                    }
                InnerNode.Children.MoveInOrder(new_child, insert_after.Node);
            }
            else
            {
                Children.Add(child_view);
            }
            return child_view;
        }

        public void RemoveChild(ChildView child_view)
        {
            Children.Remove(child_view);
        }

        public void Dispose()
        {
            if (!disposed)
            {
                Children.CollectionChanged -= Children_CollectionChanged;
                foreach (var cv in Children)
                {
                    cv.PropertyChanged -= ChildView_PropertyChanged;
                    cv.Dispose();
                }
                foreach (var ncbg in CountByGroups)
                    ncbg.PropertyChanged -= NullableCountByGroup_PropertyChanged;
                disposed = true;
            }
        }
    }
}
