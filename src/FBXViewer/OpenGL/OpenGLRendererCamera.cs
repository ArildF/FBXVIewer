using System.Numerics;

namespace FBXViewer.OpenGL
{
    public class OpenGLRendererCamera : IRendererCamera
    {
        private Vector3 _upDirection;
        private Vector3 _lookDirection;
        private Vector3 _position;
        public Vector3 Position => _position;
        public bool IsOrthographic { get; }

        public OpenGLRendererCamera(Vector3 position, Vector3 lookDirection, Vector3 upDirection)
        {
            _upDirection = upDirection;
            _lookDirection = lookDirection;
            _position = position;
        }

        public void Move(Vector3 position, Vector3 lookDirection, Vector3 upDirection)
        {
            _position = position;
            _lookDirection = lookDirection;
            _upDirection = upDirection;
        }

        public void TogglePerspectiveOrthographic()
        {
        }

        public void AdjustWidth(in float delta)
        {
        }

        public Matrix4x4 ViewMatrix => Matrix.LookAtDirection(_position, _lookDirection, _upDirection);

        public Matrix4x4 ProjectionMatrix (float width, float height)  
            => Matrix.PerspectiveProjection(45, width / height, 0.5f, 1000);
    }
}