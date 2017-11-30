using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using Microsoft.DwayneNeed.Media.Imaging;

namespace WebPad.Utilities
{
    public class BitmapUtilities
    {
        public static ImageSource CreateTransparentBitmap(string iconUri)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(iconUri);
            bitmap.EndInit();
            /*var addTransparency = new ColorKeyBitmap {Source = bitmap};
            return addTransparency;*/
            return bitmap;
        }
    }
}