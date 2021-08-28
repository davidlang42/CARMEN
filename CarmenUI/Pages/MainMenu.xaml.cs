using CarmenUI.ViewModels;
using CarmenUI.Windows;
using Microsoft.EntityFrameworkCore;
using Carmen.ShowModel;
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
using Carmen.ShowModel.Structure;

namespace CarmenUI.Pages
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page, IDisposable
    {
        DbContextOptions<ShowContext> contextOptions;
        string defaultShowName;

        private ShowContext? _context;
        private ShowContext context => _context
            ?? throw new ApplicationException("Tried to use context after it was disposed.");

        public ShowSummary ShowSummary { get; init; }
        public RegistrationSummary RegistrationSummary { get; init; } = new();
        public AuditionSummary AuditionSummary { get; init; } = new();
        public CastSummary CastSummary { get; init; } = new();
        public ItemsSummary ItemsSummary { get; init; } = new();
        public RolesSummary RolesSummary { get; init; } = new();

        private Summary[] allSummaries => new Summary[] { ShowSummary, RegistrationSummary, AuditionSummary, CastSummary, ItemsSummary, RolesSummary };

        public MainMenu(DbContextOptions<ShowContext> context_options, string default_show_name)
        {
            defaultShowName = default_show_name;
            ShowSummary = new(default_show_name);
            _context = new ShowContext(context_options);
            contextOptions = context_options;
            InitializeComponent();
        }

        public void Dispose()
        {
            if (_context != null)
            {
                _context.Dispose();
                _context = null;
            }
        }

        /// <summary>Handles return events from sub-pages (e==null for cancel,
        /// otherwise e.Result indicates which objects may have changed)</summary>
        private void HandleChangesOnReturn(object sender, ReturnEventArgs<DataObjects> e)
        {
            if (e?.Result is DataObjects changes)
                InvalidateSummaries(changes);
        }

        private void NavigateToSubPage(SubPage sub_page)
        {
            sub_page.Return += HandleChangesOnReturn;
            this.NavigationService.Navigate(sub_page);
        }

        private void menuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ListViewItem selected) {
                if (selected == ConfigureShow)
                    NavigateToSubPage(new ConfigureShow(contextOptions, defaultShowName));
                else if (selected == AllocateRoles)
                    NavigateToSubPage(new AllocateRoles(contextOptions));
                else if (selected == ConfigureItems)
                    NavigateToSubPage(new ConfigureItems(contextOptions));
                else if (selected == SelectCast)
                    NavigateToSubPage(new SelectCast(contextOptions));
                else if (selected == AuditionApplicants)
                    NavigateToSubPage(new EditApplicants(contextOptions, false, true, false));
                else if (selected == RegisterApplicants)
                    NavigateToSubPage(new EditApplicants(contextOptions, true, true, true));
                menuList.SelectedItem = null;
            }
        }

        private void ConfigureShow_MouseEnter(object sender, MouseEventArgs e)
            => SummaryPanel.Content = ShowSummary;

        private void RegisterApplicants_MouseEnter(object sender, MouseEventArgs e)
            => SummaryPanel.Content = RegistrationSummary;

        private void AuditionApplicants_MouseEnter(object sender, MouseEventArgs e)
            => SummaryPanel.Content = AuditionSummary;

        private void SelectCast_MouseEnter(object sender, MouseEventArgs e)
            => SummaryPanel.Content = CastSummary;

        private void ConfigureItems_MouseEnter(object sender, MouseEventArgs e)
            => SummaryPanel.Content = ItemsSummary;

        private void AllocateRoles_MouseEnter(object sender, MouseEventArgs e)
            => SummaryPanel.Content = RolesSummary;

        private void Page_Initialized(object sender, EventArgs e)
            => InvalidateSummaries(DataObjects.All);

        private void InvalidateSummaries(DataObjects changes)//TODO (SUMMARY) force update of objects?
        {
            // Determine which summaries need updating
            List<Summary> summaries = new();
            if (changes.HasFlag(DataObjects.Criterias)
                || changes.HasFlag(DataObjects.CastGroups)
                || changes.HasFlag(DataObjects.AlternativeCasts)
                || changes.HasFlag(DataObjects.Tags)
                || changes.HasFlag(DataObjects.SectionTypes)
                || changes.HasFlag(DataObjects.Requirements))
                summaries.Add(ShowSummary);
            if (changes.HasFlag(DataObjects.Applicants)
                || changes.HasFlag(DataObjects.CastGroups)
                || changes.HasFlag(DataObjects.Requirements)
                || changes.HasFlag(DataObjects.Criterias))
                summaries.AddRange(new Summary[] { RegistrationSummary, AuditionSummary });
            if (changes.HasFlag(DataObjects.CastGroups)
                || changes.HasFlag(DataObjects.Tags))
                summaries.Add(CastSummary);
            if (changes.HasFlag(DataObjects.Nodes)
                || changes.HasFlag(DataObjects.CastGroups)
                || changes.HasFlag(DataObjects.SectionTypes))
                summaries.AddRange(new Summary[] { ItemsSummary, RolesSummary });
            // Mark them as 'Loading'
            if (summaries.Count == 0)
                return; // nothing to do
            CastingComplete.Visibility = Visibility.Hidden;
            foreach (var summary in summaries)
            {
                summary.NeedsUpdate = true;
                summary.Status = ProcessStatus.Loading;
            }
            // Trigger update (does nothing if already running)
            UpdateSummariesAsync();
        }

        bool updating_summaries = false;
        private async void UpdateSummariesAsync()//TODO (SUMMARY) force update of objects?
        {
            if (updating_summaries)
                return; // already running
            updating_summaries = true;
            // Update summaries sequentially
            while (allSummaries.FirstOrDefault(s => s.NeedsUpdate) is Summary next_to_update)
            {
                next_to_update.NeedsUpdate = false;
                await next_to_update.LoadAsync(context);
                if (next_to_update.NeedsUpdate) // may have been set while LoadAsync() was running, eg. if we went to another page and came back
                    next_to_update.Status = ProcessStatus.Loading;
            }  
            // Show complete text if required
            if (allSummaries.All(s => s.Status == ProcessStatus.Complete))
                CastingComplete.Visibility = Visibility.Visible;
            updating_summaries = false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Properties.Settings.Default.ShortcutsOnMainMenu)
            {
                var list_item = e.Key switch
                {
                    Key.S => ConfigureShow,
                    Key.P => RegisterApplicants,
                    Key.A => AuditionApplicants,
                    Key.C => SelectCast,
                    Key.I => ConfigureItems,
                    Key.R => AllocateRoles,
                    _ => null
                };
                if (list_item != null)
                {
                    e.Handled = true;
                    menuList_SelectionChanged(sender, new(e.RoutedEvent, Array.Empty<object>(), new[] { list_item }));
                }
            }
        }

        Window? window;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            window = Window.GetWindow(this);
            window.KeyDown += Window_KeyDown;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (window != null)
                window.KeyDown -= Window_KeyDown;
        }
    }
}
