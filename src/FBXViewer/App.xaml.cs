using System.IO;
using System.Windows;
using CommandLine;

namespace FBXViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var helpWriter = new StringWriter();
            var parser = new Parser(with =>
            {
                with.HelpWriter = helpWriter;
                with.CaseInsensitiveEnumValues = true;
            });
            var options = parser.ParseArguments<CommandLineOptions>(e.Args);
            options.WithParsed(o =>
            {
                var container = WindsorBootstrapper.Bootstrap(o);
                var window = container.Resolve<MainWindow>();
                window.Show();

                if (!string.IsNullOrEmpty(o.FileName))
                {
                    window.OpenFile(o.FileName);
                }
            }).WithNotParsed(errors => 
                MessageBox.Show(helpWriter.ToString()));

        }
    }
}