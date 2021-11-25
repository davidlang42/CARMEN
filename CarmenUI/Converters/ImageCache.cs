using Carmen.ShowModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CarmenUI.Converters
{
    /// <summary>
    /// A converter which takes an ImageId and converts it to a filesystem path containing the cached image.
    /// If the image is not yet cached, 
    /// </summary>
    public class ImageCache : IValueConverter
    {
        public string? CachePath { get; set; }
        public Func<int, Image>? LoadImage { get; set; }

        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int image_id)
                return "";
            if (CachePath == null)
                throw new ApplicationException($"{nameof(CachePath)} not set");
            var filename = $"{CachePath}{image_id}.jpg";
            if (!File.Exists(filename))
            {
                if (!Directory.Exists(CachePath))
                    Directory.CreateDirectory(CachePath);
                if (LoadImage == null)
                    throw new ApplicationException($"{nameof(LoadImage)} not set");
                var image = LoadImage(image_id);
                File.WriteAllBytes(filename, image.ImageData);
            }
            return filename;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
