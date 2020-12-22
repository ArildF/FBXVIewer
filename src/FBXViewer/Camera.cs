using System;
using System.Collections;
using System.Diagnostics;
using System.Numerics;
using Quaternion = System.Numerics.Quaternion;

namespace FBXViewer
{
    public class Camera
    {
        private readonly IRendererCamera _rendererCamera;
        private readonly ILight? _cameraLight;
        private readonly Coroutines _coroutines;
        private Vector3 _originalPivot;
        private Vector3 _position;
        private Vector3 _originalPosition;
        private Vector3 _pivot;
        private Quaternion _rotation;
        private Quaternion _originalRotation;

        public Camera(IRendererCamera rendererCamera, Vector3 initialPivot, Coroutines coroutines, ILight? cameraLight = null)
        {
            _rendererCamera = rendererCamera;
            _cameraLight = cameraLight;
            _coroutines = coroutines;
            _originalPivot = _pivot = initialPivot;
            _position = _originalPosition = _rendererCamera.Position;
            _rotation = (_pivot - _position).ToLookRotation(Vector3.UnitY);
            MoveCamera(_position, _rotation, _pivot);

            _originalRotation = _rotation;
        }

        public bool IsOrthographic => _rendererCamera.IsOrthographic;

        public void Pan(float x, float y)
        {
            var position = _position + _rotation.Up() * y + _rotation.Right() * x;
            var pivot = _pivot + _rotation.Up() * y + _rotation.Right() * x;

            MoveCamera(position, _rotation, pivot);
        }

        private void MoveCamera(Vector3 position, Quaternion rotation, Vector3 pivot, bool animate = false)
        {
            if (!animate)
            {
                _position = position;
                _rotation = rotation;
                _pivot = pivot;
                _rendererCamera.Move(position, _rotation.Forward(), _rotation.Up());
                if (_cameraLight != null)
                {
                    _cameraLight.Position = position;
                }
            }
            else
            {
                _coroutines.StartCoroutine(AnimateCamera(position, rotation, pivot));
            }
        }

        private IEnumerator AnimateCamera(Vector3 position, Quaternion rotation, Vector3 pivot)
        {
            var angle = rotation.AngleTo(_rotation);
            var totalTimeAngle = (0.1f * angle) / 90;
            var totalTimeDistance = (pivot - _pivot).Length() * 0.003;

            var totalTime = (float)Math.Max(totalTimeAngle, totalTimeDistance);
            Debug.WriteLine($"time angle: {totalTimeAngle} time dist {totalTimeDistance}");
            var time = 0.0f;
            var originalPivot = _pivot;
            var originalRotation = _rotation;
            var targetRotation = rotation;
            var distanceFromOrigin = (_position - _pivot).Length();
            while (time < totalTime)
            {
                var lerpFactor = time / totalTime;

                var newPivot = Vector3.Lerp(originalPivot, pivot, lerpFactor);
                var newRotation = Quaternion.Slerp(originalRotation, targetRotation, lerpFactor);
                
                var newUnitPosition = Vector3.Transform(-Vector3.UnitZ, newRotation);
                var newPosition = newPivot + newUnitPosition * distanceFromOrigin;
                
                MoveCamera(newPosition, newRotation, newPivot);

                time += (float)_coroutines.DeltaTime;
                
                yield return null;
            }
            MoveCamera(position, rotation, pivot);
        }

        public void Zoom(in float delta)
        {
            var position = _position + _rotation.Forward() * delta;
            MoveCamera(position, _rotation, _pivot);
            _rendererCamera.AdjustWidth(-delta);
        }

        public void ResetToDefault()
        {
            Debug.WriteLine("Resetting camera");
            _position = _originalPosition;
            _rotation = _originalRotation;
            _pivot = _originalPivot;
            MoveTo(_position, _pivot);
        }

        public void Orbit(Vector3 delta)
        {
            var rotation = Quaternion.CreateFromAxisAngle(Vector3.Transform(Vector3.UnitX, _rotation), (float) delta.Y) * _rotation;
            rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float) -delta.X) * rotation;
            rotation = Quaternion.Normalize(rotation);
            
            var distanceFromOrigin = (_position - _pivot).Length();
            var newUnitPosition = Vector3.Transform(-Vector3.UnitZ, rotation);

            var position = _pivot + newUnitPosition * distanceFromOrigin;

            MoveCamera(position, rotation, _pivot);
        }

        public void Dolly(double deltaY)
        {
            var position = _position + _rotation.Forward() * (float)deltaY;
            MoveCamera(position, _rotation, _pivot);
            _rendererCamera.AdjustWidth((float) deltaY);
        }

        public void MoveTo(Vector3 position, Vector3 pivot)
        {
            var lookDir = pivot - position;
            var rotation = lookDir.ToLookRotation(_rotation.Up());
           MoveCamera(position, rotation, pivot, animate: true);
        }

        public void MovePivotTo(Vector3 point)
        {
            var delta = point- _pivot;
            var newPosition = _position + delta;
            MoveTo(newPosition,point);
        }

        public void ResetTo(Vector3 cameraPosition, Vector3 pivot, float orthoWidth)
        {
            _pivot = pivot;
            var lookDir = pivot - cameraPosition;
            var rotation = lookDir.ToLookRotation(Vector3.UnitY);
            MoveCamera(cameraPosition, rotation, _pivot, animate: true);
            _rendererCamera.OrthographicWidth = orthoWidth;

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
            var rotation = lookDir.ToLookRotation(up);
            MoveCamera(position, rotation, _pivot, animate: true);
        }

        public void TogglePerspectiveOrthographic()
        {
           _rendererCamera.TogglePerspectiveOrthographic(); 
        }
    }
}