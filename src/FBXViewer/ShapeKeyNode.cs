using System.Collections.Generic;
using System.Linq;
using Assimp;

namespace FBXViewer
{
    public class ShapeKeyNode : BaseNode
    {
        private readonly IGrouping<string, MeshAnimationAttachment> _attachment;

        public ShapeKeyNode(IGrouping<string, MeshAnimationAttachment> attachment)
        {
            _attachment = attachment;
        }


        public override string? Text => _attachment.Key;
        public override bool HasChildren => false;
        protected override IEnumerable<INode> CreateChildren()
        {
            yield break;
        }
    }
}