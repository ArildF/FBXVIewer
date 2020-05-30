using System.Collections.Generic;
using System.Linq;

namespace FBXViewer
{
    public class ShapeKeyNode : BaseNode
    {
        private readonly IGrouping<string, ShapeKey> _attachment;
        private readonly ShapeKeyViewModel _viewModel;
        private double _value;

        public ShapeKeyNode(IGrouping<string, ShapeKey> attachment)
        {
            _attachment = attachment;
            _viewModel = new ShapeKeyViewModel(this);
        }

        public override object UIDataContext => _viewModel;

        public double Value
        {
            get => _value;
            set => _value = value;
        }

        public override string? Text => _attachment.Key;
        public override bool HasChildren => false;
        protected override IEnumerable<INode> CreateChildren()
        {
            yield break;
        }
    }
}