using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using Assimp;
using OpenGL;
using PixelFormat = OpenGL.PixelFormat;

namespace FBXViewer.OpenGL
{
    public class TextureLoader
    {
        private readonly TextureProvider<Bitmap> _textureProvider;

        public TextureLoader(TextureProvider<Bitmap> textureProvider)
        {
            _textureProvider = textureProvider;
        }

        public Texture LoadDiffuse(Mesh mesh)
        {
            return LoadTexture(mesh, TextureType.Diffuse);
        }

        public Texture LoadNormalMap(Mesh mesh)
        {
            return LoadTexture(mesh, TextureType.Normal);
        }
        
        public Texture LoadSpecularMap(Mesh mesh)
        {
            return LoadTexture(mesh, TextureType.Specular);
        }

        private Texture LoadTexture(Mesh mesh, TextureType type)
        {
            var textureTask = LoadTextureAsync(mesh, type);
            return new Texture(textureTask);
        }

        private async Task<uint?> LoadTextureAsync(Mesh mesh, TextureType type)
        {
            var bitmap = await Task.Run(() => _textureProvider.GetTexture(mesh, type));
            if (bitmap == null)
            {
                return null;
            }
            
            
            uint textureId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2d, textureId);

            if (bitmap.PixelFormat.NotIn(System.Drawing.Imaging.PixelFormat.Format32bppArgb, 
                System.Drawing.Imaging.PixelFormat.Format24bppRgb))
            {
                bitmap = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            }
            
            // bitmap.Save(@$"E:\tmp\textures\{type}.bmp");

            var (internalFormat, pixelFormat) = bitmap.PixelFormat switch
            {
                System.Drawing.Imaging.PixelFormat.Format32bppArgb => (InternalFormat.Rgba, PixelFormat.Bgr),
                System.Drawing.Imaging.PixelFormat.Format24bppRgb => (InternalFormat.Rgb, PixelFormat.Bgr),
                // 4 => (InternalFormat.Rgba, PixelFormat.Rgba),
                // 3 => (InternalFormat.Rgb, PixelFormat.Rgb),
                _ => throw new ArgumentException($"Unsupported pixel format: {bitmap.PixelFormat}", nameof(bitmap))
            };
            var bits = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);
            {
                Gl.TexImage2D(TextureTarget.Texture2d, 0, internalFormat, bitmap.Width, bitmap.Height,
                    0, pixelFormat, PixelType.UnsignedByte, bits.Scan0);
            }

            var error = Gl.GetError();
            if (error != ErrorCode.NoError)
            {
                throw new Exception(error.ToString());
            }

            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, Gl.LINEAR);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, Gl.LINEAR_MIPMAP_LINEAR);
            Gl.GenerateMipmap(TextureTarget.Texture2d);

            return textureId;
        }
    }
}