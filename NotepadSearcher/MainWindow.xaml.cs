using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace NotepadSearcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string globalPath = "";
        private FilePathObj fpo = new FilePathObj();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchBar == null || string.IsNullOrWhiteSpace(globalPath))
            {
                listBox.Items.Clear();
                noteBox.Document.Blocks.Clear();
                return;
            }

            // If there's text in the search bar, perform the search
            if (!string.IsNullOrWhiteSpace(searchBar.Text))
            {
                List<string> searchResults = SearchObj.SearchTextResults(searchBar.Text, globalPath);
                listBox.Items.Clear();

                // Populate ListBox with file names and store full paths in Tag
                foreach (string resultFilePath in searchResults)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(resultFilePath);
                    ListBoxItem item = new ListBoxItem();
                    item.Content = fileName;
                    item.Tag = resultFilePath;
                    listBox.Items.Add(item);
                }
            }

            else
            {
                listBox.Items.Clear();

                if (!string.IsNullOrEmpty(globalPath) && Directory.Exists(globalPath))
                {
                    string[] filePathList = FillListBox.PathList(globalPath);
                    foreach (string fullFilePath in filePathList)
                    {
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(fullFilePath);
                        ListBoxItem item = new ListBoxItem();
                        item.Content = fileName;
                        item.Tag = fullFilePath;
                        listBox.Items.Add(item);
                    }
                }

                noteBox.Document.Blocks.Clear();
            }
        }

        private void OpenDir_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;

            dialog.Title = "Select Directory to Search";

            // Show the dialog and check the result
            CommonFileDialogResult result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                string path = dialog.FileName;

                if (!path.EndsWith("\\"))
                {
                    path += "\\";
                }

                globalPath = path;
                fpo.filePath = path;

                string[] filePathList = FillListBox.PathList(path);
                listBox.Items.Clear(); // Clear previous items

                // Populate ListBox with file names and store full paths in Tag
                foreach (string fullFilePath in filePathList)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(fullFilePath);
                    ListBoxItem item = new ListBoxItem();
                    item.Content = fileName;
                    item.Tag = fullFilePath;
                    listBox.Items.Add(item);
                }

                searchBar.Clear();
                noteBox.Document.Blocks.Clear();
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Correctly check if the selected item is a ListBoxItem
            if (listBox.SelectedItem is ListBoxItem selectedListBoxItem)
            {
                // Retrieve the full file path stored in the Tag property
                string filePath = selectedListBoxItem.Tag as string;

                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    try
                    {
                        string fileContent = File.ReadAllText(filePath);
                        string searchTerm = searchBar.Text;

                        // Load content and highlight only if there's a search term
                        LoadFileIntoRichTextBox(noteBox, fileContent, !string.IsNullOrWhiteSpace(searchTerm) ? searchTerm : null);
                    }

                    catch (Exception ex)
                    {
                        LoadFileIntoRichTextBox(noteBox, $"Error reading file: {ex.Message}", null);
                    }
                }

                else
                {
                    LoadFileIntoRichTextBox(noteBox, "Error: File not found or path invalid. Please check the file path.", null);
                }
            }

            else
            {
                noteBox.Document.Blocks.Clear();
            }
        }

        private void LoadFileIntoRichTextBox(System.Windows.Controls.RichTextBox rtb, string text, string highlightTerm)
        {
            rtb.Document.Blocks.Clear();

            // Add the text to a Paragraph
            Paragraph paragraph = new Paragraph(new Run(text));
            rtb.Document.Blocks.Add(paragraph);

            if (!string.IsNullOrEmpty(highlightTerm))
            {
                HighlightText(rtb, highlightTerm);
            }
        }

        private void HighlightText(System.Windows.Controls.RichTextBox rtb, string textToHighlight)
        {
            TextRange textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            string fullText = textRange.Text;

            StringComparison comparisonType = StringComparison.OrdinalIgnoreCase;

            int startIndex = 0;
            while (startIndex < fullText.Length)
            {
                int foundIndex = fullText.IndexOf(textToHighlight, startIndex, comparisonType);

                if (foundIndex == -1)
                {
                    break;
                }

                // Get TextPointers for the start and end of the found term
                TextPointer startPointer = GetTextPointerAtOffset(rtb.Document.ContentStart, foundIndex);
                TextPointer endPointer = GetTextPointerAtOffset(rtb.Document.ContentStart, foundIndex + textToHighlight.Length);

                // Ensure pointers are valid before creating TextRange
                if (startPointer != null && endPointer != null)
                {
                    TextRange highlightRange = new TextRange(startPointer, endPointer);

                    // Apply the highlight
                    highlightRange.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.LightBlue);
                    highlightRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black); // Ensure visibility
                }

                startIndex = foundIndex + textToHighlight.Length;
            }
        }

        // Helper method to get a TextPointer at a specific character offset
        private TextPointer GetTextPointerAtOffset(TextPointer start, int offset)
        {
            TextPointer pointer = start;
            int charsMoved = 0;

            while (charsMoved < offset && pointer != null)
            {
                TextPointer nextContextPosition = pointer.GetNextContextPosition(LogicalDirection.Forward);
                if (nextContextPosition == null)
                {
                    break;
                }

                switch (pointer.GetPointerContext(LogicalDirection.Forward))
                {
                    case TextPointerContext.Text:
                        int textRunLength = pointer.GetTextRunLength(LogicalDirection.Forward);
                        if (charsMoved + textRunLength > offset)
                        {
                            pointer = pointer.GetPositionAtOffset(offset - charsMoved);
                            charsMoved = offset;
                        }

                        else
                        {
                            pointer = pointer.GetPositionAtOffset(textRunLength);
                            charsMoved += textRunLength;
                        }

                        break;
                    case TextPointerContext.ElementStart:
                    case TextPointerContext.ElementEnd:
                    case TextPointerContext.EmbeddedElement:
                        pointer = nextContextPosition;
                        break;
                    
                    default:
                        pointer = nextContextPosition;
                        break;
                }
            }

            return pointer;
        }

        private void NoteBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}