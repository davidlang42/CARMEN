using Microsoft.EntityFrameworkCore;
using ShowModel;
using ShowModel.Applicants;
using ShowModel.Criterias;
using ShowModel.Requirements;
using ShowModel.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class ShowSummary : Summary
    {
        public void Load(IReadOnlyCollection<Criteria> criterias, IReadOnlyCollection<CastGroup> cast_groups,
            IReadOnlyCollection<AlternativeCast> alternative_casts, IReadOnlyCollection<Tag> tags,
            IReadOnlyCollection<SectionType> section_types, IReadOnlyCollection<Requirement> requirements)
        {
            StartLoad();
            Rows.Add(new Row { Success = $"{criterias.Count} Audition Criteria" });
            Rows.Add(new Row { Success = cast_groups.Count.Plural("Cast Group") });
            var any_groups_alternating = cast_groups.Any(g => g.AlternateCasts);
            if (any_groups_alternating)
                Rows.Add(new Row { Success = alternative_casts.Count.Plural("Alternative Cast") });
            else
                Rows.Add(new Row { Success = "Alternating Casts are disabled" });
            Rows.Add(new Row { Success = tags.Count.Plural("Cast Tag") });
            Rows.Add(new Row { Success = section_types.Count.Plural("Section Type") });
            Rows.Add(new Row { Success = requirements.Count.Plural("Requirement") });
            if (criterias.Count == 0)
                Rows.Add(new Row { Fail = "At least one Criteria is required" });
            if (cast_groups.Count == 0)
                Rows.Add(new Row { Fail = "At least one Cast Group is required" });
            if (any_groups_alternating && alternative_casts.Count < 2)
                Rows.Add(new Row { Fail = "At least 2 alternative casts are required" });
            if (section_types.Count == 0)
                Rows.Add(new Row { Fail = "At least one Section Type is required" });
            FinishLoad(true);
        }
    }
}
