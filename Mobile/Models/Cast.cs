using Carmen.Mobile.Collections;
using Carmen.Mobile.Converters;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Models
{
    internal class Cast : Applicants
    {
        static DetailOption defaultOption = new DetailOption("Cast Number", new ApplicantComparer
        {
            ByFields =
            {
                a => a.CastNumber,
                a => a.AlternativeCast?.Initial
            }
        }, new FieldGetter<Applicant>(a => $"#{a.CastNumberAndCast} ({a.CastGroup?.Name})"));

        public DetailOption[] DetailOptions { get; private set; } = new[] { defaultOption };

        private DetailOption selectedOption = defaultOption;
        public DetailOption SelectedOption
        {
            get => selectedOption;
            set
            {
                if (selectedOption == value)
                    return;
                if (value != null)
                    selectedOption = value;
                OnPropertyChanged();
                if (Collection is FilteredSortedCollection<Applicant> collection)
                {
                    collection.SortBy = selectedOption.SortBy;
                }
            }
        }

        public Cast() : base(SimpleFilterByName)
        { }

        static bool SimpleFilterByName(Applicant a, string filter)
            => FullNameFormatter.Format(a.FirstName, a.LastName).Contains(filter, StringComparison.OrdinalIgnoreCase);

        public void Loaded(Applicant[] collection, Criteria[] criterias, Tag[] tags)
        {
            DetailOptions = defaultOption.Yield()
                .Concat(criterias.Select(DetailOption.FromCriteria))
                .Concat(tags.Select(DetailOption.FromTag))
                .ToArray();
            OnPropertyChanged(nameof(DetailOptions));
            base.Loaded(collection, selectedOption.SortBy);
        }
    }
}
