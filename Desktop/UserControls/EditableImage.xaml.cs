using Carmen.ShowModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using Image = Carmen.ShowModel.Image;

namespace Carmen.Desktop.UserControls
{
    /// <summary>
    /// Interaction logic for EditableImage.xaml
    /// </summary>
    public partial class EditableImage : UserControl
    {
        public static readonly DependencyProperty ImageBytesProperty = DependencyProperty.Register(
           nameof(ImageObject), typeof(Image), typeof(EditableImage), new PropertyMetadata(null));

        public event ImageChangedEventHandler? ImageChanged = null;

        public Image? ImageObject
        {
            get => (Image?)GetValue(ImageBytesProperty);
            set => SetValue(ImageBytesProperty, value);
        }

        public EditableImage()
        {
            InitializeComponent();
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
            => overlay.Visibility = Visibility.Visible;

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
            => overlay.Visibility = Visibility.Hidden;

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                UploadImage_Click(sender, e);
        }

        private void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Upload Image",
                Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp|All Files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                var old_image = ImageObject;
                ImageObject = new Image
                {
                    Name = System.IO.Path.GetFileName(dialog.FileName),
                    ImageData = UserException.Handle(() => File.ReadAllBytes(dialog.FileName), "Error uploading image.")
                };
                ImageChanged?.Invoke(this, new ImageChangedEventArgs(old_image, ImageObject));
            }
        }

        private void PasteImage_Click(object sender, RoutedEventArgs e)
        {
            if (Clipboard.GetImage() is BitmapSource source)
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                using var stream = new MemoryStream();
                encoder.Save(stream);
                var old_image = ImageObject;
                ImageObject = new Image
                {
                    Name = $"Pasted at {DateTime.Now:yyyy-MM-dd HH:mm}.png",
                    ImageData = stream.ToArray()
                };
                ImageChanged?.Invoke(this, new ImageChangedEventArgs(old_image, ImageObject));
            }
        }

        private void ClearImage_Click(object sender, RoutedEventArgs e)
        {
            var old_image = ImageObject;
            ImageObject = null;
            ImageChanged?.Invoke(this, new ImageChangedEventArgs(old_image, ImageObject));
        }
    }
}
