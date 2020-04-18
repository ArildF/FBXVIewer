using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Assimp;

namespace FBXViewer
{
    public class TextureNode :BaseNode 
    {
        private readonly EmbeddedTexture _texture;

        public TextureNode(EmbeddedTexture texture)
        {
            _texture = texture;
        }
        
        public override object GetPreview()
        {
            if (!_texture.HasCompressedData)
            {
                return false;
            }
            var elt = new Image();
            
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(_texture.CompressedData);
            bitmapImage.EndInit();

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