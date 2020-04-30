using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Assimp;
using MVector3D = System.Windows.Media.Media3D.Vector3D;
using Vector = System.Windows.Vector;
using Vector3D = Assimp.Vector3D;

namespace FBXViewer
{
    public class ModelPreview
    {
        private readonly Camera _camera;
        private IDragHandler _dragHandler;
        private Viewport3D _viewPort;
        private PerspectiveCamera _perspectiveCamera;

        public UIElement Element { get; }
        
        private readonly Dictionary<Mesh, ModelVisual3D> _meshes = new Dictionary<Mesh,ModelVisual3D>();

        public ModelPreview()
        {
            _viewPort = new Viewport3D();

            var center = Vector3.Zero;
            

            _perspectiveCamera = new PerspectiveCamera(
                center.AsPoint3D(),
                new Vector3(0, 0, 1).AsMVector3D(), 
                new MVector3D(0, 1, 0), 45);
            
            var lightGroup = new Model3DGroup();
            var light = new PointLight(Colors.Cornsilk, _perspectiveCamera.Position){};
            lightGroup.Children.Add(light);
            _viewPort.Children.Add(new ModelVisual3D{Content = lightGroup});
            _camera = new Camera(_perspectiveCamera, center, light);
            
            _viewPort.Camera = _perspectiveCamera;

            var border = new Border {Background = Brushes.Black};
            border.AddHandler(UIElement.PreviewMouseWheelEvent, new MouseWheelEventHandler(MouseWheel), true);
            border.AddHandler(UIElement.PreviewMouseMoveEvent, new MouseEventHandler(MouseMove), true);
            border.AddHandler(UIElement.PreviewMouseDownEvent, new MouseButtonEventHandler(MouseDown), true);
            border.AddHandler(UIElement.PreviewMouseUpEvent, new MouseButtonEventHandler(MouseUp), true);
            border.AddHandler(UIElement.PreviewKeyDownEvent, new KeyEventHandler(KeyDown), true);
            border.Child = _viewPort;

            Element = border;
        }

        public void LoadMesh(Mesh mesh)
        {
            UnloadMesh(mesh);
            
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
            // _viewPort.Children.Add(modelVisual);
            var wireFrame = CreateWireFrame(mesh);
            _viewPort.Children.Add(wireFrame);
            _meshes[mesh] = modelVisual;
            
            var center = geometry.Bounds.Location.AsVector3() + (geometry.Bounds.Size.AsVector3() / 2);
            var biggestExtent = new[] {geometry.Bounds.SizeX, geometry.Bounds.SizeY, geometry.Bounds.SizeZ}
                .OrderByDescending(s => s).First();
            var cameraOffset = biggestExtent * 2f;
            var cameraPosition = center + new Vector3(0, 0, (float)cameraOffset);
            var lookDir = Vector3.Normalize(center - cameraPosition);
            
            _camera.MoveTo(cameraPosition, lookDir, center);
        }

        private ModelVisual3D CreateWireFrame(Mesh mesh)
        {
            
            
            var set = new HashSet<(int, int)>();
            var edgesFrom = mesh.Faces.SelectMany(FindEdgesFromFace)
                .ToLookup(p => p.Item1);
            // var otherWay = pairs.Select(p => (First: p.Last, Last: p.First));
            // var edgesFrom = pairs.Concat(otherWay).ToLookup(p => p.First);
                
            var points = new Point3DCollection();
            var tris = new Int32Collection();
            foreach (var meshFace in mesh.Faces)
            {
                foreach (var edge in meshFace.Indices.Pairs().Concat(new[]{(meshFace.Indices.Last(), meshFace.Indices.First())}))
                {
                    if (set.Contains(edge))
                    {
                        continue;
                    }

                    var v1 = mesh.Vertices[edge.Item1].AsVector3();
                    var v2 = mesh.Vertices[edge.Item2].AsVector3();
                    
                    var edgeVector = (v2 - v1);
                    var normalVector = edgesFrom[edge.Item1].Where(e => e != edge)
                        .Select(e => (mesh.Vertices[e.Item2] - mesh.Vertices[e.Item1]).AsVector3())
                        .Select(v => Vector3.Cross(edgeVector, v)).Average();
                    normalVector = Vector3.Normalize(normalVector);
                    var sideVector = Vector3.Normalize(Vector3.Cross(edgeVector, normalVector));

                    var p1 = v1 - sideVector * 0.1f;
                    var p2 = v2 - sideVector * 0.1f;
                    var p3 = v2 + sideVector * 0.1f;
                    var p4 = v1 + sideVector * 0.1f;

                    var i1 = points.Count;
                    var i2 = i1 + 1;
                    var i3 = i2 + 1;
                    var i4 = i3 + 1;
                    
                    points.Add(p1.AsPoint3D());
                    points.Add(p2.AsPoint3D());
                    points.Add(p3.AsPoint3D());
                    points.Add(p4.AsPoint3D());
                    
                    tris.Add(i1);
                    tris.Add(i2);
                    tris.Add(i3);
                    tris.Add(i1);
                    tris.Add(i3);
                    tris.Add(i4);
                    
                    tris.Add(i1);
                    tris.Add(i3);
                    tris.Add(i2);
                    tris.Add(i1);
                    tris.Add(i4);
                    tris.Add(i3);
                    
                    set.Add(edge);
                }
            }
            var geometry = new MeshGeometry3D
            {
                Positions = points,
                Normals = new Vector3DCollection(),
                TriangleIndices = tris
            };

            var geometryModel = new GeometryModel3D
            {
                Material = new MaterialGroup
                {
                    Children = new MaterialCollection
                    {
                        new DiffuseMaterial(Brushes.White),
                        // new SpecularMaterial(Brushes.Red, 1)
                    }
                },
                Geometry = geometry,
            };


            var group = new Model3DGroup();
            group.Children.Add(geometryModel);

            var modelVisual = new ModelVisual3D {Content = @group};
            return modelVisual;
        }

        private IEnumerable<(int, int)> FindEdgesFromFace(Face arg)
        {
            foreach (var valueTuple in arg.Indices.Pairs())
            {
                yield return valueTuple;
                yield return (valueTuple.Last, valueTuple.First);
            }

            yield return (arg.Indices.Last(), arg.Indices.First());
            yield return (arg.Indices.First(), arg.Indices.Last());
        }

        public void UnloadMesh(Mesh mesh)
        {
            if (_meshes.TryGetValue(mesh, out var modelVisual3D))
            {
                _viewPort.Children.Remove(modelVisual3D);
                _meshes.Remove(mesh);
            }
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
            else if (e.MiddleButton == MouseButtonState.Pressed && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                _dragHandler = new DollyHandler(this, e);
                Element.CaptureMouse();
            }
            else if (e.MiddleButton == MouseButtonState.Pressed)
            {
                _dragHandler = new OrbitHandler(this, e);
                Element.CaptureMouse();
            }

        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            _dragHandler?.MouseDrag(e);
        }

        private void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var delta = e.Delta * 0.25f;
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
                _pos = newPos;

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
                delta *= 50;
                Outer._camera.Pan((float) delta.X, (float) delta.Y);
            }
        }

        private class OrbitHandler : DragHandlerBase
        {
            public OrbitHandler(ModelPreview outer, MouseEventArgs args) : base(outer, args)
            {
            }

            protected override void DoMouseDrag(Vector delta)
            {
                Outer._camera.Orbit(delta);
            }
        }

        private class DollyHandler : DragHandlerBase
        {
            public DollyHandler(ModelPreview outer, MouseEventArgs args) : base(outer, args)
            {
            }

            protected override void DoMouseDrag(Vector delta)
            {
                Outer._camera.Dolly(delta.Y * -15);
            }
        }
    }
}