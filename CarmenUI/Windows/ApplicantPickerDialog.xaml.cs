using Carmen.ShowModel.Applicants;
using CarmenUI.Converters;
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
    /// Interaction logic for ApplicantPickerDialog.xaml
    /// </summary>
    public partial class ApplicantPickerDialog : Window
    {
        readonly CollectionViewSource applicantsViewSource;

        public Applicant? SelectedApplicant { get; private set; }

        public ApplicantPickerDialog(Applicant[] applicants, string match_first_name, string match_last_name)
        {
            InitializeComponent();
            applicantsViewSource = (CollectionViewSource)FindResource(nameof(applicantsViewSource));
            applicantsViewSource.Source = applicants;
            foreach (var sd in Properties.Settings.Default.FullNameFormat.ToSortDescriptions())
                if (!applicantsViewSource.View.SortDescriptions.Contains(sd))
                    applicantsViewSource.View.SortDescriptions.Add(sd);
            foreach (var applicant in applicants)
                if (applicant.FirstName.Equals(match_first_name, StringComparison.OrdinalIgnoreCase) && applicant.LastName.Equals(match_last_name, StringComparison.OrdinalIgnoreCase))
                {
                    ApplicantsList.SelectedItem = applicant;
                    break;
                }
        }

        private void ApplicantsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedApplicant = ApplicantsList.SelectedItem as Applicant;
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedApplicant == null)
                MessageBox.Show("Please select an applicant to import.");
            else
                this.DialogResult = true;
        }
    }
}
