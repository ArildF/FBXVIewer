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
        private readonly Func<IEnumerable<IGrouping<string, ShapeKey>>, ShapeKeysNode> _shapeKeysNodeFactory;

        public MeshesNode(List<Mesh> sceneMeshes, Func<Mesh, MeshNode> meshNodeFactory, 
            Func<List<Mesh>, MeshesNode> meshesFactory, 
            Func<IEnumerable<IGrouping<string, ShapeKey>>, ShapeKeysNode> shapeKeysNodeFactory)
        {
            _meshes = sceneMeshes;
            _meshNodeFactory = meshNodeFactory;
            _meshesFactory = meshesFactory;
            _shapeKeysNodeFactory = shapeKeysNodeFactory;
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

        public override string Text => IsSubMeshParent 
            ? _meshes.FirstOrDefault()?.Name ?? "?"
            : "Meshes";
        
        public override bool HasChildren => _meshes.Any();
        public bool IsSubMeshParent { get; set; }

        protected override IEnumerable<INode> CreateChildren()
        {
            if (IsSubMeshParent)
            {
                foreach (var mesh in _meshes)
                {
                    var meshNode = _meshNodeFactory(mesh);
                    meshNode.IsSubMesh = true;
                    yield return meshNode;
                }

                var keys = (from mesh in _meshes
                    from at in mesh.MeshAnimationAttachments
                    select new ShapeKey(mesh, at)).GroupBy(s => s.Attachment.Name);
                yield return _shapeKeysNodeFactory(keys);

                yield break;
            }
            
            var groupedByName = _meshes.GroupBy(m => m.Name)
                .OrderBy(g => g.Key);
            
            foreach (var grouping in groupedByName)
            {
                if (grouping.Count() > 1)
                {
                    var subMeshes = _meshesFactory(grouping.ToList());
                    subMeshes.IsSubMeshParent = true;
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