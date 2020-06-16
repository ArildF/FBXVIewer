using System;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using MVector3D = System.Windows.Media.Media3D.Vector3D;

namespace FBXViewer.Wpf
{
    public class WpfRendererCamera : IRendererCamera
    {
        private readonly Viewport3D _viewPort;
        private readonly PerspectiveCamera _perspectiveCamera;
        private readonly OrthographicCamera _orthographicCamera;
        private ProjectionCamera _currentCamera;

        public WpfRendererCamera(Viewport3D viewPort, Vector3 center)
        {
            _viewPort = viewPort;
            _perspectiveCamera = new PerspectiveCamera(
                center.AsPoint3D(),
                new Vector3(0, 0, 1).AsMVector3D(), 
                new MVector3D(0, 1, 0), 45);
            _orthographicCamera = new OrthographicCamera(
                center.AsPoint3D(),
                new MVector3D(0, 0, 1), 
                new MVector3D(0, 1, 0), 20);

            _viewPort.Camera = _currentCamera = _perspectiveCamera;

        }

        public Vector3 Position => _currentCamera.Position.AsVector3();

        public float OrthographicWidth
        {
            get => (float) _orthographicCamera.Width;
            set => _orthographicCamera.Width = value;
        }

        public void Move(Vector3 position, Vector3 lookDirection, Vector3 upDirection)
        {
            _currentCamera.Position = position.AsPoint3D();
            _currentCamera.LookDirection = lookDirection.AsMVector3D();
            _currentCamera.UpDirection = upDirection.AsMVector3D();
        }

        public void TogglePerspectiveOrthographic()
        {
            var previousCamera = _currentCamera;
            var newCamera = previousCamera == _orthographicCamera 
                ? _perspectiveCamera 
                : (ProjectionCamera)_orthographicCamera;
            newCamera.Position = previousCamera.Position;
            newCamera.LookDirection = previousCamera.LookDirection;
            newCamera.UpDirection = previousCamera.UpDirection;

            _viewPort.Camera = _currentCamera = newCamera;
        }

        public void AdjustWidth(in float delta)
        {
            _orthographicCamera.Width = Math.Max(_orthographicCamera.Width + delta, 0);
        }

        public bool IsOrthographic => _currentCamera == _orthographicCamera;
    }
}