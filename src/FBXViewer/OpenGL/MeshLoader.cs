using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Assimp;

namespace FBXViewer.OpenGL
{
    public class MeshLoader
    {
        public GLMesh Create(Mesh mesh)
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

            return new GLMesh(vertexArray, indexArray, uvArray);
        }
    }
}