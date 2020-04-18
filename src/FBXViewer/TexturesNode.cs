using System;
using System.Collections.Generic;
using System.Linq;
using Assimp;

namespace FBXViewer
{
    public class TexturesNode :BaseNode 
    {
        private readonly List<EmbeddedTexture> _textures;
        private readonly Func<EmbeddedTexture, TextureNode> _textureNodeFactory;

        public TexturesNode(List<EmbeddedTexture> sceneTextures, 
            Func<EmbeddedTexture, TextureNode> textureNodeFactory)
        {
            _textures = sceneTextures;
            _textureNodeFactory = textureNodeFactory;
        }

        public override string Text => "Textures";
        public override bool HasChildren => _textures.Any();
        protected override IEnumerable<INode> CreateChildren()
        {
            foreach (var texture in _textures)
            {
                yield return _textureNodeFactory(texture);
            }
        }
    }
}