using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Carmen.Desktop.Bindings
{
    public class TimingBinding : Binding
    {
        public TimingBinding()
        {
            Initialise();
        }

        public TimingBinding(string path)
            : base(path)
        {
            Initialise();
        }

        private void Initialise()
        {
            Source = Properties.Timings.Default;
            Mode = BindingMode.TwoWay;
        }
    }
}
