using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Management;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ADODB;
using System.Data.SqlClient;
using System.IO;
using System.Net;

public enum ProgramStartupStatus
{
    ProgramStartupStatus_INIT,
    ProgramStartupStatus_CHECK_VERSION,
    ProgramStartupStatus_CURRENT_PGMNAME_CHANGE,
    ProgramStartupStatus_DOWNLOAD_NEW_PGM,
    ProgramStartupStatus_SHUTDOWN_CURRENT_PGM,
    ProgramStartupStatus_EXECUTE_NEW_PGM,
    ProgramStartupStatus_SHUTDOWN_STARTUP_PGM,
    ProgramStartupStatus_DONE
}
namespace HanaSales_SelfCheckOut_Startup
{
    using hanas.com.codb;
    using hanas.com.colibs;
    
    public partial class HanaSales_SelfCheckOut_Startup : Form
    {
        private static cls_codb00 c_localdb;
        private static cls_codb00 c_remotedb;
        private static cls_colibs00 c_colib = new cls_colibs00();

        private static clsPOSComInfo c_poscominfo = new clsPOSComInfo();

        string g_sMessage = string.Empty;
        string g_sProcessor = System.AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "");

        ProgramStartupStatus g_iStartupStatus_Step = ProgramStartupStatus.ProgramStartupStatus_INIT;

        // Store Area 
        int GintLocation = c_poscominfo.ci_mklocation;

        // Update Check Folder
        string g_strUpdateCheckFolder = c_colib.app_path + "INI";
        string g_strServerUpdateTime = string.Empty;
        string g_strServerPgmVersion = string.Empty;

        public HanaSales_SelfCheckOut_Startup()
        {
            InitializeComponent();

            c_localdb = new cls_codb00();
            c_remotedb = new cls_codb00();

            c_localdb.subSetSystemDBInfo();
            c_remotedb.subSetRemoteDBInfo();

            cPStartupWaiting.IsRunning = true;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            g_sMessage = string.Format("[{0}] Program Update Process Start).", sMethod);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            bgUpdateWork.DoWork += new DoWorkEventHandler(bgUpdateWork_DoWork);
            bgUpdateWork.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgUpdateWork_RunWorkerCompleted);

            // Location, Market, Station 정보 읽어오기.
            GetMarketStationInfo(GetIpAddress());

            UpdateReadyTimer.Interval = 3000;
            UpdateReadyTimer.Start();
        }

        [DllImport("KERNEL32.DLL")]
        private static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        [DllImport("KERNEL32.DLL")]
        static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        // INI File Read
        private string getIni(string IpAppName, string IpKeyName, string lpDefalut, string filePath)
        {
            string inifile = filePath; //Path + File
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            try
            {
                StringBuilder result = new StringBuilder(255);
                GetPrivateProfileString(IpAppName, IpKeyName, lpDefalut, result, result.Capacity, inifile);

                return result.ToString();
            }
            catch (Exception ex)
            {
                g_sMessage = string.Format("[{0}] Failed to load INI file (line: {1})\n[{0}] {2}", sMethod, ex.Source.ToString(), ex.Message.ToString());
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                return "Failed to load INI File.";
            }
        }

        // INI File Write
        private Boolean setIni(string IpAppName, string IpKeyName, string IpValue, string filePath)
        {
            string inifile = filePath;
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            try
            {
                WritePrivateProfileString(IpAppName, IpKeyName, IpValue, inifile);

                return true;
            }
            catch (Exception ex)
            {
                g_sMessage = string.Format("[{0}] Failed to set INI file (line: {1})\n[{0}] {2}", sMethod, ex.Source.ToString(), ex.Message.ToString());
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                return false;
            }
        }

        // INI File Create
        private Boolean CreateIni(string strFileName)
        {
            string strCheckFolder = string.Empty;
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            try
            {
                //strCheckFolder = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\'));
                //strCheckFolder += "\\INI";
                if (!System.IO.Directory.Exists(g_strUpdateCheckFolder))
                {
                    System.IO.Directory.CreateDirectory(g_strUpdateCheckFolder);
                }

                strCheckFolder = g_strUpdateCheckFolder + "\\" + strFileName + ".ini";

                if (!System.IO.File.Exists(strCheckFolder))
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(strCheckFolder, true, Encoding.GetEncoding(949)))
                    {
                        sw.Write("\r\n"); sw.Flush(); sw.Close();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                g_sMessage = string.Format("[{0}] Failed to create INI file (line: {1})\n[{0}] {2}", sMethod, ex.Source.ToString(), ex.Message.ToString());
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                return false;
            }
        }

        private void GetMarketStationInfo(string strIpNumber)
        {
            string sQBuff = string.Empty;
            long lReturn = 0;
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            try
            {
                lReturn = c_localdb.DBConnection();

                if (lReturn < 0)
                {
                    g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    return;
                }

                sQBuff = "SELECT st_bhno, st_no, st_printer1, st_scale1, st_scale2, st_pinpadStationID, bh_location, bh_IPGroup, bh_DBSvr, bh_GstNo " +
                          "FROM hanamart.dbo.tb_Station left join hanamart.dbo.tb_Branch on st_bhno = bh_cd " +
                          "WHERE st_ip = '" + strIpNumber + "' ";

                lReturn = c_localdb.RsOpen(sQBuff);

                if (lReturn == 1)
                {
                    if (c_localdb.record_count == 1)
                    {
                        c_poscominfo.ci_mkno = Convert.ToString(c_localdb.rs.Fields["st_bhno"].Value);
                        c_poscominfo.si_counternum = Convert.ToString(c_localdb.rs.Fields["st_no"].Value);
                        c_poscominfo.si_scaleport = Convert.ToInt32(c_localdb.rs.Fields["st_scale1"].Value);
                        c_poscominfo.si_scaletype = Convert.ToInt32(c_localdb.rs.Fields["st_scale2"].Value);

                        //c_poscominfo.si_StationEnabled = Convert.ToInt32(c_localdb.rs.Fields["st_enabled"].Value);
                        c_poscominfo.ci_mklocation = Convert.ToInt32(c_localdb.rs.Fields["bh_location"].Value);
                        GintLocation = c_poscominfo.ci_mklocation;
                        c_poscominfo.si_StationIPGroup = Convert.ToString(c_localdb.rs.Fields["bh_IPGroup"].Value);
                        c_poscominfo.si_StationDBSvr = Convert.ToString(c_localdb.rs.Fields["bh_DBSvr"].Value);
                        c_poscominfo.si_PinpadStationID = Convert.ToString(c_localdb.rs.Fields["st_pinpadStationID"].Value);
                        c_poscominfo.si_MarketGstNum = Convert.ToString(c_localdb.rs.Fields["bh_GstNo"].Value);
                        c_poscominfo.si_sPrinter1 = Convert.ToString(c_localdb.rs.Fields["st_printer1"].Value);

                        g_sMessage = string.Format("[{0}] Maket Information OK", sMethod);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                    }
                }
                else
                {
                    g_sMessage = string.Format("[{0}] Maket Information data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    c_localdb.DBClose();

                    return;
                }
                c_localdb.RsClose();
            }
            catch (SqlException ex)
            {
                g_sMessage = string.Format("[{0}] Databasae error caused while operating (line: {1})\n[{0}] {2}", sMethod, ex.Source.ToString(), ex.Message.ToString());
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            }
            catch (Exception ex)
            {
                g_sMessage = string.Format("[{0}] Error caused while operating (line: {1})\n[{0}] {2}", sMethod, ex.Source.ToString(), ex.Message.ToString());
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            }
            finally
            {
                c_localdb.DBClose();
            }
        }

        private string GetIpAddress()
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            String strIpAddress = "";

            ObjectQuery objQuery = new ObjectQuery("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled='True'");
            ManagementObjectSearcher mobjSearcher = new ManagementObjectSearcher(objQuery);

            try
            {
                foreach (ManagementObject obj in mobjSearcher.Get())
                {
                    strIpAddress = ((String[])obj["IPAddress"])[0];
                    break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                strIpAddress = "";
            }
            g_sMessage = string.Format("[{0}] GetIpAddress : {1}).", sMethod, strIpAddress);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            return strIpAddress;
        }

        public bool CopyFile(string strOriginFile, string strCopyFile)
        {
            FileInfo fi = new FileInfo(strOriginFile);
            long lSize = 0;
            long lTotalSize = fi.Length;

            byte[] bBuf = new byte[1024];

            // 동일파일이 존재하면 삭제하고 다시 
            if (File.Exists(strCopyFile))
            {
                File.Delete(strCopyFile);
            }

            //원본 파일 열기
            FileStream fsIn = new FileStream(strOriginFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            //대상 파일 열기
            FileStream fsOut = new FileStream(strCopyFile, FileMode.Create, FileAccess.Write);

            while (lSize < lTotalSize)
            {
                try
                {
                    int iLen = fsIn.Read(bBuf, 0, bBuf.Length);
                    lSize += iLen;
                    fsOut.Write(bBuf, 0, iLen);
                }
                catch (Exception ex)
                {
                    //파일 연결 해제.
                    fsOut.Flush();
                    fsOut.Close();
                    fsIn.Close();

                    // 에러시 삭제
                    if (File.Exists(strCopyFile))
                    {
                        File.Delete(strCopyFile);
                    }
                    return false;
                }
            }
            //파일 연결 해제.
            fsOut.Flush();
            fsOut.Close();
            fsIn.Close();

            return true;
        }

        public void ProcessSelfCheckOutPGM_Startup()
        {            
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            bool bInitStatus = false;

            //Status Check 하는 로직 
            while (g_iStartupStatus_Step != ProgramStartupStatus.ProgramStartupStatus_DONE)
            {
                switch (g_iStartupStatus_Step)
                {
                    case ProgramStartupStatus.ProgramStartupStatus_INIT:
                        // Self Check Out PGM 실행 여부 확인. Flag 처리 필요.
                        Process[] arrayProgram = Process.GetProcesses();
                        for (int i = 0; i < arrayProgram.Length; i++)
                        {
                            if (arrayProgram[i].ProcessName.Equals("HanaSales_SelfCheckOut"))
                            {
                                // Auto Version UPDATE LOGIC
                                bInitStatus = false;
                            }
                            else
                            {
                                // Init PGM Startup
                                bInitStatus = true;
                            }
                        }
                        g_iStartupStatus_Step = ProgramStartupStatus.ProgramStartupStatus_CHECK_VERSION;
                        break;
                    case ProgramStartupStatus.ProgramStartupStatus_CHECK_VERSION:
                        // Server PGM Version 확인, Local에 있는 같이 확인해서 다르면 Update Logic, 같으면 실행 Flag에 따라서 프로그램 실행 여부 결정하여 스텝전환.

                        lbStartupMessage.Text = "Check New Version...";

                        string sQBuff = string.Empty;
                        long lReturn = 0;
                        string strINIPgmVersion = string.Empty;

                        // File PGM Version 과 LocalDB의 PGM Version와 비교.
                        strINIPgmVersion = this.getIni("UPDATE", "Program Version", "", g_strUpdateCheckFolder + "\\UpdateTime.ini");

                        try
                        {
                            lReturn = c_localdb.DBConnection();

                            if (lReturn < 0)
                            {
                                g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                                return;
                            }

                            // Local DB에 Time Stamp 체크
                            sQBuff = "SELECT st_pgmVersion " +
                                     "FROM hanamart.dbo.tb_SelfcheckTimestamp";

                            lReturn = c_localdb.RsOpen(sQBuff);

                            if (lReturn != 1)
                            {
                                g_sMessage = string.Format("[{0}] Data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                                c_localdb.DBClose();

                                return;
                            }
                            else
                            {
                                if (c_localdb.rs.RecordCount == 1)
                                {
                                    g_strServerPgmVersion = Convert.ToString(c_localdb.rs.Fields["st_pgmVersion"].Value);
                                }
                                else
                                {
                                    g_sMessage = string.Format("[{0}] Update File Check Failed. (error code: {1}).", sMethod, g_strServerUpdateTime);
                                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                                    c_localdb.RsClose();

                                    return;
                                }
                            }
                        }
                        catch (SqlException ex)
                        {
                            g_sMessage = string.Format("[{0}] Databasae error caused while operating (line: {1})\n[{0}] {2}", sMethod, ex.Source.ToString(), ex.Message.ToString());
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                        }
                        catch (Exception ex)
                        {
                            g_sMessage = string.Format("[{0}] Error caused while operating (line: {1})\n[{0}] {2}", sMethod, ex.Source.ToString(), ex.Message.ToString());
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                        }
                        finally
                        {
                            c_localdb.DBClose();
                        }

                        // 비교 로직 
                        if (strINIPgmVersion != g_strServerPgmVersion)
                        {
                            g_iStartupStatus_Step = ProgramStartupStatus.ProgramStartupStatus_CURRENT_PGMNAME_CHANGE;
                        }
                        else
                        {
                            g_iStartupStatus_Step = ProgramStartupStatus.ProgramStartupStatus_EXECUTE_NEW_PGM;
                        }

                        break;
                    case ProgramStartupStatus.ProgramStartupStatus_CURRENT_PGMNAME_CHANGE:
                        // 현재 프로그램 이름 오늘 업데이트 되는 날짜로 변경                      
                        string strCurPGMPath = Application.StartupPath + "\\HanaSales_SelfCheckOut.exe";
                        string strNewPGMPath = Application.StartupPath + "\\HanaSales_SelfCheckOut_" + DateTime.Now.ToString("yyyyMMdd_hmmsstt") + ".exe";

                        lbStartupMessage.Text = "Downloading New Program...";

                        if (File.Exists(strCurPGMPath) == true)
                        {
                            File.Move(strCurPGMPath, strNewPGMPath);
                        }
                        else                                // 파일 없으면 Error
                        {
                            g_sMessage = string.Format("[{0}] ProgramStartupStatus Error [{1}]).", sMethod, g_iStartupStatus_Step.ToString());
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                            g_iStartupStatus_Step = ProgramStartupStatus.ProgramStartupStatus_SHUTDOWN_STARTUP_PGM;
                            break;
                        }
                        g_iStartupStatus_Step = ProgramStartupStatus.ProgramStartupStatus_DOWNLOAD_NEW_PGM;
                        break;
                    case ProgramStartupStatus.ProgramStartupStatus_DOWNLOAD_NEW_PGM:
                        // 서버에서 신규 프로그램 다운로드함.
                        string strServerFolder = string.Empty;
                        string strLocalFolder = string.Empty;
                        bool bCopyFolderStatusDone = false;

                        // Market GB 정보에 있는 서버로 접속하기.
                        strServerFolder = @"\\" + c_poscominfo.si_StationIPGroup + c_poscominfo.si_StationDBSvr + "\\hanaro\\SelfCheckOut_Station\\HanaSales_SelfCheckOut.exe";            //
                        strLocalFolder = Application.StartupPath + "\\HanaSales_SelfCheckOut.exe";          //Local Folder Path

                        g_sMessage = string.Format("[{0}] Server Folder : {1}", sMethod, strServerFolder);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                        g_sMessage = string.Format("[{0}] Local Folder : {1}", sMethod, strLocalFolder);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                        if (File.Exists(strServerFolder) == true)         // 서버 경로에 파일이 있으면 Local로 Copy함.
                        {
                            // 파일 Copy
                            bCopyFolderStatusDone = CopyFile(strServerFolder, strLocalFolder);                  // 폴더 카피.

                            if (bCopyFolderStatusDone == false)
                            {
                                // Error Message
                                g_sMessage = string.Format("[{0}] PGM DOWNLOAD ERROR : {1} -> {2}", sMethod, strServerFolder, strLocalFolder);
                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                                g_iStartupStatus_Step = ProgramStartupStatus.ProgramStartupStatus_SHUTDOWN_STARTUP_PGM;
                                break;
                            }

                            // INI 파일에 PGM Version을 업데이트 한다.
                            this.setIni("UPDATE", "Program Version", g_strServerPgmVersion, g_strUpdateCheckFolder + "\\UpdateTime.ini");
                            lbStartupMessage.Text = "Start Self Check Out Program..";
                        }
                        else                                                                            // 파일 없으면 Error
                        {
                            g_sMessage = string.Format("[{0}] ProgramStartupStatus Error [{1}]).", sMethod, g_iStartupStatus_Step.ToString());
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                            g_iStartupStatus_Step = ProgramStartupStatus.ProgramStartupStatus_SHUTDOWN_STARTUP_PGM;
                            break;
                        }
                        
                        g_iStartupStatus_Step = ProgramStartupStatus.ProgramStartupStatus_SHUTDOWN_CURRENT_PGM;
                        break;
                    case ProgramStartupStatus.ProgramStartupStatus_SHUTDOWN_CURRENT_PGM:
                        // 기존 실행중인 프로그램 종료.
                        
                        Process[] CurExecuteProgram = Process.GetProcesses();
                        for (int i = 0; i < CurExecuteProgram.Length; i++)
                        {
                            if (CurExecuteProgram[i].ProcessName.Equals("HanaSales_SelfCheckOut"))
                            {
                                CurExecuteProgram[i].Kill();
                            }
                        }
                        
                        g_iStartupStatus_Step = ProgramStartupStatus.ProgramStartupStatus_EXECUTE_NEW_PGM;
                        break;
                    case ProgramStartupStatus.ProgramStartupStatus_EXECUTE_NEW_PGM:
                        // 신규 프로그램 실행.                        
                        Process.Start(Application.StartupPath + "\\HanaSales_SelfCheckOut.exe");
                        
                        g_iStartupStatus_Step = ProgramStartupStatus.ProgramStartupStatus_SHUTDOWN_STARTUP_PGM;
                        break;
                    case ProgramStartupStatus.ProgramStartupStatus_SHUTDOWN_STARTUP_PGM:
                        // Startup PGM 종료.
                        Process[] CurStartupProgram = Process.GetProcesses();
                        for (int i = 0; i < CurStartupProgram.Length; i++)
                        {
                            if (CurStartupProgram[i].ProcessName.Equals("HanaSales_SelfCheckOut_Startup") || CurStartupProgram[i].ProcessName.Equals("vshost32.exe"))                                
                            {
                                CurStartupProgram[i].Kill();
                            }
                        }
                        g_iStartupStatus_Step = ProgramStartupStatus.ProgramStartupStatus_DONE;
                        break;                   
                    default:
                        g_iStartupStatus_Step = ProgramStartupStatus.ProgramStartupStatus_DONE;
                        break;
                }
            }
            bgUpdateWork.CancelAsync();
        }

        private void UpdateReadyTimer_Tick(object sender, EventArgs e)
        {
            UpdateReadyTimer.Stop();

            bgUpdateWork.RunWorkerAsync();            
        }

        private void bgUpdateWork_DoWork(object sender, DoWorkEventArgs e)
        {
            ProcessSelfCheckOutPGM_Startup();
        }

        private void bgUpdateWork_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        //public enum ProgramStartupStatus
        //{
        //    ProgramStartupStatus_INIT,
        //    ProgramStartupStatus_CHECK_VERSION,
        //    ProgramStartupStatus_CURRENT_PGMNAME_CHANGE,
        //    ProgramStartupStatus_DOWNLOAD_NEW_PGM,
        //    ProgramStartupStatus_SHUTDOWN_CURRENT_PGM,
        //    ProgramStartupStatus_EXECUTE_NEW_PGM,
        //    ProgramStartupStatus_SHUTDOWN_STARTUP_PGM,
        //    ProgramStartupStatus_DONE
        //}
    }
}
