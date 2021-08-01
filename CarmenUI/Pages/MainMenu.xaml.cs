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
        public ApplicantsSummary ApplicantsSummary { get; init; } = new();
        public AuditionSummary AuditionSummary { get; init; } = new();
        public CastSummary CastSummary { get; init; } = new();
        public ItemsSummary ItemsSummary { get; init; } = new();
        public RolesSummary RolesSummary { get; init; } = new();

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
                MessageBox.Show($"The following objects have changed: {changes}");
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
            => NavigateToSubPage(new EditApplicants(contextOptions, true));

        private void RegisterApplicants_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateToSubPage(new EditApplicants(contextOptions, false));

        private void ConfigureShow_MouseEnter(object sender, MouseEventArgs e)
            => SummaryPanel.DataContext = ShowSummary;

        private void RegisterApplicants_MouseEnter(object sender, MouseEventArgs e)
            => SummaryPanel.DataContext = ApplicantsSummary;

        private void AuditionApplicants_MouseEnter(object sender, MouseEventArgs e)
            => SummaryPanel.DataContext = AuditionSummary;

        private void SelectCast_MouseEnter(object sender, MouseEventArgs e)
            => SummaryPanel.DataContext = CastSummary;

        private void ConfigureItems_MouseEnter(object sender, MouseEventArgs e)
            => SummaryPanel.DataContext = ItemsSummary;

        private void AllocateRoles_MouseEnter(object sender, MouseEventArgs e)
            => SummaryPanel.DataContext = RolesSummary;

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //TODO load based on view priority, abstract from ConfigureShow
            await ShowSummary.LoadAsync(context);
            await ApplicantsSummary.LoadAsync(context);
            await AuditionSummary.LoadAsync(context);
            await CastSummary.LoadAsync(context);
            await ItemsSummary.LoadAsync(context);
            await RolesSummary.LoadAsync(context);
        }
    }
}
