using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Microsoft.Win32;

namespace FBXViewer
{
    public class TextureSearcher
    {
        private readonly MainWindow _mainWindow;

        public TextureSearcher(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public string Search(string textureFileName)
        {
            var ofd = new OpenFileDialog
            {
                Filters = new List<FileDialogFilter>
                {
                    new() {Name = textureFileName, Extensions = new List<string>{"*.*"}}
                },
            };
            var result = ofd.ShowAsync(_mainWindow).Result;
            return result switch
            {
                {Length: > 0} => result.First(),
                _ => textureFileName
            };
        }
    }
}