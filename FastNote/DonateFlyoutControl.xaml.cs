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
    public sealed partial class DonateFlyoutControl : UserControl
    {
        Misc.Purchases purch = new Misc.Purchases();
        public DonateFlyoutControl()
        {
            this.InitializeComponent();
        }
        private void Donate0099_Click(object sender, RoutedEventArgs e)
        {
            purch.PurchaseAddOn(0);
        }

        private void Donate0249_Click(object sender, RoutedEventArgs e)
        {
            purch.PurchaseAddOn(1);
        }

        private void Donate0499_Click(object sender, RoutedEventArgs e)
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
    }
}
