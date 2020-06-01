using System.Numerics;
using Assimp;

namespace FBXViewer
{
    public readonly struct RayCastResult
    {
        public RayCastResult(Vector3 pointHit)
        {
            PointHit = pointHit;
        }

        public Vector3 PointHit { get; }
    }
}