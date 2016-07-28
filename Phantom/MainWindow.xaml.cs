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
        string currentlyOpenFileName = "New File";

        AboutWindow aboutWindow;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = $"{BASE_TITLE} - {currentlyOpenFileName}";

            // check for updates
            Updater.BeginUpdateTimer();
        }

        private void saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            QuickSave();
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
            if (!savedSinceLastChange)
            {
                MessageBoxResult msg = MessageBox.Show("You have unsaved changes.  Would you like to save the current file before opening a different one?", "", MessageBoxButton.YesNoCancel);

                if (msg == MessageBoxResult.Yes)
                {
                    var save = QuickSave();
                    if (!save)
                        return;
                }
                else if (msg == MessageBoxResult.Cancel)
                    return;
            }

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = FILTER;

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                // first, get the extension we opened the file as and cast it to the correct filetype
                var fileType = (FileType)dlg.FilterIndex;

                // next, open our file as a string
                var file = File.ReadAllText(dlg.FileName);

                // set the filename and type in our texthandler so we can save this easily in the future.
                TextHandler.QuickSaveData = new Tuple<string, FileType>(dlg.FileName, fileType);

                // set the filename in our window title
                currentlyOpenFileName = System.IO.Path.GetFileName(dlg.FileName);
                this.Title = $"{BASE_TITLE} - {currentlyOpenFileName}";

                // open the file accordingly
                if (fileType == FileType.HTML)
                {
                    editorDocument.Blocks.Clear();
                    editorDocument.Blocks.Add(
                        new Paragraph(
                            new Run(
                                TextHandler.Convert(file, FileType.Markdown, FileType.HTML)
                                )));
                }

                if (fileType == FileType.Markdown)
                {
                    editorDocument.Blocks.Clear();
                    editorDocument.Blocks.Add(
                        new Paragraph(
                            new Run(
                                file
                                )));
                }

                savedSinceLastChange = true;
            }
        }

        private bool QuickSave()
        {
            if (TextHandler.QuickSaveData != null)
            {
                return SaveFile(TextHandler.QuickSaveData.Item1, TextHandler.QuickSaveData.Item2);
            }
            else
            {
                return SaveFile();
            }
        }

        private bool SaveFile(string filePath = null, FileType fileType = FileType.None)
        {

            Nullable<bool> result = false;

            // present the save dialog if we don't have parameters already set
            if (filePath == null || fileType == FileType.None)
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                //dlg.FileName = "Document"; // Default file name
                dlg.DefaultExt = ".md"; // Default file extension
                dlg.Filter = FILTER; // Filter files by extension

                // Show save file dialog box
                result = dlg.ShowDialog();

                if ((FileType)dlg.FilterIndex != FileType.None && !string.IsNullOrEmpty(dlg.FileName))
                {
                    fileType = (FileType)dlg.FilterIndex;
                    filePath = dlg.FileName;
                }
                else
                {
                    // operation was cancelled
                    return false;
                }
            }
            else
            {
                result = true;
            }

            if (result == true)
            {
                // Process save file dialog box results
                var document = new TextRange(editorDocument.ContentStart, editorDocument.ContentEnd).Text;
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

                // Save document
                string filename = filePath;

                File.WriteAllText(filename, saveFile);

                // store our file in the TextHandler for quick saving
                TextHandler.QuickSaveData = new Tuple<string, FileType>(filename, fileType);

                // let us know that we saved the file
                savedSinceLastChange = true;
                currentlyOpenFileName = System.IO.Path.GetFileName(filename);

                this.Title = $"{BASE_TITLE} - {currentlyOpenFileName}";
            }

            return true;
        }

        private void editorTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                // save hotkey
                QuickSave();
            }
            else if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ShowPreview();
            }
            else if (Keyboard.Modifiers != ModifierKeys.Control && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                // it was just regular input.  we changed the document, so let's make sure we know we have unsaved changes
                savedSinceLastChange = false;
                this.Title = $"{BASE_TITLE} - {currentlyOpenFileName} *";
            }
        }

        private void ShowPreview()
        {
            if (previewWindow.Visibility == Visibility.Hidden)
            {
                BackgroundWorker worker = new BackgroundWorker();
                string html = "";
                worker.DoWork += ((object sender, DoWorkEventArgs e) =>
                {
                    html = TextHandler.Convert(new TextRange(editorDocument.ContentStart, editorDocument.ContentEnd).Text, FileType.HTML, FileType.Markdown);
                    e.Result = html;
                });

                worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                worker.RunWorkerAsync();
            }
            else
            {
                previewWindow.Visibility = Visibility.Hidden;
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            previewWindow.NavigateToString(e.Result.ToString());
            previewWindow.Visibility = Visibility.Visible;
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
                    var save = QuickSave();
                    if (!save)
                        return;
                }
                else if (msg == MessageBoxResult.Cancel)
                    return;

                ResetDocument();
            }
        }

        private void ResetDocument()
        {
            // reset stuff
            editorDocument.Blocks.Clear();
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
    }
}
