using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CarmenUI.Windows
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window, IDisposable
    {
        public LoadingWindow(Window owner)
        {
            Owner = owner;
            owner.IsEnabled = false;
            WinUser.EnableWindow(Owner, false);
            InitializeComponent();
            Show();
        }

        public LoadingWindow(Page page) : this(GetWindow(page))
        { }

        public void Dispose()
        {
            WinUser.EnableWindow(Owner, true);
            Owner.IsEnabled = true;
            Hide();
        }
    }
}
