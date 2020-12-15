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
                var slot = type switch
                {
                    TextureType.Diffuse => material.TextureDiffuse,
                    TextureType.Normal => material.TextureNormal,
                    TextureType.Specular => material.TextureSpecular,
                    _ => throw new NotSupportedException(),
                };
                
                var texture = scene.Textures.Select((embeddedTexture, index) => new{embeddedTexture, index})
                    .FirstOrDefault(t => t.index == slot.TextureIndex);
                if (slot.TextureType != Assimp.TextureType.None || texture == null)
                {
                    return GetDefaultTexture(type);
                }

                var bitmapImage = _loader.FromStream(new MemoryStream(texture.embeddedTexture.CompressedData));

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