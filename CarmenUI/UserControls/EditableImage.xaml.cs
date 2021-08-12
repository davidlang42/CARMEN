using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CarmenUI.UserControls
{
    /// <summary>
    /// Interaction logic for EditableImage.xaml
    /// </summary>
    public partial class EditableImage : UserControl
    {
        public static readonly DependencyProperty ImageBytesProperty = DependencyProperty.Register(
           nameof(ImageBytes), typeof(IList<byte>), typeof(EditableImage), new PropertyMetadata(null));

        public IList<byte>? ImageBytes
        {
            get => (IList<byte>?)GetValue(ImageBytesProperty);
            set => SetValue(ImageBytesProperty, value);
        }

        public EditableImage()
        {
            InitializeComponent();
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
            => editPencil.Visibility = Visibility.Visible;

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
            => editPencil.Visibility = Visibility.Hidden;

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void UploadImage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PasteImage_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
