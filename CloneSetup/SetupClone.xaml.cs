using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Web;
using System.Windows.Media;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;

namespace CloneSetup
{
    /// <summary>
    /// Interaction logic for Page2.xaml
    /// </summary>
    public partial class Page2 : Page
    {
        public Page2(string value)
        {
            InitializeComponent();
        }
        public Page2()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                imgLoading.Visibility = Visibility.Visible;
                btnSetup.Visibility = Visibility.Hidden;
                OnDoWork();

                imgLoading.Visibility = Visibility.Hidden;
                //BackgroundWorker worker = new BackgroundWorker();
                //worker.DoWork += OnDoWork;
                //worker.RunWorkerCompleted += OnRunWorkerCompleted;
                //worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                lblMessage.Content = "Error: " + ex.Message;
            }
            finally
            {
                imgLoading.Visibility = Visibility.Hidden;
                btnSetup.Visibility = Visibility.Visible;
            }
        }
        private void OnDoWork()
        {
            //this.Dispatcher.Invoke(() =>
            //{
            PaymentsSetup.CloneSetup setup = new PaymentsSetup.CloneSetup();

            string serverName = txtServer.Text;
            string pmcId = txtPmcId.Text;
            string siteId = txtSiteId.Text;
            string username = string.Empty;
            string requiredWHCenter = rblWH.IsChecked == true ? rblWH.Content.ToString() : (rblWelcomeHomeWidgets.IsChecked == true ? rblWelcomeHomeWidgets.Content.ToString() : rblWelcomeHomeAffordable.Content.ToString());

            string uri = this.NavigationService.CurrentSource.ToString();
            string querystring = uri.ToString().Substring(uri.IndexOf('?'));
            System.Collections.Specialized.NameValueCollection parameters = HttpUtility.ParseQueryString(querystring);

            string val = parameters["key1"];
            if (!string.IsNullOrEmpty(val))
            {
                username = val;
            }

            string userId = setup.UserIdFor(serverName, username);

            if (string.IsNullOrEmpty(userId))
            {
                lblMessage.Content = "You dont have IA Access!";
                return;
            }

            if (txtServer.Text.Trim() == string.Empty)
            {
                lblMessage.Content = "Please enter DB Server.";
                return;
            }

            if (txtPmcId.Text.Trim() == string.Empty)
            {
                lblMessage.Content = "Please enter PmcId.";
                return;
            }

            if (txtSiteId.Text.Trim() == string.Empty)
            {
                lblMessage.Content = "Please enter SiteId.";
                return;
            }

            if (!(setup.IsDBExists(serverName, "P" + pmcId) && setup.IsDBExists(serverName, "S" + siteId)))
            {
                lblMessage.Content = "P or S database does not exist. Please provide valid Pmc Id and Site Id.";
                return;
            }

            if (chkImplementation.IsChecked == false && chkWHCenter.IsChecked == false && chkPayments30.IsChecked == false && chkMIDLIDSetup.IsChecked == false)
            {
                lblMessage.Content = "Please select atleast one required setting.";
                return;
            }

            if (chkImplementation.IsChecked == true)
            {
                setup.AssignRoles(serverName, pmcId, siteId, userId);
            }
            if (chkWHCenter.IsChecked == true)
            {
                setup.ActivateWelcomeHome(serverName, pmcId, siteId, userId, requiredWHCenter);
            }
            if (chkPayments30.IsChecked == true)
            {
                setup.ActivatePayments3x(serverName, pmcId, siteId, userId);
            }
            if (chkMIDLIDSetup.IsChecked == true)
            {
                setup.ActivatePaymentsSettings(serverName, pmcId, siteId, userId);
            }

            lblMessage.Content = "Clone setup is completed!";
            lblMessage.Foreground = Brushes.Green;
            //});
        }

        private void OnRunWorkerCompleted(object o, RunWorkerCompletedEventArgs args)
        {
            imgLoading.Visibility = Visibility.Hidden;
        }

        public void ResetCache(string pmcId, string siteId, string applicationGuid)
        {
            PaymentsSetup.CloneSetup setup = new PaymentsSetup.CloneSetup();
            string serverName = txtServer.Text;

            if (!(setup.IsDBExists(serverName, "P" + pmcId) && setup.IsDBExists(serverName, "S" + siteId)))
            {
                lblMessage.Content = "P or S database does not exist. Please provide valid Pmc Id and Site Id.";
                return;
            }

            Guid appGUID = Guid.Empty;
            PaymentsSettings client = new PaymentsSettings();
            client.Url = "http://" + rblSAT.Content.ToString() + ".realpage.com/Payments/PaymentSettingsSvc/PaymentsSettings.asmx";

            if (applicationGuid != "ALL" && Guid.TryParse(applicationGuid, out appGUID))
            {
                client.ResetCachedSiteSettings(Convert.ToInt32(pmcId), Convert.ToInt32(siteId), applicationGuid);
            }
            else
            {
                client.ResetCachedSiteSettings(Convert.ToInt32(pmcId), Convert.ToInt32(siteId), "FAC7C095-D652-471C-9218-E55B7CF4A643");
                client.ResetCachedSiteSettings(Convert.ToInt32(pmcId), Convert.ToInt32(siteId), "ADD708A3-E0AC-4456-B07E-6ECB4F2A8EB2");
                client.ResetCachedSiteSettings(Convert.ToInt32(pmcId), Convert.ToInt32(siteId), "814E0549-99DF-4973-BD15-BE2A1D781AA8");
                client.ResetCachedSiteSettings(Convert.ToInt32(pmcId), Convert.ToInt32(siteId), "525DEE88-F72B-4EFB-A0A1-380539905F23");
                client.ResetCachedSiteSettings(Convert.ToInt32(pmcId), Convert.ToInt32(siteId), "4A1D892A-2179-4cab-8FBF-F0CB1F666AF1");
                client.ResetCachedSiteSettings(Convert.ToInt32(pmcId), Convert.ToInt32(siteId), "305F92F0-B732-4269-B7FC-94B598EBBF49");
                client.ResetCachedSiteSettings(Convert.ToInt32(pmcId), Convert.ToInt32(siteId), "95D38800-8215-4FA2-9302-51FC61208610");

            }
            lblMessage.Content = "Clone cache reset is done!";
            lblMessage.Foreground = Brushes.Green;
        }

        private void ResetCache_Click(object sender, RoutedEventArgs e)
        {

            this.ResetCache(txtPmcId.Text, txtSiteId.Text, "ALL");
        }
    }
}
