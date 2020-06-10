using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Assimp;
using OpenGL;
using Matrix4x4 = System.Numerics.Matrix4x4;
using PrimitiveType = OpenGL.PrimitiveType;

namespace FBXViewer.OpenGL
{
    public class GLMesh
    {
        private uint? _vao;
        private readonly Vector3[] _vertexArray;

        private GLMesh(Vector3[] vertexArray)
        {
            _vertexArray = vertexArray;
            ModelMatrix = Matrix4x4.Identity;
        }

        public Matrix4x4 ModelMatrix { get; }  

        public void Render()
        {
            if (_vao == null)
            {
                _vao = CreateVao(_vertexArray);
            }
            Gl.EnableVertexAttribArray(0);
            var err = Gl.GetError();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, _vao.Value);
            Gl.VertexAttribPointer(0, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);
            
            Gl.DrawArrays(PrimitiveType.Triangles, 0, _vertexArray.Length);
            
            Gl.DisableVertexAttribArray(0);
        }

        public static GLMesh Create(Mesh mesh)
        {
            var vertices = new List<Vector3>(mesh.Faces.Count * 4);
            foreach (var face in mesh.Faces)
            {
                vertices.Add(mesh.Vertices[face.Indices[0]].AsVector3());
                vertices.Add(mesh.Vertices[face.Indices[1]].AsVector3());
                vertices.Add(mesh.Vertices[face.Indices[2]].AsVector3());
                if (face.IndexCount == 4)
                {
                    vertices.Add(mesh.Vertices[face.Indices[0]].AsVector3());
                    vertices.Add(mesh.Vertices[face.Indices[2]].AsVector3());
                    vertices.Add(mesh.Vertices[face.Indices[3]].AsVector3());
                }

                if (face.IndexCount > 4)
                {
                    Debug.WriteLine($"Found {face.IndexCount}gon, only generating quad");
                }
            }

            var vertexArray = vertices.ToArray();

            return new GLMesh(vertexArray);
        }

        private static uint CreateVao(Vector3[] vertexArray)
        {
            var vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            uint vertexBuffer = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint) (vertexArray.Length * 12), vertexArray, BufferUsage.StaticDraw);
            return vao;
        }
    }
}