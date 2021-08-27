using CarmenUI.Converters;
using CarmenUI.ViewModels;
using CarmenUI.Windows;
using Microsoft.EntityFrameworkCore;
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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CarmenUI.Pages
{
    /// <summary>
    /// Interaction logic for EditApplicants.xaml
    /// </summary>
    public partial class EditApplicants : SubPage
    {
        const int AUTO_COLLAPSE_GROUP_THRESHOLD = 10; //LATER why not make this a user setting?

        private CollectionViewSource applicantsViewSource;
        private CollectionViewSource criteriasViewSource;
        private BooleanLookupDictionary groupExpansionLookup;

        public EditApplicants(DbContextOptions<ShowContext> context_options, bool show_incomplete_applicants, bool show_registered_applicants, bool show_auditioned_applicants) : base(context_options)
        {
            InitializeComponent();
            applicantsViewSource = (CollectionViewSource)FindResource(nameof(applicantsViewSource));
            criteriasViewSource = (CollectionViewSource)FindResource(nameof(criteriasViewSource));
            groupExpansionLookup = (BooleanLookupDictionary)FindResource(nameof(groupExpansionLookup));
            groupCombo.SelectedIndex = 0; // must be after InitializeComponent() because it triggers groupCombo_SelectionChanged
            showIncompleteApplicants.IsChecked = show_incomplete_applicants;
            showRegisteredApplicants.IsChecked = show_registered_applicants;
            showAuditionedApplicants.IsChecked = show_auditioned_applicants;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            using (var loading = new LoadingOverlay(this).AsSegment(nameof(EditApplicants)))
            {
                using (loading.Segment(nameof(ShowContext.Applicants), "Applicants"))
                    await context.Applicants.LoadAsync();
                applicantsViewSource.Source = context.Applicants.Local.ToObservableCollection();
                using (loading.Segment(nameof(EditApplicants) + nameof(ConfigureGroupingAndSorting), "Sorting"))
                    ConfigureGroupingAndSorting(groupCombo.SelectedItem);
                applicantsList.SelectedItem = null;
                using (loading.Segment(nameof(ShowContext.Criterias), "Criteria"))
                    await context.Criterias.LoadAsync();
                criteriasViewSource.Source = context.Criterias.Local.ToObservableCollection();
            }
            filterText.Focus();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
            => CancelChangesAndReturn();

        private void SaveButton_Click(object sender, RoutedEventArgs e)
            => SaveChangesAndReturn();

        private void AddApplicant_Click(object sender, RoutedEventArgs e)//LATER fix names
        {//LATER this is really slow, maybe add a loading dialog? but hang on, why the hell is it slow?
            if (applicantsViewSource.Source is IList list)//LATER change ilist/not null view checks into hard casts (now that loading is done before showing page)
            {
                var applicant = new Applicant();
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
                filterText.Text = "";
                EnsureApplicantIsShown(applicant);
                list.Add(applicant);
                applicantsList.SelectedItem = applicant;
                applicantsList.ScrollIntoView(applicant);
                if (applicantsViewSource.View.Groups?.FirstOrDefault(g => ((CollectionViewGroup)g).Items.Contains(applicant)) is CollectionViewGroup group)
                    applicantsList.VisualDescendants<Expander>().First(ex => ex.DataContext == group).IsExpanded = true; // ensure new applicant group is expanded
                firstNameText.Focus();
            }
        }

        private void filterText_TextChanged(object sender, TextChangedEventArgs e)
            => ConfigureFiltering();

        private Dictionary<string, int> GetGroupCounts(ICollectionView view)
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
        {
            //LATER persist the "group by" in user settings, but as 2 separate settings for Register/Audition UI links
            if (groupCombo.SelectedItem != null)
                ConfigureGroupingAndSorting(groupCombo.SelectedItem);
        }

        private void ConfigureGroupingAndSorting(object selected_grouping)
        {
            if (applicantsViewSource.View == null)
                return;
            // clear old grouping
            applicantsViewSource.GroupDescriptions.Clear();
            // add new grouping
            applicantListContextMenu.IsOpen = false;
            applicantListContextMenu.Visibility = Visibility.Visible;
            if (selected_grouping is string group_by_string)
            {
                if (group_by_string == string.Empty)
                {
                    // don't group by anything
                    applicantListContextMenu.Visibility = Visibility.Collapsed;
                }
                else
                {
                    PropertyGroupDescription gd = groupCombo.SelectedItem switch
                    {
                        "First Name" => new(nameof(Applicant.FirstName)),
                        "Last Name" => new(nameof(Applicant.LastName)),
                        "Gender" => new(nameof(Applicant.Gender)),
                        "Year of Birth" => new(nameof(Applicant.DateOfBirth), new DateToYear()),
                        _ => throw new NotImplementedException($"String value not implemented: {group_by_string}")
                    }; 
                    applicantsViewSource.GroupDescriptions.Add(gd);
                    applicantsViewSource.View.SortDescriptions.Add(new(gd.PropertyName, ListSortDirection.Ascending));
                }
            }
            else if (selected_grouping is Criteria group_by_criteria)
            {
                var converter = new AbilitySelector(group_by_criteria);
                PropertyGroupDescription gd = new(nameof(Applicant.Abilities), converter);
                applicantsViewSource.GroupDescriptions.Add(gd);
                gd.SortDescriptions.Add(new(nameof(CollectionViewGroup.Name), ListSortDirection.Ascending));
            }
            else
            {
                throw new NotImplementedException($"Object value not implemented: {selected_grouping.GetType().Name}");
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
                var previous_counts = GetGroupCounts(view);
                view.Filter = o => o is not Applicant a || FilterPredicate(a);
                var new_counts = GetGroupCounts(view);
                foreach (var (key, new_count) in new_counts)
                {
                    if (!previous_counts.TryGetValue(key, out var old_count) // the group wasn't previously shown
                        || ((old_count > AUTO_COLLAPSE_GROUP_THRESHOLD) != (new_count > AUTO_COLLAPSE_GROUP_THRESHOLD))) // or it crossed the threshold
                    {
                        groupExpansionLookup.Dictionary[key] = new_count <= AUTO_COLLAPSE_GROUP_THRESHOLD;
                    }
                }
                if (applicantsList.Items.Count == 1)
                    applicantsList.SelectedIndex = 0;
            }
        }

        private bool FilterPredicate(Applicant applicant)
        {
            if (!AnyFormatOfNameContains(applicant, filterText.Text))
                return false;
            if (!applicant.IsRegistered)
                return showIncompleteApplicants.IsChecked == true;
            if (ShowIfApplicantRegisteredAndAuditioned.Check(applicant.IsRegistered, applicant.Abilities, criteriasViewSource))
                return showAuditionedApplicants.IsChecked == true;
            return showRegisteredApplicants.IsChecked == true;
        }

        /// <summary>This modifies at most one of the show*Applicants checkboxes to ensure the given applicant is shown,
        /// assuming that filterText is blank. This duplicates logic in FilterPredicate()</summary>
        private void EnsureApplicantIsShown(Applicant applicant)
        {
            if (!applicant.IsRegistered)
                showIncompleteApplicants.IsChecked = true;
            else if (ShowIfApplicantRegisteredAndAuditioned.Check(applicant.IsRegistered, applicant.Abilities, criteriasViewSource))
                showAuditionedApplicants.IsChecked = true;
            else
                showRegisteredApplicants.IsChecked = true;
        }

        private bool AnyFormatOfNameContains(Applicant applicant, string filter_text)
            => Enum.GetValues<FullNameFormat>().Any(f => FullName.Format(applicant, f).Contains(filter_text, StringComparison.OrdinalIgnoreCase));

        private void applicantsList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && applicantsList.SelectedItem is Applicant applicant)
            {
                e.Handled = true;
                var msg = $"Are you sure you want to delete '{FullName.Format(applicant)}'?";
                if (MessageBox.Show(msg, WindowTitle, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    ((IList)applicantsViewSource.Source).Remove(applicant);
            }
        }

        private void showApplicantsCheckbox_Changed(object sender, RoutedEventArgs e)
            => ConfigureFiltering();

        private void applicantsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Properties.Settings.Default.SaveOnApplicantChange)
                SaveChanges();
        }

        private void ExpandAll_Click(object sender, RoutedEventArgs e)
            => ExpandListHeaders(true);

        private void CollapseAll_Click(object sender, RoutedEventArgs e)
            => ExpandListHeaders(false);

        private void RefreshGroups_Click(object sender, RoutedEventArgs e)
            => ConfigureGroupingAndSorting(groupCombo.SelectedItem);

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
                AddApplicant_Click(sender, e);
                e.Handled = true;
            }
            else if (Properties.Settings.Default.FilterOnCtrlF
                && e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                filterText.Focus();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && applicantsList.SelectedItem != null)
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
                filterText.Text = "";
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
    }
}
