using System.Diagnostics;
using System.Numerics;
using System.Windows.Media.Media3D;
using Vector = System.Windows.Vector;

namespace FBXViewer
{
    public class Camera
    {
        private readonly ProjectionCamera _camera;
        private readonly PointLightBase _cameraLight;
        private readonly Vector3 _initialPivot;
        private Vector3 _position;
        private readonly Vector3 _originalForward;
        private readonly Vector3 _originalUp;
        private readonly Vector3 _originalPosition;
        private Vector3 _pivot;
        private Vector3 _forward;
        private Vector3 _up;

        public Camera(ProjectionCamera camera, Vector3 initialPivot, PointLightBase cameraLight = null)
        {
            _camera = camera;
            _cameraLight = cameraLight;
            _initialPivot = _pivot = initialPivot;
            MoveCamera(_camera.Position);
            _originalPosition = _camera.Position.AsVector3();
            _up = Vector3.Normalize(_camera.UpDirection.AsVector3());
            _forward = Vector3.Normalize(_camera.LookDirection.AsVector3());

            _originalUp = _up;
            _originalForward = _forward;
        }

        public void Pan(float x, float y)
        {
            var right = Vector3.Cross(_up, _forward);
            _position += _up * y + right * x;
            _pivot += _up * y + right * x;

            MoveCamera(_position.AsPoint3D());
            var lookDir = (_pivot - _position);
            lookDir = Vector3.Normalize(lookDir);
            _camera.LookDirection = lookDir.AsMVector3D();
            
            Debug.WriteLine($"Camera position: {_camera.Position}. dir: {_camera.LookDirection} Pivot {_pivot}");
        }

        private void MoveCamera(Point3D point3D)
        {
            _camera.Position = point3D;
            if (_cameraLight != null)
            {
                _cameraLight.Position = point3D;
            }
        }

        public void Zoom(in float delta)
        {
            _position += _forward * delta;
            MoveCamera(_position.AsPoint3D());
            
            Debug.WriteLine($"Camera position: {_camera.Position}. Forward: {_forward}");
        }

        public void Reset()
        {
            Debug.WriteLine("Resetting camera");
            _position = (_camera.Position = _originalPosition.AsPoint3D()).AsVector3();
            _camera.LookDirection = _originalForward.AsMVector3D();
            _camera.UpDirection = _originalUp.AsMVector3D();
        }

        public void Orbit(Vector delta)
        {
            delta *= -1;
            var rotation = System.Numerics.Quaternion.CreateFromYawPitchRoll(
                (float) delta.X, (float) delta.Y, 0);
            var dirFromPivot = _position - _pivot;
            var newDir = Vector3.Transform(dirFromPivot, rotation);
            var newPosition = _pivot + newDir;
            _forward = Vector3.Normalize(-newDir);
            _position = newPosition;

            MoveCamera(_position.AsPoint3D());
            _camera.LookDirection = _forward.AsMVector3D();
        }
    }
}