using System.Collections.Generic;
using System.Reflection;
using Assimp;

namespace FBXViewer
{
    public class MeshNode : BaseNode
    {
        private readonly Mesh _mesh;
        private readonly ModelPreview _modelPreview;

        public MeshNode(Mesh mesh, ModelPreview modelPreview)
        {
            _mesh = mesh;
            _modelPreview = modelPreview;
        }

        public override bool SupportsMultiSelect => true;

        public override object GetPreviewThumbnail()
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
                    else if (!ShouldShow)
                    {
                       _modelPreview.UnloadMesh(_mesh); 
                    }
                }
            }
        }

        private bool _shouldShow;

        public override bool ShouldShow
        {
            get => _shouldShow;
            set
            {
                if (value != _shouldShow)
                {
                    _shouldShow = value;
                    if (_shouldShow)
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

        public override string Text => $"Mesh '{_mesh.Name}'";
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