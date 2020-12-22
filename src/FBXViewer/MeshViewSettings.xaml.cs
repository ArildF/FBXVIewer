using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FBXViewer
{
    public class MeshViewSettings : UserControl
    {
        public MeshViewSettings(MeshViewSettingsViewModel vm)
        {
            AvaloniaXamlLoader.Load(this);
            DataContext = vm;
        }
    }
}