using Carmen.CastingEngine.Allocation;
using Carmen.CastingEngine.Audition;
using Carmen.CastingEngine.Selection;
using Carmen.ShowModel.Applicants;
using CarmenUI.Properties;
using CarmenUI.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace CarmenUI.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        bool closing = false;

        public Applicant ExampleApplicant { get; init; } = new Applicant {
            FirstName = "David",
            LastName = "Lang"
        };

        public string ApplicationVersion
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                return (assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
                    assembly.GetName().Version) switch
                {
                    (string info, Version v) => $"v{info} (build {v.Build} revision {v.Revision})",
                    (_, Version v) => $"v{v.Major}.{v.Minor}.{v.Build}.{v.Revision}",
                    (string info, _) => $"v{info}",
                    _ => "unknown"
                };
            }
        }

        public string[] AuditionEngines => AuditionEngine.Implementations.Select(t => t.Name).ToArray();
        public string[] SelectionEngines => SelectionEngine.Implementations.Select(t => t.Name).ToArray();
        public string[] AllocationEngines => AllocationEngine.Implementations.Select(t => t.Name).ToArray();

        public string SettingsPath => ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;

        public string DefaultImageCachePath => ApplicantImage.DefaultImageCachePath;

        public SettingsWindow()
        {
            Save(); // save on load so that if we cancel, we reload the settings as they were when we opened
            InitializeComponent();
        }

        private void OpenSettingsPath_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", @$"/select,""{SettingsPath}""");
        }

        private void ReloadSettings_Click(object sender, RoutedEventArgs e)
        {
            Reload();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Reload();
            closing = true;
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Save();
            closing = true;
            this.Close();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetAndSave();
            closing = true;
            this.Close();
        }

        private void Save()
        {
            Settings.Default.Save();
            Widths.Default.Save();
            Timings.Default.Save();
            Imports.Default.Save();
        }

        private void Reload()
        {
            Settings.Default.Reload();
            Widths.Default.Reload();
            Timings.Default.Reload();
            Imports.Default.Reload();
            var settings = Settings.Default;
            if (settings.FirstRun)
            {
                settings.FirstRun = false;
                settings.SetDefaultWindowPosition();
                settings.ClearRecentShowsList();
                settings.ClearReportDefinitionsList();
                Widths.Default.ClearAllocateRolesGrid();
                Timings.Default.ClearTimings();
                Imports.Default.ClearImportSettings();
            }
        }

        private void ResetAndSave()
        {
            Settings.Default.Reset();
            Settings.Default.SetDefaultWindowPosition();
            Settings.Default.ClearRecentShowsList();
            Settings.Default.ClearReportDefinitionsList();
            Widths.Default.Reset();
            Widths.Default.ClearAllocateRolesGrid();
            Timings.Default.Reset();
            Timings.Default.ClearTimings();
            Imports.Default.Reset();
            Imports.Default.ClearImportSettings();
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

        private void ResetWindowPositionButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.SetDefaultWindowPosition();
        }

        private void ClearRecentShowsButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ClearRecentShowsList();
        }

        private void ClearReportDefinitionsButton_Click(object sender, RoutedEventArgs e)//TODO
        {
            Settings.Default.ClearReportDefinitionsList();
        }

        private void ClearLoadingTimesButton_Click(object sender, RoutedEventArgs e)
        {
            Timings.Default.ClearTimings();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!closing)
                Reload(); // equivalent to cancel
        }

        private void ClearAllocateRolesWidthsButton_Click(object sender, RoutedEventArgs e)
        {
            Widths.Default.ClearAllocateRolesGrid();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void ClearImageCacheButton_Click(object sender, RoutedEventArgs e)
        {
            var image_cache_path = Settings.Default.ImageCachePath;
            if (string.IsNullOrEmpty(image_cache_path))
                image_cache_path = DefaultImageCachePath;
            if (!Directory.Exists(image_cache_path))
            {
                MessageBox.Show($"Directory '{image_cache_path}' does not exist.", Title);
                return;
            }    
            if (MessageBox.Show($"Are you sure you wanted to clear all cached images in '{image_cache_path}'?", Title, MessageBoxButton.YesNo) == MessageBoxResult.No)
                return;
            Directory.Delete(image_cache_path, true); //TODO bug: file handle still open to last viewed image in EditApplicants
            MessageBox.Show("Image cache cleared", Title);
        }
    }
}
