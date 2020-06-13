using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Assimp;

namespace FBXViewer
{
    public class TextureProvider<TBitmap> where TBitmap : class
    {
        // private readonly List<Scene> _scenes = new List<Scene>();

        private readonly TextureSearcher _textureSearcher;
        private readonly ITextureLoader<TBitmap> _loader;
        private readonly SceneContext _sceneContext;
        
        private readonly List<string> _searchDirectories = new List<string>();

        public TextureProvider(TextureSearcher textureSearcher, ITextureLoader<TBitmap> loader, SceneContext sceneContext)
        {
            _textureSearcher = textureSearcher;
            _loader = loader;
            _sceneContext = sceneContext;
        }

        public TBitmap? GetDiffuseTexture(Mesh mesh)
        {
            var scene = _sceneContext.CurrentScene;
            if (scene == null)
            {
                return null;
            }
            if (mesh.MaterialIndex < scene.MaterialCount)
            {
                var material = scene.Materials[mesh.MaterialIndex];
                var texture = scene.Textures.FirstOrDefault(
                    t => t.Filename == material.TextureDiffuse.FilePath);
                if (texture == null)
                {
                    return TryLoadFromDisk(material);
                }

                var bitmapImage = _loader.FromStream(new MemoryStream(texture.CompressedData));

                return bitmapImage;
            }

            return null;
        }

        private TBitmap? TryLoadFromDisk(Material material)
        {
            return DoTryLoadFromDisk(material).FirstOrDefault();
        }
        private IEnumerable<TBitmap> DoTryLoadFromDisk(Material material)
        {
            TBitmap? LoadIfExists(string path)
            {
                if (File.Exists(path))
                {
                    return _loader.FromPath(path);
                }

                return null;
            }

            if (String.IsNullOrEmpty(material.TextureDiffuse.FilePath))
            {
                yield break;
            }

            var source = LoadIfExists(material.TextureDiffuse.FilePath);
            if (source != null)
            {
                yield return source;
            }

            var textureFileName = Path.GetFileName(material.TextureDiffuse.FilePath);

            var fbxDirectory = Path.GetDirectoryName(textureFileName);

            if (fbxDirectory != null)
            {
                source = LoadIfExists(Path.Combine(fbxDirectory, textureFileName));
                if (source != null)
                {
                    yield return source;
                }
            }

            foreach (var searchDirectory in _searchDirectories)
            {
                source = LoadIfExists(Path.Combine(searchDirectory, textureFileName));
                if (source != null)
                {
                    yield return source;
                }
            }

            var searchPath = _textureSearcher.Search(textureFileName);
            source = LoadIfExists(searchPath);
            if (source != null)
            {
                var directory = Path.GetDirectoryName(searchPath);
                if (directory != null)
                {
                    _searchDirectories.Add(directory);
                    yield return source;
                }
            }
        }
    }
}