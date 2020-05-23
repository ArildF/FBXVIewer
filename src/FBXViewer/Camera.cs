using System;
using System.Diagnostics;
using System.Numerics;
using System.Windows.Media.Media3D;
using Quaternion = System.Numerics.Quaternion;
using Vector = System.Windows.Vector;

namespace FBXViewer
{
    public class Camera
    {
        private readonly WpfCamera _camera;
        private readonly PointLightBase? _cameraLight;
        private Vector3 _originalPivot;
        private Vector3 _position;
        private Vector3 _originalPosition;
        private Vector3 _pivot;
        private Quaternion _rotation;
        private Quaternion _originalRotation;

        public Camera(WpfCamera camera, Vector3 initialPivot, PointLightBase? cameraLight = null)
        {
            _camera = camera;
            _cameraLight = cameraLight;
            _originalPivot = _pivot = initialPivot;
            _position = _originalPosition = _camera.Position.AsVector3();
            _rotation = (_pivot - _position).ToLookRotation(Vector3.UnitY);
            MoveCamera(_position, _rotation.Forward(), _rotation.Up());

            _originalRotation = _rotation;
        }

        public void Pan(float x, float y)
        {
            _position += _rotation.Up() * y + _rotation.Right() * x;
            _pivot += _rotation.Up() * y + _rotation.Right() * x;

            MoveCamera(_position, _rotation.Forward(), _rotation.Up());
        }

        private void MoveCamera(Vector3 position, Vector3 lookDirection, Vector3 upDirection)
        {
            _camera.Move(position, lookDirection, upDirection);
            if (_cameraLight != null)
            {
                _cameraLight.Position = position.AsPoint3D();
            }
        }

        public void Zoom(in float delta)
        {
            _position += _rotation.Forward() * delta;
            MoveCamera(_position, _rotation.Forward(), _rotation.Up());
            _camera.AdjustWidth(-delta);
        }

        public void ResetToDefault()
        {
            Debug.WriteLine("Resetting camera");
            _position = _originalPosition;
            _rotation = _originalRotation;
            _pivot = _originalPivot;
            MoveTo(_position, _pivot);
        }

        public void Orbit(Vector delta)
        {
            _rotation = Quaternion.CreateFromAxisAngle(Vector3.Transform(Vector3.UnitX, _rotation), (float) delta.Y) * _rotation;
            _rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float) -delta.X) * _rotation;
            _rotation = Quaternion.Normalize(_rotation);
            
            var distanceFromOrigin = (_position - _pivot).Length();
            var newUnitPosition = Vector3.Transform(-Vector3.UnitZ, _rotation);

            _position = _pivot + newUnitPosition * distanceFromOrigin;

            MoveCamera(_position, _rotation.Forward(), _rotation.Up());
        }

        public void Dolly(double deltaY)
        {
            _position += _rotation.Forward() * (float)deltaY;
            MoveCamera(_position, _rotation.Forward(), _rotation.Up());
            _camera.AdjustWidth((float) deltaY);
        }

        public void MoveTo(Vector3 position, Vector3 pivot)
        {
            var lookDir = pivot - position;
            _position = position;
            _rotation = lookDir.ToLookRotation(_rotation.Up());
           MoveCamera(position, _rotation.Forward(), _rotation.Up());
           _pivot = pivot;
        }

        public void MovePivotTo(Point3D pointHit)
        {
            var delta = pointHit.AsVector3() - _pivot;
            var newPosition = _position + delta;
            MoveTo(newPosition,pointHit.AsVector3());
        }

        public void ResetTo(Vector3 cameraPosition, Vector3 pivot)
        {
            _position = cameraPosition;
            _pivot = pivot;
            var lookDir = pivot - cameraPosition;
            _rotation = lookDir.ToLookRotation(Vector3.UnitY);
            MoveCamera(_position, _rotation.Forward(), _rotation.Up());

            _originalPosition = _position;
            _originalRotation = _rotation;
            _originalPivot = pivot;
        }

        public void MoveToView(View view)
        {
            var distanceFromPivot = (_pivot - _position).Length();
            var (position, up) = view switch
            {
                View.Front => (_pivot + Vector3.UnitZ * distanceFromPivot, Vector3.UnitY),
                View.Back => (_pivot - Vector3.UnitZ * distanceFromPivot, Vector3.UnitY),
                View.Right => (_pivot + Vector3.UnitX * distanceFromPivot, Vector3.UnitY),
                View.Left => (_pivot - Vector3.UnitX * distanceFromPivot, Vector3.UnitY),
                View.Top => (_pivot + Vector3.UnitY * distanceFromPivot, -Vector3.UnitZ),
                View.Bottom => (_pivot - Vector3.UnitY * distanceFromPivot, Vector3.UnitZ),
                _ => throw new ArgumentException(nameof(view)),
            };

            var lookDir = _pivot - position;
            _rotation = lookDir.ToLookRotation(up);
            _position = position;
            MoveCamera(_position, _rotation.Forward(), _rotation.Up());
        }
    }
}