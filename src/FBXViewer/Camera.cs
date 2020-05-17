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
        private readonly PointLightBase? _cameraLight;
        private Vector3 _originalPivot;
        private Vector3 _position;
        private Vector3 _originalPosition;
        private Vector3 _pivot;
        private Quaternion _rotation;
        private Quaternion _originalRotation;

        public Camera(ProjectionCamera camera, Vector3 initialPivot, PointLightBase? cameraLight = null)
        {
            _camera = camera;
            _cameraLight = cameraLight;
            _originalPivot = _pivot = initialPivot;
            MoveCamera(_camera.Position);
            _position = _originalPosition = _camera.Position.AsVector3();
            _rotation = (_pivot - _position).ToLookRotation(Vector3.UnitY);

            _originalRotation = _rotation;
            
            Debug.WriteLine($"Forward {Vector3.Transform(new Vector3(0, 0, 1), _rotation)}");
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

        public void ResetToDefault()
        {
            Debug.WriteLine("Resetting camera");
            _position = _originalPosition;
            _rotation = _originalRotation;
            _pivot = _originalPivot;
            MoveTo(_position, _pivot);
            _camera.UpDirection = _rotation.Up().AsMVector3D();
            _camera.LookDirection = _rotation.Forward().AsMVector3D();
        }

        public void Orbit(Vector delta)
        {
            _rotation = Quaternion.CreateFromAxisAngle(Vector3.Transform(Vector3.UnitX, _rotation), (float) delta.Y) * _rotation;
            _rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float) -delta.X) * _rotation;
            _rotation = Quaternion.Normalize(_rotation);
            
            var distanceFromOrigin = (_position - _pivot).Length();
            var newUnitPosition = Vector3.Transform(-Vector3.UnitZ, _rotation);

            _position = _pivot + newUnitPosition * distanceFromOrigin;

            MoveCamera(_position.AsPoint3D());
            _camera.LookDirection = _rotation.Forward().AsMVector3D();
            _camera.UpDirection = _rotation.Up().AsMVector3D();
        }

        public void Dolly(double deltaY)
        {
            _position += _rotation.Forward() * (float)deltaY;
            MoveCamera(_position.AsPoint3D());
        }

        public void MoveTo(Vector3 position, Vector3 pivot)
        {
            var lookDir = pivot - position;
            _position = position;
            _rotation = lookDir.ToLookRotation(_rotation.Up());
           MoveCamera(position.AsPoint3D());
           _camera.LookDirection = _rotation.Forward().AsMVector3D();
           _camera.UpDirection = _rotation.Up().AsMVector3D();
           _pivot = pivot;
        }

        public void MovePivotTo(Point3D pointHit)
        {
            var delta = pointHit.AsVector3() - _pivot;
            var newPosition = _position + delta;
            var lookDir = pointHit.AsVector3() - newPosition;
            MoveTo(newPosition,pointHit.AsVector3());
        }

        public void ResetTo(Vector3 cameraPosition, Vector3 pivot)
        {
            _position = cameraPosition;
            _pivot = pivot;
            var lookDir = pivot - cameraPosition;
            _rotation = lookDir.ToLookRotation(Vector3.UnitY);
            MoveCamera(_position.AsPoint3D());
            _camera.LookDirection = _rotation.Forward().AsMVector3D();
            _camera.UpDirection = _rotation.Up().AsMVector3D();

            _originalPosition = _position;
            _originalRotation = _rotation;
            _originalPivot = pivot;
        }
    }
}