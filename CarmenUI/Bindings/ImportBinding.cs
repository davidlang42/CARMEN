using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CarmenUI.Bindings
{
    public class ImportBinding : Binding
    {
        public ImportBinding()
        {
            Initialise();
        }

        public ImportBinding(string path)
            : base(path)
        {
            Initialise();
        }

        private void Initialise()
        {
            Source = Properties.Imports.Default;
            Mode = BindingMode.TwoWay;
        }
    }
}
