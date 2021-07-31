using CarmenUI.Converters;
using CarmenUI.ViewModels;
using Microsoft.EntityFrameworkCore;
using ShowModel;
using ShowModel.Applicants;
using ShowModel.Criterias;
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
        const int AUTO_COLLAPSE_GROUP_THRESHOLD = 10;

        private CollectionViewSource applicantsViewSource;
        private CollectionViewSource criteriasViewSource;
        private BooleanLookupDictionary groupExpansionLookup;

        public EditApplicants(DbContextOptions<ShowContext> context_options, bool hide_complete_applicants) : base(context_options)
        {
            InitializeComponent();
            applicantsViewSource = (CollectionViewSource)FindResource(nameof(applicantsViewSource));
            criteriasViewSource = (CollectionViewSource)FindResource(nameof(criteriasViewSource));
            groupExpansionLookup = (BooleanLookupDictionary)FindResource(nameof(groupExpansionLookup));
            groupCombo.SelectedIndex = 0; // must be after InitializeComponent() because it triggers groupCombo_SelectionChanged
            hideCompleteApplicants.IsChecked = hide_complete_applicants;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)//LATER perform this loading before showing page, using a loadingoverlay
        {
            // initialise with "Loading..."
            applicantsViewSource.Source = new[] { "Loading..." };
            // populate source asynchronously
            var applicants = TaskToLoad(c => c.Applicants);
            applicants.Start();
            applicantsViewSource.Source = await applicants;
            ConfigureGroupingAndSorting(groupCombo.SelectedItem);
            applicantsList.SelectedItem = null;
            var criterias = TaskToLoad(c => c.Criterias);
            criterias.Start();
            criteriasViewSource.Source = await criterias;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)//TODO abstract this to all pages
        {
            if (context.ChangeTracker.HasChanges()
                && MessageBox.Show("Are you sure you want to cancel?\nAny unsaved changes will be lost.", WindowTitle, MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;
            OnReturn(null);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)//TODO abstract this to all pages
        {
            if (SaveChanges())
                OnReturn(DataObjects.Applicants);
        }

        private void AddApplicant_Click(object sender, RoutedEventArgs e)//LATER fix names
        {
            if (applicantsViewSource.Source is IList list)//LATER change ilist/not null view checks into hard casts (once loading is done before showing page)
            {
                filterText.Text = "";
                var applicant = new Applicant();
                list.Add(applicant);
                applicantsList.SelectedItem = applicant;
                applicantsList.ScrollIntoView(applicant);
                if (applicantsViewSource.View.Groups?.FirstOrDefault(g => ((CollectionViewGroup)g).Items.Contains(applicant)) is CollectionViewGroup group)
                    applicantsList.VisualDescendants<Expander>().First(ex => ex.DataContext == group).IsExpanded = true; // ensure new applicant group is expanded
            }
        }

        private void filterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConfigureFiltering();
        }

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
            {
                applicant.Gender = null;
                //TODO implement INotifyPropertyChanged on all objects in ShowModel, then the refresh below won't be required
                applicantsList.SelectedItem = null;
                applicantsList.SelectedItem = applicant;
            }
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
                view.Filter = o => o is not Applicant a // always show "Loading..." text
                    || (FullName.Format(a).Contains(filterText.Text, StringComparison.OrdinalIgnoreCase) // filter text
                    && (hideCompleteApplicants.IsChecked == false || !IsApplicantComplete.Check(a, criteriasViewSource))); // hide complete applicants
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

        private void hideCompleteApplicants_Checked(object sender, RoutedEventArgs e)
            => ConfigureFiltering();

        private void hideCompleteApplicants_Unchecked(object sender, RoutedEventArgs e)
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
    }
}
