using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace DiscoRoboOfficial.Tutorial
{
    public partial class Tutorial7 : PhoneApplicationPage
    {
        public Tutorial7()
        {
            InitializeComponent();
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("/Tutorial/Tutorial6.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("/Tutorial/Tutorial8.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }
    }
}