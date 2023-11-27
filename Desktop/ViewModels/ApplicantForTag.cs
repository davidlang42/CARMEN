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
    internal class ApplicantForTag : ApplicantForSelection
    {
        readonly Tag tag;

        public override string SelectionText => $"Apply {tag.Name} to {FirstName}";

        public override bool IsSelected
        {
            get => tag.Members.Contains(Applicant);
            set
            {
                if (value)
                {
                    if (!tag.Members.Contains(Applicant))
                    {
                        tag.Members.Add(Applicant);
                    }
                }
                else
                {
                    if (tag.Members.Contains(Applicant))
                    {
                        tag.Members.Remove(Applicant);
                    }
                }
                OnPropertyChanged();
            }
        }

        public ApplicantForTag(Applicant applicant, Criteria[] criterias, Tag tag)
            : base(applicant, criterias)
        {
            this.tag = tag;
        }
    }
}
