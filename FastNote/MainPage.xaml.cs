using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
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

       
        public MainPage()
        {
            this.InitializeComponent();

            timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            timer.Tick += Timer_Tick;
            ShareSelectedTextContent.Visibility = Visibility.Collapsed;

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
        }

        private async void ThemeRB_Checked(object sender, RoutedEventArgs e)
        {
            await MainView.Fade(value: 0f, duration: 125, delay: 0, easingType: EasingType.Linear).StartAsync();
            if (TDefault.IsChecked == true) this.RequestedTheme = ElementTheme.Default;
            if (TDark.IsChecked == true) this.RequestedTheme = ElementTheme.Dark;
            if (TLight.IsChecked == true) this.RequestedTheme = ElementTheme.Light;
            await MainView.Fade(value: 1f, duration: 125, delay: 0, easingType: EasingType.Linear).StartAsync();
        }

        private void MainPagePage_Loaded(object sender, RoutedEventArgs e)
        {
            MainView.IsPaneOpen = true;
            MainView.IsPaneOpen = false;
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
                picker.DefaultFileExtension = ".rtf";
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                picker.FileTypeChoices.Add("Rich Text", new List<string>() { ".rtf" });
                picker.FileTypeChoices.Add("HTML page", new List<string>() { ".html" });
                picker.FileTypeChoices.Add("Plain text", new List<string>() { ".txt" });
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
                }
                else
                {
                    MessageDialog md = new MessageDialog("File wasn't saved.", "Operation cancelled");
                    await md.ShowAsync();
                }
                LoadingControl.IsLoading = false;
            }
            if (MoreOptionsList.SelectedIndex == 1)
            {                
                LoadingControl.IsLoading = true;

                StorageFolder folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("share", CreationCollisionOption.ReplaceExisting);
                StorageFile sharefile = await file.CopyAsync(folder, Settings.Default.DefaultShareName + ".rtf", NameCollisionOption.ReplaceExisting);

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

        private void MainView_PaneOpening(SplitView sender, object args)
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
            string text, strColour, strFntName, strHTML;
            richEditBox.Document.GetText(TextGetOptions.None, out text);
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
            string charcount;
            MainEdit.Document.GetText(TextGetOptions.None, out charcount);
            CharCount.Text = " " + charcount.Length.ToString();
        }
    }
}
