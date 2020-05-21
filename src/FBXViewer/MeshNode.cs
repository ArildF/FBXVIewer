using System.Collections.Generic;
using System.Reflection;
using Assimp;
using ReactiveUI;

namespace FBXViewer
{
    public class MeshNode : BaseNode
    {
        private readonly Mesh _mesh;
        private readonly ModelPreview _modelPreview;
        private readonly MaterialProvider _materialProvider;

        public MeshNode(Mesh mesh, ModelPreview modelPreview, MaterialProvider materialProvider)
        {
            _mesh = mesh;
            _modelPreview = modelPreview;
            _materialProvider = materialProvider;
        }

        public override bool SupportsMultiSelect => true;

        public override object? GetPreviewThumbnail()
        {
            return null;
        }

        private bool _isSelected;

        public override bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    if (_isSelected)
                    {
                        _modelPreview.LoadMesh(_mesh);
                    }
                    else if (!IsChecked)
                    {
                       _modelPreview.UnloadMesh(_mesh); 
                    }
                }
            }
        }

        public bool IsSubMesh { get; set; }

        private bool _isChecked;

        public override bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (value != _isChecked)
                {
                    this.RaiseAndSetIfChanged(ref _isChecked, value);
                    if (_isChecked)
                    {
                        _modelPreview.LoadMesh(_mesh);
                    }
                    else
                    {
                        _modelPreview.UnloadMesh(_mesh);
                    }
                }
            }
        }

        public override object GetPreview()
        {
            return _modelPreview.Element;
        }

        public override string Text => IsSubMesh 
            ? _materialProvider.GetByIndex(_mesh.MaterialIndex)?.Name ?? "?"
            : $"Mesh '{_mesh.Name}'";
        
        public override bool HasChildren => true;
        protected override IEnumerable<INode> CreateChildren()
        {
            foreach (var node in _mesh.PrimitiveProperties())
            {
                yield return node;
            }
        }
    }
}