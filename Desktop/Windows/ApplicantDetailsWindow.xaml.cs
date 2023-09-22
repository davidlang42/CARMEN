using Carmen.CastingEngine.Audition;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.Desktop.Converters;
using Carmen.Desktop.UserControls;
using Carmen.Desktop.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Carmen.Desktop.Windows
{
    /// <summary>
    /// Interaction logic for ApplicantDetailsWindow.xaml
    /// </summary>
    public partial class ApplicantDetailsWindow : Window
    {
        ShowConnection connection;
        Applicant applicant;

        readonly CollectionViewSource criteriasViewSource;
        readonly OverallAbilityCalculator overallAbilityCalculator;

        public bool IsClosed { get; private set; } = false;

        public ApplicantDetailsWindow(ShowConnection connection, Criteria[] criterias, IAuditionEngine audition_engine, ApplicantForRole applicant_for_role)
        {
            this.connection = connection;
            this.applicant = applicant_for_role.Applicant;
            Title = WindowTitleFor(applicant);
            InitializeComponent();
            criteriasViewSource = (CollectionViewSource)FindResource(nameof(criteriasViewSource));
            criteriasViewSource.Source = criterias;
            overallAbilityCalculator = (OverallAbilityCalculator)FindResource(nameof(overallAbilityCalculator));
            overallAbilityCalculator.AuditionEngine = audition_engine;
            DataContext = applicant_for_role; // must be done after setting up overallAbilityCalculator
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (applicant.PhotoImageId is int photo_id)
                ImageControl.Source = await ApplicantImage.CachedImage(applicant.PhotoImageId.Value, applicant.ShowRoot, LoadApplicantImage);
            else
                ImageControl.Source = null;
        }

        /// <summary>Loads the applicant's photo in a dedicated db context to avoid concurrency issues</summary>
        private Image LoadApplicantImage()
        {
            using var context = ShowContext.Open(connection);
            return context.Images.Single(i => i.ImageId == applicant.PhotoImageId);
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }
    }
}
