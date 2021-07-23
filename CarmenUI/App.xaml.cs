using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CarmenUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            CarmenUI.Properties.Settings.Default.Save();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (CarmenUI.Properties.Settings.Default.FirstRun)
                SetDefaultSettings();
        }

        public void SetDefaultSettings()
        {
            var settings = CarmenUI.Properties.Settings.Default;
            settings.FirstRun = false;
            settings.Width = 1024;
            settings.Height = 768;
            settings.Left = (SystemParameters.PrimaryScreenWidth - settings.Width) / 2;
            settings.Top = (SystemParameters.PrimaryScreenHeight - settings.Height) / 2;
            settings.WindowState = WindowState.Normal;
        }
    }
}
