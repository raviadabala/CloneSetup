using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Payments.Domain;
using System.Web;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PaymentsSetup
{
    public class WHLoginHelper
    {
        public WHLoginHelper()
        {
        }

        #region ControllerMethods
        public string GetLoginCredentials(WHLoginSetupModel wHLoginSetup)
        {
            try
            {
                string environment = wHLoginSetup.Environment == "OCRT" ? "ocrt.realpage.com" : "sat.realpage.com";

                Context context = new Context(new Guid(ApplicationGuids.WelcomeHome), Convert.ToInt32(wHLoginSetup.PMCId), Convert.ToInt32(wHLoginSetup.SiteId), 13);
                Profiles profilesService = InitializeProfileService(context, environment);
                WHLoginHelper wHLoginHelper = new WHLoginHelper();
                Tuple<int, int, int, string> tuple = wHLoginHelper.GetProfileId(wHLoginSetup.ServerName, wHLoginSetup.PMCId, wHLoginSetup.SiteId, wHLoginSetup.EmailId, wHLoginSetup.Unit, wHLoginSetup.FirstName, wHLoginSetup.LastName, wHLoginSetup.UserFullName);

                bool status = profilesService.ChangePassword(tuple.Item1, wHLoginSetup.NewPassword, true, "");
                if (status == true && tuple.Item2 > 0 && tuple.Item3 > 0)
                {
                    StringBuilder sb = this.CreateAccount(wHLoginSetup.PMCId, wHLoginSetup.SiteId, wHLoginSetup.EmailId, tuple.Item2, tuple.Item3, environment);
                    return string.Format(@"User Id: {0} (reshId:{1}, resmId:{2}) and {3}", tuple.Item4, tuple.Item2, tuple.Item3, sb.ToString());
                }
                else
                    return "There is some issue with provided details. Please check and resubmit.";

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private StringBuilder CreateAccount(string pmcId, string siteId, string email, int reshId, int resmId, string env)
        {
            StringBuilder sb = new StringBuilder();
            string url = string.Format("https://{0}/Payments/PaymentsApi/api/v1/Gateway/CreateToken", env);
            //string url = "https://sat.realpage.com/Payments/PaymentsApi/api/v1/Gateway/CreateToken";

            string request = this.GetRequest(pmcId, siteId, email, "Checking", Convert.ToString(reshId), Convert.ToString(resmId));
            var accountResponse = JsonConvert.DeserializeObject<Account>(this.CreateToken(url, request));
            if (accountResponse != null && accountResponse.AccountReferenceId != null)
            {
                sb.Append("Ach Account Created,");
            }
            request = this.GetRequest(pmcId, siteId, email, "MasterCard", Convert.ToString(reshId), Convert.ToString(resmId));
            accountResponse = JsonConvert.DeserializeObject<Account>(this.CreateToken(url, request));
            if (accountResponse != null && accountResponse.AccountReferenceId != null)
            {
                sb.Append("MasterCard Account Created");
            }

            return sb;
        }
        private string CreateToken(string url, string request)
        {
            string responseContent;
            HttpContent httpContent = new StringContent(request);
            httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            var encoding = (StringContent)httpContent;

            using (var client = new HttpClient())
            {
                var response = client.PostAsync(url, encoding).Result;
                responseContent = response.Content.ReadAsStringAsync().Result;
            }
            return responseContent;
        }
        private string GetRequest(string pmcid, string siteid, string email, string accounttype, string reshId, string resmId)
        {
            string request = string.Empty;
            if (accounttype.Equals("Checking"))
            {
                request = "{\"Context\":{\"ClientVendorId\":\"" + pmcid + "-" + siteid + "\",\"ApplicationGuid\":\"ADD708A3-E0AC-4456-B07E-6ECB4F2A8EB2\"}," +
                                         "\"Customer\":{\"CustomerId\":\"" + reshId + "-" + resmId + "\",\"Email\":\"" + email + "\"}," +
                                         "\"Account\":{ " +
                                          "\"AccountNickName\":\"Test Bank Account\"," +
                                          "\"AccountNumber\":\"12345\"," +
                                          "\"AccountReferenceId\":\"OLRP" + Guid.NewGuid() + "\"," +
                                          "\"RoutingNumber\":\"111000025\"," +
                                          "\"AccountType\":\"Checking\"," +
                                          "\"NameOnAccount\":\"TestFullName\"," +
                                          "\"FirstName\":\"TestFirstName\"," +
                                          "\"LastName\":\"TestLastName\"," +
                                          "\"Address\":{  " +
                                             "\"Address1\":\"Water Wood\"," +
                                             "\"Address2\":\"St.\"," +
                                             "\"City\":\"Richardson\"," +
                                             "\"StateRegion\":\"Tx\"," +
                                             "\"Country\":\"US\"," +
                                             "\"PostalCode\":\"70085\" " +
                                          "}}}";
            }
            else
            {

                request = "{\"Context\":{\"ClientVendorId\":\"" + pmcid + "-" + siteid + "\",\"ApplicationGuid\":\"ADD708A3-E0AC-4456-B07E-6ECB4F2A8EB2\"}," +
                            "\"Customer\":{\"CustomerId\":\"" + reshId + "-" + resmId + "\",\"Email\":\"" + email + "\"}, " +
                            "\"Account\":{" +
                            "\"AccountNickName\":\"Test Card Account\"," +
                            "\"AccountNumber\":\"5454545454545454\"," +
                            "\"AccountReferenceId\":\"OLRP" + Guid.NewGuid() + "\"," +
                            "\"CVV\":123," +
                            "\"ExpirationMonth\":03," +
                            "\"ExpirationYear\":25," +
                            "\"AccountType\":\"MasterCard\"," +
                            "\"NameOnAccount\":\"TestFullName\"," +
                            "\"FirstName\":\"TestFirstName\", " +
                            "\"LastName\":\"TestLastName\"," +
                            "\"Address\":{" +
                            "\"Address1\":\"Water Wood\"," +
                            "\"Address2\":\"St.\"," +
                            "\"City\":\"Richardson\"," +
                            "\"StateRegion\":\"Tx\"," +
                            "\"Country\":\"US\"," +
                            "\"PostalCode\":\"70085\"" +
                            "}}}";
            }
            return request;
        }
        protected Profiles InitializeProfileService(Context context, string env)
        {
            string url = string.Format("https://{0}/WebServices/CrossFire/ResidentProfiles/Profiles.asmx", env);
            //string url = "https://sat.realpage.com/WebServices/CrossFire/ResidentProfiles/Profiles.asmx";
            Profiles profilesService = new Profiles();
            profilesService.Url = url;
            profilesService.UserAuthInfoValue = new UserAuthInfo();
            profilesService.UserAuthInfoValue.UserName = "CrossFire$internal";
            profilesService.UserAuthInfoValue.Password = "q*deTru2";
            profilesService.UserAuthInfoValue.PmcID = context.PmcId;
            profilesService.UserAuthInfoValue.SiteID = context.SiteId;
            return profilesService;
        }
        #endregion
        #region Public Methods      

        public Tuple<int, int, int, string> GetProfileId(string dbServer, string pmcId, string siteId, string email, string unit, string firstName, string lastName, string userFullName)
        {
            string userid = string.Empty, loginName = string.Empty;
            int resmId = 0, reshId = 0, profileId = 0;
            using (SqlConnection con = new SqlConnection(this.ConnectionStringFor(dbServer, "S" + siteId.ToString())))
            {
                using (SqlCommand cmd = new SqlCommand(string.Empty, con))
                {
                    con.Open();
                    string cmdText = string.Format(@"select rm.resmID,RH.Reshid from dbo.Lease l
                                                left outer JOIN dbo.Unit u WITH(NOLOCK) ON l.unitID = u.unitID
                                                LEFT OUTER JOIN dbo.Building B WITH (NOLOCK) ON u.bldgID = b.bldgID 
                                                INNER JOIN dbo.ResidentMemberLease rml WITH(NOLOCK) ON rml.leaID = l.leaID
                                                INNER JOIN dbo.ResidentMember rm WITH(NOLOCK) ON rm.resmID = rml.resmID
                                                inner Join dbo.ResidentHouseHold RH on RH.Reshid=l.Reshid 
                                                where rm.resmFirstName like '%{0}%' and rm.resmLastName like '%{1}%'
                                                and U.unitNumber = (
                                                Case 
                                                when B.bldgNumber is null or B.bldgNumber='N/A' then '{2}'
                                                else (select itemString from udfSplitString('{2}','-') where itemNumber=2) 
                                                end)", firstName, lastName, unit);
                    cmd.CommandText = cmdText;
                    cmd.CommandType = CommandType.Text;
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            resmId = Convert.ToInt32(reader["resmID"]);
                            reshId = Convert.ToInt32(reader["Reshid"]);
                        }
                    }
                    reader.Close();

                    cmdText = string.Format("update ResidentAddress set raddEmail='{0}' where resmid={1} and raddFirstName like '%{2}%' and raddLastName like '%{3}%'", email, resmId, firstName, lastName);
                    cmd.CommandText = cmdText;
                    cmd.ExecuteNonQuery();

                    loginName = SuggestLoginName(dbServer, email, pmcId, siteId);

                    cmdText = string.Format(@"select ProfileID from ResidentProfiles where resmID={0}", resmId);
                    cmd.CommandText = cmdText;
                    profileId = Convert.ToInt32(cmd.ExecuteScalar());

                    if (profileId == 0)
                    {
                        this.AddorUpdateResidentProfile(dbServer, pmcId, siteId, loginName, userFullName, resmId);

                        cmdText = string.Format(@"select ProfileID from ResidentProfiles where resmID={0}", resmId);
                        cmd.CommandText = cmdText;
                        profileId = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    cmdText = string.Format("update ResidentProfiles set LoginName='{0}' where resmID={1}", loginName, resmId);
                    cmd.CommandText = cmdText;
                    cmd.ExecuteNonQuery();
                }
            }
            return Tuple.Create(profileId, reshId, resmId, loginName);
        }

        public string SuggestLoginName(string dbServer, string email, string pmcId, string siteId)
        {
            string loginName = string.Empty;
            using (SqlConnection con = new SqlConnection(this.ConnectionStringFor(dbServer, "S" + siteId.ToString())))
            {
                using (SqlCommand cmd = new SqlCommand("uspResProfSuggestLoginName", con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@InternalEntityID", SqlDbType.VarChar).Value = pmcId;
                    cmd.Parameters.Add("@InternalUserID", SqlDbType.VarChar).Value = 13;
                    cmd.Parameters.Add("@InternalSiteID", SqlDbType.VarChar).Value = siteId;
                    cmd.Parameters.Add("@addrEmail ", SqlDbType.VarChar).Value = email;

                    loginName = Convert.ToString(cmd.ExecuteScalar());
                    con.Close();
                }
            }
            return loginName;
        }
        public int AddorUpdateResidentProfile(string dbServer, string pmcId, string siteId, string loginName, string userFullName, int resmid)
        {
            int profileId = 0;
            using (SqlConnection con = new SqlConnection(this.ConnectionStringFor(dbServer, "S" + siteId.ToString())))
            {
                using (SqlCommand cmd = new SqlCommand("uspResProfCreateOrUpdateProfile", con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@InternalEntityID", SqlDbType.VarChar).Value = pmcId;
                    cmd.Parameters.Add("@InternalUserID", SqlDbType.VarChar).Value = 13;
                    cmd.Parameters.Add("@InternalSiteID", SqlDbType.VarChar).Value = siteId;
                    cmd.Parameters.Add("@InternalUserFullname ", SqlDbType.VarChar).Value = userFullName;
                    cmd.Parameters.Add("@ProfileID ", SqlDbType.VarChar).Value = null;
                    cmd.Parameters.Add("@resmID ", SqlDbType.VarChar).Value = resmid;
                    cmd.Parameters.Add("@LoginName ", SqlDbType.VarChar).Value = loginName;
                    cmd.Parameters.Add("@Active ", SqlDbType.Bit).Value = 1;
                    cmd.Parameters.Add("@AccessOSR ", SqlDbType.Bit).Value = 1;
                    cmd.Parameters.Add("@AccessOLRP ", SqlDbType.Bit).Value = 1;
                    cmd.Parameters.Add("@AccessFeaResv ", SqlDbType.Bit).Value = 1;
                    cmd.Parameters.Add("@AccessDocs ", SqlDbType.Bit).Value = 1;
                    cmd.Parameters.Add("@AccessCal ", SqlDbType.Bit).Value = 1;
                    cmd.Parameters.Add("@ocsForGuestRegistrations ", SqlDbType.Bit).Value = 1;
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            return profileId;
        }
        #endregion

        #region Private Methods

        private string ConnectionStringFor(string dbServer, string dbName)
        {
            return new SqlConnectionStringBuilder()
            {
                DataSource = dbServer,
                InitialCatalog = dbName,
                UserID = "One_1_admin",
                Password = "admin"
                //IntegratedSecurity = true
            }.ConnectionString;

        }
        #endregion

    }
}
