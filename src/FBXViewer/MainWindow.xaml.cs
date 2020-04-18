using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace FBXViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Func<AssImpFile> _fileFactory;
        private AssImpFile _file;
        private readonly object _dummyTag = new object();

        public MainWindow(Func<AssImpFile> fileFactory)
        {
            _fileFactory = fileFactory;
            InitializeComponent();
        }

        private void Open_OnClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog(this) == true)
            {
                OpenFile(ofd.FileName);
            }
        }

        private void OpenFile(string fileName)
        {
            _file = _fileFactory();
            _file.Load(fileName);
            TreeView.Items.Add(NodeToTreeViewItem(_file));
        }

        private object NodeToTreeViewItem(INode node)
        {
            var tvi = new TreeViewItem
            {
                Tag = node
            };
            var preview = node.GetPreviewThumbnail();
            if (preview != null)
            {
                var content = new ContentControl();
                var dock = new DockPanel {MaxHeight = 16};
                
                dock.Children.Add(new TextBlock {Text = node.Text, Margin = new Thickness(0, 0, 4, 0)});
                dock.Children.Add((UIElement)preview);
                content.Content = dock;
                tvi.Header = content;
            }
            else
            {
                tvi.Header = node.Text;
            }
            if (node.HasChildren)
            {
                var dummy = new TreeViewItem();
                dummy.Tag = _dummyTag;
                tvi.Items.Add(dummy);
            }

            return tvi;
        }

        private void TreeView_OnExpanded(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem) e.Source;
            if (item.Items.Count > 0 && ((TreeViewItem) item.Items[0]).Tag == _dummyTag)
            {
                item.Items.Clear();
                var node = (INode) item.Tag;
                foreach (var child in node.GetChildren())
                {
                    item.Items.Add(NodeToTreeViewItem(child));
                }
            }
        }

        private void TreeView_OnSelected(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem) e.Source;
            var node = (INode) item.Tag;
            var preview = node.GetPreview();
            PreviewContent.Content = preview;

        }
    }
}