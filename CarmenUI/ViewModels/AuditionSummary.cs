using ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarmenUI.ViewModels
{
    public class AuditionSummary : Summary
    {
        public override async Task LoadAsync(ShowContext context)//TODO 2) this re-uses a lot of info & processing done by ApplicantsSummary
        {
            /*
                <StackPanel x:Name="AuditionApplicantsSummary" Visibility="Collapsed">
                    <TextBlock Text="152 Applicants Auditioned" HorizontalAlignment="Left" FontSize="20"/>
                    <TextBlock Text="" HorizontalAlignment="Left" FontSize="10"/>
                    <TextBlock Text="36 eligible for Senior Boy" HorizontalAlignment="Left" FontSize="20"/>
                    <TextBlock Text="60 eligible for Senior Girl" HorizontalAlignment="Left" FontSize="20"/>
                    <TextBlock Text="18 eligible for Junior Boy" HorizontalAlignment="Left" FontSize="20"/>
                    <TextBlock Text="28 eligible for Junior Girl" HorizontalAlignment="Left" FontSize="20"/>
                    <TextBlock Text="10 registered applicants have not auditioned" TextWrapping="Wrap" HorizontalAlignment="Left" FontSize="20" FontStyle="Italic" Foreground="Red"/>
                </StackPanel>
            */
            StartLoad();
            var criterias = await context.ColdCountAsync(c => c.Criterias);
            Rows.Add(new Row { Success = $"{criterias} Audition Criteria" });
            await context.ColdLoadAsync(c => c.CastGroups);
            var cast_groups = context.CastGroups.Local.Count();
            Rows.Add(new Row { Success = cast_groups.Plural("Cast Group") });
            var any_groups_alternating = context.CastGroups.Local.Any(g => g.AlternateCasts);
            var alternative_casts = await context.ColdCountAsync(c => c.AlternativeCasts);
            if (any_groups_alternating)
                Rows.Add(new Row { Success = alternative_casts.Plural("Alternative Cast") });
            else
                Rows.Add(new Row { Success = "Alternating Casts are disabled" });
            var tags = await context.ColdCountAsync(c => c.Tags);
            Rows.Add(new Row { Success = tags.Plural("Cast Tag") });
            var section_types = await context.ColdCountAsync(c => c.SectionTypes);
            Rows.Add(new Row { Success = section_types.Plural("Section Type") });
            var requirements = await context.ColdCountAsync(c => c.Requirements);
            Rows.Add(new Row { Success = requirements.Plural("Requirement") });
            if (criterias == 0)
                Rows.Add(new Row { Fail = "At least one Criteria is required" });
            if (cast_groups == 0)
                Rows.Add(new Row { Fail = "At least one Cast Group is required" });
            if (any_groups_alternating && alternative_casts < 2)
                Rows.Add(new Row { Fail = "At least 2 alternative casts are required" });
            if (section_types == 0)
                Rows.Add(new Row { Fail = "At least one Section Type is required" });
            FinishLoad(true);
        }
    }
}
