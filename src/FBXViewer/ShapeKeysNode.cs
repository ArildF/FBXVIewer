using System;
using System.Collections.Generic;
using System.Linq;

namespace FBXViewer
{
    public class ShapeKeysNode : BaseNode
    {
        private readonly List<IGrouping<string, ShapeKey>> _keys;
        private readonly Func<IGrouping<string, ShapeKey>, ShapeKeyNode> _shapeKeyNodeFactory;

        public ShapeKeysNode(IEnumerable<IGrouping<string, ShapeKey>> keys,
            Func<IGrouping<string, ShapeKey>, ShapeKeyNode> shapeKeyNodeFactory)
        {
            _keys = keys.ToList();
            _shapeKeyNodeFactory = shapeKeyNodeFactory;
        }

        public override string? Text => $"Shapekeys ({_keys.Count})";
        public override bool HasChildren => _keys.Any();
        protected override IEnumerable<INode> CreateChildren()
        {
            foreach (var attachment in _keys)
            {
                yield return _shapeKeyNodeFactory(attachment);
            }
        }
    }
}