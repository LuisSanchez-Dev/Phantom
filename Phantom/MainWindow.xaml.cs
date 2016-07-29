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
using MarkdownSharp;
using mshtml;
using System.IO;
using Microsoft.Win32;
using System.ComponentModel;

namespace Phantom
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string FILTER = "Markdown Document (.md)|*.md|Web Document (.html)|*.html|Text File (.txt)|*.txt";

        const string BASE_TITLE = "Phantom";

        AboutWindow aboutWindow;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;

            fileListView.ItemsSource = FileManager.Files;
            fileListView.SelectionChanged += FileListView_SelectionChanged;
        }

        private void FileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FileManager.Files.Count == 0)
            {
                // no more files, disable the editor.
                rightHandGrid.Visibility = Visibility.Hidden;

            }else
            {
                rightHandGrid.Visibility = Visibility.Visible;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // open a new file
            var file = new EditorFile("New File");
            FileManager.Files.Add(file);
            FileManager.CurrentFile = file;
            FocusOpenedFile(file);
            
            // check for updates
            Updater.BeginUpdateTimer();
        }

        private void ChangeToCurrentFile()
        {
            editorFrame.Navigate(FileManager.CurrentFile.Page);
            currentFileName.DataContext = FileManager.CurrentFile;
            this.Title = $"{BASE_TITLE} - {FileManager.CurrentFile.Name}";
        }

        private void MarkFileAsUnsaved()
        {
            FileManager.CurrentFile.IsSaved = false;
        }

        private void MarkFileAsSaved()
        {
            FileManager.CurrentFile.IsSaved = true;
        }

        private void CloseCurrentFile()
        {
            if (!FileManager.CurrentFile.IsSaved) {
                var msg = MessageBox.Show("Would you like to save changes before closing this file?", null, MessageBoxButton.YesNoCancel);
                if (msg == MessageBoxResult.Cancel)
                    return;
                if (msg == MessageBoxResult.Yes)
                    if (!SaveFile())
                        return;
            }

            var index = FileManager.Files.IndexOf(FileManager.CurrentFile);
            FileManager.Files.Remove(FileManager.CurrentFile);
            currentFileName.DataContext = null;
            if (FileManager.Files.Count > 0)
            {
                FocusOpenedFile(FileManager.Files.LastOrDefault());
            }
        }


        private void saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void saveAsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile(true);
        }

        private void openFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void FocusOpenedFile(EditorFile file)
        {
            var desiredFile = FileManager.Files.Where(x => x == file);

            // if our file exists
            if (desiredFile.Count() > 0)
            {
                fileListView.SelectedItems.Clear();
                fileListView.SelectedItem = desiredFile.First();
            }
        }

        private void OpenFile()
        {
            // if we're not explicity opening a file from a path, show a dialog

            // open a dialog
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = FILTER;

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {

                // first, get the extension we opened the file as and cast it to the correct filetype
                var fileType = (FileType)dlg.FilterIndex;

                // get our file path
                var filePath = dlg.FileName;

                // create a new file in our List
                var file = new EditorFile(true, filePath, fileType);

                // make sure we don't already have this file of the same path open 
                if (FileManager.Files.Where(x => x.FilePath == file.FilePath).Count() > 0)
                {
                    // we already have this file open, let's just focus the file
                    FocusOpenedFile(file);
                }
                else
                {
                    // we don't have this file open yet, lets add it and do stuff
                    FileManager.Files.Add(file);

                    // open the file accordingly
                    string fileString = File.ReadAllText(file.FilePath);

                    // the final content we output to the editor
                    string content = "";


                    if (fileType == FileType.HTML)
                        content = TextHandler.Convert(fileString, fileType, FileType.Markdown);
                    else if (fileType == FileType.Markdown || fileType == FileType.Text)
                        content = fileString;

                    // set the content
                    file.Page.editorDocument.Blocks.Add(
                            new Paragraph(
                                new Run(
                                    content
                                    )));

                    FocusOpenedFile(file);
                }
            }
            


         }


        private bool SaveFile(bool forceDialog = false)
        {
            string filePath = "";
            FileType fileType = FileType.None;

            // check if our filepath is currently set (meaning we've saved before)
            if (string.IsNullOrEmpty(FileManager.CurrentFile.FilePath) || forceDialog)
            {
                // open the dialog to save
                Nullable<bool> result = false;

                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                //dlg.FileName = "Document"; // Default file name
                dlg.DefaultExt = ".md"; // Default file extension
                dlg.Filter = FILTER; // Filter files by extension

                // Show save file dialog box
                result = dlg.ShowDialog();

                if (!result.Value)
                {
                    // operation was cancelled
                    return false;
                }

                if ((FileType)dlg.FilterIndex != FileType.None)
                {
                    fileType = (FileType)dlg.FilterIndex;
                    filePath = dlg.FileName;

                    FileManager.CurrentFile.FilePath = dlg.FileName;
                    FileManager.CurrentFile.Name = System.IO.Path.GetFileName(dlg.FileName);
                }
            }
            // our file has been saved before, so we have a location for it.
            else
            {
                filePath = FileManager.CurrentFile.FilePath;
                fileType = FileManager.CurrentFile.FileType;
            }

            

            // Process save file dialog box results
            var document = new TextRange(FileManager.CurrentFile.Page.editorDocument.ContentStart, FileManager.CurrentFile.Page.editorDocument.ContentEnd).Text;

            // the string we'll eventually write
            string saveFile = "";
           
            // find what extension we saved as
            if (fileType == FileType.Markdown)
            {
                saveFile = document;
            }
            else if (fileType == FileType.HTML)
            {
                saveFile = TextHandler.Convert(document, FileType.HTML, FileType.Markdown);
            }
           
            // Write to the file
            File.WriteAllText(filePath, saveFile);

            // let our file know that we're all up to date
            FileManager.CurrentFile.IsSaved = true;
            MarkFileAsSaved();

            return true;
        }

        private void ShowPreview()
        {
            var page = FileManager.CurrentFile.Page;

           if (page.previewWindow.Visibility == Visibility.Hidden)
           {
               BackgroundWorker worker = new BackgroundWorker();
               string html = "";
               worker.DoWork += ((object sender, DoWorkEventArgs e) =>
               {
                   html = TextHandler.Convert(new TextRange(page.editorDocument.ContentStart, page.editorDocument.ContentEnd).Text, FileType.HTML, FileType.Markdown);
                   e.Result = html;
               });
           
               worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
               worker.RunWorkerAsync();
           }
           else
           {
               page.previewWindow.Visibility = Visibility.Hidden;
               page.editorTextBox.Visibility = Visibility.Visible;
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var page = FileManager.CurrentFile.Page;

            page.previewWindow.NavigateToString(e.Result.ToString());
            page.previewWindow.Visibility = Visibility.Visible;
            page.editorTextBox.Visibility = Visibility.Hidden;
        }

        private void previewButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPreview();
        }

        private void newFileButton_Click(object sender, RoutedEventArgs e)
        {
            var file = new EditorFile("New File");
            FileManager.Files.Add(file);
            FocusOpenedFile(file);
        }

        private void ResetDocument()
        {
            // reset stuff
            //editorDocument.Blocks.Clear();
            TextHandler.QuickSaveData = null;
        }

        private void infoButton_Click(object sender, RoutedEventArgs e)
        {
            // open up a window that shows us the about
            if (aboutWindow != null)
                aboutWindow.Close();

            aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }

        /// <summary>
        /// Change the currently opened file
        /// </summary>
        private void fileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // find the file in our FileManager
            if (e.AddedItems.Count > 0)
            {
                // set our current file
                FileManager.CurrentFile = FileManager.Files.Where(x => x == (EditorFile)e.AddedItems[0]).First();

                // open our file
                ChangeToCurrentFile();
            }
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            // put all of our hotkeys here

            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                // save hotkey
                SaveFile();
            }
            else if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
            {
                // show preview hotkey
                ShowPreview();
            }
            else if (Keyboard.Modifiers != ModifierKeys.Control && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                // it was just regular input.  we changed the document, so let's make sure we know we have unsaved changes
                MarkFileAsUnsaved();
            }
        }

        private void closeFileButton_Click(object sender, RoutedEventArgs e)
        {
            CloseCurrentFile();
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            OpenFile();
        }
    }
}
