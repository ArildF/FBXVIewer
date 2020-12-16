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
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Sdl;
using Brushes = System.Windows.Media.Brushes;
using Control = System.Windows.Forms.Control;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace FBXViewer.OpenGL.Silk.Net
{
    public unsafe class SilkScene : IScene
    {
        private readonly MeshLoader _meshLoader;
        private readonly MeshViewSettingsViewModel _settingsViewModel;

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

        public SilkScene(MeshLoader meshLoader, MeshViewSettingsViewModel settingsViewModel)
        {
            _meshLoader = meshLoader;
            _settingsViewModel = settingsViewModel;
            _glControl = new System.Windows.Forms.Control();
                
                
            // glControl.ContextCreated += GlControlOnContextCreated;
            // glControl.Render += GlControlOnRender;


            var grid = new Grid {Background = Brushes.Aqua};

            var windowsFormsHost = new WindowsFormsHost {Child = _glControl};
            _view = SdlWindowing.CreateFrom((void*) _glControl.Handle);

            var window = _view as IWindow;

            MouseInput = new WinFormsMouseInput(_glControl);
            
            grid.Children.Add(windowsFormsHost);

            Visual = grid;

            CameraLight = new OpenGLLight();
            _openGLCamera =  new OpenGLRendererCamera(-Vector3.UnitZ, Vector3.UnitZ, Vector3.UnitY);
            
            _view.Render += ViewOnRender;
            
            _glControl.CreateControl();
            
            ViewOnLoad();
        }

        private void ViewOnLoad()
        {
            _gl = GL.GetApi(_view);
            
            CreateShaders();
            
            _gl.Enable(EnableCap.DepthTest);
            _gl.DepthFunc(DepthFunction.Less);
            
            _gl.Enable(EnableCap.Multisample);
            
            _view.Run();
        }

        public IRendererCamera RendererCamera => _openGLCamera;

        public UIElement Visual { get; }

        public ILight? CameraLight { get; }

        private readonly OpenGLRendererCamera _openGLCamera;

        private uint _program;

        private readonly List<MeshEntry> _meshes = new List<MeshEntry>();

        private readonly IView _view;

        private GL _gl;
        private readonly Control? _glControl;

        private void ViewOnRender(double obj)
        {
            if (_gl == null)
            {
                return;
            }
            var senderControl = _glControl;
            int vpx = 0;
            int vpy = 0;
            uint vpw = (uint) (senderControl?.ClientSize.Width ?? 1);
            uint vph = (uint) (senderControl?.ClientSize.Height ?? 1);

            _gl.Viewport(vpx, vpy, vpw, vph);
            _gl.Clear((uint) (GLEnum.ColorBufferBit | GLEnum.DepthBufferBit));

            var projectionMatrix = _openGLCamera.ProjectionMatrix(vpw, vph);
            var viewMatrix = _openGLCamera.ViewMatrix;

            _gl.UseProgram(_program);

            var u = Uniforms.Get(_program);

            foreach (var meshEntry in _meshes.Where(m => m.Enabled))
            {
                var modelMatrix = meshEntry.GLMesh.ModelMatrix;
                _gl.UniformMatrix4(u.M, 1, true, modelMatrix);
                _gl.UniformMatrix4(u.P, 1, true, projectionMatrix);
                _gl.UniformMatrix4(u.V, 1, true, viewMatrix);
                _gl.Uniform3(u.LightPosition, CameraLight?.Position ?? new Vector3(-50, 200, 50));
                _gl.Uniform1(u.LightPower, 1, _settingsViewModel.LightStrength);
                _gl.Uniform1(u.LightPower, 1, _settingsViewModel.LightStrength);
                _gl.Uniform1(u.Ambient, 1, _settingsViewModel.Ambient);
                _gl.Uniform1(u.SpecularStrength, 1, _settingsViewModel.SpecularMapStrength);

                meshEntry.GLMesh.Render(u);
            }
        }

        private void CreateShaders()
        {
            var fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
            var vertexShader = _gl.CreateShader(ShaderType.VertexShader);

            void CompileShader(uint id, string file)
            {
                var source = LoadShaderFromResource(file);
                _gl.ShaderSource(id, source);
                _gl.CompileShader(id);

                _gl.GetShader(id, ShaderParameterName.CompileStatus, out int compileStatus);
                Debug.WriteLine($"Compile status: {compileStatus}");
                var log  = _gl.GetShaderInfoLog(id);
                Debug.WriteLine(log);
            }
            
            CompileShader(fragmentShader, "FragmentShader.glsl");
            CompileShader(vertexShader, "VertexShader.glsl");

            _program = _gl.CreateProgram();
            _gl.AttachShader(_program, fragmentShader);
            _gl.AttachShader(_program, vertexShader);
            _gl.LinkProgram(_program);
            
            _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int linkResult);
            Debug.WriteLine($"Link status: {linkResult}");
            var log = _gl.GetProgramInfoLog(_program);
            Debug.WriteLine(log);
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


        public IMouseInput MouseInput { get; }
    }
}