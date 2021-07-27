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
    public partial class MainMenu : Page
    {
        ShowContext context;
        DbContextOptions<ShowContext> contextOptions;

        public MainMenu(ShowContext context, DbContextOptions<ShowContext> context_options)
        {
            this.context = context;
            this.contextOptions = context_options;
            InitializeComponent();
        }

        /// <summary>Handles return events from sub-pages (e==null for cancel,
        /// otherwise e.Result indicates which objects may have changed)</summary>
        private void HandleChangesOnReturn(object sender, ReturnEventArgs<DataObjects> e)
        {
            if (e?.Result is DataObjects changes)
            {
                //TODO handle changed objects
                MessageBox.Show($"The following objects have changed: {changes}");
            }
            //TODO save context at the other end
        }

        private void NavigateToSubPage(SubPage sub_page)
        {
            sub_page.Return += HandleChangesOnReturn;
            this.NavigationService.Navigate(sub_page);
        }

        private void ConfigureShow_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateToSubPage(new ConfigureShow(contextOptions));

        private void AllocateRoles_MouseUp(object sender, MouseButtonEventArgs e)
        {
            NavigateToSubPage(new AllocateRoles(contextOptions));
        }

        private void ConfigureItems_MouseUp(object sender, MouseButtonEventArgs e)
        {
            NavigateToSubPage(new ConfigureItems(contextOptions));
        }

        private void SelectCast_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateToSubPage(new SelectCast(contextOptions));

        private void AuditionApplicants_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateToSubPage(new EditApplicants(contextOptions));

        private void RegisterApplicants_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateToSubPage(new EditApplicants(contextOptions));

        private void ConfigureShow_MouseEnter(object sender, MouseEventArgs e)
            => ShowOneChild(SummaryPanel, ConfigureShowSummary);

        private void RegisterApplicants_MouseEnter(object sender, MouseEventArgs e)
            => ShowOneChild(SummaryPanel, RegisterApplicantsSummary);

        private void AuditionApplicants_MouseEnter(object sender, MouseEventArgs e)
            => ShowOneChild(SummaryPanel, AuditionApplicantsSummary);

        private void SelectCast_MouseEnter(object sender, MouseEventArgs e)
            => ShowOneChild(SummaryPanel, SelectCastSummary);

        private void ConfigureItems_MouseEnter(object sender, MouseEventArgs e)
            => ShowOneChild(SummaryPanel, ConfigureItemsSummary);

        private void AllocateRoles_MouseEnter(object sender, MouseEventArgs e)
            => ShowOneChild(SummaryPanel, AllocateRolesSummary);

        private void ShowOneChild(StackPanel panel, UIElement visible_child)
        {
            foreach (UIElement child in panel.Children)
                child.Visibility = child == visible_child ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
