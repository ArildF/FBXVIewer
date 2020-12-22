using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommandLine;

namespace FBXViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public class App : Application
    {
        public override void Initialize()
        {
            base.Initialize();
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            base.OnFrameworkInitializationCompleted();

            var helpWriter = new StringWriter();
            var parser = new Parser(with =>
            {
                with.HelpWriter = helpWriter;
                with.CaseInsensitiveEnumValues = true;
            });
            var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
            var options = parser.ParseArguments<CommandLineOptions>(args);
            options.WithParsed(o =>
            {
                var container = WindsorBootstrapper.Bootstrap(o);
                var window = container.Resolve<MainWindow>();
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    desktop.MainWindow = window;
                // window.Show();

                if (!string.IsNullOrEmpty(o.FileName))
                {
                    window.OpenFile(o.FileName);
                }
            }).WithNotParsed(async _ =>
            {
                Debug.WriteLine(helpWriter.ToString());
                // await MessageBoxSlim.Avalonia.BoxedMessage
                //     .Create(new MessageBoxParams
                //     {
                //         ContentMessage = helpWriter.ToString()
                //     }).ShowDialogAsync(null);
            });

        }
    }
}