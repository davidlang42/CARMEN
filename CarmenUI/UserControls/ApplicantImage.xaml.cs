using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Structure;
using CarmenUI.Windows;
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

        public static string DefaultImageCachePath => Path.GetTempPath() + "CarmenImageCache";

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
            var cache_path = GetCachePath(show);
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

        public static string GetCachePath(ShowRoot show_root) //TODO public?
        {
            var root_path = Properties.Settings.Default.ImageCachePath;
            if (string.IsNullOrEmpty(root_path))
                root_path = DefaultImageCachePath;
            if (!root_path.EndsWith(Path.DirectorySeparatorChar))
                root_path += Path.DirectorySeparatorChar;
            return root_path + string.Concat(show_root.Name.Split(Path.GetInvalidFileNameChars())) + Path.DirectorySeparatorChar;
        }

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

        private void FixOrientation_Click(object sender, RoutedEventArgs e)
        {
            if (ApplicantObject == null)
                return;
            //TODO loading overlay
            // load the original image
            if (ApplicantObject.Photo is not Image original_image)
                return;
            using var original_stream = new MemoryStream(original_image.ImageData);
            // find the correct rotation from metadata
            if (BitmapFrame.Create(original_stream).Metadata is not BitmapMetadata metadata)
                return;
            var query = "System.Photo.Orientation";
            if (!metadata.ContainsQuery(query))
                return;
            var rotation = (metadata.GetQuery("System.Photo.Orientation") as ushort?) switch
            {
                1 => Rotation.Rotate0,
                3 => Rotation.Rotate180,
                6 => Rotation.Rotate90,
                8 => Rotation.Rotate270,
                _ => throw new ApplicationException($"Invalid orientation: {metadata.GetQuery("System.Photo.Orientation")}")
            };
            // create image with correct rotation
            var corrected = new BitmapImage();
            corrected.BeginInit();
            corrected.CacheOption = BitmapCacheOption.OnLoad;
            original_stream.Seek(0, SeekOrigin.Begin);
            corrected.StreamSource = original_stream;
            corrected.Rotation = rotation;
            corrected.EndInit();
            corrected.Freeze();
            // re-encode as png
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(corrected));
            using var corrected_stream = new MemoryStream();
            encoder.Save(corrected_stream);
            // update the applicant
            ApplicantObject.Photo = new Image
            {
                Name = original_image.Name,
                ImageData = corrected_stream.ToArray()
            };
            UpdateImage();
        }
    }
}
