using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace DiscoRoboOfficial.Tutorial
{
    public partial class Tutorial10 : PhoneApplicationPage
    {
        public Tutorial10()
        {
            InitializeComponent();
        }

        private void PreviouseButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("/Tutorial/Tutorial9.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri("/Tutorial/Tutorial11.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }
    }
}