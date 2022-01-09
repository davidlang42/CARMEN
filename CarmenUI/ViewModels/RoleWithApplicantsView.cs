using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class RoleWithApplicantsView : RoleView
    {
        public CastGroupAndCast[] CastGroupsByCast { get; init; }

        /// <summary>Indicies match CastGroupsByCast</summary>
        public uint[] RequiredCast { get; init; }

        /// <summary>First-order indicies match CastGroupsByCast</summary>
        public Applicant[][] SelectedCast
        {
            get
            {
                var dictionary = Role.Cast.OrderBy(a => a.CastNumber).GroupBy(a => new CastGroupAndCast(a)).ToDictionary(g => g.Key, g => g.ToArray());
                var result = new Applicant[CastGroupsByCast.Length][];
                for (var i = 0; i < CastGroupsByCast.Length; i++)
                    if (dictionary.TryGetValue(CastGroupsByCast[i], out var group_cast))
                        result[i] = group_cast;
                    else
                        result[i] = Array.Empty<Applicant>();
                return result;
            }
        }

        public bool AnyCastSelected => Role.Cast.Any();

        /// <summary>Array arguments are not expected not to change over the lifetime of this View.
        /// Elements of the array may be monitored for changes, but the collection itself is not.</summary>
        public RoleWithApplicantsView(Role role, CastGroupAndCast[] cast_groups_by_cast)
            : base(role)
        {
            CastGroupsByCast = cast_groups_by_cast;
            RequiredCast = new uint[CastGroupsByCast.Length];
            for (var i = 0; i < CastGroupsByCast.Length; i++)
                RequiredCast[i] = role.CountFor(CastGroupsByCast[i].CastGroup);
        }
    }
}
