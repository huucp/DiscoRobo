using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DiscoRoboOfficial
{
    public partial class Tutorial12 : PhoneApplicationPage
    {
        public Tutorial12()
        {
            InitializeComponent();
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("/Tutorial/Tutorial11.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("/MainPage.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }
    }
}