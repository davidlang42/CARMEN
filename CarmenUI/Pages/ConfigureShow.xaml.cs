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
    public partial class ConfigureShow : PageFunction<bool>
    {
        public ConfigureShow()
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
            OnReturn(new ReturnEventArgs<bool>(true));
        }
    }
}
