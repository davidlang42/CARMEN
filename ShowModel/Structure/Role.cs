using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ShowModel.Applicants;
using ShowModel.Requirements;

namespace ShowModel.Structure
{
    public class Role : ICounted, INameOrdered //LATER implement INotifyPropertyChanged for completeness
    {
        public enum RoleStatus
        {
            NotCast,
            FullyCast,
            OverCast,
            UnderCast,
        }

        [Key]
        public int RoleId { get; private set; }
        public string Name { get; set; } = "";
        public virtual ICollection<Item> Items { get; private set; } = new ObservableCollection<Item>();
        public virtual ICollection<Requirement> Requirements { get; set; } = new ObservableCollection<Requirement>();
        public virtual ICollection<CountByGroup> CountByGroups { get; private set; } = new ObservableCollection<CountByGroup>();
        public virtual ICollection<Applicant> Cast { get; private set; } = new ObservableCollection<Applicant>();

        public uint CountFor(CastGroup group)
            => CountByGroups.Where(c => c.CastGroup == group).Select(c => c.Count).SingleOrDefault(); // defaults to 0

        public RoleStatus CheckStatus(AlternativeCast[] alternative_casts)
        {
            if (CountByGroups.Count == 0 || CountByGroups.All(cbg => cbg.Count == 0))
                return Cast.Count > 0 ? RoleStatus.OverCast : RoleStatus.FullyCast;
            if (Cast.Count == 0)
                return RoleStatus.NotCast;
            bool under_cast = false;
            foreach (var cast_by_group in Cast.GroupBy(a => a.CastGroup))
            {
                if (cast_by_group.Key is not CastGroup cast_group)
                    return RoleStatus.OverCast;
                //TODO handle alternate casts here properly (probably by bringing CastGroupAndCast into ShowModel)
                var required_count = CountFor(cast_group);
                if (cast_group.AlternateCasts)
                {
                    foreach(var alternative_cast in alternative_casts)
                    {
                        var actual_count = cast_by_group.Where(a => a.AlternativeCast == alternative_cast).Count();
                        if (required_count > actual_count)
                            return RoleStatus.OverCast;
                        if (required_count < actual_count)
                            under_cast = true; // don't return yet, because if any count is over cast, we want to return that first
                    }
                }
                else
                {
                    var actual_count = cast_by_group.Count();
                    if (required_count > actual_count)
                        return RoleStatus.OverCast;
                    if (required_count < actual_count)
                        under_cast = true; // don't return yet, because if any count is over cast, we want to return that first
                }
            }
            if (under_cast)
                return RoleStatus.UnderCast;
            return RoleStatus.FullyCast;
        }
    }
}
