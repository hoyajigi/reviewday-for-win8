//http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh872190.aspx
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236
using Windows.UI.ApplicationSettings;
namespace ContosoCookbook
{
    public sealed partial class AboutUserControl : UserControl
    {
        public AboutUserControl()
        {
            this.InitializeComponent();
        }
        private void OnBackButtonClicked(object sender, RoutedEventArgs e)
        {
            if (this.Parent.GetType() == typeof(Popup))
            {
                ((Popup)this.Parent).IsOpen = false;
            }
            SettingsPane.Show();
        }

    }
}
