using System;
using OpenGL;
using Matrix4x4 = System.Numerics.Matrix4x4;
using PrimitiveType = OpenGL.PrimitiveType;

namespace FBXViewer.OpenGL
{
    public class GLMesh
    {
        private readonly uint _vertexBuffer;
        private readonly uint _indexBuffer;
        private readonly  uint _uvBuffer;
        private readonly uint _normalBuffer;
        private readonly int _indexCount;


        public GLMesh(in uint vertexBuffer, in uint indexBuffer, in uint uvBuffer, in uint normalBuffer, int indexCount)
        {
            _vertexBuffer = vertexBuffer;
            _indexBuffer = indexBuffer;
            _uvBuffer = uvBuffer;
            _normalBuffer = normalBuffer;
            _indexCount = indexCount;
            ModelMatrix = Matrix4x4.Identity;
        }

        public Matrix4x4 ModelMatrix { get; }
        public Texture DiffuseTexture { get; set; }

        public void Render(int diffuseTextureId)
        {
            Gl.EnableVertexAttribArray(0);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            Gl.VertexAttribPointer(0, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);
            
            Gl.EnableVertexAttribArray(1);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, _uvBuffer);
            Gl.VertexAttribPointer(1, 2, VertexAttribType.Float, false, 0, IntPtr.Zero);
            
            Gl.EnableVertexAttribArray(2);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, _normalBuffer);
            Gl.VertexAttribPointer(2, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);
            
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);

            if (DiffuseTexture != null)
            {
                Gl.ActiveTexture(TextureUnit.Texture0);
                Gl.BindTexture(TextureTarget.Texture2d, DiffuseTexture.Buffer);
                Gl.Uniform1i(diffuseTextureId, 1, 0);
            }
            
            Gl.DrawElements(PrimitiveType.Triangles, _indexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
            
            Gl.DisableVertexAttribArray(0);
        }
    }
}