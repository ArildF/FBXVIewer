using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using Assimp;
using OpenGL;
using PixelFormat = OpenGL.PixelFormat;

namespace FBXViewer.OpenGL
{
    public class MeshLoader
    {
        private readonly TextureProvider<Bitmap> _textureProvider;

        public MeshLoader(TextureProvider<Bitmap> textureProvider)
        {
            _textureProvider = textureProvider;
        }

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

            uint? textureId = LoadTexture(mesh);

            return new GLMesh(vertexBuffer, indexBuffer, uvBuffer, normalBuffer, indexArray.Length) {DiffuseTextureId = textureId};
        }

        private uint? LoadTexture(Mesh mesh)
        {
            var bitmap = _textureProvider.GetDiffuseTexture(mesh);
            if (bitmap == null)
            {
                return null;
            }

            uint textureId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2d, textureId);

            var (internalFormat, pixelFormat) = bitmap.PixelFormat switch
            {
                System.Drawing.Imaging.PixelFormat.Format32bppArgb => (InternalFormat.Rgba, PixelFormat.Bgr),
                System.Drawing.Imaging.PixelFormat.Format24bppRgb => (InternalFormat.Rgb, PixelFormat.Bgr),
                // 4 => (InternalFormat.Rgba, PixelFormat.Rgba),
                // 3 => (InternalFormat.Rgb, PixelFormat.Rgb),
                _ => throw new ArgumentException(nameof(bitmap))
            };
            var bits = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);
            {
                Gl.TexImage2D(TextureTarget.Texture2d, 0, internalFormat, bitmap.Width, bitmap.Height,
                    0, pixelFormat, PixelType.UnsignedByte, bits.Scan0);
            }

            var error = Gl.GetError();
            if (error != ErrorCode.NoError)
            {
                throw new Exception(error.ToString());
            }

            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, Gl.LINEAR);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, Gl.LINEAR_MIPMAP_LINEAR);
            Gl.GenerateMipmap(TextureTarget.Texture2d);

            return textureId;
        }
    }
}