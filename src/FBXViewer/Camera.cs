using System.Diagnostics;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using Assimp;
using Quaternion = Assimp.Quaternion;
using Vector3D = Assimp.Vector3D;

namespace FBXViewer
{
    public class Camera
    {
        private ProjectionCamera _camera;
        private Vector3D _position;
        private Quaternion _rotation;
        private readonly Vector3D _originalForward;
        private readonly Vector3D _originalUp;
        private readonly Vector3D _originalPosition;

        public Camera(ProjectionCamera camera)
        {
            _camera = camera;
            _position = _camera.Position.AsVector3D();
            _originalPosition = _camera.Position.AsVector3D();
            var up = _camera.UpDirection.AsVector3D();
            var forward = _camera.LookDirection.AsVector3D();

            _originalUp = up;
            _originalForward = forward;
            _rotation = CreateRotationQuaternion(up, forward);
        }

        private Quaternion CreateRotationQuaternion(Vector3D up, Vector3D forward)
        {
            var left = Vector3D.Cross(up, forward);
            var mat = new Matrix3x3();
            mat[1, 1] = forward.X;
            mat[2, 1] = forward.Y;
            mat[3, 1] = forward.Z;
            mat[1, 2] = left.X;
            mat[2, 2] = left.Y;
            mat[3, 2] = left.Z;
            mat[1, 3] = up.X;
            mat[2, 3] = up.Y;
            mat[3, 3] = up.Z;
            return new Quaternion(mat);
        }

        public void Pan(float x, float y)
        {
            var up = _rotation.GetMatrix() * new Vector3D(0, 1, 0) ;
            var right = _rotation.GetMatrix() * new Vector3D(1, 0, 0);
            _position += up * y + right * x;
            _camera.Position = _position.AsPoint3D();
            
            Debug.WriteLine($"Camera position: {_camera.Position}. Up: {up} Right: {right}");
        }

        public void Zoom(in float delta)
        {
            var forward = _rotation.GetMatrix() * new Vector3D(0, 0, 1);
            forward.Normalize();
            
            _position += forward * delta;
            _camera.Position = _position.AsPoint3D();
            
            Debug.WriteLine($"Camera position: {_camera.Position}. Forward: {forward}");
        }

        public void Reset()
        {
            Debug.WriteLine("Resetting camera");
            _position = (_camera.Position = _originalPosition.AsPoint3D()).AsVector3D();
            _rotation = CreateRotationQuaternion(_originalUp, _originalForward);
            _camera.LookDirection = _originalForward.AsMVector3D();
            _camera.UpDirection = _originalUp.AsMVector3D();
        }

    }
}