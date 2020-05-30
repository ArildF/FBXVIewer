using System.Collections.Generic;
using System.Linq;

namespace FBXViewer
{
    public class ShapeKeyNode : BaseNode
    {
        private readonly IGrouping<string, ShapeKey> _attachment;

        public ShapeKeyNode(IGrouping<string, ShapeKey> attachment)
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