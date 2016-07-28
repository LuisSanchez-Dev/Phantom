using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Phantom
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public string Version
        {
            get { return "Phantom Version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        public AboutWindow()
        {
            InitializeComponent();
        }

        private void donateButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://vaporsoft.net/donate/");
        }

        private void websiteButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://vaporsoft.net/phantom/");
        }
    }
}
