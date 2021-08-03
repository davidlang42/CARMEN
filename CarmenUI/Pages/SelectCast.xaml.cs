using CarmenUI.Converters;
using CarmenUI.ViewModels;
using Microsoft.EntityFrameworkCore;
using ShowModel;
using ShowModel.Applicants;
using A = ShowModel.Applicants;
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

        public CastNumberModel[] CastNumbers { get; set; }//TODO remove
        public SelectCast(DbContextOptions<ShowContext> context_options) : base(context_options)
        {
            CastNumbers = new CastNumberModel[]
            {
                new AlternateCastMembers
                {
                     CastNumber = 1,
                     TotalCasts = 2,
                     Names = new[] {"Name 1a", "Name 1b"}
                },
                new AlternateCastMembers
                {
                     CastNumber = 2,
                     TotalCasts = 2,
                     Names = new[] {"Name 2a", "Name 2b"}
                },
                new SingleCastMember
                {
                     CastNumber = 3,
                     TotalCasts = 2,
                     Name ="Both cast 1"
                },
                new SingleCastMember
                {
                     CastNumber = 4,
                     TotalCasts = 2,
                     Name ="Both cast 2"
                }
            };
            InitializeComponent();
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
            DataContext = this;//TODO remove
            castStatusCombo.SelectedIndex = 0; // must be here because it triggers event below
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)//TODO perform this loading before showing page, using a loadingoverlay
        {
            // initialise with "Loading..."
            castGroupsViewSource.Source = new[] { "Loading..." };
            tagsViewSource.Source = new[] { "Loading..." };
            alternativeCastsViewSource.Source = new[] { "Loading..." };
            // populate source asynchronously
            await context.AlternativeCasts.LoadAsync();
            alternativeCastsViewSource.Source = context.AlternativeCasts.Local.ToObservableCollection();
            await context.CastGroups.Include(cg => cg.Members).LoadAsync();
            castGroupsViewSource.Source = context.CastGroups.Local.ToObservableCollection();
            await context.Tags.Include(cg => cg.Members).LoadAsync();
            tagsViewSource.Source = context.Tags.Local.ToObservableCollection();
            await context.Applicants.LoadAsync();
            allApplicantsViewSource.Source = context.Applicants.Local.ToObservableCollection();
            await context.Requirements.LoadAsync();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(null);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveChanges())
                OnReturn(DataObjects.Nodes);
        }

        private void selectCastButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO auto select cast
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
            ConfigureFiltering();
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
            ConfigureFiltering();
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
            ConfigureFiltering();
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
            ConfigureFiltering();
        }

        private void availableList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => addButton_Click(sender, e);

        private void selectedList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => removeButton_Click(sender, e);

        private void castStatusCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ConfigureFiltering();

        private void selectionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedApplicantsViewSource.Source = selectionList.SelectedItem switch
            {
                CastGroup cast_group => cast_group.Members,
                Tag tag => tag.Members,
                _ => null
            };
            //TODO show/hide cast numbers/selection UIs
            ConfigureFiltering();
        }

        private void ConfigureFiltering()
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

    public abstract class CastNumberModel
    {
        public int CastNumber { get; set; }
        public int TotalCasts { get; set; }
        public abstract string[] GetNames { get; }
        public int CountNames => GetNames.Length;
        public IEnumerable<TextBlock> TextBlocks
            => GetNames.Select(n => new TextBlock { Text = n });
    }

    public class SingleCastMember : CastNumberModel
    {
        public string Name { get; set; } = "";
        public override string[] GetNames => new[] { Name };

    }

    public class AlternateCastMembers : CastNumberModel
    {
        public string[] Names { get; set; } = new string[0];

        public override string[] GetNames => Names;
    }
}
