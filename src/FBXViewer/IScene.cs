using System.Numerics;
using System.Windows;
using Assimp;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Quaternion = System.Windows.Media.Media3D.Quaternion;

namespace FBXViewer
{
    public interface IScene
    {
        IRendererCamera RendererCamera { get; }
        UIElement Visual { get; }
        ILight? CameraLight { get; }
        void LoadMesh(Mesh mesh, Matrix4x4 transform);
        void SetShapeKeyWeight(Mesh mesh, float weight, MeshAnimationAttachment attachment);
        void UnloadMesh(Mesh mesh);
        bool RayCast(Vector2 mousePos, out RayCastResult rayCastResult);
        Bounds GetBoundingBox(Mesh mesh);
        IMouseInput MouseInput { get; }
    }
}