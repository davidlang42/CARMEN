using ShowModel;
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
        public override async void LoadAsync(ShowContext context)
        {
            //TODO add delay and implement async
            uint criterias = 4;
            uint cast_groups = 4;
            bool any_groups_alternating = true;
            uint alternative_casts = 1;
            uint tags = 6;
            uint section_types = 2;
            uint requirements = 12;
            await Task.Run(() => Thread.Sleep(1000));
            Rows.Clear();
            Rows.Add(new Row { Success = $"{criterias} Audition Criteria" });
            await Task.Run(() => Thread.Sleep(1000));
            Rows.Add(new Row { Success = cast_groups.Plural("Cast Group") });
            if (any_groups_alternating)
                Rows.Add(new Row { Success = alternative_casts.Plural("Alternative Cast") });
            else
                Rows.Add(new Row { Success = "Alternating Casts are disabled" });
            Rows.Add(new Row { Success = tags.Plural("Cast Tag") });
            Rows.Add(new Row { Success = section_types.Plural("Section Type") });
            Rows.Add(new Row { Success = requirements.Plural("Requirement") });
            await Task.Run(() => Thread.Sleep(1000));
            if (criterias == 0)
                Rows.Add(new Row { Fail = "At least one Criteria is required" });
            if (cast_groups == 0)
                Rows.Add(new Row { Fail = "At least one Cast Group is required" });
            if (criterias == 0)
                Rows.Add(new Row { Fail = "At least one Criteria is required" });
            if (any_groups_alternating && alternative_casts < 2)
                Rows.Add(new Row { Fail = "At least 2 alternative casts are required" });
            if (criterias == 0)
                Rows.Add(new Row { Fail = "At least one Section Type is required" });
            UpdateStatus(true);
        }
    }
}
