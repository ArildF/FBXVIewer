using System;
using System.Collections.Generic;
using Assimp;

namespace FBXViewer
{
    public class SceneNode : BaseNode
    {
        private readonly Node _node;
        private SceneNode? Parent { get; set; }
        private readonly Func<Node, SceneNode> _sceneNodeFactory;

        public SceneNode(Node node, Func<Node, SceneNode> sceneNodeFactory)
        {
            _node = node;
            _sceneNodeFactory = sceneNodeFactory;
        }

        public override string? Text => _node.Name;
        public override bool HasChildren => true;
        protected override IEnumerable<INode> CreateChildren()
        {
            foreach (var child in _node.Children)
            {
                var sceneNode = _sceneNodeFactory(child);
                sceneNode.Parent = this;
                yield return sceneNode;
            }

            foreach (var property in _node.PrimitiveProperties())
            {
                yield return property;
            }
            
        }
    }
}