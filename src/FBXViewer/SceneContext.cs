using Assimp;

namespace FBXViewer
{
    public class SceneContext
    {
        public Scene? CurrentScene { get; set; }

        public Mesh? GetMeshByIndex(int index)
        {
            return index < CurrentScene?.MeshCount ? CurrentScene.Meshes[index] : null;
        }
    }
}