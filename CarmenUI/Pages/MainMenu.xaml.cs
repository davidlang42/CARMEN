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
        public MainMenu()
        {
            InitializeComponent();
        }

        private void ConfigureShow_Selected(object sender, RoutedEventArgs e)
            => this.NavigationService.Navigate(new Uri("/Pages/ConfigureShow.xaml", UriKind.Relative));

        private void ConfigureShow_MouseEnter(object sender, MouseEventArgs e)
        {
            UpdateSummaryPanel("Summary panel for configuring the show.");
        }

        private void RegisterApplicants_Selected(object sender, RoutedEventArgs e)
            => this.NavigationService.Navigate(new Uri("/Pages/Applicants.xaml", UriKind.Relative));

        private void RegisterApplicants_MouseEnter(object sender, MouseEventArgs e)
        {
            UpdateSummaryPanel("Summary panel for registering applicants.");
        }

        private void AuditionApplicants_Selected(object sender, RoutedEventArgs e)
            => this.NavigationService.Navigate(new Uri("/Pages/Applicants.xaml", UriKind.Relative));

        private void AuditionApplicants_MouseEnter(object sender, MouseEventArgs e)
        {
            UpdateSummaryPanel("Summary panel for auditioning applicants.");
        }

        private void SelectCast_Selected(object sender, RoutedEventArgs e)
            => this.NavigationService.Navigate(new Uri("/Pages/SelectCast.xaml", UriKind.Relative));

        private void SelectCast_MouseEnter(object sender, MouseEventArgs e)
        {
            UpdateSummaryPanel("Summary panel for selecting cast.");
        }

        private void ConfigureItems_Selected(object sender, RoutedEventArgs e)
            => this.NavigationService.Navigate(new Uri("/Pages/ConfigureItems.xaml", UriKind.Relative));

        private void ConfigureItems_MouseEnter(object sender, MouseEventArgs e)
        {
            UpdateSummaryPanel("Summary panel for configuring items and roles.");
        }

        private void AllocateRoles_Selected(object sender, RoutedEventArgs e)
            => this.NavigationService.Navigate(new Uri("/Pages/AllocateRoles.xaml", UriKind.Relative));

        private void AllocateRoles_MouseEnter(object sender, MouseEventArgs e)
        {
            UpdateSummaryPanel("Summary panel for allocating roles.");
        }

        private void UpdateSummaryPanel(string text)
        {
            SummaryPanel.Children.Clear();
            SummaryPanel.Children.Add(new TextBlock
            {
                Text = text
            });
        }
    }
}
