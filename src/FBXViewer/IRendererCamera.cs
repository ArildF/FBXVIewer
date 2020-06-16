using System.Numerics;

namespace FBXViewer
{
    public interface IRendererCamera
    {
        Vector3 Position { get; }
        bool IsOrthographic { get; }
        float OrthographicWidth { get; set; }
        void Move(Vector3 position, Vector3 lookDirection, Vector3 upDirection);
        void TogglePerspectiveOrthographic();
        void AdjustWidth(in float delta);
    }
}