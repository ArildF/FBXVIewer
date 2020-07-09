using System.Drawing;
using System.IO;
using System.Xml;

namespace FBXViewer
{
    public class BitmapTextureLoader : ITextureLoader<Bitmap>
    {
        public Bitmap FromPath(string path)
        {
            return (Bitmap) Image.FromFile(path);
        }

        public Bitmap FromStream(Stream stream)
        {
            return (Bitmap) Image.FromStream(stream);
        }

        public Bitmap FromColor(in Color color)
        {
            var bitmap = new Bitmap(2, 2);
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    bitmap.SetPixel(x, y, color);
                }
                
            }
            return bitmap;
        }
    }
}