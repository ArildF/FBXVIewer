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
    }
}