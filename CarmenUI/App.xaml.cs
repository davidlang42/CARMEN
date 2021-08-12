﻿using CarmenUI.ViewModels;
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
                SetDefaultUserSettings();
        }

        public void SetDefaultUserSettings()
        {
            var settings = CarmenUI.Properties.Settings.Default;
            settings.FirstRun = false;
            settings.Width = 1024;
            settings.Height = 768;
            settings.Left = (SystemParameters.PrimaryScreenWidth - settings.Width) / 2;
            settings.Top = (SystemParameters.PrimaryScreenHeight - settings.Height) / 2;
            settings.WindowState = WindowState.Normal;
            settings.RecentShows = new List<RecentShow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
#if !DEBUG
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
#endif
        }

#if !DEBUG
        private void HandleUnhandledException(Exception ex, string handler)
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}\\Logs";
            Directory.CreateDirectory(path);
            var filename = $"{path}\\Error_{DateTime.Now:yyyy-MM-dd_HH-mm-ss_fffffff}.log";
            using (var file = File.CreateText(filename))
                file.WriteLine($"Handler: {handler}\n{ExceptionString(ex)}");
            MessageBox.Show($"Error in {handler}: {ex.Message}");
        }

        private string ExceptionString(Exception ex)
        {
            var str = ex.ToString();
            if (ex.Data is IDictionary data && data.Count > 0)
            {
                str += "\nData:";
                foreach (var key in data.Keys)
                    str += $"\n{key}={data[key]}";
            }
            if (ex.InnerException is Exception inner)
                str += $"\nInner:\n{ExceptionString(inner)}";
            return str;
        }
#endif
    }
}
