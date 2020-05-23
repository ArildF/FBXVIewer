using System.Windows.Controls;

namespace FBXViewer
{
    public partial class MeshPreviewSettings : UserControl
    {
        public MeshPreviewSettings(MeshPreviewSettingsViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}