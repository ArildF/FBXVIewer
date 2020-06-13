using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
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
        private readonly TextureProvider<BitmapSource> _textureProvider;
        private readonly MaterialProvider _materialProvider;

        public AssImpFileNode(Func<List<Mesh>, MeshesNode> meshesFactory, Func<List<Material>, MaterialsNode> materialsFactory, 
            Func<List<EmbeddedTexture>, TexturesNode> texturesFactory, TextureProvider<BitmapSource> textureProvider, MaterialProvider materialProvider)
        {
            _meshesFactory = meshesFactory;
            _materialsFactory = materialsFactory;
            _texturesFactory = texturesFactory;
            _textureProvider = textureProvider;
            _materialProvider = materialProvider;
        }

        public void Load(string fileName)
        {
            _context = new AssimpContext();
            _fileName = fileName;
            _scene = _context.ImportFile(fileName);
            _textureProvider.LoadScene(_fileName, _scene);
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