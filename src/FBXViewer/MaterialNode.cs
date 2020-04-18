using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assimp;

namespace FBXViewer
{
    public class MaterialNode : BaseNode
    {
        private readonly Material _material;

        public MaterialNode(Material material)
        {
            _material = material;
        }

        public override string Text => _material.Name;
        public override bool HasChildren => true;
        protected override IEnumerable<INode> CreateChildren()
        {

            var textures = new[]
            {
                (_material.TextureDiffuse, "Diffuse"),
                (_material.TextureAmbient, "Ambient"),
                (_material.TextureDisplacement, "Displacement"),
                (_material.TextureEmissive, "Emissive"),
                (_material.TextureHeight, "Height"),
                (_material.TextureNormal, "Normal"),
                (_material.TextureOpacity, "Opacity"),
                (_material.TextureReflection, "Reflection"),
                (_material.TextureSpecular, "Specular"),
                (_material.TextureAmbientOcclusion, "Ambient Occlusion"),
            };
            foreach (var texture in textures)
            {
                yield return new TextureSlotNode(texture.Item1, texture.Item2);
            }
            
            foreach (var node in _material.PrimitiveProperties())
            {
                yield return node;
            }
        }
    }
}