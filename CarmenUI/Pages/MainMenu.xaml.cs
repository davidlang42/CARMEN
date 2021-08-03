using CarmenUI.ViewModels;
using CarmenUI.Windows;
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
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page, IDisposable
    {
        DbContextOptions<ShowContext> contextOptions;

        private ShowContext? _context;
        private ShowContext context => _context
            ?? throw new ApplicationException("Tried to use context after it was disposed.");

        public ShowSummary ShowSummary { get; init; } = new();
        public RegistrationSummary RegistrationSummary { get; init; } = new();
        public AuditionSummary AuditionSummary { get; init; } = new();
        public CastSummary CastSummary { get; init; } = new();
        public ItemsSummary ItemsSummary { get; init; } = new();
        public RolesSummary RolesSummary { get; init; } = new();

        private Summary[] allSummaries => new Summary[] { ShowSummary, RegistrationSummary, AuditionSummary, CastSummary, ItemsSummary, RolesSummary };

        public MainMenu(DbContextOptions<ShowContext> context_options)
        {
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
                LoadSummariesAsync(changes);
        }

        private void NavigateToSubPage(SubPage sub_page)
        {
            sub_page.Return += HandleChangesOnReturn;
            this.NavigationService.Navigate(sub_page);
        }

        private void ConfigureShow_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateToSubPage(new ConfigureShow(contextOptions));

        private void AllocateRoles_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateToSubPage(new AllocateRoles(contextOptions));

        private void ConfigureItems_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateToSubPage(new ConfigureItems(contextOptions));

        private void SelectCast_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateToSubPage(new SelectCast(contextOptions));

        private void AuditionApplicants_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateToSubPage(new EditApplicants(contextOptions, false, true, false));

        private void RegisterApplicants_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateToSubPage(new EditApplicants(contextOptions, true, true, true));

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
            => LoadSummariesAsync(DataObjects.All);

        private async void LoadSummariesAsync(DataObjects changes)
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
            if (summaries.Any())
                CastingComplete.Visibility = Visibility.Hidden;
            foreach (var summary in summaries)
                summary.Status = ProcessStatus.Loading;
            // Update them sequentially
            foreach (var summary in summaries)
                await summary.LoadAsync(context);
            // Update complete text
            if (allSummaries.All(s => s.Status == ProcessStatus.Complete))
                CastingComplete.Visibility = Visibility.Visible;
        }
    }
}
