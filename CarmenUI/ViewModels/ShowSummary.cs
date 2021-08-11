using Microsoft.EntityFrameworkCore;
using Carmen.ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CarmenUI.ViewModels
{
    public class ShowSummary : Summary
    {
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

        public override async Task LoadAsync(ShowContext c)
        {
            StartLoad();
            await c.Entry(c.ShowRoot).ReloadAsync();//LATER do I need to force reload entities on invalidate to ensure new values get checked?
            var show_root = c.ShowRoot;
            Heading = show_root.Name;
            SubHeading = show_root.ShowDate.HasValue ? $"opening {show_root.ShowDate.Value:dd MMMM yyyy}" : "";//LATER opening 12th July 2021
            LogoImage = show_root.Logo?.ImageData;
            await c.Criterias.LoadAsync();
            Rows.Add(new Row { Success = $"{c.Criterias.Local.Count} Audition Criteria" });
            await c.CastGroups.LoadAsync();
            Rows.Add(new Row { Success = c.CastGroups.Local.Count.Plural("Cast Group") });
            var any_groups_alternating = c.CastGroups.Local.Any(g => g.AlternateCasts);
            await c.AlternativeCasts.LoadAsync();
            if (any_groups_alternating)
                Rows.Add(new Row { Success = c.AlternativeCasts.Local.Count.Plural("Alternative Cast") });
            else
                Rows.Add(new Row { Success = "Alternating Casts are disabled" });
            await c.Tags.LoadAsync();
            Rows.Add(new Row { Success = c.Tags.Local.Count.Plural("Cast Tag") });
            await c.SectionTypes.LoadAsync();
            Rows.Add(new Row { Success = c.SectionTypes.Local.Count.Plural("Section Type") });
            await c.Requirements.LoadAsync();
            Rows.Add(new Row { Success = c.Requirements.Local.Count.Plural("Requirement") });
            if (c.Criterias.Local.Count == 0)
                Rows.Add(new Row { Fail = "At least one Criteria is required" });
            if (c.CastGroups.Local.Count == 0)
                Rows.Add(new Row { Fail = "At least one Cast Group is required" });
            if (any_groups_alternating && c.AlternativeCasts.Local.Count < 2)
                Rows.Add(new Row { Fail = "At least 2 alternative casts are required" });
            if (c.SectionTypes.Local.Count == 0)
                Rows.Add(new Row { Fail = "At least one Section Type is required" });
            FinishLoad(true);
        }
    }
}
