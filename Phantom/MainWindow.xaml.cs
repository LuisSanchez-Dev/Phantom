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
        const string FILTER = "Markdown Document (.md)|*.md|Web Document (.html)|*.html";
        bool savedSinceLastChange = true;

        const string BASE_TITLE = "Phantom";

        AboutWindow aboutWindow;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;

            fileListView.ItemsSource = FileManager.Files;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // open a new file
            FileManager.AddAndFocus("New File");
            ChangeToCurrentFile();
            
            FileManager.Files.Add(new EditorFile("New File 2"));

            // check for updates
            Updater.BeginUpdateTimer();
        }

        private void ChangeToCurrentFile()
        {
            editorFrame.Navigate(FileManager.CurrentFile.Page);
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


        private void saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void saveAsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void openFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }


        private void OpenFile()
        {

            // first, check if we have unsaved changes
        //    if (!savedSinceLastChange)
        //    {
        //        MessageBoxResult msg = MessageBox.Show("You have unsaved changes.  Would you like to save the current file before opening a different one?", "", MessageBoxButton.YesNoCancel);
        //
        //        if (msg == MessageBoxResult.Yes)
        //        {
        //            var save = QuickSave();
        //            if (!save)
        //                return;
        //        }
        //        else if (msg == MessageBoxResult.Cancel)
        //            return;
        //    }
        //
        //    OpenFileDialog dlg = new OpenFileDialog();
        //    dlg.Filter = FILTER;
        //
        //    Nullable<bool> result = dlg.ShowDialog();
        //    if (result == true)
        //    {
        //        // first, get the extension we opened the file as and cast it to the correct filetype
        //        var fileType = (FileType)dlg.FilterIndex;
        //
        //        // next, open our file as a string
        //        var file = File.ReadAllText(dlg.FileName);
        //
        //        // set the filename and type in our texthandler so we can save this easily in the future.
        //        TextHandler.QuickSaveData = new Tuple<string, FileType>(dlg.FileName, fileType);
        //
        //        // set the filename in our window title
        //        currentlyOpenFileName = System.IO.Path.GetFileName(dlg.FileName);
        //        this.Title = $"{BASE_TITLE} - {currentlyOpenFileName}";
        //
        //        // open the file accordingly
        //        if (fileType == FileType.HTML)
        //        {
        //            editorDocument.Blocks.Clear();
        //            editorDocument.Blocks.Add(
        //                new Paragraph(
        //                    new Run(
        //                        TextHandler.Convert(file, FileType.Markdown, FileType.HTML)
        //                        )));
        //        }
        //
        //        if (fileType == FileType.Markdown)
        //        {
        //            editorDocument.Blocks.Clear();
        //            editorDocument.Blocks.Add(
        //                new Paragraph(
        //                    new Run(
        //                        file
        //                        )));
        //        }
        //
        //        savedSinceLastChange = true;
        //    }
        }


        private void SaveFile()
        {
            string filePath = "";
            FileType fileType = FileType.None;

            // check if our filepath is currently set (meaning we've saved before)
            if (string.IsNullOrEmpty(FileManager.CurrentFile.FilePath))
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
                    return;
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
        }

            

        private void editorTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void ShowPreview()
        {
           // if (previewWindow.Visibility == Visibility.Hidden)
           // {
           //     BackgroundWorker worker = new BackgroundWorker();
           //     string html = "";
           //     worker.DoWork += ((object sender, DoWorkEventArgs e) =>
           //     {
           //         html = TextHandler.Convert(new TextRange(editorDocument.ContentStart, editorDocument.ContentEnd).Text, FileType.HTML, FileType.Markdown);
           //         e.Result = html;
           //     });
           //
           //     worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
           //     worker.RunWorkerAsync();
           // }
           // else
           // {
           //     previewWindow.Visibility = Visibility.Hidden;
           // }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
           // previewWindow.NavigateToString(e.Result.ToString());
           // previewWindow.Visibility = Visibility.Visible;
        }

        private void previewButton_Click(object sender, RoutedEventArgs e)
        {
            ShowPreview();
        }

        private void newFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (savedSinceLastChange)
            {
                ResetDocument();
            }
            else
            {
                MessageBoxResult msg = MessageBox.Show("You have unsaved changes.  Would you like to save the current file before creating a new one?", "", MessageBoxButton.YesNoCancel);

                if (msg == MessageBoxResult.Yes)
                {
                    SaveFile();
                }
                else if (msg == MessageBoxResult.Cancel)
                    return;

                ResetDocument();
            }
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
    }
}
