using Carmen.CastingEngine.Audition;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using CarmenUI.Converters;
using CarmenUI.ViewModels;
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
    /// Interaction logic for ApplicantDetailsWindow.xaml
    /// </summary>
    public partial class ApplicantDetailsWindow : Window
    {
        ShowConnection connection;

        readonly CollectionViewSource criteriasViewSource;
        readonly OverallAbilityCalculator overallAbilityCalculator;

        public bool IsClosed { get; private set; } = false;

        public ApplicantDetailsWindow(ShowConnection connection, Criteria[] criterias, IAuditionEngine audition_engine, ApplicantForRole applicant)
        {
            Title = WindowTitleFor(applicant.Applicant);
            this.connection = connection;
            InitializeComponent();
            criteriasViewSource = (CollectionViewSource)FindResource(nameof(criteriasViewSource));
            criteriasViewSource.Source = criterias;
            overallAbilityCalculator = (OverallAbilityCalculator)FindResource(nameof(overallAbilityCalculator));
            overallAbilityCalculator.AuditionEngine = audition_engine;
            this.DataContext = applicant;
        }

        private static string WindowTitleFor(Applicant a)
        {
            var s = $"{a.FirstName} {a.LastName}";
            if (a.CastNumberAndCast is string cast_number)
                s = $"#{cast_number} " + s;
            if (a.CastGroup is CastGroup cg)
                s += $" ({cg.Abbreviation})";
            else if (a.Gender.HasValue && a.DateOfBirth.HasValue)
                s += $" ({a.Age}{a.Gender.Value.ToChar()})";
            return s;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            IsClosed = true;
        }
    }
}
