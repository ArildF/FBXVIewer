using System;
using System.Collections.Generic;
using System.Linq;
using Assimp;
using ReactiveUI;

namespace FBXViewer
{
    public class MeshesNode : BaseNode
    {
        private readonly List<Mesh> _meshes;
        private readonly Func<Mesh, MeshNode> _meshNodeFactory;

        public MeshesNode(List<Mesh> sceneMeshes, Func<Mesh, MeshNode> meshNodeFactory)
        {
            _meshes = sceneMeshes;
            _meshNodeFactory = meshNodeFactory;
        }

        public override bool SupportsMultiSelect => true;
        private bool _shouldShow;

        public override bool IsChecked 
        {
            get => _shouldShow;
            set
            {
                this.RaiseAndSetIfChanged(ref _shouldShow, value);
                foreach (var child in GetChildren())
                {
                    child.IsChecked = value;
                }
            }
        }

    public override string Text => "Meshes";
        public override bool HasChildren => _meshes.Any();
        protected override IEnumerable<INode> CreateChildren()
        {
            foreach (var mesh in _meshes)
            {
                yield return _meshNodeFactory(mesh);
            }
        }
    }
}