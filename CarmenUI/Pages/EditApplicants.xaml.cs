using Microsoft.EntityFrameworkCore;
using ShowModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for EditApplicants.xaml
    /// </summary>
    public partial class EditApplicants : SubPage
    {
        public EditApplicants(DbContextOptions<ShowContext> context_options) : base(context_options)
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(null);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveChanges())
                OnReturn(DataObjects.Applicants);
        }

        private void ImportApplicants_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddApplicant_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
