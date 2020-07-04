using ReactiveUI;

namespace FBXViewer
{
    public class MeshViewSettingsViewModel : ReactiveObject
    {
        public MeshViewSettingsViewModel()
        {
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
            }
        }


        private float _zRotation;

        public float ZRotation
        {
            get => _zRotation;
            set
            {
                this.RaiseAndSetIfChanged(ref _zRotation, value);
            }
        }
        
        private float _yRotation;

        public float YRotation
        {
            get => _yRotation;
            set
            {
                this.RaiseAndSetIfChanged(ref _yRotation, value);
            }
        }
        
        private float _xRotation;

        public float XRotation
        {
            get => _xRotation;
            set
            {
                this.RaiseAndSetIfChanged(ref _xRotation, value);
            }
        }

        private string _cameraType = "Perspective";

        public string CameraType
        {
            get => _cameraType;
            set
            {
                this.RaiseAndSetIfChanged(ref _cameraType, value);
            }
        }

        private bool _meshEnabled;

        public bool MeshEnabled
        {
            get => _meshEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _meshEnabled, value);
            }
        }

        private float _lightStrength;

        public float LightStrength
        {
            get => _lightStrength;
            set => this.RaiseAndSetIfChanged(ref _lightStrength, value);
        }
    }
}