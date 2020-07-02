using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using Assimp;
using OpenGL;
using Brushes = System.Windows.Media.Brushes;
using Matrix4x4 = System.Numerics.Matrix4x4;
using Quaternion = System.Windows.Media.Media3D.Quaternion;

namespace FBXViewer.OpenGL
{
    public class OpenGLScene : IScene
    {
        private readonly MeshLoader _meshLoader;

        private class MeshEntry
        {
            public readonly Mesh Mesh;
            public readonly GLMesh GLMesh;
            public bool Enabled;

            public MeshEntry(Mesh mesh, GLMesh glMesh)
            {
                Mesh = mesh;
                GLMesh = glMesh;
                Enabled = true;
            }
        }

        public OpenGLScene(MeshLoader meshLoader)
        {
            _meshLoader = meshLoader;
            var glControl = new GlControl
            {
                Animation = true,
                DepthBits = 24,
                MultisampleBits = 32,
            };
            glControl.ContextCreated += GlControlOnContextCreated;
            glControl.Render += GlControlOnRender;


            var grid = new Grid {Background = Brushes.Aqua};

            var windowsFormsHost = new WindowsFormsHost {Child = glControl};

            MouseInput = new WinFormsMouseInput(glControl);
            
            grid.Children.Add(windowsFormsHost);

            Visual = grid;

            CameraLight = new OpenGLLight();
            _openGLCamera =  new OpenGLRendererCamera(-Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitY);
            
            glControl.CreateControl();
        }

        public IRendererCamera RendererCamera => _openGLCamera;
        public UIElement Visual { get; }
        public ILight? CameraLight { get; }

        private readonly OpenGLRendererCamera _openGLCamera;
        private uint _program;
        private readonly List<MeshEntry> _meshes = new List<MeshEntry>();

        private void GlControlOnRender(object? sender, GlControlEventArgs e)
        {
            var senderControl = sender as GlControl;

            int vpx = 0;
            int vpy = 0;
            int vpw = senderControl?.ClientSize.Width ?? 1;
            int vph = senderControl?.ClientSize.Height ?? 1;

            Gl.Viewport(vpx, vpy, vpw, vph);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var projectionMatrix = _openGLCamera.ProjectionMatrix(vpw, vph);
            var viewMatrix = _openGLCamera.ViewMatrix;

            Gl.UseProgram(_program);
            
            var mLocation = Gl.GetUniformLocation(_program, "M");
            var vLocation = Gl.GetUniformLocation(_program, "V");
            var pLocation = Gl.GetUniformLocation(_program, "P");
            var lightPositionLocation = Gl.GetUniformLocation(_program, "LightPosition_worldSpace");
            var diffuseSampler = Gl.GetUniformLocation(_program, "diffuseTextureSampler");

            foreach (var meshEntry in _meshes.Where(m => m.Enabled))
            {
                var modelMatrix = meshEntry.GLMesh.ModelMatrix;
                Gl.UniformMatrix4f(mLocation, 1, true, modelMatrix);
                Gl.UniformMatrix4f(pLocation, 1, true, projectionMatrix);
                Gl.UniformMatrix4f(vLocation, 1, true, viewMatrix);
                Gl.Uniform3f(lightPositionLocation, 1, new Vector3(-50, 200, 50));

                meshEntry.GLMesh.Render(diffuseSampler);
            }
        }

        private void GlControlOnContextCreated(object? sender, GlControlEventArgs e)
        {
            CreateShaders();
            
            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Less);
            
            Gl.Enable(EnableCap.Multisample);
        }

        private void CreateShaders()
        {
            var fragmentShader = Gl.CreateShader(ShaderType.FragmentShader);
            var vertexShader = Gl.CreateShader(ShaderType.VertexShader);

            var sb = new StringBuilder(1000);
            
            void CompileShader(uint id, string file)
            {
                var source = LoadShaderFromResource(file);
                Gl.ShaderSource(id, new[]{source});
                Gl.CompileShader(id);

                Gl.GetShader(id, ShaderParameterName.CompileStatus, out int compileStatus);
                Debug.WriteLine($"Compile status: {compileStatus == Gl.TRUE}");
                Gl.GetShaderInfoLog(id, sb.Capacity, out int _, sb);
                Debug.WriteLine(sb.ToString());
            }
            
            CompileShader(fragmentShader, "FragmentShader.glsl");
            CompileShader(vertexShader, "VertexShader.glsl");

            _program = Gl.CreateProgram();
            Gl.AttachShader(_program, fragmentShader);
            Gl.AttachShader(_program, vertexShader);
            Gl.LinkProgram(_program);
            
            Gl.GetProgram(_program, ProgramProperty.LinkStatus, out int linkResult);
            Debug.WriteLine($"Link status: {linkResult == Gl.TRUE}");
            Gl.GetProgramInfoLog(_program, sb.Capacity, out int _, sb);
        }

        private string LoadShaderFromResource(string file)
        {
            var uri = new Uri("/OpenGL/Shaders/" + file, UriKind.Relative);
            var stream = Application.GetResourceStream(uri);
            using var reader = new StreamReader(stream.Stream);
            return reader.ReadToEnd();
        }

        public void LoadMesh(Mesh mesh, Matrix4x4 transform)
        {
            var entry = _meshes.FirstOrDefault(m => m.Mesh == mesh);
            if (entry != null)
            {
                entry.Enabled = true;
                return;
            }
            _meshes.Add(new MeshEntry(mesh, _meshLoader.Create(mesh, transform)));
        }

        public void SetShapeKeyWeight(Mesh mesh, float weight, MeshAnimationAttachment attachment)
        {
        }

        public void UnloadMesh(Mesh mesh)
        {
            var entry = _meshes.FirstOrDefault(m => m.Mesh == mesh);
            if (entry != null)
            {
                entry.Enabled = false;
            }
        }

        public bool RayCast(Vector2 mousePos, out RayCastResult rayCastResult)
        {
            rayCastResult = new RayCastResult();
            return false;
        }

        public Bounds GetBoundingBox(Mesh mesh)
        {
            var entry = _meshes.FirstOrDefault(m => m.Mesh == mesh);
            if (entry == null)
            {
                return new Bounds();
            }

            var matrix = Matrix4x4.Transpose(entry.GLMesh.ModelMatrix);
            return mesh.BoundingBox.ToBounds() * matrix;
        }

        public void ToggleWireFrame(in bool wireFrameEnabled)
        {
        }

        public void ToggleMesh(in bool meshEnabled)
        {
        }

        public void SetRootRotation(Quaternion quaternion)
        {
        }

        public IMouseInput MouseInput { get; }
    }
}