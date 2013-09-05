using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace DiscoRoboOfficial.Tutorial
{
    public partial class Tutorial9 : PhoneApplicationPage
    {
        public Tutorial9()
        {
            InitializeComponent();
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("/Tutorial/Tutorial8.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("/Tutorial/Tutorial10.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }
    }
}