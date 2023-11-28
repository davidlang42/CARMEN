using Carmen.Desktop.Converters;
using Carmen.Desktop.ViewModels;
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
using Carmen.Desktop.Windows;
using Carmen.ShowModel.Criterias;
using Carmen.CastingEngine.Selection;
using Carmen.CastingEngine.Audition;

namespace Carmen.Desktop.Pages
{
    /// <summary>
    /// Interaction logic for SelectCast.xaml
    /// </summary>
    public partial class SelectCast : SubPage
    {
        private readonly CollectionViewSource selectedApplicantsViewSource;
        private readonly CollectionViewSource allApplicantsViewSource;
        private readonly CollectionViewSource castGroupsViewSource;
        private readonly CollectionViewSource sameCastSetsViewSource;
        private readonly CollectionViewSource tagsViewSource;
        private readonly CollectionViewSource alternativeCastsViewSource;
        private readonly CollectionViewSource castNumbersViewSource;
        private readonly CollectionViewSource castNumberMissingViewSource;
        private readonly ApplicantDescription applicantDescription;
        private readonly SuitabilityCalculator suitabilityCalculator;
        private readonly CheckRequiredCount checkRequiredCount;

        private Criteria[]? _criterias;
        private Criteria[] criterias => _criterias
            ?? throw new ApplicationException($"Tried to used {nameof(criterias)} before it was loaded.");

        private Applicant[]? _applicants;
        private Applicant[] applicants => _applicants
            ?? throw new ApplicationException($"Tried to used {nameof(applicants)} before it was loaded.");

        private AlternativeCast[]? _alternativeCasts;
        private AlternativeCast[] alternativeCasts => _alternativeCasts
            ?? throw new ApplicationException($"Tried to used {nameof(alternativeCasts)} before it was loaded.");

        private CastGroup[]? _castGroups;
        private CastGroup[] castGroups => _castGroups
            ?? throw new ApplicationException($"Tried to used {nameof(castGroups)} before it was loaded.");

        private Tag[]? _tags;
        private Tag[] tags => _tags
            ?? throw new ApplicationException($"Tried to used {nameof(tags)} before it was loaded.");

        private IAuditionEngine? _auditionEngine;
        private IAuditionEngine auditionEngine => _auditionEngine
            ?? throw new ApplicationException($"Tried to used {nameof(auditionEngine)} before it was loaded.");

        private ISelectionEngine? _selectionEngine;
        private ISelectionEngine selectionEngine => _selectionEngine
            ?? throw new ApplicationException($"Tried to used {nameof(selectionEngine)} before it was loaded.");

        private CastList? _castList;
        private CastList castList => _castList
            ?? throw new ApplicationException($"Tried to used {nameof(castList)} before it was loaded.");

        private readonly SuitabilityComparer suitabilityComparer;

        public SelectCast(RecentShow connection) : base(connection)
        {
            InitializeComponent();
            castNumbersViewSource = (CollectionViewSource)FindResource(nameof(castNumbersViewSource));
            castNumbersViewSource.SortDescriptions.Add(new(nameof(CastNumber.Number), ListSortDirection.Ascending));
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
            suitabilityCalculator = (SuitabilityCalculator)FindResource(nameof(suitabilityCalculator));
            checkRequiredCount = (CheckRequiredCount)FindResource(nameof(checkRequiredCount));
            SortByName(castNumberMissingViewSource);
            suitabilityComparer = new SuitabilityComparer(suitabilityCalculator);
            castStatusCombo.SelectedIndex = 0; // must be here because it triggers event below
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            using var loading = new LoadingOverlay(this).AsSegment(nameof(SelectCast));
            using (loading.Segment(nameof(ShowContext.Criterias), "Criteria"))
                _criterias = await context.Criterias.ToArrayAsync();
            using (loading.Segment(nameof(ShowContext.Requirements), "Requirements"))
                await context.Requirements.LoadAsync();
            using (loading.Segment(nameof(ShowContext.AlternativeCasts) + nameof(AlternativeCast.Members), "Alternative casts"))
            {
                _alternativeCasts = await context.AlternativeCasts.Include(ac => ac.Members).InNameOrder().ToArrayAsync();
                checkRequiredCount.AlternativeCasts = alternativeCasts.Length;
                alternativeCastsViewSource.Source = context.AlternativeCasts.Local.ToObservableCollection();
                alternativeCastsViewSource.SortDescriptions.Add(StandardSort.For<AlternativeCast>());
            }
            using (loading.Segment(nameof(ShowContext.CastGroups) + nameof(CastGroup.Members) + nameof(CastGroup.Requirements), "Cast groups"))
                _castGroups = await context.CastGroups.Include(cg => cg.Members).Include(cg => cg.Requirements).InOrder().ToArrayAsync();
            checkRequiredCount.AlternatingCastGroups = castGroups.Where(cg => cg.AlternateCasts).Count();
            castGroupsViewSource.Source = context.CastGroups.Local.ToObservableCollection();
            using (loading.Segment(nameof(ShowContext.SameCastSets), "Same cast sets"))
                await context.SameCastSets.LoadAsync();
            sameCastSetsViewSource.Source = context.SameCastSets.Local.ToObservableCollection();
            using (loading.Segment(nameof(ShowContext.Tags) + nameof(A.Tag.Members) + nameof(A.Tag.Requirements), "Tags"))
                _tags = await context.Tags.Include(t => t.Members).Include(t => t.Requirements).InNameOrder().ToArrayAsync();
            tagsViewSource.Source = context.Tags.Local.ToObservableCollection();
            using (loading.Segment(nameof(ShowContext.Applicants) + nameof(Applicant.SameCastSet), "Applicants"))
            {
                _applicants = await context.Applicants.Include(a => a.SameCastSet).ToArrayAsync();
                allApplicantsViewSource.Source = context.Applicants.Local.ToObservableCollection();
            }
            using (loading.Segment(nameof(CastList), "Cast list"))
            {
                _castList = new CastList(applicants, alternativeCasts);
                FillGapsButton.SetBinding(Button.VisibilityProperty, new Binding
                {
                    Source = castList,
                    Path = new(nameof(CastList.ContainsGaps)),
                    Converter = new BooleanToVisibilityConverter()
                });
                castNumberMissingViewSource.Source = castList.MissingNumbers;
                castNumbersViewSource.Source = castList.CastNumbers;
            }
            using (loading.Segment(nameof(ISelectionEngine), "Selection engine"))
            {
                var show_root = context.ShowRoot;
                _auditionEngine = ParseAuditionEngine() switch
                {
                    nameof(NeuralAuditionEngine) => new NeuralAuditionEngine(criterias, NeuralEngineConfirm),
                    nameof(WeightedSumEngine) => new WeightedSumEngine(criterias),
                    _ => throw new ArgumentException($"Audition engine not handled: {ParseAuditionEngine()}")
                };
                applicantDescription.AuditionEngine = _auditionEngine;
                _selectionEngine = suitabilityCalculator.SelectionEngine = ParseSelectionEngine() switch
                {
                    nameof(HeuristicSelectionEngine) => new HeuristicSelectionEngine(_auditionEngine, alternativeCasts, show_root.CastNumberOrderBy, show_root.CastNumberOrderDirection),
                    nameof(ChunkedPairsSatEngine) => new ChunkedPairsSatEngine(_auditionEngine, alternativeCasts, show_root.CastNumberOrderBy, show_root.CastNumberOrderDirection, criterias),
                    nameof(TopPairsSatEngine) => new TopPairsSatEngine(_auditionEngine, alternativeCasts, show_root.CastNumberOrderBy, show_root.CastNumberOrderDirection, criterias),
                    nameof(ThreesACrowdSatEngine) => new ThreesACrowdSatEngine(_auditionEngine, alternativeCasts, show_root.CastNumberOrderBy, show_root.CastNumberOrderDirection, criterias),
                    nameof(HybridPairsSatEngine) => new HybridPairsSatEngine(_auditionEngine, alternativeCasts, show_root.CastNumberOrderBy, show_root.CastNumberOrderDirection, criterias),
                    nameof(RankDifferenceSatEngine) => new RankDifferenceSatEngine(_auditionEngine, alternativeCasts, show_root.CastNumberOrderBy, show_root.CastNumberOrderDirection, criterias),
                    nameof(BestPairsSatEngine) => new BestPairsSatEngine(_auditionEngine, alternativeCasts, show_root.CastNumberOrderBy, show_root.CastNumberOrderDirection, criterias),
                    _ => throw new ArgumentException($"Allocation engine not handled: {ParseSelectionEngine()}")
                };
            }
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
                            await selectionEngine.BalanceAlternativeCasts(applicants, context.SameCastSets.Local);
                        using (processing.Segment(nameof(ISelectionEngine.AllocateCastNumbers), "Allocating cast numbers"))
                            await selectionEngine.AllocateCastNumbers(applicants);
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
                await selectionEngine.UserSelectedCast(applicants.Where(a => a.IsAccepted), applicants.Where(a => !a.IsAccepted));
            return true;
        }

        private async void selectCastButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = new AutoSelectSettings(castGroups, alternativeCasts, tags, context.ShowRoot);
            var dialog = new AutoSelectDialog(settings)
            {
                Owner = Window.GetWindow(this)
            };
            if (dialog.ShowDialog() != true)
                return;
            ClearMainPanel();
            using var processing = new LoadingOverlay(this).AsSegment(nameof(selectCastButton_Click), "Processing...");
            using (processing.Segment(nameof(ISelectionEngine.SelectCastGroups), "Selecting applicants"))
            {
                if (settings.SelectCastGroups == true)
                    ClearCastGroups();
                if (settings.SelectCastGroups != false)
                    await selectionEngine.SelectCastGroups(applicants, castGroups);
            }
            using (processing.Segment(nameof(ISelectionEngine.BalanceAlternativeCasts), "Balancing alternating casts"))
            {
                if (settings.BalanceAlternativeCasts == true)
                    ClearAlternativeCasts();
                if (settings.BalanceAlternativeCasts != false)
                    await selectionEngine .BalanceAlternativeCasts(applicants, context.SameCastSets.Local);
            }
            using (processing.Segment(nameof(ISelectionEngine.AllocateCastNumbers), "Allocating cast numbers"))
            {
                if (settings.AllocateCastNumbers == true)
                    ClearCastNumbers();
                if (settings.AllocateCastNumbers != false)
                    await selectionEngine.AllocateCastNumbers(applicants);
            }
            using (processing.Segment(nameof(ISelectionEngine.ApplyTags), "Applying tags"))
            {
                if (settings.ApplyTags == true)
                    ClearTags();
                if (settings.ApplyTags != false)
                    await selectionEngine.ApplyTags(applicants, tags);
            }
        }

        private void ClearCastGroups()
        {
            foreach (var applicant in applicants)
                applicant.CastGroup = null;
        }

        private void ClearAlternativeCasts()
        {
            foreach (var applicant in applicants)
                applicant.AlternativeCast = null;
        }

        private void ClearCastNumbers()
        {
            foreach (var applicant in applicants)
                applicant.CastNumber = null;
        }

        private void ClearTags()
        {
            foreach (var applicant in applicants)
                applicant.Tags.Clear();
            foreach (var tag in tags)
                tag.Members.Clear();
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

        private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var list_box = (ListBox)sender;
            if (list_box.SelectedItem is Applicant applicant)
            {
                ShowDetailsWindow(new ApplicantForSelection(applicant, criterias));
                e.Handled = true;
            }
        }

        Dictionary<Applicant, ApplicantDetailsWindow> detailsWindows = new();

        void ShowDetailsWindow(ApplicantForSelection afs)
        {
            if (!detailsWindows.TryGetValue(afs.Applicant, out var window) || window.IsClosed)
            {
                window = new ApplicantDetailsWindow(connection, criterias, auditionEngine, afs)
                {
                    Owner = Window.GetWindow(this)
                };
                detailsWindows[afs.Applicant] = window;
                window.Show();
            }
            if (window.WindowState == WindowState.Minimized)
                window.WindowState = WindowState.Normal;
            window.Activate();
        }

        protected override void DisposeInternal()
        {
            foreach (var window in detailsWindows.Values)
            {
                window.Close();
            }
            detailsWindows.Clear();
            base.DisposeInternal();
        }

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

        private static void AddToSameCastSet(SameCastSet set, Applicant[] applicants)
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

        private static void RemoveFromSameCastSet(SameCastSet set, Applicant[] applicants)
        {
            foreach (var applicant in applicants)
            {
                set.Applicants.Remove(applicant);
                applicant.SameCastSet = null;
            }
        }

        private void ClearMainPanel()
        {
            selectionList.SelectedItem = null;
            RefreshMainPanel();
        }

        private void selectionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (suitabilityComparer != null)
                suitabilityComparer.SelectedCastGroupOrTag = selectionList.SelectedItem;
            RefreshMainPanel();
        }

        private void RefreshMainPanel()
        {
            selectionPanel.Visibility = numbersPanel.Visibility = sameCastSetsPanel.Visibility = Visibility.Collapsed;
            sortBySuitabilityCheckbox.Visibility = groupByCastGroupCheckbox.Visibility = Visibility.Hidden;
            if (selectionList.SelectedItem is CastGroup cast_group)
            {
                selectedApplicantsViewSource.Source = cast_group.Members;
                castStatusCombo.SelectedItem = CastStatus.Eligible;
                selectionPanel.Visibility = Visibility.Visible;
                castStatusNoun.Text = "applicants";
                sortBySuitabilityCheckbox.Visibility = Visibility.Visible;
            }
            else if (selectionList.SelectedItem is AlternativeCast alternative_cast)
            {
                selectedApplicantsViewSource.Source = alternative_cast.Members;
                castStatusCombo.SelectedItem = CastStatus.Available;
                selectionPanel.Visibility = Visibility.Visible;
                castStatusNoun.Text = "alternating cast members";
                sortBySuitabilityCheckbox.Visibility = Visibility.Visible;
                groupByCastGroupCheckbox.Visibility = Visibility.Visible;
            }
            else if (selectionList.SelectedItem is Tag tag)
            {
                selectedApplicantsViewSource.Source = tag.Members;
                castStatusCombo.SelectedItem = CastStatus.Eligible;
                selectionPanel.Visibility = Visibility.Visible;
                castStatusNoun.Text = "cast members";
                sortBySuitabilityCheckbox.Visibility = Visibility.Visible;
                groupByCastGroupCheckbox.Visibility = Visibility.Visible;
            }
            else if (selectionList.SelectedItem == finalCastList)
                numbersPanel.Visibility = Visibility.Visible;
            else if (selectionList.SelectedItem == keepApplicantsTogether)
                sameCastSetsPanel.Visibility = Visibility.Visible;
            else // no selection (initial state)
                return;
            ConfigureApplicantGroupingAndSorting();
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
                new_same_cast_sets = await selectionEngine.DetectFamilies(applicants);
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
                var index = sameCastSetsList.SelectedIndex;
                if (index == sameCastSetsList.Items.Count - 1)
                    index -= 1; // deleting last item
                context.DeleteSameCastSet(set);
                ConfigureAllApplicantsFiltering();
                sameCastSetsList.SelectedIndex = index;
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

        private void FillGapsButton_Click(object sender, RoutedEventArgs e)
        {
            castList.FillGaps();
        }

        private void moveUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (CastNumbersList.SelectedItems.Count == 0)
                return;
            int dont_move_above = CastNumberSet.FIRST_CAST_NUMBER;
            foreach (var cast_number in CastNumbersList.SelectedItems.OfType<CastNumber>().OrderBy(n => n.Number).ToArray()) // order ensures they move together when adjacent
            {
                castList.MoveUp(cast_number, dont_move_above);
                dont_move_above = cast_number.Number + 1; // avoid swapping with other selected rows when they reach the top of the list
            }
        }

        private void moveDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (CastNumbersList.SelectedItems.Count == 0)
                return;
            foreach (var cast_number in CastNumbersList.SelectedItems.OfType<CastNumber>().OrderByDescending(n => n.Number).ToArray()) // order ensures they move together when adajcent
                castList.MoveDown(cast_number);
        }

        private void mergeButton_Click(object sender, RoutedEventArgs e)
        {
            if (CastNumbersList.SelectedItems.Count == 0)
                return;
            var selection = CastNumbersList.SelectedItems.OfType<CastNumber>().ToArray();
            if (selection.Length < 2)
                MessageBox.Show("Please select 2 or more rows to merge.", WindowTitle);
            else
                castList.Merge(selection);
        }

        private void splitButton_Click(object sender, RoutedEventArgs e)
        {
            if (CastNumbersList.SelectedItems.Count == 0)
                return;
            var previous_selection = CastNumbersList.SelectedItems.OfType<CastNumber>().ToArray();
            CastNumbersList.SelectedItems.Clear();
            foreach (var cast_number in previous_selection)
                foreach (var new_cast_number in castList.Split(cast_number))
                    CastNumbersList.SelectedItems.Add(new_cast_number);
        }

        private void CastNumbersList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Properties.Settings.Default.MoveOnCtrlArrow
                && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Up)
                {
                    moveUpButton_Click(sender, e);
                    e.Handled = true;
                }
                else if (e.Key == Key.Down)
                {
                    moveDownButton_Click(sender, e);
                    e.Handled = true;
                }
            }
        }

        private void SortBySuitabilityCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            if (selectionList?.SelectedItem != null)
                ConfigureApplicantGroupingAndSorting();
        }

        private void ConfigureApplicantGroupingAndSorting()
        {
            allApplicantsViewSource.GroupDescriptions.Clear();
            selectedApplicantsViewSource.GroupDescriptions.Clear();
            var group = selectionList.SelectedItem is not CastGroup && Properties.Settings.Default.SelectCastGroupByCastGroup;
            if (group)
            {
                var gd = new PropertyGroupDescription(nameof(Applicant.CastGroup));
                allApplicantsViewSource.GroupDescriptions.Add(gd);
                selectedApplicantsViewSource.GroupDescriptions.Add(gd);
            }
            if (Properties.Settings.Default.SelectCastSortBySuitability)
            {
                suitabilityComparer.GroupByCastGroup = group;
                SortBySuitability(allApplicantsViewSource);
                SortBySuitability(selectedApplicantsViewSource);
            }
            else
            {
                SortByName(allApplicantsViewSource);
                SortByName(selectedApplicantsViewSource);
            }
            ConfigureAllApplicantsFiltering();
        }

        private static void SortByName(CollectionViewSource collection)
        {
            collection.SortDescriptions.Clear();
            foreach (var gd in collection.GroupDescriptions.OfType<PropertyGroupDescription>())
                collection.SortDescriptions.Add(new(gd.PropertyName, ListSortDirection.Ascending));
            foreach (var sd in Properties.Settings.Default.FullNameFormat.ToSortDescriptions())
                collection.SortDescriptions.Add(sd);
        }

        private void SortBySuitability(CollectionViewSource collection)
        {
            if (collection.View is ListCollectionView view)
                view.CustomSort = suitabilityComparer;
        }
                
        private void GroupByCastGroupCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            if (selectionList?.SelectedItem != null)
                ConfigureApplicantGroupingAndSorting();
        }
    }

    public class SuitabilityComparer : IComparer
    {
        SuitabilityCalculator suitability;

        public bool GroupByCastGroup { get; set; }
        public object? SelectedCastGroupOrTag { get; set; }
        
        public SuitabilityComparer(SuitabilityCalculator suitability_calculator)
        {
            suitability = suitability_calculator;
        }

        public int Compare(object? x, object? y)
        {
            if (x is not Applicant a1)
                return int.MinValue;
            if (y is not Applicant a2)
                return int.MaxValue;
            // sort by cast group (if grouping)
            if (GroupByCastGroup)
            {
                // Note: if CastGroup.Order somehow gets set to int.MinValue, it will be considered the same ordering as no cast group
                var cg1 = a1.CastGroup?.Order ?? int.MinValue;
                var cg2 = a2.CastGroup?.Order ?? int.MinValue;
                if (cg1 > cg2)
                    return 1;
                if (cg1 < cg2)
                    return -1;
            }
            // sort by suitability
            var s1 = suitability.Calculate(a1, SelectedCastGroupOrTag);
            var s2 = suitability.Calculate(a2, SelectedCastGroupOrTag);
            if (s1 < s2)
                return 1; // descending order
            else if (s1 > s2)
                return -1; // descending order
            // fallback to name
            var n1 = FullName.Format(a1);
            var n2 = FullName.Format(a2);
            return n1.CompareTo(n2);
        }
    }

    public enum CastStatus
    {
        Available,
        Eligible
    }
}
