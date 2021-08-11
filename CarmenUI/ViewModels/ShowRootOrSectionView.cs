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

namespace CarmenUI.ViewModels
{
    public class ShowRootOrSectionView : IDisposable, INotifyPropertyChanged
    {
        bool disposed = false;

        private CastGroup[] castGroups;

        public event PropertyChangedEventHandler? PropertyChanged;

        public InnerNode InnerNode { get; init; } //LATER this should be private

        public ObservableCollection<ChildView> Children { get; init; }

        public string SectionTypeName => (InnerNode as Section)?.SectionType.Name ?? "Show";

        public string SectionDescription
        {
            get
            {
                if (InnerNode is not Section section)
                    return "";
                return (section.SectionType.AllowNoRoles ?
                    "Some cast members may not have a role in this " : "Every cast member must have a role within this ")
                    + section.SectionType.Name + "."
                    + "\n"
                    + (section.SectionType.AllowMultipleRoles ?
                    "Cast members can have more than one role in this " : "Cast members cannot have more than one role in this ")
                    + section.SectionType.Name + ".";
            }
        }

        public uint?[] SumOfChildrenCount
        {
            get
            {
                var sum = new uint?[castGroups.Length];
                for (var i = 0; i < sum.Length; i++)
                    sum[i] = 0;
                foreach (var child in Children)
                    for (var i = 0; i < sum.Length; i++)
                        if (child.CountByGroups[i].Count is uint count && sum[i] != null)
                            sum[i] += count;
                        else
                            sum[i] = null;
                return sum;
            }
        }

        /// <summary>The sum of SumOfChildrenCount, if they are all set, otherwise null</summary>
        public uint? SumOfChildrenTotal
        {
            get
            {
                uint sum = 0;
                foreach (uint? value in SumOfChildrenCount)
                    if (value == null)
                        return null;
                    else
                        sum += value.Value;
                return sum;
            }
        }

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
                        && SumOfChildrenCount[i] is uint sum_count
                        && sum_count != required_count)
                        colors[i] = "LightCoral";//LATER use constants, also convert to brush
                    else
                        colors[i] = "White";//LATER use constants, also convert to brush
                return colors;
            }
        }

        public string NoChildrenErrorBackgroundColor => InnerNode.Children.Count == 0 ? "LightCoral" : "WhiteSmoke";//LATER use constants, also convert to brush

        public ShowRootOrSectionView(InnerNode inner_node, CastGroup[] cast_groups)
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
                            InnerNode.Children.Remove(cv.Node);
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
                OnPropertyChanged(nameof(SumOfChildrenCount));
                OnPropertyChanged(nameof(SumOfChildrenTotal));
                OnPropertyChanged(nameof(CountErrorBackgroundColors));
                OnPropertyChanged(nameof(NoChildrenErrorBackgroundColor));
            }
        }

        //TODO
        //public RoleOnlyView AddRole()
        //{
        //    var role_view = new RoleOnlyView(new Role(), castGroups);
        //    Children.Add(role_view);
        //    return role_view;
        //}

        //public void DeleteRole(RoleOnlyView role_view)
        //{
        //    Children.Remove(role_view);
        //}

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
