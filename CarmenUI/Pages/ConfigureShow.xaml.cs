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
        private CollectionViewSource criteriasViewSource = new();
        private CollectionViewSource castGroupsViewSource = new();
        private CollectionViewSource alternativeCastsViewSource = new();
        private CollectionViewSource tagsViewSource = new();
        private CollectionViewSource sectionTypesViewSource = new();
        private CollectionViewSource requirementsViewSource = new();

        public ConfigureShow(DbContextOptions<ShowContext> context_options) : base(context_options)
        {
            InitializeComponent();
            configList.SelectedIndex = 0; // must be set after InitializeComponent() because it triggers Selected event below
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            int i = 0; //TODO remove test code
            var remaining = new HashSet<CollectionViewSource>()
            {
                criteriasViewSource, castGroupsViewSource, alternativeCastsViewSource, tagsViewSource, sectionTypesViewSource, requirementsViewSource
            };
            CollectionViewSource load_next;
            while (remaining.Count != 0)
            {
                // prioritsing loading whatever is in view
                if (objectList.GetBindingExpression(ItemsControl.ItemsSourceProperty).ParentBinding.Source is CollectionViewSource source_in_view
                    && remaining.Contains(source_in_view))
                    load_next = source_in_view;
                else
                    load_next = remaining.First();
                // remove from remaining list
                remaining.Remove(load_next);
                // handle individually due to strict generic typing
                if (load_next == criteriasViewSource)
                    await PopulateView(criteriasViewSource, c => c.Criterias, i++);
                else if (load_next == castGroupsViewSource)
                    await PopulateView(castGroupsViewSource, c => c.CastGroups, i++);
                else if (load_next == alternativeCastsViewSource)
                    await PopulateView(alternativeCastsViewSource, c => c.AlternativeCasts, i++);
                else if (load_next == tagsViewSource)
                    await PopulateView(tagsViewSource, c => c.Tags, i++);
                else if (load_next == sectionTypesViewSource)
                    await PopulateView(sectionTypesViewSource, c => c.SectionTypes, i++);
                else if (load_next == requirementsViewSource)
                    await PopulateView(requirementsViewSource, c => c.Requirements, i++);
                else
                    throw new ApplicationException("View source not implemented.");
            }
        }

        private async Task PopulateView<T>(CollectionViewSource view, Func<ShowContext,DbSet<T>> db_set_getter, int i) where T : class //TODO this will crash if still running when the page is cancelled, maybe I need to wrap this in a LoadingOverlay afterall
        {
            view.Source = new[] { $"Sleeping #{i}" };
            await Task.Run(() => Thread.Sleep(1000));//TODO remove test code
            view.Source = new[] { $"Loading #{i}" };//TODO remove test code
            var db_set = await context.ColdLoadAsync(db_set_getter);
            //TODO view.Source = db_set.Local.ToObservableCollection();
            view.Source = new[] { $"Loaded #{i}" };//TODO remove test code
        }

        private void BindObjectList(string header, CollectionViewSource view_source)
        {
            objectHeading.Text = header;
            objectList.SetBinding(ItemsControl.ItemsSourceProperty, new Binding { Source = view_source });
            //TODO also change the buttons somehow, probably populate a list with types, and use a uniform grid
        }

        private void Criteria_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Audition Criteria", criteriasViewSource);

        private void CastGroups_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Cast Groups", castGroupsViewSource);

        private void AlternativeCasts_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Alternative Casts", alternativeCastsViewSource);

        private void Tags_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Tags", tagsViewSource);

        private void SectionTypes_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Section Types", sectionTypesViewSource);

        private void Requirements_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Role Requirements", requirementsViewSource);

        private void Import_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Import Settings", castGroupsViewSource);//TODO

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(null);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(DataObjects.AlternativeCasts | DataObjects.CastGroups | DataObjects.Criterias | DataObjects.Images | DataObjects.Requirements | DataObjects.SectionTypes | DataObjects.Tags);
        }

        private void ResetToDefaultButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
