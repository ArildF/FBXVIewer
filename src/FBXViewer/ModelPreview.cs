using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Assimp;
using MVector3D = System.Windows.Media.Media3D.Vector3D;
using Vector = System.Windows.Vector;

namespace FBXViewer
{
    public class ModelPreview
    {
        private readonly TextureProvider _textureProvider;
        private readonly Camera _camera;
        private IDragHandler? _dragHandler;
        private readonly Viewport3D _viewPort;

        public UIElement Element { get; }
        
        private readonly Dictionary<Mesh, MeshEntry> _meshes = new Dictionary<Mesh,MeshEntry>();
        private readonly Model3DGroup _meshModelGroup;
        private readonly Model3DGroup _wireFrameModelGroup;
        private readonly Model3DGroup _allModelGroup;

        private struct MeshEntry
        {
            public Model3DGroup ModelGroup;
            public Model3DGroup WireframeGroup;

            public MeshEntry(Model3DGroup modelGroup, Model3DGroup wireframeGroup)
            {
                ModelGroup = modelGroup;
                WireframeGroup = wireframeGroup;
            }
        }
        public ModelPreview(TextureProvider textureProvider, MainWindow mainWindow)
        {
            _textureProvider = textureProvider;
            _viewPort = new Viewport3D();

            var center = Vector3.Zero;
            

            var perspectiveCamera = new PerspectiveCamera(
                center.AsPoint3D(),
                new Vector3(0, 0, 1).AsMVector3D(), 
                new MVector3D(0, 1, 0), 45);
            
            var lightGroup = new Model3DGroup();
            var light = new PointLight(Colors.Cornsilk, perspectiveCamera.Position);
            lightGroup.Children.Add(light);
            _viewPort.Children.Add(new ModelVisual3D{Content = lightGroup});
            _camera = new Camera(perspectiveCamera, center, light);
            
            _viewPort.Camera = perspectiveCamera;
            
            _meshModelGroup = new Model3DGroup();
            _wireFrameModelGroup = new Model3DGroup();
            
            _allModelGroup = new Model3DGroup();
            _allModelGroup.Children.Add(_meshModelGroup);
            _allModelGroup.Children.Add(_wireFrameModelGroup);
            
            var rotation = new RotateTransform3D();
            var quaternionRotation = new QuaternionRotation3D();
            rotation.Rotation = quaternionRotation;
            _allModelGroup.Transform = rotation;
            
            var visual = new ModelVisual3D {Content = _allModelGroup};
            
            _viewPort.Children.Add(visual);

            var border = new Border {Background = Brushes.Black};
            border.Child = _viewPort;
            
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(4, GridUnitType.Star)});
            grid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(1, GridUnitType.Auto)});

            grid.Children.Add(border);
            border.SetValue(Grid.RowSpanProperty, 2);

            var settings = new MeshPreviewSettings(this);
            grid.Children.Add(settings);
            settings.SetValue(Grid.RowProperty, 1);
            
            var binding = new Binding("Rotation");
            binding.Source = settings.DataContext;
            BindingOperations.SetBinding(quaternionRotation, QuaternionRotation3D.QuaternionProperty,
                binding);
            

            grid.AddHandler(UIElement.PreviewMouseWheelEvent, new MouseWheelEventHandler(MouseWheel), true);
            grid.AddHandler(UIElement.PreviewMouseMoveEvent, new MouseEventHandler(MouseMove), true);
            grid.AddHandler(UIElement.PreviewMouseDownEvent, new MouseButtonEventHandler(MouseDown), true);
            grid.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(DoubleClick), true);
            grid.AddHandler(UIElement.PreviewMouseUpEvent, new MouseButtonEventHandler(MouseUp), true);
            mainWindow.AddHandler(UIElement.PreviewKeyDownEvent, new KeyEventHandler(KeyDown), true);
            Element = grid;
        }

        private void DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var mousePos = e.GetPosition(_viewPort);
                var hitParams = new PointHitTestParameters(mousePos);
                VisualTreeHelper.HitTest(_viewPort, null, result =>
                {
                    Debug.WriteLine($"Hit something! {result.VisualHit}");
                    
                    var rayMeshResult = 
                        result as RayMeshGeometry3DHitTestResult;
                    if (rayMeshResult != null)
                    {
                        _camera.MovePivotTo(rayMeshResult.PointHit);
                        Debug.WriteLine($"Impact at {rayMeshResult.MeshHit}. {rayMeshResult.PointHit}");
                    }
                    return HitTestResultBehavior.Stop;
                }, hitParams);
            }
        }

        public void LoadMesh(Mesh mesh)
        {
            UnloadMesh(mesh);

            var textureCoords = mesh.TextureCoordinateChannelCount > 0 
                ? mesh.TextureCoordinateChannels[0].Select(uv => uv.AsUvPoint()) : null;

            var triangleIndices = new List<int>(mesh.FaceCount * 4);
            foreach (var face in mesh.Faces)
            {
                triangleIndices.Add(face.Indices[0]);
                triangleIndices.Add(face.Indices[1]);
                triangleIndices.Add(face.Indices[2]);
                if (face.IndexCount == 4)
                {
                    triangleIndices.Add(face.Indices[0]);
                    triangleIndices.Add(face.Indices[2]);
                    triangleIndices.Add(face.Indices[3]);
                }
                if (face.IndexCount > 4)
                {
                    Debug.WriteLine($"Found {face.IndexCount}gon, only generating quad");
                }
            }

            var geometry = new MeshGeometry3D
            {
                Positions = new Point3DCollection(
                    mesh.Vertices.Select(v => new Point3D(v.X, v.Y, v.Z))),
                Normals = new Vector3DCollection(
                    mesh.Normals.Select(n => new MVector3D(n.X, n.Y, n.Z))),
                TriangleIndices = new Int32Collection(triangleIndices),
                TextureCoordinates = textureCoords != null ? new PointCollection(textureCoords) : null
            };
            var diffuse = _textureProvider.GetDiffuseTexture(mesh);
            
            // the ViewPortUnits is very important, or the brush will map MaxU x MaxV to 1 x 1
            // see https://books.google.no/books?id=ubgRAAAAQBAJ&pg=PA582&lpg=PA582
            // TileMode also seems necessary
            var brush = diffuse != null ? new ImageBrush(diffuse)
            {
                ViewportUnits = BrushMappingMode.Absolute,
                TileMode = TileMode.Tile
            } : (Brush)Brushes.Pink ; 

            var geometryModel = new GeometryModel3D
            {
                Material = new MaterialGroup
                {
                    Children = new MaterialCollection
                    {
                        new DiffuseMaterial(brush),
                        // new SpecularMaterial(Brushes.Red, 1)
                    }
                },
                Geometry = geometry,
            };


            var group = new Model3DGroup();
            group.Children.Add(geometryModel);
            _meshModelGroup.Children.Add(group);

            var wireFrame = CreateWireFrame(mesh);
            _wireFrameModelGroup.Children.Add(wireFrame);
            _meshes[mesh] = new MeshEntry(group, wireFrame);
            
            var center = geometry.Bounds.Location.AsVector3() + (geometry.Bounds.Size.AsVector3() / 2);
            var biggestExtent = new[] {geometry.Bounds.SizeX, geometry.Bounds.SizeY, geometry.Bounds.SizeZ}
                .OrderByDescending(s => s).First();
            var cameraOffset = biggestExtent * 2f;
            var cameraPosition = center + new Vector3(0, 0, (float)cameraOffset);

            _camera.ResetTo(cameraPosition, center);
        }

        private Model3DGroup CreateWireFrame(Mesh mesh)
        {
            var set = new HashSet<(int, int)>();
                
            var points = new List<Point3D>();
            var tris = new List<int>();
            foreach (var meshFace in mesh.Faces)
            {
                for(int i = 0; i < meshFace.Indices.Count; i++)
                {
                    var edge = (meshFace.Indices[i], meshFace.Indices[(i + 1) % meshFace.Indices.Count]);
                    if (set.Contains(edge))
                    {
                        continue;
                    }

                    var thirdVertex = meshFace.Indices[(i + 2) % meshFace.Indices.Count];

                    var v1 = mesh.Vertices[edge.Item1].AsVector3();
                    var v2 = mesh.Vertices[edge.Item2].AsVector3();
                    var v3 = mesh.Vertices[thirdVertex].AsVector3();
                    
                    var edgeVector = (v2 - v1);
                    var otherEdgeVector = v3 - v1;
                    var normalVector = Vector3.Cross(otherEdgeVector, edgeVector);
                    normalVector = Vector3.Normalize(normalVector);
                    var sideVector = Vector3.Normalize(Vector3.Cross(edgeVector, normalVector));

                    var width = 0.05f;
                    var p1 = v1 - sideVector * width;
                    var p2 = v2 - sideVector * width;
                    var p3 = v2 + sideVector * width;
                    var p4 = v1 + sideVector * width;

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
                    
                    set.Add(edge);
                }
            }
            var geometry = new MeshGeometry3D
            {
                Positions = new Point3DCollection(points),
                Normals = new Vector3DCollection(),
                TriangleIndices = new Int32Collection(tris)
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

            return group;

        }

        public void UnloadMesh(Mesh mesh)
        {
            if (_meshes.TryGetValue(mesh, out var entry))
            {
                _meshModelGroup.Children.Remove(entry.ModelGroup);
                _wireFrameModelGroup.Children.Remove(entry.WireframeGroup);
                _meshes.Remove(mesh);
            }
        }

        private void KeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine($"Key down {e.Key}");
            if (e.Key == Key.Decimal)
            {
                _camera.ResetToDefault();
            }

            View? view = (e.Key, Keyboard.Modifiers) switch
            {
                (Key.NumPad1, ModifierKeys.None) => View.Front,
                (Key.NumPad1, ModifierKeys.Control) => View.Back,
                (Key.NumPad3, ModifierKeys.None) => View.Right,
                (Key.NumPad3, ModifierKeys.Control) => View.Left,
                (Key.NumPad7, ModifierKeys.None) => View.Top,
                (Key.NumPad7, ModifierKeys.Control) => View.Bottom,
                _ => null,
            };
            if (view != null)
            {
                _camera.MoveToView(view.Value);
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
            var delta = e.Delta * 0.10f;
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
                var delta = (newPos - _pos) * 0.01;

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

        public void ToggleWireFrame(in bool wireFrameEnabled)
        {
           ToggleElement(wireFrameEnabled, _wireFrameModelGroup); 
        }

        public void ToggleMesh(in bool meshEnabled)
        {
            ToggleElement(meshEnabled, _meshModelGroup);
        }

        private void ToggleElement(bool enabled, Model3DGroup group)
        {
            if (enabled && !_allModelGroup.Children.Contains(group))
            {
                _allModelGroup.Children.Add(group);
            }

            if (!enabled)
            {
                _allModelGroup.Children.Remove(group);
            }
        }
    }
}