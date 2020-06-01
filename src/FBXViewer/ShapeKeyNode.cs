using System.Collections.Generic;
using System.Linq;

namespace FBXViewer
{
    public class ShapeKeyNode : BaseNode
    {
        private readonly IGrouping<string, ShapeKey> _attachment;
        private readonly IScene _scene;
        private readonly ShapeKeyViewModel _viewModel;
        private float _value;

        public ShapeKeyNode(IGrouping<string, ShapeKey> attachment, IScene scene)
        {
            _attachment = attachment;
            _scene = scene;
            _viewModel = new ShapeKeyViewModel(this);
        }

        public override object UIDataContext => _viewModel;

        public float Value
        {
            get => _value;
            set
            {
                _value = value;
                foreach (var key in _attachment)
                {
                    _scene.SetShapeKeyWeight(key.Mesh, _value, key.Attachment);
                }
            }
        }

        public override string? Text => _attachment.Key;
        public override bool HasChildren => false;
        protected override IEnumerable<INode> CreateChildren()
        {
            yield break;
        }
    }
}