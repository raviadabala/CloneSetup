using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Default
{
    public class LedgerHelper
    {
        #region properties     
        #endregion

        #region data elements
        #endregion

        #region methods

        private LedgerToken GetNewAccessToken( string pmcId, string siteId )
        {
            var token = new LedgerToken();
            var client = new HttpClient();
            var postMessage = new Dictionary<string, string>();
            postMessage.Add( "grant_type", "client_credentials");
            postMessage.Add( "client_id", "Integrations_Login" );
            postMessage.Add( "client_secret", "BD40BCB6-01D5-4FBB-8FC9-87D9C68174AB" );
            postMessage.Add( "pmc_id", "7189747" );
            postMessage.Add( "site_id", "7189749" );
            string LedgerAPI = string.Format( "{0}{1}", "sat.realpage.com", "/api/core/authentication/login" );
            var url = new Uri( LedgerAPI );
            var request = new HttpRequestMessage( HttpMethod.Post, url )
            {
                Content = new FormUrlEncodedContent( postMessage ),
            };
            var response = client.SendAsync( request ).Result;
            if( response.IsSuccessStatusCode )
            {
                var strResponse = response.Content.ReadAsStringAsync().Result;
                token = JsonConvert.DeserializeObject<LedgerToken>( strResponse );
                token.ExpiresAt = DateTime.UtcNow.AddSeconds( token.ExpiresIn );
            }
            else
            {
                var strResponse = response.Content.ReadAsStringAsync().Result;
                throw new ApplicationException( string.Format( "{0} : {1}", "Unable to retrieve access token", strResponse ) );
            }

            return token;
        }

        public Dictionary<bool, string> OnesitePostToLedger()
        {
            try
            {
                LedgerToken ledgerToken = this.GetNewAccessToken( "7189747", "7189749" );

                var objTransactionDetails = new
                {
                    pmcGCID = "7189747",
                    transBatchID = "T:G5K6BBLLLA5",
                    changeReason = "tetsing 8 dollar trans",
					editQueue = "true"
                };

                string PosttoLedgerAPI = string.Format( "{0}{1}", "sat.realpage.com", "/api/ledger/v1/ledger/PostToLedger" );
                var baseUrl = new Uri( PosttoLedgerAPI );
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Bearer", string.Format( "{0}", ledgerToken.AccessToken ) );
                var result = client.PutAsync( baseUrl, GetPostContentFromObject( objTransactionDetails ) ).Result;
                if( result.IsSuccessStatusCode )
                {
                    return new Dictionary<bool, string> { { true, string.Empty } };
                }
                else
                {
                    var strResponse = result.Content.ReadAsStringAsync().Result;
                    return new Dictionary<bool, string> { { false, strResponse } };
                }
            }
            catch( Exception ex )
            {

                return new Dictionary<bool, string> { { false, ex.ToString() } };
            }
        }

        public StringContent GetPostContentFromObject( object obj )
        {
            StringContent httpContent = null;
            try
            {
                if( obj != null )
                {

                    var jsonString = JsonConvert.SerializeObject( obj, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, } );

                    jsonString = jsonString.Replace( "{", "{\n \"$type\": \"Realpage.OneSite.Ledger.BusinessObject.TransActivity, RealPage.OneSite.Ledger\", " );

                    httpContent = new StringContent( jsonString, Encoding.UTF8, "application/json" );
                }
            }
            catch( Exception ) { }

            return httpContent;
        }
        #endregion
    }

    public class LedgerToken
    {
        [JsonProperty( PropertyName = "access_token" )]
        public string AccessToken { get; set; }

        [JsonProperty( PropertyName = "expires_in" )]
        public int ExpiresIn { get; set; }

        public DateTime ExpiresAt { get; set; }

        public string Scope { get; set; }

        [JsonProperty( PropertyName = "token_type" )]
        public string TokenType { get; set; }

        public bool IsValidAndNotExpiring
        {
            get
            {
                return !String.IsNullOrEmpty( this.AccessToken ) && this.ExpiresAt > DateTime.UtcNow.AddSeconds( 30 );
            }
        }
    }
}