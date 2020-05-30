using Assimp;

namespace FBXViewer
{
    public class ShapeKey
    {
        public Mesh Mesh { get; }
        public MeshAnimationAttachment Attachment { get; }

        public ShapeKey(Mesh mesh, MeshAnimationAttachment attachment)
        {
            Mesh = mesh;
            Attachment = attachment;
        }
    }
}