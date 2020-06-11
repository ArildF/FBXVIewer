using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Assimp;
using OpenGL;
using Matrix4x4 = System.Numerics.Matrix4x4;
using PrimitiveType = OpenGL.PrimitiveType;

namespace FBXViewer.OpenGL
{
    public class GLMesh
    {
        private uint? _vertexBuffer;
        private uint _indexBuffer;
        private readonly Vector3[] _vertexArray;
        private readonly uint[] _indexArray;
        private readonly Vector2[] _uvArray;
        private uint _uvBuffer;

        public GLMesh(Vector3[] vertexArray, uint[] indexArray, Vector2[] uvArray)
        {
            _vertexArray = vertexArray;
            _indexArray = indexArray;
            _uvArray = uvArray;
            ModelMatrix = Matrix4x4.Identity;
        }

        public Matrix4x4 ModelMatrix { get; }  

        public void Render()
        {
            if (_vertexBuffer == null)
            {
                CreateBuffers();
            }
            Gl.EnableVertexAttribArray(0);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer.Value);
            Gl.VertexAttribPointer(0, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);
            
            Gl.EnableVertexAttribArray(1);
            Gl.BindBuffer(BufferTarget.ArrayBuffer, _uvBuffer);
            Gl.VertexAttribPointer(1, 2, VertexAttribType.Float, false, 0, IntPtr.Zero);
            
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
            
            Gl.DrawElements(PrimitiveType.Triangles, _indexArray.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
            
            Gl.DisableVertexAttribArray(0);
        }


        private void CreateBuffers()
        {
            _vertexBuffer = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer.Value);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint) (_vertexArray.Length * 12), _vertexArray, BufferUsage.StaticDraw);

            _indexBuffer = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
            Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint) (_indexArray.Length * sizeof(int)), _indexArray, BufferUsage.StaticDraw);

            _uvBuffer = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, _uvBuffer);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(_uvArray.Length * 8), _uvArray, BufferUsage.StaticDraw);
        }
    }
}