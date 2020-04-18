using System.Collections.Generic;
using System.Linq;

namespace FBXViewer
{
    public abstract class BaseNode : INode
    {
        private INode[] _children;
        public abstract string Text { get; }
        public abstract bool HasChildren { get; }
        public IEnumerable<INode> GetChildren()
        {
            _children ??= CreateChildren().ToArray();
            return _children;
        }

        public virtual object GetPreview()
        {
            return null;
        }

        public virtual object GetPreviewThumbnail()
        {
            return GetPreview();
        }

        protected abstract IEnumerable<INode> CreateChildren();
    }
}