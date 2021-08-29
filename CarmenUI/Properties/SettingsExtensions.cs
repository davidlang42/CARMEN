using CarmenUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CarmenUI.Properties
{
    internal static class SettingsExtensions
    {
        public static void SetDefaultWindowPosition(this Settings settings)
        {
            settings.Width = 1024;
            settings.Height = 768;
            settings.Left = (SystemParameters.PrimaryScreenWidth - settings.Width) / 2;
            settings.Top = (SystemParameters.PrimaryScreenHeight - settings.Height) / 2;
            settings.WindowState = WindowState.Normal;
        }

        public static void ClearRecentShowsList(this Settings settings)
        {
            settings.RecentShows = new List<RecentShow>();
        }
    }
}
