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

        public MainMenu(ShowContext context)
        {
            this.context = context;
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
            => NavigateAndSaveOnReturn(new T());

        private void NavigateAndSaveOnReturn(PageFunction<bool> page_function)
        {
            page_function.Return += SaveOnReturn;
            this.NavigationService.Navigate(page_function);
        }

        private void ConfigureShow_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateAndSaveOnReturn<ConfigureShow>();

        private void AllocateRoles_MouseUp(object sender, MouseButtonEventArgs e)
        {
            context.Nodes.Load();//TODO this shouldn't be here
            NavigateAndSaveOnReturn(new AllocateRoles(context.Nodes.Local.ToObservableCollection()));
        }

        private void ConfigureItems_MouseUp(object sender, MouseButtonEventArgs e)
        {
            context.Nodes.Load();//TODO this shouldn't be here
            NavigateAndSaveOnReturn(new ConfigureItems(context.Nodes.Local.ToObservableCollection()));
        }

        private void SelectCast_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateAndSaveOnReturn<SelectCast>();

        private void AuditionApplicants_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateAndSaveOnReturn<EditApplicants>();

        private void RegisterApplicants_MouseUp(object sender, MouseButtonEventArgs e)
            => NavigateAndSaveOnReturn<EditApplicants>();

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
