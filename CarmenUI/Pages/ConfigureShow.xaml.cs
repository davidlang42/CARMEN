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
                if (objectList.GetBindingExpression(ItemsControl.ItemsSourceProperty)?.ParentBinding.Source is CollectionViewSource source_in_view
                    && tasks.ContainsKey(source_in_view))
                    next_source = source_in_view;
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

        private void BindObjectList(string header, CollectionViewSource view_source, AddableObject[][] add_buttons)
        {
            objectHeading.Text = header;
            objectList.SetBinding(ItemsControl.ItemsSourceProperty, new Binding { Source = view_source });
            objectAddButtons.ItemsSource = add_buttons;
        }

        static readonly AddableObject[][] criteriasButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Numeric Criteria", () => new NumericCriteria { Name = "New Numeric Criteria" }),
                new AddableObject("Add Tick Criteria", () => new BooleanCriteria { Name = "New Tick Criteria" }),
                new AddableObject("Add List Criteria", () => new SelectCriteria { Name = "New List Criteria" })
            }
        };

        private void Criteria_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Audition Criteria", criteriasViewSource, criteriasButtons);

        static readonly AddableObject[][] castGroupsButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Cast Group", () => new CastGroup { Name = "New Cast Group" })
            }
        };

        private void CastGroups_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Cast Groups", castGroupsViewSource, castGroupsButtons);


        static readonly AddableObject[][] alternativeCastsButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Alternative Cast", () => new AlternativeCast { Name = "New Alternative Cast" })
            }
        };

        private void AlternativeCasts_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Alternative Casts", alternativeCastsViewSource, alternativeCastsButtons);

        static readonly AddableObject[][] tagsButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Tag", () => new Tag { Name = "New Tag" })
            }
        };

        private void Tags_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Tags", tagsViewSource, tagsButtons);

        static readonly AddableObject[][] sectionTypesButtons = new[]
        {
            new[]
            {
                new AddableObject("Add Section Type", () => new SectionType { Name = "New Section Type" })
            }
        };

        private void SectionTypes_Selected(object sender, RoutedEventArgs e)
            => BindObjectList("Section Types", sectionTypesViewSource, sectionTypesButtons);

        static readonly AddableObject[][] requirementsButtons = new[]
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
            => BindObjectList("Role Requirements", requirementsViewSource, requirementsButtons);

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
            OnReturn(DataObjects.AlternativeCasts | DataObjects.CastGroups | DataObjects.Criterias | DataObjects.Images | DataObjects.Requirements | DataObjects.SectionTypes | DataObjects.Tags);
        }

        private void ResetToDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO implement reset to default
        }

        private void AddObjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (objectList.GetBindingExpression(ItemsControl.ItemsSourceProperty)?.ParentBinding.Source is CollectionViewSource view_source
                && view_source.Source is IList list)
            {
                var button = (Button)sender;
                var addable = (AddableObject)button.DataContext;
                var new_object = addable.CreateObject();
                list.Add(new_object);
            }
        }
    }
}
