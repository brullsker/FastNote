using System;
using System.Collections.Generic;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace FastNote
{
    public sealed partial class CustomTextToolBarControl : UserControl
    {
        public RichEditBox Editor;
        Windows.UI.Text.ITextCharacterFormat formatting;

        public CustomTextToolBarControl()
        {
            this.InitializeComponent();
        }

        private void BoldBtn_Checked(object sender, RoutedEventArgs e)
        {
            formatting.Bold = Windows.UI.Text.FormatEffect.On;
        }

        private void BoldBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            formatting.Bold = Windows.UI.Text.FormatEffect.Off;
        }

        private void ItalicBtn_Checked(object sender, RoutedEventArgs e)
        {
            formatting.Italic = Windows.UI.Text.FormatEffect.On;
        }

        private void ItalicBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            formatting.Italic = Windows.UI.Text.FormatEffect.Off;
        }

        private void UnderlineBtn_Checked(object sender, RoutedEventArgs e)
        {
            formatting.Underline = Windows.UI.Text.UnderlineType.Single;
        }

        private void UnderlineBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            formatting.Underline = Windows.UI.Text.UnderlineType.None;
        }

        private void StrikeOutBtn_Checked(object sender, RoutedEventArgs e)
        {
            formatting.Strikethrough = Windows.UI.Text.FormatEffect.On;
        }

        private void StrikeOutBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            formatting.Strikethrough = Windows.UI.Text.FormatEffect.Off;
        }

        private void BulletListBtn_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void BulletListBtn_Unchecked(object sender, RoutedEventArgs e)
        {

        }
    }
}
