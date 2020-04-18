using System.Collections.Generic;
using System.Reflection;
using Assimp;

namespace FBXViewer
{
    public class MeshNode : BaseNode
    {
        private readonly Mesh _mesh;

        public MeshNode(Mesh mesh)
        {
            _mesh = mesh;
        }

        public override object GetPreviewThumbnail()
        {
            return null;
        }

        public override object GetPreview()
        {
            var preview = new ModelPreview(_mesh);
            return preview.Element;
        }

        public override string Text => $"Mesh '{_mesh.Name}'";
        public override bool HasChildren => true;
        protected override IEnumerable<INode> CreateChildren()
        {
            foreach (var node in _mesh.PrimitiveProperties())
            {
                yield return node;
            }
        }
    }
}