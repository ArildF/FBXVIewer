using System.Collections.Generic;
using System.Linq;

namespace FBXViewer
{
    public class ShapeKeyNode : BaseNode
    {
        private readonly IGrouping<string, ShapeKey> _attachment;
        private readonly ModelPreview _preview;
        private readonly ShapeKeyViewModel _viewModel;
        private float _value;

        public ShapeKeyNode(IGrouping<string, ShapeKey> attachment, ModelPreview preview)
        {
            _attachment = attachment;
            _preview = preview;
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
                    _preview.SetShapeKeyWeight(key.Mesh, _value, key.Attachment);
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