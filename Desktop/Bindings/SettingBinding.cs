using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Carmen.Desktop.Bindings
{
    /// <summary>
    /// A two-way binding to the settings properties.
    /// </summary>
    public class SettingBinding : Binding
    {
        public SettingBinding()
        {
            Initialise();
        }

        public SettingBinding(string path)
            : base(path)
        {
            Initialise();
        }

        private void Initialise()
        {
            Source = Properties.Settings.Default;
            Mode = BindingMode.TwoWay;
        }
    }
}
