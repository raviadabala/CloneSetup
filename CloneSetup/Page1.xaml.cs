using CloneSetup.LoginService;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CloneSetup
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        public Page1()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RPLoginSvcClient svc = new RPLoginSvcClient();
                if (svc.IsAuthenticatedUser(txtUsername.Text, txtPassword.Password))
                {
                    UserInfo info = svc.GetUserInfo(txtUsername.Text);
                    NavigationService.Navigate(new Uri("Tab.xaml?key1="+txtUsername.Text, UriKind.RelativeOrAbsolute));
                }
                else
                {
                    lblMessage.Content = "Username/password is invalid.";
                }
            }
            catch(Exception ex)
            {
                lblMessage.Content = "Username/password is invalid.";
            }
        }
    }
}
