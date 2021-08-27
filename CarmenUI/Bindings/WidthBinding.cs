using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CarmenUI.Bindings
{
    /// <summary>
    /// A two-way binding to the width properties.
    /// </summary>
    public class WidthBinding : Binding
    {
        public WidthBinding()
        {
            Initialise();
        }

        public WidthBinding(string path)
            : base(path)
        {
            Initialise();
        }

        private void Initialise()
        {
            Source = Properties.Widths.Default;
            Mode = BindingMode.TwoWay;
        }
    }
}
