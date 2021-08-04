using CarmenUI.Converters;
using CarmenUI.ViewModels;
using CarmenUI.Windows;
using Microsoft.EntityFrameworkCore;
using ShowModel;
using ShowModel.Applicants;
using ShowModel.Criterias;
using ShowModel.Requirements;
using ShowModel.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for ConfigureShow.xaml
    /// </summary>
    public partial class ConfigureShow : SubPage
    {
        private readonly CollectionViewSource criteriasViewSource; // xaml resource loaded in constructor
        private readonly CollectionViewSource castGroupsViewSource; // xaml resource loaded in constructor
        private readonly CollectionViewSource alternativeCastsViewSource; // xaml resource loaded in constructor
        private readonly CollectionViewSource tagsViewSource; // xaml resource loaded in constructor
        private readonly CollectionViewSource sectionTypesViewSource = new() { SortDescriptions = { StandardSort.For<SectionType>() } };
        private readonly CollectionViewSource requirementsViewSource = new() { SortDescriptions = { StandardSort.For<Requirement>() } };
        private readonly CollectionViewSource requirementsSelectionSource; // xaml resource loaded in constructor

        private CollectionViewSource? currentViewSource;

        public ConfigureShow(DbContextOptions<ShowContext> context_options) : base(context_options)
        {
            InitializeComponent();
            alternativeCastsViewSource = (CollectionViewSource)FindResource(nameof(alternativeCastsViewSource));
            alternativeCastsViewSource.SortDescriptions.Add(StandardSort.For<AlternativeCast>());
            criteriasViewSource = (CollectionViewSource)FindResource(nameof(criteriasViewSource));
            criteriasViewSource.SortDescriptions.Add(StandardSort.For<Criteria>());
            castGroupsViewSource = (CollectionViewSource)FindResource(nameof(castGroupsViewSource));
            castGroupsViewSource.SortDescriptions.Add(StandardSort.For<CastGroup>());
            tagsViewSource = (CollectionViewSource)FindResource(nameof(tagsViewSource));
            tagsViewSource.SortDescriptions.Add(StandardSort.For<Tag>());
            requirementsSelectionSource = (CollectionViewSource)FindResource(nameof(requirementsSelectionSource));
            requirementsSelectionSource.SortDescriptions.Add(StandardSort.For<Requirement>());
            configList.SelectedIndex = 0; // must be set after InitializeComponent() because it triggers Selected event below
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            using var loading = new LoadingOverlay(this);
            loading.Progress = 0;
            await context.Requirements.LoadAsync();
            requirementsViewSource.Source = requirementsSelectionSource.Source = context.Requirements.Local.ToObservableCollection();
            loading.Progress = 17;
            await context.Criterias.LoadAsync();
            criteriasViewSource.Source = context.Criterias.Local.ToObservableCollection();
            loading.Progress = 34;
            await context.CastGroups.Include(cg => cg.Requirements).LoadAsync();
            castGroupsViewSource.Source = context.CastGroups.Local.ToObservableCollection();
            loading.Progress = 51;
            await context.AlternativeCasts.LoadAsync();
            alternativeCastsViewSource.Source = context.AlternativeCasts.Local.ToObservableCollection();
            loading.Progress = 68;
            await context.Tags.Include(t => t.Requirements).LoadAsync();
            tagsViewSource.Source = context.Tags.Local.ToObservableCollection();
            loading.Progress = 77;
            await context.SectionTypes.LoadAsync();
            sectionTypesViewSource.Source = context.SectionTypes.Local.ToObservableCollection();
            loading.Progress = 100;
        }

        private void BindObjectList(string header, string tool_tip, CollectionViewSource view_source, AddableObject[][] add_buttons)
        {
            objectHeading.Text = header;
            objectList.ToolTip = tool_tip;
            objectList.SetBinding(ItemsControl.ItemsSourceProperty, new Binding { Source = view_source });
            currentViewSource = view_source;
            objectAddButtons.ItemsSource = add_buttons;
        }

        readonly AddableObject[][] criteriasButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Numeric Criteria", () => new NumericCriteria { Name = "New Numeric Criteria" }),
                new AddableObject("Add Tick Criteria", () => new BooleanCriteria { Name = "New Tick Criteria" }),
                new AddableObject("Add List Criteria", () => new SelectCriteria { Name = "New List Criteria" })
            }
        };

        private void Criteria_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Audition Criteria", "Drag to re-order", criteriasViewSource, criteriasButtons);

        readonly AddableObject[][] castGroupsButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Cast Group", () => new CastGroup { Name = "New Cast Group" })
            }
        };

        private void CastGroups_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Cast Groups", "Drag to re-order", castGroupsViewSource, castGroupsButtons);


        readonly AddableObject[][] alternativeCastsButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Alternative Cast", () => new AlternativeCast { Name = "New Alternative Cast" })
            }
        };

        private void AlternativeCasts_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Alternative Casts", "Sorted by name", alternativeCastsViewSource, alternativeCastsButtons);

        readonly AddableObject[][] tagsButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Tag", () => new Tag { Name = "New Tag" })
            }
        };

        private void Tags_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Tags", "Sorted by name", tagsViewSource, tagsButtons);

        readonly AddableObject[][] sectionTypesButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Section Type", () => new SectionType { Name = "New Section Type" })
            }
        };

        private void SectionTypes_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Section Types", "Sorted by name", sectionTypesViewSource, sectionTypesButtons);

        readonly AddableObject[][] requirementsButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Age", () => new AgeRequirement { Name = "New Age Requirement" }),
                new AddableObject("Add Gender", () => new GenderRequirement { Name = "New Gender Requirement" }),
                new AddableObject("Add Tag", () => new TagRequirement { Name = "New Tag Requirement" }),
                new AddableObject("Add Exact Ability", () => new AbilityExactRequirement { Name = "New Ability Requirement (Exact)" }),
                new AddableObject("Add Ability Range", () => new AbilityRangeRequirement { Name = "New Ability Requirement (Range)" })
            },
            new[]
            {
                new AddableObject("Add 'AND'", () => new AndRequirement { Name = "New AND Requirement" }),
                new AddableObject("Add 'OR'", () => new OrRequirement { Name = "New OR Requirement" }),
                new AddableObject("Add 'XOR'", () => new XorRequirement { Name = "New XOR Requirement" }),
                new AddableObject("Add 'NOT'", () => new NotRequirement { Name = "New NOT Requirement" })
            }
        };

        private void Requirements_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Role Requirements", "Drag to re-order", requirementsViewSource, requirementsButtons);

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (CancelChanges())
                OnReturn(null);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveChanges())
                //LATER only flag changes on objects which actually changed, rather than all possible changes on this form
                OnReturn(DataObjects.AlternativeCasts | DataObjects.CastGroups | DataObjects.Criterias | DataObjects.Images | DataObjects.Requirements | DataObjects.SectionTypes | DataObjects.Tags);
        }

        private void AddObjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentViewSource?.Source is IList list && !list.IsFixedSize)
            {
                var button = (Button)sender;
                var addable = (AddableObject)button.DataContext;
                var new_object = addable.CreateObject();
                if (new_object is IOrdered ordered)
                    ordered.Order = list.OfType<IOrdered>().NextOrder();
                list.Add(new_object);
                objectList.SelectedItem = new_object;
            }
        }

        private void Name_LostFocus(object sender, RoutedEventArgs e)
        {
            currentViewSource?.View.Refresh();
        }
    }
}
