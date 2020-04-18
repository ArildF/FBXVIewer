using System.Collections.Generic;

namespace FBXViewer
{
    public interface INode
    {
        string Text { get; }
        bool HasChildren { get; }
        IEnumerable<INode> GetChildren();
        object GetPreview();
        object GetPreviewThumbnail();
    }
}