﻿using CarmenUI.Converters;
using CarmenUI.ViewModels;
using Microsoft.EntityFrameworkCore;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using A = Carmen.ShowModel.Applicants;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using CarmenUI.Windows;
using Carmen.CastingEngine;
using Carmen.CastingEngine.Heuristic;
using Carmen.ShowModel.Criterias;
using Carmen.CastingEngine.SAT;
using Carmen.CastingEngine.Selection;
using Carmen.CastingEngine.Dummy;
using Carmen.CastingEngine.Analysis;

namespace CarmenUI.Pages
{
    /// <summary>
    /// Interaction logic for SelectCast.xaml
    /// </summary>
    public partial class SelectCast : SubPage
    {
        private CollectionViewSource selectedApplicantsViewSource;
        private CollectionViewSource allApplicantsViewSource;
        private CollectionViewSource castGroupsViewSource;
        private CollectionViewSource tagsViewSource;
        private CollectionViewSource alternativeCastsViewSource;
        private CollectionViewSource castNumbersViewSource;

        private Criteria[]? _criterias;
        private ISelectionEngine? _engine;

        private Criteria[] criterias => _criterias
            ?? throw new ApplicationException($"Tried to used {nameof(criterias)} before it was loaded.");

        private ISelectionEngine engine => _engine
            ?? throw new ApplicationException($"Tried to used {nameof(engine)} before it was loaded.");

        public SelectCast(DbContextOptions<ShowContext> context_options) : base(context_options)
        {
            InitializeComponent();
            castNumbersViewSource = (CollectionViewSource)FindResource(nameof(castNumbersViewSource));
            castGroupsViewSource = (CollectionViewSource)FindResource(nameof(castGroupsViewSource));
            castGroupsViewSource.SortDescriptions.Add(StandardSort.For<CastGroup>());
            tagsViewSource = (CollectionViewSource)FindResource(nameof(tagsViewSource));
            tagsViewSource.SortDescriptions.Add(StandardSort.For<Tag>());
            alternativeCastsViewSource = (CollectionViewSource)FindResource(nameof(alternativeCastsViewSource));
            alternativeCastsViewSource.SortDescriptions.Add(StandardSort.For<AlternativeCast>());
            selectedApplicantsViewSource = (CollectionViewSource)FindResource(nameof(selectedApplicantsViewSource));
            allApplicantsViewSource = (CollectionViewSource)FindResource(nameof(allApplicantsViewSource));
            foreach (var sd in Properties.Settings.Default.FullNameFormat.ToSortDescriptions())
            {
                allApplicantsViewSource.SortDescriptions.Add(sd);
                selectedApplicantsViewSource.SortDescriptions.Add(sd);
            }
            castStatusCombo.SelectedIndex = 0; // must be here because it triggers event below
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            using var loading = new LoadingOverlay(this).AsSegment(nameof(SelectCast));
            AlternativeCast[] alternative_casts;
            using (loading.Segment(nameof(ShowContext.AlternativeCasts), "Alternative casts"))
                alternative_casts = await context.AlternativeCasts.ToArrayAsync();
            alternativeCastsViewSource.Source = context.AlternativeCasts.Local.ToObservableCollection();
            alternativeCastsViewSource.SortDescriptions.Add(StandardSort.For<AlternativeCast>());
            using (loading.Segment(nameof(ShowContext.CastGroups) + nameof(CastGroup.Members) + nameof(CastGroup.Requirements), "Cast groups"))
                await context.CastGroups.Include(cg => cg.Members).Include(cg => cg.Requirements).LoadAsync();
            castGroupsViewSource.Source = context.CastGroups.Local.ToObservableCollection();
            using (loading.Segment(nameof(ShowContext.Tags) + nameof(A.Tag.Members) + nameof(A.Tag.Requirements), "Tags"))
                await context.Tags.Include(t => t.Members).Include(t => t.Requirements).LoadAsync();
            tagsViewSource.Source = context.Tags.Local.ToObservableCollection();
            using (loading.Segment(nameof(ShowContext.Applicants), "Applicants"))
                await context.Applicants.LoadAsync();
            castNumbersViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Applicant.CastNumber)));
            allApplicantsViewSource.Source = castNumbersViewSource.Source = context.Applicants.Local.ToObservableCollection();
            TriggerCastNumbersRefresh();
            using (loading.Segment(nameof(ShowContext.Requirements), "Requirements"))
                await context.Requirements.LoadAsync();
            using (loading.Segment(nameof(ShowContext.Criterias), "Criteria"))
                _criterias = await context.Criterias.ToArrayAsync();
            using (loading.Segment(nameof(ISelectionEngine), "Selection engine"))
            {
                var show_root = context.ShowRoot;
                IApplicantEngine applicant_engine = ParseApplicantEngine() switch
                {
                    nameof(DummyApplicantEngine) => new DummyApplicantEngine(),
                    nameof(WeightedSumEngine) => new WeightedSumEngine(criterias),
                    _ => throw new ArgumentException($"Applicant engine not handled: {ParseApplicantEngine()}")
                };
                _engine = ParseSelectionEngine() switch
                {
                    nameof(DummySelectionEngine) => new DummySelectionEngine(applicant_engine, alternative_casts, show_root.CastNumberOrderBy, show_root.CastNumberOrderDirection),
                    nameof(HeuristicSelectionEngine) => new HeuristicSelectionEngine(applicant_engine, alternative_casts, show_root.CastNumberOrderBy, show_root.CastNumberOrderDirection),
                    nameof(ChunkedPairsSatEngine) => new ChunkedPairsSatEngine(applicant_engine, alternative_casts, show_root.CastNumberOrderBy, show_root.CastNumberOrderDirection, criterias),
                    _ => throw new ArgumentException($"Allocation engine not handled: {ParseSelectionEngine()}")
                };
            }
        }

        private void TriggerCastNumbersRefresh()
        {
            castNumbersViewSource.View.SortDescriptions.Add(new(nameof(Applicant.CastNumber), ListSortDirection.Ascending));
            castNumbersViewSource.View.SortDescriptions.Add(new(nameof(Applicant.AlternativeCast), ListSortDirection.Ascending));
            castNumbersViewSource.View.Filter = a => ((Applicant)a).CastNumber != null;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => CancelChangesAndReturn();

        private void SaveButton_Click(object sender, RoutedEventArgs e)
            => SaveChangesAndReturn();

        private void selectCastButton_Click(object sender, RoutedEventArgs e)
        {
            //LATER handle exceptions on all engine calls
            using var processing = new LoadingOverlay(this).AsSegment(nameof(selectCastButton_Click), "Processing...");
            using (processing.Segment(nameof(ISelectionEngine.SelectCastGroups), "Selecting applicants"))
                engine.SelectCastGroups(context.Applicants.Local, context.CastGroups.Local);
            using (processing.Segment(nameof(ISelectionEngine.BalanceAlternativeCasts), "Balancing alternating casts"))
                engine.BalanceAlternativeCasts(context.Applicants.Local, Enumerable.Empty<SameCastSet>());
            using (processing.Segment(nameof(ISelectionEngine.AllocateCastNumbers), "Allocating cast numbers"))
                engine.AllocateCastNumbers(context.Applicants.Local);
            using (processing.Segment(nameof(ISelectionEngine.ApplyTags), "Applying tags"))
                engine.ApplyTags(context.Applicants.Local, context.Tags.Local);
            TriggerCastNumbersRefresh();
            //LATER remove test message
#if DEBUG
            var alternative_casts = context.AlternativeCasts.Local.ToArray();
            string msg = "";
            if (engine is ChunkedPairsSatEngine chunky)
            {
                var r = chunky.Results.Last();
                if (r.Solved)
                    msg += $"Balanced casts by solving a {r.MaxLiterals}-SAT problem with {r.Variables} variables and {r.Clauses} clauses."
                        + $"\nIt took {chunky.Results.Count} chunking attempts with a final chunk size of {r.ChunkSize}";
                else
                    msg += $"Failed to solve a {r.MaxLiterals}-SAT problem with {r.Variables} variables and {r.Clauses} clauses."
                        + $"\nAfter {chunky.Results.Count} chunking attempts, and a final chunk size of {r.ChunkSize}, it was still unsolvable.";
                msg += "\n\n";
            }
            var marks = criterias.Where(c => c.Primary).Select(c => alternative_casts.Select(ac => ac.Members.Select(a => a.MarkFor(c)).ToArray()).Select(m => (m, MarkDistribution.Analyse(m))).ToArray()).ToArray();
            msg += "The resulting casts have the follow distributions by primary criteria:\n";
            foreach (var criteria in marks)
            {
                foreach (var ac in criteria)
                {
                    msg += $"\n{ac.Item2}";
                    msg += $"\n[{string.Join(",", ac.Item1.OrderByDescending(m => m))}]";
                }
                msg += "\n";
            }
            MessageBox.Show(msg);
#endif
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedApplicantsViewSource.Source is IList list)
                AddToList(list, availableList.SelectedItems.Cast<Applicant>());
            ConfigureAllApplicantsFiltering();
        }

        private void addAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedApplicantsViewSource.Source is IList list)
                AddToList(list, availableList.Items.Cast<Applicant>());
            ConfigureAllApplicantsFiltering();
        }

        private void AddToList(IList list, IEnumerable<Applicant> applicants)
        {
            foreach (var applicant in applicants)
            {
                if (!list.Contains(applicant))
                    list.Add(applicant);
                if (selectionList.SelectedItem is CastGroup cast_group)
                    applicant.CastGroup = cast_group;
                else if (selectionList.SelectedItem is Tag tag)
                    if (!applicant.Tags.Contains(tag))
                        applicant.Tags.Add(tag);
                //LATER alternative cast needs to be handled here
            }
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedApplicantsViewSource.Source is IList list)
                RemoveFromList(list, selectedList.SelectedItems.Cast<Applicant>().ToArray());
            ConfigureAllApplicantsFiltering();
        }

        private void removeAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedApplicantsViewSource.Source is IList list)
                RemoveFromList(list, selectedList.Items.OfType<Applicant>().ToArray());
            ConfigureAllApplicantsFiltering();
        }

        private void RemoveFromList(IList list, Applicant[] applicants)
        {
            if (applicants.Length == 0)
                return;
            if (selectionList.SelectedItem is CastGroup cast_group)
            {
                string prefix, alt_cast;
                if (applicants.Length == 1)
                {
                    prefix = $"Removing '{applicants[0].FirstName} {applicants[0].LastName}' from '{cast_group.Name}' will also";
                    alt_cast = $"the '{applicants[0].AlternativeCast?.Name}' cast";
                }
                else
                {
                    prefix = $"Removing {applicants.Length} applicants from '{cast_group.Name}' will also";
                    alt_cast = "their alternative casts";
                }
                var has_roles = applicants.Any(a => a.Roles.Any());
                var has_alternative_cast = cast_group.AlternateCasts && applicants.Any(a => a.AlternativeCast != null);
                var confirmed = (has_roles, has_alternative_cast) switch
                {
                    (true, true) => Confirm($"{prefix} remove them from {alt_cast} and uncast them from any roles allocated to them. Do you want to continue?"),
                    (true, false) => Confirm($"{prefix} uncast them from any roles allocated to them. Do you want to continue?"),
                    (false, true) => Confirm($"{prefix} remove them from {alt_cast}. Do you want to continue?"),
                    _ => true // (false, false)
                };
                if (!confirmed)
                    return;
            }
            foreach (var applicant in applicants)
            {
                list.Remove(applicant);
                if (selectionList.SelectedItem is CastGroup)
                    applicant.CastGroup = null;
                else if (selectionList.SelectedItem is Tag tag)
                    applicant.Tags.Remove(tag);
                //LATER alternative cast needs to be handled here
            }
        }

        private void availableList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => addButton_Click(sender, e);

        private void selectedList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => removeButton_Click(sender, e);

        private void castStatusCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ConfigureAllApplicantsFiltering();

        private void selectionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectionList.SelectedItem is CastGroup cast_group)
            {
                numbersPanel.Visibility = Visibility.Collapsed;
                selectedApplicantsViewSource.Source = cast_group.Members;
                selectionPanel.Visibility = Visibility.Visible;
                castStatusNoun.Text = "applicants";
                ConfigureAllApplicantsFiltering();
            }
            else if (selectionList.SelectedItem is Tag tag)
            {
                numbersPanel.Visibility = Visibility.Collapsed;
                selectedApplicantsViewSource.Source = tag.Members;
                selectionPanel.Visibility = Visibility.Visible;
                castStatusNoun.Text = "cast members";
                ConfigureAllApplicantsFiltering();
            }
            else if (selectionList.SelectedItem != null) // Cast Numbers
            {
                TriggerCastNumbersRefresh(); //TODO (TAGS) this isn't picking up changed tags for tag icons
                selectionPanel.Visibility = Visibility.Collapsed;
                numbersPanel.Visibility = Visibility.Visible;
            }
            else
            {
                selectionPanel.Visibility = Visibility.Collapsed;
                numbersPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void ConfigureAllApplicantsFiltering()
        {
            if (allApplicantsViewSource.View is CollectionView view
                && selectedApplicantsViewSource.Source is ObservableCollection<Applicant> selected_applicants)
            {
                view.Filter = (selectionList.SelectedItem, castStatusCombo.SelectedItem) switch
                {
                    (CastGroup, CastStatus.Available) => o => o is Applicant a && !selected_applicants.Contains(a) && a.CastGroup == null,
                    (CastGroup cg, CastStatus.Eligible) => o => o is Applicant a && !selected_applicants.Contains(a) && cg.Requirements.All(r => r.IsSatisfiedBy(a)),
                    (A.Tag, CastStatus.Available) => o => o is Applicant a && a.IsAccepted && !selected_applicants.Contains(a),
                    (Tag t, CastStatus.Eligible) => o => o is Applicant a && a.IsAccepted && !selected_applicants.Contains(a) && t.Requirements.All(r => r.IsSatisfiedBy(a)),
                    _ => null
                };
            }
        }
    }

    public enum CastStatus
    {
        Available,
        Eligible
    }
}
