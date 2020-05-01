using System.Windows.Controls;

namespace FBXViewer
{
    public partial class MeshPreviewSettings : UserControl
    {
        public MeshPreviewSettings(ModelPreview preview)
        {
            InitializeComponent();
            DataContext = new MeshPreviewSettingsViewModel(preview);
        }
    }
}