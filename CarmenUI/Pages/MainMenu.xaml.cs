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

        /// <summary>Handles return events from sub-pages.
        /// e==null for cancel, e.Result==true when context save is required, e.Result==false when no save is required</summary>
        private void SaveOnReturn(object sender, ReturnEventArgs<bool> e)
        {
            if (e?.Result == true)
                MessageBox.Show("Save required."); //TODO save context, probably with saving/loading dialog
        }

        private void NavigateAndSaveOnReturn<T>() where T : PageFunction<bool>, new()
        {
            var page_function = new T();
            page_function.Return += SaveOnReturn;
            this.NavigationService.Navigate(page_function);
        }

        private void ConfigureShow_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateAndSaveOnReturn<ConfigureShow>();

        private void AllocateRoles_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateAndSaveOnReturn<AllocateRoles>();

        private void ConfigureItems_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateAndSaveOnReturn<ConfigureItems>();

        private void SelectCast_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateAndSaveOnReturn<SelectCast>();

        private void AuditionApplicants_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateAndSaveOnReturn<EditApplicants>();

        private void RegisterApplicants_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateAndSaveOnReturn<EditApplicants>();

        private void ConfigureShow_MouseEnter(object sender, MouseEventArgs e)
        {
            UpdateSummaryPanel("Summary panel for configuring the show.");
        }

        private void RegisterApplicants_MouseEnter(object sender, MouseEventArgs e)
        {
            UpdateSummaryPanel("Summary panel for registering applicants.");
        }

        private void AuditionApplicants_MouseEnter(object sender, MouseEventArgs e)
        {
            UpdateSummaryPanel("Summary panel for auditioning applicants.");
        }

        private void SelectCast_MouseEnter(object sender, MouseEventArgs e)
        {
            UpdateSummaryPanel("Summary panel for selecting cast.");
        }

        private void ConfigureItems_MouseEnter(object sender, MouseEventArgs e)
        {
            UpdateSummaryPanel("Summary panel for configuring items and roles.");
        }

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
