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
    internal class ApplicantForAlternativeCast : ApplicantForSelection
    {
        readonly AlternativeCast alternativeCast;

        public override string SelectionText => $"Assign {FirstName} to {alternativeCast.Name}";

        public override bool IsSelected
        {
            get => alternativeCast.Members.Contains(Applicant);
            set
            {
                if (value)
                {
                    if (!alternativeCast.Members.Contains(Applicant))
                    {
                        Applicant.AlternativeCast = alternativeCast;
                        alternativeCast.Members.Add(Applicant);
                    }
                }
                else
                {
                    if (alternativeCast.Members.Contains(Applicant))
                    {
                        Applicant.AlternativeCast = null;
                        alternativeCast.Members.Remove(Applicant);
                    }
                }
                OnPropertyChanged();
            }
        }

        public ApplicantForAlternativeCast(Applicant applicant, Criteria[] criterias, AlternativeCast alternative_cast)
            : base(applicant, criterias)
        {
            alternativeCast = alternative_cast;
        }
    }
}
