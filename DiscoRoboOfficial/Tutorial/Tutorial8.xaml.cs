using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace DiscoRoboOfficial.Tutorial
{
    public partial class Tutorial8 : PhoneApplicationPage
    {
        public Tutorial8()
        {
            InitializeComponent();
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("/Tutorial/Tutorial7.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("/Tutorial/Tutorial9.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }
    }
}