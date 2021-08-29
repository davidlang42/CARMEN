using Carmen.ShowModel.Applicants;
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
using System.Windows.Shapes;

namespace CarmenUI.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public Applicant ExampleApplicant { get; init; } = new Applicant {
            FirstName = "David",
            LastName = "Lang"
        };

        public SettingsWindow()
        {
            Save(); // save on load so that if we cancel, we reload the settings as they were when we opened
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Reload();//TODO handle closing event and do the same as cancel if nothing was pressed
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Save();
            this.Close();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void Save()
        {
            Properties.Settings.Default.Save();
        }

        private void Reload()
        {
            Properties.Settings.Default.Reload();
        }

        private void Reset()
        {
            Properties.Settings.Default.Reset();
        }

        private void FullNameFormatCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (fullNameExample != null)
            {
                // force update of example text
                var data_context = fullNameExample.DataContext;
                fullNameExample.DataContext = null;
                fullNameExample.DataContext = data_context;
            }
        }
    }
}
