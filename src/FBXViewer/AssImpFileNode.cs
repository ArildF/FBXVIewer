using System;
using System.Collections.Generic;
using Assimp;
using Assimp.Configs;
using Assimp.Unmanaged;

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
        private readonly Func<Node, SceneNode> _sceneNodeFactory;
        private readonly Func<List<Animation>, AnimationsNode> _animationsNodeFactory;
        private readonly MaterialProvider _materialProvider;
        private readonly SceneContext _sceneContext;

        public AssImpFileNode(Func<List<Mesh>, MeshesNode> meshesFactory, 
            Func<List<Material>, MaterialsNode> materialsFactory, 
            Func<List<EmbeddedTexture>, TexturesNode> texturesFactory, 
            Func<Node, SceneNode> sceneNodeFactory,
            Func<List<Animation>, AnimationsNode> animationsNodeFactory,
            MaterialProvider materialProvider, 
            SceneContext sceneContext)
        {
            _meshesFactory = meshesFactory;
            _materialsFactory = materialsFactory;
            _texturesFactory = texturesFactory;
            _sceneNodeFactory = sceneNodeFactory;
            _animationsNodeFactory = animationsNodeFactory;
            _materialProvider = materialProvider;
            _sceneContext = sceneContext;
        }

        public void Load(string fileName)
        {
            _context = new AssimpContext();
            
            _context.SetConfig(new BooleanPropertyConfig(AiConfigs.AI_CONFIG_IMPORT_FBX_PRESERVE_PIVOTS, false));
            
            _fileName = fileName;
            _scene = _context.ImportFile(fileName, 
                PostProcessSteps.GenerateBoundingBoxes | 
                PostProcessSteps.CalculateTangentSpace | 
                PostProcessSteps.EmbedTextures);
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

            yield return _sceneNodeFactory(_scene.RootNode);
            yield return _meshesFactory(_scene.Meshes);
            yield return _texturesFactory(_scene.Textures);
            yield return _materialsFactory(_scene.Materials);
            yield return _animationsNodeFactory(_scene.Animations);
        }
    }
}