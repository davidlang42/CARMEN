using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
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
using Image = Carmen.ShowModel.Image;

namespace CarmenUI.UserControls
{
    /// <summary>
    /// Interaction logic for ApplicantImage.xaml
    /// </summary>
    public partial class ApplicantImage : UserControl
    {
        public static readonly DependencyProperty ApplicantObjectProperty = DependencyProperty.Register(
           nameof(ApplicantObject), typeof(Applicant), typeof(ApplicantImage), new PropertyMetadata(null, OnApplicantObjectChanged));

        private static void OnApplicantObjectChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            => ((ApplicantImage)sender).UpdateImage(); //TODO really should add change handler for Applicant.Photo rather than call UpdateImage() after each change

        public Applicant? ApplicantObject
        {
            get => (Applicant?)GetValue(ApplicantObjectProperty);
            set => SetValue(ApplicantObjectProperty, value);
        }

        public ApplicantImage()
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

        private void UpdateImage()
        {
            if (ApplicantObject == null)
                ImageControl.Source = null;
            else if (ApplicantObject.PhotoImageId != null)
                ImageControl.Source = CachedImage(ApplicantObject.PhotoImageId.Value, ApplicantObject.ShowRoot, ApplicantObject.Photo);
            else if (ApplicantObject.Photo != null)
                ImageControl.Source = ActualImage(ApplicantObject.Photo);
            else
                ImageControl.Source = null;
        }

        private ImageSource CachedImage(int image_id, ShowRoot show, Image? lazy_loading_photo)
        {
            var cache_path = CachePath(show);
            var filename = $"{cache_path}{image_id}.jpg";
            if (!File.Exists(filename))
            {
                if (!Directory.Exists(cache_path))
                    Directory.CreateDirectory(cache_path);
                if (lazy_loading_photo == null)
                    throw new ApplicationException("Applicant photo not set, but photo ID was.");
                File.WriteAllBytes(filename, lazy_loading_photo.ImageData);
            }
            return new BitmapImage(new Uri(filename));
        }

        public static string CachePath(ShowRoot show_root)
            => Path.GetTempPath() + Path.DirectorySeparatorChar + string.Concat(show_root.Name.Split(Path.GetInvalidFileNameChars())) + Path.DirectorySeparatorChar; //TODO public?

        private ImageSource? ActualImage(Image photo)
        {
            using (var stream = new MemoryStream(photo.ImageData))
            {
                try
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                    image.Freeze();
                    return image;
                }
                catch
                {
                    // invalid image data
                }
            }
            return null;
        }

        private void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicantObject == null)
                return;
            var dialog = new OpenFileDialog
            {
                Title = "Upload Image",
                Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp|All Files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                ApplicantObject.Photo = new Image
                {
                    Name = Path.GetFileName(dialog.FileName),
                    ImageData = File.ReadAllBytes(dialog.FileName)
                };
                UpdateImage();
            }
        }

        private void PasteImage_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicantObject == null)
                return;
            if (Clipboard.GetImage() is BitmapSource source)
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                using var stream = new MemoryStream();
                encoder.Save(stream);
                ApplicantObject.Photo = new Image
                {
                    Name = $"Pasted at {DateTime.Now:yyyy-MM-dd HH:mm}",
                    ImageData = stream.ToArray()
                };
                UpdateImage();
            }
        }

        private void ClearImage_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicantObject == null)
                return;
            ApplicantObject.Photo = null;
            UpdateImage();
        }
    }
}
