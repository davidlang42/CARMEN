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
    /// Interaction logic for PageFunction1.xaml
    /// </summary>
    public partial class ConfigureShow : SubPage
    {
        public ConfigureShow(DbContextOptions<ShowContext> context_options) : base(context_options)
        {
            InitializeComponent();
        }

        private void Criteria_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void CastGroups_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void Tags_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void SectionTypes_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void Requirements_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void Import_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(null);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            OnReturn(DataObjects.AlternativeCasts | DataObjects.CastGroups | DataObjects.Criterias | DataObjects.Images | DataObjects.Requirements | DataObjects.SectionTypes | DataObjects.Tags);
        }

        private void AlternativeCasts_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void ResetToDefaultButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
