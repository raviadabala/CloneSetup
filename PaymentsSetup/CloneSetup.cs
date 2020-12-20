using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PaymentsSetup
{
    public class CloneSetup
    {
        public string GetRoles( string dbServer, string pmcId, string siteId, string userId )
        {
            string roleList = ",";

            using( SqlConnection con = new SqlConnection( this.ConnectionStringFor(dbServer,"Security") ) )
            {
                string commandText = "select role_id from roles where role_name in ('Developer','RealPage Central','Implementation','Implementation','Online Payment Administrator','Payments Internal Administrator','Internal User')";
                using( SqlCommand cmd = new SqlCommand( commandText, con ) )
                {                    
                    cmd.CommandType = CommandType.Text;

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    adapter.Fill( ds );

                    if(ds!=null && ds.Tables[0]!=null)
                    {
                        foreach(DataRow dr in ds.Tables[0].Rows)
                        {
                            roleList += dr["Role_Id"].ToString()+",";
                        }
                    }
                }
            }

            return roleList;
        }

        public void AssignRoles( string dbServer, string pmcId, string siteId, string userId )
        {
            string roleList = this.GetRoles( dbServer, pmcId, siteId, userId );

            using( SqlConnection con = new SqlConnection( this.ConnectionStringFor( dbServer, "Security" ) ) )
            {                
                using( SqlCommand cmd = new SqlCommand( "uspUmtAssignRolesInsert", con ) )
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add( "@InternalEntityID", SqlDbType.VarChar ).Value = pmcId;
                    cmd.Parameters.Add( "@InternalUserID", SqlDbType.VarChar ).Value = userId;
                    cmd.Parameters.Add( "@InternalSiteID", SqlDbType.VarChar ).Value = pmcId;
                    cmd.Parameters.Add( "@IPVC_USERID", SqlDbType.VarChar ).Value = userId;
                    cmd.Parameters.Add( "@IPVC_ROLEIDS", SqlDbType.VarChar ).Value = roleList;

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public string UserIdFor(string dbServer, string username)
        {
            string userid = string.Empty;
            using (SqlConnection con = new SqlConnection(this.ConnectionStringFor(dbServer, "Security")))
            {
                string cmdText = string.Format("select top 10 * from security..User_profile where user_first = '{0}' and user_last='ialogin'",username);
                using (SqlCommand cmd = new SqlCommand(cmdText, con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.Text;

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataSet ds = new DataSet();
                    adapter.Fill(ds);

                    if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                    {
                        userid = ds.Tables[0].Rows[0]["User_Id"].ToString();
                    }
                }
            }

            return userid;
        }

        public void EnableOrDisableCenter( string dbServer, string pmcId, string siteId, string userId, int actId, int oneId, string status )
        {
            using( SqlConnection con = new SqlConnection( this.ConnectionStringFor( dbServer, "S" + siteId.ToString() ) ) )
            {
                using( SqlCommand cmd = new SqlCommand( "uspCGMPEditCentersUpdate", con ) )
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add( "@InternalEntityID", SqlDbType.VarChar ).Value = pmcId;
                    cmd.Parameters.Add( "@InternalUserID", SqlDbType.VarChar ).Value = userId;
                    cmd.Parameters.Add( "@InternalSiteID", SqlDbType.VarChar ).Value = siteId;
                    cmd.Parameters.Add( "@IPVC_PROPERTYID", SqlDbType.VarChar ).Value = siteId;
                    cmd.Parameters.Add( "@IPVC_ACTRID", SqlDbType.VarChar ).Value = actId;
                    cmd.Parameters.Add( "@IPVC_ONEID", SqlDbType.VarChar ).Value = oneId;
                    cmd.Parameters.Add( "@IPVC_COPTSEQUENCE", SqlDbType.VarChar ).Value = string.Empty;
                    cmd.Parameters.Add( "@IPVC_ACTIVEBIT", SqlDbType.VarChar ).Value = status == "INACTIVE" ? "0" : "1";
                    cmd.Parameters.Add( "@IPVC_ACTIVEDATE", SqlDbType.VarChar ).Value = status == "INACTIVE" ? "" : DateTime.Now.ToString("yyyy/MM/dd");
                    cmd.Parameters.Add( "@IPVC_ENDDATE", SqlDbType.VarChar ).Value = status == "INACTIVE" ? "" : "2099/12/31";
                    cmd.Parameters.Add( "@IPVC_STATUS", SqlDbType.VarChar ).Value = status;

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool IsDBExists(string dbServer, string dbName)
        {
            bool isDBExists = false;
            using( SqlConnection con = new SqlConnection( this.ConnectionStringFor( dbServer, "master" ) ) )
            {
                string qry = string.Empty;
                
                qry = String.Format( "SELECT dbid FROM master.dbo.sysdatabases WHERE( '[' + name + ']' = '{0}' OR name = '{0}' )", dbName);

                using( SqlCommand cmd = new SqlCommand( qry, con ) )
                {
                    con.Open();
                    cmd.CommandType = CommandType.Text; 

                    object resultObj = cmd.ExecuteScalar();

                    int databaseID = 0;

                    if( resultObj != null)
                    {
                        int.TryParse( resultObj.ToString(), out databaseID );
                    }
                    isDBExists = databaseID > 0 ? true : false;
                    con.Close();
                }
            }

            return isDBExists;
        }

        public bool ActivateWelcomeHome( string dbServer, string pmcId, string siteId, string userId, string requiredCenter )
        {
            DataSet ds = new DataSet();
            bool isDBExists = false;
            using( SqlConnection con = new SqlConnection( this.ConnectionStringFor( dbServer, "S"+siteId ) ) )
            {
                string qry = @"select cast (oc.oneId as int) as oneId,oc.oneCenterName,cast(ac.actrID as int) actrID from OneSiteCenters oc
                                left join ActiveCenters ac on ac.oneID = oc.oneID
                                where oneCenterName  in ('Welcome Home', 'Welcome Home Affordable', 'Welcome Home Widgets', 'Commercial')";

                using( SqlCommand cmd = new SqlCommand( qry, con ) )
                {
                    con.Open();
                    cmd.CommandType = CommandType.Text;
                    IDataAdapter da = new SqlDataAdapter(cmd);
                    
                    da.Fill( ds );
                    
                    con.Close();
                }
            }

            if(ds != null && ds.Tables != null && ds.Tables[0].Rows.Count > 0)
            {
                List<ActiveCenters> list = ConvertDataTable<ActiveCenters>( ds.Tables[0] );
                ActiveCenters commerical = list.Find( l => l.oneCenterName == "Commercial" );
                ActiveCenters whAf = list.Find( l => l.oneCenterName == "Welcome Home Affordable" );
                ActiveCenters whWg = list.Find( l => l.oneCenterName == "Welcome Home Widgets" );
                ActiveCenters wh = list.Find( l => l.oneCenterName == "Welcome Home" );

                this.EnableOrDisableCenter( commerical, dbServer, pmcId, siteId, userId, requiredCenter == "Commercial" );
                this.EnableOrDisableCenter( whAf, dbServer, pmcId, siteId, userId, requiredCenter == "Welcome Home Affordable" );
                this.EnableOrDisableCenter( whWg, dbServer, pmcId, siteId, userId, requiredCenter == "Welcome Home Widgets" );
                this.EnableOrDisableCenter( wh, dbServer, pmcId, siteId, userId, requiredCenter == "Welcome Home" );
            }

            return isDBExists;
        }

        private void EnableOrDisableCenter(ActiveCenters ac, string dbServer, string pmcId, string siteId, string userId, bool requiredCenter )
        {
            if( ac != null && ac.actrID > 0 )
            {
                if( requiredCenter )
                    this.EnableOrDisableCenter( dbServer, pmcId, siteId, userId, Convert.ToInt32( ac.actrID ), Convert.ToInt32( ac.oneId ), "ACTIVE" );
                else
                    this.EnableOrDisableCenter( dbServer, pmcId, siteId, userId, Convert.ToInt32( ac.actrID ), Convert.ToInt32( ac.oneId ), "INACTIVE" );
            }
            else if( requiredCenter )
            {
                this.EnableOrDisableCenter( dbServer, pmcId, siteId, userId, 0, Convert.ToInt32( ac.oneId ), "ACTIVE" );
            }
        }

        public void ActivatePayments3x(string dbServer, string pmcId, string siteId, string userId)
        {
            string roleList = this.GetRoles(dbServer, pmcId, siteId, userId);

            using (SqlConnection con = new SqlConnection(this.ConnectionStringFor(dbServer, "SiteMaster")))
            {
                using (SqlCommand cmd = new SqlCommand("uspUpdateSiteProfile", con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@InternalEntityID", SqlDbType.VarChar).Value = pmcId;
                    cmd.Parameters.Add("@InternalUserID", SqlDbType.VarChar).Value = userId;
                    cmd.Parameters.Add("@InternalSiteID", SqlDbType.VarChar).Value = siteId;
                    cmd.Parameters.Add("@isActivateNewUI", SqlDbType.VarChar).Value = "1";

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public int GetProviderSettingIdForMID(string dbServer, string pmcId, string siteId, string userId)
        {
            DataSet ds = new DataSet();
            int providerSettingsID = 0;
            using (SqlConnection con = new SqlConnection(this.ConnectionStringFor(dbServer, "S" + siteId)))
            {
                string qry = @"uspOSPProviderSettingsLookup";

                using (SqlCommand cmd = new SqlCommand(qry, con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@InternalEntityID", SqlDbType.VarChar).Value = pmcId;
                    cmd.Parameters.Add("@InternalUserID", SqlDbType.VarChar).Value = userId;
                    cmd.Parameters.Add("@InternalSiteID", SqlDbType.VarChar).Value = siteId;
                    cmd.Parameters.Add("@ProviderGUID", SqlDbType.VarChar).Value = "DFB76AFD-3EBD-4B58-A50A-AF139E9857F2";
                    IDataAdapter da = new SqlDataAdapter(cmd);

                    da.Fill(ds);

                    con.Close();
                }
            }

            if (ds != null && ds.Tables != null && ds.Tables[0].Rows.Count > 0)
            {
                try
                {
                    providerSettingsID = Convert.ToInt32(ds.Tables[0].AsEnumerable().First(s => s.Field<Guid>("ProviderSettingsLookupGUID").ToString().ToLower() == "D17AC1AC-69DB-40FF-B39C-2D071D8A953E".ToLower())["ProviderSettingsId"].ToString());
                }
                catch(Exception ex) { }
            }

            return providerSettingsID;
        }

        public void ActivatePaymentsSettings(string dbServer, string pmcId, string siteId, string userId)
        {
            using (SqlConnection con = new SqlConnection(this.ConnectionStringFor(dbServer, "S"+siteId)))
            {
                using (SqlCommand cmd = new SqlCommand("uspSavePropertyNumberLocationID", con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@InternalEntityID", SqlDbType.VarChar).Value = pmcId;
                    cmd.Parameters.Add("@InternalUserID", SqlDbType.VarChar).Value = userId;
                    cmd.Parameters.Add("@InternalSiteID", SqlDbType.VarChar).Value = siteId;
                    cmd.Parameters.Add("@PropertyID", SqlDbType.VarChar).Value = "1";
                    cmd.Parameters.Add("@LocationID", SqlDbType.VarChar).Value = "55805";
                    cmd.Parameters.Add("@RBPLocationID", SqlDbType.VarChar).Value = "55805";
                    cmd.Parameters.Add("@FTNISettlementID", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@FTNIDepositID", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@ResidentDirectID", SqlDbType.VarChar).Value = "1228397";
                    cmd.Parameters.Add("@IBTLocationID", SqlDbType.VarChar).Value = "55805";

                    cmd.ExecuteNonQuery();
                }
            }
            this.ActivateMID(dbServer, pmcId, siteId, userId);
        }
        public void ActivateMID(string dbServer, string pmcId, string siteId, string userId)
        {
            string roleList = this.GetRoles(dbServer, pmcId, siteId, userId);

            using (SqlConnection con = new SqlConnection(this.ConnectionStringFor(dbServer, "S" + siteId)))
            {
                using (SqlCommand cmd = new SqlCommand("uspProviderSettingsUpdate", con))
                {
                    con.Open();
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@InternalEntityID", SqlDbType.VarChar).Value = pmcId;
                    cmd.Parameters.Add("@InternalUserID", SqlDbType.VarChar).Value = userId;
                    cmd.Parameters.Add("@InternalSiteID", SqlDbType.VarChar).Value = siteId;
                    cmd.Parameters.Add("@ProviderSettingsLookupGUID", SqlDbType.VarChar).Value = "D17AC1AC-69DB-40FF-B39C-2D071D8A953E";
                    cmd.Parameters.Add("@SettingsValue", SqlDbType.VarChar).Value = "24428";
                    cmd.Parameters.Add("@ProviderSettingsID", SqlDbType.VarChar).Value = this.GetProviderSettingIdForMID(dbServer, pmcId, siteId, userId);

                    int result = cmd.ExecuteNonQuery();
                }
            }
        }

        private string ConnectionStringFor(string dbServer, string dbName )
        {
            return new SqlConnectionStringBuilder()
            {
                DataSource = dbServer,
                InitialCatalog = dbName,
                UserID = "One_1_admin",
                Password = "admin"
            }.ConnectionString;

        }

        private static List<T> ConvertDataTable<T>( DataTable dt )
        {
            List<T> data = new List<T>();
            foreach( DataRow row in dt.Rows )
            {
                T item = GetItem<T>( row );
                data.Add( item );
            }
            return data;
        }

        private static T GetItem<T>( DataRow dr )
        {
            Type temp = typeof( T );
            T obj = Activator.CreateInstance<T>();

            foreach( DataColumn column in dr.Table.Columns )
            {
                foreach( PropertyInfo pro in temp.GetProperties() )
                {
                    if( pro.Name == column.ColumnName && dr[column.ColumnName] != DBNull.Value )
                        pro.SetValue( obj, dr[column.ColumnName], null );
                    else
                        continue;
                }
            }
            return obj;
        }
    }

    public class ActiveCenters
    {
        public string oneCenterName { get; set; }
        public int actrID { get; set; }
        public int oneId { get; set; }
    }
}
