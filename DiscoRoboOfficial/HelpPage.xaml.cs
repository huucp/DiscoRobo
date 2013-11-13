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
            
            ChangeModeExpander.Expanded += ChangeModeExpanderOnExpanded;
            ChangeModeExpander.Collapsed += ChangeModeExpanderOnCollapsed;

            bool showAgain;
            if (AppSettings.TryGetSetting("ShowAgain",out showAgain))
            {
                ShowAgainCheckBox.IsChecked = !showAgain;
            }
            else
            {
                ShowAgainCheckBox.IsChecked = false;
            }
        }

        private void ChangeModeExpanderOnCollapsed(object sender, RoutedEventArgs e)
        {
            ChangeModeGrid.Height = 100;
            ChangeModeExpander.Height = 73;
            var transform = new CompositeTransform {Rotation = -90};
            IndicatorImage.RenderTransform = transform;
        }

        private void ChangeModeExpanderOnExpanded(object sender, RoutedEventArgs routedEventArgs)
        {
            ChangeModeGrid.Height = 530;
            ChangeModeExpander.Height = 530;
            var transform = new CompositeTransform { Rotation = 0 };
            IndicatorImage.RenderTransform = transform;
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

        private void ShowAgainCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            AppSettings.StoreSetting("ShowAgain",false);
        }

        private void ShowAgainCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            AppSettings.StoreSetting("ShowAgain", true);
        }

        private void IndicatorImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!ChangeModeExpander.IsExpanded)
            {
                ChangeModeExpanderOnExpanded(null, null);
                ChangeModeExpander.IsExpanded = true;
            }
            else
            {
                ChangeModeExpanderOnCollapsed(null,null);
                ChangeModeExpander.IsExpanded = false;
            }
        }
    }
}