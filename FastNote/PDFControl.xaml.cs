using Syncfusion.Pdf.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace FastNote
{
    public sealed partial class PDFControl : UserControl
    {
        string val;
        ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

        public PDFControl(string value)
        {
            InitializeComponent();
            ExportPDFBtn.IsEnabled = false; PrintBtn.IsEnabled = false; ShareBtn.IsEnabled = false; OpenInBtn.IsEnabled = false;
            val = value;
            DisplayPDF();
        }

        public void RefreshVal(string value) { val = value; }

        private async void CreatePDFBtn_Click(object sender, RoutedEventArgs e)
        {
            string apiKey = Settings.Default.ApiKey; // Of course, my api key is not "xxxxxx"
            string value = val;
            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("apikey", apiKey), // we talked about this before
                    new KeyValuePair<string, string>("value", value),
                    new KeyValuePair<string, string>("MarginLeft", "20"),
                    new KeyValuePair<string, string>("MarginRight", "20"),
                    new KeyValuePair<string, string>("MarginTop", "20"),
                    new KeyValuePair<string, string>("MarginBottom", "20")
                });

                try
                {
                    var result = client.PostAsync("http://api.html2pdfrocket.com/pdf", content).Result;

                    if (result.IsSuccessStatusCode)
                    {
                        StorageFile pdfFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("CreatedPDF.pdf", CreationCollisionOption.ReplaceExisting); // Creates a PDF file in the app's applicationdata folder
                        byte[] bytes = result.Content.ReadAsByteArrayAsync().Result; // gets the bytes out of the result
                        File.WriteAllBytes(pdfFile.Path, bytes); // writes the bytes to the pdf
                        DisplayPDF();
                    }
                }
                catch (Exception)
                {
                    ErrorTB.Visibility = Visibility.Visible;
                }
            }
        }

        public async void DisplayPDF()
        {
            StorageFile pdf;
            try
            {
                pdf = await ApplicationData.Current.LocalFolder.GetFileAsync("CreatedPDF.pdf");
                Stream stream = await pdf.OpenStreamForReadAsync();
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                PdfLoadedDocument loadedDocument = new PdfLoadedDocument(buffer);
                Viewer.LoadDocument(loadedDocument);
                ErrorTB.Visibility = Visibility.Collapsed;
                ExportPDFBtn.IsEnabled = true;
                ShareBtn.IsEnabled = true;
                PrintBtn.IsEnabled = true;
                OpenInBtn.IsEnabled = true;
            }
            catch (Exception)
            {
                ExportPDFBtn.IsEnabled = false; PrintBtn.IsEnabled = false; ShareBtn.IsEnabled = false; OpenInBtn.IsEnabled = false;
                ErrorTB.Visibility = Visibility.Visible;
            }
        }

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            Viewer.Print();
        }

        private async void HelpBtn_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog cd = new ContentDialog();
            cd.Title = resourceLoader.GetString("HowPDFWorkTitle");
            string str = resourceLoader.GetString("HowPDFWorkContent");
            Run run0 = new Run(); run0.Text = str.Substring(0, str.IndexOf("{"));
            str = str.Substring(str.IndexOf("{") + 3);
            Run run1 = new Run(); run1.Text = "html2pdfrocket.com";
            Run run2 = new Run(); run2.Text = str.Substring(0, str.IndexOf("{"));
            str = str.Substring(str.IndexOf("{") + 3);
            Run run3 = new Run(); run3.Text = "Manage My Account";
            Run run4 = new Run(); run4.Text = str;
            Hyperlink hp1 = new Hyperlink() { NavigateUri = new Uri("https://www.html2pdfrocket.com/Account/Register")}; hp1.Inlines.Add(run1);
            Hyperlink hp2 = new Hyperlink() { NavigateUri = new Uri("https://www.html2pdfrocket.com/Account/Manage") }; hp2.Inlines.Add(run3);
            TextBlock tb = new TextBlock();
            tb.TextWrapping = TextWrapping.WrapWholeWords;
            tb.Inlines.Add(run0);
            tb.Inlines.Add(hp1);
            tb.Inlines.Add(run2);
            tb.Inlines.Add(hp2);
            tb.Inlines.Add(run4);
            cd.Content = tb;
            System.Diagnostics.Debug.WriteLine(cd.Content);
            cd.PrimaryButtonText = resourceLoader.GetString("DialogClose");
            await cd.ShowAsync();
        }

        private async void ExportPDFBtn_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.Pickers.FileSavePicker savepicker = new Windows.Storage.Pickers.FileSavePicker();
            savepicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            savepicker.SuggestedFileName = Settings.Default.DefaultExportName + ".pdf";
            savepicker.FileTypeChoices.Add(resourceLoader.GetString("FilePDF"), new List<string>() { ".pdf" });
            savepicker.CommitButtonText = resourceLoader.GetString("CommitExportText");
            StorageFile savefile = await savepicker.PickSaveFileAsync();
            if (savefile != null)
            {
                StorageFile pdf = await ApplicationData.Current.LocalFolder.GetFileAsync("CreatedPDF.pdf");
                await pdf.CopyAndReplaceAsync(savefile);
            }
            else
            {
                MessageDialog md = new MessageDialog(resourceLoader.GetString("Dialog_FileNotSaved"), resourceLoader.GetString("Dialog_OperationCancelled"));
                await md.ShowAsync();
            }
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFile pdf = await ApplicationData.Current.LocalFolder.GetFileAsync("CreatedPDF.pdf");
            await Launcher.LaunchFileAsync(pdf);
        }

        private void ApiKeyInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(ApiKeyInput.Text) || string.IsNullOrWhiteSpace(ApiKeyInput.Text)) CreatePDFBtn.IsEnabled = false;
            else CreatePDFBtn.IsEnabled = true;
        }

        private IReadOnlyList<StorageFile> storageItems;
        private async void ShareBtn_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("share", CreationCollisionOption.ReplaceExisting);
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("CreatedPDF.pdf");
            StorageFile sharefile = await file.CopyAsync(folder, Settings.Default.DefaultShareName + ".pdf", NameCollisionOption.ReplaceExisting);

            IReadOnlyList<StorageFile> pickedFiles = await folder.GetFilesAsync();

            if (pickedFiles.Count > 0)
            {
                storageItems = pickedFiles;

                // Display the file names in the UI.
                string selectedFiles = string.Empty;
                for (int index = 0; index < pickedFiles.Count; index++)
                {
                    selectedFiles += pickedFiles[index].Name;

                    if (index != (pickedFiles.Count - 1))
                    {
                        selectedFiles += ", ";
                    }
                }
            }
            DataTransferManager.GetForCurrentView().DataRequested += ShareFile_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private void ShareFile_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.SetStorageItems(storageItems);
            args.Request.Data.Properties.Title = "FastNote";
            args.Request.Data.Properties.Description = resourceLoader.GetString("ShareUI_FileDesc");
        }
    }
}
