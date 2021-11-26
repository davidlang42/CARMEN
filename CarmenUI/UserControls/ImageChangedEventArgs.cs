using System;
using Image = Carmen.ShowModel.Image;

namespace CarmenUI.UserControls
{
    public class ImageChangedEventArgs : EventArgs
    {
        public Image? OldImage { get; }
        public Image? NewImage { get; }

        public ImageChangedEventArgs(Image? old_image, Image? new_image)
        {
            OldImage = old_image;
            NewImage = new_image;
        }
    }

    public delegate void ImageChangedEventHandler(object sender, ImageChangedEventArgs e);
}
