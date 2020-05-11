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
        private Vector3 _initialPivot;
        private Vector3 _position;
        private Vector3 _originalPosition;
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
            _rotation = CalculateRotation();

            _originalRotation = _rotation;
            
            Debug.WriteLine($"Forward {Vector3.Transform(new Vector3(0, 0, 1), _rotation)}");
        }

        private Quaternion CalculateRotation()
        {
            var up = Vector3.Normalize(_camera.UpDirection.AsVector3());
            var forward = Vector3.Normalize(_camera.LookDirection.AsVector3());
            return forward.ToLookRotation(up);
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
            _position = _originalPosition;
            _rotation = _originalRotation;
            _pivot = _initialPivot;
            _camera.UpDirection = _rotation.Up().AsMVector3D();
            MoveTo(_position, _pivot - _position, _pivot);
        }

        public void Orbit(Vector delta)
        {
            _rotation = Quaternion.CreateFromAxisAngle(Vector3.Transform(Vector3.UnitX, _rotation), (float) delta.Y) * _rotation;
            _rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float) delta.X) * _rotation;
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

        public void MoveTo(Vector3 position, Vector3 lookDir, Vector3 pivot, bool storeAsOriginal = false)
        {
            _position = position;
           MoveCamera(position.AsPoint3D());
           _camera.LookDirection = lookDir.AsMVector3D();
           _rotation = CalculateRotation();
           _pivot = pivot;
           if (storeAsOriginal)
           {
               _originalPosition = position;
               _originalRotation = _rotation;
               _initialPivot = _pivot;
           }

        }

        public void MovePivotTo(Point3D pointHit)
        {
            var delta = pointHit.AsVector3() - _pivot;
            var newPosition = _position + delta;
            var lookDir = pointHit.AsVector3() - newPosition;
            MoveTo(newPosition, lookDir, pointHit.AsVector3());
        }
    }
}