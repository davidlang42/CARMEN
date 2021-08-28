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
        private ICastingEngine engine;

        public SelectCast(DbContextOptions<ShowContext> context_options) : base(context_options)
        {
            InitializeComponent();
            castNumbersViewSource = (CollectionViewSource)FindResource(nameof(castNumbersViewSource));
            castGroupsViewSource = (CollectionViewSource)FindResource(nameof(castGroupsViewSource));
            tagsViewSource = (CollectionViewSource)FindResource(nameof(tagsViewSource));
            alternativeCastsViewSource = (CollectionViewSource)FindResource(nameof(alternativeCastsViewSource));
            selectedApplicantsViewSource = (CollectionViewSource)FindResource(nameof(selectedApplicantsViewSource));
            allApplicantsViewSource = (CollectionViewSource)FindResource(nameof(allApplicantsViewSource));
            foreach (var sd in Properties.Settings.Default.FullNameFormat.ToSortDescriptions())
            {
                allApplicantsViewSource.SortDescriptions.Add(sd);
                selectedApplicantsViewSource.SortDescriptions.Add(sd);
            }
            castStatusCombo.SelectedIndex = 0; // must be here because it triggers event below
            engine = new DummyEngine(); //LATER use real engine
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            using var loading = new LoadingOverlay(this).AsSegment(nameof(SelectCast));
            using (loading.Segment(nameof(ShowContext.AlternativeCasts), "Alternative casts"))
                await context.AlternativeCasts.LoadAsync();
            alternativeCastsViewSource.Source = context.AlternativeCasts.Local.ToObservableCollection();
            alternativeCastsViewSource.SortDescriptions.Add(StandardSort.For<AlternativeCast>());
            using (loading.Segment(nameof(ShowContext.CastGroups), "Cast groups"))
                await context.CastGroups.Include(cg => cg.Members).LoadAsync();
            castGroupsViewSource.Source = context.CastGroups.Local.ToObservableCollection();
            using (loading.Segment(nameof(ShowContext.Tags) + nameof(A.Tag.Members), "Tags"))
                await context.Tags.Include(cg => cg.Members).LoadAsync();
            tagsViewSource.Source = context.Tags.Local.ToObservableCollection();
            using (loading.Segment(nameof(ShowContext.Applicants), "Applicants"))
                await context.Applicants.LoadAsync();
            castNumbersViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Applicant.CastNumber)));
            allApplicantsViewSource.Source = castNumbersViewSource.Source = context.Applicants.Local.ToObservableCollection();
            TriggerCastNumbersRefresh();
            using (loading.Segment(nameof(ShowContext.Requirements), "Requirements"))
                await context.Requirements.LoadAsync();
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
            using var processing = new LoadingOverlay(this).AsSegment(nameof(selectCastButton_Click),"Processing...");
            var alternative_casts = context.AlternativeCasts.Local.ToArray();
            using (processing.Segment(nameof(ICastingEngine.SelectCastGroups), "Selecting applicants"))
                engine.SelectCastGroups(context.Applicants.Local, context.CastGroups.Local, (uint)alternative_casts.Length);
            using (processing.Segment(nameof(ICastingEngine.BalanceAlternativeCasts), "Balancing alternating casts"))
                engine.BalanceAlternativeCasts(context.Applicants.Local, alternative_casts, Enumerable.Empty<SameCastSet>());
            using (processing.Segment(nameof(ICastingEngine.AllocateCastNumbers), "Allocating cast numbers"))
                engine.AllocateCastNumbers(context.Applicants.Local, alternative_casts, context.ShowRoot.CastNumberOrderBy, context.ShowRoot.CastNumberOrderDirection);
            using (processing.Segment(nameof(ICastingEngine.ApplyTags), "Applying tags"))
                engine.ApplyTags(context.Applicants.Local, context.Tags.Local);
            TriggerCastNumbersRefresh();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedApplicantsViewSource.Source is IList list)
                foreach (var item in availableList.SelectedItems)
                {
                    if (!list.Contains(item))
                        list.Add(item);
                    if (selectionList.SelectedItem is CastGroup cast_group)
                        ((Applicant)item).CastGroup = cast_group;
                }
            ConfigureAllApplicantsFiltering();
        }

        private void addAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedApplicantsViewSource.Source is IList list)
                foreach (var item in availableList.Items)
                {
                    if (!list.Contains(item))
                        list.Add(item);
                    if (selectionList.SelectedItem is CastGroup cast_group)
                        ((Applicant)item).CastGroup = cast_group;
                }
            ConfigureAllApplicantsFiltering();
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedApplicantsViewSource.Source is IList list)
                foreach (var item in selectedList.SelectedItems.OfType<object>().ToList())
                {
                    list.Remove(item);
                    if (selectionList.SelectedItem is CastGroup)
                        ((Applicant)item).CastGroup = null;
                }
            ConfigureAllApplicantsFiltering();
        }

        private void removeAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedApplicantsViewSource.Source is IList list)
                foreach (var item in selectedList.Items.OfType<object>().ToList())
                {
                    list.Remove(item);
                    if (selectionList.SelectedItem is CastGroup)
                        ((Applicant)item).CastGroup = null;
                }
            ConfigureAllApplicantsFiltering();
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
                ConfigureAllApplicantsFiltering();
            }
            else if (selectionList.SelectedItem is Tag tag)
            {
                numbersPanel.Visibility = Visibility.Collapsed;
                selectedApplicantsViewSource.Source = tag.Members;
                selectionPanel.Visibility = Visibility.Visible;
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
                    (A.Tag, CastStatus.Available) => o => o is Applicant a && !selected_applicants.Contains(a),
                    (CastGroup cg, CastStatus.Eligible) => o => o is Applicant a && !selected_applicants.Contains(a) && cg.Requirements.All(r => r.IsSatisfiedBy(a)),
                    (Tag t, CastStatus.Eligible) => o => o is Applicant a && !selected_applicants.Contains(a) && t.Requirements.All(r => r.IsSatisfiedBy(a)),
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
