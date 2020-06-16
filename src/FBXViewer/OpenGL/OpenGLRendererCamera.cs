using System;
using System.Diagnostics;
using System.Numerics;

namespace FBXViewer.OpenGL
{
    public class OpenGLRendererCamera : IRendererCamera
    {
        private Vector3 _upDirection;
        private Vector3 _lookDirection;
        private Vector3 _position;

        public float OrthographicWidth { get; set; } = 20f; 
        public Vector3 Position => _position;
        public bool IsOrthographic => _projection == _orthoProjection;

        private readonly Func<float, float, Matrix4x4> _perspectiveProjection = (w, h) =>
            Matrix.PerspectiveProjection(45, w / h, 0.5f, 1000);

        private readonly Func<float, float, Matrix4x4> _orthoProjection;
        
        
        private Func<float, float, Matrix4x4> _projection;

        public OpenGLRendererCamera(Vector3 position, Vector3 lookDirection, Vector3 upDirection)
        {
            _upDirection = upDirection;
            _lookDirection = lookDirection;
            _position = position;

            _projection = _perspectiveProjection;

            _orthoProjection = (w, h) => Matrix.OrthographicProjection(
                 - OrthographicWidth / 2,
                OrthographicWidth / 2,
                -(OrthographicWidth * (h / w)) / 2.0f,
                (OrthographicWidth * (h /w)) / 2.0f,
                -1,
                100000
            );
        }

        public void Move(Vector3 position, Vector3 lookDirection, Vector3 upDirection)
        {
            _position = position;
            _lookDirection = lookDirection;
            _upDirection = upDirection;
        }

        public void TogglePerspectiveOrthographic()
        {
            _projection = _projection == _perspectiveProjection ? _orthoProjection : _perspectiveProjection;
        }

        public void AdjustWidth(in float delta)
        {
            OrthographicWidth = Math.Max(0, OrthographicWidth + delta);
            Debug.WriteLine(OrthographicWidth);
        }

        public Matrix4x4 ViewMatrix => Matrix.LookAtDirection(_position, _lookDirection, _upDirection);

        public Matrix4x4 ProjectionMatrix(float width, float height)
            => _projection(width, height);
    }
}