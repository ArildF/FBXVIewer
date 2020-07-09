using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

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

        public BitmapSource FromColor(in Color color)
        {
            var bitmapImage = new WriteableBitmap(2, 2, 72, 72, PixelFormats.Bgr32, null);

            try
            {
                bitmapImage.Lock();
                unsafe
                {
                    var backBuffer = bitmapImage.BackBuffer;
                    for (int y = 0; y < 2; y++)
                    {
                        for (int x = 0; x < 2; x++)
                        {
                            var address = backBuffer + y * bitmapImage.BackBufferStride + x * 4;
                            *(int*) address = color.ToArgb();
                        }
                    }
                }

            }
            finally
            {
                bitmapImage.Unlock();
            }

            return bitmapImage;
        }
    }
}