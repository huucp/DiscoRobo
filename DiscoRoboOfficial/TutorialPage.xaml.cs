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
    public partial class TutorialPage : PhoneApplicationPage
    {
        public TutorialPage()
        {
            InitializeComponent();
        }

        private void NextButton_OnMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var uriImage = new Uri("/UIComponent/btn_bg_press.png", UriKind.Relative);
            NextButton.Background = new ImageBrush
                                        {
                                            ImageSource = new BitmapImage(uriImage)
                                        };
            var uri = new Uri("/HelpPage.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void NextButton_OnMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var uriImage = new Uri("/UIComponent/btn_bg_nor.png", UriKind.Relative);
            var background = new BitmapImage(uriImage);
            NextButton.Background = new ImageBrush
                                        {
                                            ImageSource = background
                                        };
        }

        private void SkipButton_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var uriImage = new Uri("/UIComponent/btn_bg_press.png", UriKind.Relative);
            SkipButton.Background = new ImageBrush
            {
                ImageSource = new BitmapImage(uriImage)
            };
            var uri = new Uri("/MainPage.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void SkipButton_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var uriImage = new Uri("/UIComponent/btn_bg_nor.png", UriKind.Relative);
            SkipButton.Background = new ImageBrush
            {
                ImageSource = new BitmapImage(uriImage)
            };
        }
    }
}