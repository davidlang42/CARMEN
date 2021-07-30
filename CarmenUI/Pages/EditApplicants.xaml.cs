using CarmenUI.Converters;
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
        //TODO checkbox setting for "Hide complete applicants", which defaults to false if you
        //     click "Register Applicants" but defaults to true if you click "Audition Applicants"
        //TODO add checkbox setting for "Save changes per applicant", or something like that, so that
        //     changes are saved every time the applicant changes. persist in user settings.
        //     add save/cancel button to standard location. if save per applicant is unticked,
        //     save button works as normal, if ticked, save button changes to 'OK' rather than save
        //     and icon changes (or tick at least changes color), and implement saving
        //TODO make header text "Selected from (123) (incomplete) applicants (containing 'Walt')" (123) is total count in view, (incomplete) is removed
        //     if hide complete applicants is unticked, (containing ...) is shown if filterText is set. If count is exactly 1, text should be
        //     "Selected 1 (incomplete) applicant (containing 'Walt')". If none, "No (incomplete) applicants found (containing 'Walt')"
        //TODO add right click on applicantsList for expand all/collapse all

        //TODO make applicant Status shown by a Converter Binding / to whole object
        //TODO make applicant Description (Female, 28 years old) shown by a multi converter, which can omit either field if not set
        //TODO generate and populate criterias
        //TODO show/edit photo
        //TODO fix bug with dob, where it doesn't change when i chaneg applicants, probably a dd/mm/yyyy vs mm/dd/yyyy format thing
        //TODO handle add new applicant
        //TODO delete with confirmation in applicant list

        private CollectionViewSource applicantsViewSource;
        private CollectionViewSource criteriasViewSource;
        private BooleanLookupDictionary groupExpansionLookup;

        public EditApplicants(DbContextOptions<ShowContext> context_options) : base(context_options)
        {
            InitializeComponent();
            applicantsViewSource = (CollectionViewSource)FindResource(nameof(applicantsViewSource));
            criteriasViewSource = (CollectionViewSource)FindResource(nameof(criteriasViewSource));
            groupExpansionLookup = (BooleanLookupDictionary)FindResource(nameof(groupExpansionLookup));
            groupCombo.SelectedIndex = 0; // must be after InitializeComponent() because it triggers groupCombo_SelectionChanged
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // initialise with "Loading..."
            applicantsViewSource.Source = new[] { "Loading..." };
            // populate source asynchronously
            var applicants = TaskToLoad(c => c.Applicants);
            applicants.Start();
            applicantsViewSource.Source = await applicants;
            ConfigureGroupingAndSorting(groupCombo.SelectedItem);
            var criterias = TaskToLoad(c => c.Criterias);
            criterias.Start();
            criteriasViewSource.Source = await criterias;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(null);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveChanges())
                OnReturn(DataObjects.Applicants);
        }

        private void ImportApplicants_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddApplicant_Click(object sender, RoutedEventArgs e)
        {

        }

        private void filterText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (applicantsViewSource.View is ICollectionView view)
            {
                var previous_counts = GetGroupCounts(view);
                view.Filter = a => FullName.Format((Applicant)a).Contains(filterText.Text, StringComparison.OrdinalIgnoreCase);
                var new_counts = GetGroupCounts(view);
                foreach (var (key, new_count) in new_counts)
                {
                    if (!previous_counts.TryGetValue(key, out var old_count) // the group wasn't previously shown
                        || ((old_count > AUTO_COLLAPSE_GROUP_THRESHOLD) != (new_count > AUTO_COLLAPSE_GROUP_THRESHOLD))) // or it crossed the threshold
                    {
                        groupExpansionLookup.Dictionary[key] = new_count <= AUTO_COLLAPSE_GROUP_THRESHOLD;
                    }
                }
            }
        }

        private Dictionary<string, int> GetGroupCounts(ICollectionView view)
            => view.Groups.OfType<CollectionViewGroup>()
            .Where(g => g.Name != null)
            .Where(g => g.ItemCount > 0)
            .ToDictionary(g => g.Name.ToString()!, g => g.ItemCount);

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
                //LATER implement INotifyPropertyChanged on all objects in ShowModel, then the refresh below won't be required
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
            if (selected_grouping is string group_by_string)
            {
                if (group_by_string != string.Empty) // empty string means don't group by anything
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
                    applicantsViewSource.View.SortDescriptions.Add(new(gd.PropertyName, ListSortDirection.Ascending)); //LATER this also worked: gd.SortDescriptions.Add(new("Name", ListSortDirection.Ascending));
                }
            }
            else if (selected_grouping is Criteria group_by_criteria)
            {
                //TODO implement grouping by criteria, sorting might be tricky (probably use GroupDescriptions.CustomSort)
            }
            else
            {
                throw new NotImplementedException($"Object value not implemented: {selected_grouping.GetType().Name}");
            }
            // add sort by name (based on full name formatting setting)
            foreach (var sd in Properties.Settings.Default.FullNameFormat.ToSortDescriptions())
                if (!applicantsViewSource.View.SortDescriptions.Contains(sd))
                    applicantsViewSource.View.SortDescriptions.Add(sd);
        }
    }
}
