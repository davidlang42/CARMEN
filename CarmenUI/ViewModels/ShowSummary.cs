using Carmen.ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;

namespace CarmenUI.ViewModels
{
    public class ShowSummary : Summary
    {
        string defaultShowName;

        public static readonly DependencyProperty HeadingProperty = DependencyProperty.Register(
            nameof(Heading), typeof(string), typeof(ShowSummary), new PropertyMetadata(""));

        public string Heading
        {
            get => (string)GetValue(HeadingProperty);
            set => SetValue(HeadingProperty, value);
        }

        public static readonly DependencyProperty SubHeadingProperty = DependencyProperty.Register(
            nameof(SubHeading), typeof(string), typeof(ShowSummary), new PropertyMetadata(""));

        public string SubHeading
        {
            get => (string)GetValue(SubHeadingProperty);
            set => SetValue(SubHeadingProperty, value);
        }

        public static readonly DependencyProperty LogoImageProperty = DependencyProperty.Register(
            nameof(LogoImage), typeof(byte[]), typeof(ShowSummary), new PropertyMetadata(null));

        public byte[]? LogoImage
        {
            get => (byte[]?)GetValue(LogoImageProperty);
            set => SetValue(LogoImageProperty, value);
        }

        public ShowSummary(string default_show_name)
        {
            defaultShowName = default_show_name;
        }

        public override async Task LoadAsync(ShowContext c, CancellationToken cancel)
        {
            StartLoad();
            var show_root = await c.Nodes.OfType<ShowRoot>().Include(sr => sr.Logo).SingleAsync();
            Heading = show_root.Name;
            SubHeading = show_root.ShowDate switch {
                DateTime d when d >= DateTime.Now => $"opening {show_root.ShowDate.Value.Day.ToOrdinal()} {show_root.ShowDate.Value:MMMM yyyy}",
                DateTime d => $"opened {show_root.ShowDate.Value.Day.ToOrdinal()} {show_root.ShowDate.Value:MMMM yyyy}",
                _ => ""
            };
            LogoImage = show_root.Logo?.ImageData;
            if (string.IsNullOrEmpty(show_root.Name))
                Rows.Add(new Row { Fail = "Show name is required" });
            if (!show_root.ShowDate.HasValue)
                Rows.Add(new Row { Fail = "Show date is required" });
            var criterias = await c.Criterias.ToArrayAsync();
            Rows.Add(CountRow(criterias.Length, 1, "Audition Criteria", true));
            var cast_groups = await c.CastGroups.ToArrayAsync();
            Rows.Add(CountRow(cast_groups.Length, 1, "Cast Group"));
            var alternative_casts = await c.AlternativeCasts.ToArrayAsync();
            if (cast_groups.Any(g => g.AlternateCasts))
                Rows.Add(CountRow(alternative_casts.Length, 2, "Alternative Cast"));
            else if (alternative_casts.Length == 0)
                Rows.Add(new Row { Success = "Alternating Casts are disabled" });
            else
                Rows.Add(new Row
                {
                    Success = alternative_casts.Length.Plural("Alternative Cast"),
                    Fail = "(but are not enabled)"
                });
            var tags = await c.Tags.ToArrayAsync();
            Rows.Add(CountRow(tags.Length, 0, "Cast Tag"));
            var section_types = await c.SectionTypes.ToArrayAsync(); // need to load entities rather than count for CheckDefaultShowSettings
            Rows.Add(CountRow(section_types.Length, 1, "Section Type"));
            var requirements = await c.Requirements.ToArrayAsync();
            Rows.Add(CountRow(requirements.Length, 0, "Requirement"));
            FinishLoad(cancel, c.CheckDefaultShowSettings(defaultShowName, false));
        }

        private static Row CountRow(int actual_count, int minimum_count, string single_name, bool same_plural = false)
        {
            var row = new Row();
            if (same_plural)
                row.Success = $"{actual_count} {single_name}";
            else
                row.Success = actual_count.Plural(single_name);
            if (actual_count < minimum_count)
                row.Fail = $"(at least {minimum_count} required)";
            return row;
        }
    }
}
