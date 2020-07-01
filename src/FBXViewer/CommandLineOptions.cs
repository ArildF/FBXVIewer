using CommandLine;

namespace FBXViewer
{
    public enum Renderer
    {
        OpenGL,
        WPF
    }
    public class CommandLineOptions
    {
        [Value(0, Required = false)]
        public string? FileName { get; set; }
        
        [Option(Required = false, Default = (Renderer)Renderer.OpenGL)]
        public Renderer Renderer { get; set; }
        
    }
}