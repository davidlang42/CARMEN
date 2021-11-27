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
    /// Interaction logic for ApplicantPickerDialog.xaml
    /// </summary>
    public partial class ApplicantPickerDialog : Window
    {
        public Applicant? SelectedApplicant { get; private set; }

        public ApplicantPickerDialog(Applicant[] applicants, string match_first_name, string match_last_name)
        {
            InitializeComponent();
        }
    }
}
