using Microsoft.EntityFrameworkCore;
using Carmen.ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Carmen.ShowModel.Requirements;

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
            var show_root = c.ShowRoot;
            Heading = show_root.Name;
            SubHeading = show_root.ShowDate.HasValue ? $"opening {show_root.ShowDate.Value.Day.ToOrdinal()} {show_root.ShowDate.Value:MMMM yyyy}" : "";
            LogoImage = show_root.Logo?.ImageData;
            if (string.IsNullOrEmpty(show_root.Name))
                Rows.Add(new Row { Fail = "Show name is required" });
            if (!show_root.ShowDate.HasValue)
                Rows.Add(new Row { Fail = "Show date is required" });
            await c.Criterias.LoadAsync();
            Rows.Add(CountRow(c.Criterias.Local.Count, 1, "Audition Criteria", true));
            await c.CastGroups.LoadAsync();
            Rows.Add(CountRow(c.CastGroups.Local.Count, 1, "Cast Group"));
            await c.AlternativeCasts.LoadAsync();
            if (c.CastGroups.Local.Any(g => g.AlternateCasts))
                Rows.Add(CountRow(c.AlternativeCasts.Local.Count, 2, "Alternative Cast"));
            else if (c.AlternativeCasts.Local.Count == 0)
                Rows.Add(new Row { Success = "Alternating Casts are disabled" });
            else
                Rows.Add(new Row
                {
                    Success = c.AlternativeCasts.Local.Count.Plural("Alternative Cast"),
                    Fail = "(but are not enabled)"
                });
            await c.Tags.LoadAsync();
            Rows.Add(CountRow(c.Tags.Local.Count, 0, "Cast Tag"));
            await c.SectionTypes.LoadAsync();
            Rows.Add(CountRow(c.SectionTypes.Local.Count, 1, "Section Type"));
            await c.Requirements.LoadAsync();
            Rows.Add(CountRow(c.Requirements.Local.Count, 0, "Requirement"));
            FinishLoad(cancel, c.CheckDefaultShowSettings(defaultShowName, false));
        }
    }
}
