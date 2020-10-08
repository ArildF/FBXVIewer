using System.Collections.Generic;
using Assimp;

namespace FBXViewer
{
    public class AnimationNode : BaseNode
    {
        private readonly Animation _animation;

        public AnimationNode(Animation animation)
        {
            _animation = animation;
        }

        public override string? Text => "Animation: " + _animation.Name;
        public override bool HasChildren => true;
        protected override IEnumerable<INode> CreateChildren()
        {
            return _animation.PrimitiveProperties();
        }
    }
}