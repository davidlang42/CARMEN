using CarmenUI.Converters;
using CarmenUI.ViewModels;
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
        //TODO (LATER) editing panel heading fails to bind when a string (eg. "Loading...") is selected in objectList. The observed behaviour is that the text block is blank, which is the desired behaviour, but binding fails are bad.
        //TODO implement drag to re-order (only if selection is IOrdered)
        //TODO create EditableImage control (for CastGroup.Icon)
        //TODO create CheckBoxList / CheckBoxCombo control (for CastGroup.Requirements)
        //TODO (FUTURE) set CastGroup.Abbreviation when Name changes
        //TODO (FUTURE) set AlternativeCast.Initial when Name changes
        //TODO need to make edit panels for: tags, section types, requirements
        //TODO (LATER) add validation to all edit panel fields
        //TODO disable castgroup.alternatingcasts checkbox if less than 2 alternative casts, with tooltip explaining why
        //TODO handle delete key on objectList to delete selected object, but obey rules about minimums: at least one criteria, at least one cast group, at least 2 alternative casts if any cast group has alternatecasts, at least one section type

        static readonly SortDescription sortByOrder = new SortDescription(nameof(IOrdered.Order), ListSortDirection.Ascending);
        static readonly SortDescription sortByName = new SortDescription(nameof(INamed.Name), ListSortDirection.Ascending);

        private readonly CollectionViewSource criteriasViewSource = new() { SortDescriptions = { sortByOrder } };
        private readonly CollectionViewSource castGroupsViewSource = new() { SortDescriptions = { sortByOrder } };
        private readonly CollectionViewSource alternativeCastsViewSource; // xaml resource loaded in constructor
        private readonly CollectionViewSource tagsViewSource = new() { SortDescriptions = { sortByName } };
        private readonly CollectionViewSource sectionTypesViewSource = new() { SortDescriptions = { sortByName } };
        private readonly CollectionViewSource requirementsViewSource = new() { SortDescriptions = { sortByOrder } };

        private readonly EnumerateCountByGroups enumerateCountByGroups;

        private CollectionViewSource? currentViewSource;

        public ConfigureShow(DbContextOptions<ShowContext> context_options) : base(context_options)
        {
            InitializeComponent();
            enumerateCountByGroups = (EnumerateCountByGroups)FindResource(nameof(enumerateCountByGroups));
            enumerateCountByGroups.CastGroups = castGroupsViewSource;
            alternativeCastsViewSource = (CollectionViewSource)FindResource(nameof(alternativeCastsViewSource));
            alternativeCastsViewSource.SortDescriptions.Add(sortByName);
            configList.SelectedIndex = 0; // must be set after InitializeComponent() because it triggers Selected event below
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // make tasks to load each collection view source
            var tasks = new Dictionary<CollectionViewSource, Task<IList>>()
            {
                { criteriasViewSource, TaskToLoad(context, c => c.Criterias) },
                { castGroupsViewSource, TaskToLoad(context, c => c.CastGroups) },
                { alternativeCastsViewSource, TaskToLoad(context, c => c.AlternativeCasts) },
                { tagsViewSource, TaskToLoad(context, c => c.Tags) },
                { sectionTypesViewSource, TaskToLoad(context, c => c.SectionTypes) },
                { requirementsViewSource, TaskToLoad(context, c => c.Requirements) }
            };
            // initialise all sources with "Loading..."
            var loading = new[] { "Loading..." };
            foreach (var view_source in tasks.Keys)
                view_source.Source = loading;
            // load one at a time because DbContext is not thread safe
            CollectionViewSource next_source;
            Task<IList> next_task;
            while (tasks.Count != 0)
            {
                // prioritise loading whatever is in view
                if (currentViewSource != null && tasks.ContainsKey(currentViewSource))
                    next_source = currentViewSource;
                else
                    next_source = tasks.Keys.First();
                // extract task & remove from list
                next_task = tasks[next_source];
                tasks.Remove(next_source);
                // populate source asynchronously
                next_task.Start();
                next_source.Source = await next_task;
            }
        }

        private static Task<IList> TaskToLoad<T>(ShowContext context, Func<ShowContext, DbSet<T>> db_set_getter) where T : class //TODO this will crash if still running when the page is cancelled, maybe I need to wrap this in a LoadingOverlay afterall
            => new Task<IList>(() =>
            {
                var db_set = db_set_getter(context);
                db_set.Load();
                return db_set.Local.ToObservableCollection();
            });

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

        private void Import_Selected(object sender, RoutedEventArgs e)
        {
            //TODO (FUTURE) import show config from another show
            MessageBox.Show("Importing setting from another show is not yet implemented. Please populate manually.");
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(null);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO save here
            //TODO (FUTURE) only flag changes on objects which actually changed, rather than all possible changes on this form
            OnReturn(DataObjects.AlternativeCasts | DataObjects.CastGroups | DataObjects.Criterias | DataObjects.Images | DataObjects.Requirements | DataObjects.SectionTypes | DataObjects.Tags);
        }

        private void ResetToDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO implement reset to default
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
    }
}
