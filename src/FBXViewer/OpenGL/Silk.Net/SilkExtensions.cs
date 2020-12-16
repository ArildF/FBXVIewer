using System.Numerics;
using Silk.NET.OpenGL;

namespace FBXViewer.OpenGL.Silk.Net
{
    public static unsafe class SilkExtensions
    {
        public static unsafe void UniformMatrix4(this GL gl, int location, uint count, bool transpose, Matrix4x4 matrix)
        {
            gl.UniformMatrix4(location, count, transpose, &matrix.M11);
        }

        public static unsafe void Uniform3(this GL gl, int location, Vector3 vector)
        {
            gl.Uniform3(location, vector.X, vector.Y, vector.Z);
        }
    }
}