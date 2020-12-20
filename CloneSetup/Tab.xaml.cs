using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CloneSetup
{
    /// <summary>
    /// Interaction logic for Tab.xaml
    /// </summary>
    public partial class Tab : Page
    {
        public Tab()
        {
            InitializeComponent();

          
        }

        private void tabClone_ContentRendered(object sender, EventArgs e)
        {
            string username = string.Empty;
            string uri = this.NavigationService.CurrentSource.ToString();
            string querystring = uri.ToString().Substring(uri.IndexOf('?'));
            System.Collections.Specialized.NameValueCollection parameters = HttpUtility.ParseQueryString(querystring);

            string val = parameters["key1"];
            if (!string.IsNullOrEmpty(val))
            {
                username = val;
            }

            tabClone.Source = new Uri("SetupClone.xaml?key1=" + username, UriKind.RelativeOrAbsolute);
        }
        private void tabWHClone_ContentRendered(object sender, EventArgs e)
        {
            string username = string.Empty;
            string uri = this.NavigationService.CurrentSource.ToString();
            string querystring = uri.ToString().Substring(uri.IndexOf('?'));
            System.Collections.Specialized.NameValueCollection parameters = HttpUtility.ParseQueryString(querystring);

            string val = parameters["key1"];
            if (!string.IsNullOrEmpty(val))
            {
                username = val;
            }

            tabWHClone.Source = new Uri("WHLoginSetup.xaml?key1=" + username, UriKind.RelativeOrAbsolute);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Page1.xaml", UriKind.RelativeOrAbsolute));
        }
    }
}
