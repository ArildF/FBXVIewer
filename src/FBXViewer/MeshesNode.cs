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
        private readonly Func<List<Mesh>, MeshesNode> _meshesFactory;

        public MeshesNode(List<Mesh> sceneMeshes, Func<Mesh, MeshNode> meshNodeFactory, 
            Func<List<Mesh>, MeshesNode> meshesFactory)
        {
            _meshes = sceneMeshes;
            _meshNodeFactory = meshNodeFactory;
            _meshesFactory = meshesFactory;
        }

        public override bool SupportsMultiSelect => true;
        private bool _shouldShow;
        private string _text;

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

        public override string Text => IsSubmeshParent 
            ? _meshes.FirstOrDefault()?.Name ?? "?"
            : "Meshes";
        
        public override bool HasChildren => _meshes.Any();
        public bool IsSubmeshParent { get; set; }

        protected override IEnumerable<INode> CreateChildren()
        {
            if (IsSubmeshParent)
            {
                foreach (var mesh in _meshes)
                {
                    yield return _meshNodeFactory(mesh);
                }

                yield break;
            }
            
            var groupedByName = _meshes.GroupBy(m => m.Name)
                .OrderBy(g => g.Key);
            
            foreach (var grouping in groupedByName)
            {
                if (grouping.Count() > 1)
                {
                    var subMeshes = _meshesFactory(grouping.ToList());
                    subMeshes.IsSubmeshParent = true;
                    yield return subMeshes;
                }
                else
                {
                    yield return _meshNodeFactory(grouping.First());
                }
            }
        }
    }
}