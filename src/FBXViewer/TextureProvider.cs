using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Assimp;

namespace FBXViewer
{
    public class TextureProvider
    {
        // private readonly List<Scene> _scenes = new List<Scene>();

        private TextureSearcher _textureSearcher;

        private Scene? _currentScene;
        private string? _fileName;
        
        private readonly List<string> _searchDirectories = new List<string>();

        public TextureProvider(TextureSearcher textureSearcher)
        {
            _textureSearcher = textureSearcher;
        }

        public void LoadScene(string filename, Scene scene)
        {
            _fileName = filename;
            _currentScene = scene;
        }
        public ImageSource? GetDiffuseTexture(Mesh mesh)
        {
            if (mesh.MaterialIndex < _currentScene?.MaterialCount)
            {
                var material = _currentScene.Materials[mesh.MaterialIndex];
                var texture = _currentScene.Textures.FirstOrDefault(
                    t => t.Filename == material.TextureDiffuse.FilePath);
                if (texture == null)
                {
                    return TryLoadFromDisk(material);
                }
                
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(texture.CompressedData);
                bitmapImage.EndInit();

                return bitmapImage;
            }

            return null;
        }

        private ImageSource TryLoadFromDisk(Material material)
        {
            return DoTryLoadFromDisk(material).FirstOrDefault();
        }
        private IEnumerable<ImageSource> DoTryLoadFromDisk(Material material)
        {
            ImageSource? LoadIfExists(string path)
            {
                if (File.Exists(path))
                {
                    return new BitmapImage(new Uri(path));
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

            var fbxDirectory = Path.GetDirectoryName(_fileName);

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