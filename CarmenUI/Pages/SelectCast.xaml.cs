using CarmenUI.Converters;
using CarmenUI.ViewModels;
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
using Carmen.ShowModel.Criterias;
using Carmen.CastingEngine.Selection;
using Carmen.CastingEngine.Audition;

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
        private Criteria[] criterias => _criterias
            ?? throw new ApplicationException($"Tried to used {nameof(criterias)} before it was loaded.");

        private Applicant[]? _applicants;
        private Applicant[] applicants => _applicants
            ?? throw new ApplicationException($"Tried to used {nameof(applicants)} before it was loaded.");

        private CastGroup[]? _castGroups;
        private CastGroup[] castGroups => _castGroups
            ?? throw new ApplicationException($"Tried to used {nameof(castGroups)} before it was loaded.");

        private Tag[]? _tags;
        private Tag[] tags => _tags
            ?? throw new ApplicationException($"Tried to used {nameof(tags)} before it was loaded.");

        private ISelectionEngine? _engine;
        private ISelectionEngine engine => _engine
            ?? throw new ApplicationException($"Tried to used {nameof(engine)} before it was loaded.");

        public SelectCast(RecentShow connection) : base(connection)
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
            using (loading.Segment(nameof(ShowContext.Criterias), "Criteria"))
                _criterias = await context.Criterias.ToArrayAsync();
            using (loading.Segment(nameof(ShowContext.Requirements), "Requirements"))
                await context.Requirements.LoadAsync();
            using (loading.Segment(nameof(ShowContext.AlternativeCasts) + nameof(AlternativeCast.Members), "Alternative casts"))
            {
                alternative_casts = await context.AlternativeCasts.Include(ac => ac.Members).ToArrayAsync();
                alternativeCastsViewSource.Source = context.AlternativeCasts.Local.ToObservableCollection();
                alternativeCastsViewSource.SortDescriptions.Add(StandardSort.For<AlternativeCast>());
            }
            using (loading.Segment(nameof(ShowContext.CastGroups) + nameof(CastGroup.Members) + nameof(CastGroup.Requirements), "Cast groups"))
                _castGroups = await context.CastGroups.Include(cg => cg.Members).Include(cg => cg.Requirements).ToArrayAsync();
            castGroupsViewSource.Source = context.CastGroups.Local.ToObservableCollection();
            using (loading.Segment(nameof(ShowContext.SameCastSets), "Same cast sets"))
                await context.SameCastSets.LoadAsync();
            sameCastSetsViewSource.Source = context.SameCastSets.Local.ToObservableCollection();
            using (loading.Segment(nameof(ShowContext.Tags) + nameof(A.Tag.Members) + nameof(A.Tag.Requirements), "Tags"))
                _tags = await context.Tags.Include(t => t.Members).Include(t => t.Requirements).ToArrayAsync();
            tagsViewSource.Source = context.Tags.Local.ToObservableCollection();
            using (loading.Segment(nameof(ShowContext.Applicants) + nameof(Applicant.SameCastSet), "Applicants"))
                _applicants = await context.Applicants.Include(a => a.SameCastSet).ToArrayAsync();
            using (loading.Segment(nameof(TriggerCastNumbersRefresh) + nameof(castNumbersViewSource), "Cast numbers"))
            {
                castNumbersViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Applicant.CastNumber)));
                allApplicantsViewSource.Source = castNumbersViewSource.Source = castNumberMissingViewSource.Source
                    = context.Applicants.Local.ToObservableCollection();
                TriggerCastNumbersRefresh();
            }
            using (loading.Segment(nameof(ISelectionEngine), "Selection engine"))
            {
                var show_root = context.ShowRoot;
                IAuditionEngine audition_engine = ParseAuditionEngine() switch
                {
                    nameof(NeuralAuditionEngine) => new NeuralAuditionEngine(criterias, NeuralEngineConfirm),
                    nameof(WeightedSumEngine) => new WeightedSumEngine(criterias),
                    _ => throw new ArgumentException($"Audition engine not handled: {ParseAuditionEngine()}")
                };
                applicantDescription.AuditionEngine = audition_engine;
                _engine = ParseSelectionEngine() switch
                {
                    nameof(HeuristicSelectionEngine) => new HeuristicSelectionEngine(audition_engine, alternative_casts, show_root.CastNumberOrderBy, show_root.CastNumberOrderDirection),
                    nameof(ChunkedPairsSatEngine) => new ChunkedPairsSatEngine(audition_engine, alternative_casts, show_root.CastNumberOrderBy, show_root.CastNumberOrderDirection, criterias),
                    nameof(TopPairsSatEngine) => new TopPairsSatEngine(audition_engine, alternative_casts, show_root.CastNumberOrderBy, show_root.CastNumberOrderDirection, criterias),
                    nameof(ThreesACrowdSatEngine) => new ThreesACrowdSatEngine(audition_engine, alternative_casts, show_root.CastNumberOrderBy, show_root.CastNumberOrderDirection, criterias),
                    nameof(HybridPairsSatEngine) => new HybridPairsSatEngine(audition_engine, alternative_casts, show_root.CastNumberOrderBy, show_root.CastNumberOrderDirection, criterias),
                    nameof(RankDifferenceSatEngine) => new RankDifferenceSatEngine(audition_engine, alternative_casts, show_root.CastNumberOrderBy, show_root.CastNumberOrderDirection, criterias),
                    nameof(BestPairsSatEngine) => new BestPairsSatEngine(audition_engine, alternative_casts, show_root.CastNumberOrderBy, show_root.CastNumberOrderDirection, criterias),
                    _ => throw new ArgumentException($"Allocation engine not handled: {ParseSelectionEngine()}")
                };
            }
        }

        private void TriggerCastNumbersRefresh()
        {
            castNumbersViewSource.View.SortDescriptions.Add(new(nameof(Applicant.CastNumber), ListSortDirection.Ascending));
            castNumbersViewSource.View.SortDescriptions.Add(new(nameof(Applicant.AlternativeCast), ListSortDirection.Ascending));
            castNumbersViewSource.View.Filter = a => ((Applicant)a).CastNumber != null;
            castNumberMissingViewSource.View.Filter = a => ((Applicant)a).CastNumber == null && ((Applicant)a).CastGroup != null;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => CancelChangesAndReturn();

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
            => await SaveChangesAndReturn();

        protected override async Task<bool> PreSaveChecks()
        {
            var inconsistent_applicants = applicants
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
                    using (var processing = new LoadingOverlay(this).AsSegment(nameof(SelectCast) + nameof(PreSaveChecks), "Processing..."))
                    {
                        using (processing.Segment(nameof(ISelectionEngine.BalanceAlternativeCasts), "Balancing alternating casts"))
                            await Task.Run(() => engine.BalanceAlternativeCasts(applicants, context.SameCastSets.Local));
                        using (processing.Segment(nameof(ISelectionEngine.AllocateCastNumbers), "Allocating cast numbers"))
                            await Task.Run(() => engine.AllocateCastNumbers(applicants));
                    }
                    var updated_inconsistent_applicants = applicants
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
            using (new LoadingOverlay(this).AsSegment(nameof(IAuditionEngine) + nameof(IAuditionEngine.UserSelectedCast), "Learning...", "Cast selected by the user"))
                await Task.Run(() => engine.AuditionEngine.UserSelectedCast(applicants.Where(a => a.IsAccepted), applicants.Where(a => !a.IsAccepted)));
            return true;
        }

        private async void selectCastButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO handle exceptions on all engine calls
            using var processing = new LoadingOverlay(this).AsSegment(nameof(selectCastButton_Click), "Processing...");
            using (processing.Segment(nameof(ISelectionEngine.SelectCastGroups), "Selecting applicants"))
                await Task.Run(() => engine.SelectCastGroups(applicants, castGroups));
            using (processing.Segment(nameof(ISelectionEngine.BalanceAlternativeCasts), "Balancing alternating casts"))
                await Task.Run(() => engine.BalanceAlternativeCasts(applicants, context.SameCastSets.Local));
            using (processing.Segment(nameof(ISelectionEngine.AllocateCastNumbers), "Allocating cast numbers"))
                await Task.Run(() => engine.AllocateCastNumbers(applicants));
            using (processing.Segment(nameof(ISelectionEngine.ApplyTags), "Applying tags"))
                await Task.Run(() => engine.ApplyTags(applicants, tags));
            using (processing.Segment(nameof(RefreshMainPanel), "Refreshing cast lists"))
                RefreshMainPanel();
            //TODO remove test message
#if DEBUG
            var alternative_casts = context.AlternativeCasts.Local.ToArray();
            string msg = "";
            if (engine is PairsSatEngine chunky && chunky.Results.LastOrDefault() is PairsSatEngine.Result r)
            {
                var chunk_size = $"{r.ChunkSize}";
                if (r.MaxChunks != int.MaxValue)
                    chunk_size += $" (max {r.MaxChunks})";
                if (r.Solved)
                    msg += $"Balanced casts by solving a {r.MaxLiterals}-SAT problem with {r.Variables} variables and {r.Clauses} clauses."
                        + $"\nIt took {chunky.Results.Count} chunking attempts with a final chunk size of {chunk_size}";
                else
                    msg += $"Failed to solve a {r.MaxLiterals}-SAT problem with {r.Variables} variables and {r.Clauses} clauses."
                        + $"\nAfter {chunky.Results.Count} chunking attempts, and a final chunk size of {chunk_size}, it was still unsolvable.";
                msg += "\n\n";
            }
            msg += "The resulting casts have the follow distributions:\n";
            foreach (var cg in castGroups)
            {
                if (cg.Members.Count == 0 || !cg.AlternateCasts)
                    continue;
                foreach (var c in criterias.Where(c => c.Primary))
                {
                    msg += $"\n{c.Name} ({cg.Abbreviation})";
                    foreach (var ac in alternative_casts)
                    {
                        var marks = ac.Members.Where(a => a.CastGroup == cg).Select(a => a.MarkFor(c)).ToArray();
                        var dist = MarkDistribution.Analyse(marks);
                        msg += $"\n{dist}";
                        msg += $"\n[{string.Join(",", marks.OrderByDescending(m => m))}]";
                    }
                    msg += "\n";
                }
            }
            ((App)App.Current).CreateLog("Cast", msg);
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

        private async void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedApplicantsViewSource.Source is IList list)
                await RemoveFromList(list, selectedList.SelectedItems.Cast<Applicant>().ToArray());
            ConfigureAllApplicantsFiltering();
        }

        private async void removeAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedApplicantsViewSource.Source is IList list)
                await RemoveFromList(list, selectedList.Items.OfType<Applicant>().ToArray());
            ConfigureAllApplicantsFiltering();
        }

        private async Task RemoveFromList(IList list, Applicant[] applicants)
        {
            LoadingOverlay? overlay = null;
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
                if (actions.Any())
                {
                    if (!Confirm($"Removing {description} from '{cast_group.Name}' will also {actions.JoinWithCommas()}. Do you want to continue?"))
                        return;
                    overlay = new LoadingOverlay(this) { MainText = "Removing...", SubText = "Applicants from cast group" };
                    await context.Roles.Include(r => r.Cast).LoadAsync();
                }
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
                {
                    tag.Members.Remove(applicant);
                    applicant.Tags.Remove(tag);
                }
                else
                    throw new NotImplementedException($"Selection list type not handled: {selectionList.SelectedItem.GetType().Name}");
            }
            if (overlay != null)
                overlay.Dispose();
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
            => RefreshMainPanel();

        private void RefreshMainPanel()
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

        private async void DetectSiblings_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<SameCastSet> new_same_cast_sets;
            using (new LoadingOverlay(this).AsSegment(nameof(SelectCast) + nameof(PreSaveChecks), "Processing...", "Detecting siblings"))
                new_same_cast_sets = await Task.Run(() =>
                {
                    engine.DetectFamilies(applicants, out var new_same_cast_sets);
                    return new_same_cast_sets;
                });
            var list = (IList)sameCastSetsViewSource.Source;
            foreach (var new_same_cast_set in new_same_cast_sets)
                list.Add(new_same_cast_set);
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

        private void castStatusCombo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var new_index = castStatusCombo.SelectedIndex + 1;
            if (new_index >= castStatusCombo.Items.Count)
                new_index = 0;
            castStatusCombo.SelectedIndex = new_index;
        }
    }

    public enum CastStatus
    {
        Available,
        Eligible
    }
}
