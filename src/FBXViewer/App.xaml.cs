using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FBXViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var container = WindsorBootstrapper.Bootstrap();
            var window = container.Resolve<MainWindow>();
            window.Show();
            if (e.Args.Length == 1)
            {
                window.OpenFile(e.Args[0]);
            }
        }
    }
}