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
                Filter = textureFileName + "|" + "*.*"
            };
            if (ofd.ShowDialog(_mainWindow) == true)
            {
                return ofd.FileName;
            }

            return textureFileName;
        }
    }
}