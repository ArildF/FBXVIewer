using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace FBXViewer
{
    public class BitmapSourceTextureLoader : ITextureLoader<BitmapSource>
    {
        public BitmapSource FromPath(string path)
        {
            return new BitmapImage(new Uri(path));
        }

        public BitmapSource FromStream(Stream stream)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();

            return bitmapImage;
        }
    }
}