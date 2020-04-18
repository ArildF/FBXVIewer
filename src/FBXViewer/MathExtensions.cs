using System.Windows.Media.Media3D;
using Vector3D = Assimp.Vector3D;
using MVector3D = System.Windows.Media.Media3D.Vector3D;
namespace FBXViewer
{
    public static class MathExtensions
    {
        public static Vector3D AsVector3D(this Point3D self)
        {
            return new Vector3D((float)self.X, (float)self.Y, (float)self.Z);
        }
        
        public static Vector3D AsVector3D(this Size3D self)
        {
            return new Vector3D((float)self.X, (float)self.Y, (float)self.Z);
        }
        
        public static Vector3D AsVector3D(this MVector3D self)
        {
            return new Vector3D((float)self.X, (float)self.Y, (float)self.Z);
        }

        public static MVector3D AsMVector3D(this Vector3D self)
        {
            return new MVector3D(self.X, self.Y, self.Z);
        }
        
        public static Point3D AsPoint3D(this Vector3D self)
        {
            return new Point3D(self.X, self.Y, self.Z);
        }
    }
}