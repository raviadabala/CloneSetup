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
using PaymentsSetup;
namespace CloneSetup
{
    /// <summary>
    /// Interaction logic for WHLoginSetup.xaml
    /// </summary>
    public partial class WHLoginSetup : Page
    {
        public WHLoginSetup()
        {
            InitializeComponent();
        }

        private void btnWHSetup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WHLoginSetupModel wHLoginSetupModel = new WHLoginSetupModel();

                string uri = this.NavigationService.CurrentSource.ToString();
                string querystring = uri.ToString().Substring(uri.IndexOf('?'));
                System.Collections.Specialized.NameValueCollection parameters = HttpUtility.ParseQueryString(querystring);
                string username = string.Empty;
                string val = parameters["key1"];
                if (!string.IsNullOrEmpty(val))
                {
                    username = val;
                }

                wHLoginSetupModel.PMCId = txtWHPmcId.Text.ToString();
                wHLoginSetupModel.SiteId = txtWHSiteId.Text.ToString();
                wHLoginSetupModel.ServerName = txtWHServer.Text.ToString();
                wHLoginSetupModel.FirstName = txtResidentFirstName.Text.ToString();
                wHLoginSetupModel.LastName = txtResidentLastName.Text.ToString();
                wHLoginSetupModel.Unit = txtResidentUnit.Text.ToString();
                wHLoginSetupModel.EmailId = txtEmailId.Text.ToString();
                wHLoginSetupModel.NewPassword = txtPassword.Text.ToString();
                if (rbWHSat.IsChecked == true)
                    wHLoginSetupModel.Environment = "SAT";
                else if (rbWHOCRT.IsChecked == true)
                    wHLoginSetupModel.Environment = "OCRT";
                wHLoginSetupModel.UserFullName = username + " ialogin";

                WHLoginHelper wHLoginHelper = new WHLoginHelper();
                string message = wHLoginHelper.GetLoginCredentials(wHLoginSetupModel);
                lblWHMessage.Content = message;
                lblWHLoginUrl.Content = "https://"+ wHLoginSetupModel.Environment + ".crossfire.realpage.com/welcomehome/home/login?siteId="+ wHLoginSetupModel.SiteId + "#url=%23login";
                lblWHWidgetUrl.Content = "https://" + wHLoginSetupModel.Environment + ".crossfire.realpage.com/welcomehome/test/integrationtestharness";               
            }
            catch (Exception ex)
            {
                lblWHMessage.Content = ex.Message;
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.txtWHServer.Text = string.Empty;
            this.txtWHPmcId.Text = string.Empty;
            this.txtWHSiteId.Text = string.Empty;
            this.txtResidentFirstName.Text = string.Empty;
            this.txtResidentLastName.Text = string.Empty;
            this.txtResidentUnit.Text = string.Empty;
            this.txtEmailId.Text = string.Empty;
            this.txtPassword.Text = string.Empty;
        }

        private void hlWHLoginUrl_Click(object sender, RoutedEventArgs e)
        {
            NavigationWindow window = new NavigationWindow();
            Uri source = new Uri("http://www.c-sharpcorner.com/Default.aspx", UriKind.Absolute);
            window.Source = source; window.Show();
        }
    }
}
