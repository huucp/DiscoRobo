using System;
using Microsoft.Phone.Controls;

namespace DiscoRoboOfficial.Tutorial
{
    public partial class Tutorial1 : PhoneApplicationPage
    {
        public Tutorial1()
        {
            InitializeComponent();
        }

        private void PreviousButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void NextButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var uri = new Uri("/Tutorial/Tutorial2.xaml", UriKind.Relative);
            NavigationService.Navigate(uri);
        }
    }
}