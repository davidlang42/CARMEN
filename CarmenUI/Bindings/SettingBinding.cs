using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CarmenUI.Bindings
{
    /// <summary>
    /// A two-way binding to the settings properties.
    /// SOURCE: https://thomaslevesque.com/2008/11/18/wpf-binding-to-application-settings-using-a-markup-extension/
    /// </summary>
    public class SettingBinding : Binding
    {
        public SettingBinding()
        {
            Initialize();
        }

        public SettingBinding(string path)
            : base(path)
        {
            Initialize();
        }

        private void Initialize()
        {
            Source = Properties.Settings.Default;
            Mode = BindingMode.TwoWay;
        }
    }
}
