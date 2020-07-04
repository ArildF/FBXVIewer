using System.Windows.Controls;

namespace FBXViewer
{
    public partial class MeshViewSettings : UserControl
    {
        public MeshViewSettings(MeshViewSettingsViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}