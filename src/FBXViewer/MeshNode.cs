using System.Collections.Generic;
using Assimp;
using ReactiveUI;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace FBXViewer
{
    public class MeshNode : BaseNode
    {
        private readonly Mesh _mesh;
        private readonly ModelView _modelView;
        private readonly MaterialProvider _materialProvider;

        public MeshNode(Mesh mesh, ModelView modelView, MaterialProvider materialProvider)
        {
            _mesh = mesh;
            _modelView = modelView;
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
                        _modelView.LoadMesh(_mesh, true, CalculateTransform());
                    }
                    else if (!IsChecked)
                    {
                       _modelView.UnloadMesh(_mesh); 
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
                        _modelView.LoadMesh(_mesh, false, CalculateTransform());
                    }
                    else
                    {
                        _modelView.UnloadMesh(_mesh);
                    }
                }
            }
        }

        private Matrix4x4 CalculateTransform()
        {
            return (SceneParent?.Transform ?? Assimp.Matrix4x4.Identity).ToNumMatrix4x4();
        }

        public override object GetPreview()
        {
            return _modelView.Element;
        }

        public override string Text => IsSubMesh 
            ? _materialProvider.GetByIndex(_mesh.MaterialIndex)?.Name ?? "?"
            : $"Mesh '{_mesh.Name}'";
        
        public override bool HasChildren => true;
        public SceneNode? SceneParent { get; set; }

        protected override IEnumerable<INode> CreateChildren()
        {
            foreach (var node in _mesh.PrimitiveProperties())
            {
                yield return node;
            }
        }
    }
}