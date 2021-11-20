using Carmen.ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Import
{
    class ImageImportColumn : ImportColumn
    {
        public override bool MatchExisting
        {
            get => base.MatchExisting;
            set { }
        }

        public ImageImportColumn(string name, Action<Applicant, Image> setter, Func<string, byte[]> load_image)
            : base(name, (a, s) => SetImage(s, a, load_image, setter), (a, s) => CompareImage(a.Photo, s, load_image))
        {
            base.MatchExisting = false;
        }

        private static void SetImage(string filename, Applicant applicant, Func<string, byte[]> load_image, Action<Applicant, Image> setter)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return; // nothing to do (don't clear image if no image is supplied)
            var image = new Image
            {
                Name = filename,
                ImageData = load_image(filename)
            };
            setter(applicant, image);
        }

        private static bool CompareImage(Image? existing, string filename, Func<string, byte[]> load_image)
        {
            if (string.IsNullOrWhiteSpace(filename) && existing == null)
                return true;
            if (string.IsNullOrWhiteSpace(filename) || existing == null)
                return false;
            var new_data = load_image(filename);
            var existing_data = existing.ImageData;
            if (new_data.Length != existing_data.Length)
                return false;
            for (var i = 0; i < new_data.Length; i++)
                if (new_data[i] != existing_data[i])
                    return false;
            return true;
        }
    }
}
