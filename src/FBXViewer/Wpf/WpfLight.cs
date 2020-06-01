using System.Numerics;
using System.Windows.Media.Media3D;

namespace FBXViewer.Wpf
{
    public class WpfLight : ILight
    {
        private readonly PointLightBase _light;

        public WpfLight(PointLightBase light)
        {
            _light = light;
        }

        public Vector3 Position
        {
            get => _light.Position.AsVector3();
            set => _light.Position = value.AsPoint3D();
        }
    }
}