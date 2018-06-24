using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Helpers;
using Windows.UI.Text;
using Windows.Graphics.Printing;
using Windows.UI.Xaml.Printing;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using System.Globalization;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace FastNote
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        string documentName = "doc.rtf";
        string documentTempName = "temp.rtf";

        StorageFolder storageFolder;
        StorageFile file;
        StorageFile tempFile;

        DispatcherTimer timer = new DispatcherTimer();

        RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
        ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

        string[] fonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies();

        public MainPage()
        {
            this.InitializeComponent();

            timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            timer.Tick += Timer_Tick;
            ShareSelectedTextContent.Visibility = Visibility.Collapsed;
            if (Settings.Default.ThemeDefault == true) this.RequestedTheme = ElementTheme.Default;
            if (Settings.Default.ThemeDark == true) this.RequestedTheme = ElementTheme.Dark;
            if (Settings.Default.ThemeLight == true) this.RequestedTheme = ElementTheme.Light;
            foreach (string font in fonts) { Debug.WriteLine("Font: " + font); }
            List<string> FontList = fonts.ToList<string>();
            FontList.Sort();
            foreach (string font in FontList) { Debug.WriteLine("Sorted Font: " + font); }
            LoadDocument();
        }

        public async void LoadDocument()
        {
            storageFolder = ApplicationData.Current.LocalFolder;
            Debug.WriteLine("Loading document: StorageFolder found");
            string filepath = storageFolder.Path.ToString() + "/" + documentName;
            Debug.WriteLine("Loading document: filepath set");
            if (File.Exists(filepath))
            {
                Debug.WriteLine(filepath + " exists. It will be loaded now.");
            }
            else
            {
                Debug.WriteLine(filepath + " exists not. It will be created now.");
                file = await storageFolder.CreateFileAsync(documentName);
                Debug.WriteLine(filepath + "File was created. It will be loaded now.");
            }
            file = await storageFolder.GetFileAsync(documentName);
            if (File.Exists(storageFolder.Path.ToString() + "/" + documentTempName) == false) await storageFolder.CreateFileAsync(documentTempName);
            tempFile = await storageFolder.GetFileAsync(documentTempName);
            Debug.WriteLine("Temp file found.");
            await file.CopyAndReplaceAsync(tempFile);
            Debug.WriteLine("file copied");
            IRandomAccessStream randAccStream = await tempFile.OpenAsync(FileAccessMode.Read);
            MainEdit.Document.LoadFromStream(Windows.UI.Text.TextSetOptions.FormatRtf, randAccStream);
            Debug.WriteLine("File loaded");
            randAccStream.Dispose();
            MainEdit.Focus(FocusState.Keyboard);
            timer.Start();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (AboutAppTextBlock.Text == "1") AboutAppTextBlock.Text = string.Format("{0} {1}.{2}.{3}.{4}", Package.Current.DisplayName.ToString().ToUpper(), Package.Current.Id.Version.Major.ToString(), Package.Current.Id.Version.Minor.ToString(), Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision);
            MainView.IsPaneOpen = true;
            MainView_PaneOpening(sender, e);
        }

        bool updatetheme = false;
        private async void ThemeRB_Checked(object sender, RoutedEventArgs e)
        {
            if (updatetheme == false) updatetheme = true;
            else
            {
                await MainView.Fade(value: 0f, duration: 125, delay: 0, easingType: EasingType.Linear).StartAsync();
                if (TDefault.IsChecked == true) this.RequestedTheme = ElementTheme.Default;
                if (TDark.IsChecked == true) this.RequestedTheme = ElementTheme.Dark;
                if (TLight.IsChecked == true) this.RequestedTheme = ElementTheme.Light;
                await MainView.Fade(value: 1f, duration: 125, delay: 0, easingType: EasingType.Linear).StartAsync();
            }
        }

        private async void Timer_Tick(object sender, object e)
        {
            Debug.WriteLine("Saving file");
            StorageFile file = await storageFolder.GetFileAsync(documentName);
            CachedFileManager.DeferUpdates(file);
            IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite);
            MainEdit.Document.SaveToStream(Windows.UI.Text.TextGetOptions.FormatRtf, randAccStream);
            Debug.WriteLine("File saved");
            randAccStream.Dispose();
            Debug.WriteLine(Window.Current.Bounds.Width.ToString() + "; " + Window.Current.Bounds.Height.ToString());
        }

        private void SettingsButton_Close_Click(object sender, RoutedEventArgs e)
        {
            MainView.IsPaneOpen = false;
        }

        private async void MoreOptionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MoreOptionsList.SelectedIndex == 0)
            {
                LoadingControl.IsLoading = true;
                Windows.Storage.Pickers.FileSavePicker picker = new Windows.Storage.Pickers.FileSavePicker();
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                picker.FileTypeChoices.Add("Rich Text", new List<string>() { ".rtf" });
                picker.FileTypeChoices.Add("HTML page", new List<string>() { ".html" });
                picker.FileTypeChoices.Add("Plain text", new List<string>() { ".txt" });
                picker.FileTypeChoices.Add("JPG image", new List<string>() { ".jpg" });
                picker.FileTypeChoices.Add("Portable Networks Graphics", new List<string>() { ".png" });
                picker.FileTypeChoices.Add("Bitmap", new List<string>() { ".bmp" });
                picker.FileTypeChoices.Add("GIF image", new List<string>() { ".gif" });
                picker.FileTypeChoices.Add("TIFF image", new List<string>() { ".tiff" });
                picker.SuggestedFileName = Settings.Default.DefaultExportName;
                StorageFile saveFile = await picker.PickSaveFileAsync();
                if (saveFile != null)
                {
                    if (saveFile.FileType == ".rtf")
                    {
                        MainEdit.RequestedTheme = ElementTheme.Light;
                        await file.CopyAndReplaceAsync(saveFile);
                        if (Settings.Default.ThemeDefault == true) MainEdit.RequestedTheme = ElementTheme.Default;
                        if (Settings.Default.ThemeDark == true) MainEdit.RequestedTheme = ElementTheme.Dark;
                        if (Settings.Default.ThemeLight == true) MainEdit.RequestedTheme = ElementTheme.Light;
                    }
                    if (saveFile.FileType == ".html") await FileIO.WriteTextAsync(saveFile, ConvertToHtml(MainEdit));
                    if (saveFile.FileType == ".txt")
                    {
                        MainEdit.Document.GetText(TextGetOptions.None, out string txtstring);
                        await FileIO.WriteTextAsync(saveFile, txtstring);
                    }
                    if (saveFile.FileType == ".jpg")
                    {
                        MainEdit.RequestedTheme = ElementTheme.Light;
                        MainEdit.Focus(FocusState.Programmatic);
                        Debug.WriteLine("Focus set");
                        await renderTargetBitmap.RenderAsync(MainEdit);
                        Debug.WriteLine("Rendered");
                        using (var stream = await saveFile.OpenStreamForWriteAsync())
                        {
                            var logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
                            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
                            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream.AsRandomAccessStream());
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)renderTargetBitmap.PixelWidth, (uint)renderTargetBitmap.PixelHeight, logicalDpi, logicalDpi, pixelBuffer.ToArray());
                            await encoder.FlushAsync();
                        }
                        if (Settings.Default.ThemeDefault == true) MainEdit.RequestedTheme = ElementTheme.Default;
                        if (Settings.Default.ThemeDark == true) MainEdit.RequestedTheme = ElementTheme.Dark;
                        if (Settings.Default.ThemeLight == true) MainEdit.RequestedTheme = ElementTheme.Light;
                    }
                    if (saveFile.FileType == ".png")
                    {
                        MainEdit.RequestedTheme = ElementTheme.Light;
                        MainEdit.Focus(FocusState.Programmatic);
                        Debug.WriteLine("Focus set");
                        await renderTargetBitmap.RenderAsync(MainEdit);
                        Debug.WriteLine("Rendered");
                        using (var stream = await saveFile.OpenStreamForWriteAsync())
                        {
                            var logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
                            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
                            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream.AsRandomAccessStream());
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)renderTargetBitmap.PixelWidth, (uint)renderTargetBitmap.PixelHeight, logicalDpi, logicalDpi, pixelBuffer.ToArray());
                            await encoder.FlushAsync();
                        }
                        if (Settings.Default.ThemeDefault == true) MainEdit.RequestedTheme = ElementTheme.Default;
                        if (Settings.Default.ThemeDark == true) MainEdit.RequestedTheme = ElementTheme.Dark;
                        if (Settings.Default.ThemeLight == true) MainEdit.RequestedTheme = ElementTheme.Light;
                    }
                    if (saveFile.FileType == ".bmp")
                    {
                        MainEdit.RequestedTheme = ElementTheme.Light;
                        MainEdit.Focus(FocusState.Programmatic);
                        Debug.WriteLine("Focus set");
                        await renderTargetBitmap.RenderAsync(MainEdit);
                        Debug.WriteLine("Rendered");
                        using (var stream = await saveFile.OpenStreamForWriteAsync())
                        {
                            var logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
                            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
                            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream.AsRandomAccessStream());
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)renderTargetBitmap.PixelWidth, (uint)renderTargetBitmap.PixelHeight, logicalDpi, logicalDpi, pixelBuffer.ToArray());
                            await encoder.FlushAsync();
                        }
                        if (Settings.Default.ThemeDefault == true) MainEdit.RequestedTheme = ElementTheme.Default;
                        if (Settings.Default.ThemeDark == true) MainEdit.RequestedTheme = ElementTheme.Dark;
                        if (Settings.Default.ThemeLight == true) MainEdit.RequestedTheme = ElementTheme.Light;
                    }
                    if (saveFile.FileType == ".gif")
                    {
                        MainEdit.RequestedTheme = ElementTheme.Light;
                        MainEdit.Focus(FocusState.Programmatic);
                        Debug.WriteLine("Focus set");
                        await renderTargetBitmap.RenderAsync(MainEdit);
                        Debug.WriteLine("Rendered");
                        using (var stream = await saveFile.OpenStreamForWriteAsync())
                        {
                            var logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
                            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
                            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.GifEncoderId, stream.AsRandomAccessStream());
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)renderTargetBitmap.PixelWidth, (uint)renderTargetBitmap.PixelHeight, logicalDpi, logicalDpi, pixelBuffer.ToArray());
                            await encoder.FlushAsync();
                        }
                        if (Settings.Default.ThemeDefault == true) MainEdit.RequestedTheme = ElementTheme.Default;
                        if (Settings.Default.ThemeDark == true) MainEdit.RequestedTheme = ElementTheme.Dark;
                        if (Settings.Default.ThemeLight == true) MainEdit.RequestedTheme = ElementTheme.Light;
                    }
                    if (saveFile.FileType == ".tiff")
                    {
                        MainEdit.RequestedTheme = ElementTheme.Light;
                        MainEdit.Focus(FocusState.Programmatic);
                        Debug.WriteLine("Focus set");
                        await renderTargetBitmap.RenderAsync(MainEdit);
                        Debug.WriteLine("Rendered");
                        using (var stream = await saveFile.OpenStreamForWriteAsync())
                        {
                            var logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
                            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
                            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.TiffEncoderId, stream.AsRandomAccessStream());
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)renderTargetBitmap.PixelWidth, (uint)renderTargetBitmap.PixelHeight, logicalDpi, logicalDpi, pixelBuffer.ToArray());
                            await encoder.FlushAsync();
                        }
                        if (Settings.Default.ThemeDefault == true) MainEdit.RequestedTheme = ElementTheme.Default;
                        if (Settings.Default.ThemeDark == true) MainEdit.RequestedTheme = ElementTheme.Dark;
                        if (Settings.Default.ThemeLight == true) MainEdit.RequestedTheme = ElementTheme.Light;
                    }
                }
                else
                {
                    MessageDialog md = new MessageDialog(resourceLoader.GetString("Dialog_FileNotSaved"), resourceLoader.GetString("Dialog_OperationCancelled"));
                    await md.ShowAsync();
                }
                LoadingControl.IsLoading = false;
            }
            if (MoreOptionsList.SelectedIndex == 1)
            {
                LoadingControl.IsLoading = true;

                StorageFolder folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("share", CreationCollisionOption.ReplaceExisting);

                if (Settings.Default.ShareFileType == 0)
                {
                    StorageFile sharefile = await file.CopyAsync(folder, Settings.Default.DefaultShareName + ".rtf", NameCollisionOption.ReplaceExisting);
                }
                if (Settings.Default.ShareFileType == 1)
                {
                    StorageFile shareFile = await folder.CreateFileAsync(Settings.Default.DefaultShareName + ".html", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(shareFile, ConvertToHtml(MainEdit));
                }
                if (Settings.Default.ShareFileType == 2)
                {
                    StorageFile shareFile = await folder.CreateFileAsync(Settings.Default.DefaultShareName + ".txt", CreationCollisionOption.ReplaceExisting);
                    MainEdit.Document.GetText(TextGetOptions.None, out string txtstring);
                    await FileIO.WriteTextAsync(shareFile, txtstring);
                }
                if (Settings.Default.ShareFileType >= 3)
                {
                    string ext = ".jpg";
                    if (Settings.Default.ShareFileType == 4) ext = ".png";
                    if (Settings.Default.ShareFileType == 5) ext = ".bmp";
                    if (Settings.Default.ShareFileType == 6) ext = ".gif";
                    if (Settings.Default.ShareFileType == 7) ext = ".tiff";
                    StorageFile shareFile = await folder.CreateFileAsync(Settings.Default.DefaultShareName + ext, CreationCollisionOption.ReplaceExisting);
                    MainEdit.RequestedTheme = ElementTheme.Light;
                    MainEdit.Focus(FocusState.Programmatic);
                    Debug.WriteLine("Focus set");
                    await renderTargetBitmap.RenderAsync(MainEdit);
                    Debug.WriteLine("Rendered");
                    using (var stream = await shareFile.OpenStreamForWriteAsync())
                    {
                        var logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
                        var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
                        if (Settings.Default.ShareFileType == 3)
                        {
                            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream.AsRandomAccessStream()); ;
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)renderTargetBitmap.PixelWidth, (uint)renderTargetBitmap.PixelHeight, logicalDpi, logicalDpi, pixelBuffer.ToArray());
                            await encoder.FlushAsync();
                        }
                        if (Settings.Default.ShareFileType == 4)
                        {
                            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream.AsRandomAccessStream()); ;
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)renderTargetBitmap.PixelWidth, (uint)renderTargetBitmap.PixelHeight, logicalDpi, logicalDpi, pixelBuffer.ToArray());
                            await encoder.FlushAsync();
                        }
                        if (Settings.Default.ShareFileType == 5)
                        {
                            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream.AsRandomAccessStream()); ;
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)renderTargetBitmap.PixelWidth, (uint)renderTargetBitmap.PixelHeight, logicalDpi, logicalDpi, pixelBuffer.ToArray());
                            await encoder.FlushAsync();
                        }
                        if (Settings.Default.ShareFileType == 6)
                        {
                            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.GifEncoderId, stream.AsRandomAccessStream()); ;
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)renderTargetBitmap.PixelWidth, (uint)renderTargetBitmap.PixelHeight, logicalDpi, logicalDpi, pixelBuffer.ToArray());
                            await encoder.FlushAsync();
                        }
                        if (Settings.Default.ShareFileType == 7)
                        {
                            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.TiffEncoderId, stream.AsRandomAccessStream()); ;
                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)renderTargetBitmap.PixelWidth, (uint)renderTargetBitmap.PixelHeight, logicalDpi, logicalDpi, pixelBuffer.ToArray());
                            await encoder.FlushAsync();
                        }
                        stream.Dispose();
                    }
                    if (Settings.Default.ThemeDefault == true) MainEdit.RequestedTheme = ElementTheme.Default;
                    if (Settings.Default.ThemeDark == true) MainEdit.RequestedTheme = ElementTheme.Dark;
                    if (Settings.Default.ThemeLight == true) MainEdit.RequestedTheme = ElementTheme.Light;
                }

                IReadOnlyList<StorageFile> pickedFiles = await folder.GetFilesAsync();

                if (pickedFiles.Count > 0)
                {
                    this.storageItems = pickedFiles;

                    // Display the file names in the UI.
                    string selectedFiles = String.Empty;
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
                LoadingControl.IsLoading = false;
            }
            if (MoreOptionsList.SelectedIndex == 2)
            {
                LoadingControl.IsLoading = true;
                if (MainEdit.Document.Selection.Length == 0)
                {
                    DataTransferManager.GetForCurrentView().DataRequested += ShareTextAll_DataRequested;
                    DataTransferManager.ShowShareUI();
                }
                else
                {
                    DataTransferManager.GetForCurrentView().DataRequested += ShareText_DataRequested;
                    DataTransferManager.ShowShareUI();
                }
                LoadingControl.IsLoading = false;
            }
            if (MoreOptionsList.SelectedIndex == 3)
            {
                LoadingControl.IsLoading = true;
                await file.DeleteAsync();
                timer.Stop();
                LoadDocument();
                LoadingControl.IsLoading = false;
            }
            if (MoreOptionsList.SelectedIndex == 4) Application.Current.Exit();
            MoreOptionsList.SelectedItem = null;
        }

        private IReadOnlyList<StorageFile> storageItems;

        private void ShareText_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.SetText(MainEdit.Document.Selection.Text);
            args.Request.Data.Properties.Title = Package.Current.DisplayName;
            args.Request.Data.Properties.Description = "Share selected text";
        }

        private void ShareTextAll_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            MainEdit.Document.GetText(Windows.UI.Text.TextGetOptions.None, out string data);
            args.Request.Data.SetText(data);
            args.Request.Data.Properties.Title = Package.Current.DisplayName;
            args.Request.Data.Properties.Description = "Share whole text";
        }

        private void ShareFile_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.SetStorageItems(this.storageItems);
            args.Request.Data.Properties.Title = Package.Current.DisplayName;
            args.Request.Data.Properties.Description = "Share file";
        }

        private void MainView_PaneOpening(object sender, RoutedEventArgs e)
        {
            SettingsButton.Scale(scaleX: 0f, scaleY: 0f, centerX: 34, centerY: 24, duration: 250, delay: 0, easingType: EasingType.Linear).Start();
            SettingsButton_Close.Scale(scaleX: 1f, scaleY: 1f, centerX: 34, centerY: 24, duration: 250, delay: 0, easingType: EasingType.Linear).Start();
        }

        private void MainView_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            SettingsButton_Close.Scale(scaleX: 0f, scaleY: 0f, centerX: 34, centerY: 24, duration: 250, delay: 0, easingType: EasingType.Linear).Start();
            SettingsButton.Scale(scaleX: 1f, scaleY: 1f, centerX: 34, centerY: 24, duration: 250, delay: 0, easingType: EasingType.Linear).Start();
        }

        private void MainEdit_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (MainEdit.Document.Selection.Length == 0) { ShareSelectedTextContent.Visibility = Visibility.Collapsed; ShareWholeTextContent.Visibility = Visibility.Visible; }
            else { ShareWholeTextContent.Visibility = Visibility.Collapsed; ShareSelectedTextContent.Visibility = Visibility.Visible; }
        }

        public static string ConvertToHtml(RichEditBox richEditBox)
        {
            string strColour, strFntName, strHTML;
            richEditBox.Document.GetText(TextGetOptions.None, out string text);
            ITextRange txtRange = richEditBox.Document.GetRange(0, text.Length);
            strHTML = "<html>";
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

        private void RestoreDefaultExpFN_Click(object sender, RoutedEventArgs e)
        {
            DefExpFN.Text = "FastNote Export";
        }

        private void RestoreDefaultShrFN_Click(object sender, RoutedEventArgs e)
        {
            DefShrFN.Text = "FastNote Share";
        }

        private void MainEdit_TextChanged(object sender, RoutedEventArgs e)
        {
            MainEdit.Document.GetText(TextGetOptions.None, out string charcount);
            CharCount.Text = Convert.ToString(charcount.Length - 1);
        }

        private void ShareRTF_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 0;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareHTML_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 1;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareTXT_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 2;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareJPG_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 3;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void SharePNG_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 4;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareBMP_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 5;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareGIF_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 6;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareTIFF_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 7;
            MoreOptionsList.SelectedItem = ShareItem;
        }
    }
}
