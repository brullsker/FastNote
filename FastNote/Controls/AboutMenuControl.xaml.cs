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

namespace FastNote.Controls
{
    public sealed partial class AboutMenuControl : UserControl
    {
        MainPage mp;
        public string AppVersion
        {
            get { return _appVersion; }
            set
            {
                _appVersion = value;
                AboutAppTextBlock.Text = AppVersion;
            }
        }
        private string _appVersion;

        public AboutMenuControl(MainPage m)
        {
            this.InitializeComponent();

            mp = m;
        }

        private void DonateLink_Click(object sender, RoutedEventArgs e)
        {
            mp.DonateLink_Click(sender, e);
        }
    }
}
