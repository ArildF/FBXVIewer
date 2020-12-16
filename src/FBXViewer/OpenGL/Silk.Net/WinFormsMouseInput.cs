using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace FBXViewer.OpenGL.Silk.Net
{
    public class WinFormsMouseInput : IMouseInput
    {
        public WinFormsMouseInput(Control control)
        {
            control.MouseWheel += (sender, args) => MouseWheel?.Invoke(control, new MouseWheelEventArgs(args.Delta));
            control.MouseDown += (sender, args) => MouseDown?.Invoke(
                control, new MouseButtonEventArgs(Convert(args.Button), new Point(args.X, args.Y), args.Clicks));
            control.MouseUp += (sender, args) => MouseUp?.Invoke(
                control, new MouseButtonEventArgs(Convert(args.Button), new Point(args.X, args.Y), args.Clicks));
            control.MouseMove += (sender, args) => MouseMove?.Invoke(
                control, new MouseMoveEventArgs(new Point(args.X, args.Y)));
        }
            
        public event EventHandler<MouseWheelEventArgs>? MouseWheel;
        public event EventHandler<MouseMoveEventArgs>? MouseMove;
        public event EventHandler<MouseButtonEventArgs>? MouseDown;
        public event EventHandler<MouseButtonEventArgs>? MouseUp;
        
        
        private MouseButton Convert(MouseButtons button)
        {
            return button switch
            {
                MouseButtons.Left => MouseButton.Left,
                MouseButtons.Middle => MouseButton.Middle,
                MouseButtons.Right => MouseButton.Right,
                MouseButtons.XButton1 => MouseButton.XButton1,
                MouseButtons.XButton2 => MouseButton.XButton2,
                MouseButtons.None => MouseButton.Left, //?,
                _ => throw new ArgumentException(nameof(button))
            };
        }
    }
}