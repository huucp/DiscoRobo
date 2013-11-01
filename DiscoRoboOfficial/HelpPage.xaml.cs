using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DiscoRoboOfficial
{
    public partial class HelpPage : PhoneApplicationPage
    {
        public HelpPage()
        {
            InitializeComponent();
        }

        private void CloseButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var uriImage = new Uri("/UIComponent/btn_bg_press.png", UriKind.Relative);
            CloseButton.Background = new ImageBrush
            {
                ImageSource = new BitmapImage(uriImage)
            };
            var uri = new Uri("/MainPage.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void CloseButton_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var uriImage = new Uri("/UIComponent/btn_bg_nor.png", UriKind.Relative);
            CloseButton.Background = new ImageBrush
            {
                ImageSource = new BitmapImage(uriImage)
            };
        }
    }
}