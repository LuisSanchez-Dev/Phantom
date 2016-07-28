using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phantom
{
    /// <summary>
    /// Keeps track of filename/path, last saves, etc.  Should be instantiated with each open file.  A list of these exists on the page that uses them.
    /// </summary>
    public class EditorFile : INotifyPropertyChanged
    {
        
        public EditorPage Page { get; set; } = new EditorPage();

        private bool isSaved = true;
        public bool IsSaved
        {
            get
            {
                return isSaved;
            }
            set
            {
                if (isSaved == value)
                    return;
                isSaved = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsSaved"));
            }
        }

        public string FilePath { get; set; }
        public FileType FileType { get; set; }

        public string name { get; set; }
        public string Name
        {
            get { return name; }
            set
            {
                if (name == value)
                    return;
                name = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Name"));
            }
        }




        public EditorFile(string name, FileType fileType = FileType.Markdown)
        {
            Name = name;
            FileType = fileType;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
}
