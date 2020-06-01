using System;
using System.Windows;
using System.Windows.Input;

namespace FBXViewer
{
    public interface IMouseInput
    {
        event EventHandler<MouseWheelEventArgs> MouseWheel;
        event EventHandler<MouseMoveEventArgs> MouseMove;
        event EventHandler<MouseButtonEventArgs> MouseDown;
        event EventHandler<MouseButtonEventArgs> MouseUp;
    }

    public class MouseButtonEventArgs
    {
        public MouseButtonEventArgs(MouseButton mouseButton, Point position, int clicks)
        {
            MouseButton = mouseButton;
            Position = position;
            Clicks = clicks;
        }

        public int Clicks { get; }
        public MouseButton MouseButton { get; }
        public Point Position { get; }

    }

    public class MouseMoveEventArgs
    {
        public MouseMoveEventArgs(Point position)
        {
            Position = position;
        }

        public Point Position { get; }
        
    }

    public class MouseWheelEventArgs
    {
        public MouseWheelEventArgs(float delta)
        {
            Delta = delta;
        }

        public float Delta { get; private set; }
    }
}