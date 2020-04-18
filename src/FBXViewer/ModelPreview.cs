using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Assimp;
using Vector3D = Assimp.Vector3D;
using MVector3D = System.Windows.Media.Media3D.Vector3D;

namespace FBXViewer
{
    internal class ModelPreview
    {
        private readonly Camera _camera;
        private IDragHandler _dragHandler;

        public UIElement Element { get; private set; }

        public ModelPreview(Mesh mesh)
        {
            var geometry = new MeshGeometry3D
            {
                Positions = new Point3DCollection(
                    mesh.Vertices.Select(v => new Point3D(v.X, v.Y, v.Z))),
                Normals = new Vector3DCollection(
                    mesh.Normals.Select(n => new MVector3D(n.X, n.Y, n.Z))),
                TriangleIndices = new Int32Collection()
            };
            foreach (var face in mesh.Faces)
            {
                geometry.TriangleIndices.Add(face.Indices[0]);
                geometry.TriangleIndices.Add(face.Indices[1]);
                geometry.TriangleIndices.Add(face.Indices[2]);
                if (face.IndexCount == 4)
                {
                    geometry.TriangleIndices.Add(face.Indices[0]);
                    geometry.TriangleIndices.Add(face.Indices[2]);
                    geometry.TriangleIndices.Add(face.Indices[3]);
                }
                if (face.IndexCount > 4)
                {
                    Debug.WriteLine($"Found {face.IndexCount}gon, only generating quad");
                }
            }

            var geometryModel = new GeometryModel3D
            {
                Material = new MaterialGroup
                {
                    Children = new MaterialCollection
                    {
                        new DiffuseMaterial(Brushes.Pink),
                        // new SpecularMaterial(Brushes.Red, 1)
                    }
                },
                Geometry = geometry,
            };


            var group = new Model3DGroup();
            group.Children.Add(geometryModel);

            var modelVisual = new ModelVisual3D {Content = @group};

            var viewPort = new Viewport3D();
            viewPort.Children.Add(modelVisual);

            var center = geometry.Bounds.Location.AsVector3D() + (geometry.Bounds.Size.AsVector3D() / 2);
            var biggestExtent = new[] {geometry.Bounds.SizeX, geometry.Bounds.SizeY, geometry.Bounds.SizeZ}
                .OrderByDescending(s => s).First();
            var cameraOffset = biggestExtent * 2f;
            var cameraPosition = center + new Vector3D(0, 0, (float)+cameraOffset);
            var lookDir = (center - cameraPosition);
            
            Debug.WriteLine($"center: {center}, cameraPosition: {cameraPosition}, lookDir: {lookDir}");

            var perspectiveCamera = new PerspectiveCamera(
                new Point3D(cameraPosition.X, cameraPosition.Y, cameraPosition.Z),
                lookDir.AsMVector3D(), new MVector3D(0, 1, 0), 50f);

            _camera = new Camera(perspectiveCamera);
            
            
            var light = new PointLight(Colors.Cornsilk, perspectiveCamera.Position){};
            group.Children.Add(light);

            viewPort.Camera = perspectiveCamera;

            var border = new Border {Background = Brushes.Black};
            border.AddHandler(UIElement.PreviewMouseWheelEvent, new MouseWheelEventHandler(MouseWheel), true);
            border.AddHandler(UIElement.PreviewMouseMoveEvent, new MouseEventHandler(MouseMove), true);
            border.AddHandler(UIElement.PreviewMouseDownEvent, new MouseButtonEventHandler(MouseDown), true);
            border.AddHandler(UIElement.PreviewMouseUpEvent, new MouseButtonEventHandler(MouseUp), true);
            border.AddHandler(UIElement.PreviewKeyDownEvent, new KeyEventHandler(KeyDown), true);
            border.Child = viewPort;

            Element = border;
        }

        private void KeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine($"Key down {e.Key}");
            if (e.Key == Key.Decimal)
            {
                _camera.Reset();
            }
        }

        private void MouseUp(object sender, MouseEventArgs e)
        {
            _dragHandler = null;
            Element.ReleaseMouseCapture();
        }

        private void MouseDown(object sender, MouseEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed && Keyboard.IsKeyDown(Key.LeftShift))
            {
                _dragHandler = new PanDragHandler(this, e);
                Element.CaptureMouse();
            }

        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            _dragHandler?.MouseDrag(e);
        }

        private void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var delta = e.Delta * -0.12f;
            _camera.Zoom(delta);
        }

        private interface IDragHandler
        {
            void MouseDrag(MouseEventArgs args);
        }
        
        private abstract class DragHandlerBase : IDragHandler
        {
            protected readonly ModelPreview Outer;
            private Point _pos;
            private DateTime _time;

            protected DragHandlerBase(ModelPreview outer, MouseEventArgs args)
            {
                _pos = args.GetPosition(outer.Element);
                Outer = outer;
                _time = DateTime.Now;
            }

            public void MouseDrag(MouseEventArgs args)
            {
                var timeNow = DateTime.Now;
                var newPos = args.GetPosition(Outer.Element);
                var deltaTime = (timeNow - _time).TotalSeconds;
                var delta = (newPos - _pos) * deltaTime;

                _time = timeNow;

                DoMouseDrag(delta);

            }

            protected abstract void DoMouseDrag(Vector delta);
        }

        private class PanDragHandler : DragHandlerBase
        {
            public PanDragHandler(ModelPreview outer, MouseEventArgs mouseEventArgs) : base(outer, mouseEventArgs)
            {
            }
            protected override void DoMouseDrag(Vector delta)
            {
                Outer._camera.Pan((float) delta.X, (float) delta.Y);
            }
        }
    }
}