using System.Numerics;
using System.Windows;
using Assimp;
using Quaternion = System.Windows.Media.Media3D.Quaternion;

namespace FBXViewer
{
    public interface IScene
    {
        IRendererCamera RendererCamera { get; }
        UIElement Visual { get; }
        ILight? CameraLight { get; }
        void LoadMesh(Mesh mesh);
        void SetShapeKeyWeight(Mesh mesh, float weight, MeshAnimationAttachment attachment);
        void UnloadMesh(Mesh mesh);
        bool RayCast(Vector2 mousePos, out RayCastResult rayCastResult);
        Bounds GetBoundingBox(Mesh mesh);
        void ToggleWireFrame(in bool wireFrameEnabled);
        void ToggleMesh(in bool meshEnabled);
        void SetRootRotation(Quaternion quaternion);
        
        IMouseInput MouseInput { get; }
    }
}