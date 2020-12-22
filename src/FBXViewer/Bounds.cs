using System.Numerics;

namespace FBXViewer
{
    public struct Bounds
    {
        public float SizeX;
        public float SizeY;
        public float SizeZ;
        public Vector3 Location;

        public Bounds(float sizeX, float sizeY, float sizeZ, Vector3 location)
        {
            SizeX = sizeX;
            SizeY = sizeY;
            SizeZ = sizeZ;
            Location = location;
        }
        
        public static Bounds operator *(Bounds bounds, Matrix4x4 matrix4X4)
        {
            return new Bounds(bounds.SizeX, bounds.SizeY, bounds.SizeZ,
                Vector3.Transform(bounds.Location, matrix4X4));
        }
    }
}