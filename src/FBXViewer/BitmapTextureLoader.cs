using System.Drawing;
using System.IO;

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
    }
}