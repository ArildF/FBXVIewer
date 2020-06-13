using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Assimp;
using OpenGL;

namespace FBXViewer.OpenGL
{
    public class MeshLoader
    {
        private readonly TextureProvider _textureProvider;

        public MeshLoader(TextureProvider textureProvider)
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
            
            uint vertexBuffer = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint) (vertexArray.Length * 12), vertexArray, BufferUsage.StaticDraw);

            uint indexBuffer = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer);
            Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint) (indexArray.Length * sizeof(int)), indexArray, BufferUsage.StaticDraw);

            uint uvBuffer = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, uvBuffer);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(uvArray.Length * 8), uvArray, BufferUsage.StaticDraw);

            uint? textureId = LoadTexture(mesh);

            return new GLMesh(vertexBuffer, indexBuffer, uvBuffer, indexArray.Length) {DiffuseTextureId = textureId};
        }

        private uint? LoadTexture(Mesh mesh)
        {
            var bitmap = _textureProvider.GetDiffuseTexture(mesh);
            if (bitmap == null)
            {
                return null;
            }
            int stride = bitmap.PixelWidth * ((bitmap.Format.BitsPerPixel + 7) / 8);
            byte[] pixels = new byte[stride * bitmap.PixelHeight];
            bitmap.CopyPixels(pixels, stride, 0);

            uint textureId = Gl.GenTexture();
            Gl.BindTexture(TextureTarget.Texture2d, textureId);

            var (internalFormat, pixelFormat) = bitmap.Format.BitsPerPixel switch
            {
                32 => (InternalFormat.Rgba, PixelFormat.Bgra),
                24 => (InternalFormat.Rgb, PixelFormat.Bgr),
                _ => throw new ArgumentException(nameof(bitmap))
            };
            Gl.TexImage2D(TextureTarget.Texture2d, 0, internalFormat, bitmap.PixelWidth, bitmap.PixelHeight,
                0, pixelFormat, PixelType.UnsignedByte, pixels);
            var error = Gl.GetError();
            if (error != ErrorCode.NoError)
            {
                throw new Exception(error.ToString());
            }

            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, Gl.NEAREST);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, Gl.NEAREST);

            return textureId;
        }
    }
}