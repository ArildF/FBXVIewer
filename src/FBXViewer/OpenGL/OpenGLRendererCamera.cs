using System.Numerics;
using OpenGL;

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

        public void OnRender()
        {
            Gl.MatrixMode(MatrixMode.Modelview);
            Gl.LoadIdentity();
            Gl.MultMatrixf(Matrix4x4f.LookAtDirection(_position.AsVertex3f(), _lookDirection.AsVertex3f(),
                _upDirection.AsVertex3f()));
        }
    }
}