using System;
using System.Collections.Generic;
using System.Linq;
using Assimp;

namespace FBXViewer
{
    public class AnimationsNode : BaseNode
    {
        private readonly List<Animation> _animations;
        private readonly Func<Animation, AnimationNode> _nodeFactory;

        public AnimationsNode(List<Animation> animations, Func<Animation, AnimationNode> nodeFactory)
        {
            _animations = animations;
            _nodeFactory = nodeFactory;
        }
        public override string? Text => "Animations";
        public override bool HasChildren => _animations.Count > 0;
        protected override IEnumerable<INode> CreateChildren()
        {
            return _animations.Select(_nodeFactory).ToArray();
        }
    }
}