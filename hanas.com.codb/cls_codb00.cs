namespace hanas.com.codb
{
    using System;
    using System.Configuration;

    using System.Data;
    using System.Data.SqlClient;
    using ADODB;

    using hanas.com.isecure;

    public class cls_codb00
    {
        private static cls_isecure00 _isSecure = new cls_isecure00();
        private static string c_secure_key = string.Empty;

        private string sConfigPath;
        private Configuration cfConfigManager = null;

        private object c_affected_records;
        private long c_record_count;
        private string c_error_message;


        private SqlConnection conDatabase;
        private DataSet dsDataSet;
        private DataTable dtDataTable;
        private SqlDataAdapter daSqlDataAdapter;
        private SqlCommand cmdSqlCommand;

        private DBInformation c_dbinformation;

        private string c_dbparams_gd = String.Empty;
        private string c_dbparams_rs = String.Empty;

        //protected cls_covar00 c_covar;

        private ADODB.Connection c_dbcon;
        private ADODB.Recordset c_rs;

        public cls_codb00()
        {
            sConfigPath = this.GetType().Assembly.Location;
            cfConfigManager = ConfigurationManager.OpenExeConfiguration(sConfigPath);

            c_secure_key = String.IsNullOrEmpty(_isSecure.GetAppSetting(cfConfigManager, "DBSecurityKey")) ? "" : _isSecure.GetAppSetting(cfConfigManager, "DBSecurityKey");
            c_secure_key = _isSecure.tDecrypt(c_secure_key);

            c_dbcon = new ADODB.Connection();
            c_rs = new ADODB.Recordset();
        }

        public cls_codb00(string v_params)
        {
            c_dbcon = new ADODB.Connection();
            c_rs = new ADODB.Recordset();

            c_dbparams_rs = v_params;
        }

        ~cls_codb00()
        {
            c_dbcon = null;
            c_rs = null;
        }

        public string db_host
        {
            get { return this.c_dbinformation.db_host; }
            set { this.c_dbinformation.db_host = value; }
        }

        public string db_port
        {
            set { this.c_dbinformation.db_port = value; }
        }

        public string db_name
        {
            set { this.c_dbinformation.db_name = value; }
        }

        public string db_userid
        {
            set { this.c_dbinformation.db_userid = value; }
        }

        public string db_password
        {
            set { this.c_dbinformation.db_password = value; }
        }

        public string db_params
        {
            get { return this.c_dbparams_rs; }
            set { this.c_dbparams_rs = value; }
        }

        public ADODB.Recordset rs
        {
            get { return this.c_rs; }
        }

        public long record_count
        {
            get { return this.c_record_count; }
        }

        public long affectedrecords
        {
            get { return Convert.ToInt32(this.c_affected_records); }
        }

        public string error_message
        {
            get { return this.c_error_message; }
        }

        public void subSetSystemDBInfo()
        {
            this.c_dbinformation.db_host = String.IsNullOrEmpty(_isSecure.GetAppSetting(cfConfigManager, "sysdbhost")) ? "" : _isSecure.tDecrypt(_isSecure.GetAppSetting(cfConfigManager, "sysdbhost"));
            this.c_dbinformation.db_port = String.IsNullOrEmpty(_isSecure.GetAppSetting(cfConfigManager, "sysdbport")) ? "" : _isSecure.tDecrypt(_isSecure.GetAppSetting(cfConfigManager, "sysdbport"));
            this.c_dbinformation.db_name = String.IsNullOrEmpty(_isSecure.GetAppSetting(cfConfigManager, "sysdbname")) ? "" : _isSecure.tDecrypt(_isSecure.GetAppSetting(cfConfigManager, "sysdbname"));
            this.c_dbinformation.db_userid = String.IsNullOrEmpty(_isSecure.GetAppSetting(cfConfigManager, "sysdbuserid")) ? "" : _isSecure.tDecrypt(_isSecure.GetAppSetting(cfConfigManager, "sysdbuserid"));
            this.c_dbinformation.db_password = String.IsNullOrEmpty(_isSecure.GetAppSetting(cfConfigManager, "sysdbpassword")) ? "" : _isSecure.tDecrypt(_isSecure.GetAppSetting(cfConfigManager, "sysdbpassword"));

            this.subGenDBConnectionStrings();
        }

        public void subSetRemoteDBInfo()
        {
            this.c_dbinformation.db_host = String.IsNullOrEmpty(_isSecure.GetAppSetting(cfConfigManager, "remotedbhost")) ? "" : _isSecure.tDecrypt(_isSecure.GetAppSetting(cfConfigManager, "remotedbhost"));
            this.c_dbinformation.db_port = String.IsNullOrEmpty(_isSecure.GetAppSetting(cfConfigManager, "remotedbport")) ? "" : _isSecure.tDecrypt(_isSecure.GetAppSetting(cfConfigManager, "remotedbport"));
            this.c_dbinformation.db_name = String.IsNullOrEmpty(_isSecure.GetAppSetting(cfConfigManager, "remotedbname")) ? "" : _isSecure.tDecrypt(_isSecure.GetAppSetting(cfConfigManager, "remotedbname"));
            this.c_dbinformation.db_userid = String.IsNullOrEmpty(_isSecure.GetAppSetting(cfConfigManager, "remotedbuserid")) ? "" : _isSecure.tDecrypt(_isSecure.GetAppSetting(cfConfigManager, "remotedbuserid"));
            this.c_dbinformation.db_password = String.IsNullOrEmpty(_isSecure.GetAppSetting(cfConfigManager, "remotedbpassword")) ? "" : _isSecure.tDecrypt(_isSecure.GetAppSetting(cfConfigManager, "remotedbpassword"));

            this.subGenDBConnectionStrings();
        }

        public void subGenDBConnectionStrings()
        {
            this.c_dbparams_gd = @"uid=" + this.c_dbinformation.db_userid + ";password=" + this.c_dbinformation.db_password + ";database=" + this.c_dbinformation.db_name + ";Data Source=" + this.c_dbinformation.db_host + "," + this.c_dbinformation.db_port + ";";
            this.c_dbparams_rs = "Provider=SQLOLEdb.1;uid=" + this.c_dbinformation.db_userid + ";password=" + this.c_dbinformation.db_password + ";database=" + this.c_dbinformation.db_name + ";Data Source=" + this.c_dbinformation.db_host + "," + this.c_dbinformation.db_port + ";";
        }

        public long DBConnection()
        {
            string sErrorMessage = string.Empty;
            long lErrorCode = 0;

            try
            {
                if (this.c_dbcon != null && (this.c_dbcon.State == Convert.ToInt16(ConnectionState.Closed) || this.c_dbcon.State == Convert.ToInt16(ConnectionState.Broken)))
                {
                    this.c_dbcon.Open(c_dbparams_rs, "", "", -1);
                }

                lErrorCode = this.c_dbcon.State;
            }
            catch (SqlException ex)
            {
                sErrorMessage = ex.Message.ToString();
                lErrorCode = ex.ErrorCode > 0 ? ex.ErrorCode * (-1) : ex.ErrorCode;
            }
            catch (Exception e)
            {
                sErrorMessage = e.Source.ToString() + ", " + e.Message.ToString();
                lErrorCode = -999;
            }
            finally
            {
                this.c_error_message = sErrorMessage;
            }

            return lErrorCode;
        }

        public void DBClose()
        {
            if (this.c_dbcon != null && this.c_dbcon.State == 1)
            {
                this.c_dbcon.Close();
            }
        }

        public long DBConnection(ref Connection v_dbcon)
        {
            string sErrorMessage = string.Empty;
            long lErrorCode = 0;

            try
            {
                if (v_dbcon != null && (v_dbcon.State == Convert.ToInt16(ConnectionState.Closed) || v_dbcon.State == Convert.ToInt16(ConnectionState.Broken)))
                {
                    v_dbcon.Open(c_dbparams_rs, "", "", -1);
                }

                lErrorCode = v_dbcon.State;
            }
            catch (SqlException ex)
            {
                sErrorMessage = ex.Message.ToString();
                lErrorCode = ex.ErrorCode > 0 ? ex.ErrorCode * (-1) : ex.ErrorCode;
            }
            catch (Exception e)
            {
                sErrorMessage = e.Source.ToString() + ", " + e.Message.ToString();
                lErrorCode = -999;
            }
            finally
            {
                this.c_error_message = sErrorMessage;
            }

            return lErrorCode;
        }

        public void DBClose(ref Connection v_dbcon)
        {
            if (v_dbcon != null && v_dbcon.State == 1)
            {
                v_dbcon.Close();
            }
        }

        public long RsOpen(string v_qbuff)
        {
            string sErrorMessage = string.Empty;
            long lErrorCode = 0;

            if (this.c_dbcon == null)
            {
                return (-999);
            }
            else
            {
                if (this.c_dbcon.State != 1) return (-999);
            }

            if (this.c_rs.State == 1)
            {
                this.c_rs.Close();
            }

            try
            {
                this.c_rs.CursorLocation = ADODB.CursorLocationEnum.adUseClient;
                this.c_rs.Open(v_qbuff, this.c_dbcon, ADODB.CursorTypeEnum.adOpenStatic, ADODB.LockTypeEnum.adLockOptimistic, 1);

                if (this.c_rs.State == 1)
                {
                    this.c_record_count = this.c_rs.RecordCount;
                    lErrorCode = 1;
                }
                else
                {
                    lErrorCode = -999;
                }
            }
            catch (SqlException ex)
            {
                sErrorMessage = ex.Message.ToString();
                lErrorCode = ex.ErrorCode > 0 ? ex.ErrorCode * (-1) : ex.ErrorCode;
            }
            catch (Exception e)
            {
                sErrorMessage = e.Source.ToString() + ", " + e.Message.ToString();
                lErrorCode = -999;
            }
            finally
            {
                this.c_error_message = sErrorMessage;
            }

            return lErrorCode;
        }

        public void RsClose()
        {
            if (this.c_rs != null && this.c_rs.State == 1)
            {
                this.c_rs.Close();
            }
        }

        public long RsOpen(ref Recordset v_rs, string v_qbuff)
        {
            string sErrorMessage = string.Empty;
            long lErrorCode = 0;

            if (this.c_dbcon == null)
            {
                return (-999);
            }
            else
            {
                if (this.c_dbcon.State != 1) return (-999);
            }

            try
            {
                v_rs.CursorLocation = ADODB.CursorLocationEnum.adUseClient;
                v_rs.Open(v_qbuff, this.c_dbcon, ADODB.CursorTypeEnum.adOpenKeyset, ADODB.LockTypeEnum.adLockOptimistic, -1);

                if (v_rs.State == 1)
                {
                    this.c_record_count = v_rs.RecordCount;
                    lErrorCode = 1;
                }
                else
                {
                    return (-999);
                }
            }
            catch (SqlException ex)
            {
                sErrorMessage = ex.Message.ToString();
                lErrorCode = ex.ErrorCode > 0 ? ex.ErrorCode * (-1) : ex.ErrorCode;
            }
            catch (Exception e)
            {
                sErrorMessage = e.Source.ToString() + ", " + e.Message.ToString();
                lErrorCode = -999;
            }
            finally
            {
                this.c_error_message = sErrorMessage;
            }

            return lErrorCode;
        }

        public long RsOpen(ref Connection v_dbcon, ref Recordset v_rs, string v_qbuff)
        {
            string sErrorMessage = string.Empty;
            long lErrorCode = 0;

            if (v_dbcon == null)
            {
                return (-999);
            }
            else
            {
                if (v_dbcon.State != 1) return (-999);
            }

            try
            {
                v_rs.CursorLocation = ADODB.CursorLocationEnum.adUseClient;
                v_rs.Open(v_qbuff, v_dbcon, ADODB.CursorTypeEnum.adOpenKeyset, ADODB.LockTypeEnum.adLockOptimistic, -1);

                if (v_rs.State == 1)
                {
                    this.c_record_count = v_rs.RecordCount;
                    lErrorCode = 1;
                }
                else
                {
                    return (-999);
                }
            }
            catch (SqlException ex)
            {
                sErrorMessage = ex.Message.ToString();
                lErrorCode = ex.ErrorCode > 0 ? ex.ErrorCode * (-1) : ex.ErrorCode;
            }
            catch (Exception e)
            {
                sErrorMessage = e.Source.ToString() + ", " + e.Message.ToString();
                lErrorCode = -999;
            }
            finally
            {
                this.c_error_message = sErrorMessage;
            }

            return lErrorCode;
        }

        public void RsClose(ref ADODB.Recordset v_rs)
        {
            if (v_rs != null && v_rs.State == 1)
            {
                v_rs.Close();
            }

            //v_rs = null;
        }

        public int CallStoredProcedure(string v_qbuff, ref string v_result)
        {
            SqlConnection oSqlConn = new SqlConnection(c_dbparams_gd);

            SqlCommand oSqlCommand = new SqlCommand(v_qbuff, oSqlConn);
            oSqlCommand.CommandType = CommandType.StoredProcedure;

            SqlParameter oSqlParameter = new SqlParameter("@vLotNo", SqlDbType.VarChar, 10);

            try
            {
                oSqlParameter.Direction = ParameterDirection.Output;
                oSqlCommand.Parameters.Add(oSqlParameter);

                oSqlConn.Open();
                oSqlCommand.ExecuteNonQuery();

                v_result = oSqlParameter.Value.ToString();

                oSqlConn.Close();

                oSqlParameter = null;
                oSqlCommand = null;
                oSqlConn = null;
            }
            catch (SqlException ex)
            {
                v_result = ex.Message.ToString();
                return -1;
            }
            catch (Exception e)
            {
                v_result = e.Message.ToString();
                return -1;
            }

            return 1;
        }

        public long DBExcute(string v_qbuff)
        {
            string sErrorMessage = string.Empty;
            long lErrorCode = 1;

            c_affected_records = 0;

            try
            {
                this.c_dbcon.Execute(v_qbuff, out c_affected_records, 0);
            }
            catch (SqlException ex)
            {
                sErrorMessage = ex.Message.ToString();
                lErrorCode = ex.ErrorCode > 0 ? ex.ErrorCode * (-1) : ex.ErrorCode;
            }
            catch (Exception e)
            {
                sErrorMessage = e.Source.ToString() + ", " + e.Message.ToString();
                lErrorCode = -999;
            }
            finally
            {
                this.c_error_message = sErrorMessage;
            }

            return lErrorCode;
        }

        public long DBExcute(ref Connection v_dbcon, string v_qbuff)
        {
            string sErrorMessage = string.Empty;
            long lErrorCode = 1;

            c_affected_records = 0;

            if (v_dbcon == null)
            {
                return (-999);
            }
            else
            {
                if (v_dbcon.State != 1) return (-999);
            }

            try
            {
                v_dbcon.Execute(v_qbuff, out c_affected_records, 0);
            }
            catch (SqlException ex)
            {
                sErrorMessage = ex.Message.ToString();
                lErrorCode = ex.ErrorCode > 0 ? ex.ErrorCode * (-1) : ex.ErrorCode;
            }
            catch (Exception e)
            {
                sErrorMessage = e.Source.ToString() + ", " + e.Message.ToString();
                lErrorCode = -999;
            }
            finally
            {
                this.c_error_message = sErrorMessage;
            }

            return lErrorCode;
        }

        public void DBBeginTrans()
        {
            this.c_dbcon.BeginTrans();
        }

        public void DBBeginTrans(ref Connection v_dbcon)
        {
            v_dbcon.BeginTrans();
        }

        public void DBCommitTrans()
        {
            this.c_dbcon.CommitTrans();
        }

        public void DBCommitTrans(ref Connection v_dbcon)
        {
            v_dbcon.CommitTrans();
        }

        public void DBRollbackTrans()
        {
            this.c_dbcon.RollbackTrans();
        }

        public void DBRollbackTrans(ref Connection v_dbcon)
        {
            v_dbcon.RollbackTrans();
        }

        public DataSet GetDataSet(string vQBuff)
        {
            this.conDatabase = new SqlConnection(c_dbparams_gd);
            this.dsDataSet = new DataSet();
            this.cmdSqlCommand = new SqlCommand();

            this.cmdSqlCommand.CommandText = vQBuff;
            this.cmdSqlCommand.Connection = this.conDatabase;
            this.conDatabase.Open();

            this.daSqlDataAdapter = new SqlDataAdapter();

            this.daSqlDataAdapter.SelectCommand = this.cmdSqlCommand;
            this.daSqlDataAdapter.Fill(this.dsDataSet, "GetDataGrid");

            this.cmdSqlCommand = null;
            this.conDatabase = null;

            return this.dsDataSet;
        }

        public DataTable GetDataTable(string vQBuff)
        {
            this.conDatabase = new SqlConnection(c_dbparams_gd);
            this.dtDataTable = new DataTable();

            this.daSqlDataAdapter = new SqlDataAdapter(vQBuff, this.conDatabase);

            try
            {
                this.conDatabase.Open();

                this.daSqlDataAdapter.Fill(this.dtDataTable);
                this.conDatabase.Close();
                return this.dtDataTable;
            }
            catch
            {
                return null;
            }
        }
    }
}
