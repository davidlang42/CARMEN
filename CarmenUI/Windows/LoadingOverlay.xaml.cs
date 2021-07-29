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
    /// Interaction logic for LoadingOverlay.xaml
    /// </summary>
    public partial class LoadingOverlay : Window, IDisposable
    {
        public static readonly DependencyProperty MainTextProperty = DependencyProperty.Register(
            nameof(MainText), typeof(string), typeof(LoadingOverlay), new PropertyMetadata("Loading..."));

        private IntPtr ownerHwnd;

        public string MainText
        {
            get => (string)GetValue(MainTextProperty);
            set => SetValue(MainTextProperty, value);
        }

        public static readonly DependencyProperty SubTextProperty = DependencyProperty.Register(
            nameof(SubText), typeof(string), typeof(LoadingOverlay), new PropertyMetadata(null));

        public string? SubText
        {
            get => (string?)GetValue(SubTextProperty);
            set
            {
                SetValue(SubTextProperty, value);
                SubTextElement.Visibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
            nameof(Progress), typeof(int?), typeof(LoadingOverlay), new PropertyMetadata(null));

        public int? Progress
        {
            get => (int?)GetValue(ProgressProperty);
            set
            {
                SetValue(ProgressProperty, value);
                ProgressElement.Visibility = value == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public LoadingOverlay(Window owner)
        {
            Owner = owner;
            ownerHwnd = new WindowInteropHelper(owner).Handle;
            if (ownerHwnd == IntPtr.Zero)
                throw new InvalidOperationException("Could not get window handle, wait until window is fully initialised.");
            owner.IsEnabled = false;
            WinUser.EnableWindow(ownerHwnd, false);
            DataContext = this;
            InitializeComponent();
            Show();
        }

        public LoadingOverlay(Page page) : this(GetWindow(page))
        { }

        public void Dispose()
        {
            WinUser.EnableWindow(ownerHwnd, true);
            Owner.IsEnabled = true;
            Hide();
        }
    }
}
