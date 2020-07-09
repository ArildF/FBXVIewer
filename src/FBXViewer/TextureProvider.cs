using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Assimp;

namespace FBXViewer
{
    public class TextureProvider<TBitmap> where TBitmap : class
    {
        // private readonly List<Scene> _scenes = new List<Scene>();

        private readonly TextureSearcher _textureSearcher;
        private readonly ITextureLoader<TBitmap> _loader;
        private readonly SceneContext _sceneContext;
        

        public TextureProvider(TextureSearcher textureSearcher, ITextureLoader<TBitmap> loader, SceneContext sceneContext)
        {
            _textureSearcher = textureSearcher;
            _loader = loader;
            _sceneContext = sceneContext;
        }

        public TBitmap? GetTexture(Mesh mesh, TextureType type)
        {
            var scene = _sceneContext.CurrentScene;
            if (scene == null)
            {
                return null;
            }
           
            if (mesh.MaterialIndex < scene.MaterialCount)
            {
                var material = scene.Materials[mesh.MaterialIndex];
                var path = type switch
                {
                    TextureType.Diffuse => material.TextureDiffuse.FilePath,
                    TextureType.Normal => material.TextureNormal.FilePath,
                    TextureType.Specular => material.TextureSpecular.FilePath,
                    _ => throw new NotSupportedException(),
                };
                var texture = scene.Textures.FirstOrDefault(
                    t => t.Filename == path);
                if (texture == null)
                {
                    return GetDefaultTexture(type);
                }

                var bitmapImage = _loader.FromStream(new MemoryStream(texture.CompressedData));

                return bitmapImage;
            }

            return null;
        }

        private TBitmap? GetDefaultTexture(TextureType type)
        {
            var color = type switch
            {
                TextureType.Diffuse => Color.Fuchsia,
                TextureType.Normal => Color.Blue,
                TextureType.Specular => Color.FromArgb(255, 255 / 3, 255 / 3, 255 / 3),
                _ => throw new ArgumentException($"Unsupported texture type: {type}", nameof(type)),
            };
            return _loader.FromColor(color);
        }
    }
}