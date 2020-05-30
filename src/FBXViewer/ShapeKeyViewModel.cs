using ReactiveUI;

namespace FBXViewer
{
    public class ShapeKeyViewModel : ReactiveObject
    {
        private readonly ShapeKeyNode _node;
        private double _value;

        public ShapeKeyViewModel(ShapeKeyNode node)
        {
            _node = node;
            _value = _node.Value;
        }
        
        public double Value
        {
            get => _node.Value;
            set
            {
                _node.Value = value;
                this.RaiseAndSetIfChanged(ref _value, value);
            }
        }
    }
}