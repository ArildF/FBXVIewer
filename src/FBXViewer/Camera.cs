using System.Diagnostics;
using System.Numerics;
using System.Windows.Media.Media3D;
using Quaternion = System.Numerics.Quaternion;
using Vector = System.Windows.Vector;

namespace FBXViewer
{
    public class Camera
    {
        private readonly ProjectionCamera _camera;
        private readonly PointLightBase _cameraLight;
        private readonly Vector3 _initialPivot;
        private Vector3 _position;
        private readonly Vector3 _originalPosition;
        private Vector3 _pivot;
        private Quaternion _rotation;
        private Quaternion _originalRotation;

        public Camera(ProjectionCamera camera, Vector3 initialPivot, PointLightBase cameraLight = null)
        {
            _camera = camera;
            _cameraLight = cameraLight;
            _initialPivot = _pivot = initialPivot;
            MoveCamera(_camera.Position);
            _position = _originalPosition = _camera.Position.AsVector3();
            CalculateRotation();

            _originalRotation = _rotation;
            
            Debug.WriteLine($"Forward {Vector3.Transform(new Vector3(0, 0, 1), _rotation)}");
        }

        private void CalculateRotation()
        {
            var up = Vector3.Normalize(_camera.UpDirection.AsVector3());
            var forward = Vector3.Normalize(_camera.LookDirection.AsVector3());
            _rotation = forward.ToLookRotation(up);
        }

        public void Pan(float x, float y)
        {
            _position += _rotation.Up() * y + _rotation.Right() * x;
            _pivot += _rotation.Up() * y + _rotation.Right() * x;

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
            _position += _rotation.Forward() * delta;
            MoveCamera(_position.AsPoint3D());
            
            Debug.WriteLine($"Camera position: {_camera.Position}. Forward: {_rotation.Forward()}");
        }

        public void Reset()
        {
            Debug.WriteLine("Resetting camera");
            // _position = (_camera.Position = _originalPosition.AsPoint3D()).AsVector3();
            // _camera.LookDirection = _originalForward.AsMVector3D();
            // _camera.UpDirection = _originalUp.AsMVector3D();
        }

        public void Orbit(Vector delta)
        {
            delta *= -1;
            var deltaRotation = Quaternion.Normalize(Quaternion.CreateFromYawPitchRoll(
                (float) delta.X, (float) delta.Y, 0));
            
            // _rotation *= deltaRotation;
            // _rotation = Quaternion.Normalize(_rotation);
            // var distance = (_pivot - _position).Length();
            // var newPosition = _pivot + Vector3.Transform(-Vector3.UnitZ * distance, _rotation);
            var positionFromOrigin = (_position - _pivot);
            positionFromOrigin = Vector3.Transform(positionFromOrigin, deltaRotation);
            var newPosition = positionFromOrigin + _pivot;
            
            _position = newPosition;
            _rotation = (_pivot - _position).ToLookRotation(Vector3.UnitY);

            MoveCamera(_position.AsPoint3D());
            _camera.LookDirection = _rotation.Forward().AsMVector3D();
        }

        public void Dolly(double deltaY)
        {
            _position += _rotation.Forward() * (float)deltaY;
            MoveCamera(_position.AsPoint3D());
        }

        public void MoveTo(Vector3 position, Vector3 lookDir, Vector3 pivot)
        {
            _position = position;
           MoveCamera(position.AsPoint3D());
           _camera.LookDirection = lookDir.AsMVector3D();
           CalculateRotation();
           _pivot = pivot;
        }
    }
}