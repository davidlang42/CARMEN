using CarmenUI.ViewModels;
using Carmen.ShowModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Text.Json;
using CarmenUI.Properties;
using System.Diagnostics.CodeAnalysis;
using CarmenUI.Windows;
using Serilog;
using Microsoft.Extensions.Logging;

namespace CarmenUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Settings.Default.Save();
            Timings.Default.Save();
            Widths.Default.Save();
            Imports.Default.Save();
            Log.Information("Exit");
            Log.CloseAndFlush();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var settings = Settings.Default;
            if (settings.FirstRun)
            {
                settings.FirstRun = false;
                settings.SetDefaultWindowPosition();
                settings.ClearRecentShowsList();
                settings.ClearReportDefinitionsList();
            }
            ConfigureLogging();
        }

        public void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File("CARMEN_.log", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
                .CreateLogger();
            Log.Information("Launch");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
#if !DEBUG
            AttachExceptionHandlers();
#endif
        }

        private void AttachExceptionHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                HandleUnhandledException((Exception)e.ExceptionObject, "AppDomain");
            DispatcherUnhandledException += (s, e) =>
            {
                HandleUnhandledException(e.Exception, "Dispatcher");
                e.Handled = true;
            };
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                HandleUnhandledException(e.Exception, "TaskScheduler");
                e.SetObserved();
            };
        }

        private void HandleUnhandledException(Exception ex, string handler)
        {
            Log.Error(ex, $"Unhandled exception in {handler}");
            if (ex is UserException user)
                MessageBox.Show(user.Message, "CARMEN");
            else
            {
                MessageBox.Show($"Error in {handler}: {ex.Message}", "CARMEN");
                bool main_window_found = false;
                foreach (var obj in Current.Windows)
                {
                    if (obj is MainWindow main_window)
                    {
                        Log.Information($"Returning to {nameof(MainWindow)}");
                        main_window_found = true;
                        main_window.Show();
                        main_window.NavigateToMainMenu();
                    }
                    else if (obj is Window window)
                    {
                        Log.Information($"Closing {obj.GetType().Name} '{window.Title}'");
                        window.Close();
                    }
                }
                if (!main_window_found)
                {
                    Log.Information($"Relaunching {nameof(StartWindow)}");
                    var start_window = new StartWindow();
                    start_window.Show();
                }
            }
        }
    }
}
