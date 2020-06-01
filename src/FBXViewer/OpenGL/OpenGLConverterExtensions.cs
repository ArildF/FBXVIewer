using System.Numerics;
using OpenGL;

namespace FBXViewer.OpenGL
{
    public static class OpenGLConverterExtensions
    {
        public static Vertex3f AsVertex3f(this Vector3 vector3)
        {
            return new Vertex3f(vector3.X, vector3.Y, vector3.Z);
        }
        
    }
}