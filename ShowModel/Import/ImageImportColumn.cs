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

        public ImageImportColumn(string name, Action<Applicant, Image?> setter, Func<string, byte[]?> load_image)
            : base(name, (a, s) => setter(a, MakeImage(s, load_image)), (a, s) => false)
        {
            base.MatchExisting = false;
        }

        private static Image? MakeImage(string filename, Func<string, byte[]?> load_image)
        {
            var data = load_image(filename);
            if (data == null)
                return null;
            return new Image
            {
                Name = filename,
                ImageData = data
            };
        }
    }
}
