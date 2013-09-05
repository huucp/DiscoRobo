using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace DiscoRoboOfficial.Tutorial
{
    public partial class Tutorial_3 : PhoneApplicationPage
    {
        public Tutorial_3()
        {
            InitializeComponent();
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("/Tutorial/Tutorial2.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("/Tutorial/Tutorial4.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }
    }
}