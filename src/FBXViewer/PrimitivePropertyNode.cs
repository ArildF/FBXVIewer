using System.Collections.Generic;

namespace FBXViewer
{
    internal class PrimitivePropertyNode : BaseNode
    {
        private readonly string _propName;
        private readonly object? _value;

        public PrimitivePropertyNode(string propName, object? value)
        {
            _propName = propName;
            _value = value;
        }

        public override string Text => $"{_propName}: {_value}";
        public override bool HasChildren => false;
        protected override IEnumerable<INode> CreateChildren()
        {
            yield break;
        }
    }
}