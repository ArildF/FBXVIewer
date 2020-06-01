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
    }
}