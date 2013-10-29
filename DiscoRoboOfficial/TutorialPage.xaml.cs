using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
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

        private void NextButton_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var uri = new Uri("/UIComponent/btn_bg_press.png", UriKind.Relative);            
            NextButton.Background = new ImageBrush
                                        {
                                            ImageSource = new BitmapImage(uri)
                                        };            
        }

        private void NextButton_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var uri = new Uri("/UIComponent/btn_bg_nor.png", UriKind.Relative);
            var background = new BitmapImage(uri);
            NextButton.Background = new ImageBrush { ImageSource = background, Stretch = Stretch.Fill };
        }
    }
}