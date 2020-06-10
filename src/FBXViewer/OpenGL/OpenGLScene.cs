using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using Assimp;
using OpenGL;
using Brushes = System.Windows.Media.Brushes;
using Quaternion = System.Windows.Media.Media3D.Quaternion;

namespace FBXViewer.OpenGL
{
    public class OpenGLScene : IScene
    {
        public OpenGLScene()
        {
            var glControl = new GlControl
            {
                Animation = true
            };
            glControl.ContextCreated += GlControlOnContextCreated;
            glControl.Render += GlControlOnRender;

            var grid = new Grid();
            grid.Background = Brushes.Aqua;

            var winformsHost = new WindowsFormsHost();
            winformsHost.Child = glControl;

            MouseInput = new WinFormsMouseInput(glControl);
            
            grid.Children.Add(winformsHost);

            Visual = grid;

            CameraLight = new OpenGLLight();
            _openGLCamera =  new OpenGLRendererCamera(-Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitY);
        }

        public IRendererCamera RendererCamera => _openGLCamera;
        public UIElement Visual { get; }
        public ILight? CameraLight { get; }

        private readonly OpenGLRendererCamera _openGLCamera;
        private uint _program;
        private readonly List<GLMesh> _meshes = new List<GLMesh>();


        private void GlControlOnRender(object? sender, GlControlEventArgs e)
        {
            var senderControl = sender as GlControl;

            int vpx = 0;
            int vpy = 0;
            int vpw = senderControl?.ClientSize.Width ?? 1;
            int vph = senderControl?.ClientSize.Height ?? 1;

            Gl.Viewport(vpx, vpy, vpw, vph);
            Gl.Clear(ClearBufferMask.ColorBufferBit);

            var matrix = _openGLCamera.ProjectionMatrix(vpw, vph) * _openGLCamera.ViewMatrix;

            Gl.UseProgram(_program);
            
            var location = Gl.GetUniformLocation(_program, "MVP");

            foreach (var glMesh in _meshes)
            {
                var modelMatrix = matrix * glMesh.ModelMatrix;
                Gl.UniformMatrix4f(location, 1, true, modelMatrix);

                glMesh.Render();
            }
        }

        private void GlControlOnContextCreated(object? sender, GlControlEventArgs e)
        {
            CreateShaders();
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

                Gl.GetShader(id, ShaderParameterName.CompileStatus, out int result);
                Debug.WriteLine($"Compile status: {result == Gl.TRUE}");
                Gl.GetShaderInfoLog(id, sb.Capacity, out int length, sb);
                Debug.WriteLine(sb.ToString());
            }
            
            CompileShader(fragmentShader, "FragmentShader.glsl");
            CompileShader(vertexShader, "VertexShader.glsl");

            _program = Gl.CreateProgram();
            Gl.AttachShader(_program, fragmentShader);
            Gl.AttachShader(_program, vertexShader);
            Gl.LinkProgram(_program);
            
            Gl.GetProgram(_program, ProgramProperty.LinkStatus, out int result);
            Debug.WriteLine($"Link status: {result == Gl.TRUE}");
            Gl.GetProgramInfoLog(_program, sb.Capacity, out int length, sb);
        }

        private string LoadShaderFromResource(string file)
        {
            var uri = new Uri("/OpenGL/Shaders/" + file, UriKind.Relative);
            var stream = Application.GetResourceStream(uri);
            using (var reader = new StreamReader(stream.Stream))
            {
                return reader.ReadToEnd();
            }
        }

        public void LoadMesh(Mesh mesh)
        {
            _meshes.Add(GLMesh.Create(mesh));
        }

        public void SetShapeKeyWeight(Mesh mesh, float weight, MeshAnimationAttachment attachment)
        {
        }

        public void UnloadMesh(Mesh mesh)
        {
        }

        public bool RayCast(Vector2 mousePos, out RayCastResult rayCastResult)
        {
            rayCastResult = new RayCastResult();
            return false;
        }

        public Bounds GetBoundingBox(Mesh mesh)
        {
            return new Bounds(1, 1, 1, new Vector3(0, 0, 0));
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