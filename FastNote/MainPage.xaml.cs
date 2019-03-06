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
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.UI.Text;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.ApplicationModel.Core;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.System;
using Syncfusion.DocIO.DLS;
using Color = Windows.UI.Color;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls.Primitives;
using Microsoft.Advertising.WinRT.UI;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace FastNote
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public string appid = new Constants().appid;
        public string adid = new Constants().adid;
        public string intappid = new Constants().intappid;
        public string intadid = new Constants().intadid;

        string cversion = string.Format("{0} {1}.{2}.{3}.{4}", "FASTNOTE", Package.Current.Id.Version.Major.ToString(), Package.Current.Id.Version.Minor.ToString(), Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision);

        string documentName = "doc.rtf";
        string documentTempName = "temp.rtf";

        StorageFolder storageFolder;
        StorageFile file;
        StorageFile tempFile;

        DispatcherTimer timer = new DispatcherTimer();

        RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
        ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

        List<string> FontList = new List<string>();
        List<string> EncodingList = new List<string>();

        CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
        InterstitialAd myInterstitialAd = null;

        public MainPage()
        {
            InitializeComponent();

            if (SystemInformation.DeviceFamily == "Windows.Mobile")
            {
                if (Settings.Default.ToolBarOnBottomMobile == true)
                {
                    TopBarGrid.Visibility = Visibility.Collapsed;
                    BottomBarGrid.Visibility = Visibility.Visible;
                }
                else if (Settings.Default.ToolBarOnBottomMobile == false)
                {
                    BottomBarGrid.Visibility = Visibility.Collapsed;
                    TopBarGrid.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (Settings.Default.ToolBarOnBottomDesktop == true) Grid.SetRow(TopBarGrid, 2);
                else if (Settings.Default.ToolBarOnBottomDesktop == false)
                {
                    coreTitleBar.ExtendViewIntoTitleBar = true;
                    Window.Current.SetTitleBar(Draggable1);
                    TitleBarExtensions.SetButtonBackgroundColor(MainPagePage, Windows.UI.Colors.Transparent);
                }
            }
            if (Settings.Default.version == cversion)
            {
                Settings.Default.version = cversion;
                ChangelogPopUp();
            }
            timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            timer.Tick += Timer_Tick;
            ShareSelectedTextContent.Visibility = Visibility.Collapsed;
            if (Settings.Default.ThemeDefault == true) RequestedTheme = ElementTheme.Default;
            if (Settings.Default.ThemeDark == true) RequestedTheme = ElementTheme.Dark;
            if (Settings.Default.ThemeLight == true) RequestedTheme = ElementTheme.Light;
            if (Settings.Default.ShowAd == false) MenuAd.Visibility = Visibility.Collapsed;

            if (Settings.Default.ShowAd == true)
            {
                myInterstitialAd = new InterstitialAd();
            }
            LoadDocument();
            Debug.WriteLine("AdFree: " + Settings.Default.AdFree.ToString());
        }

        private void FontStuff()
        {
            string[] fonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies();
            Debug.WriteLine("Got fonts");
            FontList = fonts.ToList(); Debug.WriteLine("FontList made");
            FontList.Sort(); Debug.WriteLine("List ordered");
            List<ComboBoxItem> FontItems = new List<ComboBoxItem>();
            foreach (string font in FontList) { FontItems.Add(new ComboBoxItem { Content = new TextBlock { Text = font, FontFamily = new FontFamily(font) } }); };
            FontFamBox.ItemsSource = FontItems;
            if (Settings.Default.FontFamSegFS == true)
            {
                int sui = FontList.IndexOf("Segoe UI");
                Debug.WriteLine(sui);
                Settings.Default.FontFamily = sui;
                Settings.Default.FontFamSegFS = false;
            }
            else Debug.WriteLine("Not first scan");
        }

        private void HTMLStuff()
        {
            EncodingList.Add(resourceLoader.GetString("DefaultString"));
            EncodingList.Add("ANSI");
            EncodingList.Add("ASCII");
            EncodingList.Add("ISO-8859-1");
            EncodingList.Add("UTF-8");
            List<ComboBoxItem> HTMLItems = new List<ComboBoxItem>();
            foreach (string code in EncodingList) { HTMLItems.Add(new ComboBoxItem { Content = new TextBlock { Text = code } }); };
            HTMLEncodingBox.ItemsSource = HTMLItems;
        }

        public async void LoadDocument()
        {
            FontStuff();
            HTMLStuff();
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
            MainEdit.Document.LoadFromStream(TextSetOptions.FormatRtf, randAccStream);
            Debug.WriteLine("File loaded");
            randAccStream.Dispose();
            MainEdit.Focus(FocusState.Keyboard); MainEdit.Document.GetText(TextGetOptions.None, out string txt);
            MainEdit.Document.Selection.SetRange(startPosition: txt.Length, endPosition: txt.Length);
            timer.Start();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (AboutAppTextBlock.Text == "1") AboutAppTextBlock.Text = string.Format("{0} {1}.{2}.{3}.{4}", "FASTNOTE", Package.Current.Id.Version.Major.ToString(), Package.Current.Id.Version.Minor.ToString(), Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision);
            MainView.IsPaneOpen = true;
            MainView_PaneOpening(sender, e);
            if (Settings.Default.ShowAd == true && myInterstitialAd != null) myInterstitialAd.RequestAd(AdType.Display, intappid, intadid);
        }

        bool updatetheme = false;
        private async void ThemeRB_Checked(object sender, RoutedEventArgs e)
        {
            if (updatetheme == false) updatetheme = true;
            else
            {
                await MainView.Fade(value: 0f, duration: 125, delay: 0, easingType: EasingType.Linear).StartAsync();
                if (TDefault.IsChecked == true) RequestedTheme = ElementTheme.Default;
                if (TDark.IsChecked == true) RequestedTheme = ElementTheme.Dark;
                if (TLight.IsChecked == true) RequestedTheme = ElementTheme.Light;
                await MainView.Fade(value: 1f, duration: 125, delay: 0, easingType: EasingType.Linear).StartAsync();
            }
        }

        private async void Timer_Tick(object sender, object e)
        {
            //Debug.WriteLine("Saving file");
            StorageFile file = await storageFolder.GetFileAsync(documentName);
            CachedFileManager.DeferUpdates(file);
            IRandomAccessStream randAccStream = await file.OpenAsync(FileAccessMode.ReadWrite);
            MainEdit.Document.SaveToStream(TextGetOptions.FormatRtf, randAccStream);
            //Debug.WriteLine("File saved");
            randAccStream.Dispose();
            if (CreatePDFBtn.IsChecked == true)
            {
                string value = ConvertToHtml(MainEdit, EncodingList);
                control.RefreshVal(value);
            }
            //Debug.WriteLine(Window.Current.Bounds.Width.ToString() + "; " + Window.Current.Bounds.Height.ToString());
        }

        private void SettingsButton_Close_Click(object sender, RoutedEventArgs e)
        {
            MainView.IsPaneOpen = false;
        }

        private async void MoreOptionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MoreOptionsList.SelectedIndex == 0)
            {
                if (Settings.Default.ShowAd == true && InterstitialAdState.Ready == myInterstitialAd.State) myInterstitialAd.Show();
                LoadingControl.IsLoading = true;
                Windows.Storage.Pickers.FileSavePicker picker = new Windows.Storage.Pickers.FileSavePicker();
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                picker.FileTypeChoices.Add(resourceLoader.GetString("FileRTF"), new List<string>() { ".rtf" });
                picker.FileTypeChoices.Add(resourceLoader.GetString("FileHTML"), new List<string>() { ".html" });
                picker.FileTypeChoices.Add(resourceLoader.GetString("FileDocx"), new List<string>() { ".docx" });
                picker.FileTypeChoices.Add(resourceLoader.GetString("FileDoc"), new List<string>() { ".doc" });
                picker.FileTypeChoices.Add(resourceLoader.GetString("FileEpub"), new List<string>() { ".epub" });
                picker.FileTypeChoices.Add(resourceLoader.GetString("FileTXT"), new List<string>() { ".txt" });
                picker.FileTypeChoices.Add(resourceLoader.GetString("FileJPG"), new List<string>() { ".jpg" });
                picker.FileTypeChoices.Add(resourceLoader.GetString("FilePNG"), new List<string>() { ".png" });
                picker.FileTypeChoices.Add(resourceLoader.GetString("FileBMP"), new List<string>() { ".bmp" });
                picker.FileTypeChoices.Add(resourceLoader.GetString("FileGIF"), new List<string>() { ".gif" });
                picker.FileTypeChoices.Add(resourceLoader.GetString("FileTIFF"), new List<string>() { ".tiff" });
                picker.CommitButtonText = resourceLoader.GetString("CommitExportText");
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
                    if (saveFile.FileType == ".html") await FileIO.WriteTextAsync(saveFile, ConvertToHtml(MainEdit, EncodingList));
                    if (saveFile.FileType == ".docx") SfSave(saveFile, Syncfusion.DocIO.FormatType.Docx);
                    if (saveFile.FileType == ".doc") SfSave(saveFile, Syncfusion.DocIO.FormatType.Doc);
                    if (saveFile.FileType == ".epub") SfSave(saveFile, Syncfusion.DocIO.FormatType.EPub);
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
                if (Settings.Default.ShowAd == true && InterstitialAdState.Showing == myInterstitialAd.State) myInterstitialAd.Close();
            }
            if (MoreOptionsList.SelectedIndex == 1)
            {
                MainEdit.Document.GetText(TextGetOptions.None, out string value);
                if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value)) Import(2);
                else Import(Settings.Default.ImportOption);
            }

            if (MoreOptionsList.SelectedIndex == 2)
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
                    await FileIO.WriteTextAsync(shareFile, ConvertToHtml(MainEdit, EncodingList));
                }
                if (Settings.Default.ShareFileType == 2)
                {
                    StorageFile shareFile = await folder.CreateFileAsync(Settings.Default.DefaultShareName + ".docx", CreationCollisionOption.ReplaceExisting);
                    SfSave(shareFile, Syncfusion.DocIO.FormatType.Docx);
                }
                if (Settings.Default.ShareFileType == 3)
                {
                    StorageFile shareFile = await folder.CreateFileAsync(Settings.Default.DefaultShareName + ".doc", CreationCollisionOption.ReplaceExisting);
                    SfSave(shareFile, Syncfusion.DocIO.FormatType.Doc);
                }
                if (Settings.Default.ShareFileType == 4)
                {
                    StorageFile shareFile = await folder.CreateFileAsync(Settings.Default.DefaultShareName + ".epub", CreationCollisionOption.ReplaceExisting);
                    SfSave(shareFile, Syncfusion.DocIO.FormatType.EPub);
                }
                if (Settings.Default.ShareFileType == 5)
                {
                    StorageFile shareFile = await folder.CreateFileAsync(Settings.Default.DefaultShareName + ".txt", CreationCollisionOption.ReplaceExisting);
                    MainEdit.Document.GetText(TextGetOptions.None, out string txtstring);
                    await FileIO.WriteTextAsync(shareFile, txtstring);
                }
                if (Settings.Default.ShareFileType >= 6)
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
                LoadingControl.IsLoading = false;
            }
            if (MoreOptionsList.SelectedIndex == 3)
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
            if (MoreOptionsList.SelectedIndex == 4)
            {
                LoadingControl.IsLoading = true;
                MainEdit.Document.SetText(TextSetOptions.None, "");
                LoadingControl.IsLoading = false;
            }
            if (MoreOptionsList.SelectedIndex == 5) Application.Current.Exit();
            MoreOptionsList.SelectedItem = null;
        }

        private IReadOnlyList<StorageFile> storageItems;

        private void ShareText_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.SetText(MainEdit.Document.Selection.Text);
            args.Request.Data.Properties.Title = "FastNote";
            args.Request.Data.Properties.Description = resourceLoader.GetString("ShareUI_SelectedTextDesc");
        }

        private void ShareTextAll_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            MainEdit.Document.GetText(TextGetOptions.None, out string data);
            args.Request.Data.SetText(data);
            args.Request.Data.Properties.Title = "FastNote";
            args.Request.Data.Properties.Description = resourceLoader.GetString("ShareUI_WholeTextDesc");
        }

        private void ShareFile_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.SetStorageItems(storageItems);
            args.Request.Data.Properties.Title = "FastNote";
            args.Request.Data.Properties.Description = resourceLoader.GetString("ShareUI_FileDesc");
        }

        private void MainView_PaneOpening(object sender, RoutedEventArgs e)
        {
            SettingsButton.Scale(scaleX: 0f, scaleY: 0f, centerX: 34, centerY: 24, duration: 250, delay: 0, easingType: EasingType.Linear).Start();
            SettingsButton2.Scale(scaleX: 0f, scaleY: 0f, centerX: 34, centerY: 24, duration: 250, delay: 0, easingType: EasingType.Linear).Start();
            SettingsButton_Close.Scale(scaleX: 1f, scaleY: 1f, centerX: 34, centerY: 24, duration: 250, delay: 0, easingType: EasingType.Linear).Start();
        }

        private void MainView_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            SettingsButton_Close.Scale(scaleX: 0f, scaleY: 0f, centerX: 34, centerY: 24, duration: 250, delay: 0, easingType: EasingType.Linear).Start();
            SettingsButton.Scale(scaleX: 1f, scaleY: 1f, centerX: 34, centerY: 24, duration: 250, delay: 0, easingType: EasingType.Linear).Start();
            SettingsButton2.Scale(scaleX: 1f, scaleY: 1f, centerX: 34, centerY: 24, duration: 250, delay: 0, easingType: EasingType.Linear).Start();
        }

        private void MainEdit_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (MainEdit.Document.Selection.Length == 0) { ShareSelectedTextContent.Visibility = Visibility.Collapsed; ShareWholeTextContent.Visibility = Visibility.Visible; }
            else { ShareWholeTextContent.Visibility = Visibility.Collapsed; ShareSelectedTextContent.Visibility = Visibility.Visible; }
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

        private void ShareDOCX_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 2;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareDOC_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 3;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareEPUB_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 4;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareTXT_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 5;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareJPG_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 6;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void SharePNG_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 7;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareBMP_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 8;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareGIF_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 9;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void ShareTIFF_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ShareFileType = 10;
            MoreOptionsList.SelectedItem = ShareItem;
        }

        private void FontFamBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string font = FontList[Settings.Default.FontFamily];
            Debug.WriteLine(font);
            FontFamily fontfam = new FontFamily(font);
            MainEdit.FontFamily = fontfam;
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            int fs = Convert.ToInt32(FontSizeTextBox.Text);
            if (fs < 1638)
            {
                fs++;
                FontSizeTextBox.Text = fs.ToString();
            }
        }

        private void RepeatButton_Click_1(object sender, RoutedEventArgs e)
        {
            int fs = Convert.ToInt32(FontSizeTextBox.Text);
            if (fs > 0)
            {
                fs--;
                FontSizeTextBox.Text = fs.ToString();
            }
        }

        private void FontSizeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (FontSizeTextBox.Text != null)
                {
                    int fs = Convert.ToInt32(FontSizeTextBox.Text);
                    if (fs < 1)
                    {
                        fs = 1;
                        MainEdit.FontSize = fs;
                        Settings.Default.FontSize = fs;
                    }
                    else if (fs > 1368)
                    {
                        fs = 1368;
                    }
                    else
                    {
                        MainEdit.FontSize = fs;
                        Settings.Default.FontSize = fs;
                    }
                }
                else
                {
                    MainEdit.FontSize = 1;
                    Settings.Default.FontSize = 1;
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
                MainEdit.FontSize = 1;
                Settings.Default.FontSize = 1;
                FontSizeTextBox.Text = "1";
            }
        }

        private void FontSizeTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            FontSizeButtons.RequestedTheme = ElementTheme.Light;
        }

        private void FontSizeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.ThemeDefault == true) FontSizeButtons.RequestedTheme = ElementTheme.Default;
            if (Settings.Default.ThemeLight == true) FontSizeButtons.RequestedTheme = ElementTheme.Light;
            if (Settings.Default.ThemeDark == true) FontSizeButtons.RequestedTheme = ElementTheme.Dark;
        }

        private void ShareMenu_OptionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShareMenu_OptionsList.SelectedItem = null;
        }

        private void Draggable1_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Window.Current.SetTitleBar(Draggable1);
        }

        private void Draggable2_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Window.Current.SetTitleBar(Draggable2);
        }

        private void Draggable3_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Window.Current.SetTitleBar(Draggable3);
        }

        private void Draggable1_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.SetTitleBar(Draggable1);
        }

        private void Draggable2_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.SetTitleBar(Draggable2);
        }

        private void Draggable3_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Window.Current.SetTitleBar(Draggable3);
        }

        private void DraggableBtn_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (MainView.IsPaneOpen == true && MainView.DisplayMode == SplitViewDisplayMode.Inline) Window.Current.SetTitleBar(DraggableBtn);
        }

        private void ToggleToolBar_Toggled(object sender, RoutedEventArgs e)
        {
            if (SystemInformation.DeviceFamily == "Windows.Mobile")
            {
                if (Settings.Default.ToolBarOnBottomMobile == true)
                {
                    TopBarGrid.Visibility = Visibility.Collapsed;
                    BottomBarGrid.Visibility = Visibility.Visible;
                }
                else if (Settings.Default.ToolBarOnBottomMobile == false)
                {
                    BottomBarGrid.Visibility = Visibility.Collapsed;
                    TopBarGrid.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (Settings.Default.ToolBarOnBottomDesktop == true)
                {
                    var color = (Color)Resources["SystemAltHighColor"];
                    Debug.WriteLine(color.ToString());
                    Grid.SetRow(TopBarGrid, 2);
                    coreTitleBar.ExtendViewIntoTitleBar = false;
                    TitleBarExtensions.SetButtonBackgroundColor(MainPagePage, color);
                }
                else if (Settings.Default.ToolBarOnBottomDesktop == false)
                {
                    Grid.SetRow(TopBarGrid, 0);
                    coreTitleBar.ExtendViewIntoTitleBar = true;
                    Window.Current.SetTitleBar(Draggable1);
                    TitleBarExtensions.SetButtonBackgroundColor(MainPagePage, Windows.UI.Colors.Transparent);
                }
            }
        }

        private void ToggleAd_Toggled(object sender, RoutedEventArgs e)
        {
            //if (ToggleAd.IsOn == true) MenuAd.Suspend();
            //else if (ToggleAd.IsOn == false) { MenuAd.Resume(); if (MenuAd.HasAd == false) MenuAd.Refresh(); }
        }

        private void DonateLink_Click(object sender, RoutedEventArgs e)
        {
            DonateFlyout.ShowAt(DonateLink);
            //Uri uri = new Uri("https://paypal.me/brullskerservices");
            //await Launcher.LaunchUriAsync(uri);
        }

        Misc.Purchases purch = new Misc.Purchases();

        private void DonateTest_Click(object sender, RoutedEventArgs e)
        {
            purch.PurchaseAddOn(0);
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

        public async void SfSave(StorageFile docfile, Syncfusion.DocIO.FormatType formatType)
        {
            LoadingControl.IsLoading = true;
            MainEdit.RequestedTheme = ElementTheme.Light;

            StorageFile cachefile = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync("sfcache.rtf", CreationCollisionOption.ReplaceExisting);

            CachedFileManager.DeferUpdates(cachefile);
            IRandomAccessStream randAccStream = await cachefile.OpenAsync(FileAccessMode.ReadWrite);
            MainEdit.Document.SaveToStream(TextGetOptions.FormatRtf, randAccStream);
            Debug.WriteLine("File saved");
            randAccStream.Dispose();

            Stream stream = await cachefile.OpenStreamForReadAsync();
            WordDocument document = new WordDocument(stream, Syncfusion.DocIO.FormatType.Rtf);
            Stream savestream = await docfile.OpenStreamForWriteAsync();
            document.Save(savestream, formatType);
            stream.Dispose();
            savestream.Dispose();
            if (Settings.Default.ThemeDefault == true) MainEdit.RequestedTheme = ElementTheme.Default;
            if (Settings.Default.ThemeDark == true) MainEdit.RequestedTheme = ElementTheme.Dark;
            if (Settings.Default.ThemeLight == true) MainEdit.RequestedTheme = ElementTheme.Light;
            LoadingControl.IsLoading = false;
        }

        public async Task SfImport(StorageFile importFile, Syncfusion.DocIO.FormatType formatType)
        {
            StorageFile cacheFile = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync("sfimportcache.rtf", CreationCollisionOption.ReplaceExisting);
            CachedFileManager.DeferUpdates(cacheFile);

            Stream stream = await importFile.OpenStreamForReadAsync();
            WordDocument document = new WordDocument(stream, formatType);
            Stream saveStram = await cacheFile.OpenStreamForWriteAsync();
            document.Save(saveStram, Syncfusion.DocIO.FormatType.Rtf);
            stream.Dispose();
            saveStram.Dispose();
        }

        private void Donate0099_Click(object sender, RoutedEventArgs e)
        {
            purch.PurchaseAddOn(1);
        }

        private void Donate0249_Click(object sender, RoutedEventArgs e)
        {
            purch.PurchaseAddOn(2);
        }

        private void Donate0999_Click(object sender, RoutedEventArgs e)
        {
            purch.PurchaseAddOn(3);
        }

        private void Donate2499_Click(object sender, RoutedEventArgs e)
        {
            purch.PurchaseAddOn(4);
        }

        private void Donate4999_Click(object sender, RoutedEventArgs e)
        {
            purch.PurchaseAddOn(5);
        }

        private void Donate9999_Click(object sender, RoutedEventArgs e)
        {
            purch.PurchaseAddOn(6);
        }

        PDFControl control;
        bool pdfo = false;
        private void CreatePDFBtn_Click(object sender, RoutedEventArgs e) // Called when a button is clicked
        {
            string value = ConvertToHtml(MainEdit, EncodingList); //Converts the content of a RichEditBox (Rich text editor contol in UWP) to a HTML string
            control = new PDFControl(value);
            if (pdfo == false)
            {
                if (Settings.Default.ShowAd == true && myInterstitialAd.State == InterstitialAdState.Ready) { myInterstitialAd.Close(); myInterstitialAd = null; }
                MoreStack.Children.Add(control);
                pdfo = true;
            }
            else
            {
                if (Settings.Default.ShowAd == true) myInterstitialAd = new InterstitialAd();
                if (myInterstitialAd != null) myInterstitialAd.RequestAd(AdType.Display, intappid, intadid);
                int c = MoreStack.Children.Count - 1;
                MoreStack.Children.RemoveAt(c);
                pdfo = false;
            }
        }

        private void ImportMenu_OptionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImportMenu_OptionsList.SelectedItem != null)
            {
                Import(ImportMenu_OptionsList.SelectedIndex);
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

        public async void Import(int i)
        {
            if (Settings.Default.ShowAd == true && InterstitialAdState.Ready == myInterstitialAd.State) myInterstitialAd.Show();
            LoadingControl.IsLoading = true;
            Windows.Storage.Pickers.FileOpenPicker openpicker = new Windows.Storage.Pickers.FileOpenPicker();
            openpicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            openpicker.FileTypeFilter.Add(".rtf");
            openpicker.FileTypeFilter.Add(".doc");
            openpicker.FileTypeFilter.Add(".docx");
            openpicker.FileTypeFilter.Add(".epub");
            openpicker.FileTypeFilter.Add(".html");
            openpicker.FileTypeFilter.Add(".txt");
            openpicker.CommitButtonText = resourceLoader.GetString("CommitImportText");
            StorageFile importfile = await openpicker.PickSingleFileAsync();
            if (importfile != null)
            {
                if (importfile.FileType != ".rtf")
                {
                    if (importfile.FileType == ".doc") await SfImport(importfile, Syncfusion.DocIO.FormatType.Doc);
                    else if (importfile.FileType == ".docx") await SfImport(importfile, Syncfusion.DocIO.FormatType.Docx);
                    else if (importfile.FileType == ".epub") await SfImport(importfile, Syncfusion.DocIO.FormatType.EPub);
                    else if (importfile.FileType == ".html") await SfImport(importfile, Syncfusion.DocIO.FormatType.Html);
                    else if (importfile.FileType == ".txt") await SfImport(importfile, Syncfusion.DocIO.FormatType.Txt);


                    importfile = await ApplicationData.Current.LocalCacheFolder.GetFileAsync("sfimportcache.rtf");
                }

                MainEdit.Document.GetText(TextGetOptions.FormatRtf, out string content);

                IRandomAccessStream randAccStream = await importfile.OpenAsync(FileAccessMode.Read);
                MainEdit.Document.SetText(TextSetOptions.None, string.Empty);
                MainEdit.Document.LoadFromStream(TextSetOptions.FormatRtf, randAccStream);
                MainEdit.Document.GetText(TextGetOptions.FormatRtf, out string importcontent);
                Debug.WriteLine(content);
                Debug.WriteLine(importcontent);
                if (i == 0)
                {
                    Debug.WriteLine("Import before content");
                    MainEdit.Document.SetText(TextSetOptions.None, string.Empty);
                    // Sets importcontent as the content of the document
                    MainEdit.Document.SetText(TextSetOptions.FormatRtf, importcontent);
                    // Get a new text range for the active story of the document.
                    var range = MainEdit.Document.GetRange(0, importcontent.Length);
                    // Collapses the text range into a degenerate point at the end of the range for inserting.
                    range.Collapse(false);
                    // Inserts original content
                    range.SetText(TextSetOptions.FormatRtf, content);
                }
                else if (i == 1)
                {
                    Debug.WriteLine("Import after content");
                    MainEdit.Document.SetText(TextSetOptions.None, string.Empty);
                    // Sets original content as the content of the document
                    MainEdit.Document.SetText(TextSetOptions.FormatRtf, content);
                    // Get a new text range for the active story of the document.
                    var range = MainEdit.Document.GetRange(0, content.Length);
                    // Collapses the text range into a degenerate point at the end of the range for inserting.
                    range.Collapse(false);
                    // Inserts importcontent
                    range.SetText(TextSetOptions.FormatRtf, importcontent);
                }
                else Debug.WriteLine("Replace existing");
                MainEdit.Focus(FocusState.Keyboard);
            }
            else
            {
                MessageDialog md = new MessageDialog(resourceLoader.GetString("DialogCancelled"), resourceLoader.GetString("Dialog_OperationCancelled"));
                await md.ShowAsync();
            }
            if (Settings.Default.ShowAd == true && InterstitialAdState.Showing == myInterstitialAd.State) myInterstitialAd.Close();
            LoadingControl.IsLoading = false;
        }

        void ChangelogPopUp()
        {
        }

        private void BuyAdFree_Click(object sender, RoutedEventArgs e)
        {
            purch.PurchaseAddOn2();
        }
    }
}
