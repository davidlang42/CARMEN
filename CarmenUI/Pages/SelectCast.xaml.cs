using CarmenUI.Converters;
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
        private CollectionViewSource sameCastSetsViewSource;
        private CollectionViewSource tagsViewSource;
        private CollectionViewSource alternativeCastsViewSource;
        private CollectionViewSource castNumbersViewSource;
        private CollectionViewSource castNumberMissingViewSource;
        private ApplicantDescription applicantDescription;

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
            sameCastSetsViewSource = (CollectionViewSource)FindResource(nameof(sameCastSetsViewSource));
            tagsViewSource = (CollectionViewSource)FindResource(nameof(tagsViewSource));
            tagsViewSource.SortDescriptions.Add(StandardSort.For<Tag>());
            alternativeCastsViewSource = (CollectionViewSource)FindResource(nameof(alternativeCastsViewSource));
            alternativeCastsViewSource.SortDescriptions.Add(StandardSort.For<AlternativeCast>());
            selectedApplicantsViewSource = (CollectionViewSource)FindResource(nameof(selectedApplicantsViewSource));
            allApplicantsViewSource = (CollectionViewSource)FindResource(nameof(allApplicantsViewSource));
            castNumberMissingViewSource = (CollectionViewSource)FindResource(nameof(castNumberMissingViewSource));
            applicantDescription = (ApplicantDescription)FindResource(nameof(applicantDescription));
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
            using (loading.Segment(nameof(ShowContext.SameCastSets), "Same cast sets"))
                await context.SameCastSets.LoadAsync();
            sameCastSetsViewSource.Source = context.SameCastSets.Local.ToObservableCollection();
            using (loading.Segment(nameof(ShowContext.Tags) + nameof(A.Tag.Members) + nameof(A.Tag.Requirements), "Tags"))
                await context.Tags.Include(t => t.Members).Include(t => t.Requirements).LoadAsync();
            tagsViewSource.Source = context.Tags.Local.ToObservableCollection();
            using (loading.Segment(nameof(ShowContext.Applicants), "Applicants"))
                await context.Applicants.LoadAsync();
            castNumbersViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Applicant.CastNumber)));
            allApplicantsViewSource.Source = castNumbersViewSource.Source = castNumberMissingViewSource.Source
                = context.Applicants.Local.ToObservableCollection();
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
                applicantDescription.ApplicantEngine = applicant_engine;
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
            //LATER this is taking considerable time to update, add loading overlay if this is unavoidable (think about where this is called)
            castNumbersViewSource.View.SortDescriptions.Add(new(nameof(Applicant.CastNumber), ListSortDirection.Ascending));
            castNumbersViewSource.View.SortDescriptions.Add(new(nameof(Applicant.AlternativeCast), ListSortDirection.Ascending));
            castNumbersViewSource.View.Filter = a => ((Applicant)a).CastNumber != null;
            castNumberMissingViewSource.View.Filter = a => ((Applicant)a).CastNumber == null && ((Applicant)a).CastGroup != null;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => CancelChangesAndReturn();

        private void SaveButton_Click(object sender, RoutedEventArgs e)
            => SaveChangesAndReturn();

        protected override bool PreSaveChecks()
        {
            var inconsistent_applicants = context.Applicants.Local
                .Where(a => a.CastGroup is CastGroup cg && (a.CastNumber == null || cg.AlternateCasts != (a.AlternativeCast != null)))
                .ToArray();
            if (inconsistent_applicants.Any())
            {
                string msg;
                if (inconsistent_applicants.Length == 1)
                    msg = $"'{inconsistent_applicants[0].FirstName} {inconsistent_applicants[0].LastName}' has";
                else
                    msg = $"{inconsistent_applicants.Length} applicants have";
                msg += " accepted into the cast but don't have a Cast Number or Alternative Cast set. Would you like to auto-complete them?";
                msg += "\nClick 'No' to remove them from the cast instead, or 'Cancel' to continue editing without saving.";
                var response = MessageBox.Show(msg, WindowTitle, MessageBoxButton.YesNoCancel);
                if (response == MessageBoxResult.Cancel)
                    return false;
                else if (response == MessageBoxResult.Yes)
                {
                    // auto-complete cast numbers & alternative casts
                    using (var processing = new LoadingOverlay(this).AsSegment(nameof(PreSaveChecks), "Processing..."))
                    {
                        using (processing.Segment(nameof(ISelectionEngine.BalanceAlternativeCasts), "Balancing alternating casts"))
                            engine.BalanceAlternativeCasts(context.Applicants.Local, Enumerable.Empty<SameCastSet>());
                        using (processing.Segment(nameof(ISelectionEngine.AllocateCastNumbers), "Allocating cast numbers"))
                            engine.AllocateCastNumbers(context.Applicants.Local);
                    }
                    var updated_inconsistent_applicants = context.Applicants.Local
                        .Where(a => a.CastGroup is CastGroup cg && (a.CastNumber == null || cg.AlternateCasts != (a.AlternativeCast != null)));
                    if (updated_inconsistent_applicants.Any())
                    {
                        MessageBox.Show("Failed to auto-complete cast numbers. Changes not saved.", WindowTitle);
                        return false;
                    }
                }
                else
                {
                    // remove cast group
                    foreach (var applicant in inconsistent_applicants)
                        applicant.CastGroup = null;
                }
            }
            return true;
        }

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
            MessageBox.Show(msg, "DEBUG");
#endif
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedApplicantsViewSource.Source is IList list)
                AddToList(list, availableList.SelectedItems.Cast<Applicant>().ToArray());
            ConfigureAllApplicantsFiltering();
        }

        private void addAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedApplicantsViewSource.Source is IList list)
                AddToList(list, availableList.Items.Cast<Applicant>().ToArray());
            ConfigureAllApplicantsFiltering();
        }

        private void AddToList(IList list, Applicant[] applicants)
        {
            string description, alt_cast, cast_group_name;
            if (applicants.Length == 1)
            {
                description = $"'{applicants[0].FirstName} {applicants[0].LastName}'";
                alt_cast = $"the '{applicants[0].AlternativeCast?.Name}' cast";
                cast_group_name = $"'{applicants[0].CastGroup?.Name}'";
            }
            else
            {
                description = $"{applicants.Length} applicants";
                alt_cast = "their existing alternative casts";
                cast_group_name = "their existing cast groups";
            }
            if (selectionList.SelectedItem is CastGroup cg)
            {
                var actions = new List<string>();
                if (applicants.Any(a => a.CastGroup != null))
                    actions.Add($"remove them from {cast_group_name}");
                if (applicants.Any(a => a.AlternativeCast != null))
                {
                    actions.Add($"remove them from {alt_cast}");
                    actions.Add($"clear their cast number");
                }
                if (actions.Any() && !Confirm($"Adding {description} to '{cg.Name}' will also {actions.JoinWithCommas()}. Do you want to continue?"))
                    return;
            }
            else if (selectionList.SelectedItem is AlternativeCast ac)
            {
                var actions = new List<string>();
                if (applicants.Any(a => a.AlternativeCast != null))
                {
                    actions.Add($"remove them from {alt_cast}");
                    actions.Add($"clear their cast number");
                }
                var broken_sets = applicants
                    .Select(a => a.SameCastSet).OfType<SameCastSet>().Distinct() // any sets containing these applicants
                    .Where(set => set.VerifyAlternativeCasts(out var common_or_null) // which are currently met
                        && common_or_null is AlternativeCast common_ac && common_ac != ac) // and will be broken by this assignment
                    .ToArray();
                if (broken_sets.Length == 1)
                    actions.Add($"split up the same-cast {broken_sets[0].Description.UnCapitalise()}");
                else if (broken_sets.Length > 1)
                    actions.Add($"split up {broken_sets.Length} same-cast sets.");
                if (actions.Any() && !Confirm($"Adding {description} to '{ac.Name}' will also {actions.JoinWithCommas()}. Do you want to continue?"))
                    return;
            }
            foreach (var applicant in applicants)
            {
                if (!list.Contains(applicant))
                    list.Add(applicant);
                if (selectionList.SelectedItem is CastGroup cast_group)
                    applicant.CastGroup = cast_group;
                else if (selectionList.SelectedItem is AlternativeCast alternative_cast)
                    applicant.AlternativeCast = alternative_cast;
                else if (selectionList.SelectedItem is Tag tag)
                {
                    if (!applicant.Tags.Contains(tag))
                        applicant.Tags.Add(tag);
                }
                else
                    throw new NotImplementedException($"Selection list type not handled: {selectionList.SelectedItem.GetType().Name}");
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
            string description, alt_cast;
            if (applicants.Length == 1)
            {
                description = $"'{applicants[0].FirstName} {applicants[0].LastName}'";
                alt_cast = $"the '{applicants[0].AlternativeCast?.Name}' cast";
            }
            else
            {
                description = $"{applicants.Length} applicants";
                alt_cast = "their alternative casts";
            }
            if (selectionList.SelectedItem is CastGroup cast_group)
            {
                var actions = new List<string>();
                if (cast_group.AlternateCasts && applicants.Any(a => a.AlternativeCast != null))
                {
                    actions.Add($"remove them from {alt_cast}");
                    actions.Add($"clear their cast number");
                }
                if (applicants.Any(a => a.Roles.Any()))
                    actions.Add("uncast them from any roles allocated to them");
                if (applicants.Any(a => a.Tags.Any()))
                    actions.Add("remove any tags they have been applied");
                if (actions.Any() && !Confirm($"Removing {description} from '{cast_group.Name}' will also {actions.JoinWithCommas()}. Do you want to continue?"))
                    return;
            }
            else if (selectionList.SelectedItem is AlternativeCast alternative_cast)
            {
                if (applicants.Any(a => a.CastNumber.HasValue) && !Confirm($"Removing {description} from '{alternative_cast.Name}' will also clear their cast number. Do you want to continue?"))
                    return;
            }
            foreach (var applicant in applicants)
            {
                list.Remove(applicant);
                if (selectionList.SelectedItem is CastGroup)
                    applicant.CastGroup = null;
                else if (selectionList.SelectedItem is AlternativeCast)
                    applicant.AlternativeCast = null;
                else if (selectionList.SelectedItem is Tag tag)
                    applicant.Tags.Remove(tag);
                else
                    throw new NotImplementedException($"Selection list type not handled: {selectionList.SelectedItem.GetType().Name}");
            }
        }

        private void availableList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => addButton_Click(sender, e);

        private void selectedList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => removeButton_Click(sender, e);

        private void castStatusCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ConfigureAllApplicantsFiltering();

        private void addSameCastSetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sameCastSetsList.SelectedItem is SameCastSet set)
                AddToSameCastSet(set, availableSameCastSetList.SelectedItems.Cast<Applicant>().ToArray());
            ConfigureAllApplicantsFiltering();
        }

        private void addAllSameCastSetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sameCastSetsList.SelectedItem is SameCastSet set)
                AddToSameCastSet(set, availableSameCastSetList.Items.Cast<Applicant>().ToArray());
            ConfigureAllApplicantsFiltering();
        }

        private void AddToSameCastSet(SameCastSet set, Applicant[] applicants)
        {
            foreach (var applicant in applicants)
            {
                if (!set.Applicants.Contains(applicant))
                    set.Applicants.Add(applicant);
                applicant.SameCastSet = set;
            }
        }

        private void removeSameCastSetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sameCastSetsList.SelectedItem is SameCastSet set)
                RemoveFromSameCastSet(set, selectedSameCastSetList.SelectedItems.Cast<Applicant>().ToArray());
            ConfigureAllApplicantsFiltering();
        }

        private void removeAllSameCastSetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sameCastSetsList.SelectedItem is SameCastSet set)
                RemoveFromSameCastSet(set, selectedSameCastSetList.Items.Cast<Applicant>().ToArray());
            ConfigureAllApplicantsFiltering();
        }

        private void RemoveFromSameCastSet(SameCastSet set, Applicant[] applicants)
        {
            foreach (var applicant in applicants)
            {
                set.Applicants.Remove(applicant);
                applicant.SameCastSet = null;
            }
        }

        private void availableSameCastSetList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => addSameCastSetButton_Click(sender, e);

        private void selectedSameCastSetList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => removeSameCastSetButton_Click(sender, e);

        private void selectionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectionPanel.Visibility = numbersPanel.Visibility = sameCastSetsPanel.Visibility = Visibility.Collapsed;
            if (selectionList.SelectedItem is CastGroup cast_group)
            {
                selectedApplicantsViewSource.Source = cast_group.Members;
                selectionPanel.Visibility = Visibility.Visible;
                castStatusNoun.Text = "applicants";
            }
            else if (selectionList.SelectedItem is AlternativeCast alternative_cast)
            {
                selectedApplicantsViewSource.Source = alternative_cast.Members;
                selectionPanel.Visibility = Visibility.Visible;
                castStatusNoun.Text = "alternating cast members";
            }
            else if (selectionList.SelectedItem is Tag tag)
            {
                selectedApplicantsViewSource.Source = tag.Members;
                selectionPanel.Visibility = Visibility.Visible;
                castStatusNoun.Text = "cast members";
            }
            else if (selectionList.SelectedItem == finalCastList)
            {
                TriggerCastNumbersRefresh();
                numbersPanel.Visibility = Visibility.Visible;
            }
            else if (selectionList.SelectedItem == keepApplicantsTogether)
                sameCastSetsPanel.Visibility = Visibility.Visible;
            else // no selection (initial state)
                return;
            ConfigureAllApplicantsFiltering();
        }

        private void ConfigureAllApplicantsFiltering()
        {
            if (allApplicantsViewSource.View is CollectionView view)
            {
                var selected_applicants = (ObservableCollection<Applicant>)selectedApplicantsViewSource.Source;
                view.Filter = (selectionList.SelectedItem, castStatusCombo.SelectedItem) switch
                {
                    (CastGroup, CastStatus.Available) => o => o is Applicant a && !selected_applicants.Contains(a) && a.CastGroup == null,
                    (CastGroup cg, CastStatus.Eligible) => o => o is Applicant a && !selected_applicants.Contains(a) && cg.Requirements.All(r => r.IsSatisfiedBy(a)),
                    (AlternativeCast, CastStatus.Available) => o => o is Applicant a && !selected_applicants.Contains(a) && a.CastGroup is CastGroup cg && cg.AlternateCasts && a.AlternativeCast == null,
                    (AlternativeCast ac, CastStatus.Eligible) => o => o is Applicant a && !selected_applicants.Contains(a) && a.CastGroup is CastGroup cg && cg.AlternateCasts,
                    (A.Tag, CastStatus.Available) => o => o is Applicant a && a.IsAccepted && !selected_applicants.Contains(a),
                    (Tag t, CastStatus.Eligible) => o => o is Applicant a && a.IsAccepted && !selected_applicants.Contains(a) && t.Requirements.All(r => r.IsSatisfiedBy(a)),
                    (ListBoxItem lbi, _) when lbi == keepApplicantsTogether => o => o is Applicant a && a.SameCastSet == null,
                    _ => null
                };
            }
        }

        private void DetectSiblings_Click(object sender, RoutedEventArgs e)
        {
            var siblings = context.Applicants.Local
                .Where(a => !string.IsNullOrEmpty(a.LastName))
                .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
                .GroupBy(a => a.LastName)
                .Select(g => g.ToArray())
                .ToArray();
            foreach (var family in siblings.Where(f => f.Length > 1))
            {
                var set = family.Select(a => a.SameCastSet).OfType<SameCastSet>().FirstOrDefault();
                if (set == null)
                {
                    set = new SameCastSet();
                    context.SameCastSets.Add(set);
                }
                foreach (var applicant in family)
                    if (applicant.SameCastSet == null)
                    {
                        applicant.SameCastSet = set;
                        set.Applicants.Add(applicant);
                    }
            }
        }

        private void AddSameCastSet_Click(object sender, RoutedEventArgs e)
        {
            if (sameCastSetsViewSource.Source is IList list)
            {
                var set = new SameCastSet();
                list.Add(set);
                sameCastSetsList.SelectedItem = set;
            }
        }

        private void DeleteSameCastSet_Click(object sender, RoutedEventArgs e)
        {
            if (sameCastSetsList.SelectedItem is SameCastSet set)
            {
                context.DeleteSameCastSet(set);
                ConfigureAllApplicantsFiltering();
            }
        }

        private void sameCastSetsList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                DeleteSameCastSet_Click(sender, e);
        }
    }

    public enum CastStatus
    {
        Available,
        Eligible
    }
}
