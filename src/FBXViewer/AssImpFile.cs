using System;
using System.Collections.Generic;
using Assimp;

namespace FBXViewer
{
    public class AssImpFile : BaseNode
    {
        private AssimpContext _context;
        private Scene _scene;
        private string _fileName;

        private readonly Func<List<Mesh>, MeshesNode> _meshesFactory;
        private readonly Func<List<Material>, MaterialsNode> _materialsFactory;
        private readonly Func<List<EmbeddedTexture>, TexturesNode> _texturesFactory;

        public AssImpFile(Func<List<Mesh>, MeshesNode> meshesFactory, Func<List<Material>, MaterialsNode> materialsFactory, 
            Func<List<EmbeddedTexture>, TexturesNode> texturesFactory)
        {
            _meshesFactory = meshesFactory;
            _materialsFactory = materialsFactory;
            _texturesFactory = texturesFactory;
        }

        public void Load(string fileName)
        {
            _context = new AssimpContext();
            _fileName = fileName;
            _scene = _context.ImportFile(fileName);
        }

        public override string Text => _fileName;
        public override bool HasChildren => true;
        protected override IEnumerable<INode> CreateChildren()
        {
            yield return _meshesFactory(_scene.Meshes);
            yield return _texturesFactory(_scene.Textures);
            yield return _materialsFactory(_scene.Materials);
        }
    }
}