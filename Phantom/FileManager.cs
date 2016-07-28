using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phantom
{
    /// <summary>
    /// keeps track of all of our editor files
    /// </summary>
    public static class FileManager
    {
        public static ObservableCollection<EditorFile> Files = new ObservableCollection<EditorFile>();
        public static EditorFile CurrentFile;

        public static void AddAndFocus(string name)
        {
            var newFile = new EditorFile(name);
            Files.Add(newFile);
            CurrentFile = newFile;
        }
    }
}
