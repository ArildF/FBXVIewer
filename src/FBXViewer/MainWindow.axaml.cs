using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Castle.Core;

namespace FBXViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class MainWindow : Window
    {
        private readonly MainWindowViewModel _vm;

        public MainWindow(MainWindowViewModel vm)
        {
            _vm = vm;
            DataContext = vm;
            InitializeComponent();
        }

        [DoNotSelect]
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void Open_OnClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            var files = await ofd.ShowAsync(this);
            var file = files.FirstOrDefault();
            if (file is {})
            {
                OpenFile(file);
            }
        }

        public void OpenFile(string fileName)
        {
            _vm.Load(fileName);
        }
    }
}