using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Assimp;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using KeyEventHandler = System.Windows.Input.KeyEventHandler;
using MVector3D = System.Windows.Media.Media3D.Vector3D;
using Vector = System.Windows.Vector;

namespace FBXViewer
{
    public class ModelPreview
    {
        private readonly Camera _camera;
        private IDragHandler? _dragHandler;

        public UIElement Element { get; }
        
        private readonly MeshPreviewSettingsViewModel _settingsViewModel;
        private readonly IScene _scene;

        public ModelPreview(MainWindow mainWindow, IScene scene, Coroutines coroutines)
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

            _settingsViewModel = new MeshPreviewSettingsViewModel(scene);
            var settings = new MeshPreviewSettings(_settingsViewModel);
            grid.Children.Add(settings);
            settings.SetValue(Grid.RowProperty, 1);

            var input = scene.MouseInput;

            input.MouseWheel += MouseWheel;
            input.MouseMove += MouseMove;
            input.MouseDown += MouseDown;
            input.MouseUp += MouseUp;

            input.MouseDown += DoubleClick;
              
            mainWindow.AddHandler(UIElement.PreviewKeyDownEvent, new KeyEventHandler(KeyDown), true);
            Element = grid;
        }

        private void DoubleClick(object sender, MouseButtonEventArgs e)
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

            if (e.Key == Key.NumPad5)
            {
                _camera.TogglePerspectiveOrthographic();
                _settingsViewModel.CameraType = _camera.IsOrthographic ? "Orthographic" : "Perspective";
            }
        }

        

        private void MouseUp(object? sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            _dragHandler = null;
            Element.ReleaseMouseCapture();
        }

        private void MouseDown(object? sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine($"Mouse down: {e.MouseButton} {e.Position}");
            bool isMiddle = e.MouseButton == MouseButton.Middle;
            if (isMiddle && Keyboard.IsKeyDown(Key.LeftShift))
            {
                _dragHandler = new PanDragHandler(this, e.Position);
                // Element.CaptureMouse();
            }
            else if (isMiddle && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                _dragHandler = new DollyHandler(this, e.Position);
                // Element.CaptureMouse();
            }
            else if (isMiddle)
            {
                _dragHandler = new OrbitHandler(this, e.Position);
                // Element.CaptureMouse();
            }

        }

        private void MouseMove(object sender, MouseMoveEventArgs e)
        {
            _dragHandler?.MouseDrag(e.Position);
        }

        private void MouseWheel(object sender, MouseWheelEventArgs e)
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
            protected readonly ModelPreview Outer;
            private Point _pos;

            protected DragHandlerBase(ModelPreview outer, Point point)
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

            protected abstract void DoMouseDrag(Vector delta);
        }

        private class PanDragHandler : DragHandlerBase
        {
            public PanDragHandler(ModelPreview outer, Point point) : base(outer, point)
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
            public OrbitHandler(ModelPreview outer, Point point) : base(outer, point)
            {
            }

            protected override void DoMouseDrag(Vector delta)
            {
                Outer._camera.Orbit(delta);
            }
        }

        private class DollyHandler : DragHandlerBase
        {
            public DollyHandler(ModelPreview outer, Point point) : base(outer, point)
            {
            }

            protected override void DoMouseDrag(Vector delta)
            {
                Outer._camera.Dolly(delta.Y * -15);
            }
        }

        public void LoadMesh(Mesh mesh)
        {
            _scene.LoadMesh(mesh);

            var bounds = _scene.GetBoundingBox(mesh);

            var center = bounds.Location;
            var biggestExtent = new[] {bounds.SizeX, bounds.SizeY, bounds.SizeZ}
                .OrderByDescending(s => s).First();
            var cameraOffset = biggestExtent * 2f;
            var cameraPosition = center + new Vector3(0, 0, (float)cameraOffset);

            _camera.ResetTo(cameraPosition, center);
        }

        public void UnloadMesh(Mesh mesh)
        {
            _scene.UnloadMesh(mesh);
        }
    }
}