using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Microsoft.Win32;
using ReactiveUI;

namespace FBXViewer
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly Func<INode, TreeNodeViewModel> _nodeFactory;
        private readonly Func<AssImpFileNode> _fileFactory;

        public MainWindowViewModel(Func<INode, TreeNodeViewModel> nodeFactory,
            Func<AssImpFileNode> fileFactory)
        {
            _nodeFactory = nodeFactory;
            _fileFactory = fileFactory;

            _preview = this.WhenAnyValue(x => x.Selected)
                .Select(x => x?.Preview)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, vm => vm.Preview);
        }

        private readonly ObservableCollection<TreeNodeViewModel> _rootNodes = new ObservableCollection<TreeNodeViewModel>();
        public ObservableCollection<TreeNodeViewModel> RootNodes
        {
            get => _rootNodes;
        }

        private TreeNodeViewModel _selected;
        public TreeNodeViewModel Selected
        {
            get => _selected;
            set => this.RaiseAndSetIfChanged(ref _selected, value);
        }

        private ObservableAsPropertyHelper<object> _preview;

        public object Preview
        {
            get => _preview.Value;
        }
        
        public void Load(string fileName)
        {
            var fileNode = _fileFactory();
            fileNode.Load(fileName);
            RootNodes.Add(_nodeFactory(fileNode));
        }
    }
}