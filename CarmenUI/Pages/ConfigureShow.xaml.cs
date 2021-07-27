using Microsoft.EntityFrameworkCore;
using ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for PageFunction1.xaml
    /// </summary>
    public partial class ConfigureShow : SubPage
    {
        private CollectionViewSource criteriasViewSource;
        private CollectionViewSource castGroupsViewSource;
        private CollectionViewSource alternativeCastsViewSource;
        private CollectionViewSource tagsViewSource;
        private CollectionViewSource sectionTypesViewSource;
        private CollectionViewSource requirementsViewSource;

        public ConfigureShow(DbContextOptions<ShowContext> context_options) : base(context_options)
        {
            InitializeComponent();
            criteriasViewSource = (CollectionViewSource)FindResource(nameof(criteriasViewSource));
            castGroupsViewSource = (CollectionViewSource)FindResource(nameof(castGroupsViewSource));
            alternativeCastsViewSource = (CollectionViewSource)FindResource(nameof(alternativeCastsViewSource));
            tagsViewSource = (CollectionViewSource)FindResource(nameof(tagsViewSource));
            sectionTypesViewSource = (CollectionViewSource)FindResource(nameof(sectionTypesViewSource));
            requirementsViewSource = (CollectionViewSource)FindResource(nameof(requirementsViewSource));
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await PopulateView(criteriasViewSource, c => c.Criterias);
            await PopulateView(castGroupsViewSource, c => c.CastGroups);//TODO ideally load whatever is in view, first
            await PopulateView(alternativeCastsViewSource, c => c.AlternativeCasts);
            await PopulateView(tagsViewSource, c => c.Tags);
            await PopulateView(sectionTypesViewSource, c => c.SectionTypes);
            await PopulateView(requirementsViewSource, c => c.Requirements);
        }

        private async Task PopulateView<T>(CollectionViewSource view, Func<ShowContext,DbSet<T>> db_set_getter) where T : class //TODO this will crash if still running when the page is cancelled, maybe I need to wrap this in a LoadingOverlay afterall
        {
            view.Source = new[] { "Loading..." };
            await Task.Run(() => Thread.Sleep(1000));//TODO remove test code
            view.Source = new[] { "Actually loading..." };//TODO remove test code
            var db_set = await context.ColdLoadAsync(db_set_getter);
            view.Source = db_set.Local.ToObservableCollection();
        }

        private void Criteria_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void CastGroups_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void Tags_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void SectionTypes_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void Requirements_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void Import_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(null);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(DataObjects.AlternativeCasts | DataObjects.CastGroups | DataObjects.Criterias | DataObjects.Images | DataObjects.Requirements | DataObjects.SectionTypes | DataObjects.Tags);
        }

        private void AlternativeCasts_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void ResetToDefaultButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
