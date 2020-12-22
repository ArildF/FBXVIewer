using System.Collections.Generic;
using System.IO;
using Assimp;
using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace FBXViewer
{
    public class TextureNode :BaseNode 
    {
        private readonly EmbeddedTexture _texture;

        public TextureNode(EmbeddedTexture texture)
        {
            _texture = texture;
        }
        
        public override object? GetPreview()
        {
            if (!_texture.HasCompressedData)
            {
                return null;
            }
            var elt = new Image();
            
            var bitmapImage = new Bitmap(new MemoryStream(_texture.CompressedData));

            elt.Source = bitmapImage;

            return elt;
        }

        public override string Text => $"Texture '{_texture.Filename}'";
        public override bool HasChildren => true;
        protected override IEnumerable<INode> CreateChildren()
        {
            foreach (var node in _texture.PrimitiveProperties())
            {
                yield return node;
            }
        }
    }
}