using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;

namespace FBXViewer
{
    public class TreeNodeViewModel : ReactiveObject
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly INode _node;
        private readonly Func<INode, TreeNodeViewModel> _nodeFactory;
        private readonly List<object> _children = new List<object>();

        private static readonly object Dummy = new object();
        
        public TreeNodeViewModel(MainWindowViewModel mainWindowViewModel, INode node, Func<INode, TreeNodeViewModel> nodeFactory)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _node = node;
            _nodeFactory = nodeFactory;
            
            if (node.HasChildren)
            {
                _children.Add(Dummy);
            }

            _isChecked = _node.WhenAnyValue(n => n.IsChecked)
                .ToProperty(this, vm => vm.IsChecked);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                _node.IsSelected = value;
                if (value)
                {
                    _mainWindowViewModel.Selected = this;
                }
            }
        }
        public object? Preview => _node.GetPreview();
        public string Text => _node.Text ?? "";
        public object? PreviewThumbnail => _node.GetPreviewThumbnail();

        public bool IsMultiSelect => _node.SupportsMultiSelect;

        public List<object> Children
        {
            get
            {
                if (_children.Any() && _children[0] == Dummy)
                {
                    _children.Clear();
                    _children.AddRange(_node.GetChildren().Select(_nodeFactory));
                }
                return _children;
            }
        }

        private readonly ObservableAsPropertyHelper<bool> _isChecked;
        public bool IsChecked
        {
            get => _isChecked.Value;
            set => _node.IsChecked = value;
        }
    }
}