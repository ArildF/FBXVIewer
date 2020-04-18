using System;
using System.Collections.Generic;
using System.Linq;
using Assimp;

namespace FBXViewer
{
    public class MaterialsNode : INode
    {
        private readonly List<Material> _materials;
        private readonly Func<Material, MaterialNode> _materialFactory;

        public MaterialsNode(List<Material> materials, Func<Material, MaterialNode> materialFactory)
        {
            _materials = materials;
            _materialFactory = materialFactory;
        }

        public string Text => "Materials";
        public bool HasChildren => _materials.Any();
        public IEnumerable<INode> GetChildren()
        {
            foreach (var material in _materials)
            {
               yield return _materialFactory(material); 
            }
        }

        public object GetPreview()
        {
            return null;
        }

        public object GetPreviewThumbnail()
        {
            return null;
        }
    }
}