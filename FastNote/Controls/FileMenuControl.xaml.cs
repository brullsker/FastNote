extern alias syncfusion;
extern alias syncfusionport;

using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using syncfusion.Syncfusion.DocIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace FastNote.Controls
{
    public sealed partial class FileMenuControl : UserControl
    {
        MainPage mp;
        RichEditBox me;
        private IReadOnlyList<StorageFile> storageItems;

        public int MainEditSelectionLength
        {
            get { return _mainEditSelectionLength; }
            set
            {
                _mainEditSelectionLength = value;
                if (_mainEditSelectionLength == 0) { ShareSelectedTextContent.Visibility = Visibility.Collapsed; ShareWholeTextContent.Visibility = Visibility.Visible; }
                else { ShareWholeTextContent.Visibility = Visibility.Collapsed; ShareSelectedTextContent.Visibility = Visibility.Visible; }
            }
        }
        private int _mainEditSelectionLength;

        public FileMenuControl(MainPage m, RichEditBox r)
        {
            this.InitializeComponent();
            Debug.WriteLine("FileMenu InitializeComponent called");
            mp = m;
            me = r;
        }

        private async void MoreOptionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MoreOptionsList.SelectedIndex == 0)
            {
                mp.ChangeLoading(true);
                Windows.Storage.Pickers.FileSavePicker picker = new Windows.Storage.Pickers.FileSavePicker();
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                picker.FileTypeChoices.Add(mp.resourceLoader.GetString("FileRTF"), new List<string>() { ".rtf" });
                picker.FileTypeChoices.Add(mp.resourceLoader.GetString("FilePDF"), new List<string>() { ".pdf" });
                picker.FileTypeChoices.Add(mp.resourceLoader.GetString("FileHTML"), new List<string>() { ".html" });
                picker.FileTypeChoices.Add(mp.resourceLoader.GetString("FileDocx"), new List<string>() { ".docx" });
                picker.FileTypeChoices.Add(mp.resourceLoader.GetString("FileDoc"), new List<string>() { ".doc" });
                picker.FileTypeChoices.Add(mp.resourceLoader.GetString("FileEpub"), new List<string>() { ".epub" });
                picker.FileTypeChoices.Add(mp.resourceLoader.GetString("FileTXT"), new List<string>() { ".txt" });
                picker.FileTypeChoices.Add(mp.resourceLoader.GetString("FileJPG"), new List<string>() { ".jpg" });
                picker.FileTypeChoices.Add(mp.resourceLoader.GetString("FilePNG"), new List<string>() { ".png" });
                picker.FileTypeChoices.Add(mp.resourceLoader.GetString("FileBMP"), new List<string>() { ".bmp" });
                picker.FileTypeChoices.Add(mp.resourceLoader.GetString("FileGIF"), new List<string>() { ".gif" });
                picker.FileTypeChoices.Add(mp.resourceLoader.GetString("FileTIFF"), new List<string>() { ".tiff" });
                picker.CommitButtonText = mp.resourceLoader.GetString("CommitExportText");
                picker.SuggestedFileName = Settings.Default.DefaultExportName;
                StorageFile saveFile = await picker.PickSaveFileAsync();
                if (saveFile != null)
                {
                    if (saveFile.FileType == ".rtf")
                    {
                        me.RequestedTheme = ElementTheme.Light;
                        await mp.file.CopyAndReplaceAsync(saveFile);
                        if (Settings.Default.ThemeDefault == true) me.RequestedTheme = ElementTheme.Default;
                        if (Settings.Default.ThemeDark == true) me.RequestedTheme = ElementTheme.Dark;
                        if (Settings.Default.ThemeLight == true) me.RequestedTheme = ElementTheme.Light;
                    }
                    if (saveFile.FileType == ".html") await FileIO.WriteTextAsync(saveFile, ConvertToHtml(me, mp.EncodingList));
                    if (saveFile.FileType == ".docx") mp.SfSave(saveFile, FormatType.Docx);
                    if (saveFile.FileType == ".doc") mp.SfSave(saveFile, FormatType.Doc);
                    if (saveFile.FileType == ".epub") mp.SfSave(saveFile, FormatType.EPub);
                    if (saveFile.FileType == ".txt")
                    {
                        me.Document.GetText(TextGetOptions.None, out string txtstring);
                        await FileIO.WriteTextAsync(saveFile, txtstring);
                    }
                    if (saveFile.FileType == ".jpg")
                    {
                        me.RequestedTheme = ElementTheme.Light;
                        me.Focus(FocusState.Programmatic);
                        Debug.WriteLine("Focus set");
                        await mp.renderTargetBitmap.RenderAsync(me);
                        Debug.WriteLine("Rendered");
                        using (var stream = await saveFile.OpenStreamForWriteAsync())
                        {
                            var logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
                            var pixelBuffer = await mp.renderTargetBitmap.GetPixelsAsync();
                            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream.AsRandomAccessStream());
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)mp.renderTargetBitmap.PixelWidth, (uint)mp.renderTargetBitmap.PixelHeight, logicalDpi, logicalDpi, pixelBuffer.ToArray());
                            await encoder.FlushAsync();
                        }
                        if (Settings.Default.ThemeDefault == true) me.RequestedTheme = ElementTheme.Default;
                        if (Settings.Default.ThemeDark == true) me.RequestedTheme = ElementTheme.Dark;
                        if (Settings.Default.ThemeLight == true) me.RequestedTheme = ElementTheme.Light;
                    }
                    if (saveFile.FileType == ".png")
                    {
                        me.RequestedTheme = ElementTheme.Light;
                        me.Focus(FocusState.Programmatic);
                        Debug.WriteLine("Focus set");
                        await mp.renderTargetBitmap.RenderAsync(me);
                        Debug.WriteLine("Rendered");
                        using (var stream = await saveFile.OpenStreamForWriteAsync())
                        {
                            var logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
                            var pixelBuffer = await mp.renderTargetBitmap.GetPixelsAsync();
                            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream.AsRandomAccessStream());
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)mp.renderTargetBitmap.PixelWidth, (uint)mp.renderTargetBitmap.PixelHeight, logicalDpi, logicalDpi, pixelBuffer.ToArray());
                            await encoder.FlushAsync();
                        }
                        if (Settings.Default.ThemeDefault == true) me.RequestedTheme = ElementTheme.Default;
                        if (Settings.Default.ThemeDark == true) me.RequestedTheme = ElementTheme.Dark;
                        if (Settings.Default.ThemeLight == true) me.RequestedTheme = ElementTheme.Light;
                    }
                    if (saveFile.FileType == ".bmp")
                    {
                        me.RequestedTheme = ElementTheme.Light;
                        me.Focus(FocusState.Programmatic);
                        Debug.WriteLine("Focus set");
                        await mp.renderTargetBitmap.RenderAsync(me);
                        Debug.WriteLine("Rendered");
                        using (var stream = await saveFile.OpenStreamForWriteAsync())
                        {
                            var logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
                            var pixelBuffer = await mp.renderTargetBitmap.GetPixelsAsync();
                            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream.AsRandomAccessStream());
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)mp.renderTargetBitmap.PixelWidth, (uint)mp.renderTargetBitmap.PixelHeight, logicalDpi, logicalDpi, pixelBuffer.ToArray());
                            await encoder.FlushAsync();
                        }
                        if (Settings.Default.ThemeDefault == true) me.RequestedTheme = ElementTheme.Default;
                        if (Settings.Default.ThemeDark == true) me.RequestedTheme = ElementTheme.Dark;
                        if (Settings.Default.ThemeLight == true) me.RequestedTheme = ElementTheme.Light;
                    }
                    if (saveFile.FileType == ".gif")
                    {
                        me.RequestedTheme = ElementTheme.Light;
                        me.Focus(FocusState.Programmatic);
                        Debug.WriteLine("Focus set");
                        await mp.renderTargetBitmap.RenderAsync(me);
                        Debug.WriteLine("Rendered");
                        using (var stream = await saveFile.OpenStreamForWriteAsync())
                        {
                            var logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
                            var pixelBuffer = await mp.renderTargetBitmap.GetPixelsAsync();
                            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.GifEncoderId, stream.AsRandomAccessStream());
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)mp.renderTargetBitmap.PixelWidth, (uint)mp.renderTargetBitmap.PixelHeight, logicalDpi, logicalDpi, pixelBuffer.ToArray());
                            await encoder.FlushAsync();
                        }
                        if (Settings.Default.ThemeDefault == true) me.RequestedTheme = ElementTheme.Default;
                        if (Settings.Default.ThemeDark == true) me.RequestedTheme = ElementTheme.Dark;
                        if (Settings.Default.ThemeLight == true) me.RequestedTheme = ElementTheme.Light;
                    }
                    if (saveFile.FileType == ".tiff")
                    {
                        me.RequestedTheme = ElementTheme.Light;
                        me.Focus(FocusState.Programmatic);
                        Debug.WriteLine("Focus set");
                        await mp.renderTargetBitmap.RenderAsync(me);
                        Debug.WriteLine("Rendered");
                        using (var stream = await saveFile.OpenStreamForWriteAsync())
                        {
                            var logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
                            var pixelBuffer = await mp.renderTargetBitmap.GetPixelsAsync();
                            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.TiffEncoderId, stream.AsRandomAccessStream());
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)mp.renderTargetBitmap.PixelWidth, (uint)mp.renderTargetBitmap.PixelHeight, logicalDpi, logicalDpi, pixelBuffer.ToArray());
                            await encoder.FlushAsync();
                        }
                        if (Settings.Default.ThemeDefault == true) me.RequestedTheme = ElementTheme.Default;
                        if (Settings.Default.ThemeDark == true) me.RequestedTheme = ElementTheme.Dark;
                        if (Settings.Default.ThemeLight == true) me.RequestedTheme = ElementTheme.Light;
                    }
                    if (saveFile.FileType == ".pdf") await mp.SfPdfSave(saveFile);
                }
                else
                {
                    MessageDialog md = new MessageDialog(mp.resourceLoader.GetString("Dialog_FileNotSaved"), mp.resourceLoader.GetString("Dialog_OperationCancelled"));
                    await md.ShowAsync();
                }
                mp.ChangeLoading(false);
            }
            if (MoreOptionsList.SelectedIndex == 1)
            {
                me.Document.GetText(TextGetOptions.None, out string value);
                if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value)) mp.Import(2);
                else mp.Import(Settings.Default.ImportOption);
            }

            if (MoreOptionsList.SelectedIndex == 2)
            {
                mp.ChangeLoading(true);

                StorageFolder folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("share", CreationCollisionOption.ReplaceExisting);

                if (Settings.Default.ShareFileType == 0)
                {
                    StorageFile sharefile = await mp.file.CopyAsync(folder, Settings.Default.DefaultShareName + ".rtf", NameCollisionOption.ReplaceExisting);
                }
                if (Settings.Default.ShareFileType == 1)
                {
                    StorageFile sharefile = await folder.CreateFileAsync(Settings.Default.DefaultShareName + ".pdf", CreationCollisionOption.ReplaceExisting);
                    await mp.SfPdfSave(sharefile);
                }
                if (Settings.Default.ShareFileType == 2)
                {
                    StorageFile shareFile = await folder.CreateFileAsync(Settings.Default.DefaultShareName + ".html", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(shareFile, ConvertToHtml(me, mp.EncodingList));
                }
                if (Settings.Default.ShareFileType == 3)
                {
                    StorageFile shareFile = await folder.CreateFileAsync(Settings.Default.DefaultShareName + ".docx", CreationCollisionOption.ReplaceExisting);
                    mp.SfSave(shareFile, FormatType.Docx);
                }
                if (Settings.Default.ShareFileType == 4)
                {
                    StorageFile shareFile = await folder.CreateFileAsync(Settings.Default.DefaultShareName + ".doc", CreationCollisionOption.ReplaceExisting);
                    mp.SfSave(shareFile, FormatType.Doc);
                }
                if (Settings.Default.ShareFileType == 5)
                {
                    StorageFile shareFile = await folder.CreateFileAsync(Settings.Default.DefaultShareName + ".epub", CreationCollisionOption.ReplaceExisting);
                    mp.SfSave(shareFile, FormatType.EPub);
                }
                if (Settings.Default.ShareFileType == 6)
                {
                    StorageFile shareFile = await folder.CreateFileAsync(Settings.Default.DefaultShareName + ".txt", CreationCollisionOption.ReplaceExisting);
                    me.Document.GetText(TextGetOptions.None, out string txtstring);
                    await FileIO.WriteTextAsync(shareFile, txtstring);
                }
                if (Settings.Default.ShareFileType >= 7)
                {
                    string ext = ".jpg";
                    if (Settings.Default.ShareFileType == 8) ext = ".png";
                    if (Settings.Default.ShareFileType == 9) ext = ".bmp";
                    if (Settings.Default.ShareFileType == 10) ext = ".gif";
                    if (Settings.Default.ShareFileType == 11) ext = ".tiff";
                    StorageFile shareFile = await folder.CreateFileAsync(Settings.Default.DefaultShareName + ext, CreationCollisionOption.ReplaceExisting);
                    me.RequestedTheme = ElementTheme.Light;
                    me.Focus(FocusState.Programmatic);
                    Debug.WriteLine("Focus set");
                    await mp.renderTargetBitmap.RenderAsync(me);
                    Debug.WriteLine("Rendered");
                    using (var stream = await shareFile.OpenStreamForWriteAsync())
                    {
                        var logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
                        var pixelBuffer = await mp.renderTargetBitmap.GetPixelsAsync();
                        if (Settings.Default.ShareFileType == 7)
                        {
                            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream.AsRandomAccessStream()); ;
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)mp.renderTargetBitmap.PixelWidth, (uint)mp.renderTargetBitmap.PixelHeight, logicalDpi, logicalDpi, pixelBuffer.ToArray());
                            await encoder.FlushAsync();
                        }
                        if (Settings.Default.ShareFileType == 8)
                        {
                            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream.AsRandomAccessStream()); ;
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)mp.renderTargetBitmap.PixelWidth, (uint)mp.renderTargetBitmap.PixelHeight, logicalDpi, logicalDpi, pixelBuffer.ToArray());
                            await encoder.FlushAsync();
                        }
                        if (Settings.Default.ShareFileType == 9)
                        {
                            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream.AsRandomAccessStream()); ;
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)mp.renderTargetBitmap.PixelWidth, (uint)mp.renderTargetBitmap.PixelHeight, logicalDpi, logicalDpi, pixelBuffer.ToArray());
                            await encoder.FlushAsync();
                        }
                        if (Settings.Default.ShareFileType == 10)
                        {
                            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.GifEncoderId, stream.AsRandomAccessStream()); ;
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)mp.renderTargetBitmap.PixelWidth, (uint)mp.renderTargetBitmap.PixelHeight, logicalDpi, logicalDpi, pixelBuffer.ToArray());
                            await encoder.FlushAsync();
                        }
                        if (Settings.Default.ShareFileType == 11)
                        {
                            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.TiffEncoderId, stream.AsRandomAccessStream()); ;
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)mp.renderTargetBitmap.PixelWidth, (uint)mp.renderTargetBitmap.PixelHeight, logicalDpi, logicalDpi, pixelBuffer.ToArray());
                            await encoder.FlushAsync();
                        }
                        stream.Dispose();
                    }
                    if (Settings.Default.ThemeDefault == true) me.RequestedTheme = ElementTheme.Default;
                    if (Settings.Default.ThemeDark == true) me.RequestedTheme = ElementTheme.Dark;
                    if (Settings.Default.ThemeLight == true) me.RequestedTheme = ElementTheme.Light;
                }

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
                mp.ChangeLoading(false);
            }
            if (MoreOptionsList.SelectedIndex == 3)
            {
                mp.ChangeLoading(true);
                if (me.Document.Selection.Length == 0)
                {
                    DataTransferManager.GetForCurrentView().DataRequested += ShareTextAll_DataRequested;
                    DataTransferManager.ShowShareUI();
                }
                else
                {
                    DataTransferManager.GetForCurrentView().DataRequested += ShareText_DataRequested;
                    DataTransferManager.ShowShareUI();
                }
                mp.ChangeLoading(false);
            }
            if (MoreOptionsList.SelectedIndex == 4)
            {
                mp.ChangeLoading(true);
                me.Document.SetText(TextSetOptions.None, "");
                mp.ChangeLoading(false);
            }
            if (MoreOptionsList.SelectedIndex == 5) Application.Current.Exit();
            MoreOptionsList.SelectedItem = null;
        }
        public static string ConvertToHtml(RichEditBox richEditBox, List<string> list)
        {
            string strColour, strFntName, strHTML;
            richEditBox.Document.GetText(TextGetOptions.None, out string text);
            ITextRange txtRange = richEditBox.Document.GetRange(0, text.Length);
            strHTML = "<!DOCTYPE html><html>";
            if (Settings.Default.HTMLEncoding != 0)
            {
                strHTML += "<meta charset=\"" + list[Settings.Default.HTMLEncoding] + "\">";
            }
            strHTML += "<head><title>" + Settings.Default.DefaultExportName + "</title></head>";
            int lngOriginalStart = txtRange.StartPosition;
            int lngOriginalLength = txtRange.EndPosition;
            float shtSize = 11;
            // txtRange.SetRange(txtRange.StartPosition, txtRange.EndPosition);
            bool bOpened = false, liOpened = false, numbLiOpened = false, iOpened = false, uOpened = false, bulletOpened = false, numberingOpened = false;
            for (int i = 0; i < text.Length; i++)
            {
                txtRange.SetRange(i, i + 1);
                if (i == 0)
                {
                    strColour = Windows.UI.Colors.Black.ToHex().ToString();
                    shtSize = txtRange.CharacterFormat.Size;
                    strFntName = txtRange.CharacterFormat.Name;
                    strHTML += "<span style=\"font-family:" + strFntName + "; font-size: " + shtSize + "pt; color: #" + strColour.Substring(3) + "\">";
                }
                if (txtRange.CharacterFormat.Size != shtSize)
                {
                    shtSize = txtRange.CharacterFormat.Size;
                    strHTML += "</span><span style=\"font-family: " + txtRange.CharacterFormat.Name + "; font-size: " + txtRange.CharacterFormat.Size + "pt; color: #" + txtRange.CharacterFormat.ForegroundColor.ToString().Substring(3) + "\">";
                }
                if (txtRange.Character == Convert.ToChar(13))
                {
                    strHTML += "<br/>";
                }
                #region bullet
                if (txtRange.ParagraphFormat.ListType == MarkerType.Bullet)
                {
                    if (!bulletOpened)
                    {
                        strHTML += "<ul>";
                        bulletOpened = true;
                    }

                    if (!liOpened)
                    {
                        strHTML += "<li>";
                        liOpened = true;
                    }

                    if (txtRange.Character == Convert.ToChar(13))
                    {
                        strHTML += "</li>";
                        liOpened = false;
                    }
                }
                else
                {
                    if (bulletOpened)
                    {
                        strHTML += "</ul>";
                        bulletOpened = false;
                    }
                }
                #endregion
                #region numbering
                if (txtRange.ParagraphFormat.ListType == MarkerType.LowercaseRoman)
                {
                    if (!numberingOpened)
                    {
                        strHTML += "<ol type=\"i\">";
                        numberingOpened = true;
                    }

                    if (!numbLiOpened)
                    {
                        strHTML += "<li>";
                        numbLiOpened = true;
                    }

                    if (txtRange.Character == Convert.ToChar(13))
                    {
                        strHTML += "</li>";
                        numbLiOpened = false;
                    }
                }
                else
                {
                    if (numberingOpened)
                    {
                        strHTML += "</ol>";
                        numberingOpened = false;
                    }
                }
                #endregion
                #region bold
                if (txtRange.CharacterFormat.Bold == FormatEffect.On)
                {
                    if (!bOpened)
                    {
                        strHTML += "<b>";
                        bOpened = true;
                    }
                }
                else
                {
                    if (bOpened)
                    {
                        strHTML += "</b>";
                        bOpened = false;
                    }
                }
                #endregion
                #region italic
                if (txtRange.CharacterFormat.Italic == FormatEffect.On)
                {
                    if (!iOpened)
                    {
                        strHTML += "<i>";
                        iOpened = true;
                    }
                }
                else
                {
                    if (iOpened)
                    {
                        strHTML += "</i>";
                        iOpened = false;
                    }
                }
                #endregion
                #region underline
                if (txtRange.CharacterFormat.Underline == UnderlineType.Single)
                {
                    if (!uOpened)
                    {
                        strHTML += "<u>";
                        uOpened = true;
                    }
                }
                else
                {
                    if (uOpened)
                    {
                        strHTML += "</u>";
                        uOpened = false;
                    }
                }
                #endregion
                strHTML += txtRange.Character;
            }
            strHTML += "</span></html>";
            return strHTML;
        }

        private void ShareText_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.SetText(me.Document.Selection.Text);
            args.Request.Data.Properties.Title = "FastNote";
            args.Request.Data.Properties.Description = mp.resourceLoader.GetString("ShareUI_SelectedTextDesc");
        }

        private void ShareTextAll_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            me.Document.GetText(TextGetOptions.None, out string data);
            args.Request.Data.SetText(data);
            args.Request.Data.Properties.Title = "FastNote";
            args.Request.Data.Properties.Description = mp.resourceLoader.GetString("ShareUI_WholeTextDesc");
        }

        private void ShareFile_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.SetStorageItems(storageItems);
            args.Request.Data.Properties.Title = "FastNote";
            args.Request.Data.Properties.Description = mp.resourceLoader.GetString("ShareUI_FileDesc");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.ThemeDefault == true)
            {
                var c = Application.Current.Resources["SystemAltHighColor"];
                Debug.WriteLine(c.ToString());
                if (c.ToString() == "#FF000000")
                {
                    BitmapImage docx = new BitmapImage(new Uri("ms-appx:///Misc/wordnewwhite (Custom).png"));
                    BitmapImage doc = new BitmapImage(new Uri("ms-appx:///Misc/wordoldwhite (Custom).png"));
                    docx_image.Source = docx;
                    doc_image.Source = doc;
                }
                else
                {
                    BitmapImage docx = new BitmapImage(new Uri("ms-appx:///Misc/wordnewblack (Custom).png"));
                    BitmapImage doc = new BitmapImage(new Uri("ms-appx:///Misc/wordoldblack (Custom).png"));
                    docx_image.Source = docx;
                    doc_image.Source = doc;
                }
            }
            else if (Settings.Default.ThemeDark == true)
            {
                BitmapImage docx = new BitmapImage(new Uri("ms-appx:///Misc/wordnewwhite (Custom).png"));
                BitmapImage doc = new BitmapImage(new Uri("ms-appx:///Misc/wordoldwhite (Custom).png"));
                docx_image.Source = docx;
                doc_image.Source = doc;
            }
            else if (Settings.Default.ThemeLight == true)
            {
                BitmapImage docx = new BitmapImage(new Uri("ms-appx:///Misc/wordnewblack (Custom).png"));
                BitmapImage doc = new BitmapImage(new Uri("ms-appx:///Misc/wordoldblack (Custom).png"));
                docx_image.Source = docx;
                doc_image.Source = doc;

            }
        }
        private void ShareMenu_OptionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShareMenu_OptionsList.SelectedItem = null;
        }
        private void ShareRTF_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 0;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void SharePDF_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 1;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareHTML_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 2;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareDOCX_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 3;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareDOC_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 4;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareEPUB_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 5;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareTXT_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 6;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareJPG_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 7;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void SharePNG_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 8;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareBMP_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 9;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareGIF_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 10;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareTIFF_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 11;
            MoreOptionsList.SelectedItem = ShareItem;
        }
        private void ImportMenu_OptionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImportMenu_OptionsList.SelectedItem != null)
            {
                mp.Import(ImportMenu_OptionsList.SelectedIndex);
                ImportMenu_OptionsList.SelectedItem = null;
            }
        }
        private void ImportBefore_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Settings.Default.ImportOption = 0;
        }

        private void ImportAfter_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Settings.Default.ImportOption = 1;
        }

        private void ImportReplace_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Settings.Default.ImportOption = 2;
        }
    }
}
