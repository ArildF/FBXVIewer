using Assimp;

namespace FBXViewer
{
    public class MaterialProvider
    {
        private Scene? _scene;

        public void Load(Scene scene)
        {
            _scene = scene;
        }
        
        public Material? GetByIndex(int index)
        {
            return _scene?.Materials[index] ?? null;
        }
    }
}