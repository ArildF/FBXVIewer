using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Assimp;
using Quaternion = System.Windows.Media.Media3D.Quaternion;
using Vector3D = System.Windows.Media.Media3D.Vector3D;

namespace FBXViewer.Wpf
{
    public class WpfScene : IScene
    {
        private readonly TextureProvider<BitmapSource> _textureProvider;
        private readonly WpfRendererCamera _rendererCamera;
        private readonly Dictionary<Mesh, MeshEntry> _meshes = new Dictionary<Mesh, MeshEntry>();
        private readonly Model3DGroup _meshModelGroup;
        private readonly Model3DGroup _wireFrameModelGroup;
        private readonly Viewport3D _viewPort;
        private readonly Model3DGroup _allModelGroup;
        private readonly QuaternionRotation3D _quaternionRotation;
        public IRendererCamera RendererCamera => _rendererCamera;
        public UIElement Visual { get; }

        public ILight CameraLight { get; }

        public WpfScene(TextureProvider<BitmapSource> textureProvider)
        {
            _viewPort = new Viewport3D();
            
            _rendererCamera = new WpfRendererCamera(_viewPort, Vector3.Zero);

            _textureProvider = textureProvider;
            var lightGroup = new Model3DGroup();
            var light = new PointLight(Colors.Cornsilk, _rendererCamera.Position.AsPoint3D());
            CameraLight = new WpfLight(light);
            lightGroup.Children.Add(light);

            _viewPort.Children.Add(new ModelVisual3D {Content = lightGroup});

            _meshModelGroup = new Model3DGroup();
            _wireFrameModelGroup = new Model3DGroup();

            _allModelGroup = new Model3DGroup();
            _allModelGroup.Children.Add(_meshModelGroup);
            _allModelGroup.Children.Add(_wireFrameModelGroup);

            var rotation = new RotateTransform3D();
            _quaternionRotation = new QuaternionRotation3D();
            rotation.Rotation = _quaternionRotation;
            _allModelGroup.Transform = rotation;

            var visual = new ModelVisual3D {Content = _allModelGroup};

            _viewPort.Children.Add(visual);

            var border = new Border{Background = Brushes.Black};
            border.Child = _viewPort;
            Visual = border;
            MouseInput = new WpfMouseInput(border);
        }

        public void LoadMesh(Mesh mesh)
        {
            UnloadMesh(mesh);

            var textureCoords = mesh.TextureCoordinateChannelCount > 0
                ? mesh.TextureCoordinateChannels[0].Select(uv => uv.AsUvPoint())
                : null;

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
                    mesh.Normals.Select(n => new Vector3D(n.X, n.Y, n.Z))),
                TriangleIndices = new Int32Collection(triangleIndices),
                TextureCoordinates = textureCoords != null ? new PointCollection(textureCoords) : null
            };
            var diffuse = _textureProvider.GetDiffuseTexture(mesh);

            // the ViewPortUnits is very important, or the brush will map MaxU x MaxV to 1 x 1
            // see https://books.google.no/books?id=ubgRAAAAQBAJ&pg=PA582&lpg=PA582
            // TileMode also seems necessary
            var brush = diffuse != null
                ? new ImageBrush(diffuse)
                {
                    ViewportUnits = BrushMappingMode.Absolute,
                    TileMode = TileMode.Tile
                }
                : (Brush) Brushes.Pink;

            var geometryModel = new GeometryModel3D
            {
                Material = new MaterialGroup
                {
                    Children = new MaterialCollection
                    {
                        new DiffuseMaterial(brush),
                    }
                },
                Geometry = geometry,
            };


            var group = new Model3DGroup();
            group.Children.Add(geometryModel);
            _meshModelGroup.Children.Add(group);

            var (wireFrame, wireFrameGeometry) = CreateWireFrame(mesh);
            _wireFrameModelGroup.Children.Add(wireFrame);
            _meshes[mesh] = new MeshEntry(group, wireFrame, geometry, wireFrameGeometry);
            
        }

        public void SetShapeKeyWeight(Mesh mesh, float weight, MeshAnimationAttachment attachment)
        {
            if (!_meshes.TryGetValue(mesh, out var entry))
            {
                return;
            }

            var positions = entry.Geometry.Positions;
            var normals = entry.Geometry.Normals;
            for (int i = 0; i < attachment.VertexCount; i++)
            {
                var oldPos = mesh.Vertices[i].AsVector3();
                var keyPos = attachment.Vertices[i].AsVector3();
                if ((keyPos - oldPos).LengthSquared() > 0.001)
                {
                    var newPos = Vector3.Lerp(
                        oldPos,
                        keyPos,
                        weight);
                    positions[i] = newPos.AsPoint3D();
                }

                var oldNormal = mesh.Normals[i].AsVector3();
                var keyNormal = attachment.Normals[i].AsVector3();
                if ((keyNormal - oldNormal).LengthSquared() > 0.001)
                {
                    var newNormal = Vector3.Lerp(
                        oldNormal,
                        keyNormal,
                        weight);
                    normals[i] = newNormal.AsMVector3D();
                }
            }
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
        public void ToggleWireFrame(in bool wireFrameEnabled)
        {
           ToggleElement(wireFrameEnabled, _wireFrameModelGroup); 
        }
        
        public void ToggleMesh(in bool meshEnabled)
        {
            ToggleElement(meshEnabled, _meshModelGroup);
        }

        public void SetRootRotation(Quaternion quaternion)
        {
            _quaternionRotation.Quaternion = quaternion;
        }

        public IMouseInput MouseInput { get; }

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

        public bool RayCast(Vector2 mousePos, out RayCastResult rayCastResult)
        {
            var rcr = new RayCastResult();
            bool hit = false;
            var hitParams = new PointHitTestParameters(mousePos.AsPoint());
            VisualTreeHelper.HitTest(_viewPort, null, result =>
            {
                Debug.WriteLine($"Hit something! {result.VisualHit}");

                if (result is RayMeshGeometry3DHitTestResult rayMeshResult)
                {
                    rcr = new RayCastResult(rayMeshResult.PointHit.AsVector3());
                    hit = true;
                }

                return HitTestResultBehavior.Stop;
            }, hitParams);

            rayCastResult = rcr;

            return hit;
        }

        public Bounds GetBoundingBox(Mesh mesh)
        {
            if (!_meshes.TryGetValue(mesh, out var meshEntry))
            {
                throw new ArgumentException(nameof(mesh));
            }

            var bounds = meshEntry.Geometry.Bounds;
            return new Bounds((float) bounds.SizeX, (float) bounds.SizeY, (float) bounds.SizeZ, 
                bounds.Location.AsVector3() + (bounds.Size.AsVector3() / 2));
        }

        private (Model3DGroup, MeshGeometry3D) CreateWireFrame(Mesh mesh)
        {
            var set = new HashSet<(int, int)>();

            var points = new List<Point3D>();
            var tris = new List<int>();
            foreach (var meshFace in mesh.Faces)
            {
                for (int i = 0; i < meshFace.Indices.Count; i++)
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

            return (group, geometry);
        }


        private readonly struct MeshEntry
        {
            public readonly Model3DGroup ModelGroup;
            public readonly Model3DGroup WireframeGroup;
            public readonly MeshGeometry3D Geometry;
            public readonly MeshGeometry3D WireFrameGeometry;

            public MeshEntry(Model3DGroup modelGroup, Model3DGroup wireframeGroup, MeshGeometry3D geometry,
                MeshGeometry3D wireFrameGeometry)
            {
                ModelGroup = modelGroup;
                WireframeGroup = wireframeGroup;
                Geometry = geometry;
                WireFrameGeometry = wireFrameGeometry;
            }
        }
    }
}