using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Assimp;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace FBXViewer
{
    public class ModelView
    {
        private readonly Camera _camera;
        private IDragHandler? _dragHandler;

        public Visual Element { get; }
        
        private readonly MeshViewSettingsViewModel _settingsViewModel;
        private readonly IScene _scene;
        private MeshViewSettings _settings;

        public ModelView(MainWindow mainWindow, IScene scene, Coroutines coroutines, 
            MeshViewSettingsViewModel settingsViewModel)
        {
            _scene = scene;
            _camera = new Camera(scene.RendererCamera, Vector3.Zero, coroutines, scene.CameraLight); 
            
            var border = new Border {Background = Brushes.Black};
            border.Child = scene.Visual;
            
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(4, GridUnitType.Star)});
            grid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(1, GridUnitType.Auto)});

            grid.Children.Add(border);
            border.SetValue(Grid.RowSpanProperty, 1);

            _settingsViewModel = settingsViewModel;
            _settings = new MeshViewSettings(_settingsViewModel);
            grid.Children.Add(_settings);
            _settings.SetValue(Grid.RowProperty, 1);

            var input = scene.MouseInput;

            input.MouseWheel += MouseWheel;
            input.MouseMove += MouseMove;
            input.MouseDown += MouseDown;
            input.MouseUp += MouseUp;

            input.MouseDown += DoubleClick;
              
            mainWindow.AddHandler(InputElement.KeyDownEvent, KeyDown,RoutingStrategies.Bubble, false);
            Element = grid;
        }

        private void DoubleClick(object? sender, MouseButtonEventArgs e)
        {
            if (e.Clicks == 2)
            {
                var mousePos = e.Position;
                if (_scene.RayCast(mousePos.AsVector2(), out RayCastResult result))
                {
                    _camera.MovePivotTo(result.PointHit);
                }
            }
        }
        
        private void KeyDown(object? sender, KeyEventArgs e)
        {
            if (_settings.IsKeyboardFocusWithin)
            {
                return;
            }
            Debug.WriteLine($"Key down {e.Key} {e.Handled} {e.Source}");
            if (e.Key == Key.Decimal)
            {
                _camera.ResetToDefault();
            }

            View? view = (e.Key, e.KeyModifiers) switch
            {
                (Key.NumPad1, KeyModifiers.None) => View.Front,
                (Key.NumPad1, KeyModifiers.Control) => View.Back,
                (Key.NumPad3, KeyModifiers.None) => View.Right,
                (Key.NumPad3, KeyModifiers.Control) => View.Left,
                (Key.NumPad7, KeyModifiers.None) => View.Top,
                (Key.NumPad7, KeyModifiers.Control) => View.Bottom,
                _ => null,
            };
            if (view != null)
            {
                _camera.MoveToView(view.Value);
            }

            if (e.Key == Key.NumPad5)
            {
                _camera.TogglePerspectiveOrthographic();
                _settingsViewModel.CameraType = _camera.IsOrthographic ? "Orthographic" : "Perspective";
            }
        }

        

        private void MouseUp(object? sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            _dragHandler = null;
        }

        private void MouseDown(object? sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine($"Mouse down: {e.MouseButton} {e.Position}");
            bool isMiddle = e.MouseButton == MouseButton.Middle;
            // if (isMiddle && Keyboard.IsKeyDown(Key.LeftShift))
            // {
            //     _dragHandler = new PanDragHandler(this, e.Position);
            // }
            // else if (isMiddle && Keyboard.IsKeyDown(Key.LeftCtrl))
            // {
            //     _dragHandler = new DollyHandler(this, e.Position);
            // }
            if (isMiddle)
            {
                _dragHandler = new OrbitHandler(this, e.Position);
            }
        }

        private void MouseMove(object? sender, MouseMoveEventArgs e)
        {
            _dragHandler?.MouseDrag(e.Position);
        }

        private void MouseWheel(object? sender, MouseWheelEventArgs e)
        {
            Debug.WriteLine("Mousewheel");
            var delta = e.Delta * 0.10f;
            _camera.Zoom(delta);
        }

        private interface IDragHandler
        {
            void MouseDrag(Point point);
        }

        private abstract class DragHandlerBase : IDragHandler
        {
            protected readonly ModelView Outer;
            private Point _pos;

            protected DragHandlerBase(ModelView outer, Point point)
            {
                _pos = point;
                Outer = outer;
            }

            public void MouseDrag(Point point)
            {
                var newPos = point;
                var delta = (newPos - _pos) * 0.01;

                _pos = newPos;

                DoMouseDrag(delta);

            }

            protected abstract void DoMouseDrag(Point delta);
        }

        private class PanDragHandler : DragHandlerBase
        {
            public PanDragHandler(ModelView outer, Point point) : base(outer, point)
            {
            }
            protected override void DoMouseDrag(Point delta)
            {
                delta *= 50;
                Outer._camera.Pan((float) delta.X, (float) delta.Y);
            }
        }

        private class OrbitHandler : DragHandlerBase
        {
            public OrbitHandler(ModelView outer, Point point) : base(outer, point)
            {
            }

            protected override void DoMouseDrag(Point delta)
            {
                Outer._camera.Orbit(delta.AsVector3());
            }
        }

        private class DollyHandler : DragHandlerBase
        {
            public DollyHandler(ModelView outer, Point point) : base(outer, point)
            {
            }

            protected override void DoMouseDrag(Point delta)
            {
                Outer._camera.Dolly(delta.Y * -15);
            }
        }

        public void LoadMesh(Mesh mesh, bool resetCamera, Matrix4x4 transform)
        {
            _scene.LoadMesh(mesh, transform);
            if (!resetCamera)
            {
                return;
            }

            var bounds = _scene.GetBoundingBox(mesh);

            var center = bounds.Location;
            var biggestExtent = new[] {bounds.SizeX, bounds.SizeY, bounds.SizeZ}
                .OrderByDescending(s => s).First();
            var cameraOffset = biggestExtent * 2f;
            var cameraPosition = center + new Vector3(0, 0, (float)cameraOffset);

            var orthoWidth = bounds.SizeX * 2.0f;

            _camera.ResetTo(cameraPosition, center, orthoWidth);
        }

        public void UnloadMesh(Mesh mesh)
        {
            _scene.UnloadMesh(mesh);
        }
    }
}