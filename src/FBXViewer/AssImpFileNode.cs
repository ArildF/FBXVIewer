using System;
using System.Collections.Generic;
using Assimp;

namespace FBXViewer
{
    public class AssImpFileNode : BaseNode
    {
        private AssimpContext? _context;
        private Scene? _scene;
        private string? _fileName;

        private readonly Func<List<Mesh>, MeshesNode> _meshesFactory;
        private readonly Func<List<Material>, MaterialsNode> _materialsFactory;
        private readonly Func<List<EmbeddedTexture>, TexturesNode> _texturesFactory;
        private readonly MaterialProvider _materialProvider;
        private readonly SceneContext _sceneContext;

        public AssImpFileNode(Func<List<Mesh>, MeshesNode> meshesFactory, Func<List<Material>, MaterialsNode> materialsFactory, 
            Func<List<EmbeddedTexture>, TexturesNode> texturesFactory, MaterialProvider materialProvider, SceneContext sceneContext)
        {
            _meshesFactory = meshesFactory;
            _materialsFactory = materialsFactory;
            _texturesFactory = texturesFactory;
            _materialProvider = materialProvider;
            _sceneContext = sceneContext;
        }

        public void Load(string fileName)
        {
            _context = new AssimpContext();
            _fileName = fileName;
            _scene = _context.ImportFile(fileName);
            _sceneContext.CurrentScene = _scene;
            _materialProvider.Load(_scene);
        }

        public override string? Text => _fileName;
        public override bool HasChildren => true;
        protected override IEnumerable<INode> CreateChildren()
        {
            if (_scene == null)
            {
               yield break;
            }
            yield return _meshesFactory(_scene.Meshes);
            yield return _texturesFactory(_scene.Textures);
            yield return _materialsFactory(_scene.Materials);
        }
    }
}