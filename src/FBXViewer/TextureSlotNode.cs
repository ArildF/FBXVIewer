using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Assimp;

namespace FBXViewer
{
    internal class TextureSlotNode :BaseNode 
    {
        private readonly TextureSlot _textureSlot;
        private readonly string _typeName;

        public TextureSlotNode(TextureSlot textureSlot, string typeName)
        {
            _textureSlot = textureSlot;
            _typeName = typeName;
        }

        public override string Text => _typeName;
        public override bool HasChildren => true;

        public override object? GetPreview()
        {
            if (!File.Exists(_textureSlot.FilePath))
            {
                return null;
            }
            var elt = new Image();
            elt.Source = new BitmapImage(new Uri(_textureSlot.FilePath));

            return elt;
        }

        protected override IEnumerable<INode> CreateChildren()
        {
            foreach (var node in _textureSlot.PrimitiveProperties())
            {
                yield return node;
            }
        }
    }
}