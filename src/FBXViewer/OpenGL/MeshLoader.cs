using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Assimp;
using OpenGL;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace FBXViewer.OpenGL
{
    public class MeshLoader
    {
        private readonly TextureLoader _loader;

        public MeshLoader(TextureLoader loader)
        {
            _loader = loader;
        }

        public GLMesh Create(Mesh mesh, Matrix4x4 transform)
        {
            var vertexIndexes = new List<uint>(mesh.Faces.Count * 4);
            var uvs = new List<Vector2>(mesh.Faces.Count * 4);
            foreach (var face in mesh.Faces)
            {
                void Add(params int[] index)
                {
                    for (int i = 0; i < index.Length; i++)
                    {
                        vertexIndexes.Add((uint)face.Indices[index[i]]); 
                    }
                }
                Add(0, 1, 2);
                if (face.IndexCount == 4)
                {
                    Add(0, 2,3);
                }

                if (face.IndexCount > 4)
                {
                    Debug.WriteLine($"Found {face.IndexCount}gon, only generating quad");
                }
            }

            var indexArray = vertexIndexes.ToArray();
            var vertexArray = mesh.Vertices.Select(v => v.AsVector3()).ToArray();
            var uvArray = mesh.TextureCoordinateChannels[0]
                .Select(uv => new Vector2(uv.X, uv.Y)).ToArray();
            var normalArray = mesh.Normals.Select(n => n.AsVector3()).ToArray();
            
            uint vertexBuffer = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint) (vertexArray.Length * 12), vertexArray, BufferUsage.StaticDraw);

            uint indexBuffer = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint) (indexArray.Length * sizeof(int)), indexArray, BufferUsage.StaticDraw);

            uint uvBuffer = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, uvBuffer);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(uvArray.Length * 8), uvArray, BufferUsage.StaticDraw);

            uint normalBuffer = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, normalBuffer);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint) (normalArray.Length * 12), normalArray, BufferUsage.StaticDraw);


            var texture = _loader.LoadDiffuse(mesh);
            return new GLMesh(vertexBuffer, indexBuffer, uvBuffer, normalBuffer, indexArray.Length)
            {
                DiffuseTexture = texture,
                ModelMatrix = transform,
            };
        }
    }
}