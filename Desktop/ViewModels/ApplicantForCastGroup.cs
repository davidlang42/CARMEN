using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Desktop.ViewModels
{
    internal class ApplicantForCastGroup : ApplicantForSelection
    {
        readonly CastGroup castGroup;

        public override string SelectionText => $"Select {FirstName} into {castGroup.Name}";

        public override bool IsSelected
        {
            get => castGroup.Members.Contains(Applicant);
            set
            {
                if (value)
                {
                    if (!castGroup.Members.Contains(Applicant))
                    {
                        Applicant.CastGroup = castGroup;
                        castGroup.Members.Add(Applicant);
                    }
                }
                else
                {
                    if (castGroup.Members.Contains(Applicant))
                    {
                        Applicant.CastGroup = null;
                        castGroup.Members.Remove(Applicant);
                    }   
                }
                OnPropertyChanged();
            }
        }

        public ApplicantForCastGroup(Applicant applicant, Criteria[] criterias, CastGroup cast_group)
            : base(applicant, criterias)
        {
            castGroup = cast_group;
        }
    }
}
