using Carmen.Mobile.Collections;
using Carmen.Mobile.Converters;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.Mobile.Models
{
    internal class DetailOption
    {
        public string Name { get; }
        public IComparer<Applicant> SortBy { get; }
        public FieldGetter<Applicant> DetailGetter { get; }

        public DetailOption(string name, IComparer<Applicant> sort_by, FieldGetter<Applicant> detail_getter)
        {
            Name = name;
            SortBy = sort_by;
            DetailGetter = detail_getter;
        }

        public static DetailOption FromCriteria(Criteria criteria)
            => new(criteria.Name, new ApplicantComparer
            {
                ByFields =
                {
                    a => a.MarkFor(criteria),
                    a => a.CastNumber,
                    a => a.AlternativeCast?.Initial
                }
            }, new FieldGetter<Applicant>(a => $"#{a.CastNumberAndCast} ({criteria.Name}: {a.FormattedMarkFor(criteria)})"));

        public static DetailOption FromTag(Tag tag)
            => new(tag.Name, new ApplicantComparer
            {
                ByFields =
                    {
                        a => a.Tags.Contains(tag),
                        a => a.CastNumber,
                        a => a.AlternativeCast?.Initial
                    }
            }, new FieldGetter<Applicant>(a => $"#{a.CastNumberAndCast} ({string.Join(", ", a.Tags.Select(t => t.Name))})"));
    }
}
