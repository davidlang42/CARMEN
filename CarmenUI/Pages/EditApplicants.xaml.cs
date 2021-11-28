using CarmenUI.Converters;
using CarmenUI.ViewModels;
using CarmenUI.Windows;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CarmenUI.Bindings;
using Microsoft.Win32;
using Carmen.ShowModel.Import;
using System.Diagnostics;
using System.IO;
using Carmen.CastingEngine.Audition;

namespace CarmenUI.Pages
{
    /// <summary>
    /// Interaction logic for EditApplicants.xaml
    /// </summary>
    public partial class EditApplicants : SubPage
    {
        readonly CollectionViewSource applicantsViewSource;
        readonly CollectionViewSource criteriasViewSource;
        readonly BooleanLookupDictionary groupExpansionLookup;
        readonly OverallAbilityCalculator overallAbilityCalculator;

        private Criteria[]? _criterias;
        private Criteria[] criterias => _criterias ?? throw new ApplicationException($"Tried to used {nameof(criterias)} before it was loaded.");

        public EditApplicantsMode Mode { get; init; }

        public EditApplicants(RecentShow connection, EditApplicantsMode mode) : base(connection)
        {
            this.Mode = mode;
            InitializeComponent();
            applicantsViewSource = (CollectionViewSource)FindResource(nameof(applicantsViewSource));
            criteriasViewSource = (CollectionViewSource)FindResource(nameof(criteriasViewSource));
            groupExpansionLookup = (BooleanLookupDictionary)FindResource(nameof(groupExpansionLookup));
            overallAbilityCalculator = (OverallAbilityCalculator)((FakeItTilYouUpdateIt)FindResource(nameof(overallAbilityCalculator))).First();
            if (mode == EditApplicantsMode.RegisterApplicants)
            {
                this.Title = "Register Applicants";
                filterHeading.Text = $"Filter applicants list by:";
                hideFinishedApplicants.Content = $"Hide applicants who have completed registration";
                addApplicantText.Text = "Add new applicant";
                importButton.Content = "Import applicants from CSV";
                groupCombo.SetBinding(ComboBox.SelectedIndexProperty, new SettingBinding(nameof(Properties.Settings.RegisterApplicantsGroupByIndex)));
            }
            else if (mode == EditApplicantsMode.AuditionApplicants)
            {
                this.Title = "Audition Applicants";
                filterHeading.Text = $"Filter auditionees list by:";
                hideFinishedApplicants.Content = $"Hide auditionees who have all marks set";
                addApplicantText.Text = "Add new auditionee";
                importButton.Content = "Import marks from another show";
                groupCombo.SetBinding(ComboBox.SelectedIndexProperty, new SettingBinding(nameof(Properties.Settings.AuditionApplicantsGroupByIndex)));
            }
            else
                throw new NotImplementedException($"Enum not handled: {mode}");
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            using (var loading = new LoadingOverlay(this).AsSegment(nameof(EditApplicants)))
            {
                using (loading.Segment(nameof(ShowContext.Criterias), "Criteria"))
                {
                    _criterias = await context.Criterias.InOrder().ToArrayAsync();
                    criteriasViewSource.Source = context.Criterias.Local.ToObservableCollection();
                    criteriasViewSource.View.SortDescriptions.Add(StandardSort.For<Criteria>());
                }
                using (loading.Segment(nameof(IAuditionEngine), "Audition engine"))
                    overallAbilityCalculator.AuditionEngine = ParseAuditionEngine() switch
                    {
                        nameof(NeuralAuditionEngine) => new NeuralAuditionEngine(criterias, NeuralEngineConfirm),
                        nameof(WeightedSumEngine) => new WeightedSumEngine(criterias),
                        _ => throw new ArgumentException($"Audition engine not handled: {ParseAuditionEngine()}")
                    };
                using (loading.Segment(nameof(ShowContext.Applicants) + nameof(Applicant.Notes), "Applicants"))
                    await context.Applicants.Include(a => a.Notes).LoadAsync();
                using (loading.Segment(nameof(EditApplicants) + nameof(Applicant), "First applicant"))
                {
                    applicantsViewSource.Source = context.Applicants.Local.ToObservableCollection();
                    applicantsList.SelectedItem = null;
                }
                using (loading.Segment(nameof(EditApplicants) + nameof(ConfigureGroupingAndSorting), "Sorting"))
                    ConfigureGroupingAndSorting();
            }
            filterText.Focus();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => CancelChangesAndReturn();

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
            => await SaveChangesAndReturn();

        private void AddApplicant_Click(object sender, RoutedEventArgs e)
        {
            var list = (IList)applicantsViewSource.Source;
            var applicant = new Applicant { ShowRoot = context.ShowRoot };
            if (filterText.Text.Contains(","))
            {
                var names = filterText.Text.Split(",");
                applicant.FirstName = names[1].Trim().ToProperCase();
                applicant.LastName = names[0].Trim().ToProperCase();
            }
            else
            {
                var names = filterText.Text.Split(" ", 2);
                applicant.FirstName = names[0].Trim().ToProperCase();
                if (names.Length > 1)
                    applicant.LastName = names[1].Trim().ToProperCase();
            }
            ClearFilter();
            list.Add(applicant);
            applicantsList.SelectedItem = applicant;
            applicantsList.ScrollIntoView(applicant);
            if (applicantsViewSource.View.Groups?.FirstOrDefault(g => ((CollectionViewGroup)g).Items.Contains(applicant)) is CollectionViewGroup group)
                applicantsList.VisualDescendants<Expander>().First(ex => ex.DataContext == group).IsExpanded = true; // ensure new applicant group is expanded
            firstNameText.Focus();
        }

        private void ClearFilter()
        {
            filterText.Text = "";
            hideFinishedApplicants.IsChecked = false;
        }

        private void filterText_TextChanged(object sender, TextChangedEventArgs e)
            => ConfigureFiltering();

        private static Dictionary<string, int> GetGroupCounts(ICollectionView view)
            => view.Groups?.OfType<CollectionViewGroup>()
            .Where(g => g.Name != null)
            .Where(g => g.ItemCount > 0)
            .ToDictionary(g => g.Name.ToString()!, g => g.ItemCount)
            ?? new();

        private void GroupExpander_Expanded(object sender, RoutedEventArgs e)
            => StoreExpanderState((Expander)sender, true);

        private void GroupExpander_Collapsed(object sender, RoutedEventArgs e)
            => StoreExpanderState((Expander)sender, false);

        private void StoreExpanderState(Expander expander, bool is_expanded)
        {
            if (expander.DataContext is CollectionViewGroup group && group.Name?.ToString() is string key)
                groupExpansionLookup.Dictionary[key] = is_expanded;
        }

        private void ClearGender_Click(object sender, RoutedEventArgs e)
        {
            if (applicantsViewSource.View.CurrentItem is Applicant applicant)
                applicant.Gender = null;
        }

        private void groupCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ConfigureGroupingAndSorting();

        private void ConfigureGroupingAndSorting()
        {
            if (applicantsViewSource.View == null)
                return;
            // clear old grouping
            applicantsViewSource.GroupDescriptions.Clear();
            // add new grouping
            applicantListContextMenu.IsOpen = false;
            applicantListContextMenu.Visibility = Visibility.Visible;
            if (groupCombo.SelectedItem is Criteria group_by_criteria)
            {
                var converter = new AbilitySelector(group_by_criteria);
                PropertyGroupDescription gd = new(nameof(Applicant.Abilities), converter);
                applicantsViewSource.GroupDescriptions.Add(gd);
                gd.SortDescriptions.Add(new(nameof(CollectionViewGroup.Name), ListSortDirection.Ascending));
            }
            else if (groupCombo.SelectedItem == null || string.Empty.Equals(groupCombo.SelectedItem))
            {
                // don't group by anything
                applicantListContextMenu.Visibility = Visibility.Collapsed;
            }
            else if (groupCombo.SelectedItem is string non_empty_string)
            {
                PropertyGroupDescription gd = non_empty_string switch
                {
                    "First Name" => new(nameof(Applicant.FirstName)),
                    "Last Name" => new(nameof(Applicant.LastName)),
                    "Gender" => new(nameof(Applicant.Gender)),
                    "Year of Birth" => new(nameof(Applicant.DateOfBirth), new DateToYear()),
                    _ => throw new NotImplementedException($"String value not implemented: {non_empty_string}")
                };
                applicantsViewSource.GroupDescriptions.Add(gd);
                applicantsViewSource.View.SortDescriptions.Add(new(gd.PropertyName, ListSortDirection.Ascending));
            }
            else
            {
                throw new NotImplementedException($"Object value not implemented: {groupCombo.SelectedItem.GetType().Name}");
            }
            // add sort by name (based on full name formatting setting)
            foreach (var sd in Properties.Settings.Default.FullNameFormat.ToSortDescriptions())
                if (!applicantsViewSource.View.SortDescriptions.Contains(sd))
                    applicantsViewSource.View.SortDescriptions.Add(sd);
            // re-configure filtering
            ConfigureFiltering();
        }

        private void ConfigureFiltering()
        {
            if (applicantsViewSource.View is ICollectionView view)
            {
                var auto_collapse_group_threshold = Properties.Settings.Default.AutoCollapseGroupThreshold;
                var previous_counts = GetGroupCounts(view);
                var text = filterText.Text;
                view.Filter = (Mode, hideFinishedApplicants.IsChecked) switch
                {
                    (EditApplicantsMode.RegisterApplicants, true) when string.IsNullOrEmpty(text) => o => o is Applicant a && !a.IsRegistered,
                    (EditApplicantsMode.RegisterApplicants, true) => o => o is Applicant a && AnyNameContains(a, text) && !a.IsRegistered,
                    (EditApplicantsMode.AuditionApplicants, true) when string.IsNullOrEmpty(text) => o => o is Applicant a && !a.HasAuditioned(criterias),
                    (EditApplicantsMode.AuditionApplicants, true) => o => o is Applicant a && AnyNameContains(a, text) && !a.HasAuditioned(criterias),
                    _ when string.IsNullOrEmpty(text) => o => o is Applicant a,
                    _ => o => o is Applicant a && AnyNameContains(a, text),
                };
                var new_counts = GetGroupCounts(view);
                foreach (var (key, new_count) in new_counts)
                {
                    if (!previous_counts.TryGetValue(key, out var old_count) // the group wasn't previously shown
                        || ((old_count > auto_collapse_group_threshold) != (new_count > auto_collapse_group_threshold))) // or it crossed the threshold
                    {
                        groupExpansionLookup.Dictionary[key] = new_count <= auto_collapse_group_threshold;
                    }
                }
                if (applicantsList.Items.Count == 1)
                    applicantsList.SelectedIndex = 0;
            }
        }

        private readonly FullNameFormat[] allNameFormats = Enum.GetValues<FullNameFormat>();

        /// <summary>Checks if this appliants name, in any of the possible name formats, contains the filter text</summary>
        private bool AnyNameContains(Applicant applicant, string filter_text)
            => allNameFormats.Any(f => FullName.Format(applicant, f).Contains(filter_text, StringComparison.OrdinalIgnoreCase));

        private void applicantsList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && applicantsList.SelectedItem is Applicant applicant)
            {
                e.Handled = true;
                DeleteApplicant(applicant);
            }
        }

        private void hideFinishedApplicants_Changed(object sender, RoutedEventArgs e)
            => ConfigureFiltering();

        private async void applicantsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CommitNewNote(e.RemovedItems.OfType<Applicant>().FirstOrDefault());
            if (Properties.Settings.Default.SaveOnApplicantChange)
                await SaveChanges(user_initiated: false);
            NotesScrollViewer.ScrollToEnd();
            ConfigureFiltering();
        }

        private void CommitNewNote(Applicant? applicant)
        {
            CommitTextboxValue();
            if (!string.IsNullOrWhiteSpace(NewNoteTextBox.Text))
            {
                if (applicant != null)
                {
                    applicant.Notes.Add(new Note
                    {
                        Applicant = applicant,
                        Text = NewNoteTextBox.Text,
                        Author = GetUserName()
                    });
                    NewNoteTextBox.Text = "";
                }
                else
                    MessageBox.Show("Unable to add new note:\n" + NewNoteTextBox.Text, WindowTitle);
            }
        }

        protected override Task<bool> PreSaveChecks()
        {
            CommitNewNote(applicantsList.SelectedItem as Applicant);
            return Task.FromResult(true);
        }

        private void ExpandAll_Click(object sender, RoutedEventArgs e)
            => ExpandListHeaders(true);

        private void CollapseAll_Click(object sender, RoutedEventArgs e)
            => ExpandListHeaders(false);

        private void RefreshGroups_Click(object sender, RoutedEventArgs e)
            => ConfigureGroupingAndSorting();

        private void ExpandListHeaders(bool expanded)
        {
            foreach (var expander in applicantsList.VisualDescendants<Expander>())
                expander.IsExpanded = expanded;
        }

        private void ClearValue_Click(object sender, RoutedEventArgs e)
        {
            if (((MenuItem)sender).DataContext is NullableAbility nullable_ability)
                nullable_ability.Mark = null;
        }

        private void filterText_GotFocus(object sender, RoutedEventArgs e)
        {
            ConfigureFiltering();
            filterText.SelectAll();
        }

        protected override void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Properties.Settings.Default.NewOnCtrlN
                && e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
            {
                CommitTextboxValue();
                AddApplicant_Click(sender, e);
                e.Handled = true;
            }
            else if (Properties.Settings.Default.FilterOnCtrlF
                && e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                filterText.Focus();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && applicantsList.SelectedItem != null && Keyboard.Modifiers == ModifierKeys.None)
            {
                filterText.Focus();
                applicantsList.SelectedItem = null;
                e.Handled = true;
            }
            else
                base.Window_KeyDown(sender, e);
        }

        private void filterText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && filterText.Text != "")
            {
                ClearFilter();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                if (applicantsList.Items.Count == 0)
                    AddApplicant_Click(sender, e);
                else
                {
                    if (applicantsList.SelectedItem == null)
                        applicantsList.SelectedIndex = 0;
                    firstNameText.Focus();
                }    
                e.Handled = true;
            }
        }

        private void NoApplicantsPanel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => AddApplicant_Click(sender, e);

        private async void importButton_Click(object sender, RoutedEventArgs e)
        {
            if (Mode == EditApplicantsMode.RegisterApplicants)
                await ImportApplicants();
            else if (Mode == EditApplicantsMode.AuditionApplicants)
                await ImportMarks();
            else
                throw new NotImplementedException($"Enum not handled: {Mode}");
        }

        private async Task ImportApplicants()
        {
            var file = new OpenFileDialog
            {
                Title = "Import applicants from CSV ",
                Filter = "Comma separated values (*.csv)|*.csv|All Files (*.*)|*.*"
            };
            if (file.ShowDialog() == false)
                return;
            CsvImporter csv;
            using (var loading = new LoadingOverlay(this).AsSegment($"{nameof(ImportApplicants)}_Preparation"))
            {
                CastGroup[] castGroups;
                AlternativeCast[] alternativeCasts;
                Tag[] tags;
                using (loading.Segment(nameof(ShowContext.CastGroups), "Cast groups"))
                    castGroups = await context.CastGroups.ToArrayAsync();
                using (loading.Segment(nameof(ShowContext.AlternativeCasts), "Alternative casts"))
                    alternativeCasts = await context.AlternativeCasts.ToArrayAsync();
                using (loading.Segment(nameof(ShowContext.Tags), "Tags"))
                    tags = await context.Tags.ToArrayAsync();
                using (loading.Segment(nameof(CsvImporter),"Columns headers"))
                    csv = new CsvImporter(file.FileName, criterias, castGroups, alternativeCasts, tags, i => context.Images.Remove(i));
            }
            var import = new ImportDialog(csv)
            {
                Title = "Import applicants from " + System.IO.Path.GetFileName(file.FileName),
                Owner = Window.GetWindow(this)
            };
            if (import.ShowDialog() == true)
            {
                ImportResult result;
                using (var importing = new LoadingOverlay(this) { MainText = "Importing...", SubText = "0 rows processed" })
                    result = csv.Import(context.Applicants.Local, context.ShowRoot, r => importing.SubText = r.Plural("row") + " processed");
                var msg = $"{result.RecordsProcessed.Plural("row")} processed";
                var buttons = MessageBoxButton.OK;
                if (result.NewApplicantsAdded != 0)
                    msg += $"\n{result.NewApplicantsAdded.Plural("applicant")} added";
                if (result.ExistingApplicantsUpdated != 0)
                    msg += $"\n{result.ExistingApplicantsUpdated.Plural("existing applicant")} updated";
                if (result.ExistingApplicantsNotChanged != 0)
                    msg += $"\n{result.ExistingApplicantsNotChanged.Plural("existing applicant")} not changed";
                if (result.ParseErrors.Count != 0)
                {
                    msg += $"\n{result.ParseErrors.Count.Plural("field")} contained errors";
                    msg += "\n\nWould you like to view the errors?";
                    buttons = MessageBoxButton.YesNo;
                }
                if (MessageBox.Show(msg, WindowTitle, buttons) == MessageBoxResult.Yes)
                {
                    var temp_file = $"{System.IO.Path.GetTempPath()}import_errors_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
                    using (var writer = new System.IO.StreamWriter(temp_file))
                        foreach (var (row, error) in result.ParseErrors)
                            writer.WriteLine($"Row {row}: {error}");
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = temp_file,
                        UseShellExecute = true
                    }); // launches default application for TXT files
                }
            }
            csv.Dispose();
        }

        private async Task ImportMarks()
        {
            if (applicantsList.SelectedItem is not Applicant applicant)
            {
                MessageBox.Show("Please select an applicant.", WindowTitle);
                return;
            }
            var dialog = new OpenFileDialog
            {
                Title = $"Import marks from another show",
                Filter = "Sqlite Database (*.db)|*.db"
            };
            if (dialog.ShowDialog() != true)
                return;
            Applicant[] import_applicants;
            ShowContext import;
            ApplicantPickerDialog picker;
            using (var loading = new LoadingOverlay(this))
            {
                loading.SubText = "Checking database integrity";
                import = ShowContext.Open(new LocalShowConnection(dialog.FileName));
                var state = await import.CheckDatabaseState();
                if (state == ShowContext.DatabaseState.Empty)
                {
                    loading.Dispose();
                    await import.DisposeAsync();
                    MessageBox.Show("The selected database is empty.", Title);
                    return;
                }
                else if (state == ShowContext.DatabaseState.SavedWithFutureVersion)
                {
                    loading.Dispose();
                    await import.DisposeAsync();
                    MessageBox.Show("The selected database was saved with a newer version of CARMEN and cannot be opened. Please install the latest version.", Title);
                    return;
                }
                else if (state == ShowContext.DatabaseState.SavedWithPreviousVersion)
                {
                    loading.SubText = "Creating temporary migration";
                    await import.DisposeAsync();
                    var temp_file = Path.GetTempFileName();
                    File.Copy(dialog.FileName, temp_file, true); // make a copy so the original is not migrated
                    import = ShowContext.Open(new LocalShowConnection(temp_file));
                    await import.UpgradeDatabase();
                }
                loading.SubText = "Applicant list";
                import_applicants = await import.Applicants.ToArrayAsync();
                picker = new ApplicantPickerDialog(import_applicants, applicant.FirstName, applicant.LastName)
                {
                    Owner = Window.GetWindow(this)
                };
            }
            int count = 0;
            using (import)
            {
                if (picker.ShowDialog() != true || picker.SelectedApplicant is not Applicant import_selected)
                    return;
                var note_text = "Imported marks";
                if (!import_selected.FirstName.Equals(applicant.FirstName, StringComparison.OrdinalIgnoreCase) || !import_selected.LastName.Equals(applicant.LastName, StringComparison.OrdinalIgnoreCase))
                {
                    var msg = $"Importing applicant '{import_selected.FirstName} {import_selected.LastName}' does not match selected applicant '{applicant.FirstName} {applicant.LastName}'. Would you like to add them as a new applicant instead?";
                    var result = MessageBox.Show(msg, WindowTitle, MessageBoxButton.YesNoCancel);
                    if (result == MessageBoxResult.Cancel)
                        return;
                    if (result == MessageBoxResult.Yes)
                    {
                        applicant = new Applicant
                        {
                            ShowRoot = context.ShowRoot,
                            FirstName = import_selected.FirstName,
                            LastName = import_selected.LastName,
                            Gender = import_selected.Gender,
                            DateOfBirth = import_selected.DateOfBirth
                        };
                        context.Applicants.Add(applicant);
                        note_text = "Imported applicant";
                    }
                }
                using (var loading = new LoadingOverlay(this) { MainText = "Importing...", SubText = $"{import_selected.FirstName} {import_selected.FirstName}" })
                {
                    note_text += " from " + import.ShowRoot.Name;
                    var import_abilities = await import.Abilities.Where(a => a.Applicant == import_selected).Include(ab => ab.Criteria).ToArrayAsync();
                    foreach (var import_ability in import_abilities)
                    {
                        var import_formatted_mark = import_ability.Criteria.Format(import_ability.Mark);
                        if (criterias.Where(c => c.Name.Equals(import_ability.Criteria.Name, StringComparison.OrdinalIgnoreCase)).SingleOrDefault() is Criteria matching_criteria)
                        {
                            if (import_formatted_mark.Equals(matching_criteria.Format(import_ability.Mark), StringComparison.OrdinalIgnoreCase)) // important for select criteria
                            {
                                if (applicant.Abilities.Where(ab => ab.Criteria == matching_criteria).SingleOrDefault() is Ability existing_ability && existing_ability.Mark != import_ability.Mark)
                                    note_text += $"\nOverwrote previous {matching_criteria.Name} ability of {matching_criteria.Format(existing_ability.Mark)} with {import_formatted_mark}";
                                applicant.SetMarkFor(matching_criteria, import_ability.Mark);
                                count++;
                            }
                            else
                                note_text += $"\nHad {matching_criteria.Name} ability of {import_formatted_mark} (format mismatch)";
                        }
                        else
                            note_text += $"\nHad {import_ability.Criteria.Name} ability of {import_formatted_mark} (no match found)";
                    }
                    applicant.Notes.Add(new Note
                    {
                        Applicant = applicant,
                        Text = note_text,
                        Author = GetUserName()
                    });
                }
            }
            applicantsList.SelectedItem = null;
            applicantsList.SelectedItem = applicant;
            MessageBox.Show($"Imported {count.Plural("ability","abilities")} for '{applicant.FirstName} {applicant.LastName}'.", WindowTitle);
        }

        private void DeleteApplicant_Click(object sender, RoutedEventArgs e)
        {
            if (applicantsList.SelectedItem is Applicant applicant)
                DeleteApplicant(applicant);
        }

        private void DeleteApplicant(Applicant applicant)
        {
            var msg = $"Are you sure you want to delete '{FullName.Format(applicant)}'?";
            if (MessageBox.Show(msg, WindowTitle, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                ((IList)applicantsViewSource.Source).Remove(applicant);
        }
    }

    public enum EditApplicantsMode
    {
        RegisterApplicants,
        AuditionApplicants
    }
}
