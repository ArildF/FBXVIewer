using System.Windows.Media.Media3D;
using ReactiveUI;

namespace FBXViewer
{
    public class MeshPreviewSettingsViewModel : ReactiveObject
    {
        private ModelPreview _modelPreview;

        public MeshPreviewSettingsViewModel(ModelPreview modelPreview)
        {
            _modelPreview = modelPreview;
            _wireFrameEnabled = true;
            _meshEnabled = true;
        }

        private bool _wireFrameEnabled;

        public bool WireFrameEnabled
        {
            get => _wireFrameEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _wireFrameEnabled, value);
                _modelPreview.ToggleWireFrame(_wireFrameEnabled);
            }
        }

        private Quaternion _rotation;

        public Quaternion Rotation
        {
            get => _rotation;
            set => this.RaiseAndSetIfChanged(ref _rotation, value);
        }

        private float _zRotation;

        public float ZRotation
        {
            get => _zRotation;
            set
            {
                this.RaiseAndSetIfChanged(ref _zRotation, value);
                UpdateRotation();
            }
        }
        
        private float _yRotation;

        public float YRotation
        {
            get => _yRotation;
            set
            {
                this.RaiseAndSetIfChanged(ref _yRotation, value);
                UpdateRotation();
            }
        }
        
        private float _xRotation;

        public float XRotation
        {
            get => _xRotation;
            set
            {
                this.RaiseAndSetIfChanged(ref _xRotation, value);
                UpdateRotation();
            }
        }

        private bool _meshEnabled;

        public bool MeshEnabled
        {
            get => _meshEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _meshEnabled, value);
                _modelPreview.ToggleMesh(_meshEnabled);
            }
        }

        private void UpdateRotation()
        {
            Rotation = 
                new Quaternion(new Vector3D(1, 0, 0), XRotation) *
                new Quaternion(new Vector3D(0, 1, 0), YRotation) *
                new Quaternion(new Vector3D(0, 0, 1), ZRotation);
        }
    }
}