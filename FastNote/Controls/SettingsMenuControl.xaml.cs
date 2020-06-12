using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace FastNote.Controls
{
    public sealed partial class SettingsMenuControl : UserControl
    {
        MainPage mp;
        RichEditBox me;
        public List<string> fontList
        {
            get { return FontList; }
            set
            {
                FontList = value;
                FontStuff();
            }
        }
        private List<string> FontList;

        public List<string> EncodingList
        {
            get { return _encodingList; }
            set
            {
                _encodingList = value;
                HTMLStuff();
            }
        }
        private List<string> _encodingList;

        public SettingsMenuControl(MainPage m, RichEditBox r)
        {
            this.InitializeComponent();
            mp = m;
            me = r;
        }

        private void FontStuff()
        {
            string[] fonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies();
            Debug.WriteLine("Got fonts");
            FontList.Clear();
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
            EncodingList.Clear();
            EncodingList.Add(mp.resourceLoader.GetString("DefaultString"));
            EncodingList.Add("ANSI");
            EncodingList.Add("ASCII");
            EncodingList.Add("ISO-8859-1");
            EncodingList.Add("UTF-8");
            List<ComboBoxItem> HTMLItems = new List<ComboBoxItem>();
            foreach (string code in EncodingList) { HTMLItems.Add(new ComboBoxItem { Content = new TextBlock { Text = code } }); };
            HTMLEncodingBox.ItemsSource = HTMLItems;
        }

        private void RestoreDefaultExpFN_Click(object sender, RoutedEventArgs e)
        {
            DefExpFN.Text = "FastNote Export";
        }

        private void RestoreDefaultShrFN_Click(object sender, RoutedEventArgs e)
        {
            DefShrFN.Text = "FastNote Share";
        }

        private void FontFamBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string font = FontList[Settings.Default.FontFamily];
            Debug.WriteLine(font);
            FontFamily fontfam = new FontFamily(font);
            me.FontFamily = fontfam;
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
                        me.FontSize = fs;
                        Settings.Default.FontSize = fs;
                    }
                    else if (fs > 1368)
                    {
                        fs = 1368;
                    }
                    else
                    {
                        me.FontSize = fs;
                        Settings.Default.FontSize = fs;
                    }
                }
                else
                {
                    me.FontSize = 1;
                    Settings.Default.FontSize = 1;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                me.FontSize = 1;
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

        bool updatetheme = false;
        private async void ThemeRB_Checked(object sender, RoutedEventArgs e)
        {
            if (updatetheme == false) updatetheme = true;
            else
            {
                await mp.MainViewFade(value: 0f, duration: 125, delay: 0, easingType: EasingType.Linear);
                if (TDefault.IsChecked == true) mp.RequestedTheme = ElementTheme.Default;
                if (TDark.IsChecked == true) mp.RequestedTheme = ElementTheme.Dark;
                if (TLight.IsChecked == true) mp.RequestedTheme = ElementTheme.Light;
                await mp.MainViewFade(value: 1f, duration: 125, delay: 0, easingType: EasingType.Linear);
            }
        }
        private void ToggleToolBar_Toggled(object sender, RoutedEventArgs e)
        {
            mp.ChangeToolBarPosition(Settings.Default.ToolBarOnBottomDesktop);
        }
    }
}
