using System;
using System.Windows;
using System.Windows.Input;

namespace FBXViewer.Wpf
{
    public class WpfMouseInput : IMouseInput
    {
        private UIElement _element;

        public WpfMouseInput(UIElement element)
        {
            element.AddHandler(UIElement.PreviewMouseDownEvent,
                new MouseButtonEventHandler((sender, args) => MouseDown?.Invoke(element,
                    new MouseButtonEventArgs(
                        args.ChangedButton,
                        args.GetPosition(element),
                        args.ClickCount))),
                    true);
            element.AddHandler(UIElement.PreviewMouseUpEvent,
                new MouseButtonEventHandler((sender, args) => MouseUp?.Invoke(element,
                    new MouseButtonEventArgs(
                        args.ChangedButton,
                        args.GetPosition(element),
                        args.ClickCount))),
                    true);
            element.AddHandler(UIElement.PreviewMouseWheelEvent,
                new MouseWheelEventHandler((sender, args) => MouseWheel?.Invoke(element,
                    new MouseWheelEventArgs(args.Delta))), true);
            element.AddHandler(UIElement.PreviewMouseMoveEvent,
                new MouseEventHandler((sender, args) => MouseMove?.Invoke(element,
                    new MouseMoveEventArgs(args.GetPosition(element)))), true);
            _element = element;
        }

        public event EventHandler<MouseWheelEventArgs>? MouseWheel;
        public event EventHandler<MouseMoveEventArgs>? MouseMove;
        public event EventHandler<MouseButtonEventArgs>? MouseDown;
        public event EventHandler<MouseButtonEventArgs>? MouseUp;
    }
}