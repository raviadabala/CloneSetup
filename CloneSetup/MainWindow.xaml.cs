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
using CloneSetup.LoginService;
using PaymentsSetup;

namespace CloneSetup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PaymentsSetup.CloneSetup setup = new PaymentsSetup.CloneSetup();

                string serverName = txtServer.Text;
                string pmcId = txtPmcId.Text;
                string siteId = txtSiteId.Text;
                string username = "13";
                string requiredWHCenter = "Welcome Home";

                string userId = setup.UserIdFor(serverName, username);

                if (string.IsNullOrEmpty(userId))
                {
                    txtMessage.Text = "You dont have IA Access!";
                }

                if (!(setup.IsDBExists(serverName, "P" + pmcId) && setup.IsDBExists(serverName, "S" + siteId)))
                {
                    txtMessage.Text = "P or S database does not exist. Please provide valid Pmc Id and Site Id.";
                }

                if (chkImplementation.IsChecked == true )
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

                txtMessage.Text = "Clone setup is completed!";
            }
            catch (Exception ex)
            {
                txtMessage.Text = "Error: " + ex.Message;
            }
        }
    }
}
