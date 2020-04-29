using System;
using System.Collections.Generic;
using System.Linq;
using Assimp;

namespace FBXViewer
{
    public class MaterialsNode : BaseNode
    {
        private readonly List<Material> _materials;
        private readonly Func<Material, MaterialNode> _materialFactory;

        public MaterialsNode(List<Material> materials, Func<Material, MaterialNode> materialFactory)
        {
            _materials = materials;
            _materialFactory = materialFactory;
        }

        public override string Text => "Materials";
        public override bool HasChildren => _materials.Any();

        protected override IEnumerable<INode> CreateChildren()
        {
            foreach (var material in _materials)
            {
               yield return _materialFactory(material); 
            }
        }

        public override object GetPreview()
        {
            return null;
        }

        public override object GetPreviewThumbnail()
        {
            return null;
        }
    }
}