using System.Threading.Tasks;

namespace FBXViewer.OpenGL
{
    public class Texture
    {
        private readonly Task<uint?> _task;

        public uint Buffer => _task.IsCompleted ? _task.Result ?? 0 : 0;

        public Texture(Task<uint?> task)
        {
            _task = task;
        }
    }
}