using OpenGL;

namespace FBXViewer.OpenGL
{
    public struct Uniforms
    {
        public static Uniforms Get(uint program)
        {
            return new Uniforms
            {
                M = Gl.GetUniformLocation(program, "M"),
                V = Gl.GetUniformLocation(program, "V"),
                P = Gl.GetUniformLocation(program, "P"),
                LightPosition = Gl.GetUniformLocation(program, "LightPosition_worldSpace"),
                DiffuseSampler = Gl.GetUniformLocation(program, "diffuseTextureSampler"),
                NormalSampler = Gl.GetUniformLocation(program, "normalTextureSampler"),
            };
        }

        public int NormalSampler { get;private set; }

        public int DiffuseSampler { get;private set; }

        public int LightPosition { get;private set; }

        public int P { get;private set; }

        public int V { get;private set; }

        public int M { get;private set; }
    }
}