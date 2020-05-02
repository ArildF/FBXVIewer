using System.Collections.Generic;

namespace FBXViewer
{
    public interface INode
    {
        string Text { get; }
        bool HasChildren { get; }
        bool SupportsMultiSelect { get; }
        bool IsChecked { get; set; }
        bool IsSelected { get; set; }
        IEnumerable<INode> GetChildren();
        object GetPreview();
        object GetPreviewThumbnail();
    }
}