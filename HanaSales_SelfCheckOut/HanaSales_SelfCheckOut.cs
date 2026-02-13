using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Transitions;
using HanaMiraXLib;
using ADODB;
using System.Net;
using System.IO;
using System.Data.SqlClient;
using ThoughtWorks;
using ThoughtWorks.QRCode;
using ThoughtWorks.QRCode.Codec;
using System.Speech.Synthesis;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Management;
using System.Media;
//using OposScale_1_8_Lib;
//using OposScanner_1_8_Lib;
using OposScale_CCO;
using OposScanner_CCO;
using Custom.CuCustomWndAPI;

namespace HanaSales_SelfCheckOut
{
    using hanas.com.codb;  
    using hanas.com.colibs;
    using System.Drawing.Imaging;

    public partial class HanaSales_SelfCheckOut : Form
    {
        /************************* Global Common Libs Instance 변수 선언 ********************************************/
        private static cls_codb00 c_localdb;
        private static cls_codb00 c_localdb2;
        private static cls_codb00 c_remotedb;
        private static cls_colibs00 c_colib = new cls_colibs00();

        private static commonClass c_poscomlibs = new commonClass();
        private static clsPOSComInfo c_poscominfo = new clsPOSComInfo();

        string g_sProcessor = System.AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "");
        string g_sMessage = string.Empty;
        /************************* Global Common Libs Instance 변수 선언 ********************************************/

        /************************* Global Object Declaration Start ********************************************/
        /*** Global Object Declaration Start ****/
        private static OPOSScannerClass OPOSScanner = new OPOSScannerClass();
        private static OPOSScaleClass OPOSScale = new OPOSScaleClass();

        private string s_scale_devicename = "SCRS232Scale";
        private string s_scanner_devicename = "SCRS232Scanner";
        /************************* Global Object Declaration End ********************************************/

        /************************* 사용 변수 선언 ********************************************/
        System.Windows.Forms.Timer objTimer = new System.Windows.Forms.Timer();

        bool g_bTouchUse = true;
        bool g_bCustomerMonitor = true;
        string g_strCounterNum = "";

        // 핀패드 사용 여부
        bool g_bPinpadUse = true;

        // QLight 사용 여부
        bool g_bQLightUse = true;

        bool g_HelpModeReady = false;
        bool g_HelpModeOn    = false;
        bool g_ReprintModeOn = false;
        bool g_ItemDiscountModeOn = false;

        bool g_ManagerCardScanReady = false;
        bool g_ManagerCardScanOn = false;

        // AGE CHECK 관련 
        bool g_CertifiedAdultReady = false;
        bool g_CertifiedAdult = false;
        int  g_iAdultLimit = 0;
        int  g_iStoredAdultLimit = 0;

        string g_strAgeCheckforProdID = string.Empty;
        
        bool GPinPadReady = false;
        bool GPayFinish = false;
        int GintAuthSeq = 1;
        public double[] CardPay = new double[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public const int iVisa = 0;
        public const int iMaster = 1;
        public const int iAmex = 2;
        public const int iDisc = 3;
        public const int iUP = 4;
        public const int iCardETC = 5;
        public const int iUnion = 6;
        public const int iEBT = 7;
        public const int iGiftCard = 8;
        public const int iDiners = 9;

        public double[] AliPay = new double[2] { 0, 0 };
        public const int iAli = 0;
        public const int iWechat = 1;

        string GstrPayType = "";
        string GstrStoreMerchantID = "";
        string GstrStoreMerchantKey = "";
        string GstrStorejobNo = "";
        string GstrStoreLoginname = "hmart01";
        string GstrStorePassword = "o8gcZZsXcPI=";

        // 테스트 모드
        bool GblTestMode = false;

        // 프린트 영수증 번호
        string GstrPrtInvno = "";
        string GstrReprint = "";
        string[] imgCouponFilesPath;
        // 쿠폰 영수증 뒤 출력용
        bool bUseCoupon = false;
        void setUseCoupon(bool useCoupon)
        {
            bUseCoupon = useCoupon;
        }

        // Store Area 
        int GintLocation = c_poscominfo.ci_mklocation;
        
        // Market Info
        //string GstrMkno = "61";
        //string GstrStno = "99";


        // 멤버 관련 변수
        bool GchkMember = false;
        bool GblMemberEarn = false;
        bool GblMemberHPuse = false;
        string GstrTotalDCType = "";
        double GdblTotalDCRate = 0;
        double GdblItemDCRate = 0;
        string GcID, GcName, GcFirst, GcStore, GcCustno, GcTelNo, GcEmail, GcHPBal, GcHPEarn, GcHPUsed, GcOrigin, GcBal, GcNPH, GcASLA, GcAPNo, GcTPNo, GcVoid;
        double GdblHPTodayUsed = 0;
        double GdblHPBal = 0;
        double GdblHPEarn = 0;
        double GdblHPUsed = 0;

        // EBT Calculation
        double GdblEBTAmountTotal = 0;          // EBT 대상 상품 합산 금액
        double GdblEBTTax1Total = 0;            // EBT 대상 상품 Tax1 합산 금액
        double GdblEBTTax2Total = 0;            // EBT 대상 상품 Tax2 합산 금액
        double GdblEBTTax3Total = 0;            // EBT 대상 상품 Tax3 합산 금액
        double GdblEBTTaxExemptAmountTotal = 0; // Tax Exemption 적용 받는 Total Amount 금액
        double GdblEBTTaxExemptTotal = 0;       // Tax Exemption 금액

        double GdblFoodStampAmt = 0;
        double GdblFoodStampTax = 0;
        double GdblFoodStampTaxableItemAmt = 0;

        bool GblEBTReusableBag = false;
        bool GblEBTTaxExamption = false;

        // Transaction Complete
        bool GblComplete = false;

        // Update Check Folder
        string g_strUpdateCheckFolder = c_colib.app_path + "INI";           // Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\'));
        string g_strServerUpdateTime = string.Empty;
        string g_strServerPgmVersion = string.Empty;
        // Update Image 관련
        int g_iUpdateInterval = 7000;                    // 10초 대기 후 업데이트 체크.

        //Back To Start Inverval
        //int g_iBackToStartIntervalFromItemScan = 5000;        // 300초(5분) 대기 상태 후 Start 화면으로 전환
        int g_iBackToStartIntervalFromItemScan = 300000;        // 300초(5분) 대기 상태 후 Start 화면으로 전환 TEST
        int g_iBackToStartInterval = 60000;                     // 60초(1분) 대기 상태 후 Start 화면으로 전환.
        int g_iBackToStartExtensionInterval = 30;           // 60초(1분) 대기 상태 후 Start 화면으로 전환.
        int g_iBackToStartExtensionCurrent = 0;           //.
                                                               //int g_iBackToStartInterval = 10000;                     // 60초(1분) 대기 상태 후 Start 화면으로 전환. TEST
                                                               // Beep Sound Inverval
        int g_iProcessBeepSoundTimer = 4000;

        // Beep Sound Break
        bool g_bBreakBeepSound = false;

        // Printer Status Check Timer Interval
        bool g_bPrinterStatusInit   = false;
        int g_iPrinterStatusInterval = 2000;                    // 3초 대기 후 업데이트 체크.

        // Voice
        string GstrVoice = "Microsoft Zira Desktop";

        SpeechSynthesizer GssWelcome = new SpeechSynthesizer();
        SpeechSynthesizer GssMembershipScan = new SpeechSynthesizer();
        SpeechSynthesizer GssScanyourItem = new SpeechSynthesizer();
        SpeechSynthesizer GssSearchItem = new SpeechSynthesizer();
        SpeechSynthesizer GssPointUse = new SpeechSynthesizer();
        SpeechSynthesizer GssThankyou = new SpeechSynthesizer();
        SpeechSynthesizer GssHelp = new SpeechSynthesizer();
        SpeechSynthesizer GssAddBag = new SpeechSynthesizer();
        SpeechSynthesizer GssSelectPayment = new SpeechSynthesizer();
        SpeechSynthesizer GssMessageBox = new SpeechSynthesizer();
        SpeechSynthesizer GssHowManyUseBag = new SpeechSynthesizer();

        // Image List view 관련
        
        bool g_SearchCategory = false;
        bool g_SearchCategory_Clicked = false;
        bool g_SearchKeyInputPLU_Number = false;
        int  g_SearchItemPageNum = 0;
        int  g_SearchItemMaxPageNum = 0;
        bool g_SearchAdditional = false;
        string g_Temp_lvItem = string.Empty;

        // Manager Key
        string g_strManagerKey = "83800030";

        // Promotion 처리관련
        int g_iRegPriceItemCount = 0;
        int g_iPromoPriceItemCount = 0;

        // Mix & Match 관련
        double GMixMatchPrice = 0;
        double GMixMatchGST = 0;
        double GMixMatchPST = 0;
        double GMixMatchHST = 0;

        // Custom Printer Status API
        CuCustomWndAPIWrap cutomWndAPIWrap = new CuCustomWndAPIWrap();
        CuCustomWndDevice CustomReceiptPrt = null;

        // Qlight 관련 
        const byte g_Lampoff = 0;
        const byte g_Lampon = 1;
        const byte g_Lampblink = 2;
        const byte g_D_not = 100;  //  // Don't care  // Do not change before state

        // Station 관련
        bool g_StationOpen = false;

        /************************* Global Common Libs Instance 변수 선언 ********************************************/
        public struct OrderItem
        {
            public string tInvNo;
            public string tID;
            public string tProd;
            public string tProdName;
            public string tProdKname;
            public string tPtype;
            public string tPtype2;
            public string tCat1;
            public string tCat2;
            public string tCat3;
            public string tCat4;
            public string tCat5;
            public string tQty;
            public string tIUprice;
            public string tOUprice;
            public string tAmt;
            public string tGst;
            public string tPst;
            public string tHst;
            public string tTax;
            public string tType;
            public string tNative;
            public string tPromo;
            public string tPromoCode;
            public string tCust;
            public string tPassWord;
            public string tPassStation;
            public string tUpCode;
            public string tSpecial;
            public string tFree;
            public string tMMBC;
            public string tSupp;
            public string tEntryCode;
            public string tFoodStamp;
            public string tGiftCardRef;
            public string tRelatedID;
            public string tMixMatch;
            public string tShift;
            public string tMemo;

            // 컬럼 모두 추가
        }
        
        private const int GROUP_BOX_LEFT = 1023;
        Control ctrlOnScreen, ctrlOffScreen, ctrlOnSubScreen, ctrlOnSubScreen2, ctrTemp, ctrTemp2;
        //private static object Properties;

        public enum ImageType
        {
            Category,
            Item,
            Additional_Data,
        }

        public enum ManualETCKeyIN_DEP
        {
            GROCERY,
            PRODUCE,
            FISH,
            MEAT,
            HW,
            DELI,
            ETC,
        }

        public string[] str_mKeyinVale = new string[6] { "GROCERY", "PRODUCE", "FISH", "MEAT", "H/W", "DELI" };
        ManualETCKeyIN_DEP g_mKeyinVale = 0;

        bool g_ManualKeyInItem = false;
        double g_ManualKeyInItemPrice = 0;

        public enum ManualMasterFunction
        {
            ItemCorrect,
            ItemBtnVoid,
            ItemBackVoid,
            ItemSuspend,
            ItemReprint,
            ItemETCKeyIN,            
            ItemETC,            
        }
        ManualMasterFunction g_mMaterFunctionVal = ManualMasterFunction.ItemETC;

        public HanaSales_SelfCheckOut()
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            g_sMessage = string.Format("[{0}] Start Process).", sMethod);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            c_localdb = new cls_codb00();
            c_localdb2 = new cls_codb00();
            c_remotedb = new cls_codb00();

            c_localdb.subSetSystemDBInfo();
            c_localdb2.subSetSystemDBInfo();
            c_remotedb.subSetRemoteDBInfo();

            InitializeComponent();

            if(GintLocation == 1)               // 벤쿠버 일때 // 임시
            {
                g_strManagerKey = "83800030";
            }
            else                                // 토론토, 미국일때
            {
                g_strManagerKey = "83800030";
            }

            // Location, Market, Station 정보 읽어오기.
            GetMarketStationInfo(GetIpAddress());

            if (g_bPinpadUse)
            {
                chkPinpadInit();
            }
            //GPinPadReady = true;

            initListView();
            ItemCSView.Enabled = false;
            //ItemCSView.Enabled = true;

            initSalesTotal();
            initPayment();
            initScale();

            InitMemberDisplay();
            initDClabel();

            // 미국용 화면 표시
            initUSADisplay();

            // EBT
            initEBTTotal();

            // 추후 Company, Station 정보 가져오는 로직 추가 예정
            // 임시 캐나다 테스트용----------------------
            //c_poscominfo.ci_mkno = "61";
            //c_poscominfo.ci_mklocation = 1;
            //GintLocation = c_poscominfo.ci_mklocation;

            ////임시 미국 테스트용---------------------- -
            //c_poscominfo.ci_mklocation = 1;
            //c_poscominfo.ci_mkno = "52";
            //GintLocation = 3;
            //// ----------------------------------------
            //c_poscominfo.si_counternum = "99";
            //c_poscominfo.si_touchuse = g_bTouchUse;
            //c_poscominfo.si_customermonitor = g_bCustomerMonitor;

            //c_poscominfo.si_scaletype = 9;
            //c_poscominfo.si_scaleport = 1;                      // OPOS의 경우, OPOS 드라이버 셋팅에서 지정 됨.
            // ------------------------------------------------

            SetGlobalInfo();
            
            GetMembershipPrefix();
            GetVIPMembership();
            GetNewInvNo();

            // OrderItem에 Item이 남아있을 경우 불러옴
            PopulateListViewProdItem("MainForm");

            st_ProcessStatus.Visible = false;
            tbStep1Name.Visible = false;
            tbStep2Name.Visible = false;
            tbStep3Name.Visible = false;
            tbStep4Name.Visible = false;

            //txtSearchCode.TextAlignChanged += txtSearchCode_TextChanged;

            if(GintLocation == 1)       // 벤쿠버 
            {
                //// 저울에서 스캔 되는거 방지.
                //if (OPOSScanner.DeviceEnabled)
                //{
                //    OPOSScanner.DataEvent -= ScannerDataEvent;
                //    OPOSScanner.DeviceEnabled = false;
                //}
                if (c_poscominfo.si_scaletype != 0)
                {
                    // Scale/Scanner Initialization 추가.
                    // 현재는 ScaleType 9 (OPOS 설정)의 경우만 구현
                    if (EnableOPOSDevices() < 0)
                        c_poscominfo.si_scaleuse = false;
                    else
                        c_poscominfo.si_scaleuse = true;
                }
            }
            else if(GintLocation == 2)      //토론토
            {
                if (c_poscominfo.si_scaletype != 0)
                {
                    // Scale/Scanner Initialization 추가.
                    // 현재는 ScaleType 9 (OPOS 설정)의 경우만 구현
                    if (EnableOPOSDevices() < 0)
                        c_poscominfo.si_scaleuse = false;
                    else
                        c_poscominfo.si_scaleuse = true;
                }
            }
            else                            // 미국
            {
                if (c_poscominfo.si_scaletype != 0)
                {
                    // Scale/Scanner Initialization 추가.
                    // 현재는 ScaleType 9 (OPOS 설정)의 경우만 구현
                    if (EnableOPOSDevices() < 0)
                        c_poscominfo.si_scaleuse = false;
                    else
                        c_poscominfo.si_scaleuse = true;
                }

                //// 저울에서 스캔 되는거 방지.
                //if (OPOSScanner.DeviceEnabled)
                //{
                //    OPOSScanner.DataEvent -= ScannerDataEvent;
                //    OPOSScanner.DeviceEnabled = false;
                //}
            }

            bgw_ReceiptPrinting.DoWork += new DoWorkEventHandler(bgw_ReceiptPrinting_DoWork);
            bgw_ReceiptPrinting.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_ReceiptPrinting_RunWorkerCompleted);
            
            bgw_ProcessUpdate.DoWork += new DoWorkEventHandler(bgw_ProcessUpdate_DoWork);
            bgw_ProcessUpdate.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_ProcessUpdate_RunWorkerCompleted);

            bgw_ProcessBeepSound.DoWork += new DoWorkEventHandler(bgw_ProcessBeepSound_DoWork);
            bgw_ProcessBeepSound.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_ProcessBeepSound_RunWorkerCompleted);

            // Light ON
            ProcessQLightControl("a0");     // all Light Off
            ProcessQLightControl("g1");     // Green Light On
          
            //Card 결제 버튼 선언 및 Bag Label 수정
            if(GintLocation == 1)                   // 벤쿠버
            {
                btnSelectCreditCard.Size = new Size(318, 92);
                btnSelectCreditCard.Text = "CREDIT/DEBIT CARD";

                lbAddBagTitle.Text = "Bag Charge";
                lbHowmanyPlasticBag.Text = "How many bags did you use?";

                //Search category 버튼 삭제.
                btnSearch_Category.Visible = false;
                btnSearch.Size = new Size(291, 127);
                btnSearch.Location = new Point(76, 195);

                // 마무스 커서 삭제.
                if (GblTestMode == true)
                {
                    //btnscalescanTest.Visible = true;
                    btnscalescanTest2.Visible = true;
                    btnscalescanTest3.Visible = true;
                }
                else
                {
                    ShowCursor(false);
                }

                btnVoid.SendToBack();
                btnItemDiscount.BringToFront();

                lbCredit_DebitCardOnly.Visible = true;

                // 초기화면 CLOSE 되도록 수정.
                pn_TempClosed.BringToFront();
                transitionSiglePage(pn_TempClosed, 0, 200);

                // Light ON
                ProcessQLightControl("a0");     // all Light Off
                ProcessQLightControl("r1");     // Red Light On

                // Station 
                g_StationOpen = false;

                //Managercard Scan Ready
                g_ManagerCardScanReady = true;

            }
            else if (GintLocation == 2 )            // 토론토
            {
                btnSelectCreditCard.Size = new Size(318, 92);
                btnSelectCreditCard.Text = "CREDIT/DEBIT CARD";

                lbAddBagTitle.Text = "Bag Charge";
                lbHowmanyPlasticBag.Text = "How many bags did you use?";

                btnVoid.SendToBack();
                btnItemDiscount.BringToFront();

                transitionSiglePage(pn_Start, 0, 300);
            }
            else                                    // 미국
            {
                //btnSelectCreditCard.Size = new Size(168, 92);
                btnSelectCreditCard.Size = new Size(318, 92);
                btnSelectCreditCard.Text = "CREDIT/DEBIT CARD";

                lbAddBagTitle.Text = "Reusable Bag Charge";
                lbHowmanyPlasticBag.Text = "How many bags did you use?";

                //Search 버튼 비활성화
                //btnSearch_Category.Visible = false;
                btnSearch.Visible = false;
                //btnSearch_Category.Size = new Size(291, 127);
                btnSearch_Category.Size = new Size(318, 127);
                btnSearch_Category.Location = new Point(58, 195);
                btnSearch_Category.Text = "";

                lbSearchOption.Text = "SEARCH ITEM";
                lbCredit_DebitCardOnly.Visible = true;

                if (GblTestMode == true)
                {
                    btnSelectPointCard.Visible = true;
                    btnEBT.Visible = true;

                    //btnscalescanTest.Visible = true;
                    btnscalescanTest2.Visible = true;
                    btnscalescanTest3.Visible = true;
                }
                else
                {   
                    // 86번 유타 인 경우 EBT 결제 버튼 출력 X
                    if (c_poscominfo.ci_mkno == "86"){  btnEBT.Visible = false; }   
                    else                             {  btnEBT.Visible = true;  }

                    ShowCursor(false);
                }

                btnItemDiscount.SendToBack();
                btnVoid.BringToFront();

                // OPEN/CLOSE 버튼 활성화.
                //swbOpenClose.Visible = true;

                // 초기화면 CLOSE 되도록 수정.
                pn_TempClosed.BringToFront();
                transitionSiglePage(pn_TempClosed, 0, 200);
                
                // Light ON
                ProcessQLightControl("a0");     // all Light Off
                ProcessQLightControl("r1");     // Red Light On

                // Station 
                g_StationOpen = false;

                //Managercard Scan Ready
                g_ManagerCardScanReady = true;
                
            }

            // Update File Init
            InitUpdateFileCheck();

            // Printer Status Check Timer Start
            if (c_poscominfo.si_sPrinter1 == "CUSTOM K80")
            {
                //Init the library
                cutomWndAPIWrap.InitLibrary();

                if(InitPrinterStatus())
                {
                    PrintStatusCheckTimer.Interval = g_iPrinterStatusInterval;
                    PrintStatusCheckTimer.Start();
                }             
            }
            else
            {
                // Update Timer Start.
                UpdateCheckTimer.Interval = g_iUpdateInterval;
                UpdateCheckTimer.Start();
            }

            // Start 화면에서 스캔 되는거 대비.
            //ctrlOnScreen = pn_Start;
            txtNumCS.Enabled = true;
            txtNumCS.Clear();
            txtNumCS.Focus();
            KeyInReady();            
        }

        ~HanaSales_SelfCheckOut()
        {
            DisableOPOSDevices();
        }

        [DllImport("user32")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int ShowCursor(bool bShow);

        [DllImport("KERNEL32.DLL")]
        private static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        [DllImport("KERNEL32.DLL")]
        static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        //[DllImport("QUvc_dll.dll")]
        //public static extern unsafe bool Usb_Qu_write(byte Q_index, byte Q_type, byte* pQ_data);
        //[DllImport("QUvc_dll.dll")]
        //public static extern void Usb_Qu_Open();
        //[DllImport("QUvc_dll.dll")]
        //public static extern void Usb_Qu_Close();
        //[DllImport("QUvc_dll.dll")]
        //public static extern int Usb_Qu_Getstate();

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

        public void InitUpdateFileCheck()
        {
            bool bStatus = false;
            string strNowDateTime = string.Empty;
            string strPgmVersion = string.Empty;

            // INI Update Time 체크
            if (!System.IO.File.Exists(g_strUpdateCheckFolder + "\\UpdateTime.ini"))            // 파일이 없으면,
            {
                bStatus = this.CreateIni("UpdateTime");                   // Ini 파일 생성하기.
                if(bStatus)                             // 파일 생성 완료면
                {
                    // 현재 날짜와 시간을  SetIni한다.
                    strNowDateTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                    strPgmVersion = "v1.0.0";
                    this.setIni("UPDATE", "Modified Time", strNowDateTime, g_strUpdateCheckFolder + "\\UpdateTime.ini");
                    this.setIni("UPDATE", "Program Version", strPgmVersion, g_strUpdateCheckFolder + "\\UpdateTime.ini");
                }
                else
                {
                    // Error Message 출력 필요.

                    return;
                }
            }
            //// Update Check
            //ProcessUpdateCheck();
        }

        public bool InitPrinterStatus()
        {
            bool bReturn = false;
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            g_sMessage = string.Format("[{0}] CUSTOM K80 Printer Init function enter : {1}", sMethod, bReturn.ToString());
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            try
            {
                //Open the device
                CustomReceiptPrt = cutomWndAPIWrap.OpenInstalledPrinter("CUSTOM K80");
            }
            catch (Exception ex)
            {
                btnStart.Enabled = false;
                ShowErrorMessage(ex);
                return bReturn;
            }

            bReturn = true;

            g_bPrinterStatusInit = true;

            g_sMessage = string.Format("[{0}] CUSTOM K80 Printer Init Result : {1}", sMethod, bReturn.ToString());
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            return bReturn;
        }

        private void InitMemberInfo()
        {
            GchkMember = false;
            GcID = "";
            GcName = "";
            GcFirst = "";
            GcStore = "";

            GcCustno = "";
            GcTelNo = "";
            GcEmail = "";

            GcHPBal = "";
            GcHPEarn = "";
            GcHPUsed = "";

            GcOrigin = "";
            GcBal = "";
            GcNPH = "";

            GcASLA = "";
            GcAPNo = "";
            GcTPNo = "";
            GcVoid = "";
        }

        private void InitMemberPointInfo()
        {
            GchkMember = false;
            GblMemberEarn = false;
            GblMemberHPuse = false;
            GstrTotalDCType = "";
            GdblTotalDCRate = 0;
            GdblItemDCRate = 0;
            //GcID, GcName, GcFirst, GcStore, GcCustno, GcTelNo, GcEmail, GcHPBal, GcHPEarn, GcHPUsed, GcOrigin, GcBal, GcNPH, GcASLA, GcAPNo, GcTPNo, GcVoid;
            GdblHPTodayUsed = 0;
            GdblHPBal = 0;
            GdblHPEarn = 0;
            GdblHPUsed = 0;
        }
        private void InitMemberDisplay()
        {
            lbMemberNameCS.Text = "";
            lbPrePointCS.Text = "";
            lbEarnPointCS.Text = "";
            lbUsedPointCS.Text = "";
            lbBalancePointCS.Text = "";
            lbBalancePointTop.Text = "";
            lbAvailableAmountNum.Text = "";
        }
        private void initScale()
        {
            lbWeightUnitCS.Text = commonClass.scaleUnitLB;
            //lbWeightCS.Text = "0.00";
         }

        private void initDClabel()
        {
            lblDCRate.Text = "";
            lblDCAmount.Text = "";
        }

        private void initUSADisplay()
        {
            if (GintLocation == 3)
            {
                // 스캔 화면에서 Tax 명칭 변경
                lbHSTCS.Visible = false;
                lbHSTValCS.Visible = false;
                lbPSTCS.Visible = false;
                lbPSTValCS.Visible = false;
                
                lbGSTCS.Text = "Tax1";
                
                // EBT Card Button 출력을 위해 Card Button 축소
                //btnSelectCreditCard.Size = new Size(168, 92);
                btnSelectCreditCard.BringToFront();
                //btnSelectCreditCard.Font = new Font("Helvetica85-Heavy", 20F, FontStyle.Bold);
                //btnSelectCreditCard.Text = "CARD";
            }
            else
            {
                btnSelectCreditCard.Size = new Size(318, 92);
                btnSelectCreditCard.BringToFront();
            }
        }

        private void initPayment()
        {
            lblPayCash.Text = "0.00";
            lblPayDebit.Text = "0.00";
            lblPayCredit.Text = "0.00";
            lblPayGiftCard.Text = "0.00";
            lblPayCerti.Text = "0.00";
            lblPayWechat.Text = "0.00";
            lblPayHmoney.Text = "0.00";
            lblPayCheck.Text = "0.00";
            lblPayEBT.Text = "0.00";
            lblPayCoupon.Text = "0.00";
            lblPayETC.Text = "0.00";
            lblPayPennyRounded.Text = "0.00";
            lblPayTotal.Text = "0.00";
            lblPayBalance.Text = "0.00";
            lbSelectPaymentBalanceNum.Text = "0.00";

            GPayFinish = false;
            GintAuthSeq = 1;
            // Card Pay 저장 배열 초기화.
            for (int i = 0; i < 10; i++)
            {
                CardPay[i] = 0;
            }
        }

        private void initSalesTotal()
        { 
            lbSubTotalValCS.Text = "0.00";
            lbItemCountValCS.Text = "0"; // Count
            lbSubTotalValCS.Text = "0.00";
            lbHSTValCS.Text = "0.00";
            lbGSTValCS.Text = "0.00";
            lbPSTValCS.Text = "0.00";
            lbTotalValCS.Text = "0.00";
            lblPayBalance.Text = "0.00";
            lbSelectPaymentBalanceNum.Text = "0.00";

            if(GintLocation == 1)           // 벤쿠버 인경우
            {
                lbGSTCS.Visible = true;
                lbPSTCS.Visible = true;
                lbHSTCS.Visible = false;

                lbGSTValCS.Visible = true;
                lbPSTValCS.Visible = true;
                lbHSTValCS.Visible = false;
            }
            else if(GintLocation == 2)      // 토론토 인경우
            {
                lbGSTCS.Visible = false;
                lbPSTCS.Visible = false;
                lbHSTCS.Visible = true;

                lbGSTValCS.Visible = false;
                lbPSTValCS.Visible = false;
                lbHSTValCS.Visible = true;
            }
            else                            // 미국인 경우
            {
                lbGSTCS.Visible = true;
                lbPSTCS.Visible = false;
                lbHSTCS.Visible = false;

                lbGSTValCS.Visible = true;
                lbPSTValCS.Visible = false;
                lbHSTValCS.Visible = false;
            }

        }

        private void initListView()
        {
            ItemCSView.Clear();
            ItemCSView.Columns.Add("PRODID", 0, HorizontalAlignment.Left);
            ItemCSView.Columns.Add("SEQ", 0, HorizontalAlignment.Left);
            ItemCSView.Columns.Add("ITEM", 280, HorizontalAlignment.Left);
            ItemCSView.Columns.Add("QTY", 60, HorizontalAlignment.Right);
            ItemCSView.Columns.Add("PRICE", 80, HorizontalAlignment.Right);
            ItemCSView.Columns.Add("AMT", 85, HorizontalAlignment.Right);
            ItemCSView.Columns.Add("T", 32, HorizontalAlignment.Right);
            ItemCSView.Columns.Add("tTYPE", 0, HorizontalAlignment.Right);
        }

        private void initEBTTotal()
        {
            GdblEBTAmountTotal = 0;
            GdblEBTTax1Total = 0;
            GdblEBTTax2Total = 0;
            GdblEBTTax3Total = 0;
            GdblEBTTaxExemptAmountTotal = 0;
            GdblEBTTaxExemptTotal = 0;

            GblEBTReusableBag = false;
            GblEBTTaxExamption = false;
        }

        private void KeyInReady()
        {
            //if (GblTestMode == true)
            //{
            //    return;
            //}

            txtNumCS.Enabled = true;
            txtNumCS.Text = "";

            /* Return 화면 활성화 되어 있을 때 그 쪽에 Focus 먼저 줌 */

            txtNumCS.Focus();

            // wWaitNextKeyin = False // 사용되는 부분 확인 필요

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
        private void btnStart_Click(object sender, EventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            // 화면 및 변수 초기화
            initSalesTotal();
            initPayment();
            initScale();
            initEBTTotal();

            if(GintLocation == 1 || GintLocation == 3)       // 벤쿠버 일때만
            {
                if (c_poscominfo.si_scaletype != 0)
                {
                    // Scale/Scanner Initialization 추가.
                    //// 현재는 ScaleType 9 (OPOS 설정)의 경우만 구현
                    if (EnableOPOSDevices() < 0)
                        c_poscominfo.si_scaleuse = false;
                    else
                        c_poscominfo.si_scaleuse = true;
                }
            }

            // 저울 Enable
            if (OPOSScanner.DeviceEnabled == false)
            {
                _EnableScannerDevice();
            }

            InitMemberInfo();
            InitMemberPointInfo();
            InitMemberDisplay();
            c_poscominfo.ClearMemberInfo();

            GblComplete = false;

            initDClabel();

            //GetNewInvNo();

            g_sMessage = string.Format("[{0}] Push Start Button (Invoice Number : {1}).", sMethod, txtInvNo.Text);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            // 화면 불러오기.

            gbScanPointCard.Visible = true;
            ctrTemp = ctrlOnScreen;
            transitionSiglePage(gbScanPointCard, 0, 100);

            // OPEN/CLOSE 버튼 사라지게.
            swbOpenClose.Visible = false;

            //gbScanPointCard.Visible = true;
            txtScanPointCard.Clear();
            txtScanPointCard.Focus();

            BackToStartTimer.Interval = g_iBackToStartInterval;
            BackToStartTimer.Start();

            // Light ON/OFF
            ProcessQLightControl("a0");     // All Light Off
            ProcessQLightControl("o1");     // Orange Light On

            // 음성 실행.
            if (GssMembershipScan.GetCurrentlySpokenPrompt() != null)                // 음성 중복 재생 방지.
            {
                GssMembershipScan.SpeakAsyncCancel(GssMembershipScan.GetCurrentlySpokenPrompt());
            }
            GssMembershipScan.SelectVoice(GstrVoice);
            GssMembershipScan.SpeakAsync("Scan your Membership Card or Push your Phone number. Please Skip Button if you don't have a membership Card.");

            PrintStatusCheckTimer.Stop();
            UpdateCheckTimer.Stop();            
        }

        private void swbOpenClose_Click(object sender, EventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            g_sMessage = string.Format("[{0}] Push Station OPEN/CLOSE Button (ctrlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            swbOpenClose.Enabled = false;
            btnStart.Enabled = false;
            
            // Entering Help Mode
            ProcessEntering_HelpMode();            
        }

        private void btnScanManagerCardExit_Click(object sender, EventArgs e)
        {
            // 저울 OFF
            //if (OPOSScanner.DeviceEnabled)
            //{
            //    OPOSScanner.DataEvent -= ScannerDataEvent;
            //    OPOSScanner.DeviceEnabled = false;
            //}
            // 화면 이동.
            transitionSiglePage(gbScanManagerCard, 1024, 200);
            ctrlOnScreen = ctrTemp;

            btnSearch.Enabled = true;
            btnSearch_Category.Enabled = true;
            btnBack.Enabled = true;
            btnItemCorrect.Enabled = true;
            btnVoid.Enabled = true;
            btnItemDiscount.Enabled = true;
            btnReprint.Enabled = true;
            btnSuspend.Enabled = true;
            ItemCSView.Enabled = true;
            
            btnAddBagToCart.Enabled = true;
            btnNoBag.Enabled = true;
            btnBagPlus.Enabled = true;
            btnMinus.Enabled = true;

            btnSelectCreditCard.Enabled = true;
            btnSelectPointCard.Enabled = true;
            btnSelectGiftCard.Enabled = true;

            //if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                          // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
            btnEBT.Enabled = true;
            btnManualETCKey.Enabled = true;

            if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
            {
                btnNext.Enabled = false;
            }
            else
            {
                btnNext.Enabled = true;
            }

            // EBT 결제 내역이 있는 경우
            if (Convert.ToDouble(lblPayEBT.Text) > 0)
            {
                btnEBT.Enabled = false;
            }
            else
            {
                //if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                              // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
                btnEBT.Enabled = true;
            }

            // 클릭시 상태 변수 변경
            if (g_StationOpen){  swbOpenClose.Value = false; }
            else                 swbOpenClose.Value = true;

            g_ManagerCardScanReady = false;
            cpScanManagerCard.IsRunning = false;
            btnStart.Enabled = true;
            swbOpenClose.Enabled = true;

            // Light ON
            ProcessQLightControl("a0");     // all Light Off
            ProcessQLightControl("o1");     // Orange Light On

            ProcessQLightControl("b0");     // Beep Off
        }

        private void btnStationOpenNo_Click(object sender, EventArgs e)
        {
            // 저울 OFF
            if (OPOSScanner.DeviceEnabled)
            {
                OPOSScanner.DataEvent -= ScannerDataEvent;
                OPOSScanner.DeviceEnabled = false;
            }
            // 클릭시 상태 변수 변경
            if (g_StationOpen) { swbOpenClose.Value = true; }
            else swbOpenClose.Value = false;

            transitionSiglePage(gbStationOpenQuestion, 1024, 200);
            ctrlOnScreen = ctrTemp;
            
            g_ManagerCardScanReady = false;
            cpScanManagerCard.IsRunning = false;
            btnStart.Enabled = true;
            swbOpenClose.Enabled = true;
        }

        private void btnStationOpenYes_Click(object sender, EventArgs e)
        {
            // 저울 OFF
            if (OPOSScanner.DeviceEnabled)
            {
                OPOSScanner.DataEvent -= ScannerDataEvent;
                OPOSScanner.DeviceEnabled = false;
            }
            // Flag 변경
            if (g_StationOpen) { g_StationOpen = false; }
            else g_StationOpen = true;

            // Message Box 사라짐/ CLOSE 창 출력
            // Flag에 따라 화면 나올지 들어갈지 결정.
            if (g_StationOpen)
            {
                pn_TempClosed.SendToBack();
                transitionSiglePage(pn_TempClosed, 1024, 200);
                transitionSiglePage(gbStationOpenQuestion, 1024, 200);
                ctrlOnScreen = pn_Start;

                // Light ON
                ProcessQLightControl("a0");     // all Light Off
                ProcessQLightControl("g1");     // Green Light On
            }
            else
            {
                pn_TempClosed.BringToFront();
                transitionDoublePage(pn_TempClosed, gbStationOpenQuestion, 0, 1024, 200);

                // Light ON
                ProcessQLightControl("a0");     // all Light Off
                ProcessQLightControl("r1");     // Red Light On
            }
            
            cpScanManagerCard.IsRunning = false;
            btnStart.Enabled = true;
            swbOpenClose.Enabled = true;            
        }


        private void BackToStartTimer_Tick(object sender, EventArgs e)
        {
            BackToStartTimer.Stop();

            gbScanPointCard.Visible = false;
            transitionSiglePage(gbScanPointCard, 1023, 300);
            ctrlOnScreen = ctrTemp;

            //// OPEN/CLOSE 버튼 활성화.
            //swbOpenClose.Visible = true;

            // Light ON
            ProcessQLightControl("a0");     // all Light Off
            ProcessQLightControl("g1");     // Green Light On
        }
        
        public void transitionSiglePage(Control cOnScreen, int iPosition, int iMovingSec)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            ctrlOnScreen = cOnScreen;
            ctrlOnScreen.BringToFront();
            Transition t = new Transition(new TransitionType_EaseInEaseOut(iMovingSec));

            t.add(ctrlOnScreen, "Left", iPosition);
            t.run();

            g_sMessage = string.Format("[{0}] TransitionSigle page (ctrlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

        }
        public void transitionDoublePage(Control cOnScreen, Control cOffScreen, int iOnPagePosition, int iOffPagePosition, int iMovingSec)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            ctrlOnScreen = cOnScreen;
            ctrlOffScreen = cOffScreen;

            ctrlOnScreen.BringToFront();
            ctrlOffScreen.SendToBack();

            Transition t = new Transition(new TransitionType_EaseInEaseOut(iMovingSec));

            t.add(ctrlOnScreen, "Left", iOnPagePosition);
            t.add(ctrlOffScreen, "Left", iOffPagePosition);
            t.run();

            g_sMessage = string.Format("[{0}] TransitionDouble page (ctrlOnScreen : {1}, ctrlOffScreen : {2}).", sMethod, ctrlOnScreen.Name, ctrlOffScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

        }
        private void btnSkipPointCard_Click(object sender, EventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            g_sMessage = string.Format("[{0}] Push Skip Point Card Button).", sMethod);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            InitMemberDisplay();
            c_poscominfo.ClearMemberInfo();
            
            lbMemberShipCS.Visible = false;
            lbMemberNameCS.Visible = false;
            lbBalanceCS.Visible = false;
            lbBalancePointCS.Visible = false;
            lbBalancePointTop.Visible = false;
            lbCurrentCusBal.Visible = false;
            lbAvailableAmount.Visible = false;
            lbAvailableAmountNum.Visible = false;

            txtNumCS.Enabled = true;
            txtNumCS.Clear();
            KeyInReady();

            // Item List에 출력해주는 모듈 필요.
            initListView();
            initSalesTotal();
            initEBTTotal();
            PopulateListViewProdItem("ChangeToItemScan");

            BackToStartTimer.Stop();
            changeToItemScan();

            // 음성 실행.
            if(GssMembershipScan.GetCurrentlySpokenPrompt() != null)                // 음성 중복 재생 방지.
            {
                GssMembershipScan.SpeakAsyncCancel(GssMembershipScan.GetCurrentlySpokenPrompt());
            }
            GssScanyourItem.SelectVoice(GstrVoice);
            GssScanyourItem.SpeakAsync("Scan your Items");

            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnBackToStart_Click(object sender, EventArgs e)
        {
            if (GssMembershipScan.GetCurrentlySpokenPrompt() != null)                // 음성 중복 재생 방지.
            {
                GssMembershipScan.SpeakAsyncCancel(GssMembershipScan.GetCurrentlySpokenPrompt());
            }
            BackToStartTimer.Stop();

            gbScanPointCard.Visible = false;
            transitionSiglePage(gbScanPointCard, 1023, 300);
            ctrlOnScreen = ctrTemp;

            //// OPEN/CLOSE 버튼 활성화.
            //swbOpenClose.Visible = true;

            // Light ON
            ProcessQLightControl("a0");     // all Light Off
            ProcessQLightControl("g1");     // Green Light On

            // Printer Status Check Timer Start
            if (c_poscominfo.si_sPrinter1 == "CUSTOM K80")
            {
                PrintStatusCheckTimer.Interval = g_iPrinterStatusInterval;
                PrintStatusCheckTimer.Start();
            }
            else
            {
                // Update Timer Start.
                UpdateCheckTimer.Interval = g_iUpdateInterval;
                UpdateCheckTimer.Start();
            }

            //if(GintLocation == 1 )              // 벤쿠버 경우
            //{
            //    // 저울에서 스캔 되는거 방지.
            //    if (OPOSScanner.DeviceEnabled)
            //    {
            //        OPOSScanner.DataEvent -= ScannerDataEvent;
            //        OPOSScanner.DeviceEnabled = false;
            //    }
            //}

            KeyInReady();

            //// Update Check timer start
            //UpdateCheckTimer.Interval = g_iUpdateInterval;
            //UpdateCheckTimer.Start();
        }

        private void txtScanPointCard_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)        // Enter or Return 일때만 동작.
            {
                BackToStartTimer.Stop();
                ScanPointCardProcess(txtScanPointCard.Text.Trim());
            }
        }

        private void ScanPointCardProcess(string pScanText)
        {
            bool chkMember = false; // 멤버십 번호 체계인지 여부
            string strTitle = string.Empty;
            string strContent = string.Empty;
            
            //string strProdID = "";

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string sMembershipNumber = pScanText;

            g_sMessage = string.Format("[{0}] ScanPointCardProcess Start (Code: {1})", sMethod, sMembershipNumber);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            // 3. 멤버십 번호 체계 분석하여 chkMembership Call
            if (sMembershipNumber.Length == 12 || sMembershipNumber.Length == 10)
            {
                if (GchkMember == false)
                {
                    chkMember = chkMemebership(sMembershipNumber);
                    if (chkMember == true)
                    {
                        GchkMember = true;
                        lbMemberShipCS.Visible = true;
                        lbMemberNameCS.Visible = true;
                        lbBalanceCS.Visible = false;
                        lbBalancePointCS.Visible = false;
                        lbBalancePointTop.Visible = true;
                        lbCurrentCusBal.Visible = true;

                        if(GblMemberHPuse == true)              // 카드로 스캔한 경우만 결제가능 금액 출력하기.
                        {
                            if(GintLocation != 3)                       // 미국이 아닌 경우만 표시.
                            {
                                lbAvailableAmount.Visible = true;
                                lbAvailableAmountNum.Visible = true;
                            }                            
                        }
                        
                        KeyInReady();
                        initListView();
                        initSalesTotal();
                        initEBTTotal();
                        PopulateListViewProdItem("ChangeToItemScan");

                        BackToStartTimer.Stop();
                        changeToItemScan();

                        // 음성 실행.
                        GssScanyourItem.SelectVoice(GstrVoice);
                        GssScanyourItem.SpeakAsync("Scan your Items");
                    }
                    else
                    {
                        GchkMember = false;

                        //Button Disable
                        btnSkipPointCard.Enabled = false;
                        btnBackToStart.Enabled = false;
                        btnPointPhoneNumber1.Enabled = false;
                        btnPointPhoneNumber2.Enabled = false;
                        btnPointPhoneNumber3.Enabled = false;
                        btnPointPhoneNumber4.Enabled = false;
                        btnPointPhoneNumber5.Enabled = false;
                        btnPointPhoneNumber6.Enabled = false;
                        btnPointPhoneNumber7.Enabled = false;
                        btnPointPhoneNumber8.Enabled = false;
                        btnPointPhoneNumber9.Enabled = false;
                        btnPointPhoneNumber0.Enabled = false;
                        btnPointPhoneNumberBackSpace.Enabled = false;
                        btnPointPhoneNumberEnter.Enabled = false;
                        
                        strTitle = "Membership";
                        strContent = "Your Membership information has a Problem.\n Please Ask to Customer Center.";
                        DisplayErrorMessageBox(strTitle, strContent, 1, sMethod);

                        BackToStartTimer.Interval = g_iBackToStartInterval;
                        BackToStartTimer.Start();
                    }
                }
                else
                {
                    lbMemberShipCS.Visible = true;
                    lbMemberNameCS.Visible = true;
                    lbBalanceCS.Visible = false;
                    lbBalancePointCS.Visible = false;
                    lbCurrentCusBal.Visible = true;

                    if (GblMemberHPuse == true)              // 카드로 스캔한 경우만 결제가능 금액 출력하기.
                    {
                        if (GintLocation != 3)                       // 미국이 아닌 경우만 표시.
                        {
                            lbAvailableAmount.Visible = true;
                            lbAvailableAmountNum.Visible = true;
                        }
                    }

                    KeyInReady();
                    initListView();
                    initSalesTotal();
                    initEBTTotal();
                    PopulateListViewProdItem("ChangeToItemScan");

                    BackToStartTimer.Stop();
                    
                    if(ctrlOnScreen == gbScanPointCard || ctrlOnScreen == pn_Start)
                    {
                        changeToItemScan();
                        // 음성 실행.
                        GssScanyourItem.SelectVoice(GstrVoice);
                        GssScanyourItem.SpeakAsync("Scan your Items");
                    }
                }
            }
            else
            {
                //Button Disable
                btnSkipPointCard.Enabled = false;
                btnBackToStart.Enabled = false;
                btnPointPhoneNumber1.Enabled = false;
                btnPointPhoneNumber2.Enabled = false;
                btnPointPhoneNumber3.Enabled = false;
                btnPointPhoneNumber4.Enabled = false;
                btnPointPhoneNumber5.Enabled = false;
                btnPointPhoneNumber6.Enabled = false;
                btnPointPhoneNumber7.Enabled = false;
                btnPointPhoneNumber8.Enabled = false;
                btnPointPhoneNumber9.Enabled = false;
                btnPointPhoneNumber0.Enabled = false;
                btnPointPhoneNumberBackSpace.Enabled = false;
                btnPointPhoneNumberEnter.Enabled = false;
                
                strTitle = "Membership";
                strContent = "Please Check your Membership card Number.";
                DisplayErrorMessageBox(strTitle, strContent, 1, sMethod);

                BackToStartTimer.Interval = g_iBackToStartInterval;
                BackToStartTimer.Start();
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

                        g_sMessage = string.Format("[{0}] Maket Information OK [{1}]", sMethod, c_poscominfo.ci_mkno);
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

        private bool chkMemebership(string strMembershipNumber)
        {
            string sQBuff = string.Empty;
            long lReturn = 0;
            bool bMemberExist = false;
            int iHPtoAmount = 0;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            // added by Jake
            /* Membership Check Logic 
            1. 멤버십 번호 체계 확인 
            2. 멤버십 번호인지 전화번호인지 확인 (전화번호면 H-Money 사용하지 못하도록 표시)
            3. Total DC 적용된 Transaction은 멤버십 적립 안 되도록 표시
            4. 멤버십 데이터 조회
                4-1. Void 여부 확인
                4-2. 멤버 Type 확인 : 내부거래 / 직원 / VIP / Wholesale (추후)
                4-3. 멤버별 Total DC 적용
            5. 멤버십 데이터 없는 경우 - 신규 멤버 데이터 등록 (ActCode 설정)
            6. 멤버십 할인 정보 초기화
            7. KeyInReady (포커스)
            8. ActCode 체크하여 CustDataChange

            */

            string strInputCode = strMembershipNumber;
            string strLeftSix = c_poscomlibs.Left(strInputCode, 6);

            InitMemberDisplay();
            c_poscominfo.ClearMemberInfo();

            // 1. 멤버십 번호 쳬계 확인
            if (strInputCode.Length == 12)
            {
                if (fncVerifyMembershipCard(strLeftSix))            // 조건 더 추가..카드 번호 체계를 하드코딩 대신 파일이나 DB에서 읽어와서 비교하는 방식 고려..
                {
                    c_poscominfo.mi_cardno = strInputCode;

                    GblMemberEarn = true;
                    GblMemberHPuse = true;
                }
                else
                {
                    GblMemberEarn = false;

                    return false;
                }
            }
            else if (strInputCode.Length == 10)                 // 2. 멤버십 전화번호 확인
            {
                c_poscominfo.mi_telno = strInputCode;

                GblMemberEarn = true;
                GblMemberHPuse = false;
            }
            else
            {
                GblMemberEarn = false;

                return false;
            }

            // 3. Total DC인 경우 Membership 포인트 적립, 사용 안 되도록 표시
            // Member Information은 보여주는 걸로..
            if (GstrTotalDCType == "41")
            {
                MessageBox.Show("In Total DC Case, Membership CANNOT be Applied!");

                GblMemberEarn = false;
                GblMemberHPuse = false;

                return false;
            }

            try
            {
                lReturn = c_localdb.DBConnection();

                if (lReturn < 0)
                {
                    g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    return false;
                }

                sQBuff = "SELECT cStore, cCustno, cID, cName, cFirst, cTelNo, cEmail, cHPBal, cHPEarn, cHPUsed, cOrigin, cBal, cNPH, cASLA, cAPNo, cTPNo, cVoid " +
                           "FROM hanamart.dbo.mfCust " +
                          "WHERE cID LIKE '" + c_poscominfo.mi_cardno + "%' " +
                            "AND cTelNo LIKE '" + c_poscominfo.mi_telno + "%' " +
                            "AND cVoid = '0'";

                lReturn = c_localdb.RsOpen(sQBuff);

                if (lReturn == 1)
                {
                    if (c_localdb.record_count == 1)
                    {
                        c_poscominfo.mi_store = Convert.ToString(c_localdb.rs.Fields["cStore"].Value);
                        c_poscominfo.mi_custno = Convert.ToInt32(c_localdb.rs.Fields["cCustno"].Value);
                        c_poscominfo.mi_cardno = Convert.ToString(c_localdb.rs.Fields["cID"].Value);
                        c_poscominfo.mi_cardno = c_poscominfo.mi_cardno.Trim();
                        c_poscominfo.mi_name = Convert.ToString(c_localdb.rs.Fields["cName"].Value);
                        c_poscominfo.mi_first = Convert.ToString(c_localdb.rs.Fields["cFirst"].Value);
                        c_poscominfo.mi_telno = Convert.ToString(c_localdb.rs.Fields["cTelNo"].Value);
                        c_poscominfo.mi_email = Convert.ToString(c_localdb.rs.Fields["cEmail"].Value);
                        c_poscominfo.mi_pointbalance = Convert.ToInt32(c_localdb.rs.Fields["cHPBal"].Value);
                        c_poscominfo.mi_pointearned = Convert.ToInt32(c_localdb.rs.Fields["cHPEarn"].Value);
                        c_poscominfo.mi_pointused = Convert.ToInt32(c_localdb.rs.Fields["cHPUsed"].Value);
                        c_poscominfo.mi_origin = Convert.ToInt16(c_localdb.rs.Fields["cOrigin"].Value);
                        c_poscominfo.mi_staffbal = Convert.ToDouble(c_localdb.rs.Fields["cBal"].Value);
                        c_poscominfo.mi_nph = Convert.ToString(c_localdb.rs.Fields["cNPH"].Value);
                        c_poscominfo.mi_asla = Convert.ToString(c_localdb.rs.Fields["cASLA"].Value);
                        c_poscominfo.mi_apno = Convert.ToString(c_localdb.rs.Fields["cAPNo"].Value);
                        c_poscominfo.mi_tpno = Convert.ToString(c_localdb.rs.Fields["cTPNo"].Value);
                        c_poscominfo.mi_void = Convert.ToBoolean(c_localdb.rs.Fields["cVoid"].Value);

                        bMemberExist = true;
                    }
                }
                else
                {
                    g_sMessage = string.Format("[{0}] Membership data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    c_localdb.DBClose();

                    return false;
                }

                c_localdb.RsClose();

                //if (strInputCode == c_poscominfo.mi_cardno)   // 입력받은 멤버십 번호가 조회되지 않는 경우 Logic 타지 않도록.. 
                // 전화번호로 조회시 고려..
                if (bMemberExist) // 입력받은 멤버십 번호가 조회되지 않는 경우 Logic 타지 않도록.. 
                {
                    //4 - 1.Void 여부 확인
                    if (c_poscominfo.mi_void)
                    {
                        MessageBox.Show("This Membership Card Number has been voided!");

                        return false;
                    }

                    //4 - 2.멤버 Type 확인 : 내부거래 / 직원 / VIP / Wholesale(추후)
                    if (GblMemberHPuse == true) //카드로 스캔한 경우
                    {
                        if (fncVerifyVIPMembershipCard(c_poscominfo.mi_cardno)) // Bloor점 VIP 할인
                        {
                            GdblTotalDCRate = 10;
                            GstrTotalDCType = "42";
                        }

                        if (GintLocation == 1) // Vancouver
                        {
                            //if (GstrMkno == "64" && (strInputCode == "822222246208" || strInputCode == "822222246246")) // Bloor점 VIP 할인
                            //{
                            //    GdblTotalDCRate = 10;
                            //    GstrTotalDCType = "42";
                            //}

                            if (c_poscominfo.mi_tpno == "2" && c_poscominfo.mi_apno != "")                      // 직원 맴버 할인
                            {

                                DateTime dtStartdate = DateTime.Today;
                                if (c_poscominfo.mi_apno.Length == 10)
                                {
                                    dtStartdate = Convert.ToDateTime(c_poscominfo.mi_apno);
                                }
                                else
                                {
                                    MessageBox.Show("Please check Employee setting!");
                                    return false;
                                }
                                DateTime dtCurdate = DateTime.Today;

                                int intDatediff = (dtCurdate - dtStartdate).Days;

                                if (intDatediff >= 90)
                                {
                                    GdblTotalDCRate = 5;                                                    //직원할인 10% -> 5%로 변경. 김종윤 팀장 요청.
                                    GstrTotalDCType = "43";
                                    GcBal = c_poscominfo.mi_staffbal.ToString();
                                }
                                else
                                {
                                    GdblTotalDCRate = 0;
                                    GstrTotalDCType = "";
                                }

                            }
                        }
                        else if (GintLocation == 2) // Toronto
                        {
                            //if (c_poscominfo.si_counternum == "0" || c_poscominfo.si_counternum == "1")
                            //{
                            //    if (c_poscominfo.mi_asla == "1" || c_poscominfo.mi_asla == "2" || c_poscominfo.mi_asla == "3")
                            //    {
                            //        GdblTotalDCRate = 100;
                            //        GstrTotalDCType = "45";
                            //    }
                            //    else
                            //    {
                            //        GdblTotalDCRate = 0;
                            //        GstrTotalDCType = "";
                            //    }
                            //}
                            //else
                            //{
                            //    MessageBox.Show("Internal Transaction Card can be used at C/S counter Only!");
                            //    return false;
                            //}

                            ////if (GstrMkno == "46" && strInputCode == "822260035000") // Bloor점 VIP 할인
                            ////{
                            ////    GdblTotalDCRate = 10;
                            ////    GstrTotalDCType = "42";
                            ////}
                        }
                        else if (GintLocation == 3) // USA
                        {

                        }
                        else
                        {
                            MessageBox.Show("Please restart the POS program!");

                            return false;
                        }

                        //4 - 3.멤버별 Total DC 적용
                        //ProcessTotalDC(); // 현재까지 스캔된 Item에 대해서 Total DC Rate로 재계산되는 Module.. ==> 이거는 밖으로 빼는 걸로..

                        //4 - 4. New Member의 경우 포인트 적립 안 되도록 


                    }

                    //InitMemberDisplay();
                    
                    lbMemberNameCS.Text = c_poscominfo.mi_first + " " + c_poscominfo.mi_name;
                    lbPrePointCS.Text = string.Format("{0:###,##0}", c_poscominfo.mi_pointbalance);
                    lbEarnPointCS.Text = "0";
                    lbUsedPointCS.Text = "0";
                    lbBalancePointCS.Text = string.Format("{0:###,##0}", c_poscominfo.mi_pointbalance);
                    lbBalancePointTop.Text = string.Format("{0:###,##0}", c_poscominfo.mi_pointbalance);

                    if (Convert.ToInt32(c_poscominfo.mi_pointbalance) < 2500)
                    {
                        iHPtoAmount = 0;
                    }
                    else
                    {
                        iHPtoAmount = Convert.ToInt32(c_poscominfo.mi_pointbalance) / 500;
                    }
                    
                    lbAvailableAmountNum.Text = "$  " + string.Format("{0:###,##0}", iHPtoAmount);

                    //lbMemberNameCT.Text = lbMemberNameCS.Text;
                    //lbPrePointCT.Text = lbPrePointCS.Text;
                    //lbEarnPointCT.Text = lbEarnPointCS.Text;
                    //lbUsedPointCT.Text = lbUsedPointCS.Text;
                    //lbBalancePointCT.Text = lbBalancePointCS.Text;

                    //NewItemScan();
                }
                else
                {
                    //MessageBox.Show("No Member Information found!");

                    InitMemberInfo();
                    InitMemberPointInfo();
                    InitMemberDisplay();
                    KeyInReady();

                    return false;
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

            return bMemberExist;
        }
        public void changeToItemScan()
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            pn_Start.Visible = false;
            gbScanPointCard.Visible = false;
            
            //txtInvNo.Visible = true;
            ctrlOnSubScreen = pnItemScanSearchBtn;
            ctrlOnSubScreen2 = gbScanPointCard;
            ctrlOnScreen = pn_ItemScan;
            ctrlOffScreen = pn_Start;

            ctrTemp = ctrlOnScreen;

            ctrlOnScreen.BringToFront();
            ctrlOffScreen.SendToBack();
            ctrlOnSubScreen.BringToFront();

            Transition t = new Transition(new TransitionType_EaseInEaseOut(500));

            t.add(ctrlOnSubScreen2, "Left", GROUP_BOX_LEFT);
            t.add(ctrlOffScreen, "Left", -1 * GROUP_BOX_LEFT);
            t.add(ctrlOnScreen, "Left", 0);
            t.add(ctrlOnSubScreen, "Left", 584);
            t.run();

            g_sMessage = string.Format("[{0}] Change to Item (ctrlOnScreen : {1}, ctrlOffScreen : {2}, ctrlOnSubScreen : {3}).", sMethod, ctrlOnScreen.Name, ctrlOffScreen.Name, ctrlOnSubScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            st_ProcessStatus.Visible = true;
            tbStep1Name.Visible = true;
            tbStep2Name.Visible = true;
            tbStep3Name.Visible = true;
            tbStep4Name.Visible = true;

            if (!g_HelpModeOn)
            {
                btnBack.Visible = false;
                btnItemCorrect.Visible = false;
                btnAdditionalSearch.Visible = false;
                btnVoid.Visible = false;
                btnItemDiscount.Visible = false;
                btnReprint.Visible = false;
                btnSuspend.Visible = false;
                btnHelp.Enabled = true;
                btnHelp.BringToFront();
                btnManualETCKey.SendToBack();
                // Item Click bug fix
                ItemCSView.Visible = true;
                gbSearchBox.Visible = false;
            }
            else
            {
                btnHelp.Enabled = false;
                lbHelpMode3.BringToFront();
                lbHelpMode4.BringToFront();
                lbHelpMode5.BringToFront();
            }

            if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
            {
                btnNext.Enabled = false;
            }
            else
            {
                btnNext.Enabled = true;
            }
            btnNext.Visible = true;
            btnNext.Font = new System.Drawing.Font("Helvetica85-Heavy", 18F, FontStyle.Bold);
            btnSearch.Enabled = true;
            btnSearch_Category.Enabled = true;

            tbStep4Name.ForeColor = System.Drawing.Color.Silver;
            tbStep3Name.ForeColor = System.Drawing.Color.Silver;
            tbStep2Name.ForeColor = System.Drawing.Color.Silver;
            tbStep1Name.ForeColor = System.Drawing.Color.DimGray;
            st_ProcessStatus.CurrentStep = 1;

            txtNumCS.Clear();
            KeyInReady();
        }

        private bool fncVerifyMembershipCard(string vPrefixCardNo)
        {
            bool bReturn = false;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            try
            {
                if (c_poscominfo.saMemberPrefix.Contains(vPrefixCardNo))
                {
                    bReturn = true;
                }
            }
            catch (Exception ex)
            {
                g_sMessage = string.Format("[{0}] Error caused while operating (line: {1})\n[{0}] {2}", sMethod, ex.Source.ToString(), ex.Message.ToString());
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            }

            return bReturn;
        }

        private bool fncVerifyVIPMembershipCard(string vCardNo)
        {
            bool bReturn = false;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            try
            {
                if (c_poscominfo.saVIPMembership.Contains(vCardNo))
                {
                    bReturn = true;
                }
            }
            catch (Exception ex)
            {
                g_sMessage = string.Format("[{0}] Error caused while operating (line: {1})\n[{0}] {2}", sMethod, ex.Source.ToString(), ex.Message.ToString());
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            }

            return bReturn;
        }

        private string GetEhfCategory(string pEcid)
        {
            string strEcofee = "0";
            string sQBuff = string.Empty;
            long lReturn = 0;
            
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            try
            {
                //lReturn = c_localdb.DBConnection();

                //if (lReturn < 0)
                //{
                //    g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                //    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                //    return "0";
                //}

                sQBuff = "Select ec_fee From hanamart.dbo.tb_ehfcategory Where ec_id = '" + pEcid + "' ";
                lReturn = c_localdb.RsOpen(sQBuff);

                if (lReturn == 1)
                {
                    while (!c_localdb.rs.EOF)
                    {
                        strEcofee = Convert.ToString(c_localdb.rs.Fields["ec_fee"].Value);
                        c_localdb.rs.MoveNext();
                    }

                    c_localdb.RsClose();
                }
                else
                {
                    g_sMessage = string.Format("[{0}] Ecofee data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                }
            }
            catch (SqlException ex)
            {
                g_sMessage = string.Format("[{0}] Database error caused while operating (line: {1})\n[{0}] {2}", sMethod, ex.Source.ToString(), ex.Message.ToString());
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            }
            catch (Exception ex)
            {
                g_sMessage = string.Format("[{0}] Error caused while operating (line: {1})\n[{0}] {2}", sMethod, ex.Source.ToString(), ex.Message.ToString());
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            }
            //finally
            //{
            //    c_localdb.DBClose();
            //}

            return strEcofee;
        }

        private void SetGlobalInfo()
        {
            this.SetLoginNo(c_poscominfo.ui_epno);
            //this.SetLoginInfo(c_poscominfo.ui_epname);
            this.SetTouchUse(c_poscominfo.si_touchuse);
            this.SetCustomerMonitor(c_poscominfo.si_customermonitor);
            this.SetCounterNum(c_poscominfo.si_counternum);
        }

        public void SetLoginNo(string info)
        {
            //this.txtEmpNo.Text = info;
            // 테스트 용 번호. Self Check Out용 번호 생성 필요.

            if(GintLocation == 1)                   //벤쿠버일 경우
            {
                if( c_poscominfo.ci_mkno == "61")
                {
                    this.txtEmpNo.Text = "H3070";       //코퀴틀람 이미순 부팀장.
                }
                else if(c_poscominfo.ci_mkno == "62")
                {
                    this.txtEmpNo.Text = "H2049";       // 다운타운 이경미 팀장
                }
            }
            else if(GintLocation == 2)              // 토론토일 경우
            {
                this.txtEmpNo.Text = "H1306002";            // 리치몬드힐 우지여 팀장
            }
            else                                  // 미국 인경우 
            {
                this.txtEmpNo.Text = "H4001";
            }
            
        }
        public void SetLoginInfo(string info)
        {
            //this.txtLoginInfo.Text = info;
        }
        public void SetTouchUse(bool info)
        {
            this.g_bTouchUse = info;
            if (info == false)
            {
                //panelCopy.Visible = true;
            }
            else
            {
                //panelCopy.Visible = false;
            }
        }
        public void SetCustomerMonitor(bool info)
        {
            this.g_bCustomerMonitor = info;
        }
        public void SetCounterNum(string info)
        {
            this.g_strCounterNum = info;
        }

        private void txtNumCS_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                if (txtNumCS.Text != "")
                {
                    ProcessEntryCode(txtNumCS.Text, 0);
                }
            }
        }

        private void ProcessEntryCode(string strEntryCode, int intKeyIn)
        {
            bool chkMember = false;
            string strProdID = "";
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            /* 입력된 Code를 분석해서 Logic 분기
                 1. 바코드 스캔한 코드인지 키보드의 키를 눌러서 입력된 코드인지 구분
                 2. 쿠폰이나 레이블에 상품 코드와 가격이 들어가 있는 코드 분석 (ex. 11번째 2자리가 00 ~ 10일 때) 앞에 C를 붙임
                 3. 멤버십 번호 체계 분석하여 chkMembership Call
                 3. C 붙어 있는 코드의 경우 ProcessItemSale 진행
                 4. 앞에 2. 붙어있는 코드는 TransactionRelease
                 5. 앞에 1. 붙어있는 코드는 SuspendRelease
                 6. 앞에 9. 붙어있는 코드는 핸디스캐너에서 스캔된 DC 할인쿠폰 ItemDiscountBarcode
                 7. 앞에 39. 붙어있는 코드는 저울에서 스캔된 DC 할인쿠폰 ItemDiscountBarcode
                 8. Else 앞에 F가 붙은 코드는 F 제거. ProcessItemSale

            */

            BackToStartTimerFromItemScan.Stop();

            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();

            // 1. 이전 Transaction 결제 직후 처음 입력받는 값의 경우 Init 처리
            if (g_HelpModeReady != true && g_ManagerCardScanReady != true)                     // 일반 아이템 스캔 및 입력인 경우.
            {
                if (g_HelpModeOn == true && strEntryCode == g_strManagerKey)  // Help Mode On 상태에서 Manager 키가 입력된 경우 
                {
                    g_HelpModeOn = false;
                    g_HelpModeReady = false;

                    //ctrlOnScreen = ctrTemp;

                    txtInvNo.Visible = false;
                    ItemCSView.Enabled = false;
                    btnItemCorrect.Visible = false;
                    btnAdditionalSearch.Visible = false;
                    btnReprint.Visible = false;
                    btnSuspend.Visible = false;
                    btnVoid.Visible = false;
                    btnItemDiscount.Visible = false;

                    //if (GintLocation == 2)                   // 토론토인 경우
                    //{
                    //    btnBack.Visible = false;
                    //}

                    lbHelpMode.Visible = false;
                    lbHelpMode.ForeColor = System.Drawing.Color.Red;
                    lbHelpMode2.Visible = false;
                    lbHelpMode2.ForeColor = System.Drawing.Color.Red;
                    lbHelpMode3.Visible = false;
                    lbHelpMode3.ForeColor = System.Drawing.Color.Red;
                    lbHelpMode4.Visible = false;
                    lbHelpMode4.ForeColor = System.Drawing.Color.Red;
                    lbHelpMode5.Visible = false;
                    lbHelpMode5.ForeColor = System.Drawing.Color.Red;

                    btnHelp.Visible = true;
                    btnHelp.BringToFront();
                    btnHelp.Enabled = true;
                    // ETC KEY BUTTON Send Back
                    btnManualETCKey.Visible = false;
                    btnManualETCKey.SendToBack();
                    btnManualETCKey.Enabled = true;

                    ctrTemp = ctrlOnScreen;
                    transitionSiglePage(pn_ManualETCKey, 1024, 200);
                    ctrlOnScreen = ctrTemp;

                    btnSearch.Enabled = true;
                    btnSearch_Category.Enabled = true;
                    btnNext.Enabled = true;

                    if (ctrlOnScreen == pnAddBag || ctrlOnScreen == pnSelectPayment)
                    {
                        btnBack.Visible = true;
                        btnBack.Enabled = true;
                        btnBack.Image = global::HanaSales_SelfCheckOut.Properties.Resources.new_Back;
                    }
                    else
                    {
                        btnBack.Visible = false;
                        btnBack.Enabled = false;
                    }


                    if (ctrlOnScreen == pnSelectPayment || ItemCSView.Items.Count == 0 || lbTotalValCS.Text == "0.00")
                    {
                        btnNext.Enabled = false;
                    }

                    KeyInReady();

                    //if (GintLocation != 3)                       // 미국이 아닌 경우
                    //{
                    //    if (ctrlOnScreen != pn_ItemScan && ctrlOnScreen != pnItemScanSearchBtn && ctrlOnScreen != gbAgeCheck)
                    //    {
                    //        txtNumCS.Enabled = false;

                    //        // 저울에서 스캔 되는거 방지.
                    //        if (OPOSScanner.DeviceEnabled)
                    //        {
                    //            OPOSScanner.DataEvent -= ScannerDataEvent;
                    //            OPOSScanner.DeviceEnabled = false;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        txtNumCS.Enabled = true;

                    //        // 저울 Enable
                    //        if (OPOSScanner.DeviceEnabled == false)
                    //        {
                    //            _EnableScannerDevice();
                    //        }
                    //    }
                    //}
                    //else                                        // 미국인 경우
                    //{
                        txtNumCS.Enabled = true;

                        // 저울 Enable
                        if (OPOSScanner.DeviceEnabled == false)
                        {
                            _EnableScannerDevice();
                        }
                    //}

                    btnSelectCreditCard.Enabled = true;
                    btnSelectPointCard.Enabled = true;
                    btnSelectGiftCard.Enabled = true;

                    //if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                                  // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
                    btnEBT.Enabled = true;

                    btnMinus.Enabled = true;
                    btnBagPlus.Enabled = true;
                    btnAddBagToCart.Enabled = true;
                    btnNoBag.Enabled = true;

                    // Light ON
                    ProcessQLightControl("a0");     // all Light Off
                    ProcessQLightControl("o1");     // Orange Light On

                }
                else                                        // 일반 아이템 일 경우
                {
                    // 1. 이전 Transaction 결제 직후 처음 입력받는 값의 경우 Init 처리
                    if (ItemCSView.Items.Count < 1)
                    {
                        initSalesTotal();
                        initPayment();
                        initScale();
                        initEBTTotal();

                        if (GblComplete == true)
                        {
                            InitMemberInfo();
                            InitMemberPointInfo();
                            InitMemberDisplay();
                            c_poscominfo.ClearMemberInfo();

                            GblComplete = false;
                        }

                        initDClabel();

                        GetNewInvNo();
                    }

                    // 2. 멤버십 번호 체계 분석하여 chkMembership Call
                    strProdID = strEntryCode.Trim();
                    // Temp
                    g_sMessage = string.Format("[{0}] Barcode was scanned.(Code: {1})", sMethod, strProdID);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

//                    if (strProdID.Length == 12 || strProdID.Length == 10)
//                    {
                        if (GchkMember == false)
                        {
                            if (strProdID.Length == 12 || strProdID.Length == 10)
                            {
                                chkMember = chkMemebership(strProdID);
                                if (chkMember == true)
                                {
                                    lbMemberShipCS.Visible = true;
                                    lbMemberNameCS.Visible = true;
                                    lbBalanceCS.Visible = false;
                                    lbBalancePointCS.Visible = false;
                                    lbBalancePointTop.Visible = true;
                                    lbCurrentCusBal.Visible = true;

                                    if (GblMemberHPuse == true)              // 카드로 스캔한 경우만 결제가능 금액 출력하기.
                                    {
                                        if (GintLocation != 3)                       // 미국이 아닌 경우만 표시.
                                        {
                                            lbAvailableAmount.Visible = true;
                                            lbAvailableAmountNum.Visible = true;
                                        }
                                    }

                                    GchkMember = true;
                                    ProcessTotalDC();
                                }
                            }                       
                        }
                        //else                                            // 멤버쉽 정보가 이미 들어가 있는 상태에서 다시 스캔된 경우
                        //{
                        //    if (chkMemebership(strProdID) == true)
                        //    {
                        //        lbMemberShipCS.Visible = true;
                        //        lbMemberNameCS.Visible = true;
                        //        lbBalanceCS.Visible = false;
                        //        lbBalancePointCS.Visible = false;
                        //        lbBalancePointTop.Visible = true;
                        //        lbCurrentCusBal.Visible = true;

                        //        if (GblMemberHPuse == true)              // 카드로 스캔한 경우만 결제가능 금액 출력하기.
                        //        {
                        //            if (GintLocation != 3)                       // 미국이 아닌 경우만 표시.
                        //            {
                        //                lbAvailableAmount.Visible = true;
                        //                lbAvailableAmountNum.Visible = true;
                        //            }
                        //        }

                        //        // Button Control
                        //        btnBack.Enabled = false;
                        //        btnItemCorrect.Enabled = false;
                        //        btnVoid.Enabled = false;
                        //        btnItemDiscount.Enabled = false;
                        //        btnReprint.Enabled = false;
                        //        btnSuspend.Enabled = false;
                        //        btnNext.Enabled = false;
                        //        ItemCSView.Enabled = false;
                        //        btnHelp.Enabled = false;

                        //        btnSearch.Enabled = false;
                        //        btnSearch_Category.Enabled = false;

                        //        btnAddBagToCart.Enabled = false;
                        //        btnNoBag.Enabled = false;
                        //        btnBagPlus.Enabled = false;
                        //        btnMinus.Enabled = false;

                        //        btnSelectCreditCard.Enabled = false;
                        //        btnSelectPointCard.Enabled = false;
                        //        btnSelectGiftCard.Enabled = false;
                        //        btnEBT.Enabled = false;
                        //        btnManualETCKey.Enabled = false;

                        //        DisplayErrorMessageBox("ITEM SCAN", "Membership is already scanned.", 1, sMethod);

                        //        g_sMessage = string.Format("[{0}] Membership is already scanned.({1})", sMethod, strProdID);
                        //        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                        //        c_localdb.RsClose();

                        //        return;
                        //    }
                        //}
                    //}

                    // 3. Suspend 바코드 분석
                    if (strProdID.Substring(0, 2) == "1." || strProdID.Substring(0, 2) == "2.")
                    {
                        //TransactionRelease(c_poscomlibs.Right(strProdID, 13));
                    }
                    // 4. ITEM DISCOUNT Barcode
                    else if ((strProdID.Substring(0, 2) == "9." && strProdID.Length == 5) || (strProdID.Substring(0, 3) == "39." && strProdID.Length == 6))               // Discount Barcode 스캔시
                    {
                        if (g_ItemDiscountModeOn == false)
                        {
                            if (GintLocation == 1)                           // 벤쿠버 인 경우
                            {
                                btnHelpExit.Visible = false;
                                lbHelpMessage3.Text = "ITEM DISCOUNT";
                                ProcessEntering_HelpMode();

                                g_sMessage = string.Format("[{0}] Discount Barcode scanned.({1})", sMethod, strProdID);
                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                                c_localdb.RsClose();

                                return;
                            }
                            else if (GintLocation == 2)                 // 토론토 인 경우 할인 바코드 사용 안하도록 요청함. 이은희 부팀장. 2023-03-22
                            {
                                btnSearch.Enabled = false;
                                btnSearch_Category.Enabled = false;
                                btnNext.Enabled = false;
                                btnHelp.Enabled = false;

                                DisplayErrorMessageBox("ITEM SCAN", "Please scan Item Barcode.", 1, sMethod);

                                g_sMessage = string.Format("[{0}] Wrong Barcode scanned.({1})", sMethod, strProdID);
                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                                c_localdb.RsClose();

                                return;
                            }
                        }
                        else
                        {
                            //g_ItemDiscountModeOn = false;
                            // 바코드에서 DC Rate 가져오기.
                            g_sMessage = string.Format("[{0}] Discount Barcode input.({1})", sMethod, strProdID);
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                            GdblItemDCRate = Convert.ToDouble(strProdID.Substring(strProdID.LastIndexOf('.') + 1, 3));          // Rate 파싱에서 넣기.

                            ProcessItemDiscountBarcode();
                            return;
                        }
                    }
                    else if (strProdID.Substring(0, 2) == "98" && strProdID.Length == 8)                // Discount coupon Table에 있는 내용 확인
                    {
                        ProcessDiscountCoupon(strProdID);
                        return;
                    }
                    else
                    {
                        if (strProdID.Length > 1 && strProdID.Substring(0, 1) == "F")
                        {
                            strProdID = c_poscomlibs.Right(strProdID, strProdID.Length - 1);
                        }
                    }


                    // 멤버 체크를 해서 멤버 정보를 읽어온 경우에는 상품 추가 Logic 생략
                    if (chkMember == false)
                    {
                        if (g_HelpModeOn == false)
                        {
                            if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
                            {
                                btnNext.Enabled = false;
                            }
                            else
                            {
                                btnNext.Enabled = true;
                            }
                        }

                        // Membership Scan 화면에서 저울에 스캔시 아이템 스캔 화면으로 이동.
                        if (ctrlOnScreen == gbScanPointCard)
                        {
                            InitMemberDisplay();
                            c_poscominfo.ClearMemberInfo();

                            lbMemberShipCS.Visible = false;
                            lbMemberNameCS.Visible = false;
                            lbBalanceCS.Visible = false;
                            lbBalancePointCS.Visible = false;
                            lbBalancePointTop.Visible = false;
                            lbCurrentCusBal.Visible = false;
                            lbAvailableAmount.Visible = false;
                            lbAvailableAmountNum.Visible = false;

                            txtNumCS.Enabled = true;
                            txtNumCS.Clear();
                            KeyInReady();

                            BackToStartTimer.Stop();
                            changeToItemScan();

                            // 음성 실행.
                            if (GssMembershipScan.GetCurrentlySpokenPrompt() != null)                // 음성 중복 재생 방지.
                            {
                                GssMembershipScan.SpeakAsyncCancel(GssMembershipScan.GetCurrentlySpokenPrompt());
                            }
                            GssScanyourItem.SelectVoice(GstrVoice);
                            GssScanyourItem.SpeakAsync("Scan your Items");

                            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
                            BackToStartTimerFromItemScan.Start();
                        }
                        else if(ctrlOnScreen == pn_Start)               // Start 화면에서 저울에 스캔시 아이템 스캔 화면으로 이동.
                        {
                            // 화면 및 변수 초기화
                            initSalesTotal();
                            initPayment();
                            initScale();
                            initEBTTotal();

                            if (GintLocation == 1 || GintLocation == 3)       // 벤쿠버 일때만
                            {
                                if (c_poscominfo.si_scaletype != 0)
                                {
                                    // Scale/Scanner Initialization 추가.
                                    //// 현재는 ScaleType 9 (OPOS 설정)의 경우만 구현
                                    if (EnableOPOSDevices() < 0)
                                        c_poscominfo.si_scaleuse = false;
                                    else
                                        c_poscominfo.si_scaleuse = true;
                                }
                            }

                            // 저울 Enable
                            if (OPOSScanner.DeviceEnabled == false)
                            {
                                _EnableScannerDevice();
                            }

                            InitMemberInfo();
                            InitMemberPointInfo();
                            InitMemberDisplay();
                            c_poscominfo.ClearMemberInfo();

                            GblComplete = false;

                            initDClabel();

                            //GetNewInvNo();

                            g_sMessage = string.Format("[{0}] Push Start Button (Invoice Number : {1}).", sMethod, txtInvNo.Text);
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                            // 화면 불러오기.

                            gbScanPointCard.Visible = true;
                            ctrTemp = ctrlOnScreen;
                            transitionSiglePage(gbScanPointCard, 0, 100);

                            // OPEN/CLOSE 버튼 사라지게.
                            //swbOpenClose.Visible = false;

                            //gbScanPointCard.Visible = true;
                            txtScanPointCard.Clear();
                            txtScanPointCard.Focus();

                            BackToStartTimer.Interval = g_iBackToStartInterval;
                            BackToStartTimer.Start();

                            // Light ON/OFF
                            ProcessQLightControl("a0");     // All Light Off
                            ProcessQLightControl("o1");     // Orange Light On

                            // 음성 실행.
                            if (GssMembershipScan.GetCurrentlySpokenPrompt() != null)                // 음성 중복 재생 방지.
                            {
                                GssMembershipScan.SpeakAsyncCancel(GssMembershipScan.GetCurrentlySpokenPrompt());
                            }
                            GssMembershipScan.SelectVoice(GstrVoice);
                            GssMembershipScan.SpeakAsync("Scan your Membership Card or Push your Phone number. Please Skip Button if you don't have a membership Card.");

                            PrintStatusCheckTimer.Stop();
                            UpdateCheckTimer.Stop();
                        }

                        ProcessItemSale(strProdID, 1);
                    }
                    else                                        // Membership 번호로 입력한 경우 
                    {
                        ScanPointCardProcess(strProdID);
                    }

                    txtNumCS.Clear();
                    KeyInReady();
                }
            }
            else if (g_HelpModeReady == true)                                               // HelpModeReady True 상태에서 Manager권한 키가 입력되었을 경우.
            {
                if (strEntryCode == g_strManagerKey)           // 임시번호 부여. 최종적으로 변경되야 함. Manager 키 입력 하는 부분 추가필요.
                {
                    g_HelpModeOn = true;
                    g_HelpModeReady = false;

                    txtInvNo.Visible = true;
                    ItemCSView.Enabled = true;
                    btnItemCorrect.Visible = true;
                    btnAdditionalSearch.Visible = true;
                    btnVoid.Visible = true;
                    btnItemDiscount.Visible = true;
                    btnReprint.Visible = true;
                    btnReprint.Enabled = true;
                    btnSuspend.Visible = true;
                    btnSuspend.Enabled = true;

                    btnItemCorrect.Enabled = true;
                    btnVoid.Enabled = true;
                    btnItemDiscount.Enabled = true;

                    btnBack.Visible = true;
                    btnBack.Enabled = true;

                    //ctrlOnScreen = ctrTemp;
                    
                    //if ((ctrlOnScreen == gbHelp && ctrTemp != pnSelectPayment && (ctrlOffScreen == pn_Start || ctrlOffScreen == pnAddBag)) || ctrlOnScreen == pn_ItemScan)
                    if (ctrlOnScreen == pn_ItemScan || ctrlOnScreen == pnItemScanSearchBtn)
                    {
                        btnBack.Image = global::HanaSales_SelfCheckOut.Properties.Resources.Exit;
                    }
                    else
                    {
                        btnBack.Image = global::HanaSales_SelfCheckOut.Properties.Resources.new_Back;
                    }
                    if (ctrlOnScreen == gbHelp)
                    {
                        ctrlOnScreen = ctrTemp;
                    }
                    
                    lbHelpMode.Visible = true;
                    lbHelpMode2.Visible = true;
                    lbHelpMode3.Visible = true;
                    lbHelpMode4.Visible = true;
                    lbHelpMode5.Visible = true;
                    btnHelp.Enabled = false;
                    btnHelp.Visible = false;
                    btnHelp.SendToBack();
                    // ETC KEY BUTTON BRING FRONT
                    btnManualETCKey.Visible = true;
                    btnManualETCKey.BringToFront();

                    if (GintLocation == 1)               // 벤쿠버 일 경우
                    {
                        if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
                        {
                            btnNext.Enabled = false;
                        }
                        else
                        {
                            btnNext.Enabled = true;
                        }
                    }
                    else if (GintLocation == 2)              // 토론토 일 경우
                    {
                        if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
                        {
                            btnNext.Enabled = false;
                        }
                        else
                        {
                            btnNext.Enabled = true;
                        }
                    }
                    else                                // 미국 인 경우
                    {
                        if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
                        {
                            btnNext.Enabled = false;
                        }
                        else
                        {
                            btnNext.Enabled = true;
                        }
                    }

                    btnSearch.Enabled = true;
                    btnSearch_Category.Enabled = true;
                    btnAddBagToCart.Enabled = true;
                    btnNoBag.Enabled = true;

                    btnBagPlus.Enabled = true;
                    btnMinus.Enabled = true;

                    btnSelectCreditCard.Enabled = true;

                    if (Convert.ToDouble(lblPayHmoney.Text) > 0) { btnSelectPointCard.Enabled = false; pbSelectpaymentPointCardApplied.Visible = true; }
                    else { btnSelectPointCard.Enabled = true; pbSelectpaymentPointCardApplied.Visible = false; }

                    //if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                                // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
                    //{
                        if (Convert.ToDouble(lblPayEBT.Text) > 0) { btnEBT.Enabled = false; pbSelectpaymentEBTApplied.Visible = true; }
                        else { btnEBT.Enabled = true; pbSelectpaymentEBTApplied.Visible = false; }
                    //}

                    btnSelectGiftCard.Enabled = true;

                    // Adult Check 여부 확인
                    if ((g_CertifiedAdultReady == true && g_CertifiedAdult == false) || (g_iStoredAdultLimit < g_iAdultLimit))
                    {
                        // AGE Check 입력창 출력
                        lbHelpMessage3.Text = "";
                        transitionSiglePage(gbHelp, 1023, 200);
                        ctrlOnScreen = ctrTemp;

                        // Beep Sound Stop.
                        g_bBreakBeepSound = false;
                        ProcessBeepSoundTimer.Stop();


                        if (GintLocation == 3)           // 미국인 경우
                        {
                            transitionSiglePage(gbAgeCheck, 372, 200);

                            // Age Check 일 경우 다른 버튼 닫기
                            btnHelp.Enabled = false;
                            btnBack.Enabled = false;
                            btnVoid.Enabled = false;
                            btnItemDiscount.Enabled = false;
                            btnItemCorrect.Enabled = false;
                            btnSuspend.Enabled = false;
                            btnReprint.Enabled = false;
                            btnSearch.Enabled = false;
                            btnSearch_Category.Enabled = false;
                            ItemCSView.Enabled = false;
                            btnNext.Enabled = false;
                            btnManualETCKey.Enabled = false;

                        }
                        else if (GintLocation == 2)      // 토론토인 경우
                        {
                            transitionSiglePage(gbAgeCheckConfirmation, 240, 200);

                            // Age Check 일 경우 다른 버튼 닫기
                            btnHelp.Enabled = false;
                            btnBack.Enabled = false;
                            btnVoid.Enabled = false;
                            btnItemDiscount.Enabled = false;
                            btnItemCorrect.Enabled = false;
                            btnSuspend.Enabled = false;
                            btnReprint.Enabled = false;
                            btnSearch.Enabled = false;
                            btnSearch_Category.Enabled = false;
                            ItemCSView.Enabled = false;
                            btnManualETCKey.Enabled = false;
                        }
                        else
                        {

                        }

                        tbAgeCheckNum.MaxLength = 8;
                        tbAgeCheckNum.Focus();
                    }
                    else
                    {
                        lbHelpMessage3.Text = "";

                        ctrTemp = ctrlOnScreen;
                        transitionSiglePage(gbHelp, 1023, 200);
                        ctrlOnScreen = ctrTemp;

                        // Beep Sound Stop.
                        g_bBreakBeepSound = false;
                        ProcessBeepSoundTimer.Stop();
                    }

                    // Flash Mode
                    lbHelpMode.ForeColor = System.Drawing.Color.Red;
                    lbHelpMode2.ForeColor = System.Drawing.Color.Red;
                    lbHelpMode3.BackColor = System.Drawing.Color.Red;
                    lbHelpMode3.BringToFront();
                    lbHelpMode4.BackColor = System.Drawing.Color.Red;
                    lbHelpMode4.BringToFront();
                    lbHelpMode5.BackColor = System.Drawing.Color.Red;
                    lbHelpMode5.BringToFront();
                    Transition.run(lbHelpMode, "ForeColor", Color.White, new TransitionType_Flash(1000, 200));
                    Transition.run(lbHelpMode2, "ForeColor", Color.White, new TransitionType_Flash(1000, 200));
                    Transition.run(lbHelpMode3, "BackColor", Color.White, new TransitionType_Flash(1000, 300));
                    Transition.run(lbHelpMode4, "BackColor", Color.White, new TransitionType_Flash(1000, 300));
                    Transition.run(lbHelpMode5, "BackColor", Color.White, new TransitionType_Flash(1000, 300));

                    if (OPOSScanner.DeviceEnabled == false)
                    {
                        _EnableScannerDevice();
                    }

                    // Light ON
                    ProcessQLightControl("a0");     // all Light Off
                    ProcessQLightControl("r1");     // Red Light On

                    ProcessQLightControl("b0");     // Beep

                    KeyInReady();
                }
                else                                        // HelpModeReady True 상태에서 Manager Key가 아닌 다른 키가 들어왔을때.
                {
                    // Error Message
                    DisplayErrorMessageBox("Help Mode", "Please Scan your Manager Key.", 1, sMethod);
                    KeyInReady();
                    return;
                }
            }
            else if (g_ManagerCardScanReady == true)                                               // g_ManagerCardScanReady True 상태에서 Manager권한 키가 입력되었을 경우.
            {
                // ManagerCardScan 창 이동.
                //// OPEN/CLOSE 할건지 묻는 창 출력.
                //if (g_StationOpen) { lbStationOpen.Text = "Do you want to Close this station?"; }
                //else { lbStationOpen.Text = "Do you want to Open this station?"; } 

                //gbStationOpenQuestion.BringToFront();
                //transitionDoublePage(gbStationOpenQuestion, gbScanManagerCard, 240, 1024, 200);

                if (strEntryCode == g_strManagerKey)           // 임시번호 부여. 최종적으로 변경되야 함. Manager 키 입력 하는 부분 추가필요.
                {
                    if(g_mMaterFunctionVal == ManualMasterFunction.ItemETC)
                    {
                        //// 저울 OFF
                        //if (OPOSScanner.DeviceEnabled)
                        //{
                        //    OPOSScanner.DataEvent -= ScannerDataEvent;
                        //    OPOSScanner.DeviceEnabled = false;
                        //}
                        // Flag 변경
                        if (g_StationOpen) { g_StationOpen = false; }
                        else g_StationOpen = true;

                        // Message Box 사라짐/ CLOSE 창 출력
                        // Flag에 따라 화면 나올지 들어갈지 결정.
                        if (g_StationOpen)
                        {
                            //pn_TempClosed.SendToBack();
                            transitionDoublePage(pn_TempClosed, gbScanManagerCard, 1024, 1024, 200);
                            ctrlOnScreen = pn_Start;

                            // Light ON
                            ProcessQLightControl("a0");     // all Light Off
                            ProcessQLightControl("g1");     // Green Light On
                            
                            ProcessQLightControl("b0");     // Beep Off
                            
                            // Start로 가는 Timer Stop
                            BackToStartTimerFromItemScan.Stop();
                        }
                        else
                        {
                            pn_TempClosed.BringToFront();
                            transitionDoublePage(pn_TempClosed, gbScanManagerCard, 0, 1024, 200);

                            // Light ON
                            ProcessQLightControl("a0");     // all Light Off
                            ProcessQLightControl("r1");     // Red Light On

                            ProcessQLightControl("b0");     // Beep Off

                            // Start로 가는 Timer Stop
                            BackToStartTimerFromItemScan.Stop();
                        }

                        cpScanManagerCard.IsRunning = false;
                        btnStart.Enabled = true;
                        swbOpenClose.Enabled = true;
                    }
                    else
                    {
                        switch(g_mMaterFunctionVal)
                        {
                            case ManualMasterFunction.ItemCorrect:
                                //processItemcorrect
                                ProcessItemCorrect();
                                // 화면 이동.
                                transitionSiglePage(gbScanManagerCard, 1024, 200);
                                ctrlOnScreen = ctrTemp;

                                btnBack.Enabled = true;
                                btnItemCorrect.Enabled = true;
                                btnVoid.Enabled = true;
                                btnItemDiscount.Enabled = true;
                                btnReprint.Enabled = true;
                                btnSuspend.Enabled = true;

                                if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
                                {
                                    btnNext.Enabled = false;
                                }
                                else
                                {
                                    btnNext.Enabled = true;
                                }
                                ItemCSView.Enabled = true;

                                btnSearch.Enabled = true;
                                btnSearch_Category.Enabled = true;

                                btnAddBagToCart.Enabled = true;
                                btnNoBag.Enabled = true;
                                btnBagPlus.Enabled = true;
                                btnMinus.Enabled = true;

                                btnSelectCreditCard.Enabled = true;
                                btnSelectPointCard.Enabled = true;
                                btnSelectGiftCard.Enabled = true;

//                                if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                                            // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
                                btnEBT.Enabled = true;

                                btnManualETCKey.Enabled = true;

                                // Light ON
                                ProcessQLightControl("a0");     // all Light Off
                                ProcessQLightControl("o1");     // Orange Light On

                                ProcessQLightControl("b0");     // Beep Off

                                g_mMaterFunctionVal = ManualMasterFunction.ItemETC;
                                break;
                            case ManualMasterFunction.ItemBtnVoid:
                                //processVoid
                                transitionSiglePage(gbScanManagerCard, 1024, 200);
                                ctrlOnScreen = ctrTemp;
                                // Item Void Error Message
                                DisplayErrorMessageBox("VOID", "Warning!! All item list will be deleted.\nAre you sure? ", 2, "btnVoid_Click");
                                g_mMaterFunctionVal = ManualMasterFunction.ItemETC;

                                // Light ON
                                ProcessQLightControl("a0");     // all Light Off
                                ProcessQLightControl("o1");     // Orange Light On

                                ProcessQLightControl("b0");     // Beep Off
                                break;
                            case ManualMasterFunction.ItemBackVoid:
                                //processVoid
                                transitionSiglePage(gbScanManagerCard, 1024, 200);
                                ctrlOnScreen = ctrTemp;
                                // Item Void Error Message
                                DisplayErrorMessageBox("VOID", "Warning!! All item list will be deleted.\nAre you sure? ", 2, "btnBack_Click");
                                g_mMaterFunctionVal = ManualMasterFunction.ItemETC;
                                // Light ON
                                ProcessQLightControl("a0");     // all Light Off
                                ProcessQLightControl("o1");     // Orange Light On

                                ProcessQLightControl("b0");     // Beep Off
                                break;
                            case ManualMasterFunction.ItemSuspend:
                                //processSuspend
                                transitionSiglePage(gbScanManagerCard, 1024, 200);
                                ctrlOnScreen = ctrTemp;
                                DisplayErrorMessageBox("Suspend", "Transaction Will be Suspended. \nAre you Sure?", 2, "btnSuspend_Click");
                                g_mMaterFunctionVal = ManualMasterFunction.ItemETC;

                                // Light ON
                                ProcessQLightControl("a0");     // all Light Off
                                ProcessQLightControl("o1");     // Orange Light On

                                ProcessQLightControl("b0");     // Beep Off
                                break;
                            case ManualMasterFunction.ItemReprint:
                                //processReprint
                                transitionSiglePage(gbScanManagerCard, 1024, 200);
                                ctrlOnScreen = ctrTemp;

                                ProcessReprint();

                                g_mMaterFunctionVal = ManualMasterFunction.ItemETC;

                                // Light ON
                                ProcessQLightControl("a0");     // all Light Off
                                ProcessQLightControl("o1");     // Orange Light On

                                ProcessQLightControl("b0");     // Beep Off
                                break;
                            case ManualMasterFunction.ItemETCKeyIN:
                                //processReprint
                                transitionSiglePage(gbScanManagerCard, 1024, 200);
                                ctrlOnScreen = ctrTemp;

                                ProcessManualETCKey();

                                g_mMaterFunctionVal = ManualMasterFunction.ItemETC;

                                // Light ON
                                ProcessQLightControl("a0");     // all Light Off
                                ProcessQLightControl("o1");     // Orange Light On

                                ProcessQLightControl("b0");     // Beep Off
                                break;
                                
                            default:
                                break;
                        }                         
                    }

                    g_ManagerCardScanReady = false;

                }
                else
                {
                    // Error Message
                    DisplayErrorMessageBox("Help Mode", "Please Scan your Manager Key.", 1, sMethod);
                    KeyInReady();
                    return;
                }
            }
        }

        private void ProcessItemSale(string strProdID, double dQty)
        {
            AddProdItem(strProdID, dQty);

            if(g_iRegPriceItemCount > 0 )
            {
                AddProdItem(strProdID, g_iRegPriceItemCount);
                g_iRegPriceItemCount = 0;
            }

            ProcessTotalDC();
            CntTotSalePayment();

        }
        private void GetNewInvNo()
        {
            string sQBuff = string.Empty;
            long lReturn = 0;
            int iInvSeq = 0;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string strNewInvNo = "", strYear = DateTime.Now.ToString("yy");
            //string sInvNoPrefix = String.Format("{0:D2}", c_poscominfo.ci_mkno) + String.Format("{0:D2}", c_poscominfo.si_counternum) + strYear;
            string sInvNoPrefix = String.Format("{0:D2}", c_poscominfo.ci_mkno) + (c_poscominfo.si_counternum.PadLeft(2, '0')) + strYear;
            string strMaxInvNo = "";
            string strCompareInvNo = "";

            try
            {
                lReturn = c_localdb.DBConnection();

                if (lReturn < 0)
                {
                    g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    return;
                }

                sQBuff = "SELECT kb_InvSeq " +
                           "FROM hanamart.dbo.tb_KeyBuff " +
                          "WHERE kb_gb = 1 " +
                            "AND kb_mkNo = " + c_poscominfo.ci_mkno + " " +
                            "AND kb_stNo = " + c_poscominfo.si_counternum + " " +
                            "AND kb_year = '" + strYear + "'";

                lReturn = c_localdb.RsOpen(sQBuff);

                if (lReturn == 1)
                {
                    if (c_localdb.record_count == 1)
                    {
                        iInvSeq = Convert.ToInt32(c_localdb.rs.Fields["kb_invseq"].Value);
                        strNewInvNo = sInvNoPrefix + String.Format("{0,7:D7}", iInvSeq + 1);
                    }
                }
                else
                {
                    g_sMessage = string.Format("[{0}] Invoice number data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    c_localdb.DBClose();

                    return;
                }

                c_localdb.RsClose();

                if (strNewInvNo.Length == 13) // Invoice No가 만들어지면 현재 KeyBuff의 내용을 삭제
                {
                    sQBuff = "DELETE FROM hanamart.dbo.tb_KeyBuff " +
                                   "WHERE kb_gb = 1 " +
                                     "AND kb_mkNo = " + c_poscominfo.ci_mkno + " " +
                                     "AND kb_stNo = " + c_poscominfo.si_counternum + " " +
                                     "AND kb_year = '" + strYear + "'";

                    c_localdb.DBExcute(sQBuff);

                    txtInvNo.Text = strNewInvNo;

                    c_localdb.DBClose();

                    return; // KeyBuff에 저장된 경우 그 Invoice No를 사용하므로 Exit
                }

                // ini 파일에서 읽어온 Mk, Station 정보를 이용하여 Payment, solditem, suspend 테이블에서 동일 Inv No가 있는지 검색
                ///////////// ini 파일에서 가져온 정보로 변경해주는 Logic 필요
                // TEST용 할당
                //strMkno = GstrMkno;
                //strStno = GstrStno;
                //strYear = "21";
                //GintLocation = 1; // 밴쿠버 : 1, 토론토 : 2, 미국 : 3

                sQBuff = "SELECT MAX(colInvNo) AS MaxInvNo " +
                           "FROM hanamart.dbo.tb_Payment " +
                          "WHERE LEN(colInvNo) = 13 " +
                            "AND colInvNo LIKE '" + sInvNoPrefix + "%'";

                lReturn = c_localdb.RsOpen(sQBuff);

                if (lReturn > 0)
                {
                    if (Convert.IsDBNull(c_localdb.rs.Fields["MaxInvNo"].Value))
                    {
                        strMaxInvNo = "0";
                    }
                    else
                    {
                        strMaxInvNo = Convert.ToString(c_localdb.rs.Fields["MaxInvNo"].Value);
                    }
                }

                c_localdb.RsClose();

                sQBuff = "SELECT MAX(tInvNo) AS MaxInvNo " +
                           "FROM hanamart.dbo.tb_SoldItem " +
                          "WHERE LEN(tInvNo) = 13 " +
                            "AND tInvNo LIKE '" + sInvNoPrefix + "%'";

                lReturn = c_localdb.RsOpen(sQBuff);

                if (lReturn > 0)
                {
                    if (Convert.IsDBNull(c_localdb.rs.Fields["MaxInvNo"].Value))
                    {
                        strCompareInvNo = "0";
                    }
                    else
                    {
                        strCompareInvNo = Convert.ToString(c_localdb.rs.Fields["MaxInvNo"].Value);
                    }

                    if (strMaxInvNo.CompareTo(strCompareInvNo) < 0)
                    {
                        strMaxInvNo = strCompareInvNo;
                    }
                }

                c_localdb.RsClose();

                sQBuff = "SELECT MAX(tInvNo) AS MaxInvNo " +
                           "FROM hanamart.dbo.tb_SuspendTrans " +
                          "WHERE LEN(tInvNo) = 13 " +
                            "AND tInvNo LIKE '" + sInvNoPrefix + "%'";

                lReturn = c_localdb.RsOpen(sQBuff);

                if (lReturn > 0)
                {
                    if (Convert.IsDBNull(c_localdb.rs.Fields["MaxInvNo"].Value))
                    {
                        strCompareInvNo = "0";
                    }
                    else
                    {
                        strCompareInvNo = Convert.ToString(c_localdb.rs.Fields["MaxInvNo"].Value);
                    }

                    if (strMaxInvNo.CompareTo(strCompareInvNo) < 0)
                    {
                        strMaxInvNo = strCompareInvNo;
                    }
                }

                c_localdb.RsClose();

                // Local에 Invoice No 조회가 되지 않을 때 서버에서 한번 더 검사
                ////////////// 서버 Constring 부분 추가 필요
                if (strMaxInvNo == "0")
                {
                    lReturn = c_remotedb.DBConnection();

                    if (lReturn < 0)
                    {
                        g_sMessage = string.Format("[{0}] Remote database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_remotedb.error_message);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                        return;
                    }

                    sQBuff = "SELECT MAX(colInvNo) AS MaxInvNo " +
                               "FROM vw_tfCollection " +
                              "WHERE LEN(colInvNo) = 13 " +
                                "AND colInvNo LIKE '" + sInvNoPrefix + "%'";

                    lReturn = c_remotedb.RsOpen(sQBuff);

                    if (lReturn > 0)
                    {
                        if (Convert.IsDBNull(c_remotedb.rs.Fields["MaxInvNo"].Value))
                        {
                            strCompareInvNo = "0";
                        }
                        else
                        {
                            strCompareInvNo = Convert.ToString(c_remotedb.rs.Fields["MaxInvNo"].Value);
                        }

                        if (strMaxInvNo.CompareTo(strCompareInvNo) < 0)
                        {
                            strMaxInvNo = strCompareInvNo;
                        }
                    }

                    c_remotedb.RsClose();

                    sQBuff = "SELECT MAX(tInvNo) AS MaxInvNo " +
                               "FROM vw_tfTran " +
                              "WHERE LEN(tInvNo) = 13 " +
                                "AND tInvNo LIKE '" + sInvNoPrefix + "%'";

                    lReturn = c_remotedb.RsOpen(sQBuff);

                    if (lReturn > 0)
                    {
                        if (Convert.IsDBNull(c_remotedb.rs.Fields["MaxInvNo"].Value))
                        {
                            strCompareInvNo = "0";
                        }
                        else
                        {
                            strCompareInvNo = Convert.ToString(c_remotedb.rs.Fields["MaxInvNo"].Value);
                        }

                        if (strMaxInvNo.CompareTo(strCompareInvNo) < 0)
                        {
                            strMaxInvNo = strCompareInvNo;
                        }
                    }

                    c_remotedb.RsClose();

                    sQBuff = "SELECT MAX(tInvNo) AS MaxInvNo " +
                               "FROM tb_SuspendTrans " +
                              "WHERE LEN(tInvNo) = 13 " +
                                "AND tInvNo LIKE '" + sInvNoPrefix + "%'";

                    lReturn = c_remotedb.RsOpen(sQBuff);

                    if (lReturn > 0)
                    {
                        if (Convert.IsDBNull(c_remotedb.rs.Fields["MaxInvNo"].Value))
                        {
                            strCompareInvNo = "0";
                        }
                        else
                        {
                            strCompareInvNo = Convert.ToString(c_remotedb.rs.Fields["MaxInvNo"].Value);
                        }

                        if (strMaxInvNo.CompareTo(strCompareInvNo) < 0)
                        {
                            strMaxInvNo = strCompareInvNo;
                        }
                    }

                    c_remotedb.RsClose();
                    c_remotedb.DBClose();
                }

                if (strMaxInvNo == "0")
                {
                    strNewInvNo = sInvNoPrefix + String.Format("{0:D7}", 1);
                }
                else
                {
                    strNewInvNo = String.Format("{0:D13}", Convert.ToInt64(strMaxInvNo) + 1);
                }

                txtInvNo.Text = strNewInvNo;
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

        private void AddProdItem(string strProdID, double dQty)
        {
            string sQBuff = string.Empty;
            long lReturn = 0;
            
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            OrderInfo oiOrderItem;
            OrderAmount oaCalcAmount;

            string strProdAmount, strTax1Amount, strTax2Amount, strTax3Amount, strProdDeposit, strProdDepositAmount, strProdCrf, strProdCrfAmount;
            string strEBTAmount, strEBTTax;
            
            int iPromoType = 0;
            bool blBarcodePrice = false;
            double dblBarcodePrice = 0;
            string strEcofee = "0";
            int iTemptID_Number = 0;
            string strAdultAge = "0";
            string strSAdultAge = "0";

            int iCurTime = 0;

            oiOrderItem.sInvNo = txtInvNo.Text.Trim();
            if ((strProdID.Substring(0, 2) == "20" || strProdID.Substring(0, 2) == "23") && strProdID.Length == 12)
            {
                oiOrderItem.sProdId = strProdID.Substring(0, 6);
                dblBarcodePrice = Convert.ToDouble(strProdID.Substring(7, 4)) / 100;

                if (dblBarcodePrice == 0)
                {
                    blBarcodePrice = false;
                }
                else
                {
                    blBarcodePrice = true;
                }
            }
            else
            {
                oiOrderItem.sProdId = strProdID.Trim();
                blBarcodePrice = false;
            }
            oiOrderItem.sCurDate = DateTime.Now.ToString("yyyy-MM-dd");
            oiOrderItem.sCurTime = DateTime.Now.ToString("h:mm:ss tt");
            //if (strProdID == "9995")
            //{
            //    oiOrderItem.dQty = Convert.ToDouble(lbBagCount.Text == "" ? "1" : lbBagCount.Text.Trim());
            //}
            //else
            //{
            //    //oiOrderItem.dQty = 1;
            //    oiOrderItem.dQty = dQty;
            //}

            oiOrderItem.dQty = dQty;

            if (GblMemberEarn == true && GdblTotalDCRate > 0)
            {
                oiOrderItem.dDCRate = GdblTotalDCRate;
            }
            else
            {
                oiOrderItem.dDCRate = 0;
            }

            if (oiOrderItem.sProdId.CompareTo("") == 0)
            {
                // Customized Message 창 생성 후 대체 예정
                MessageBox.Show("Please check product code!!");

                return;
            }

            try
            {
                lReturn = c_localdb.DBConnection();

                if (lReturn < 0)
                {
                    g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    return;
                }

                // 입력된 상품코드(UPC) 검색하여 존재 여부 확인.
                if (GintLocation == 1)                                          // 벤쿠버, 토론토인 경우
                {
                    //sQBuff = "SELECT promoPrice, isnull(promoSdate, '2020-01-01') as promoSdate, isnull(promoEdate, '2020-01-01') as promoEdate, prodOUprice, prodTax, prodDeposit, prodCrf, " +
                    //            "(CASE WHEN prodPromo = '' THEN '0' ELSE prodPromo END) AS prodPromo, prodUnit, CASE WHEN prodecid IS NULL or prodecid = '' THEN '0' ELSE prodecid END as prodecid, " +
                    //            "ISNULL(tx_tax1,'0') AS tx_tax1, ISNULL(tx_tax2,'0') AS tx_tax2, ISNULL(tx_tax3,'0') AS tx_tax3, ISNULL(pFood,'0') AS pFood, ISNULL(pAdult,'0') AS pAdult " +
                    //       "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.tb_Tax ON tx_cd = prodTax AND prodTax != '' " +
                    //       "LEFT JOIN hanamart.dbo.mfPtype ON pType = prodType " +
                    //      "WHERE prodId = '" + oiOrderItem.sProdId + "'";

                    sQBuff = "SELECT promoPrice, isnull(promoSdate, '2020-01-01') as promoSdate, isnull(promoEdate, '2020-01-01') as promoEdate, prodWprice, isnull(prodImportDate, '2020-01-01') as prodImportDate, isnull(prodEdate, '2020-01-01') as prodEdate, prodOUprice, prodTax, prodDeposit, prodCrf, " +
                                "(CASE WHEN prodPromo = '' THEN '0' ELSE prodPromo END) AS prodPromo, (CASE WHEN prodMemberPromoQty = '' THEN '0' ELSE prodMemberPromoQty END) AS prodMemberPromoQty, prodUnit, CASE WHEN prodecid IS NULL or prodecid = '' THEN '0' ELSE prodecid END as prodecid, pp_promotioncd, prodVoid, " +
                                "ISNULL(tx_tax1,'0') AS tx_tax1, ISNULL(tx_tax2,'0') AS tx_tax2, ISNULL(tx_tax3,'0') AS tx_tax3, ISNULL(pFood,'0') AS pFood, ISNULL(pAdult,'0') AS pAdult, " +
                                "ISNULL(produsehour,'0') AS produsehour, ISNULL(prodbeginhour,'1899-12-30 00:00:00') AS prodbeginhour, ISNULL(prodendhour,'1899-12-30 00:00:00') AS prodendhour, ISNULL(prodHourDCR,'0') AS prodHourDCR " +
                           "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.tb_Tax ON tx_cd = prodTax AND prodTax != '' " +
                           "LEFT JOIN hanamart.dbo.mfPtype ON pType = prodType " +
                          "WHERE prodId = '" + oiOrderItem.sProdId + "'";
                }
                else if (GintLocation == 2)                          // 토론토 인 경우
                {
                    sQBuff = "SELECT promoPrice, isnull(promoSdate, '2020-01-01') as promoSdate, isnull(promoEdate, '2020-01-01') as promoEdate, prodOUprice, prodTax, prodDeposit, " +
                                "(CASE WHEN prodPromo = '' THEN '0' ELSE prodPromo END) AS prodPromo, prodUnit, CASE WHEN prodecid IS NULL or prodecid = '' THEN '0' ELSE prodecid END as prodecid, pp_promotioncd, " +
                                "ISNULL(tx_tax1,'0') AS tx_tax1, ISNULL(tx_tax2,'0') AS tx_tax2, ISNULL(tx_tax3,'0') AS tx_tax3, ISNULL(pFood,'0') AS pFood, ISNULL(pAdult,'0') AS pAdult, ISNULL(pSAdult,'0') AS pSAdult, " +
                                "ISNULL(produsehour,'0') AS produsehour, ISNULL(prodbeginhour,'1899-12-30 00:00:00') AS prodbeginhour, ISNULL(prodendhour,'1899-12-30 00:00:00') AS prodendhour, ISNULL(prodHourDCR,'0') AS prodHourDCR " +
                           "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.tb_Tax ON tx_cd = prodTax AND prodTax != '' " +
                           "LEFT JOIN hanamart.dbo.mfPtype ON pType = prodType " +
                          "WHERE prodId = '" + oiOrderItem.sProdId + "'";
                }

                else if (GintLocation == 3)                          // 미국 인 경우
                {
                    //sQBuff = "SELECT promoPrice, isnull(promoSdate, '2020-01-01') as promoSdate, isnull(promoEdate, '2020-01-01') as promoEdate, prodOUprice, prodTax, prodDeposit, " +
                    //            "(CASE WHEN prodPromo = '' THEN '0' ELSE prodPromo END) AS prodPromo, prodUnit, prodcap, prodcapunit, prodPackQty, " +
                    //            "ISNULL(tx_gst,'0') AS tx_tax1, ISNULL(tx_pst,'0') AS tx_tax2, ISNULL(prodFoodStamp,'0') AS prodFoodStamp, ISNULL(pAdult,'0') AS pAdult, pTax " +
                    //       "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.tb_Tax ON tx_cd = prodTax AND prodTax != '' " +
                    //       "LEFT JOIN hanamart.dbo.mfPtype ON pType = prodType " +
                    //      "WHERE prodId = '" + oiOrderItem.sProdId + "'";

                    sQBuff = "SELECT promoPrice, isnull(promoSdate, '2020-01-01') as promoSdate, isnull(promoEdate, '2020-01-01') as promoEdate, prodWprice, isnull(prodImportDate, '2020-01-01') as prodImportDate, isnull(prodEdate, '2020-01-01') as prodEdate, prodOUprice, prodDeposit, " +
                             "(CASE WHEN prodPromo = '' THEN '0' ELSE prodPromo END) AS prodPromo, (CASE WHEN prodMemberPromoQty = '' THEN '0' ELSE prodMemberPromoQty END) AS prodMemberPromoQty, prodUnit, prodcap, prodcapunit, prodPackQty, prodVoid, ISNULL(pp_promotioncd,'') AS pp_promotioncd, " +
                             "(CASE WHEN prodTax = '' THEN pTax ELSE prodTax END) AS prodTax, " +
                             "ISNULL(tx_gst,'0') AS tx_tax1, ISNULL(tx_pst,'0') AS tx_tax2, ISNULL(pFood,'0') AS prodFoodStamp, ISNULL(pAdult,'0') AS pAdult " +
                             "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.mfPtype ON pType = prodType LEFT JOIN hanamart.dbo.tb_Tax ON tx_cd = pTax AND pTax != '' " +
                             "WHERE prodId = '" + oiOrderItem.sProdId + "'";
                }

                c_colib.cWriteLogs(g_sProcessor, sQBuff);       //for dubb

                lReturn = c_localdb.RsOpen(sQBuff);

                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

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
                        oiOrderItem.dProdPromo = Convert.ToDouble(c_localdb.rs.Fields["prodPromo"].Value);
                        oiOrderItem.dProdMemberPromoQty = Convert.ToDouble(c_localdb.rs.Fields["prodMemberPromoQty"].Value);        // Membership Promotion Qty 

                        if (g_ManualKeyInItem != true)                   // Manual로 Price를 입력한 경우가 아닌 경우.
                        {
                            oiOrderItem.dProdRegPrice = Convert.ToDouble(c_localdb.rs.Fields["prodOUprice"].Value);
                            oiOrderItem.sProdTax = Convert.ToString(c_localdb.rs.Fields["prodTax"].Value);
                            oiOrderItem.dTaxRate1 = Convert.ToDouble(c_localdb.rs.Fields["tx_tax1"].Value);
                            oiOrderItem.dTaxRate2 = Convert.ToDouble(c_localdb.rs.Fields["tx_tax2"].Value);
                        }
                        else                                            // Manual로 Price를 입력한 경우.
                        {
                            oiOrderItem.dProdRegPrice = g_ManualKeyInItemPrice;
                            if(swManulETCKey_TAX.Value)                     // Manual로 Tax 선택 확인.
                            {
                                oiOrderItem.sProdTax = Convert.ToString(c_localdb.rs.Fields["prodTax"].Value);
                                
                                if (oiOrderItem.sProdTax == "" || oiOrderItem.sProdTax == "0")                  // Original Product Tax Code가 0인 것.
                                {
                                    oiOrderItem.sProdTax = GetNormalTaxInfo().sProdTax;                                    
                                    oiOrderItem.dTaxRate1 = GetNormalTaxInfo().dTaxRate1;
                                    oiOrderItem.dTaxRate2 = GetNormalTaxInfo().dTaxRate2;
                                }
                                else
                                {
                                    oiOrderItem.dTaxRate1 = Convert.ToDouble(c_localdb.rs.Fields["tx_tax1"].Value);
                                    oiOrderItem.dTaxRate2 = Convert.ToDouble(c_localdb.rs.Fields["tx_tax2"].Value);
                                }
                            }
                            else
                            {
                                oiOrderItem.sProdTax = "";
                                oiOrderItem.dTaxRate1 = 0;
                                oiOrderItem.dTaxRate2 = 0;
                            }
                            g_ManualKeyInItem = false;
                        }
                        
                        oiOrderItem.dProdPromoPrice = Convert.ToDouble(c_localdb.rs.Fields["PromoPrice"].Value);
                        //oiOrderItem.sProdTax = Convert.ToString(c_localdb.rs.Fields["prodTax"].Value);
                        //oiOrderItem.dTaxRate1 = Convert.ToDouble(c_localdb.rs.Fields["tx_tax1"].Value);
                        //oiOrderItem.dTaxRate2 = Convert.ToDouble(c_localdb.rs.Fields["tx_tax2"].Value);
                        oiOrderItem.sPromoSdate = Convert.ToString(c_localdb.rs.Fields["promoSdate"].Value);
                        oiOrderItem.sPromoEdate = Convert.ToString(c_localdb.rs.Fields["promoEdate"].Value);
                        oiOrderItem.dProdDeposit = Convert.ToDouble(c_localdb.rs.Fields["prodDeposit"].Value);
                        oiOrderItem.sProdUnit = (Convert.ToString(c_localdb.rs.Fields["prodUnit"].Value)).Trim();               // Punit 값에 뒤쪽에 Space 들어간부분 제거.

                        oiOrderItem.dProdWprice = Convert.ToDouble(c_localdb.rs.Fields["ProdWprice"].Value);
                        oiOrderItem.sProdImportDate = Convert.ToString(c_localdb.rs.Fields["ProdImportDate"].Value);
                        oiOrderItem.sProdEdate = Convert.ToString(c_localdb.rs.Fields["ProdEdate"].Value);

                        if (GintLocation == 3)               // 미국 인 경우
                        {
                            oiOrderItem.dCap = Convert.ToDouble(c_localdb.rs.Fields["prodcap"].Value);
                            oiOrderItem.sCapUnit = Convert.ToString(c_localdb.rs.Fields["prodcapunit"].Value);
                            oiOrderItem.iPackQty = Convert.ToInt32(c_localdb.rs.Fields["prodPackQty"].Value);
                            c_colib.cWriteLogs(g_sProcessor, sQBuff);       //for dubb
                        }
                        else
                        {
                            oiOrderItem.dCap = 0;
                            oiOrderItem.sCapUnit = "";
                            oiOrderItem.iPackQty = 0;
                        }

                        // Item이 Void 인 경우 리턴
                        if(GintLocation == 3)
                        {
                            if (Convert.ToInt32(c_localdb.rs.Fields["prodVoid"].Value) == 1)                     // Void 된 상품이면,
                            {
                                if (g_HelpModeOn == true)
                                {
                                    DisplayErrorMessageBox("ITEM SCAN", "Item Information is void.\nPlease check this item. \n\n Barcode : " + strProdID, 1, sMethod);
                                }
                                else
                                {
                                    lbHelpMessage3.Text = "Void Item Information";
                                    btnHelpExit.Visible = false;
                                    ProcessEntering_HelpMode();
                                }

                                g_sMessage = string.Format("[{0}] Item Information is void. (error code: {1}).", sMethod, oiOrderItem.sProdId);
                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                                c_localdb.RsClose();

                                return;
                            }
                        }

                        // Item 가격이 없는 경우 리턴
                        if(oiOrderItem.dProdRegPrice <= 0 && blBarcodePrice == false)
                        {
                            if (g_HelpModeOn == true)
                            {
                                DisplayErrorMessageBox("ITEM SCAN", "No Item Price Information.\nPlease check this item. \n\n Barcode : " + strProdID, 1, sMethod);
                            }
                            else
                            {
                                lbHelpMessage3.Text = "No Item Price";
                                btnHelpExit.Visible = false;
                                ProcessEntering_HelpMode();
                            }

                            g_sMessage = string.Format("[{0}] No Item Price Information (error code: {1}).", sMethod, oiOrderItem.sProdId);
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                            c_localdb.RsClose();

                            return;
                        }

                        if (GintLocation == 1 || GintLocation == 2)    // 벤쿠버와 토론토 인 경우
                        {
                            if(GintLocation == 1)
                            {
                                oiOrderItem.dProdCrf = Convert.ToDouble(c_localdb.rs.Fields["prodCrf"].Value);
                            }
                            else
                            {
                                oiOrderItem.dProdCrf = 0;
                            }
                            
                            oiOrderItem.sPromotioncd = Convert.ToString(c_localdb.rs.Fields["pp_promotioncd"].Value);

                            oiOrderItem.sProdecid = Convert.ToString(c_localdb.rs.Fields["prodecid"].Value);
                            oiOrderItem.dTaxRate3 = Convert.ToInt32(c_localdb.rs.Fields["tx_tax3"].Value);

                            //oiOrderItem.sCurTime = DateTime.Now.ToString("HHmmss");

                            oiOrderItem.iProdusehour = Convert.ToInt32(c_localdb.rs.Fields["produsehour"].Value);
                            DateTime dtBeginHour = DateTime.Parse(Convert.ToString(c_localdb.rs.Fields["prodbeginhour"].Value));
                            oiOrderItem.sProdBeginHour = dtBeginHour.ToString("HHmmss");
                            DateTime dtEndHour = DateTime.Parse(Convert.ToString(c_localdb.rs.Fields["prodendhour"].Value));
                            oiOrderItem.sProdEndHour = dtEndHour.ToString("HHmmss");
                            oiOrderItem.sProdHourDCR = Convert.ToString(c_localdb.rs.Fields["prodHourDCR"].Value);
                        }
                        else                                         // 미국인 경우
                        {
                            oiOrderItem.dProdCrf = 0;
                            oiOrderItem.sPromotioncd = Convert.ToString(c_localdb.rs.Fields["pp_promotioncd"].Value);

                            oiOrderItem.sProdecid = "0";
                            oiOrderItem.dTaxRate3 = 0;

                            //oiOrderItem.sCurTime = "";
                            oiOrderItem.iProdusehour = 0;
                            oiOrderItem.sProdBeginHour = "";
                            oiOrderItem.sProdEndHour = "";
                            oiOrderItem.sProdHourDCR = "";
                        }

                        //if(GintLocation != 3)                   // 벤쿠버, 토론토 인경우만
                        //{
                        //    oiOrderItem.sProdecid = Convert.ToString(c_localdb.rs.Fields["prodecid"].Value);
                        //    oiOrderItem.iTaxRate3 = Convert.ToInt32(c_localdb.rs.Fields["tx_tax3"].Value);

                        //    //oiOrderItem.sCurTime = DateTime.Now.ToString("HHmmss");
                            
                        //    oiOrderItem.iProdusehour = Convert.ToInt32(c_localdb.rs.Fields["produsehour"].Value);
                        //    DateTime dtBeginHour = DateTime.Parse(Convert.ToString(c_localdb.rs.Fields["prodbeginhour"].Value));
                        //    oiOrderItem.sProdBeginHour = dtBeginHour.ToString("HHmmss");
                        //    DateTime dtEndHour = DateTime.Parse(Convert.ToString(c_localdb.rs.Fields["prodendhour"].Value));
                        //    oiOrderItem.sProdEndHour = dtEndHour.ToString("HHmmss");
                        //    oiOrderItem.sProdHourDCR = Convert.ToString(c_localdb.rs.Fields["prodHourDCR"].Value);
                        //}
                        //else                                    // 미국 인경우
                        //{
                        //    oiOrderItem.sProdecid = "0";
                        //    oiOrderItem.iTaxRate3 = 0;

                        //    oiOrderItem.sCurTime = "";
                        //    oiOrderItem.iProdusehour = 0;
                        //    oiOrderItem.sProdBeginHour = "";
                        //    oiOrderItem.sProdEndHour = "";
                        //    oiOrderItem.sProdHourDCR = "";
                        //}

                        // EBT                        
                        if (GintLocation == 3)
                        {
                            oiOrderItem.sFoodStamp = Convert.ToString(Convert.ToInt32(c_localdb.rs.Fields["prodFoodStamp"].Value));
                        }
                        else
                        {
                            oiOrderItem.sFoodStamp = Convert.ToString(Convert.ToInt32(c_localdb.rs.Fields["pFood"].Value));
                        }
                          

                        // Adult Check
                        strAdultAge = (Convert.ToString(c_localdb.rs.Fields["pAdult"].Value)).Trim();
                        if(GintLocation == 2)                   // 토론토 인 경우
                        {
                            strSAdultAge = (Convert.ToString(c_localdb.rs.Fields["pSAdult"].Value)).Trim();
                        }
                        
                        //AGE Check
                        if(GintLocation == 3)               // 미국 인 경우
                        {
                            c_colib.cWriteLogs(g_sProcessor, "AgeCheck Start");
                            if (strAdultAge != "" && strAdultAge != "0" && g_CertifiedAdult == false)           // Need Adult Check Product
                            {
                                g_CertifiedAdultReady = true;
                                g_CertifiedAdult = false;

                                g_iAdultLimit = Convert.ToInt16(strAdultAge);
                                btnHelpExit.Visible = false;
                                lbHelpMessage3.Text = "Need Age Check";

                                g_strAgeCheckforProdID = strProdID;

                                ProcessEntering_HelpMode();

                                g_sMessage = string.Format("[{0}] Adult Product scanned. (error code: {1}).", sMethod, oiOrderItem.sProdId);
                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                                c_localdb.RsClose();

                                return;
                            }
                            else if (g_CertifiedAdult == true && g_iStoredAdultLimit < Convert.ToInt16(strAdultAge)) // Age Check 된 Limit 나이보다 더 높은 나이 Limit을 가진 Item이 입력되었을때.
                            {
                                g_CertifiedAdultReady = true;
                                g_iAdultLimit = Convert.ToInt16(strAdultAge);
                                btnHelpExit.Visible = false;
                                lbHelpMessage3.Text = "Need Age Check";

                                g_strAgeCheckforProdID = strProdID;

                                ProcessEntering_HelpMode();

                                g_sMessage = string.Format("[{0}] Adult Product scanned. (error code: {1}).", sMethod, oiOrderItem.sProdId);
                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                                c_localdb.RsClose();

                                return;
                            }
                            else                                                        // Certified Adult True
                            {

                            }
                            c_colib.cWriteLogs(g_sProcessor, "AgeCheck End");
                        }
                        else if(GintLocation == 2)                  // 토론토 인 경우
                        {
                            if (strSAdultAge != "" && strSAdultAge == "1" && g_CertifiedAdult == false)           // Need Adult Check Product
                            {
                                g_CertifiedAdultReady = true;
                                g_CertifiedAdult = false;

                                //g_iAdultLimit = Convert.ToInt16(strSAdultAge);
                                btnHelpExit.Visible = false;

                                g_strAgeCheckforProdID = strProdID;

                                ProcessEntering_HelpMode();

                                g_sMessage = string.Format("[{0}] Adult Product scanned. (error code: {1}).", sMethod, oiOrderItem.sProdId);
                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                                c_localdb.RsClose();

                                return;
                            }
                            //else if (g_CertifiedAdult == true && g_iStoredAdultLimit < Convert.ToInt16(strAdultAge)) // Age Check 된 Limit 나이보다 더 높은 나이 Limit을 가진 Item이 입력되었을때.
                            //{
                            //    g_CertifiedAdultReady = true;
                            //    //g_iAdultLimit = Convert.ToInt16(strAdultAge);
                            //    btnHelpExit.Visible = false;

                            //    g_strAgeCheckforProdID = strProdID;

                            //    ProcessEntering_HelpMode();

                            //    g_sMessage = string.Format("[{0}] Adult Product scanned. (error code: {1}).", sMethod, oiOrderItem.sProdId);
                            //    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                            //    c_localdb.RsClose();

                            //    return;
                            //}
                            else                                                        // Certified Adult True
                            {

                            }
                        }
                    }
                    else
                    {
                        if(g_HelpModeOn == true)
                        {
                            DisplayErrorMessageBox("ITEM SCAN", "Cannot find the product.\nPlease check this item. \n\n Barcode : " + strProdID, 1, sMethod);
                        }
                        else
                        {
                            if(strProdID == g_strManagerKey)
                            {
                                // 일반 화면에서 Help Mode가 아닌 상태에서 매니져 키 스캔해도 바로 Help Mode들어가도록 수정.
                                g_HelpModeReady = true;
                                ProcessEntryCode(g_strManagerKey, 0);
                            }
                            else
                            {
                                btnHelpExit.Visible = false;
                                ProcessEntering_HelpMode();
                            }                            
                        }
                        
                        g_sMessage = string.Format("[{0}] Product not found (error code: {1}).", sMethod, oiOrderItem.sProdId);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                        c_localdb.RsClose();

                        return;
                    }
                }

                c_localdb.RsClose();

                /* Ecofee */
                if (oiOrderItem.sProdecid != "0")
                {
                    strEcofee = GetEhfCategory(oiOrderItem.sProdecid);
                    
                }

                /* Promotion Check */
                //oiOrderItem.dProdPromo 
                //oiOrderItem.dProdMemberPromoQty
                
                if (GchkMember == true && oiOrderItem.dProdMemberPromoQty > 0)                 // Membership이 Scan 되어 있는 경우
                {
                    iPromoType = checkPromo(oiOrderItem.sProdId, oiOrderItem.dProdMemberPromoQty.ToString(), oiOrderItem.sProdImportDate, oiOrderItem.sProdEdate, oiOrderItem.dQty.ToString(), oiOrderItem.dProdDeposit, oiOrderItem.dProdCrf);
                }

                if (iPromoType == 0 && oiOrderItem.dProdPromo > 0)
                {
                    iPromoType = checkPromo(oiOrderItem.sProdId, oiOrderItem.dProdPromo.ToString(), oiOrderItem.sPromoSdate, oiOrderItem.sPromoEdate, oiOrderItem.dQty.ToString(), oiOrderItem.dProdDeposit, oiOrderItem.dProdCrf);
                }                   

                //iPromoType = checkPromo(oiOrderItem.sProdId, oiOrderItem.dProdPromo.ToString(), oiOrderItem.sPromoSdate, oiOrderItem.sPromoEdate, oiOrderItem.dQty.ToString(), oiOrderItem.dProdDeposit, oiOrderItem.dProdCrf);

                /* 가격 & Tax 계산 */
                if (iPromoType == 1) // 일반 Date Range 프로모션
                {
                    if (blBarcodePrice)
                    {
                        oaCalcAmount.dProdPrice = dblBarcodePrice;
                    }
                    else
                    {
                        if (GchkMember == true && oiOrderItem.dProdMemberPromoQty > 0)                 // Membership이 Scan 되어 있는 경우
                        {
                            oaCalcAmount.dProdPrice = oiOrderItem.dProdWprice;
                        }
                        else
                        {
                            oaCalcAmount.dProdPrice = oiOrderItem.dProdPromoPrice;
                        }                            
                    }
                    
                    if (oiOrderItem.sProdUnit.ToUpper().Trim() == "LB" || oiOrderItem.sProdUnit.ToUpper().Trim() == "KG")
                    {
                        oiOrderItem.dPromoQty = 1;
                    }
                    else
                    {
                        if(oiOrderItem.dProdPromo != 1)
                        {
                            oiOrderItem.dQty = iPromoType;                  // 프로모션에 해당되는 갯수.
                        }
                        oiOrderItem.dPromoQty = iPromoType;
                    }
                    oaCalcAmount.dProdAmount = (oaCalcAmount.dProdPrice * oiOrderItem.dQty);
                }

                else if (iPromoType > 1 ) // 멀티 수량 프로모션
                {
                    oaCalcAmount.dProdPrice = oiOrderItem.dProdPromoPrice;
                    oiOrderItem.dQty = iPromoType;                                                      // 프로모션이 적용된 실제 아이템의 수량.
                    oiOrderItem.dPromoQty = iPromoType / Convert.ToInt16(oiOrderItem.dProdPromo);       // 프로모션 가격에 곱해져야 할 갯수.
                    
                    oaCalcAmount.dProdAmount = (oaCalcAmount.dProdPrice * oiOrderItem.dPromoQty);
                }
                else // 일반 상품
                {
                    double dblWeight = 0;

                    iCurTime = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));

                    if (oiOrderItem.iProdusehour == 1 
                        && Convert.ToInt32(oiOrderItem.sProdBeginHour) <= iCurTime
                        && Convert.ToInt32(oiOrderItem.sProdEndHour) >= iCurTime)                       // 시간별 DC Rate 적용룰 사용하는 상품인 경우. 현재 시간과 비교.
                    {
                        if (blBarcodePrice == true)
                        {
                            oaCalcAmount.dProdPrice = dblBarcodePrice - (dblBarcodePrice * Convert.ToInt32(oiOrderItem.sProdHourDCR) / 100);
                        }
                        else
                        {
                            oaCalcAmount.dProdPrice = oiOrderItem.dProdRegPrice - (oiOrderItem.dProdRegPrice * Convert.ToInt32(oiOrderItem.sProdHourDCR) / 100);
                        }
                    }
                    else
                    {
                        if (blBarcodePrice == true)
                        {
                            oaCalcAmount.dProdPrice = dblBarcodePrice;
                        }
                        else
                        {
                            oaCalcAmount.dProdPrice = oiOrderItem.dProdRegPrice;
                        }
                    }
                    
                    dblWeight = Convert.ToDouble(lbWeightCS.Text);
                    //dblWeight = 0.69;
                    if ((oiOrderItem.sProdUnit.ToUpper().Trim() == "LB" || oiOrderItem.sProdUnit.ToUpper().Trim() == "KG") && blBarcodePrice == false)
                    {
                        if (dblWeight != 0)
                        {
                            oiOrderItem.dQty = dblWeight;
                        }
                        else
                        {
                            //MessageBox.Show("Check Scale Weight");
                            DisplayErrorMessageBox("Search Item", "Check Scale Weight", 1, sMethod);

                            g_sMessage = string.Format("[{0}] Scale Weight Data Error (error code: {1})\n[{0}] {2} -- No blBarcodeprice", sMethod, lReturn.ToString(), c_localdb.error_message);
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                            c_localdb.DBClose();

                            return;
                        }
                    }
                    oiOrderItem.dPromoQty = 0;
                    oaCalcAmount.dProdAmount = (oaCalcAmount.dProdPrice * oiOrderItem.dQty);
                }

                if(gbSearchBox.Visible == true)
                {
                    gbSearchBox.Visible = false;
                }

                // Tax 계산. 미국 Hard Liquor Tax 적용 필요.

                if(GintLocation == 3)                           // 미국 인 경우
                {
                    if (oiOrderItem.sProdTax == "5" || oiOrderItem.sProdTax == "6")             // Hard Liquor 일 경우.
                    {
                        // GST 계산.
                        oaCalcAmount.dTax1Amount = oaCalcAmount.dProdAmount * oiOrderItem.dTaxRate1 / 100;

                        // ML, LT가 아닌 경우, Cap이 0보다 작인 경우, PackQty 0보다 작은 경우 Help 상품 정보 이상메세지 출력.
                        
                        // Cap Unit이 ML 인 경우 계산 
                        if (oiOrderItem.sCapUnit == "ML")
                        {
                            oiOrderItem.dCap = oiOrderItem.dCap / 1000;
                        }
                        // SLT Tax 계산.
                        oaCalcAmount.dTax2Amount = (oiOrderItem.dCap * oiOrderItem.iPackQty * oiOrderItem.dTaxRate2);  // SLTAX 계산
                    }
                    else
                    {
                        oaCalcAmount.dTax1Amount = oaCalcAmount.dProdAmount * oiOrderItem.dTaxRate1 / 100;  // GST
                        oaCalcAmount.dTax2Amount = oaCalcAmount.dProdAmount * oiOrderItem.dTaxRate2 / 100;  // PST                        
                    }

                    oaCalcAmount.dTax3Amount = oaCalcAmount.dProdAmount * oiOrderItem.dTaxRate3 / 100;  // HST
                    oaCalcAmount.dProdDeposit = oiOrderItem.dProdDeposit * oiOrderItem.dQty;
                    oaCalcAmount.dProdEcofee = Convert.ToDouble(strEcofee) * oiOrderItem.dQty;
                }
                else
                {
                    oaCalcAmount.dTax1Amount = oaCalcAmount.dProdAmount * oiOrderItem.dTaxRate1 / 100;  // GST
                    oaCalcAmount.dTax2Amount = oaCalcAmount.dProdAmount * oiOrderItem.dTaxRate2 / 100;  // PST
                    oaCalcAmount.dTax3Amount = oaCalcAmount.dProdAmount * oiOrderItem.dTaxRate3 / 100;  // HST
                    oaCalcAmount.dProdDeposit = oiOrderItem.dProdDeposit * oiOrderItem.dQty;
                    oaCalcAmount.dProdEcofee = Convert.ToDouble(strEcofee) * oiOrderItem.dQty;
                }
               
                if (GintLocation == 1)    // 벤쿠버 일때
                {
                    oaCalcAmount.dProdCrf = oiOrderItem.dProdCrf * oiOrderItem.dQty;
                }
                else
                {
                    oaCalcAmount.dProdCrf = 0;
                }
                
                // EBT
                if (oiOrderItem.sFoodStamp == "1")
                {
                    oaCalcAmount.dEBTamount = oaCalcAmount.dProdAmount;
                    oaCalcAmount.dEBTTax1Amount = oaCalcAmount.dTax1Amount;
                    oaCalcAmount.dEBTTax2Amount = oaCalcAmount.dTax2Amount;
                    oaCalcAmount.dEBTTax3Amount = oaCalcAmount.dTax3Amount;

                    GdblEBTAmountTotal += oaCalcAmount.dEBTamount;
                    GdblEBTTax1Total += oaCalcAmount.dEBTTax1Amount;
                    GdblEBTTax2Total += oaCalcAmount.dEBTTax2Amount;
                    GdblEBTTax3Total += oaCalcAmount.dEBTTax3Amount;

                }
                else
                {
                    oaCalcAmount.dEBTamount = 0;
                    oaCalcAmount.dEBTTax1Amount = 0;
                    oaCalcAmount.dEBTTax2Amount = 0;
                    oaCalcAmount.dEBTTax3Amount = 0;
                }

                // EBT Amount
                strEBTAmount = String.Format("{0:#,##0.00}", oaCalcAmount.dEBTamount, 2);
                strEBTTax    = String.Format("{0:#,##0.00}", (oaCalcAmount.dEBTTax1Amount + oaCalcAmount.dEBTTax2Amount + oaCalcAmount.dEBTTax3Amount), 2);


                strProdAmount = String.Format("{0:#,##0.00}", oaCalcAmount.dProdAmount, 2);
                strProdAmount = strProdAmount.Replace(",", "");                             // 천단위 콤마 삭제.

                strTax1Amount = String.Format("{0:#,##0.00}", oaCalcAmount.dTax1Amount, 2);
                strTax2Amount = String.Format("{0:#,##0.00}", oaCalcAmount.dTax2Amount, 2);
                strTax3Amount = String.Format("{0:#,##0.00}", oaCalcAmount.dTax3Amount, 2);

                strProdDeposit = String.Format("{0:#,##0.00}", oiOrderItem.dProdDeposit, 2);
                strProdDepositAmount = String.Format("{0:#,##0.00}", oaCalcAmount.dProdDeposit, 2);

                strProdCrf = String.Format("{0:#,##0.00}", oiOrderItem.dProdCrf, 2);
                strProdCrfAmount = String.Format("{0:#,##0.00}", oaCalcAmount.dProdCrf, 2);
                

                /* tb_orderitem Table에 아이템 추가 */

                // 1. 이미 테이블에 등록된 아이템이 있는지 확인 후 Seq No 채번

                sQBuff = "SELECT ISNULL(MAX(tID), 0) + 1 AS seq " +
                           "FROM hanamart.dbo.tb_orderitem " +
                          "WHERE tInvNo = '" + oiOrderItem.sInvNo + "'";

                lReturn = c_localdb.RsOpen(sQBuff);

                if (lReturn == 1)
                {
                    oiOrderItem.iSeq = Convert.ToInt32(c_localdb.rs.Fields["seq"].Value);
                }
                else
                {
                    g_sMessage = string.Format("[{0}] Order sequence data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    c_localdb.DBClose();

                    return;
                }

                c_localdb.RsClose();

                // 2. tb_OrderItem 입력 Query
                if(GintLocation == 1)                               // 벤쿠버 인 경우
                {
                    sQBuff = "INSERT INTO hanamart.dbo.tb_orderitem " +
                              "SELECT '" + oiOrderItem.sInvNo + "', " + oiOrderItem.iSeq.ToString() + ",'" + oiOrderItem.sCurDate + "','" + oiOrderItem.sCurTime + "' , prodId, prodType, ptCode, " +
                                     "ISNULL(prodCat1,0), ISNULL(prodCat2,0), ISNULL(prodCat3,0), ISNULL(prodCat4,0), ISNULL(prodCat5,0), '', " + oiOrderItem.dQty.ToString() + ", '" + oiOrderItem.sProdUnit + "', ";
                }
                else if (GintLocation == 2)                         // 토론토 인 경우
                {
                    sQBuff = "INSERT INTO hanamart.dbo.tb_orderitem " +
                              "SELECT '" + oiOrderItem.sInvNo + "', " + oiOrderItem.iSeq.ToString() + ",'" + oiOrderItem.sCurDate + "','" + oiOrderItem.sCurTime + "' , prodId, prodType, ptCode, " +
                                     "ISNULL(prodCat1,0), ISNULL(prodCat2,0), ISNULL(prodCat3,0), ISNULL(prodCat4,0), ISNULL(prodCat5,0), '" + oiOrderItem.dQty.ToString() + "', '" + oiOrderItem.sProdUnit + "', ";
                }
                else                                                // 미국인 경우
                {
                    sQBuff = "INSERT INTO hanamart.dbo.tb_orderitem " +
                              "SELECT '" + oiOrderItem.sInvNo + "', " + oiOrderItem.iSeq.ToString() + ",'" + oiOrderItem.sCurDate + "','" + oiOrderItem.sCurTime + "' , prodId, prodType, ptCode, " +
                                     "ISNULL(prodCat1,0), ISNULL(prodCat2,0), ISNULL(prodCat3,0), ISNULL(prodCat4,0), ISNULL(prodCat5,0), '" + oiOrderItem.dQty.ToString() + "', '" + oiOrderItem.sProdUnit + "', ";
                }
                
                if (blBarcodePrice == true)
                {
                    sQBuff += strProdAmount + "," + strProdAmount + ",";
                }
                else
                {
                    if (iPromoType >= 1) // 일반 Date Range 프로모션
                    {
                        //sQBuff += "case when prodIUprice = 0 then prodOUprice else prodIUprice end as prodIUprice, prodOUprice, ";
                        if (GchkMember == true && oiOrderItem.dProdMemberPromoQty > 0)                 // Membership이 Scan 되어 있는 경우
                        {
                            sQBuff += oiOrderItem.dProdRegPrice + "," + oiOrderItem.dProdWprice + ",";
                        }
                        else
                        {
                            sQBuff += oiOrderItem.dProdRegPrice + "," + oiOrderItem.dProdPromoPrice + ",";
                        }

                    }
                    else if(oiOrderItem.iProdusehour == 1 
                        && Convert.ToInt32(oiOrderItem.sProdBeginHour) <= iCurTime
                        && Convert.ToInt32(oiOrderItem.sProdEndHour) >= iCurTime)                       // 시간별 DC Rate 적용룰 사용하는 상품인 경우. 현재 시간과 비교.)
                    {
                        sQBuff += strProdAmount + "," + strProdAmount + ",";
                    }
                    else
                    {
                        sQBuff += "case when prodIUprice = 0 then prodOUprice else prodIUprice end as prodIUprice, prodOUprice, ";
                    }
                        
                }

                // EBT - 미국 Table과 컬럼 매치 후 EBT 관련 값 업데이트 필요
                if(GintLocation == 1)                                   // 벤쿠버 인경우
                {
                    //sQBuff += "prodWprice, " + oiOrderItem.dDCRate.ToString() + ", '" + oiOrderItem.dPromoQty.ToString() + "', ISNULL(pp_promotioncd,''), " +
                    //                  strProdAmount + ", " + strTax1Amount + "," + strTax2Amount + "," + strTax3Amount + ", prodTax, '0', '" + c_poscominfo.ui_epno + "', '" + c_poscominfo.si_counternum +
                    //                 "','','','','',prodSupp,'','',0,'',0,'','','' " +
                    //            "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.mfPtype ON prodType = pType " +
                    //           "WHERE prodId = '" + oiOrderItem.sProdId + "'";

                    sQBuff += "prodWprice, " + oiOrderItem.dDCRate.ToString() + ", '" + oiOrderItem.dPromoQty + "', ISNULL(pp_promotioncd,''), " +
                                      strProdAmount + ", " + strTax1Amount + "," + strTax2Amount + "," + strTax3Amount + ", '" + oiOrderItem.sProdTax + "', '" + c_poscominfo.mi_cardno + "', '" + txtEmpNo.Text + "', '" + c_poscominfo.si_counternum +
                                     "','','','','',prodSupp,'', prodId, 0,'',0,'','','' " +
                                "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.mfPtype ON prodType = pType " +
                               "WHERE prodId = '" + oiOrderItem.sProdId + "'";
                }
                else if (GintLocation == 2)                           // 토론토 인경우
                {
                    sQBuff += "prodWprice, " + oiOrderItem.dDCRate.ToString() + ", '" + oiOrderItem.dPromoQty.ToString() + "', " +
                                      strProdAmount + ", " + strTax1Amount + ", " + strTax2Amount + ", " + strTax3Amount + ", prodTax, '" + c_poscominfo.mi_cardno +"', '" + txtEmpNo.Text + "', '" + c_poscominfo.si_counternum +
                                     "','','','', prodSupp,'','" + oiOrderItem.sProdId + "','' , 0, 0, '','','' " +
                                "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.mfPtype ON prodType = pType " +
                               "WHERE prodId = '" + oiOrderItem.sProdId + "'";
                }
                else                                            // 미국 인 경우
                {
                    sQBuff += "prodWprice, " + oiOrderItem.dDCRate.ToString() + ", '" + oiOrderItem.dPromoQty.ToString() + "', " +
                                      strProdAmount + ", " + strTax1Amount + ", " + strTax2Amount + ", '" + oiOrderItem.sProdTax + "', 0," + "0" +", '0', '" + c_poscominfo.mi_cardno + "', '"+ txtEmpNo.Text + "', '" + c_poscominfo.si_counternum +
                                     "','','','', prodSupp,'','" + oiOrderItem.sProdId + "','" + oiOrderItem.sFoodStamp + "', 0, 0, '','','' " +
                                "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.mfPtype ON prodType = pType " +
                               "WHERE prodId = '" + oiOrderItem.sProdId + "'";
                }
                
                if (c_localdb.DBExcute(sQBuff) != 1)
                {
                    g_sMessage = string.Format("[{0}] Item insert failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    c_localdb.DBClose();
                    
                    return;
                }
                else
                {
                    g_sMessage = string.Format("[{0}] ({1} item added ({2}).", sMethod, oiOrderItem.sProdId, c_poscominfo.ui_epno);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                }
                
                // Bottle Deposit 
                if (oiOrderItem.dProdDeposit > 0)
                {
                    string strRelatedid = oiOrderItem.iSeq.ToString();
                    
                    oiOrderItem.iSeq = oiOrderItem.iSeq + 1;
                    if (GintLocation == 1)                                   // 벤쿠버 인경우
                    {
                        sQBuff = "INSERT INTO hanamart.dbo.tb_orderitem " +
                              "SELECT '" + oiOrderItem.sInvNo + "', " + oiOrderItem.iSeq.ToString() + ",'" + oiOrderItem.sCurDate + "','" + oiOrderItem.sCurTime + "' , prodId, prodType, ptCode, " +
                                     "ISNULL(prodCat1,0), ISNULL(prodCat2,0), ISNULL(prodCat3,0), ISNULL(prodCat4,0), ISNULL(prodCat5,0), '', " + oiOrderItem.dQty.ToString() + ", '" + oiOrderItem.sProdUnit + "', " + strProdDeposit + ", " +
                                     strProdDeposit + ", '0', " + oiOrderItem.dDCRate.ToString() + ", '" + oiOrderItem.dPromoQty.ToString() + "', ISNULL(pp_promotioncd,''), " +
                                      strProdDepositAmount + ", '0', '0', '0'" + ", '', '" + c_poscominfo.mi_cardno + "', '" + txtEmpNo.Text + "', '" + c_poscominfo.si_counternum +
                                     "','','','','',prodSupp,'21', prodId, 0,''," + strRelatedid + ",'','','' " +
                                "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.mfPtype ON prodType = pType " +
                               "WHERE prodId = '" + oiOrderItem.sProdId + "'";
                    }
                    else if (GintLocation == 2)                                   // 토론토 인경우
                    {
                        sQBuff = "INSERT INTO hanamart.dbo.tb_orderitem " +
                              "SELECT '" + oiOrderItem.sInvNo + "', " + oiOrderItem.iSeq.ToString() + ",'" + oiOrderItem.sCurDate + "','" + oiOrderItem.sCurTime + "' , prodId, prodType, ptCode, " +
                                     "ISNULL(prodCat1,0), ISNULL(prodCat2,0), ISNULL(prodCat3,0), ISNULL(prodCat4,0), ISNULL(prodCat5,0), " + oiOrderItem.dQty.ToString() + ", '" + oiOrderItem.sProdUnit + "', " + strProdDeposit + ", " +
                                     strProdDeposit + ", '0', " + oiOrderItem.dDCRate.ToString() + ", '" + oiOrderItem.dPromoQty.ToString() + "', " +
                                      strProdDepositAmount + ", '0', '0', '0'" + ", '', '" + c_poscominfo.mi_cardno + "', '" + txtEmpNo.Text + "', '" + c_poscominfo.si_counternum +
                                     "','','','', prodSupp, '21', '', 0, '', " + strRelatedid + ", '', '', '' " +
                                "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.mfPtype ON prodType = pType " +
                               "WHERE prodId = '" + oiOrderItem.sProdId + "'";
                    }
                    else                                                        // 미국 인 경우 
                    {
                        sQBuff = "INSERT INTO hanamart.dbo.tb_orderitem " +
                              "SELECT '" + oiOrderItem.sInvNo + "', " + oiOrderItem.iSeq.ToString() + ",'" + oiOrderItem.sCurDate + "','" + oiOrderItem.sCurTime + "' , prodId, prodType, ptCode, " +
                                     "ISNULL(prodCat1,0), ISNULL(prodCat2,0), ISNULL(prodCat3,0), ISNULL(prodCat4,0), ISNULL(prodCat5,0), '', " + oiOrderItem.dQty.ToString() + ", '" + oiOrderItem.sProdUnit + "', " + strProdDeposit + ", " +
                                     strProdDeposit + ", '0', " + oiOrderItem.dDCRate.ToString() + ", '" + oiOrderItem.dPromoQty.ToString() + "', ISNULL(pp_promotioncd,''), " +
                                      strProdDepositAmount + ", '0', '0', '0'" + ", '', '" + c_poscominfo.mi_cardno + "', '" + txtEmpNo.Text + "', '" + c_poscominfo.si_counternum +
                                     "','','','','',prodSupp,'21','',0,''," + strRelatedid + ",'','','' " +
                                "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.mfPtype ON prodType = pType " +
                               "WHERE prodId = '" + oiOrderItem.sProdId + "'";
                    }


                    if (c_localdb.DBExcute(sQBuff) != 1)
                    {
                        g_sMessage = string.Format("[{0}] Item insert failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                        c_localdb.DBClose();

                        return;
                    }
                    else
                    {
                        g_sMessage = string.Format("[{0}] ({1} item added ({2}).", sMethod, oiOrderItem.sProdId, c_poscominfo.ui_epno);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                    }
                }

                // CRF
                if (oiOrderItem.dProdCrf > 0)
                {
                    string strRelatedid = (oiOrderItem.iSeq -1).ToString();

                    oiOrderItem.iSeq = oiOrderItem.iSeq + 1;
                    if (GintLocation == 1)                                   // 벤쿠버 인경우
                    {
                        sQBuff = "INSERT INTO hanamart.dbo.tb_orderitem " +
                              "SELECT '" + oiOrderItem.sInvNo + "', " + oiOrderItem.iSeq.ToString() + ",'" + oiOrderItem.sCurDate + "','" + oiOrderItem.sCurTime + "' , prodId, prodType, ptCode, " +
                                     "ISNULL(prodCat1,0), ISNULL(prodCat2,0), ISNULL(prodCat3,0), ISNULL(prodCat4,0), ISNULL(prodCat5,0), '', " + oiOrderItem.dQty.ToString() + ", prodUnit, " + strProdCrf + ", " +
                                     strProdCrf + ", '0', " + oiOrderItem.dDCRate.ToString() + ", '" + oiOrderItem.dPromoQty.ToString() + "', ISNULL(pp_promotioncd,''), " +
                                      strProdCrfAmount + ", '0', '0', '0'" + ", '', '" + c_poscominfo.mi_cardno + "', '" + txtEmpNo.Text + "', '" + c_poscominfo.si_counternum +
                                     "','','','','',prodSupp,'20', prodId, 0,''," + strRelatedid + ",'','','' " +
                                "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.mfPtype ON prodType = pType " +
                               "WHERE prodId = '" + oiOrderItem.sProdId + "'";

                        if (c_localdb.DBExcute(sQBuff) != 1)
                        {
                            g_sMessage = string.Format("[{0}] Item insert failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                            c_localdb.DBClose();

                            return;
                        }
                        else
                        {
                            g_sMessage = string.Format("[{0}] ({1} item added ({2}).", sMethod, oiOrderItem.sProdId, c_poscominfo.ui_epno);
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                        }
                    }
                    //else if (GintLocation == 2)                                   // 토론토 인경우
                    //{
                    //    sQBuff = "INSERT INTO hanamart.dbo.tb_orderitem " +
                    //          "SELECT '" + oiOrderItem.sInvNo + "', " + oiOrderItem.iSeq.ToString() + ",'" + oiOrderItem.sCurDate + "','" + oiOrderItem.sCurTime + "' , prodId, prodType, ptCode, " +
                    //                 "ISNULL(prodCat1,0), ISNULL(prodCat2,0), ISNULL(prodCat3,0), ISNULL(prodCat4,0), ISNULL(prodCat5,0), '', " + oiOrderItem.dQty.ToString() + ", prodUnit, " + strProdDeposit + ", " +
                    //                 strProdDeposit + ", '0', " + oiOrderItem.dDCRate.ToString() + ", '" + oiOrderItem.dPromoQty.ToString() + "', " +
                    //                  strProdDepositAmount + ", '0', '0', '0'" + ", '', '0', '" + c_poscominfo.ui_epno + "', '" + c_poscominfo.si_counternum +
                    //                 "','','','', prodSupp, '21', '', 0, '', " + strRelatedid + ", '', '', '' " +
                    //            "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.mfPtype ON prodType = pType " +
                    //           "WHERE prodId = '" + oiOrderItem.sProdId + "'";
                    //}
                    //else                                                        // 미국 인 경우 
                    //{
                    //    sQBuff = "INSERT INTO hanamart.dbo.tb_orderitem " +
                    //          "SELECT '" + oiOrderItem.sInvNo + "', " + oiOrderItem.iSeq.ToString() + ",'" + oiOrderItem.sCurDate + "','" + oiOrderItem.sCurTime + "' , prodId, prodType, ptCode, " +
                    //                 "ISNULL(prodCat1,0), ISNULL(prodCat2,0), ISNULL(prodCat3,0), ISNULL(prodCat4,0), ISNULL(prodCat5,0), '', " + oiOrderItem.dQty.ToString() + ", prodUnit, " + strProdDeposit + ", " +
                    //                 strProdDeposit + ", '0', " + oiOrderItem.dDCRate.ToString() + ", '" + oiOrderItem.dPromoQty.ToString() + "', ISNULL(pp_promotioncd,''), " +
                    //                  strProdDepositAmount + ", '0', '0', '0'" + ", '', '0', '" + c_poscominfo.ui_epno + "', '" + c_poscominfo.si_counternum +
                    //                 "','','','','',prodSupp,'21','',0,''," + strRelatedid + ",'','','' " +
                    //            "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.mfPtype ON prodType = pType " +
                    //           "WHERE prodId = '" + oiOrderItem.sProdId + "'";
                    //}
                }


                // Eco Fee (환경부담금)
                if (strEcofee != "0")
                {
                    string strRelatedid = oiOrderItem.iSeq.ToString();

                    oiOrderItem.iSeq = oiOrderItem.iSeq + 1;
                    sQBuff = "INSERT INTO hanamart.dbo.tb_orderitem " +
                              "SELECT '" + oiOrderItem.sInvNo + "', " + oiOrderItem.iSeq.ToString() + ",'" + oiOrderItem.sCurDate + "','" + oiOrderItem.sCurTime + "' , prodId, prodType, ptCode, " +
                                     "ISNULL(prodCat1,0), ISNULL(prodCat2,0), ISNULL(prodCat3,0), ISNULL(prodCat4,0), ISNULL(prodCat5,0), '', " + oiOrderItem.dQty.ToString() + ", prodUnit, " + strEcofee + ", " +
                                     strEcofee + ", '0', 0, '" + oiOrderItem.dPromoQty.ToString() + "', ISNULL(pp_promotioncd,''), " +
                                      strEcofee + ", '0', '0', '0'" + ", '', '" + c_poscominfo.mi_cardno + "', '" + txtEmpNo.Text + "', '" + c_poscominfo.si_counternum +
                                     "','','','','',prodSupp,'22', prodId, 0,''," + strRelatedid + ",'','','' " +
                                "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.mfPtype ON prodType = pType " +
                               "WHERE prodId = '" + oiOrderItem.sProdId + "'";

                    if (c_localdb.DBExcute(sQBuff) != 1)
                    {
                        g_sMessage = string.Format("[{0}] Item insert failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                        c_localdb.DBClose();

                        return;
                    }
                    else
                    {
                        g_sMessage = string.Format("[{0}] ({1} item added ({2}).", sMethod, oiOrderItem.sProdId, c_poscominfo.ui_epno);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                    }
                }

                // Mix & Match 체크.

                if (oiOrderItem.sPromotioncd != "")
                {
                    // Check Promo Code
                    if(CheckMixnMatchPromo(oiOrderItem.sInvNo, oiOrderItem.sPromotioncd, oiOrderItem.dTaxRate1, oiOrderItem.dTaxRate2, oiOrderItem.dTaxRate3) == true)      // 조건 True 인 경우
                    {
                        // Mix & Match Promotion Discount 레코드 추가.
                        string strRelatedid = oiOrderItem.iSeq.ToString();

                        oiOrderItem.iSeq = oiOrderItem.iSeq + 1;
                        
                        GMixMatchPrice = GMixMatchPrice * -1;
                        GMixMatchGST = GMixMatchGST * -1;
                        GMixMatchPST = GMixMatchPST * -1;
                        GMixMatchHST = GMixMatchHST * -1;

                        if(GintLocation == 1)                       // 벤쿠버 인 경우
                        {
                            sQBuff = "INSERT INTO hanamart.dbo.tb_orderitem " +
                              "SELECT '" + oiOrderItem.sInvNo + "', " + oiOrderItem.iSeq.ToString() + ",'" + oiOrderItem.sCurDate + "','" + oiOrderItem.sCurTime + "' , '2995200000033', prodType, ptCode, " +
                                     "ISNULL(prodCat1,0), ISNULL(prodCat2,0), ISNULL(prodCat3,0), ISNULL(prodCat4,0), ISNULL(prodCat5,0), '', '1','" + oiOrderItem.sProdUnit + "', " + GMixMatchPrice + ", " +
                                     GMixMatchPrice + ", '0', " + oiOrderItem.dDCRate.ToString() + ", '" + oiOrderItem.dPromoQty.ToString() + "', ISNULL(pp_promotioncd,''), " +
                                      GMixMatchPrice + "," + GMixMatchGST + "," + GMixMatchPST + "," + GMixMatchHST + ", prodTax, '" + c_poscominfo.mi_cardno + "', '" + txtEmpNo.Text + "', '" + c_poscominfo.si_counternum +
                                     "','','','','', prodSupp,'47', ISNULL(pp_promotioncd,''), '0',''," + strRelatedid + ",'','','' " +
                                "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.mfPtype ON prodType = pType " +
                               "WHERE prodId = '" + oiOrderItem.sProdId + "'";
                        }
                        else if(GintLocation == 2)                                   // 토론토 인경우
                        {
                            sQBuff = "INSERT INTO hanamart.dbo.tb_orderitem " +
                                  "SELECT '" + oiOrderItem.sInvNo + "', " + oiOrderItem.iSeq.ToString() + ",'" + oiOrderItem.sCurDate + "','" + oiOrderItem.sCurTime + "' , '2995200000033', prodType, ptCode, " +
                                         "ISNULL(prodCat1,0), ISNULL(prodCat2,0), ISNULL(prodCat3,0), ISNULL(prodCat4,0), ISNULL(prodCat5,0), '1', '" + oiOrderItem.sProdUnit + "', " + GMixMatchPrice + ", " +
                                         GMixMatchPrice + ", '0', " + oiOrderItem.dDCRate.ToString() + ", '" + oiOrderItem.dPromoQty.ToString() + "', " +
                                          GMixMatchPrice + "," + GMixMatchGST + "," + GMixMatchPST + "," + GMixMatchHST + ", prodTax, '" + c_poscominfo.mi_cardno + "', '" + txtEmpNo.Text + "', '" + c_poscominfo.si_counternum +
                                         "','','','', prodSupp, '47', ISNULL(pp_promotioncd,''), 0, '', " + strRelatedid + ", '', '', '' " +
                                    "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.mfPtype ON prodType = pType " +
                                   "WHERE prodId = '" + oiOrderItem.sProdId + "'";
                        }
                        else                                                // 미국 인 경우               
                        {

                        }

                        if (c_localdb.DBExcute(sQBuff) != 1)
                        {
                            g_sMessage = string.Format("[{0}] Mix & Match Discount Item insert failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                            c_localdb.DBClose();

                            GMixMatchPrice = 0;
                            GMixMatchGST = 0;
                            GMixMatchPST = 0;
                            GMixMatchHST = 0;

                            return;
                        }
                        else
                        {
                            g_sMessage = string.Format("[{0}] ({1} Mix & Match Discount Item added ({2}).", sMethod, oiOrderItem.sProdId, c_poscominfo.ui_epno);
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                        }

                        GMixMatchPrice = 0;
                        GMixMatchGST = 0;
                        GMixMatchPST = 0;
                        GMixMatchHST = 0;
                    }

                }



                //// tb.OrderItme 테이블에서 promocd가 있는 경우와 같은거 끼리의 갯수 합계와 promotioncd에 있는 수량과 비교.
                //sQBuff = "SELECT tPromoCode, SUM(tQty) as CurrentOrderCount, SUM(tAmt) as TotalAmount, pm_desc, pm_qty, pm_price " +
                //         "FROM hanamart.dbo.tb_orderitem left join hanamart.dbo.tb_PromoMaster ON tPromoCode = pm_code " +
                //         "WHERE tPromoCode <> '' and tType = '' " +
                //         "GROUP BY tPromoCode, tQty, pm_desc, pm_qty, pm_price";

                //lReturn = c_localdb.RsOpen(sQBuff);

                //if (lReturn != 1)
                //{
                //    g_sMessage = string.Format("[{0}] Data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                //    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                //    c_localdb.DBClose();
                //    return;
                //}
                //else
                //{
                //    while (c_localdb.rs.EOF != true)
                //    {
                //        if (c_localdb.rs.RecordCount != 0)
                //        {
                //            //tPromocode와 PromoCode Table에 있는 Count 비교 해서 같으면 Promo 가격 가져와서 
                //            double dTempCurOrderCnt = Convert.ToDouble(c_localdb.rs.Fields["CurrentOrderCount"].Value);
                //            double dTempPromoCnt = Convert.ToDouble(c_localdb.rs.Fields["pm_qty"].Value);

                //            // 리스트에 있는 갯수와 Promo에 있는 갯수 비교
                //            if (dTempCurOrderCnt == dTempPromoCnt)               // 갯수가 같으면 Promo 가격으로 적용, TotalAmount 에서 Pm_Price 가격을 뺀다.
                //            {
                //                double dTempTotalAmount = Convert.ToDouble(c_localdb.rs.Fields["TotalAmount"].Value);
                //                double dTempPromoPrice = Convert.ToDouble(c_localdb.rs.Fields["pm_price"].Value);
                //                double dTempDicountAmount = dTempTotalAmount - dTempPromoPrice;
                //                string strTempPromoDescription = Convert.ToString(c_localdb.rs.Fields["pm_desc"].Value);

                //                // Item DC 형식으로 Order Item Table에 추가 한다.
                //                 oiOrderItem.iSeq = oiOrderItem.iSeq + 1;
                //                sQBuff = "INSERT INTO hanamart.dbo.tb_orderitem " +
                //                        "SELECT '" + oiOrderItem.sInvNo + "', " + oiOrderItem.iSeq.ToString() + ",'" + oiOrderItem.sCurDate + "','" + oiOrderItem.sCurTime + "' , prodId, prodType, ptCode, " +
                //                        "ISNULL(prodCat1,0), ISNULL(prodCat2,0), ISNULL(prodCat3,0), ISNULL(prodCat4,0), ISNULL(prodCat5,0), '', " + oiOrderItem.dQty.ToString() + ", '" + oiOrderItem.sProdUnit + "', " + strProdDeposit + ", " +
                //                        strProdDeposit + ", '0', " + oiOrderItem.dDCRate.ToString() + ", '" + oiOrderItem.dPromoQty.ToString() + "', ISNULL(pp_promotioncd,''), " +
                //                        strProdDepositAmount + ", '0', '0', '0'" + ", '', '" + c_poscominfo.mi_cardno + "', '" + c_poscominfo.ui_epno + "', '" + c_poscominfo.si_counternum +
                //                        "','','','','',prodSupp,'21','',0,''," + strRelatedid + ",'','','' " +
                //                        "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.mfPtype ON prodType = pType " +
                //                        "WHERE prodId = '" + oiOrderItem.sProdId + "'";

                //                if (c_localdb.DBExcute(sQBuff) != 1)
                //                {
                //                    g_sMessage = string.Format("[{0}] Item insert failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                //                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                             
                //                    c_localdb.DBClose();
                             
                //                    return;
                //                }
                //                else
                //                {
                //                    g_sMessage = string.Format("[{0}] ({1} item added ({2}).", sMethod, oiOrderItem.sProdId, c_poscominfo.ui_epno);
                //                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                //                }
                //            }
                //            c_localdb.rs.MoveNext();
                //        }
                //        else
                //        {
                //            g_sMessage = string.Format("[{0}] Sold Items Information is not found.", sMethod);
                //            c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                //            c_localdb.RsClose();
                //            return;
                //        }
                //    }
                //}

                initListView();
                initSalesTotal();
                PopulateListViewProdItem("AddProd");

                BackToStartTimerFromItemScan.Stop();

                BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
                BackToStartTimerFromItemScan.Start();

                if (g_HelpModeOn != true)
                {
                    btnBack.Enabled = true;
                    if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
                    {
                        btnNext.Enabled = false;
                    }
                    else
                    {
                        btnNext.Enabled = true;
                    }
                    btnSearch.Enabled = true;
                    btnSearch_Category.Enabled = true;
                    btnHelp.Enabled = true;
                }

                //txtMultiQty.Text = "";
                KeyInReady();
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

        private bool CheckMixnMatchPromo(string sInvNo, string sMixnMatchPromoCode, double dTaxRate1, double dTaxRate2, double dTaxRate3)
        {
            bool bResult = false;

            string sQBuff = string.Empty;
            long lReturn = 0;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string strPromoSDate = string.Empty;
            string strPromoEDate = string.Empty;
            int iPromoQty = 0;
            double dPromoPrice = 0;

            int iOrderQty = 0;
            double dOrderAmt = 0;
            double dOrderGST = 0;
            double dOrderPST = 0;
            double dOrderHST = 0;

            string strCurDate = DateTime.Now.ToString("yyyy-MM-dd");

            try
            {
                lReturn = c_localdb.DBConnection();

                if (lReturn < 0)
                {
                    g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    return bResult;
                }

                // Promo Code 적용 날짜 및 수량 금액 조회
                sQBuff = "SELECT * From hanamart.dbo.tb_PromoMaster Where pm_code ='" + sMixnMatchPromoCode + "'";

                lReturn = c_localdb.RsOpen(sQBuff);

                if (lReturn != 1)
                {
                    g_sMessage = string.Format("[{0}] Data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    c_localdb.DBClose();

                    return bResult;
                }
                else
                {
                    if (c_localdb.rs.RecordCount == 1)
                    {
                        strPromoSDate = Convert.ToString(c_localdb.rs.Fields["pm_sdate"].Value);
                        strPromoEDate = Convert.ToString(c_localdb.rs.Fields["pm_edate"].Value);
                        iPromoQty = Convert.ToInt32(c_localdb.rs.Fields["pm_qty"].Value);
                        dPromoPrice = Convert.ToDouble(c_localdb.rs.Fields["pm_price"].Value);
                    }
                    else
                    {
                        g_sMessage = string.Format("[{0}] Promo Data is not found (error code: {1}).", sMethod, sMixnMatchPromoCode);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                        c_localdb.RsClose();

                        return bResult;
                    }
                }
                c_localdb.RsClose();

                // Promo Code가 Activation 상태 인지 확인.
                if (iPromoQty < 1 || dPromoPrice <= 0 || Convert.ToDateTime(strPromoEDate) < Convert.ToDateTime(strCurDate) || Convert.ToDateTime(strPromoSDate) > Convert.ToDateTime(strCurDate))
                {
                    return bResult;
                }
                else
                {
                    // Order Item Table에서 Promo Code인 아이템 정보 조회
                    sQBuff = "Select Sum(tQty) as tQty, sum(tAmt) as tAmt, sum(tGst) as tGst, sum(tPst) as tPst, sum(tHst) as tHst From hanamart.dbo.tb_OrderItem " +
                             "Where tInvNo = '" + sInvNo + "' And tProd in (Select pi_prodid From hanamart.dbo.tb_PromoItem Where pi_pmcode = '" + sMixnMatchPromoCode + "') And tType = ''";

                    lReturn = c_localdb.RsOpen(sQBuff);

                    if (lReturn != 1)
                    {
                        g_sMessage = string.Format("[{0}] Data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                        c_localdb.DBClose();

                        return bResult;
                    }
                    else
                    {
                        if (c_localdb.rs.RecordCount == 1)
                        {
                            iOrderQty = Convert.ToInt32(c_localdb.rs.Fields["tQty"].Value);
                            dOrderAmt = Convert.ToDouble(c_localdb.rs.Fields["tAmt"].Value);
                            dOrderGST = Convert.ToDouble(c_localdb.rs.Fields["tGst"].Value);
                            dOrderPST = Convert.ToDouble(c_localdb.rs.Fields["tPst"].Value);
                            dOrderHST = Convert.ToDouble(c_localdb.rs.Fields["tHst"].Value);
                        }
                        else
                        {
                            g_sMessage = string.Format("[{0}] Order Item Data is not found (error code: {1}).", sMethod, sMixnMatchPromoCode);
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                            c_localdb.RsClose();

                            return bResult;

                        }

                        c_localdb.RsClose();

                        // Promo 수량과 현재 Order Item 수량과 비교
                        if (iOrderQty != 0 && iOrderQty == iPromoQty)
                        {
                            bResult = true;
                            GMixMatchPrice = dOrderAmt - dPromoPrice;
                            GMixMatchGST = dOrderGST - (Math.Round(dPromoPrice * dTaxRate1 / 100, 2));
                            GMixMatchPST = dOrderPST - (Math.Round(dPromoPrice * dTaxRate2 / 100, 2));
                            GMixMatchHST = dOrderHST - (Math.Round(dPromoPrice * dTaxRate3 / 100, 2));

                            // Order Item Table에서 Promo Code인 아이템 정보 조회
                            sQBuff = "Select * From hanamart.dbo.tb_OrderItem " +
                                     "Where tInvNo = '" + sInvNo + "' And tProd in (Select pi_prodid From hanamart.dbo.tb_PromoItem Where pi_pmcode = '" + sMixnMatchPromoCode + "') And tType = ''";

                            lReturn = c_localdb.RsOpen(sQBuff);

                            if (lReturn != 1)
                            {
                                g_sMessage = string.Format("[{0}] Data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                                c_localdb.DBClose();

                                return bResult;
                            }
                            else
                            {
                                while (!c_localdb.rs.EOF)
                                {
                                    c_localdb.rs.Fields["tType"].Value = "13";
                                    c_localdb.rs.Fields["tEntryCode"].Value = sMixnMatchPromoCode;

                                    c_localdb.rs.Update();

                                    c_localdb.rs.MoveNext();
                                }
                                c_localdb.RsClose();
                            }
                        }
                    }
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
                //c_localdb.DBClose();
            }

            return bResult;
        }

        private void SaveInvNo()
        {
            string sQBuff = string.Empty;
            long lReturn = 0;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string strMkno = c_poscominfo.ci_mkno;
            string strStno = c_poscominfo.si_counternum;
            string strYear = DateTime.Now.ToString("yy");
            string strInvSeq = txtInvNo.Text.Substring(6, 7);
            ///////////// ini 파일에서 가져온 정보로 변경해주는 Logic 필요

            try
            {
                lReturn = c_localdb.DBConnection();

                if (lReturn < 0)
                {
                    g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    return;
                }

                sQBuff = "SELECT kb_gb, kb_mkNo, kb_stNo, kb_Year, kb_InvSeq " +
                           "FROM hanamart.dbo.tb_KeyBuff " +
                          "WHERE kb_gb = 1";

                lReturn = c_localdb.RsOpen(sQBuff);

                if (lReturn == 1)
                {
                    if (c_localdb.record_count == 0)
                    {
                        c_localdb.rs.AddNew();

                        c_localdb.rs.Fields["kb_gb"].Value = 1;
                        c_localdb.rs.Fields["kb_mkNo"].Value = strMkno;
                        c_localdb.rs.Fields["kb_stNo"].Value = strStno;
                        c_localdb.rs.Fields["kb_Year"].Value = strYear;
                        c_localdb.rs.Fields["kb_InvSeq"].Value = Convert.ToInt16(strInvSeq);
                    }
                    else
                    {
                        c_localdb.rs.Fields["kb_mkNo"].Value = strMkno;
                        c_localdb.rs.Fields["kb_stNo"].Value = strStno;
                        c_localdb.rs.Fields["kb_Year"].Value = strYear;
                        c_localdb.rs.Fields["kb_InvSeq"].Value = Convert.ToInt16(strInvSeq);
                    }

                    c_localdb.rs.Update();
                }
                else
                {
                    g_sMessage = string.Format("[{0}] Invoice number data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
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
        public OrderInfo GetNormalTaxInfo()
        {
            string sQBuff = string.Empty;
            long lReturn = 0;

            OrderInfo oTaxInfo = new OrderInfo();

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            try
            {
                lReturn = c_localdb2.DBConnection();

                if (lReturn < 0)
                {
                    g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb2.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    return oTaxInfo;
                }

                if(GintLocation == 1)                                   // 벤쿠버 인 경우
                {
                    sQBuff = "SELECT tx_cd, tx_tax1 as tx_gst, tx_tax2 as tx_pst " +
                           "FROM hanamart.dbo.tb_Tax " +
                          "where tx_cd = 'B'";
                }
                else if(GintLocation == 3)                              // 미국 인 경우
                {
                    sQBuff = "SELECT tx_cd, tx_gst, tx_pst " +
                           "FROM hanamart.dbo.tb_Tax " +
                          "where tx_cd = '1'";
                }
                
                lReturn = c_localdb2.RsOpen(sQBuff);

                if (lReturn == 1)
                {
                    if (c_localdb2.record_count == 1)
                    {
                        oTaxInfo.sProdTax = Convert.ToString(c_localdb2.rs.Fields["tx_cd"].Value);                        
                        oTaxInfo.dTaxRate1 = Convert.ToDouble(c_localdb2.rs.Fields["tx_gst"].Value);
                        oTaxInfo.dTaxRate2 = Convert.ToDouble(c_localdb2.rs.Fields["tx_pst"].Value);
                    }
                    else
                    {
                        oTaxInfo.sProdTax = "";
                        oTaxInfo.dTaxRate1 = 0;
                        oTaxInfo.dTaxRate1 = 0;
                    }                    
                }
                else
                {
                    g_sMessage = string.Format("[{0}] Normal Tax Info query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb2.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    c_localdb2.DBClose();

                    return oTaxInfo;
                }

                c_localdb2.RsClose();
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
                c_localdb2.DBClose();
            }
            return oTaxInfo;
        }

        private void PopulateListViewProdItem(string pPopType)
        {
            string sQBuff = string.Empty;
            long lReturn = 0;
            double dbSavingSum = 0;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            OrderInfo oiOrderItem;

            GdblEBTAmountTotal = 0;
            GdblEBTTax1Total = 0;
            GdblEBTTax2Total = 0;
            GdblEBTTax3Total = 0;

            try
            {
                lReturn = c_localdb.DBConnection();

                if (lReturn < 0)
                {
                    g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    return;
                }

                if (pPopType == "MainForm")
                {
                    sQBuff = "DELETE FROM hanamart.dbo.tb_orderitem WHERE tType between '41' and '49' ";
                    if (c_localdb.DBExcute(sQBuff) != 1)
                    {
                        g_sMessage = string.Format("[{0}] Total DC data failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                        c_localdb.DBClose();

                        return;
                    }
                    else
                    {
                        g_sMessage = string.Format("[{0}] ({1} Total DC data deleted ({2}).", sMethod, "PopulateListItem", c_poscominfo.ui_epno);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                    }
                }

                sQBuff = "SELECT COUNT(*) AS CNT " +
                           "FROM hanamart.dbo.tb_orderitem ";
                lReturn = c_localdb.RsOpen(sQBuff);

                int iCnt = 0;
                if (lReturn == 1)
                {
                    while (!c_localdb.rs.EOF)
                    {
                        iCnt = Convert.ToInt16(c_localdb.rs.Fields["CNT"].Value);
                        c_localdb.rs.MoveNext();
                    }
                }
                else
                {
                    g_sMessage = string.Format("[{0}] Order item list data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    c_localdb.DBClose();

                    txtNumCS.Focus();

                    return;
                }
                c_localdb.RsClose();

                // 입력된 상품코드(UPC) 검색하여 존재 여부 확인.
                //sQBuff = "SELECT tProd, tID, ISNULL(prodName,'') AS prodName, ISNULL(prodKname,'') AS prodKname, tQty, ISNULL(prodUnit,'') AS prodUnit, ISNULL(tType, '') AS tType, " +
                //                "CASE WHEN tPromo = '' THEN 0 ELSE tPromo END AS prodPromo, FORMAT(tOUprice, 'N2') AS prodOUprice, FORMAT(tAmt, 'N2') AS tAmt, FORMAT(tGst, 'N2') AS tGst, FORMAT(tPst, 'N2') AS tPst, " +
                //                "FORMAT(tHst, 'N2') AS tHst, ISNULL(tTax,'') AS prodTax " +
                //            "FROM hanamart.dbo.tb_orderitem LEFT JOIN hanamart.dbo.mfProd ON prodId = tProd " +
                //        "ORDER BY tID DESC ";
                if (GintLocation == 1)                           // 벤쿠버 인경우
                {
                    //sQBuff = "SELECT tProd, tID, ISNULL(prodName,'') AS prodName, ISNULL(prodKname,'') AS prodKname, tQty, ISNULL(prodUnit,'') AS prodUnit, ISNULL(tType, '') AS tType, " +
                    //            "(CASE WHEN tPromo = '' THEN 0 ELSE tPromo END) AS prodPromo, ROUND(tOUprice, 2) AS prodOUprice, ROUND(tAmt, 2) AS tAmt, ROUND(tGst, 2) AS tGst, ROUND(tPst, 2) AS tPst, " +
                    //            "ROUND(tHst, 2) AS tHst, ISNULL(tTax,'') AS prodTax " +
                    //        "FROM hanamart.dbo.tb_orderitem LEFT JOIN hanamart.dbo.mfProd ON prodId = tProd " +
                    //    "ORDER BY tID DESC ";

                    sQBuff = "SELECT tProd, tID, ISNULL(prodName,'') AS prodName, ISNULL(prodKname,'') AS prodKname, tQty, ISNULL(prodUnit,'') AS prodUnit, ISNULL(tType, '') AS tType, " +
                                "(CASE WHEN tPromo = '' THEN '0' ELSE tPromo END) AS prodPromo, ROUND(tIUprice, 2) AS prodIUprice, ROUND(tOUprice, 2) AS prodOUprice, ROUND(tAmt, 2) AS tAmt, ROUND(tGst, 2) AS tGst, ROUND(tPst, 2) AS tPst, " +
                                "ROUND(tHst, 2) AS tHst, ISNULL(tTax,'') AS prodTax, ISNULL(tNative,'') AS prodDCRate " +
                            "FROM hanamart.dbo.tb_orderitem LEFT JOIN hanamart.dbo.mfProd ON prodId = tProd " +
                            "ORDER BY tID DESC ";
                }
                
                else if (GintLocation == 2)                           // 토론토 인경우
                {
                    sQBuff = "SELECT tProd, tID, ISNULL(prodName,'') AS prodName, ISNULL(prodKname,'') AS prodKname, tQty, ISNULL(prodUnit,'') AS prodUnit, ISNULL(tType, '') AS tType, " +
                                "(CASE WHEN tPromo = '' THEN '0' ELSE tPromo END) AS prodPromo, ROUND(tIUprice, 2) AS prodIUprice, ROUND(tOUprice, 2) AS prodOUprice, ROUND(tAmt, 2) AS tAmt, ROUND(tGst, 2) AS tGst, ROUND(tPst, 2) AS tPst, " +
                                "ROUND(tHst, 2) AS tHst, ISNULL(tTax,'') AS prodTax, ISNULL(tNative,'') AS prodDCRate " +
                            "FROM hanamart.dbo.tb_orderitem LEFT JOIN hanamart.dbo.mfProd ON prodId = tProd " +
                            "ORDER BY tID DESC ";
                }
                else                                        // 미국 인 경우
                {
                    sQBuff = "SELECT tProd, tID, ISNULL(prodName,'') AS prodName, ISNULL(prodKname,'') AS prodKname, tQty, ISNULL(prodUnit,'') AS prodUnit, ISNULL(tType, '') AS tType, " +
                                "CASE WHEN tPromo = '' THEN 0 ELSE tPromo END AS prodPromo, ROUND(tIUprice, 2) AS prodIUprice, ROUND(tOUprice, 2) AS prodOUprice, ROUND(tAmt, 2) AS tAmt, ROUND(tGst, 2) AS tGst, ROUND(tPst, 2) AS tPst, " +
                                "ISNULL(tTax,'') AS prodTax, ISNULL(tFoodStamp,'0') AS tFoodStamp " +
                            "FROM hanamart.dbo.tb_orderitem LEFT JOIN hanamart.dbo.mfProd ON prodId = tProd " +
                        "ORDER BY tID DESC ";
                }

                lReturn = c_localdb.RsOpen(sQBuff);

                if (lReturn == 1)
                {
                    while (!c_localdb.rs.EOF)
                    {
                        oiOrderItem.iSeq = Convert.ToInt16(c_localdb.rs.Fields["tID"].Value);
                        oiOrderItem.sProdId = Convert.ToString(c_localdb.rs.Fields["tProd"].Value);
                        oiOrderItem.sProdName = Convert.ToString(c_localdb.rs.Fields["prodName"].Value);
                        oiOrderItem.sProdKname = Convert.ToString(c_localdb.rs.Fields["prodKname"].Value);
                        oiOrderItem.dQty = Convert.ToDouble(c_localdb.rs.Fields["tQty"].Value);
                        oiOrderItem.dProdPromo = Convert.ToDouble(c_localdb.rs.Fields["prodPromo"].Value);
                        oiOrderItem.dProdRegPrice = Convert.ToDouble(c_localdb.rs.Fields["prodOUprice"].Value);
                        oiOrderItem.sProdUnit = Convert.ToString(c_localdb.rs.Fields["prodUnit"].Value);
                        oiOrderItem.dAmt = Convert.ToDouble(c_localdb.rs.Fields["tAmt"].Value);
                        oiOrderItem.dGst = Convert.ToDouble(c_localdb.rs.Fields["tGst"].Value);
                        oiOrderItem.dPst = Convert.ToDouble(c_localdb.rs.Fields["tPst"].Value);
                        if (GintLocation != 3)                           // 벤쿠버, 토론토 인경우
                        {
                            oiOrderItem.dHst = Convert.ToDouble(c_localdb.rs.Fields["tHst"].Value);
                            oiOrderItem.sProdTax = Convert.ToString(c_localdb.rs.Fields["prodTax"].Value);
                            oiOrderItem.sType = Convert.ToString(c_localdb.rs.Fields["tType"].Value);
                            oiOrderItem.dDCRate = Convert.ToDouble(c_localdb.rs.Fields["prodDCRate"].Value);
                            
                            if (oiOrderItem.dProdPromo >= 1)     // Promo 갯수가 1 이상
                                oiOrderItem.dIUprice = Convert.ToDouble(c_localdb.rs.Fields["prodIUprice"].Value);
                            else
                                oiOrderItem.dIUprice = 0;

                            oiOrderItem.sFoodStamp = "0";
                        }
                        else                                            // 미국 인 경우
                        {
                            oiOrderItem.dIUprice = Convert.ToDouble(c_localdb.rs.Fields["prodIUprice"].Value);
                            oiOrderItem.dHst = 0;
                            oiOrderItem.sProdTax = Convert.ToString(c_localdb.rs.Fields["prodTax"].Value);
                            // ProdTax 가 0 인 경우.
                            if (oiOrderItem.sProdTax == "" || Convert.ToInt32(oiOrderItem.sProdTax) <= 0 )     { oiOrderItem.sProdTax = ""; }
                            else if (Convert.ToInt32(oiOrderItem.sProdTax) > 0)                               { oiOrderItem.sProdTax = "T"; }                // 0보다 큰 경우 T로 표시.
                            
                            oiOrderItem.sType = Convert.ToString(c_localdb.rs.Fields["tType"].Value);
                            oiOrderItem.dDCRate = 0;
                            oiOrderItem.sFoodStamp = Convert.ToString(Convert.ToInt16(c_localdb.rs.Fields["tFoodStamp"].Value));
                        }
                        //oiOrderItem.sProdTax = Convert.ToString(c_localdb.rs.Fields["prodTax"].Value);
                        //oiOrderItem.sType = Convert.ToString(c_localdb.rs.Fields["tType"].Value);
                        //oiOrderItem.dDCRate = Convert.ToDouble(c_localdb.rs.Fields["prodDCRate"].Value);

                        //oiOrderItem.dProdPromoPrice = Convert.ToDouble(c_localdb.rs.Fields["PromoPrice"].Value);
                        //oiOrderItem.sPromoSdate = Convert.ToString(c_localdb.rs.Fields["promoSdate"].Value);
                        //oiOrderItem.sPromoEdate = Convert.ToString(c_localdb.rs.Fields["promoEdate"].Value);

                        //Bottle Deposit Item 일때.
                        if (oiOrderItem.sType == "21")
                        {
                            oiOrderItem.sProdName = "Bottle Deposit@" + oiOrderItem.sProdName;
                            oiOrderItem.sProdKname = "공병값";
                        }
                        else if (oiOrderItem.sType == "20")
                        {
                            oiOrderItem.sProdName = "CRF@" + oiOrderItem.sProdName;
                            oiOrderItem.sProdKname = "용기재활용수수료";
                        }
                        else if (oiOrderItem.sType == "22")
                        {
                            oiOrderItem.sProdName = "E.H.F. ";
                            oiOrderItem.sProdKname = "환경부담금 ";
                        }

                        else if (oiOrderItem.sType == "48")
                        {
                            oiOrderItem.sProdName = "DC -" + oiOrderItem.sProdName +"@ "+ oiOrderItem.dDCRate.ToString() + "%";
                            oiOrderItem.sProdKname = "";
                        }
                        else if (oiOrderItem.sType == "47")                 // Mix & Match 
                        {
                            oiOrderItem.sProdName = "Mix & Match Promotion";
                            oiOrderItem.sProdKname = "믹스 앤 매치 프로모션";
                        }
                        else if (oiOrderItem.sType == "43")                 // Employee Discount 
                        {
                            oiOrderItem.sProdName = "Employee Discount";
                            oiOrderItem.sProdKname = "직원 할인";
                        }

                        //else if(c_poscomlibs.GetTotalDCType(oiOrderItem.sType) == true) // Total DC인 경우 Coount Add하지 않음
                        //{
                        //    // Item Discount 인 경우 

                        //}
                        else  // 일반 상품일때.
                        {
                            calcItemCountAdd();
                        }

                        if (c_poscomlibs.GetTotalDCType(oiOrderItem.sType) == false) // Total DC인 경우 List에 표시하지 않음
                        {
                            //var prodItem = new ListViewItem(new[] { oiOrderItem.sProdId, oiOrderItem.iSeq.ToString(), oiOrderItem.sProdName, oiOrderItem.dQty.ToString(), oiOrderItem.dProdRegPrice.ToString(), oiOrderItem.dAmt.ToString(), oiOrderItem.sProdTax, oiOrderItem.sType });
                            var prodItem = new ListViewItem(new[] { oiOrderItem.sProdId, oiOrderItem.iSeq.ToString(), oiOrderItem.sProdName, String.Format("{0:#,##0.00}", oiOrderItem.dQty, 2), String.Format("{0:#,##0.00}", oiOrderItem.dProdRegPrice, 2), String.Format("{0:#,##0.00}", oiOrderItem.dAmt, 2), oiOrderItem.sProdTax, oiOrderItem.sType });

                            if (GintLocation == 1 || GintLocation == 3)                   // 벤쿠버, 미국 인 경우
                            {
                                if (oiOrderItem.dProdPromo >= 1)     // Promo 갯수가 1 이상
                                                                     //&& (oiOrderItem.sProdUnit == "EA" || oiOrderItem.sProdUnit == "PK" || oiOrderItem.sProdUnit == "BAG" || oiOrderItem.sProdUnit == "BOX" || oiOrderItem.sProdUnit == "BN")
                                {
                                    // Saving CS 및 CS Value 표시
                                    lbSavingCS.Visible = true;
                                    lbSavingValueCS.Visible = true;

                                    dbSavingSum += (oiOrderItem.dIUprice * oiOrderItem.dQty) - (oiOrderItem.dProdRegPrice * oiOrderItem.dQty);
                                    //dbSavingSum += (oiOrderItem.dIUprice * oiOrderItem.dQty) - (oiOrderItem.dProdRegPrice * oiOrderItem.dQty);
                                }                                 
                            }

                            if(GintLocation == 3)                       // 미국인 경우 
                            {       
                                if (oiOrderItem.sFoodStamp == "1")      // Food Stamp 아이템인 인 경우 EBT Total Amount / TAX Amount 다시 합산함. Item Correct 했을 경우 만 해당.
                                {
                                    GdblEBTAmountTotal += oiOrderItem.dAmt;
                                    GdblEBTTax1Total += oiOrderItem.dGst;
                                    GdblEBTTax2Total += oiOrderItem.dPst;
                                    GdblEBTTax3Total += 0;                                                                      
                                }
                            }
                            ItemCSView.Items.Add(prodItem);
                        }
                        calcTotalDisplay(oiOrderItem.dAmt.ToString(), oiOrderItem.dGst.ToString(), oiOrderItem.dPst.ToString(), oiOrderItem.dHst.ToString());

                        c_localdb.rs.MoveNext();
                        iCnt--;
                    }

                    // Saving 금액표시.
                    if(dbSavingSum == 0)
                    {
                        lbSavingCS.Visible = false;
                        lbSavingValueCS.Visible = false;
                    }
                    lbSavingValueCS.Text = c_poscomlibs.getDoubleFormat(dbSavingSum);
                }
                else
                {
                    g_sMessage = string.Format("[{0}] Order item list data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    c_localdb.DBClose();

                    txtNumCS.Focus();

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

                txtNumCS.Focus();
            }
        }
        private void ProcessTotalDC()
        {
            // 현재까지 스캔된 Item에 대해서 Total DC Rate로 재계산되는 Module..

            // 1. tb_OrderItem에서 List 불러옴 - 전체 Loop문 (tID 역순) - 
            //  1-1. Checking Total DC type (tType)이 Total DC가 아닌 항목들만 재계산
            //  1-2. 직원할인 제외 카테고리 로직
            // 2. 스캔된 Item에 대한 계산이 끝나면 다시 Total DC에 대한 부분 계산
            //  2-1. Total DC rate가 0이 아니면 진행
            //  2-2. 직원할인인 경우 카테고리 적용된 쿼리로 Sum금액 가져옴. 직원할인 현재 밸런스도 가져옴
            //  2-3. 일반할인인 경우 전체 Sum금액 가져옴
            //  2-4. Total DC 항목 추가를 위해 각 변수에 값 할당 후 Add Item 
            // 3. DC Rate, 금액 등을 화면에 표시
            // 4. CntTotSalePayment 처리 - 전체 금액 계산, 결제 내역 확인, 결제가 완료된 경우 Receupt & Kitchen Slip & Coupon Print
            // 5. KeyInReady

            double dblPayAmt = 0;
            double dblGstAmt = 0;
            double dblPstAmt = 0;
            double dblHstAmt = 0;

            double[] dblTaxRate;
            double dblGstRate = 0;
            double dblPstRate = 0;
            double dblHstRate = 0;

            double dblPayAmtDC = 0;
            double dblGstDC = 0;
            double dblPstDC = 0;
            double dblHstDC = 0;

            double dblPayAmtTot = 0;
            double dblGstTot = 0;
            double dblPstTot = 0;
            double dblHstTot = 0;

            string strInvNo = "";
            string strtType = "";
            string strtTax = "";
            string strtAmt = "";
            string strtGst = "";
            string strtPst = "";
            string strtHst = "";
            string strtID = "";

            // 직원할인
            string strtPtype = "";
            string strtPtype2 = "";
            string strtAmtEMP = "";
            string strtGstEMP = "";
            string strtPstEMP = "";
            string strtHstEMP = "";

            double dblPayAmtEMP = 0;
            double dblGstAmtEMP = 0;
            double dblPstAmtEMP = 0;
            double dblHstAmtEMP = 0;

            double dblPayAmtDCEMP = 0;
            double dblGstDCEMP = 0;
            double dblPstDCEMP = 0;
            double dblHstDCEMP = 0;

            double dblPayAmtTotEMP = 0;
            double dblGstTotEMP = 0;
            double dblPstTotEMP = 0;
            double dblHstTotEMP = 0;

            double dbltmpEMPbal = 0;

            // 1. tb_OrderItem Loop
            string constr = commonClass.constringLocal;
            SqlConnection sqlcon = new SqlConnection(constr);
            sqlcon.Open();

            string sql = "";
            if (GintLocation == 1)                           // 벤쿠버 인경우
            {
                sql += "SELECT tInvNo, tID, tDate, tTime, tProd, tPtype, tPtype2, tCat1, tCat2, tCat3, tCat4, tCat5, tCatCode, tQty, tPunit, tIUprice, " +
                          "tOUprice, tWprice, tNative, tPromo, tPromoCode, tAmt, tGst, tPst, tHst, tTax, tCust, tPassWord, tPassStation, tUpCode, " +
                          "tSpecial, tFree, tMMBC, tSupp, tType, tEntryCode, tFoodStamp, tGiftCardRef, tRelatedID, tMixMatch, tShift, 1 " +
                     "FROM hanamart.dbo.tb_OrderItem WHERE tInvNo = '" + txtInvNo.Text + "' " +
                 "ORDER BY tID DESC";
            }
            else if (GintLocation == 2)                           // 토론토 인경우
            {
                sql += "SELECT tInvNo, tID, tDate, tTime, tProd, tPtype, tPtype2, tCat1, tCat2, tCat3, tCat4, tCat5, tQty, tPunit, tIUprice, " +
                          "tOUprice, tWprice, tNative, tPromo, tAmt, tGst, tPst, tHst, tTax, tCust, tPassWord, tPassStation, tUpCode, " +
                          "tSpecial, tFree, tSupp, tType, tEntryCode, tFoodStamp, tGiftCardRef, tRelatedID, tMixMatch, tShift, 1 " +
                     "FROM hanamart.dbo.tb_OrderItem WHERE tInvNo = '" + txtInvNo.Text + "' " +
                 "ORDER BY tID DESC";
            }
            else                                            // 미국 인 경우
            {
                sql += "SELECT tInvNo, tID, tDate, tTime, tProd, tPtype, tPtype2, tCat1, tCat2, tCat3, tCat4, tCat5, tQty, tPunit, tIUprice, " +
                          "tOUprice, tWprice, tNative, tPromo, tAmt, tGst, tPst, tTax, tTxExemAmt, tTxExem, tCust, tRefunded, tPassWord, tCust, tPassWord, tPassStation, tUpCode, " +
                          "tSpecial, tFree, tSupp, tType, tEntryCode, tFoodStamp, tGiftCardRef, tRelatedID, tShift, 1, tMemo " +
                     "FROM hanamart.dbo.tb_OrderItem WHERE tInvNo = '" + txtInvNo.Text + "' " +
                 "ORDER BY tID DESC";
            }

            SqlCommand comm = new SqlCommand(sql, sqlcon);
            SqlDataReader rs = comm.ExecuteReader();

            dblPayAmtTot = 0;
            dblGstTot = 0;
            dblPstTot = 0;
            dblHstTot = 0;

            dblPayAmtTotEMP = 0;
            dblGstTotEMP = 0;
            dblPstTotEMP = 0;
            dblHstTotEMP = 0;

            dblPayAmt = 0;
            dblGstAmt = 0;
            dblPstAmt = 0;
            dblHstAmt = 0;

            dblGstRate = 0;
            dblPstRate = 0;
            dblHstRate = 0;

            dblPayAmtDC = 0;
            dblGstDC = 0;
            dblPstDC = 0;
            dblHstDC = 0;

            dblPayAmtEMP = 0;
            dblGstAmtEMP = 0;
            dblPstAmtEMP = 0;
            dblHstAmtEMP = 0;

            dblPayAmtDCEMP = 0;
            dblGstDCEMP = 0;
            dblPstDCEMP = 0;
            dblHstDCEMP = 0;

            while (rs.Read())
            {
                // Loop 때마다 Local 변수 Clear
                
                strtType = "";
                strtTax = "";
                strtAmt = "";
                strtGst = "";
                strtPst = "";
                strtHst = "";

                strtPtype = "";
                strtPtype2 = "";
                strtAmtEMP = "";
                strtGstEMP = "";
                strtPstEMP = "";
                strtHstEMP = "";
                
                strtType = rs["tType"].ToString();
                strtTax = rs["tTax"].ToString();
                strtAmt = rs["tAmt"].ToString();
                strtGst = rs["tGst"].ToString();
                strtPst = rs["tPst"].ToString();
                if (GintLocation != 3)                           // 벤쿠버, 토론토 인경우
                {
                    strtHst = rs["tHst"].ToString();
                }
                
                strtID = rs["tID"].ToString();
                strInvNo = rs["tInvNo"].ToString();

                strtPtype = rs["tPtype"].ToString();
                strtPtype2 = rs["tPtype2"].ToString();

                int inttType = 0;

                if (strtType == "")
                {
                    inttType = 0;
                }
                else
                {
                    inttType = Convert.ToInt16(strtType);
                }

                if (inttType < 41 || inttType > 46)
                {
                    if (strtTax != "")
                    {
                        dblTaxRate = c_poscomlibs.GetTaxRate(strtTax);
                        dblGstRate = dblTaxRate[0];
                        dblPstRate = dblTaxRate[1];
                        dblHstRate = dblTaxRate[2];

                    }
                    else
                    {
                        dblGstRate = 0;
                        dblPstRate = 0;
                        dblHstRate = 0;

                    }

                    dblPayAmt = Math.Round(-(Convert.ToDouble(strtAmt)) * GdblTotalDCRate / 100,2);
                    dblGstAmt = Math.Round(dblPayAmt * dblGstRate,2);
                    dblPstAmt = Math.Round(dblPayAmt * dblPstRate, 2);
                    dblHstAmt = Math.Round(dblPayAmt * dblHstRate,2);

                    if (strtTax == "T") // 전자담배(Vape) PST 20% 적용한 금액을 합산 후 GST 계산
                    {
                        dblPstAmt = Math.Round(dblPayAmt * dblPstRate,2);
                        dblGstAmt = Math.Round((dblPayAmt + dblPstAmt) * dblGstRate,2);
                    }

                    if (strtType != "22")
                    {
                        dblGstDC = dblGstDC + dblGstAmt;
                        dblPstDC = dblPstDC + dblPstAmt;
                        dblHstDC = dblHstDC + dblHstAmt;
                        dblPayAmtDC = dblPayAmtDC + dblPayAmt;
                    }

                    if (strtGst != "")
                    {
                        dblGstTot = dblGstTot + Convert.ToDouble(strtGst);
                    }
                    if (strtPst != "")
                    {
                        dblPstTot = dblPstTot + Convert.ToDouble(strtPst);
                    }
                    if (strtHst != "")
                    {
                        dblHstTot = dblHstTot + Convert.ToDouble(strtHst);
                    }
                    if (strtAmt != "")
                    {
                        dblPayAmtTot = dblPayAmtTot + Convert.ToDouble(strtAmt);
                    }

                    // 밴쿠버 직원할인 (담배-28/로또-10,13/전화카드-22/Gift Card - 96/Event Ticket - G5/Flower - W1/HW부-09 전품목 제외) - Jake
                    // 직원할인 제외 - 리치몬드 아궁이 (2020.09.08 김종윤 팀장 요청)
                    if (strtPtype == "28" || strtPtype == "10" || strtPtype == "13" || strtPtype == "22" || strtPtype == "96" || strtPtype == "G5" || strtPtype == "W1" || strtPtype == "M7" || strtPtype2 == "09")
                    {
                        dblPayAmtEMP = Math.Round(-(Convert.ToDouble(strtAmt)) * GdblTotalDCRate / 100,2);
                        dblGstAmtEMP = Math.Round(dblPayAmtEMP * dblGstRate,2);
                        dblPstAmtEMP = Math.Round(dblPayAmtEMP * dblPstRate,2);
                        dblHstAmtEMP = Math.Round(dblPayAmtEMP * dblHstRate,2);

                        if (strtTax == "T") // 전자담배(Vape) PST 20% 적용한 금액을 합산 후 GST 계산
                        {
                            dblPstAmtEMP = Math.Round(dblPayAmtEMP * dblPstRate,2);
                            dblGstAmtEMP = Math.Round((dblPayAmtEMP + dblPstAmtEMP) * dblGstRate,2);
                        }

                        if (strtType != "22")
                        {
                            dblGstDCEMP = dblGstDCEMP + dblGstAmtEMP;
                            dblPstDCEMP = dblPstDCEMP + dblPstAmtEMP;
                            dblHstDCEMP = dblHstDCEMP + dblHstAmtEMP;
                            dblPayAmtDCEMP = dblPayAmtDCEMP + dblPayAmtEMP;
                        }

                        if (strtGst != "")
                        {
                            dblGstTotEMP = dblGstTotEMP + Convert.ToDouble(strtGst);
                        }
                        if (strtPst != "")
                        {
                            dblPstTotEMP = dblPstTotEMP + Convert.ToDouble(strtPst);
                        }
                        if (strtHst != "")
                        {
                            dblHstTotEMP = dblHstTotEMP + Convert.ToDouble(strtHst);
                        }
                        if (strtAmt != "")
                        {
                            dblPayAmtTotEMP = dblPayAmtTotEMP + Convert.ToDouble(strtAmt);
                        }

                    } // 직원할인 끝

                }
                else
                {
                    // 이미 저장되어 있던 DC의 경우 삭제 처리..
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

                        //2-1. tb_SoldItem
                        sQBuff = "DELETE FROM HANAMART.dbo.tb_OrderItem WHERE tID = " + strtID;
                        if (c_localdb.DBExcute(sQBuff) != 1)
                        {
                            g_sMessage = string.Format("[{0}] Exist Discount data delete failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                            c_localdb.DBClose();

                            return;
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
                }

            } // Loop문 끝
            rs.Close();

            OrderItem o;

            if (GstrTotalDCType == "41")
            {
                lblDCtype.Text = "Total DC";
            }
            else if (GstrTotalDCType == "42")
            {
                lblDCtype.Text = "VIP DC";
            }
            else if (GstrTotalDCType == "43")
            {
                lblDCtype.Text = "Employee DC";

                // 직원할인 금액 적용
                dblGstDC = dblGstDC - dblGstDCEMP;
                dblPstDC = dblPstDC - dblPstDCEMP;
                dblHstDC = dblHstDC - dblHstDCEMP;
                dblPayAmtDC = dblPayAmtDC - dblPayAmtDCEMP;

                dblGstTot = dblGstTot - dblGstTotEMP;
                dblPstTot = dblPstTot - dblPstTotEMP;
                dblHstTot = dblHstTot - dblHstTotEMP;
                dblPayAmtTot = dblPayAmtTot - dblPayAmtTotEMP;

            }
            else if (GstrTotalDCType == "45")
            {
                lblDCtype.Text = "Internal Trx";
            }
            else if (GstrTotalDCType == "46")
            {
                lblDCtype.Text = "Wholesale Trx";
            }
            else
            {
                lblDCtype.Text = "DISCOUNT";
            }

            if (GdblTotalDCRate != 0)
            {
                if (GstrTotalDCType == "43")
                {
                    sql = "SELECT tPtype2, Sum(tAmt) as tAmt, Sum(tGst) as tGst, Sum(tPst) as tPst, Sum(tHst) as tHst FROM tb_OrderItem Where tInvNo = '" + strInvNo + "' " +
                          "AND (tPtype != '28' AND tPtype != '22' AND tPtype2 != '10' AND tPtype2 != '13' AND tPtype != '96' AND tPtype != 'G5' AND tPtype != 'W1' AND tPtype2 != '09' and tPtype != 'M7') " +
                          "Group by tPtype2";
                }
                else
                {
                    sql = "SELECT tPtype2, Sum(tAmt) as tAmt, Sum(tGst) as tGst, Sum(tPst) as tPst, Sum(tHst) as tHst FROM tb_OrderItem Where tInvNo = '" + strInvNo + "' Group by tPtype2 ";
                }

                if (GcBal != "") // 직원 할인 밸런스 임시 변수에 저장 (중복해서 Total DC 모듈이 Call되는 경우 기존의 Balance값을 가지고 계산하도록 변수 할당)
                {
                    dbltmpEMPbal = Convert.ToDouble(GcBal);
                }
                else
                {
                    dbltmpEMPbal = 0;
                }

                comm = new SqlCommand(sql, sqlcon);
                rs = comm.ExecuteReader();

                string strProdAmt = "";
                string strProdGst = "";
                string strProdPst = "";
                string strProdHst = "";

                while (rs.Read())
                {
                    o.tInvNo = strInvNo;
                    o.tID = "";
                    o.tProd = "2995200000005";
                    o.tProdName = lblDCtype.Text + " " + GdblTotalDCRate + "%";
                    o.tProdKname = "";

                    o.tPtype = "";
                    sql = "Select Top 1 pType From tb_prodType Where ptCode = '" + rs["tPtype2"].ToString() + "' Order By pType ";
                    SqlCommand comm2 = new SqlCommand(sql, sqlcon);
                    SqlDataReader rs2 = comm2.ExecuteReader();
                    while (rs2.Read())
                    {
                        o.tPtype = rs2["pType"].ToString();
                    }
                    rs2.Close();

                    o.tPtype2 = rs["tPtype2"].ToString();
                    o.tCat1 = "";
                    o.tCat2 = "";
                    o.tCat3 = "";
                    o.tCat4 = "";
                    o.tCat5 = "";
                    o.tQty = "1";
                    o.tIUprice = "0";

                    strProdAmt = rs["tAmt"].ToString();
                    strProdGst = rs["tGst"].ToString();
                    strProdPst = rs["tPst"].ToString();
                    strProdHst = rs["tHst"].ToString();


                    if (dblPayAmtDC != 0 && strProdAmt != "0")
                    {
                        o.tOUprice = c_poscomlibs.getDoubleFormat((dblPayAmtDC * Convert.ToDouble(strProdAmt) / dblPayAmtTot));
                        o.tAmt = o.tOUprice;

                    }
                    else
                    {
                        o.tOUprice = "0";
                        o.tAmt = "0";
                    }

                    if (dblGstDC != 0 && Convert.ToDouble(strProdGst) != 0)
                    {
                        o.tGst = c_poscomlibs.getDoubleFormat((dblGstDC * Convert.ToDouble(strProdGst) / dblGstTot));
                    }
                    else
                    {
                        o.tGst = "0";
                    }

                    if (dblPstDC != 0 && Convert.ToDouble(strProdPst) != 0)
                    {
                        o.tPst = c_poscomlibs.getDoubleFormat((dblPstDC * Convert.ToDouble(strProdPst) / dblPstTot));
                    }
                    else
                    {
                        o.tPst = "0";
                    }

                    if (dblHstDC != 0 && Convert.ToDouble(strProdHst) != 0)
                    {
                        o.tHst = c_poscomlibs.getDoubleFormat((dblHstDC * Convert.ToDouble(strProdHst) / dblHstTot));
                    }
                    else
                    {
                        o.tHst = "0";
                    }

                    if (GstrTotalDCType == "43")
                    {
                        if (dbltmpEMPbal < Math.Abs(Convert.ToDouble(o.tAmt))) // 직원할인 잔여금액이 할인 금액보다 적을 경우
                        {
                            o.tOUprice = c_poscomlibs.getDoubleFormat(dbltmpEMPbal);
                            o.tAmt = c_poscomlibs.getDoubleFormat(dbltmpEMPbal * -1);

                            dbltmpEMPbal = 0;
                        }
                        else
                        {
                            dbltmpEMPbal = dbltmpEMPbal + Convert.ToDouble(o.tAmt);
                        }

                        if (dbltmpEMPbal < Math.Abs(Convert.ToDouble(o.tGst)))
                        {
                            o.tGst = c_poscomlibs.getDoubleFormat(dbltmpEMPbal * -1);
                            dbltmpEMPbal = 0;
                        }
                        else
                        {
                            dbltmpEMPbal = dbltmpEMPbal + Convert.ToDouble(o.tGst);
                        }

                        if (dbltmpEMPbal < Math.Abs(Convert.ToDouble(o.tPst)))
                        {
                            o.tPst = c_poscomlibs.getDoubleFormat(dbltmpEMPbal * -1);
                            dbltmpEMPbal = 0;
                        }
                        else
                        {
                            dbltmpEMPbal = dbltmpEMPbal + Convert.ToDouble(o.tPst);
                        }

                        if (dbltmpEMPbal < Math.Abs(Convert.ToDouble(o.tHst)))
                        {
                            o.tHst = c_poscomlibs.getDoubleFormat(dbltmpEMPbal * -1);
                            dbltmpEMPbal = 0;
                        }
                        else
                        {
                            dbltmpEMPbal = dbltmpEMPbal + Convert.ToDouble(o.tHst);
                        }

                    }

                    //wtFree = "" // 어디에서 사용되는 값인지 확인 필요

                    o.tType = GstrTotalDCType;
                    o.tNative = GdblTotalDCRate.ToString();
                    o.tTax = strtTax;

                    o.tPromo = "0";
                    o.tPromoCode = "";
                    //o.tCust = ""; c_poscominfo.mi_cardno
                    o.tCust = c_poscominfo.mi_cardno;
                    o.tPassWord = txtEmpNo.Text;
                    o.tPassStation = c_poscominfo.si_counternum;
                    o.tUpCode = "";
                    o.tSpecial = "";
                    o.tFree = "";
                    o.tMMBC = "";
                    o.tSupp = "";
                    o.tEntryCode = "";
                    o.tFoodStamp = "0";
                    o.tGiftCardRef = "0";
                    o.tRelatedID = "0";
                    o.tMixMatch = "";
                    o.tShift = "";
                    o.tMemo = "";

                    if (o.tAmt != "0")
                    {
                        AddOrderItem(o);
                    }


                }
                rs.Close();

            }
            else
            {
                GdblTotalDCRate = 0;
            }

            if (GdblTotalDCRate > 0)
            {
                //lblDCtype.Text = GstrTotalDCType;
                lblDCRate.Text = GdblTotalDCRate.ToString();

                if (GstrTotalDCType == "43")
                {
                    lblDCAmount.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(GcBal) - dbltmpEMPbal);
                }
                else
                {
                    lblDCAmount.Text = c_poscomlibs.getDoubleFormat(dblPayAmtDC + dblGstDC + dblPstDC + dblHstDC);
                }

                lblDCtype.Visible = true;
                lblDCRate.Visible = true;
                lblDCunit.Visible = true;

                lblDCAmtLabel.Visible = true;
                lblDCAmount.Visible = true;
            }
            else
            {
                lblDCtype.Text = "";
                lblDCRate.Text = "0";
                lblDCAmount.Text = "0.00";

                lblDCtype.Visible = false;
                lblDCRate.Visible = false;
                lblDCunit.Visible = false;

                lblDCAmtLabel.Visible = false;
                lblDCAmount.Visible = false;
            }
            sqlcon.Close();

            o.tType = "";
            o.tNative = "0";

            //GstrTotalDCType = "";
            //GdblTotalDCRate = 0;

            //CntTotSalePayment();

        }
        private void calcItemCountAdd()
        {
            string strCount = lbItemCountValCS.Text;
            lbItemCountValCS.Text = (Convert.ToInt16(strCount) + 1).ToString();
        }

        private void calcTotalDisplay(string pAmt, string pGst, string pPst, string pHst)
        {
            string strCurAmt = lbSubTotalValCS.Text;
            string strCurGst = lbGSTValCS.Text;
            string strCurPst = lbPSTValCS.Text;
            string strCurHst = lbHSTValCS.Text;

            lbSubTotalValCS.Text = String.Format("{0:#,##0.00}", Convert.ToDouble(strCurAmt) + Convert.ToDouble(pAmt), 2);

            if(GintLocation != 3)                   // 벤쿠버, 토론토 인 경우
            {
                lbGSTValCS.Text = String.Format("{0:#,##0.00}", Convert.ToDouble(strCurGst) + Convert.ToDouble(pGst), 2);
                lbPSTValCS.Text = String.Format("{0:#,##0.00}", Convert.ToDouble(strCurPst) + Convert.ToDouble(pPst), 2);
            }
            else
            {
                lbGSTValCS.Text = String.Format("{0:#,##0.00}", Convert.ToDouble(strCurGst) + Convert.ToDouble(pGst) + Convert.ToDouble(strCurPst) + Convert.ToDouble(pPst), 2);
            }
                        
            lbHSTValCS.Text = String.Format("{0:#,##0.00}", Convert.ToDouble(strCurHst) + Convert.ToDouble(pHst), 2);
            lbTotalValCS.Text = String.Format("{0:#,##0.00}", Convert.ToDouble(lbSubTotalValCS.Text) + Convert.ToDouble(lbGSTValCS.Text) + Convert.ToDouble(lbPSTValCS.Text) + Convert.ToDouble(lbHSTValCS.Text), 2);
            lblPayBalance.Text             = c_poscomlibs.getDoubleFormat(Convert.ToDouble(lbTotalValCS.Text) - Convert.ToDouble(lblPayTotal.Text) + Convert.ToDouble(lblPayPennyRounded.Text));
            lbSelectPaymentBalanceNum.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(lbTotalValCS.Text) - Convert.ToDouble(lblPayTotal.Text) + Convert.ToDouble(lblPayPennyRounded.Text));

            //lbSubTotalValCS.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(strCurAmt) + Convert.ToDouble(pAmt));
            //lbGSTValCS.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(strCurGst) + Convert.ToDouble(pGst));
            //lbPSTValCS.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(strCurPst) + Convert.ToDouble(pPst));
            //lbHSTValCS.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(strCurHst) + Convert.ToDouble(pHst));
            //lbTotalValCS.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(lbSubTotalValCS.Text) + Convert.ToDouble(lbGSTValCS.Text) + Convert.ToDouble(lbPSTValCS.Text) + Convert.ToDouble(lbHSTValCS.Text));

        }
        private void CntTotSalePayment()
        {
            // 전체 금액 계산, 결제 내역 확인, 결제가 완료된 경우 Receupt & Kitchen Slip & Coupon Print
            //if (GblTestMode == true)
            //{
            //    return;
            //}

            string strtAmt = "";
            string strtGst = "";
            string strtPst = "";
            string strtHst = "";

            string sQBuff = string.Empty;
            long lReturn = 0;
            double dblTaxSum = 0;        
            //double dbSavingSum = 0;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string constr = commonClass.constringLocal;
            SqlConnection sqlcon = new SqlConnection(constr);
            sqlcon.Open();

            string sql;
            // 1. tb_OrderItem Loop 돌면서 Amount, Item Count, Tax 계산
            if (GintLocation != 3)                           // 벤쿠버, 토론토 인경우
            {
                sql = "Select Sum(tamt) as tAmt, Sum(tGst) as tGst, Sum(tPst) as tPst , Sum(tHst) as tHst From tb_OrderItem " +
                            "Where tInvno = '" + txtInvNo.Text + "' ";
            }
            else                                            // 미국 인 경우
            {
                sql = "Select Sum(tamt) as tAmt, Sum(tGst) as tGst, Sum(tPst) as tPst From tb_OrderItem " +
                            "Where tInvno = '" + txtInvNo.Text + "' ";
            }

            SqlCommand comm = new SqlCommand(sql, sqlcon);
            SqlDataReader rs = comm.ExecuteReader();
            while (rs.Read())
            {
                strtAmt = rs["tAmt"].ToString();
                strtGst = rs["tGst"].ToString();
                strtPst = rs["tPst"].ToString();
                if (GintLocation != 3)                           // 벤쿠버, 토론토 인경우
                {
                    strtHst = rs["tHst"].ToString();
                }
            }
            rs.Close();

            if (strtAmt != "")
            {
                lbSubTotalValCS.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(strtAmt));

                if(GintLocation != 3)                       // 벤쿠버 토론토 인 경우
                {
                    lbGSTValCS.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(strtGst));
                    lbPSTValCS.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(strtPst));

                    lbHSTValCS.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(strtHst));
                    lbTotalValCS.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(strtAmt) + Convert.ToDouble(strtGst) + Convert.ToDouble(strtPst) + Convert.ToDouble(strtHst));
                }
                else                                        // 미국 인 경우
                {
                    lbGSTValCS.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(strtGst) + Convert.ToDouble(strtPst));
                    lbTotalValCS.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(strtAmt) + Convert.ToDouble(strtGst) + Convert.ToDouble(strtPst));
                }
                
                
                //if (GintLocation != 3)                           // 벤쿠버, 토론토 인경우
                //{
                //    lbHSTValCS.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(strtHst));
                //    lbTotalValCS.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(strtAmt) + Convert.ToDouble(strtGst) + Convert.ToDouble(strtPst) + Convert.ToDouble(strtHst));
                //}
                //else
                //{
                //    lbTotalValCS.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(strtAmt) + Convert.ToDouble(strtGst) + Convert.ToDouble(strtPst));
                //}                
            }
            else
            {
                lbSubTotalValCS.Text = "0.00";
                lbGSTValCS.Text = "0.00";
                lbPSTValCS.Text = "0.00";
                lbHSTValCS.Text = "0.00";
                lbTotalValCS.Text = "0.00";
            }
            
            if (GintLocation == 3)              // 미국 인 경우
            {
                // EBT
                //string sQBuff = string.Empty;
                //long lReturn = 0;
                //double dblTaxSum = 0;

                GdblFoodStampAmt = 0;
                GdblFoodStampTax = 0;
                GdblFoodStampTaxableItemAmt = 0;

                lReturn = c_localdb.DBConnection();

                if (lReturn < 0)
                {
                    g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    return;
                }

                sQBuff = "SELECT tAmt, tGst, tPst, ISNULL(tTxExemAmt, 0) AS tTxExemAmt, ISNULL(tTxExem, 0) AS tTxExem " +
                         "FROM tb_OrderItem WHERE tInvno = '" + txtInvNo.Text + "' AND tFoodStamp <> 0 ";
                lReturn = c_localdb.RsOpen(sQBuff);

                if (lReturn == 1)
                {
                    while (!c_localdb.rs.EOF)
                    {
                        dblTaxSum = Convert.ToDouble(c_localdb.rs.Fields["tGst"].Value) + Convert.ToDouble(c_localdb.rs.Fields["tPst"].Value);

                        GdblFoodStampAmt += Convert.ToDouble(c_localdb.rs.Fields["tAmt"].Value);
                        GdblFoodStampTax += dblTaxSum;

                        if (dblTaxSum != 0)
                        {
                            GdblFoodStampTaxableItemAmt += Convert.ToDouble(c_localdb.rs.Fields["tAmt"].Value);
                        }

                        c_localdb.rs.MoveNext();
                    }                    
                }
                else
                {
                    g_sMessage = string.Format("[{0}] Tax Calculating query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    c_localdb.DBClose();
                }

                //c_localdb.RsClose();
            }

            //// Calc Saving Amount
            //sQBuff = "SELECT tIUprice, tOUprice, tPromo, tQty, tPunit " +
            //         "FROM tb_OrderItem WHERE tInvno = '" + txtInvNo.Text + "'";
            //lReturn = c_localdb.RsOpen(sQBuff);

            //if (lReturn == 1)
            //{
            //    while (!c_localdb.rs.EOF)
            //    {
            //        // Promotion 상품인 경우
            //        if (Convert.ToInt16(c_localdb.rs.Fields["tPromo"].Value) >= 1)
            //        {
            //            dbSavingSum += (Convert.ToDouble(c_localdb.rs.Fields["tIUprice"].Value) * Convert.ToDouble(c_localdb.rs.Fields["tQty"].Value)) - (Convert.ToDouble(c_localdb.rs.Fields["tOUprice"].Value) * Convert.ToDouble(c_localdb.rs.Fields["tQty"].Value));

            //            // Saving CS 및 CS Value 표시
            //            lbSavingCS.Visible = true;
            //            lbSavingValueCS.Visible = true;
            //        }
            //        c_localdb.rs.MoveNext();
            //    }

            //    // Saving 금액표시.
            //    lbSavingValueCS.Text = c_poscomlibs.getDoubleFormat(dbSavingSum);            
            //}
            //else
            //{
            //    g_sMessage = string.Format("[{0}] Calc Saving Amount error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
            //    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            //    c_localdb.DBClose();
            //}

            //c_localdb.RsClose();

            if (GPayFinish == true)
            {
                // Deli POS의 경우 Kitchen OrderSlip 출력 부분 추가

                // 
            }

            lblPayBalance.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(lbTotalValCS.Text) - Convert.ToDouble(lblPayTotal.Text) + Convert.ToDouble(lblPayPennyRounded.Text));
            lbSelectPaymentBalanceNum.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(lbTotalValCS.Text) - Convert.ToDouble(lblPayTotal.Text) + Convert.ToDouble(lblPayPennyRounded.Text));
            //calcPaymentTotal();
        }

        private int checkPromo(string strProd, string strProdPromo, string strPromoSdate, string strPromoEdate, string strQty, double dProdBottleDeposit, double dProdCrf)
        {
            string strCurDate = DateTime.Now.ToString("yyyy-MM-dd");
            int retType = 1;

            /*  1. 날짜 조건 */
            if (Convert.ToDateTime(strPromoEdate) >= Convert.ToDateTime(strCurDate))        // Promo End Date가 현재 날짜보다 큰(늦은) 경우 
            {
                if (Convert.ToDateTime(strPromoSdate) <= Convert.ToDateTime(strCurDate))    // Promo Start Date가 현재 날짜보다 작은(빠른) 경우
                {
                    retType = 1;
                }
                else
                {
                    retType = 0;
                    return retType;
                }
            }
            else
            {
                retType = 0;
                return retType;
            }

            /* 2. 수량 조건 */
            if (retType == 1 && strProdPromo == "1")  // 프로모션 등록된 Qty가 1인 경우 
            {
                retType = 1;
                return retType;
            }
            else
            {
                retType = 0;
            }

            /* 3. 멀티 구매 조건 */
            /* tb_OrderItem 에서 ProdId 검색하여 Record Count한 숫자와 Promo 등록된 숫자 비교 */
            /* Count 일치할 경우 tb_OrderItem에서 기존 스캔된 데이터는 삭제. 새로 스캔된 아이템 수량만 변경한다 */
            int promoQty = Convert.ToInt16(strProdPromo);
            
            if (promoQty > 1)
            {
                string sQBuff = string.Empty;
                long lReturn = 0;
                string strCount = "0";
                int iTempItemCount = 0;

                string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

                sQBuff = "Select Isnull(Sum(tQty),0) as tQty From hanamart.dbo.tb_OrderItem Where tInvNo = '" + txtInvNo.Text + "' And tProd = '" + strProd + "' And tPromo = '0' ";
            
                lReturn = c_localdb.RsOpen(sQBuff);

                if (lReturn == 1)
                {
                    if (c_localdb.record_count == 1)
                    {
                        iTempItemCount = Convert.ToInt16(c_localdb.rs.Fields["tQty"].Value);
                    }

                    // Deposit, CRF 값이 있는 경우 Count 다시 계산.
                    if (dProdBottleDeposit > 0 && dProdCrf > 0)
                    {
                        iTempItemCount = iTempItemCount / 3 + Convert.ToInt16(strQty);
                    }
                    else if(dProdBottleDeposit > 0 && dProdCrf == 0)
                    {
                        iTempItemCount = iTempItemCount / 2 + Convert.ToInt16(strQty);
                    }
                    else
                    {
                        iTempItemCount = iTempItemCount + Convert.ToInt16(strQty);
                    }

                    if (iTempItemCount >= promoQty)                 // 아이템리스트에 있는 상품의 갯수와 PromoQty와 비교 . 동일하거나 더 많은 수의 갯수가 있으면 True
                    {
                        g_iRegPriceItemCount = iTempItemCount % promoQty;                      // Reg Price로 계산되야 할 갯수 저장.
                        g_iPromoPriceItemCount = Convert.ToInt16(strQty) - g_iRegPriceItemCount;

                        sQBuff = "Delete From tb_OrderItem Where tInvNo = '" + txtInvNo.Text + "' And tProd = '" + strProd + "' And tPromo = '0' ";
                        c_localdb.DBExcute(sQBuff);

                        for (int i = ItemCSView.Items.Count - 1; i >= 0; --i)
                        {
                            if (ItemCSView.Items[i].SubItems[1].Text == strProd)
                            {
                                ItemCSView.Items[i].Remove();
                            }
                        }
                        
                        if(g_iRegPriceItemCount != 0)                   // Reg Price로 계산되어야 할 갯수가 있으면 전체 아이템갯수에서 빼서 전달. Reg Price는 다시 계산.
                        {
                            retType = iTempItemCount - g_iRegPriceItemCount;
                        }
                        else
                        {
                            retType = iTempItemCount;
                        }
                    }
                }
                else
                {
                    g_sMessage = string.Format("[{0}] Invoice number data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    c_localdb.DBClose();

                    return 0;
                }

                c_localdb.RsClose();
            }

            return retType;
        }
        /* ==================================== */
        private void AddOrderItem(OrderItem o)
        {
            string sQBuff = string.Empty;
            long lReturn = 0;
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            //if (GblTestMode == true)
            //{
            //    return;
            //}
            //else
            //{
            /* 필요한 Logic인지 확인
            If wQtyPromotion And Not wReCalcMember Then

                If wtType = "48" Then                             'Discount Item인 경우

                    wtIUprice = wQtyDCAmt
                    wtOUPrice = wQtyDCAmt
                    wtQty = wDCQtyPromo
                    wtAmt = wQtyDCAmt * wtQty
                Else

                    wtIUprice = wbIUprice_temp
                    wtOUPrice = wbOUprice_temp
                    wtAmt = wtOUPrice * wtQty
                End If
            End If
            */

            //string constr = commonClass.constringLocal;
            //SqlConnection sqlcon = new SqlConnection(constr);
            //sqlcon.Open();

            //2-1. tb_SoldItem
            string sql = "";

            if(GintLocation == 1)                       // 벤쿠버 인경우
            {
                sql = "INSERT INTO HANAMART.dbo.tb_OrderItem (tInvNo,tID,tDate,tTime,tProd,tPtype,tPtype2,tCat1,tCat2,tCat3,tCat4,tCat5,tCatCode,tQty,tPunit," +
                  "tIUprice,tOUprice,tWprice,tNative,tPromo,tPromoCode,tAmt,tGst,tPst,tHst,tTax,tCust,tPassWord,tPassStation,tUpCode,tSpecial,tFree,tMMBC,tSupp," +
                  "tType,tEntryCode,tFoodStamp,tGiftCardRef,tRelatedID,tMixMatch,tShift,tMemo) VALUES ('";

            }
            else if( GintLocation == 2)                 // 토론토 인경우
            {
                sql = "INSERT INTO HANAMART.dbo.tb_OrderItem (tInvNo,tID,tDate,tTime,tProd,tPtype,tPtype2,tCat1,tCat2,tCat3,tCat4,tCat5,tQty,tPunit," +
                  "tIUprice,tOUprice,tWprice,tNative,tPromo,tAmt,tGst,tPst,tHst,tTax,tCust,tPassWord,tPassStation,tUpCode,tSpecial,tFree,tSupp," +
                  "tType,tEntryCode,tFoodStamp,tGiftCardRef,tRelatedID,tMixMatch,tShift,tMemo) VALUES ('";
            }
            else                                        // 미국 인경우  다시봐야함.
            {
                sql = "INSERT INTO HANAMART.dbo.tb_OrderItem (tInvNo,tID,tDate,tTime,tProd,tPtype,tPtype2,tCat1,tCat2,tCat3,tCat4,tCat5,tQty,tPunit," +
                 "tIUprice,tOUprice,tWprice,tNative,tPromo,tAmt,tGst,tPst,tHst,tTax,tCust,tPassWord,tPassStation,tUpCode,tSpecial,tFree,tSupp," +
                 "tType,tEntryCode,tFoodStamp,tGiftCardRef,tRelatedID,tMixMatch,tShift,tMemo) VALUES ('";
            }
            
            sql += o.tInvNo + "',";
            // tID max찾아서 +1

            lReturn = c_localdb.DBConnection();

            if (lReturn < 0)
            {
                g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                return;
            }

            sQBuff = "SELECT ISNULL(MAX(tID), 0) + 1 AS seq " +
               "FROM hanamart.dbo.tb_orderitem " +
              "WHERE tInvNo = '" + txtInvNo.Text + "' ";

            lReturn = c_localdb.RsOpen(sQBuff);

            int iSeq = 0;
            if (lReturn == 1)
            {
                iSeq = Convert.ToInt32(c_localdb.rs.Fields["seq"].Value);
            }
            else
            {
                iSeq = 1;
            }
            //c_localdb.RsClose();

            sql += iSeq + ",";
            sql += "'" + DateTime.Now.ToString("yyyy-MM-dd") + "',";
            sql += "'" + DateTime.Now.ToString("h:mm:ss tt") + "',";
            sql += "'" + o.tProd + "',";
            sql += "'" + o.tPtype + "',";
            sql += "'" + o.tPtype2 + "',";
            sql += "'" + o.tCat1 + "',";
            sql += "'" + o.tCat2 + "',";
            sql += "'" + o.tCat3 + "',";
            sql += "'" + o.tCat4 + "',";
            sql += "'" + o.tCat5 + "',";
            if(GintLocation == 1)               // 벤쿠버 인경우
            {
                sql += "'',";
            }
                        
            sql += "'" + o.tQty + "',";
            sql += "'',"; // Unit

            sql += (Convert.ToDouble(o.tIUprice) == 0 ? o.tOUprice : o.tIUprice) + ",";
            sql += o.tOUprice + ",";
            sql += "0" + ",";
            sql += "'" + o.tNative + "',";
            sql += "'" + o.tPromo + "',";
            if(GintLocation == 1)               // 벤쿠버 인 경우
            {
                sql += "'" + o.tPromoCode + "',";
            }
            
            sql += o.tAmt + ",";
            sql += o.tGst + ",";
            sql += o.tPst + ",";
            sql += o.tHst + ",";
            sql += "'" + o.tTax + "',";
            sql += "'" + o.tCust + "',";
            sql += "'" + o.tPassWord + "',";
            sql += "'" + o.tPassStation + "',";
            sql += "'',"; // UpCode
            sql += "'" + o.tSpecial + "',";
            sql += "'" + o.tFree + "',";
            if(GintLocation == 1)           // 벤쿠버 인 경우
            {
                sql += "'" + o.tMMBC + "',";
            }
            
            sql += "'" + o.tSupp + "',";

            sql += "'" + o.tType + "',";
            sql += "'" + o.tEntryCode + "',";
            sql += o.tFoodStamp + ",";
            sql += "'" + o.tGiftCardRef + "',";
            sql += o.tRelatedID + ",";
            sql += "'" + o.tMixMatch + "',";
            sql += "'" + o.tShift + "',";
            sql += "'" + o.tMemo + "')";

            // Scale 단위에 따라 tQty 소수점 두자리(LB), 세자리(KG) 결정

            // 컬럼 추가
            //SqlCommand comm = new SqlCommand(sql, sqlcon);
            //comm.ExecuteNonQuery();
            
            if (c_localdb.DBExcute(sql) != 1)
            {
                g_sMessage = string.Format("[{0}] Item insert failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                c_localdb.DBClose();

                return;
            }
            else
            {
                g_sMessage = string.Format("[{0}] ({1} item added ({2}).", sMethod, o.tProd, c_poscominfo.ui_epno);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            }


            /* Total DC 아닌 항목만 화면에 표시되도록
             * 
             * '41:일반Total DC, 42:VIP DC, 43:직원 DC, 45:내부 DC, 46:도매결재보류 */
            //if (c_poscomlibs.GetTotalDCType(o.tType) == false) // Total DC인 경우 List에 표시하지 않음
            //{
            //    var prodItem = new ListViewItem(new[] { o.tProd, o.tID, o.tProdName, o.tQty, o.tOUprice, o.tAmt, o.tTax, o.tType });
            //    ItemCSView.Items.Add(prodItem);

            //    //var prodItemCT = new ListViewItem(new[] { o.tProd, o.tID, o.tProdName, o.tQty, o.tOUprice, o.tAmt, o.tTax });
            //    //ItemCTView.Items.Add(prodItemCT);
            //}

            //calcItemCountAdd();
            //calcTotalDisplay(o.tAmt, o.tGst, o.tPst, o.tHst);
            CntTotSalePayment();

            // Promotion 상품의 경우 화면 표시 색상 변경

            ClearOrderItem();
            //sqlcon.Close();
            //}
        }

        private void ClearOrderItem()
        {
            OrderItem o;
            o.tInvNo = "";
            o.tID = "";
            o.tProd = "";
            o.tProdName = "";
            o.tProdKname = "";
            o.tPtype = "";
            o.tPtype2 = "";
            o.tCat1 = "";
            o.tCat2 = "";
            o.tCat3 = "";
            o.tCat4 = "";
            o.tCat5 = "";
            o.tQty = "";
            o.tIUprice = "";
            o.tOUprice = "";
            o.tAmt = "";
            o.tGst = "";
            o.tPst = "";
            o.tHst = "";
            o.tTax = "";
            o.tType = "";
            o.tNative = "";
            o.tPromo = "";
            o.tPromoCode = "";
            o.tCust = "";
            o.tPassWord = "";
            o.tPassStation = "";
            o.tUpCode = "";
            o.tSpecial = "";
            o.tFree = "";
            o.tMMBC = "";
            o.tSupp = "";
            o.tEntryCode = "";
            o.tFoodStamp = "";
            o.tGiftCardRef = "";
            o.tRelatedID = "";
            o.tMixMatch = "";
            o.tShift = "";
            o.tMemo = "";

        }

        private void completeTransaction()
        {
            string strPayTotal = c_poscomlibs.getDoubleFormat(Convert.ToDouble(lblPayTotal.Text));
            string strTotalVal = c_poscomlibs.getDoubleFormat(Convert.ToDouble(lbTotalValCS.Text) - Convert.ToDouble(lblPayBalance.Text) + Convert.ToDouble(lblPayPennyRounded.Text));

            // 1. Total Amount와 Total Paid 비교
            if (strPayTotal == strTotalVal)
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

                    //2-1. tb_SoldItem
                    if (GintLocation == 1)                           // 벤쿠버 인경우
                    {
                        sQBuff = "INSERT INTO HANAMART.dbo.tb_SoldItem ";
                        sQBuff += "SELECT tInvNo,tID,tDate,tTime,tProd,tPtype,tPtype2,tCat1,tCat2,tCat3,tCat4,tCat5,tCatCode,tQty,tPunit,tIUprice";
                        sQBuff += ",tOUprice,tWprice,tNative,tPromo,tPromoCode,tAmt,tGst,tPst,tHst,tTax,tCust,tPassWord,tPassStation,tUpCode,tSpecial,tFree";
                        sQBuff += ",tMMBC,tSupp,tType,tEntryCode,tFoodStamp,tGiftCardRef,tRelatedID,tMixMatch,tShift,1 FROM HANAMART.dbo.tb_OrderItem ";
                    }
                    else if (GintLocation == 2)                           // 토론토 인경우
                    {
                        sQBuff = "INSERT INTO HANAMART.dbo.tb_SoldItem ";
                        sQBuff += "SELECT tInvNo,tID,tDate,tTime,tProd,tPtype,tPtype2,tCat1,tCat2,tCat3,tCat4,tCat5,tQty,tPunit,tIUprice";
                        sQBuff += ",tOUprice,tWprice,tNative,tPromo,tAmt,tGst,tPst,tHst,tTax,tCust,tPassWord,tPassStation,tUpCode,tSpecial,tFree";
                        sQBuff += ",tSupp,tType,tEntryCode,tFoodStamp,tGiftCardRef,tRelatedID,tMixMatch,tShift,1 FROM HANAMART.dbo.tb_OrderItem ";
                    }
                    else                                            // 미국 인 경우
                    {
                        sQBuff = "INSERT INTO HANAMART.dbo.tb_SoldItem ";
                        sQBuff += "SELECT tInvNo,tID,tDate,tTime,tProd,tPtype,tPtype2,tCat1,tCat2,tCat3,tCat4,tCat5,tQty,tPunit,tIUprice";
                        sQBuff += ",tOUprice,tWprice,tNative,tPromo,tAmt,tGst,tPst,tTax,tTxExemAmt,tTxExem,tRefunded,tCust,tPassWord,tPassStation,tUpCode,tSpecial,tFree";
                        sQBuff += ",tSupp,tType,tEntryCode,tFoodStamp,tGiftCardRef,tRelatedID,tShift,1 FROM HANAMART.dbo.tb_OrderItem ";
                    }

                    if (c_localdb.DBExcute(sQBuff) != 1)
                    {
                        g_sMessage = string.Format("[{0}] tb_SoldItem Item insert failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                        c_localdb.DBClose();

                        return;
                    }
                    else
                    {
                        g_sMessage = string.Format("[{0}] (tb_SoldItem data saved ({1}).", sMethod, c_poscominfo.ui_epno);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                    }

                    if (c_poscominfo.mi_cardno != "" && c_poscominfo.mi_cardno.Length == 12)
                    {
                        CntMemberPoint();
                    }

                    //2-2. tb_OrderItem Table
                    // Add - Table 삭제 
                    lReturn = c_localdb.DBConnection();

                    if (lReturn < 0)
                    {
                        g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                        return;
                    }

                    sQBuff = "DELETE FROM HANAMART.dbo.tb_OrderItem WHERE tInvno = '" + txtInvNo.Text + "' ";
                    if (c_localdb.DBExcute(sQBuff) != 1)
                    {
                        g_sMessage = string.Format("[{0}] Item Delete failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                        c_localdb.DBClose();

                        return;
                    }
                    else
                    {
                        g_sMessage = string.Format("[{0}] (tb_OrderItem data deleted successfully ({1}).", sMethod, c_poscominfo.ui_epno);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                    }

                    string strCurDate = DateTime.Now.ToString("yyyy-MM-dd");
                    string strCurTime = DateTime.Now.ToString("h:mm:ss tt");
                    string strStationNo = g_strCounterNum;
                    double dblPaid = (Convert.ToDouble(strPayTotal));
                    //string strCash = c_poscomlibs.getDoubleFormat(Convert.ToDouble(lblPayCash.Text) + Convert.ToDouble(lblPayBalance.Text));
                    string strCash = c_poscomlibs.getDoubleFormat(Convert.ToDouble(lblPayCash.Text));
                    //string strChange = c_poscomlibs.getDoubleFormat(Convert.ToDouble(lblPayBalance.Text) * -1);
                    string strChange = "0";
                    //string strEmpBal = c_poscomlibs.getDoubleFormat(c_poscominfo.mi_staffbal - Convert.ToDouble(lblDCAmount.Text));

                    string strEmpBal = "";
                    if (lblDCAmount.Text != "")
                    {
                        strEmpBal = c_poscomlibs.getDoubleFormat(c_poscominfo.mi_staffbal - Convert.ToDouble(lblDCAmount.Text));
                    }
                    else
                    {
                        strEmpBal = c_poscomlibs.getDoubleFormat(c_poscominfo.mi_staffbal);
                    }

                    //2-3. tb_Payment
                    sQBuff = "INSERT INTO HANAMART.dbo.tb_Payment VALUES (";
                    sQBuff += "'" + txtInvNo.Text + "', '" + strCurDate + "', '" + strCurTime + "',";
                    // Payment 종류별 값 저장
                    if(GintLocation == 1)                           // 벤쿠버인경우
                    {
                        sQBuff += strCash + "," + lblPayDebit.Text + "," + c_poscomlibs.getDoubleFormat(CardPay[iVisa]) + "," + c_poscomlibs.getDoubleFormat(CardPay[iMaster]) + ",";
                        sQBuff += c_poscomlibs.getDoubleFormat(CardPay[iAmex]) + "," + c_poscomlibs.getDoubleFormat(CardPay[iDisc]) + "," + c_poscomlibs.getDoubleFormat(CardPay[iUP]) + ",";
                        sQBuff += c_poscomlibs.getDoubleFormat(CardPay[iCardETC]) + "," + c_poscomlibs.getDoubleFormat(AliPay[iAli]) + "," + c_poscomlibs.getDoubleFormat(AliPay[iWechat]) + ",";
                        sQBuff += c_poscomlibs.getDoubleFormat(CardPay[iUnion]) + "," + lblPayCheck.Text + ",''," + c_poscomlibs.getDoubleFormat(CardPay[iEBT]) + "," + c_poscomlibs.getDoubleFormat(CardPay[iGiftCard]) + ",";
                        sQBuff += lblPayCerti.Text + ",'','','',''," + lblPayHmoney.Text + "," + lblPayCoupon.Text + ",0," + lbGSTValCS.Text + "," + lbPSTValCS.Text + "," + lbHSTValCS.Text + ",";
                        sQBuff += dblPaid.ToString() + "," + lblPayPennyRounded.Text + "," + strChange + ",";
                    }
                    else if (GintLocation == 2)                           // 토론토 인경우
                    {
                        sQBuff += strCash + "," + lblPayDebit.Text + "," + c_poscomlibs.getDoubleFormat(CardPay[iVisa]) + "," + c_poscomlibs.getDoubleFormat(CardPay[iMaster]) + ",";
                        sQBuff += c_poscomlibs.getDoubleFormat(CardPay[iAmex]) + "," + c_poscomlibs.getDoubleFormat(CardPay[iDisc]) + "," + c_poscomlibs.getDoubleFormat(CardPay[iUP]) + ",";
                        sQBuff += c_poscomlibs.getDoubleFormat(CardPay[iCardETC]) + ",";
                        sQBuff += lblPayCheck.Text + ",''," + c_poscomlibs.getDoubleFormat(CardPay[iEBT]) + "," + c_poscomlibs.getDoubleFormat(CardPay[iGiftCard]) + ",";
                        sQBuff += lblPayCerti.Text + ",'','','',''," + lblPayHmoney.Text + "," + lblPayCoupon.Text + ",0," + lbGSTValCS.Text + "," + lbPSTValCS.Text + "," + lbHSTValCS.Text + ",";
                        sQBuff += dblPaid.ToString() + "," + lblPayPennyRounded.Text + "," + strChange + ",";
                    }
                    else                                            // 미국 인 경우
                    {
                        sQBuff += strCash + "," + lblPayDebit.Text + "," + c_poscomlibs.getDoubleFormat(CardPay[iVisa]) + "," + c_poscomlibs.getDoubleFormat(CardPay[iMaster]) + ",";
                        sQBuff += c_poscomlibs.getDoubleFormat(CardPay[iAmex]) + "," + c_poscomlibs.getDoubleFormat(CardPay[iDisc]) + "," + c_poscomlibs.getDoubleFormat(CardPay[iDiners]) + ",";
                        sQBuff += c_poscomlibs.getDoubleFormat(CardPay[iCardETC]) + ", ";
                        sQBuff += lblPayCheck.Text + ",''," + c_poscomlibs.getDoubleFormat(CardPay[iEBT]) + "," + c_poscomlibs.getDoubleFormat(CardPay[iGiftCard]) + ",";
                        sQBuff += lblPayCerti.Text + ",''," + lblPayHmoney.Text + "," + lblPayCoupon.Text + "," + lbGSTValCS.Text + "," + lbPSTValCS.Text + ",";
                        sQBuff += dblPaid.ToString() + "," + strChange + ",";
                    }
                    // 포인트정보 및 캐쉬어 정보
                    sQBuff += "'" + c_poscominfo.mi_cardno + "',"; //colCust
                    sQBuff += "'" + c_poscominfo.mi_store + "',"; //colcStore
                    sQBuff += c_poscominfo.mi_custno + ","; //colCustNo
                    sQBuff += "'" + strEmpBal + "',"; //colType
                    sQBuff += "'" + strStationNo + "',"; //colStation
                    sQBuff += "'" + txtEmpNo.Text + "',"; //colPassword
                    sQBuff += "0,"; //colOrigin
                    sQBuff += (lbPrePointCS.Text == "" ? "0" : lbPrePointCS.Text.Replace(",", "")) + ","; //colHPPrev
                    sQBuff += (lbEarnPointCS.Text == "" ? "0" : lbEarnPointCS.Text.Replace(",", "")) + ","; //colHPEarn
                    sQBuff += "0,"; //colHPBonus
                    sQBuff += (lbUsedPointCS.Text == "" ? "0" : lbUsedPointCS.Text.Replace(",", "")) + ","; //colHPUsed
                    sQBuff += (lbBalancePointCS.Text == "" ? "0" : lbBalancePointCS.Text.Replace(",", "")) + ","; //colHPBal
                    sQBuff += "'',"; //colShift
                    sQBuff += "0,"; //colID
                    sQBuff += "1)"; //colUpFlag
                    if (c_localdb.DBExcute(sQBuff) != 1)
                    {
                        g_sMessage = string.Format("[{0}] tb_Payment Item insert failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                        c_localdb.DBClose();

                        return;
                    }
                    else
                    {
                        g_sMessage = string.Format("[{0}] (Payment data inserted ({1}).", sMethod, c_poscominfo.ui_epno);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                    }

                    c_localdb.RsClose();

                    // 3. Receipt Print
                    // Receipt
                    GstrPrtInvno = txtInvNo.Text;
                    GstrReprint = "N"; // 판매 후 일반 Print

                    // 영수증 출력 화면으로 전환.
                    transitionSiglePage(gbReceiptPrinting, 253, 200);
                    btnPointUseYes.Enabled = false;
                    btnPointUseNo.Enabled = false;

                    // Printer Process
                    bgw_ReceiptPrinting.RunWorkerAsync();

                    // Order Slip for Deli
                    string strMkno = "61"; // 테스트용 임시 할당
                    string strStno = "99";

                    if ((strMkno == "61" && strStno == "8") || (strMkno == "64" && strStno == "5")) // 하드코딩 없이 적용할 수 있는 방법 연구..
                    {
                        prtOrderSlip("1");

                        if (strMkno == "64")
                        {
                            prtOrderSlip("2");
                        }
                    }

                    // 4. Open Cash Drawer
                    //c_poscomlibs.OpenCashDrawer();

                    // 5. New Order Start
                    //5-1. Clear Variable
                    //5-2. Initializing Textbox & Label & ListView
                    initListView();
                    initScale();
                    
                    // KeyBuff에 최종 Invoice 정보 입력
                    SaveInvNo();

                    /* 초기화 시키는 부분은 결제 이후 새로운 아이템이 스캔되는 시점으로..
                    initSalesTotal();
                    initPayment();
                    // Add - 멤버십 정보 초기화
                    InitMemberDisplay();
                    InitMemberInfo();
                    // Add - Discount 정보 초기화
                    initDClabel();
                    //5-3. New Invoice No
                    GetNewInvNo();
                    */
                    GblComplete = true;
                    
                    //5-4. New Item Scan 
                    KeyInReady();

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
        }

        private void CntMemberPoint()
        {

            double dblHMSaved = 0;
            double dblTotAmt = 0;
            double dblHMSavingItem = 0;
            double dblHMEarnToday = 0;
            double dblHMEarnTodayCard = 0;
            double dblDiscountAmt = 0;
            double dblSkipAmt = 0;
            double dblHPToday = 0;

            double dblCash, dblDebit, dblVisa, dblMaster, dblAmex, dblDisc, dblUP, dblCardETC, dblDelivery, dblEBT, dblHmoney, dblGiftcard, dblGiftCerti, dblEarnTodayCard;
            double dblAli, dblWechat, dblUnionQR;

            bool blDCbit = false;

            dblCash = Convert.ToDouble(lblPayCash.Text);
            dblDebit = Convert.ToDouble(lblPayDebit.Text);

            dblVisa = Convert.ToDouble(CardPay[iVisa]);
            dblMaster = Convert.ToDouble(CardPay[iMaster]);
            dblAmex = Convert.ToDouble(CardPay[iAmex]);
            dblDisc = Convert.ToDouble(CardPay[iDisc]);
            dblUP = Convert.ToDouble(CardPay[iUP]);
            dblCardETC = Convert.ToDouble(CardPay[iCardETC]);

            dblDelivery = Convert.ToDouble(lblPayCheck.Text);
            dblEBT = Convert.ToDouble(lblPayEBT.Text);
            dblHmoney = Convert.ToDouble(lblPayHmoney.Text);
            dblGiftcard = Convert.ToDouble(lblPayGiftCard.Text);
            dblGiftCerti = Convert.ToDouble(lblPayCerti.Text);

            string sQBuff = string.Empty;

            long lReturn = 0;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            try
            {
                lReturn = c_localdb.DBConnection();

                if (lReturn < 0)
                {
                    g_sMessage = string.Format("[{0}] Remote database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    return;
                }

                // Detail Order List
                string strtprod = string.Empty;
                string strtPtype = string.Empty;
                string strtType = string.Empty;
                string strtPunit = string.Empty;
                string strtIUprice = string.Empty;
                string strtOUPrice = string.Empty;
                string strtQty = string.Empty;
                string strtAmt = string.Empty;
                string strtWprice = string.Empty;
                string strtNative = string.Empty;
                string strtTax = string.Empty;
                string strtGst = string.Empty;
                string strtPst = string.Empty;
                string strtHst = string.Empty;
                string strtPromo = string.Empty;
                string strtspecial = string.Empty;
                string strprodName = string.Empty;
                string strprodKname = string.Empty;
                string strprodDeposit = string.Empty;
                string strCustomerReceipt = string.Empty;

                if(GintLocation != 3)                           // 벤쿠버 토론토 인경우
                {
                    sQBuff = "Select tprod, tPtype, tType, tPunit, tIUprice, tOUPrice, tQty, tAmt, tWprice, tNative, tTax, tGst, tPst, tHst, tPromo, tspecial "
                            + "From HANAMART.dbo.tb_orderitem " + "where tInvNo = '" + txtInvNo.Text + "'";
                }
                else                                            // 미국 인 경우
                {
                    sQBuff = "Select tprod, tPtype, tType, tPunit, tIUprice, tOUPrice, tQty, tAmt, tWprice, tNative, tTax, tGst, tPst, tPromo, tspecial "
                            + "From HANAMART.dbo.tb_orderitem " + "where tInvNo = '" + txtInvNo.Text + "'";
                }
                

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
                    while (c_localdb.rs.EOF != true)
                    {
                        if (c_localdb.rs.RecordCount != 0)
                        {
                            strtprod = Convert.ToString(c_localdb.rs.Fields["tprod"].Value);
                            strtPtype = Convert.ToString(c_localdb.rs.Fields["tPtype"].Value);
                            strtType = Convert.ToString(c_localdb.rs.Fields["tType"].Value);
                            strtPunit = Convert.ToString(c_localdb.rs.Fields["tPunit"].Value);
                            strtIUprice = Convert.ToString(c_localdb.rs.Fields["tIUprice"].Value);
                            strtOUPrice = Convert.ToString(c_localdb.rs.Fields["tOUPrice"].Value);
                            strtQty = Convert.ToString(c_localdb.rs.Fields["tQty"].Value);
                            strtAmt = string.Format("{0:0.00}", double.Parse(Convert.ToString(c_localdb.rs.Fields["tAmt"].Value)));
                            strtWprice = Convert.ToString(c_localdb.rs.Fields["tWprice"].Value);
                            strtNative = Convert.ToString(c_localdb.rs.Fields["tNative"].Value);
                            strtTax = Convert.ToString(c_localdb.rs.Fields["tTax"].Value);
                            strtGst = Convert.ToString(c_localdb.rs.Fields["tGst"].Value);
                            strtPst = Convert.ToString(c_localdb.rs.Fields["tPst"].Value);
                            if(GintLocation != 3)               // 벤쿠버 토론토 인 경우
                            {
                                strtHst = Convert.ToString(c_localdb.rs.Fields["tHst"].Value);
                            }
                            else                                // 미국 인경우
                            {
                                strtHst = "0";
                            }
                            
                            strtPromo = Convert.ToString(c_localdb.rs.Fields["tPromo"].Value);
                            strtspecial = Convert.ToString(c_localdb.rs.Fields["tspecial"].Value);
                            //strprodName = Convert.ToString(c_localdb.rs.Fields["prodName"].Value);
                            //strprodKname = Convert.ToString(c_localdb.rs.Fields["prodKname"].Value);
                            //strprodDeposit = Convert.ToString(c_localdb.rs.Fields["prodDeposit"].Value);

                            int inttType = 0;
                            if (strtType != "")
                            {
                                inttType = Convert.ToInt16(strtType);
                            }

                            if (strtspecial == "A" || strtspecial == "B")
                            {
                                dblHMSaved = dblHMSaved + (Convert.ToDouble(strtQty) * Convert.ToDouble(strtIUprice) - Convert.ToDouble(strtAmt));
                                dblHMSavingItem = dblHMSavingItem + Convert.ToDouble(strtAmt) + Convert.ToDouble(strtGst) + Convert.ToDouble(strtPst) + Convert.ToDouble(strtHst);
                            }
                            else
                            {
                                if (inttType == 11 || (41 <= inttType && inttType <= 48) || (31 <= inttType && inttType <= 39))
                                {
                                    // 할인상품에 대한 포인트 지급 안함
                                }
                                else
                                {
                                    dblHMEarnToday = dblHMEarnToday + Convert.ToDouble(strtAmt) + Convert.ToDouble(strtGst) + Convert.ToDouble(strtPst) + Convert.ToDouble(strtHst);
                                }

                            }

                            if (strtPtype == "94" || (41 <= inttType && inttType <= 48))
                            {
                                dblDiscountAmt = dblDiscountAmt + Convert.ToDouble(strtAmt) + Convert.ToDouble(strtGst) + Convert.ToDouble(strtPst) + Convert.ToDouble(strtHst);
                            }

                            // 'VIP DC, 직원할인을 제외한 전체할인이 된경우 전 상픔에 대한 포인트 지급 안함.
                            if ((41 <= inttType && inttType <= 45) && inttType != 42 && inttType != 43)
                            {
                                blDCbit = true;
                            }

                            if (31 <= inttType && inttType <= 39)
                            {
                                dblSkipAmt = dblSkipAmt + Convert.ToDouble(strtAmt);
                            }

                            dblTotAmt = dblTotAmt + Convert.ToDouble(strtAmt);

                            c_localdb.rs.MoveNext();
                        }
                        else
                        {
                            g_sMessage = string.Format("[{0}] Order Items Information is not found (Invoice Number : {1}).", sMethod, txtInvNo.Text);
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                            c_localdb.RsClose();
                            return;
                        }
                    }
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

            if (GintLocation == 1) // 밴쿠버
            {
                dblHMEarnToday = Math.Round(dblHMEarnToday, 2);
                dblHMEarnTodayCard = 0;
            }
            else if (GintLocation == 2) // 토론토
            {
                if (dblSkipAmt != 0)
                {
                    dblHMEarnToday = 0;
                    dblHMEarnTodayCard = 0;
                }
                else
                {
                    dblHMEarnToday = Math.Round(dblCash + dblDebit + dblDelivery + dblGiftcard + dblGiftCerti, 0);  //토론토 적용   
                    dblHMEarnTodayCard = Math.Round(dblVisa + dblMaster + dblAmex + dblDisc + dblUP, 0);
                }
            }
            else if (GintLocation == 3) // 미국
            {
                //dblHMEarnToday = Math.Truncate(dblHMEarnToday);
                dblHMEarnToday = Math.Round(dblHMEarnToday, 2);
                dblHMEarnTodayCard = 0;
            }

            if (dblHMSavingItem + dblHMEarnToday + dblHMEarnTodayCard > 0) //H-Point를 주어야 할 경우 (돈을 받아야 하는 경우)
            {
                if (dblHmoney >= Math.Round(dblHMEarnToday + dblHMEarnTodayCard, 0) && Math.Round(dblHMEarnToday + dblHMEarnTodayCard, 0) >= 0) //받은 H-Money가 0보다 크고 지불한 H-Money가 받은 H-Money 보다 큰 경우 
                {
                    dblHPToday = 0;
                }
                else
                {
                    if (GintLocation == 1)
                    {
                        if (blDCbit == false)
                        {
                            dblHPToday = Math.Round(Math.Round(dblHMEarnToday + dblHMEarnTodayCard, 0) - dblHmoney, 0) * 5; // 밴쿠버
                        }
                        else
                        {
                            dblHPToday = 0;
                        }
                    }
                    else if (GintLocation == 2)
                    {
                        dblHPToday = Math.Round(dblHMEarnToday * 5, 0) + Math.Round(dblHMEarnTodayCard * 2, 0); // 토론토
                    }
                    else if (GintLocation == 3) // 미국
                    {
                        if (blDCbit == false)
                        {
                            dblHPToday = Math.Round(Math.Round(dblHMEarnToday + dblHMEarnTodayCard, 0) - dblHmoney, 0); // 미국
                            dblHPToday = Math.Truncate(dblHPToday);
                        }
                        else
                        {
                            dblHPToday = 0;
                        }
                    }
                }
            }
            else
            {
                if (dblHmoney <= Math.Round(dblHMEarnToday + dblHMEarnTodayCard) && Math.Round(dblHMEarnToday + dblHMEarnTodayCard) <= 0) // H-Point를 돌려받아야 할 경우 (돈을 내주어야 하는 경우)
                {
                    dblHPToday = 0;
                }
                else
                {
                    if (dblHMEarnToday + dblHMEarnTodayCard + dblDiscountAmt <= 0 && dblDiscountAmt < 0) // Discount에 의한 H-Point 없게 ...
                    {
                        dblHPToday = 0;
                    }
                    else
                    {
                        if (GintLocation == 1)
                        {
                            if (blDCbit == true)
                            {
                                dblHPToday = Math.Round(Math.Round(dblHMEarnToday + dblHMEarnTodayCard, 0) - dblHmoney, 0) * 5; // 밴쿠버
                            }
                            else
                            {
                                dblHPToday = 0;
                            }
                        }
                        else if (GintLocation == 2)
                        {
                            dblHPToday = Math.Round(dblHMEarnToday * 5, 0) + Math.Round(dblHMEarnTodayCard * 2, 0); // 토론토
                        }
                        else if (GintLocation == 3) // 미국
                        {
                            if (blDCbit == false)
                            {
                                dblHPToday = Math.Round(Math.Round(dblHMEarnToday + dblHMEarnTodayCard, 0) - dblHmoney, 0); // 미국
                                dblHPToday = Math.Truncate(dblHPToday);
                            }
                            else
                            {
                                dblHPToday = 0;
                            }
                        }
                    }
                }

            }

            /* 필요한 부분인지 확인 필요.
            If wHPPromoSW And wHPsDate <= Date And Date <= wHPeDate Then
                If wTotAmt >= wHPLmtAmt Then
                    If wHPDblSW = True Then
                        wHPSrv = wHPToday
                    End If


                    wHPBonus = wHPSrv
                End If
            End If
            */

            lbEarnPointCS.Text = string.Format("{0:###,##0}", dblHPToday);
            lbUsedPointCS.Text = string.Format("{0:###,##0}", GdblHPTodayUsed);
            lbBalancePointCS.Text = string.Format("{0:###,##0}", c_poscominfo.mi_pointbalance + dblHPToday - GdblHPTodayUsed);
            lbBalancePointTop.Text = string.Format("{0:###,##0}", c_poscominfo.mi_pointbalance + dblHPToday - GdblHPTodayUsed);

        }
        private void btnBack_Click(object sender, EventArgs e)
        {
            string sQBuff = string.Empty;
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            //g_sMessage = string.Format("[{0}] Push Back Button (ctrlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            g_sMessage = string.Format("[{0}] Push Back Button", sMethod);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            // 결제 내역이 있는 경우
            if (lblPayTotal.Text != "0.00")
            {
                btnSearch.Enabled = false;
                btnSearch_Category.Enabled = false;
                btnBack.Enabled = false;
                btnItemCorrect.Enabled = false;
                btnVoid.Enabled = false;
                btnItemDiscount.Enabled = false;
                btnReprint.Enabled = false;
                btnSuspend.Enabled = false;
                ItemCSView.Enabled = false;

                btnAddBagToCart.Enabled = false;
                btnNoBag.Enabled = false;
                btnBagPlus.Enabled = false;
                btnMinus.Enabled = false;

                btnSelectCreditCard.Enabled = false;
                btnSelectPointCard.Enabled = false;
                btnSelectGiftCard.Enabled = false;
                btnEBT.Enabled = false;
                btnManualETCKey.Enabled = false;

                DisplayErrorMessageBox("VOID", "BACK OPERATION NOT ALLOWED\n WHEN PAYMENT EXIST", 1, sMethod);
                return;
            }

            if (ctrlOnScreen == pn_ItemScan || ctrlOnScreen == pnItemScanSearchBtn || ctrlOnScreen == gbErrorBox)
            {
                // Item Void Error Message
                btnSearch.Enabled = false;
                btnSearch_Category.Enabled = false;
                btnBack.Enabled = false;
                btnItemCorrect.Enabled = false;
                btnVoid.Enabled = false;
                btnItemDiscount.Enabled = false;
                btnHelp.Enabled = false;
                btnNext.Enabled = false;
                btnReprint.Enabled = false;
                btnSuspend.Enabled = false;
                btnManualETCKey.Enabled = false;

                g_ManagerCardScanReady = true;
                g_mMaterFunctionVal = ManualMasterFunction.ItemBackVoid;

                if (c_poscominfo.ci_mkno != "55" && c_poscominfo.ci_mkno != "62")                // 55 오로라, 62 다운타운 매장이 아닌 경우 Manager Key Scan 필요.
                {
                    ctrTemp = ctrlOnScreen;
                    transitionSiglePage(gbScanManagerCard, 308, 200);
                    cpScanManagerCard.IsRunning = true;
                }
                else                                            // 55 오로라, 62 다운타운 매장인 경우 Manager Key Scan 필요 없음.
                {
                    ProcessEntryCode(g_strManagerKey, 0);
                }

                KeyInReady();

                //DisplayErrorMessageBox("VOID", "Warning!! All item list will be deleted.\nAre you sure? ", 2, sMethod);
            }
            else if (ctrlOnScreen == pnAddBag)
            {
                transitionDoublePage(pnItemScanSearchBtn, pnAddBag, 584, 1 * GROUP_BOX_LEFT, 300);
                ctrlOnScreen = pn_ItemScan;

                BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
                BackToStartTimerFromItemScan.Start();

                tbStep2Name.ForeColor = System.Drawing.Color.Silver;
                tbStep1Name.ForeColor = System.Drawing.Color.DimGray;
                st_ProcessStatus.CurrentStep = 1;
                
                if (GintLocation == 1)           // 벤쿠버 인 경우
                {
                    if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
                    {
                        btnNext.Enabled = false;
                    }
                    else
                    {
                        btnNext.Enabled = true;
                    }
                    // Back Image Exit로 변경
                    btnBack.Image = global::HanaSales_SelfCheckOut.Properties.Resources.Exit;

                    if (g_HelpModeOn == false)          // Help Mode 가 아닌 경우.
                    {
                        btnBack.Visible = false;
                        btnBack.Enabled = false;
                        btnBack.Image = global::HanaSales_SelfCheckOut.Properties.Resources.new_Back;
                    }
                }
                else if(GintLocation == 2)      // 토론토 인 경우
                {
                    if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
                    {
                        btnNext.Enabled = false;
                    }
                    else
                    {
                        btnNext.Enabled = true;
                    }
                }
                else                            // 미국 인경우
                {
                    if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
                    {
                        btnNext.Enabled = false;
                    }
                    else
                    {
                        btnNext.Enabled = true;
                    }

                    // Back Image Exit로 변경
                    btnBack.Image = global::HanaSales_SelfCheckOut.Properties.Resources.Exit;
                    
                    if (g_HelpModeOn == false)          // Help Mode 가 아닌 경우.
                    {
                        btnBack.Visible = false;
                        btnBack.Enabled = false;
                        btnBack.Image = global::HanaSales_SelfCheckOut.Properties.Resources.new_Back;
                    }                    
                }

                btnSearch.Enabled = true;
                btnSearch_Category.Enabled = true;
                btnNext.Visible = true;
                //btnNext.Text = "NEXT";
                btnNext.Font = new System.Drawing.Font("Helvetica85-Heavy", 18F, FontStyle.Bold);

                // 저울 스캐너 ON
                if (OPOSScanner.DeviceEnabled == false)
                {
                    _EnableScannerDevice();
                }
            }
            else if (ctrlOnScreen == pnSelectPayment || ctrlOnScreen == gbProcessCreditCard || ctrlOnScreen == gbProcessPointCard)
            {
                pnSelectPayment.Visible = true;

                if (c_poscominfo.ci_mkno != "86")                                                   // 86 West Jordan 매장이 아닌 경우
                {
                    transitionDoublePage(pnAddBag, pnSelectPayment, 584, 1 * GROUP_BOX_LEFT, 300);
                    tbStep3Name.ForeColor = System.Drawing.Color.Silver;
                    tbStep2Name.ForeColor = System.Drawing.Color.DimGray;
                    st_ProcessStatus.CurrentStep = 2;
                    //btnNext.Text = "NEXT";
                    btnNext.Font = new System.Drawing.Font("Helvetica85-Heavy", 18F, FontStyle.Bold);
                }
                else                                                                                // 86 West Jordan 매장인 경우
                {
                    transitionDoublePage(pnAddBag, pnSelectPayment, 584, 1 * GROUP_BOX_LEFT, 300);
                    transitionDoublePage(pnItemScanSearchBtn, pnAddBag, 584, 1 * GROUP_BOX_LEFT, 300);
                    ctrlOnScreen = pn_ItemScan;

                    BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
                    BackToStartTimerFromItemScan.Start();
                    
                    tbStep2Name.ForeColor = System.Drawing.Color.Silver;
                    tbStep1Name.ForeColor = System.Drawing.Color.DimGray;
                    st_ProcessStatus.CurrentStep = 1;

                    if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
                    {
                        btnNext.Enabled = false;
                    }
                    else
                    {
                        btnNext.Enabled = true;
                    }

                    // Back Image Exit로 변경
                    btnBack.Image = global::HanaSales_SelfCheckOut.Properties.Resources.Exit;

                    if (g_HelpModeOn == false)          // Help Mode 가 아닌 경우.
                    {
                        btnBack.Visible = false;
                        btnBack.Enabled = false;
                        btnBack.Image = global::HanaSales_SelfCheckOut.Properties.Resources.new_Back;
                    }

                    btnSearch.Enabled = true;
                    btnSearch_Category.Enabled = true;
                    btnNext.Visible = true;
                    //btnNext.Text = "NEXT";
                    btnNext.Font = new System.Drawing.Font("Helvetica85-Heavy", 18F, FontStyle.Bold);

                    // 저울 스캐너 ON
                    if (OPOSScanner.DeviceEnabled == false)
                    {
                        _EnableScannerDevice();
                    }
                }
            }
            else
            {
                // Item Void Error Message
                btnSearch.Enabled = false;
                btnSearch_Category.Enabled = false;
                btnBack.Enabled = false;
                btnItemCorrect.Enabled = false;
                btnVoid.Enabled = false;
                btnItemDiscount.Enabled = false;
                btnHelp.Enabled = false;
                btnNext.Enabled = false;
                btnReprint.Enabled = false;
                btnSuspend.Enabled = false;
                
                g_ManagerCardScanReady = true;
                g_mMaterFunctionVal = ManualMasterFunction.ItemBackVoid;

                if (c_poscominfo.ci_mkno != "55" && c_poscominfo.ci_mkno != "62")                // 55 오로라, 62 다운타운 매장이 아닌 경우 Manager Key Scan 필요.
                {
                    ctrTemp = ctrlOnScreen;
                    transitionSiglePage(gbScanManagerCard, 308, 200);
                    cpScanManagerCard.IsRunning = true;
                }
                else                                            // 55 오로라, 62 다운타운 매장인 경우 Manager Key Scan 필요 없음.
                {
                    ProcessEntryCode(g_strManagerKey, 0);
                }

                KeyInReady();

                //DisplayErrorMessageBox("VOID", "Warning!! All item list will be deleted.\nAre you sure? ", 2, sMethod);
            }

            KeyInReady();
        }
        
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            g_sMessage = string.Format("[{0}] Push Search Button (ctrlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            BackToStartTimerFromItemScan.Stop();

            //Timer Start
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();

            ctrTemp = ctrlOnScreen;
            ctrlOnScreen = gbSearchBox;
            ctrTemp2 = ctrlOnScreen;

            gbSearchBox.BringToFront();
            gbSearchBox.Visible = true;

            btnBack.Enabled = false;
            btnNext.Enabled = false;
            btnSearch.Enabled = false;
            btnSearch_Category.Enabled = false;
            gbHelp.Visible = false;
            btnHelp.Enabled = false;
            btnItemCorrect.Enabled = false;
            btnVoid.Enabled = false;
            btnItemDiscount.Enabled = false;
            btnReprint.Enabled = false;
            btnSuspend.Enabled = false;
            btnSearchCategory_All.Text = "ALL";
            btnSearchOrKeyinItem.Text = "Search Or Key in Item";
            btnBackToCategory.Text = "Back to All Items";

            lvSearchImage_Item.BringToFront();
            lvSearchImage_Category.SendToBack();
            pn_Keyboard.BringToFront();

            pn_Keyboard.Visible = true;
            btnKeyboardClear.Text = "CLEAR";
            btnKeyboardClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(46)))), ((int)(((byte)(95)))));

            btnSearchPageLeft.Visible = false;
            btnSearchPageRight.Visible = false;

            // Page bug 
            g_SearchItemPageNum = 0;
            g_SearchItemMaxPageNum = 0;

            txtSearchCode.Focus();
            // All Image 출력
            //GetimageListView("ALL", ImageType.Item);
            GetimageListView("INIT", ImageType.Item);

            // 저울에서 스캔 되는거 방지.
            if (OPOSScanner.DeviceEnabled)
            {
                OPOSScanner.DataEvent -= ScannerDataEvent;
                OPOSScanner.DeviceEnabled = false;
            }

            // 음성 실행.
            GssSearchItem.SelectVoice(GstrVoice);
            GssSearchItem.SpeakAsync("Search the Item...Push the Item name or THE P L U Number.");
        }

        private void btnSearch_Category_Click(object sender, EventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            g_sMessage = string.Format("[{0}] Push Search Category Button (ctrlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            BackToStartTimerFromItemScan.Stop();

            //Timer Start
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();

            gbSearchBox.BringToFront();
            gbSearchBox.Visible = true;

            btnBack.Enabled = false;
            btnNext.Enabled = false;
            btnSearch.Enabled = false;
            btnSearch_Category.Enabled = false;
            gbHelp.Visible = false;
            btnHelp.Enabled = false;
            btnItemCorrect.Enabled = false;
            btnVoid.Enabled = false;
            btnItemDiscount.Enabled = false;
            btnReprint.Enabled = false;
            btnSuspend.Enabled = false;
            pn_Keyboard.Visible = false;

            btnSearchPageLeft.Visible = false;
            btnSearchPageRight.Visible = false;

            // Page bug 
            g_SearchItemPageNum = 0;
            g_SearchItemMaxPageNum = 0;

            btnSearchCategory_TZ.Enabled = true;
            btnSearchCategory_QS.Enabled = true;
            btnSearchCategory_P.Enabled = true;
            btnSearchCategory_NO.Enabled = true;
            btnSearchCategory_KM.Enabled = true;
            btnSearchCategory_DJ.Enabled = true;
            btnSearchCategory_BC.Enabled = true;
            btnSearchCategory_A.Enabled = true;
            btnSearchCategory_All.Enabled = true;

            btnSearchCategory_All.Text = "POPULAR";

            btnKeyboardClear.Text = "CLEAR";
            btnKeyboardClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(46)))), ((int)(((byte)(95)))));
            

            if (GintLocation != 3)               // 미국이 아닌 경우
            {
                btnSearchOrKeyinItem.Text = "Search Key In Category";
                btnBackToCategory.Text = "Back to Category";
            }
            else
            {
                btnSearchOrKeyinItem.Text = "Search Key In Item";
                btnBackToCategory.Text = "Back";
            }
            
            g_SearchCategory = true;

            // All Category Image 출력
            lvSearchImage_Category.BringToFront();
            lvSearchImage_Item.SendToBack();
            
            GetimageListView("ALL", ImageType.Category);

            // 저울에서 스캔 되는거 방지.
            if (OPOSScanner.DeviceEnabled)
            {
                OPOSScanner.DataEvent -= ScannerDataEvent;
                OPOSScanner.DeviceEnabled = false;
            }

            // 음성 실행.
            GssSearchItem.SelectVoice(GstrVoice);
            GssSearchItem.SpeakAsync("Search the Item...Select the Item category.");
        }

        private void txtSearchCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                //gbSearchBox.Visible = false;                
                txtSearchCode.Clear();
                btnBack.Enabled = true;
                btnNext.Enabled = true;
                btnSearch.Enabled = true;
                btnSearch_Category.Enabled = true;                                
            }            
            // Seach Item 시작.
        }
        private void btnKeyboardClear_Click(object sender, EventArgs e)
        {
            if(g_SearchAdditional == true)
            {
                GetimageListView(txtSearchCode.Text, ImageType.Additional_Data);
            }
            else
            {
                txtSearchCode.Clear();
                txtSearchCode.Focus();

                //Timer Stop & Start
                BackToStartTimerFromItemScan.Stop();
                BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
                BackToStartTimerFromItemScan.Start();
            }            
        }

        private void btnSearchExit_Click(object sender, EventArgs e)
        {
            if (GssSearchItem.GetCurrentlySpokenPrompt() != null)                // 음성 중복 재생 방지.
            {
                GssSearchItem.SpeakAsyncCancel(GssSearchItem.GetCurrentlySpokenPrompt());
            }
            
            //ctrlOnScreen = ctrTemp;

            gbSearchBox.Visible = false;
            txtSearchCode.Clear();
            btnBack.Enabled = true;
            btnItemCorrect.Enabled = true;
            btnVoid.Enabled = true;
            btnItemDiscount.Enabled = true;
            btnReprint.Enabled = true;
            btnSuspend.Enabled = true;

            if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
            {
                btnNext.Enabled = false;
            }
            else
            {
                btnNext.Enabled = true;
            }

            btnSearch.Enabled = true;
            btnSearch_Category.Enabled = true;

            if (g_HelpModeOn == true)
                btnHelp.Enabled = false;
            else
                btnHelp.Enabled = true;
            
            iLSearchImage.Images.Clear();
            lvSearchImage_Item.Clear();
            lvSearchImage_Category.Clear();
            g_SearchCategory = false;
            g_SearchCategory_Clicked = false;
            g_SearchKeyInputPLU_Number = false;

            // Page Number 초기화
            g_SearchItemPageNum = 0;
            g_SearchItemMaxPageNum = 0;

            KeyInReady();

            if (OPOSScanner.DeviceEnabled == false)
            {
                _EnableScannerDevice();
            }

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnBagPlus_Click(object sender, EventArgs e)
        {
            int tempCount = 0;
            tempCount = int.Parse(lbBagCount.Text);
            if(tempCount == 99)
            {
                tempCount = 1;
            }
            else
            {
                tempCount += 1;
            }
            lbBagCount.Text = tempCount.ToString();

            //Timer Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnMinus_Click(object sender, EventArgs e)
        {
            int tempCount = 0;
            tempCount = int.Parse(lbBagCount.Text);
            tempCount -= 1;
            if(tempCount <= 0)   {   tempCount = 1;  }
            lbBagCount.Text = tempCount.ToString();

            //Timer Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }
        private void pnEMGExit_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void btnSelectCreditCard_Click(object sender, EventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            g_sMessage = string.Format("[{0}] Push Credit Card Button (ctrlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            BackToStartTimerFromItemScan.Stop();
            
            // 스캔된 상품이 없을때 진행 하지 않게..
            if (lbTotalValCS.Text != "0.00")
            {
                DisplayCreditCardPayment("CREDIT");
            }
            else
            {
                // Error Message
                DisplayErrorMessageBox("Select Payment", "No Scaned Items. \nPlease Scan your Item first.", 1, sMethod);
            }
        }

        private void btnSelectGiftCard_Click(object sender, EventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            // 스캔된 상품이 없을때 진행 하지 않게..
            if (lbTotalValCS.Text != "0.00")
            {
                DisplayCreditCardPayment("CREDIT");
            }
            else
            {
                // Error Message
                DisplayErrorMessageBox("Select Payment", "No Scaned Items. \nPlease Scan your Item first.", 1, sMethod);
            }
        }

        private void DisplayCreditCardPayment(string pPayType)
        {
            string strTotVal = lbTotalValCS.Text;
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            
            if (GPinPadReady == false)
            {
                DisplayErrorMessageBox("PIN Pad", "Pin Pad connection failed.", 1, sMethod);

                btnSearch.Enabled = false;
                btnSearch_Category.Enabled = false;
                btnNext.Enabled = false;
                btnHelp.Enabled = false;
                btnSelectCreditCard.Enabled = false;
                btnSelectPointCard.Enabled = false;
                btnSelectGiftCard.Enabled = false;
                btnEBT.Enabled = false;

                //Timer Start
                BackToStartTimerFromItemScan.Stop();
                BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
                BackToStartTimerFromItemScan.Start();

                KeyInReady();
                return;
            }

            if (strTotVal.Length > 6)
            {
                DisplayErrorMessageBox("PIN Pad", "Please input the pay amount less than $9,999.", 1, sMethod);

                btnSearch.Enabled = false;
                btnSearch_Category.Enabled = false;
                btnNext.Enabled = false;
                btnHelp.Enabled = false;
                btnSelectCreditCard.Enabled = false;
                btnSelectPointCard.Enabled = false;
                btnSelectGiftCard.Enabled = false;
                btnEBT.Enabled = false;

                //Timer Start
                BackToStartTimerFromItemScan.Stop();
                BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
                BackToStartTimerFromItemScan.Start();

                KeyInReady();
                return;
            }

            if (strTotVal == "0")
            {
                DisplayErrorMessageBox("PIN Pad", "Please input the correct pay amount.", 1, sMethod);

                btnSearch.Enabled = false;
                btnSearch_Category.Enabled = false;
                btnNext.Enabled = false;
                btnHelp.Enabled = false;
                btnSelectCreditCard.Enabled = false;
                btnSelectPointCard.Enabled = false;
                btnSelectGiftCard.Enabled = false;
                btnEBT.Enabled = false;
                KeyInReady();

                //Timer Start
                BackToStartTimerFromItemScan.Stop();
                BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
                BackToStartTimerFromItemScan.Start();

                return;
            }

            ctrTemp = ctrlOnScreen;

            g_sMessage = string.Format("[{0}] Pinpad PayType : {1})", sMethod, pPayType);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            //EBT
            if (pPayType == "CREDIT")
            {
                transitionSiglePage(gbProcessCreditCard, 258, 200);
                gbProcessCreditCard.BringToFront();
                gbProcessCreditCard.Visible = true;
                gbProcessCreditCard.Focus();
            }
            else if (pPayType == "EBT")
            {
                transitionSiglePage(gbProcessEBT, 258, 200);
                gbProcessEBT.BringToFront();
                gbProcessEBT.Visible = true;
                gbProcessEBT.Focus();
            }

            gbHelp.Visible = true;
            txtNumCS.Focus();

            btnSearch.Enabled = false;
            btnSearch_Category.Enabled = false;
            btnNext.Enabled = false;
            btnBack.Enabled = false;
            btnHelp.Enabled = false;

            
            btnSelectCreditCard.Enabled = false;
            btnSelectPointCard.Enabled = false;
            btnSelectGiftCard.Enabled = false;
            btnEBT.Enabled = false;
            btnHelp.Enabled = false;

            
            //cpCreditCard.IsRunning = true;
        }

        private void gbProcessCreditCard_LocationChanged(object sender, EventArgs e)
        {
            if (gbProcessCreditCard.Location.X == 258)
            {
                // Card Process
                ProcessCardPayment();
            }
            //else if (gbProcessCreditCard.Location.X == 100)
            //{
            //    // EBT Process
            //    ProcessEBTPayment();
            //}
        }
        private void gbProcessEBT_LocationChanged(object sender, EventArgs e)
        {
            if (gbProcessEBT.Location.X == 258)
            {
                // EBT Process
                ProcessEBTPayment();
            }
        }

        // EBT 
        private void btnEBT_Click(object sender, EventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            g_sMessage = string.Format("[{0}] Push EBT Card Button (ctrlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);


            // 스캔된 상품이 없을때 진행 하지 않게..
            if (lbTotalValCS.Text != "0.00")
            {
                DisplayCreditCardPayment("EBT");
            }
            else
            {
                // Error Message
                DisplayErrorMessageBox("Select Payment", "No Scaned Items. \nPlease Scan your Item first.", 1, sMethod);
            }
        }

        private void btnSelectPointCard_Click(object sender, EventArgs e)
        {
            int intHPUsablelimit = 0;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            g_sMessage = string.Format("[{0}] Push Select Point Card Button (ctrlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            // 스캔된 상품이 없을때 진행 하지 않게..
            if (lbTotalValCS.Text != "0.00")
            {
                // Point 카드를 스캔했을 경우 조건 추가 필요.
                if (GblMemberHPuse == true)
                {
                    if (GintLocation == 1) // 밴쿠버
                    {
                        intHPUsablelimit = 2500;
                    }
                    else if (GintLocation == 2) // 토론토
                    {
                        intHPUsablelimit = 5000;
                    }
                    else
                    {
                        intHPUsablelimit = 0;
                    }

                    if (c_poscominfo.mi_pointbalance < 0 || c_poscominfo.mi_pointbalance < intHPUsablelimit) // 현재 포인트가 없거나 Usable Limit보다 작은 경우 
                    {
                        if (GintLocation == 1) // 밴쿠버
                        {
                            DisplayErrorMessageBox("Point Card", "NEED MORE POINTS TO REDEEM. \n\n Minimum Need Point : " + string.Format("{0:###,##0}", intHPUsablelimit) + "  P ($ 5 Values)", 1, sMethod);
                        }
                        else
                        {
                            DisplayErrorMessageBox("Point Card", "NEED MORE POINTS TO REDEEM. \n\n Minimum Need Point : " + string.Format("{0:###,##0}", intHPUsablelimit) + "  P", 1, sMethod);
                        }
                        GPayFinish = false;
                        //KeyInReady();

                        gbHelp.Visible = true;

                        btnSearch.Enabled = false;
                        btnSearch_Category.Enabled = false;
                        btnNext.Enabled = false;

                        btnSelectCreditCard.Enabled = false;
                        btnSelectPointCard.Enabled = false;
                        btnSelectGiftCard.Enabled = false;
                        btnEBT.Enabled = false;
                        btnHelp.Enabled = false;
                        return;
                    }
                }
                else
                {
                    DisplayErrorMessageBox("Point Card", "PLEASE CHECK YOUR MEMBERSHIP CARD.", 1, sMethod);
                    //KeyInReady();

                    gbHelp.Visible = true;

                    btnSearch.Enabled = false;
                    btnSearch_Category.Enabled = false;
                    btnNext.Enabled = false;

                    btnSelectCreditCard.Enabled = false;
                    btnSelectPointCard.Enabled = false;
                    btnSelectGiftCard.Enabled = false;
                    btnEBT.Enabled = false;
                    btnHelp.Enabled = false;

                    return;
                }

                int iHPtoAmount = Convert.ToInt32(c_poscominfo.mi_pointbalance) / 500;
                lbProcessPointCard_NumBalance.Text = string.Format("{0:0,0}", c_poscominfo.mi_pointbalance);
                lbProcessPointCard_NumAvailable.Text = string.Format("{0:0}", iHPtoAmount);

                gbHelp.Visible = true;

                btnSearch.Enabled = false;
                btnSearch_Category.Enabled = false;
                btnNext.Enabled = false;

                btnSelectCreditCard.Enabled = false;
                btnSelectPointCard.Enabled = false;
                btnSelectGiftCard.Enabled = false;
                btnEBT.Enabled = false;
                btnHelp.Enabled = false;
                //KeyInReady();

                ctrTemp = ctrlOnScreen;
                btnPointUseYes.Enabled = true;
                btnPointUseNo.Enabled = true;
                transitionSiglePage(gbProcessPointCard, 240, 200);

                // 음성 
                GssPointUse.SelectVoice(GstrVoice);
                GssPointUse.SpeakAsync("Do you want to pay using your point?");
            }
            else
            {
                // Error Message
                DisplayErrorMessageBox("Select Payment", "No Scaned Items. \nPlease Scan your Item first.", 1, sMethod);
            }

            //// Point 카드를 스캔했을 경우 조건 추가 필요.
            //if (GblMemberHPuse == true)
            //{
            //    if (GintLocation == 1) // 밴쿠버
            //    {
            //        intHPUsablelimit = 2500;
            //    }
            //    else if (GintLocation == 2) // 토론토
            //    {
            //        intHPUsablelimit = 5000;
            //    }
            //    else
            //    {
            //        intHPUsablelimit = 0;
            //    }

            //    if (c_poscominfo.mi_pointbalance < 0 || c_poscominfo.mi_pointbalance < intHPUsablelimit) // 현재 포인트가 없거나 Usable Limit보다 작은 경우 
            //    {
            //        DisplayErrorMessageBox("Point Card", "NEED MORE POINTS TO REDEEM. \n\n Minimum Need Point : " + string.Format("{0:###,##0}", intHPUsablelimit) + "  P", 1, sMethod);
            //        GPayFinish = false;
            //        //KeyInReady();

            //        gbHelp.Visible = true;

            //        btnSearch.Enabled = false;
            //        btnNext.Enabled = false;

            //        btnSelectCreditCard.Enabled = false;
            //        btnSelectPointCard.Enabled = false;
            //        btnHelp.Enabled = false;
            //        return;
            //    }
            //}
            //else
            //{
            //    DisplayErrorMessageBox("Point Card", "PLEASE CHECK YOUR MEMBERSHIP CARD.", 1, sMethod);
            //    //KeyInReady();

            //    gbHelp.Visible = true;

            //    btnSearch.Enabled = false;
            //    btnNext.Enabled = false;

            //    btnSelectCreditCard.Enabled = false;
            //    btnSelectPointCard.Enabled = false;
            //    btnHelp.Enabled = false;

            //    return;
            //}

            //int iHPtoAmount = Convert.ToInt32(c_poscominfo.mi_pointbalance) / 500;
            //lbProcessPointCard_NumBalance.Text = string.Format("{0:0,0}",c_poscominfo.mi_pointbalance);
            //lbProcessPointCard_NumAvailable.Text = string.Format("{0:0}", iHPtoAmount);

            //gbHelp.Visible = true;

            //btnSearch.Enabled = false;
            //btnNext.Enabled = false;

            //btnSelectCreditCard.Enabled = false;
            //btnSelectPointCard.Enabled = false;
            //btnHelp.Enabled = false;
            ////KeyInReady();

            //ctrTemp = ctrlOnScreen;
            //btnPointUseYes.Enabled = true;
            //btnPointUseNo.Enabled = true;
            //transitionSiglePage(gbProcessPointCard, 240, 200);

            //// 음성 
            //GssPointUse.SelectVoice(GstrVoice);
            //GssPointUse.SpeakAsync("Do you want to pay using your point?");
        }

        //private void pnTempNext_Click(object sender, EventArgs e) // 나중에 지워질 예정. 넘어가기 위한 방법
        //{
        //    gbProcessCreditCard.Visible = false;

        //    transitionSiglePage(gbProcessCreditCard, 1023, 200);
        //    transitionDoublePage(pnReview, pn_ItemScan, 0, 1 * GROUP_BOX_LEFT, 300);

        //    tbStep3Name.ForeColor = System.Drawing.Color.Silver;
        //    tbStep4Name.ForeColor = System.Drawing.Color.DimGray;
        //    st_ProcessStatus.CurrentStep = 4;

        //    btnNext.Text = "NEXT";
        //    btnNext.Font = new System.Drawing.Font("Helvetica85-Heavy", 24F, FontStyle.Bold);

        //    lbBagCount.Text = "0";

        //    //cpCreditCard.IsRunning = false;
        //    gbHelp.Visible = false;
        //    btnSearch.Enabled = true;

        //    // 음성 실행.
        //    GssThankyou.SelectVoice(GstrVoice);
        //    GssThankyou.SpeakAsync("Thank you for using Self Check Out.");

        //    // 화면 전화 이후 Start 화면으로 자동으로 변경되기 위한 Timer. 
        //    ReviewToStartTimer.Interval = 5000;
        //    ReviewToStartTimer.Start();
        //}

        private void btnKeyboardV_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "V";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }
        private void btnKeyboard3_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "3";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }
        private void btnKeyboardQ_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "Q";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardW_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "W";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardE_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "E";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardR_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "R";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardT_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "T";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardY_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "Y";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardU_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "U";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardI_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "I";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardO_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "O";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardP_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "P";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardUnderBar_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "_";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardA_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "A";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardS_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "S";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardD_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "D";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardF_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "F";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardG_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "G";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardH_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "H";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardJ_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "J";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardK_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "K";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardL_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "L";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardAt_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "@";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardZ_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "Z";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardX_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "X";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardC_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "C";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardB_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "B";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }
        
        private void btnHelp_Click(object sender, EventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            g_sMessage = string.Format("[{0}] Push Help Button (ctrlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            btnHelpExit.Visible = true;
            ProcessEntering_HelpMode();
        }
        
        private void ProcessEntering_HelpMode()
        {
            gbHelp.Visible = true;

            ctrTemp = ctrlOnScreen;
            ctrTemp2 = ctrlOnScreen;

            if (ctrTemp != pn_Start && ctrTemp != pn_TempClosed)
            {
                transitionSiglePage(gbHelp, 318, 300);
                cpgbHelp.IsRunning = true;
                g_HelpModeReady = true;
            }
            else
            {
                transitionSiglePage(gbScanManagerCard, 308, 200);
                cpScanManagerCard.IsRunning = true;
                g_ManagerCardScanReady = true;
            }
                        
            txtNumCS.Enabled = true;
            txtNumCS.Clear();
            txtNumCS.Focus();
            
            BackToStartTimerFromItemScan.Stop();

            if (ctrlOnScreen == gbHelp && (ctrTemp == pnSelectPayment || ctrTemp == gbProcessPointCard))
            {
                btnSelectCreditCard.Enabled = false;
                btnSelectPointCard.Enabled = false;
                btnSelectGiftCard.Enabled = false;
                btnEBT.Enabled = false;
                btnHelp.Enabled = false;
                btnNext.Enabled = false;
                btnBack.Enabled = false;
            }
            else if (ctrlOnScreen == gbHelp && ctrTemp == pnAddBag)
            {
                btnMinus.Enabled = false;
                btnBagPlus.Enabled = false;
                btnAddBagToCart.Enabled = false;
                btnNoBag.Enabled = false;
                btnHelp.Enabled = false;
                btnNext.Enabled = false;
                btnBack.Enabled = false;
            }
            else if (ctrlOnScreen == gbHelp && (ctrTemp == pn_ItemScan || ctrTemp == pnItemScanSearchBtn))
            {
                btnSearch.Enabled = false;
                btnSearch_Category.Enabled = false;
                btnNext.Enabled = false;
                btnHelp.Enabled = false;
                btnBack.Enabled = false;
            }
            else if (ctrlOnScreen == gbHelp && ctrTemp == gbBackToStartExtension)
            {
                btnSelectCreditCard.Enabled = false;
                btnSelectPointCard.Enabled = false;
                btnSelectGiftCard.Enabled = false;
                btnEBT.Enabled = false;
                btnHelp.Enabled = false;
                btnNext.Enabled = false;

                btnMinus.Enabled = false;
                btnBagPlus.Enabled = false;
                btnAddBagToCart.Enabled = false;
                btnNoBag.Enabled = false;

                btnSearch.Enabled = false;
                btnSearch_Category.Enabled = false;
            }
            else if (ctrlOnScreen == gbHelp && ctrTemp == pn_Start)
            {
                btnStart.Enabled = false;
            }

                // Light ON
            ProcessQLightControl("a0");     // All Light On
            ProcessQLightControl("r2");     // All Light Flash
            ProcessQLightControl("o2");     // All Light Flash
            ProcessQLightControl("g2");     // All Light Flash

            ProcessQLightControl("rb1");     // Beep
            
            // 음성 실행.
            GssHelp.SelectVoice(GstrVoice);
            if (ctrTemp != pn_Start)
            {
                GssHelp.SpeakAsync("Please Wait.....Help is on the way.");
            }
            else
            {
                GssHelp.SpeakAsync("Scan your Manager Card.");
            }

            //if (GintLocation == 1 || GintLocation == 3)       // 벤쿠버 일때만
            //{
            //    if (c_poscominfo.si_scaletype != 0)
            //    {
            //        // Scale/Scanner Initialization 추가.
            //        //// 현재는 ScaleType 9 (OPOS 설정)의 경우만 구현
            //        if (EnableOPOSDevices() < 0)
            //            c_poscominfo.si_scaleuse = false;
            //        else
            //            c_poscominfo.si_scaleuse = true;
            //    }
            //}

            // 저울 Enable
            if (OPOSScanner.DeviceEnabled == false)
            {
                _EnableScannerDevice();
            }

            // Beep Sound
            // 코퀴틀람 일반 PC 캐셔대인 경우 Beep 음 스피커로 출력.
            if (c_poscominfo.ci_mklocation == 1 && c_poscominfo.ci_mkno == "61" && (c_poscominfo.si_counternum == "1" || c_poscominfo.si_counternum == "10" || c_poscominfo.si_counternum == "2" || c_poscominfo.si_counternum == "11"))
            {
                ProcessBeepSoundTimer.Interval = g_iProcessBeepSoundTimer;
                ProcessBeepSoundTimer.Start();

                g_bBreakBeepSound = true;

                bgw_ProcessBeepSound.RunWorkerAsync();
            }
            
        }

        private void btnManualETCKey_Click(object sender, EventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            g_sMessage = string.Format("[{0}] Push ManualETCKey Button.", sMethod);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            
            // Button Control
            btnBack.Enabled = false;
            btnItemCorrect.Enabled = false;
            btnVoid.Enabled = false;
            btnItemDiscount.Enabled = false;
            btnReprint.Enabled = false;
            btnSuspend.Enabled = false;
            btnNext.Enabled = false;
            ItemCSView.Enabled = false;

            btnSearch.Enabled = false;
            btnSearch_Category.Enabled = false;

            btnAddBagToCart.Enabled = false;
            btnNoBag.Enabled = false;
            btnBagPlus.Enabled = false;
            btnMinus.Enabled = false;

            btnSelectCreditCard.Enabled = false;
            btnSelectPointCard.Enabled = false;
            btnSelectGiftCard.Enabled = false;
            btnEBT.Enabled = false;
            btnManualETCKey.Enabled = false;

            // 결제 내역이 있는 경우
            if (lblPayTotal.Text != "0.00")
            {
                DisplayErrorMessageBox("Manual ETC", "Manual ETC KEY NOT ALLOWED\n WHEN PAYMENT EXIST", 1, sMethod);
                return;
            }

            g_ManagerCardScanReady = true;
            g_mMaterFunctionVal = ManualMasterFunction.ItemETCKeyIN;

            if (c_poscominfo.ci_mkno != "55" && c_poscominfo.ci_mkno != "62")                // 55 오로라, 62 다운타운 매장이 아닌 경우 Manager Key Scan 필요.
            {
                ctrTemp = ctrlOnScreen;
                transitionSiglePage(gbScanManagerCard, 308, 200);
                cpScanManagerCard.IsRunning = true;
            }
            else                                            // 55 오로라, 62 다운타운 매장인 경우 Manager Key Scan 필요 없음.
            {
                ProcessEntryCode(g_strManagerKey, 0);
            }

            KeyInReady();
        }

        private void ProcessManualETCKey()
        {
            // ETC KEY Panel 활성화
            pn_ManualETCKey.BringToFront();
            txtManualETCKey_Amount.Text = "";

            ctrTemp = ctrlOnScreen;
            transitionSiglePage(pn_ManualETCKey, 81, 200);

            g_mKeyinVale = ManualETCKeyIN_DEP.ETC;
            lbManualETCKey_SelectedDP.Text = "";
            txtManualETCKey_Amount.Clear();

            btnSearch.Enabled = false;
            btnSearch_Category.Enabled = false;
            btnNext.Enabled = false;
            btnHelp.Enabled = false;
            btnBack.Enabled = false;
            btnItemCorrect.Enabled = false;
            btnVoid.Enabled = false;
            btnReprint.Enabled = false;
            btnSuspend.Enabled = false;

            btnSelectCreditCard.Enabled = false;
            btnSelectPointCard.Enabled = false;
            btnSelectGiftCard.Enabled = false;
            btnEBT.Enabled = false;

            btnMinus.Enabled = false;
            btnBagPlus.Enabled = false;
            btnAddBagToCart.Enabled = false;
            btnNoBag.Enabled = false;

            btnManualETCKey.Enabled = false;

            swManulETCKey_TAX.Value = false;

            g_ManualKeyInItem = false;
            g_ManualKeyInItemPrice = 0;
        }

        private void btnManualETCKey_Grocery_Click(object sender, EventArgs e)
        {
            g_mKeyinVale = ManualETCKeyIN_DEP.GROCERY;

            // TAX 확인
            if(GetSelectedCategoryTax(g_mKeyinVale))
            {
                swManulETCKey_TAX.Value = true;
            }
            else
            {
                swManulETCKey_TAX.Value = false;
            }

            lbManualETCKey_SelectedDP.Text = str_mKeyinVale[(int)g_mKeyinVale];
            txtManualETCKey_Amount.Clear();
            txtManualETCKey_Amount.Focus();
        }

        private void btnManualETCKey_Produce_Click(object sender, EventArgs e)
        {
            g_mKeyinVale = ManualETCKeyIN_DEP.PRODUCE;
            // TAX 확인
            if (GetSelectedCategoryTax(g_mKeyinVale))
            {
                swManulETCKey_TAX.Value = true;
            }
            else
            {
                swManulETCKey_TAX.Value = false;
            }
            lbManualETCKey_SelectedDP.Text = str_mKeyinVale[(int)g_mKeyinVale];
            txtManualETCKey_Amount.Clear();
            txtManualETCKey_Amount.Focus();
        }

        private void btnManualETCKey_Fish_Click(object sender, EventArgs e)
        {
            g_mKeyinVale = ManualETCKeyIN_DEP.FISH;
            // TAX 확인
            if (GetSelectedCategoryTax(g_mKeyinVale))
            {
                swManulETCKey_TAX.Value = true;
            }
            else
            {
                swManulETCKey_TAX.Value = false;
            }
            lbManualETCKey_SelectedDP.Text = str_mKeyinVale[(int)g_mKeyinVale];
            txtManualETCKey_Amount.Clear();
            txtManualETCKey_Amount.Focus();
        }

        private void btnManualETCKey_Meat_Click(object sender, EventArgs e)
        {
            g_mKeyinVale = ManualETCKeyIN_DEP.MEAT;
            // TAX 확인
            if (GetSelectedCategoryTax(g_mKeyinVale))
            {
                swManulETCKey_TAX.Value = true;
            }
            else
            {
                swManulETCKey_TAX.Value = false;
            }
            lbManualETCKey_SelectedDP.Text = str_mKeyinVale[(int)g_mKeyinVale];
            txtManualETCKey_Amount.Clear();
            txtManualETCKey_Amount.Focus();
        }

        private void btnManualETCKey_Deli_Click(object sender, EventArgs e)
        {
            g_mKeyinVale = ManualETCKeyIN_DEP.DELI;
            // TAX 확인
            if (GetSelectedCategoryTax(g_mKeyinVale))
            {
                swManulETCKey_TAX.Value = true;
            }
            else
            {
                swManulETCKey_TAX.Value = false;
            }
            lbManualETCKey_SelectedDP.Text = str_mKeyinVale[(int)g_mKeyinVale];
            txtManualETCKey_Amount.Clear();
            txtManualETCKey_Amount.Focus();
        }

        private void btnManualETCKey_HW_Click(object sender, EventArgs e)
        {
            g_mKeyinVale = ManualETCKeyIN_DEP.HW;
            // TAX 확인
            if (GetSelectedCategoryTax(g_mKeyinVale))
            {
                swManulETCKey_TAX.Value = true;
            }
            else
            {
                swManulETCKey_TAX.Value = false;
            }
            lbManualETCKey_SelectedDP.Text = str_mKeyinVale[(int)g_mKeyinVale];
            txtManualETCKey_Amount.Clear();
            txtManualETCKey_Amount.Focus();
        }

        private bool GetSelectedCategoryTax(ManualETCKeyIN_DEP g_mKeyinVale)
        {
            string sQBuff = string.Empty;
            long lReturn = 0;

            bool bTaxValue = false;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string strSelectedProdID = string.Empty;

            // Item List에 입력
            switch (g_mKeyinVale)
            {
                case ManualETCKeyIN_DEP.GROCERY:
                    if(GintLocation == 1)
                    {
                        strSelectedProdID = "9968";
                    }
                    else if (GintLocation == 3)
                    {
                        if (c_poscominfo.ci_mkno == "52")               // 페더럴 웨이 매장인 경우
                            strSelectedProdID = "2995200000074";
                        else if (c_poscominfo.ci_mkno == "55")          // 오로라 매장인 경우
                            strSelectedProdID = "2995000000076";
                        else if(c_poscominfo.ci_mkno == "86")          // 웨스트 죠단 매장인 경우
                            strSelectedProdID = "9968";
                        else                                             // 린우드,레드몬드 인 경우
                            strSelectedProdID = "2995300000073";
                    }                   
                    break;
                case ManualETCKeyIN_DEP.PRODUCE:
                    if (GintLocation == 1)
                    {
                        strSelectedProdID = "9966";
                    }
                    else if (GintLocation == 3)
                    {
                        if (c_poscominfo.ci_mkno == "52")               // 페더럴 웨이 매장인 경우
                            strSelectedProdID = "2995200000067";
                        else if (c_poscominfo.ci_mkno == "55")          // 오로라 매장인 경우
                            strSelectedProdID = "2995000000069";
                        else if (c_poscominfo.ci_mkno == "86")          // 웨스트 죠단 매장인 경우
                            strSelectedProdID = "9966";
                        else                                            // 린우드,레드몬드 인 경우
                            strSelectedProdID = "2995300000066";
                    }
                    break;
                case ManualETCKeyIN_DEP.FISH:
                    if (GintLocation == 1)
                    {
                        strSelectedProdID = "9974";
                    }
                    else if (GintLocation == 3)
                    {
                        if (c_poscominfo.ci_mkno == "52")               // 페더럴 웨이 매장인 경우
                            strSelectedProdID = "2995200000050";
                        else if (c_poscominfo.ci_mkno == "55")          // 오로라 매장인 경우
                            strSelectedProdID = "2995000000052";
                        else if (c_poscominfo.ci_mkno == "86")          // 웨스트 죠단 매장인 경우
                            strSelectedProdID = "9974";
                        else                                            // 린우드,레드몬드 인 경우
                            strSelectedProdID = "2995300000059";
                    }
                    break;
                case ManualETCKeyIN_DEP.MEAT:
                    if (GintLocation == 1)
                    {
                        strSelectedProdID = "9973";
                    }
                    else if (GintLocation == 3)
                    {
                        if (c_poscominfo.ci_mkno == "52")               // 페더럴 웨이 매장인 경우
                            strSelectedProdID = "2995200000043";
                        else if (c_poscominfo.ci_mkno == "55")          // 오로라 매장인 경우
                            strSelectedProdID = "2995000000045";
                        else if (c_poscominfo.ci_mkno == "86")          // 웨스트 죠단 매장인 경우
                            strSelectedProdID = "9973";
                        else                                            // 린우드,레드몬드 인 경우
                            strSelectedProdID = "2995300000042";
                    }
                    break;
                case ManualETCKeyIN_DEP.DELI:
                    if (GintLocation == 1)
                    {
                        strSelectedProdID = "9967";
                    }
                    else if (GintLocation == 3)
                    {
                        if (c_poscominfo.ci_mkno == "52")               // 페더럴 웨이 매장인 경우
                            strSelectedProdID = "2995200000036";
                        else if (c_poscominfo.ci_mkno == "55")          // 오로라 매장인 경우
                            strSelectedProdID = "2995000000038";
                        else if (c_poscominfo.ci_mkno == "86")          // 웨스트 죠단 매장인 경우
                            strSelectedProdID = "9967";
                        else                                            // 린우드,레드몬드 인 경우
                            strSelectedProdID = "2995300000035";
                    }
                    break;
                case ManualETCKeyIN_DEP.HW:
                    if (GintLocation == 1)
                    {
                        strSelectedProdID = "9969";
                    }
                    else if (GintLocation == 3)
                    {
                        if (c_poscominfo.ci_mkno == "52")               // 페더럴 웨이 매장인 경우
                            strSelectedProdID = "2995200000081";
                        else if (c_poscominfo.ci_mkno == "55")          // 오로라 매장인 경우
                            strSelectedProdID = "2995000000083";
                        else if (c_poscominfo.ci_mkno == "86")          // 웨스트 죠단 매장인 경우
                            strSelectedProdID = "9969";
                        else                                            // 린우드,레드몬드 인 경우
                            strSelectedProdID = "2995300000080";
                    }
                    break;
                default:
                    strSelectedProdID = "2995300000073";
                    break;
            }

            // ProdID Category Tax.
            try
            {
                lReturn = c_localdb.DBConnection();

                if (lReturn < 0)
                {
                    g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    return false;
                }

                // 입력된 상품코드(UPC) 검색하여 존재 여부 확인.
                if (GintLocation == 1)                                          // 벤쿠버, 토론토인 경우
                {
                    sQBuff = "SELECT prodTax as pTax FROM hanamart.dbo.mfProd WHERE prodId = '" + strSelectedProdID + "'";
                }
                else if (GintLocation == 2)                          // 토론토 인 경우
                {
                }

                else if (GintLocation == 3)                          // 미국 인 경우
                {
                    sQBuff = "SELECT (CASE WHEN pTax >= '1' THEN pTax ELSE 0 END) as pTax " +
                            "FROM hanamart.dbo.mfProd left join hanamart.dbo.mfPtype ON prodType = pType " +
                            "WHERE prodId = '" + strSelectedProdID + "'";
                }

                lReturn = c_localdb.RsOpen(sQBuff);

                if (lReturn != 1)
                {
                    g_sMessage = string.Format("[{0}] Data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    c_localdb.DBClose();

                    return false;
                }
                else
                {
                    if (c_localdb.rs.RecordCount == 1)
                    {
                        if (GintLocation == 1)                          // 벤쿠버 인 경우
                        {                            
                            if (Convert.ToString(c_localdb.rs.Fields["pTax"].Value).All(char.IsDigit))      //숫자인지 확인 True 면 숫자, False 면 문자.
                            {
                                bTaxValue = false;
                            }
                            else
                            {
                                bTaxValue = true;
                            }
                        }
                        else if (GintLocation == 3)                          // 미국 인 경우
                        {
                            if (Convert.ToInt32(c_localdb.rs.Fields["pTax"].Value) >= 1)             // TAX Code가 1보다 크면
                            {
                                bTaxValue = true;
                            }
                            else
                            {
                                bTaxValue = false;
                            }
                        }

                            
                    }
                }                
            }
            catch (SqlException ex)
            {
                g_sMessage = string.Format("[{0}] Databasae error caused while operating (line: {1})\n[{0}] {2}", sMethod, ex.Source.ToString(), ex.Message.ToString());
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                return false;
            }
            catch (Exception ex)
            {
                g_sMessage = string.Format("[{0}] Error caused while operating (line: {1})\n[{0}] {2}", sMethod, ex.Source.ToString(), ex.Message.ToString());
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                return false;
            }
            finally
            {
                c_localdb.DBClose();
            }

            return bTaxValue;
        }


        private void btnManualETCKey_1_Click(object sender, EventArgs e)
        {
            txtManualETCKey_Amount.Text += "1";
        }

        private void btnManualETCKey_2_Click(object sender, EventArgs e)
        {
            txtManualETCKey_Amount.Text += "2";
        }

        private void btnManualETCKey_3_Click(object sender, EventArgs e)
        {
            txtManualETCKey_Amount.Text += "3";
        }

        private void btnManualETCKey_4_Click(object sender, EventArgs e)
        {
            txtManualETCKey_Amount.Text += "4";
        }

        private void btnManualETCKey_5_Click(object sender, EventArgs e)
        {
            txtManualETCKey_Amount.Text += "5";
        }

        private void btnManualETCKey_6_Click(object sender, EventArgs e)
        {
            txtManualETCKey_Amount.Text += "6";
        }

        private void btnManualETCKey_7_Click(object sender, EventArgs e)
        {
            txtManualETCKey_Amount.Text += "7";
        }

        private void btnManualETCKey_8_Click(object sender, EventArgs e)
        {
            txtManualETCKey_Amount.Text += "8";
        }

        private void btnManualETCKey_9_Click(object sender, EventArgs e)
        {
            txtManualETCKey_Amount.Text += "9";
        }

        private void btnManualETCKey_0_Click(object sender, EventArgs e)
        {
            txtManualETCKey_Amount.Text += "0";
        }

        private void btnManualETCKey_00_Click(object sender, EventArgs e)
        {
            txtManualETCKey_Amount.Text += "00";
        }

        private void btnManualETCKey_comma_Click(object sender, EventArgs e)
        {
            txtManualETCKey_Amount.Text += ".";
        }

        private void btnManualETCKey_BackSpace_Click(object sender, EventArgs e)
        {
            if (txtManualETCKey_Amount.Text.Length > 0)
            {
                txtManualETCKey_Amount.Text = txtManualETCKey_Amount.Text.Remove(txtManualETCKey_Amount.Text.Length - 1, 1);
            }

            txtManualETCKey_Amount.Focus();
            txtManualETCKey_Amount.Select(txtManualETCKey_Amount.Text.Length, 0);
        }

        private void btnManualETCKey_Clear_Click(object sender, EventArgs e)
        {
            txtManualETCKey_Amount.Clear();
            txtManualETCKey_Amount.Focus();
        }

        private void swManulETCKey_TAX_Click(object sender, EventArgs e)
        {

        }
    
        private void btnManualETCKey_Insert_Click(object sender, EventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string strSelectedProdID = string.Empty;
            // 선택한 내용 확인 하기.
            // Department 선택했는지
            if (g_mKeyinVale == ManualETCKeyIN_DEP.ETC)
            {
                // Error 출력
                pn_ManualETCKey.Enabled = false;
                DisplayErrorMessageBox("Manual ETC", "No Department Information. \n\n Please Select the Department.", 1, sMethod);
                
                return;
            }
            
            // 금액 $0 이상 입력 했는지.
            if (txtManualETCKey_Amount.Text == "" || Convert.ToDouble(txtManualETCKey_Amount.Text) <= 0 )
            {
                // Error 출력
                pn_ManualETCKey.Enabled = false;
                DisplayErrorMessageBox("Manual ETC", "No Amount Information. \n\n Please input the Amount.", 1, sMethod);
                return;
            }

            // Item List에 입력
            switch (g_mKeyinVale)
            {
                case ManualETCKeyIN_DEP.GROCERY:
                    if (GintLocation == 1)
                    {
                        strSelectedProdID = "9968";
                    }
                    else if (GintLocation == 3)
                    {
                        if (c_poscominfo.ci_mkno == "52")               // 페더럴 웨이 매장인 경우
                            strSelectedProdID = "2995200000074";
                        else if (c_poscominfo.ci_mkno == "55")          // 오로라 매장인 경우
                            strSelectedProdID = "2995000000076";
                        else if (c_poscominfo.ci_mkno == "86")          // 웨스트 죠단 매장인 경우
                            strSelectedProdID = "9968";
                        else
                            strSelectedProdID = "2995300000073";
                    }
                    break;
                case ManualETCKeyIN_DEP.PRODUCE:
                    if (GintLocation == 1)
                    {
                        strSelectedProdID = "9966";
                    }
                    else if (GintLocation == 3)
                    {
                        if (c_poscominfo.ci_mkno == "52")               // 페더럴 웨이 매장인 경우
                            strSelectedProdID = "2995200000067";
                        else if (c_poscominfo.ci_mkno == "55")          // 오로라 매장인 경우
                            strSelectedProdID = "2995000000069";
                        else if (c_poscominfo.ci_mkno == "86")          // 웨스트 죠단 매장인 경우
                            strSelectedProdID = "9966";
                        else
                            strSelectedProdID = "2995300000066";
                    }
                    break;
                case ManualETCKeyIN_DEP.FISH:
                    if (GintLocation == 1)
                    {
                        strSelectedProdID = "9974";
                    }
                    else if (GintLocation == 3)
                    {
                        if (c_poscominfo.ci_mkno == "52")               // 페더럴 웨이 매장인 경우
                            strSelectedProdID = "2995200000050";
                        else if (c_poscominfo.ci_mkno == "55")          // 오로라 매장인 경우
                            strSelectedProdID = "2995000000052";
                        else if (c_poscominfo.ci_mkno == "86")          // 웨스트 죠단 매장인 경우
                            strSelectedProdID = "9974";
                        else
                            strSelectedProdID = "2995300000059";
                    }
                    break;
                case ManualETCKeyIN_DEP.MEAT:
                    if (GintLocation == 1)
                    {
                        strSelectedProdID = "9973";
                    }
                    else if (GintLocation == 3)
                    {
                        if (c_poscominfo.ci_mkno == "52")               // 페더럴 웨이 매장인 경우
                            strSelectedProdID = "2995200000043";
                        else if (c_poscominfo.ci_mkno == "55")          // 오로라 매장인 경우
                            strSelectedProdID = "2995000000045";
                        else if (c_poscominfo.ci_mkno == "86")          // 웨스트 죠단 매장인 경우
                            strSelectedProdID = "9973";
                        else
                            strSelectedProdID = "2995300000042";
                    }
                    break;
                case ManualETCKeyIN_DEP.DELI:
                    if (GintLocation == 1)
                    {
                        strSelectedProdID = "9967";
                    }
                    else if (GintLocation == 3)
                    {
                        if (c_poscominfo.ci_mkno == "52")               // 페더럴 웨이 매장인 경우
                            strSelectedProdID = "2995200000036";
                        else if (c_poscominfo.ci_mkno == "55")          // 오로라 매장인 경우
                            strSelectedProdID = "2995000000038";
                        else if (c_poscominfo.ci_mkno == "86")          // 웨스트 죠단 매장인 경우
                            strSelectedProdID = "9967";
                        else
                            strSelectedProdID = "2995300000035";
                    }
                    break;
                case ManualETCKeyIN_DEP.HW:
                    if (GintLocation == 1)
                    {
                        strSelectedProdID = "9969";
                    }
                    else if (GintLocation == 3)
                    {
                        if (c_poscominfo.ci_mkno == "52")               // 페더럴 웨이 매장인 경우
                            strSelectedProdID = "2995200000081";
                        else if (c_poscominfo.ci_mkno == "55")          // 오로라 매장인 경우
                            strSelectedProdID = "2995000000083";
                        else if (c_poscominfo.ci_mkno == "86")          // 웨스트 죠단 매장인 경우
                            strSelectedProdID = "9969";
                        else
                            strSelectedProdID = "2995300000080";
                    }
                    break;
                default:
                    strSelectedProdID = "2995300000073";
                    break;
            }

            // Item List Insert
            g_ManualKeyInItem = true;
            g_ManualKeyInItemPrice = Convert.ToDouble(txtManualETCKey_Amount.Text) / 100;

            ProcessItemSale(strSelectedProdID, 1);

            transitionSiglePage(pn_ManualETCKey, 1024, 200);
            ctrlOnScreen = ctrTemp;

            txtManualETCKey_Amount.Text = "";

            btnSearch.Enabled = true;
            btnSearch_Category.Enabled = true;

            if (ItemCSView.Items.Count != 0)
            {
                btnNext.Enabled = true;
            }
            else
            {
                btnNext.Enabled = false;
            }

            btnHelp.Enabled = true;
            btnBack.Enabled = true;

            btnItemCorrect.Enabled = true;
            btnVoid.Enabled = true;
            btnItemDiscount.Enabled = true;
            btnReprint.Enabled = true;
            btnSuspend.Enabled = true;
            ItemCSView.Enabled = true;

            btnSelectCreditCard.Enabled = true;
            btnSelectPointCard.Enabled = true;
            btnSelectGiftCard.Enabled = true;
            btnEBT.Enabled = true;

            btnManualETCKey.Enabled = true;

            g_ManualKeyInItem = false;
            g_ManualKeyInItemPrice = 0;

            KeyInReady();
        }

        private void btnManualETCKey_Exit_Click(object sender, EventArgs e)
        {
            pn_ManualETCKey.SendToBack();
            transitionSiglePage(pn_ManualETCKey, 1024, 200);
            ctrlOnScreen = ctrTemp;
            txtManualETCKey_Amount.Text = "";

            btnSearch.Enabled = true;
            btnSearch_Category.Enabled = true;

            if (ItemCSView.Items.Count != 0)
            {
                btnNext.Enabled = true;
            }
            else
            {
                btnNext.Enabled = false;
            }
            
            btnHelp.Enabled = true;
            btnBack.Enabled = true;

            btnItemCorrect.Enabled = true;
            btnVoid.Enabled = true;
            btnItemDiscount.Enabled = true;
            btnReprint.Enabled = true;
            btnSuspend.Enabled = true;
            ItemCSView.Enabled = true;

            btnSelectCreditCard.Enabled = true;
            btnSelectPointCard.Enabled = true;
            btnSelectGiftCard.Enabled = true;
            btnEBT.Enabled = true;

            //if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                         // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
            btnEBT.Enabled = true;

            btnMinus.Enabled = true;
            btnBagPlus.Enabled = true;
            btnAddBagToCart.Enabled = true;
            btnNoBag.Enabled = true;

            btnManualETCKey.Enabled = true;

            g_ManualKeyInItem = false;
            g_ManualKeyInItemPrice = 0;

            KeyInReady();
        }

        private void btnAddBagToCart_Click(object sender, EventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            g_sMessage = string.Format("[{0}] Push AddBag to Cart Button (ctrlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            //Timer Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();

            if (GssHowManyUseBag.GetCurrentlySpokenPrompt() != null)                // 음성 중복 재생 방지.
            {
                GssHowManyUseBag.SpeakAsyncCancel(GssHowManyUseBag.GetCurrentlySpokenPrompt());
            }

            if (lbBagCount.Text != "0")
            {
                if(GintLocation == 1)                   // 벤쿠버 인 경우
                {
                    //ProcessItemSale("9988", Convert.ToDouble(lbBagCount.Text)); // 쇼핑백 코드 
                    if (c_poscominfo.ci_mkno == "61")
                    {
                        ProcessItemSale("004", Convert.ToDouble(lbBagCount.Text)); // 쇼핑백 코드 
                    }
                    else if(c_poscominfo.ci_mkno == "62")
                    {
                        ProcessItemSale("9986", Convert.ToDouble(lbBagCount.Text)); // 쇼핑백 코드 
                    }
                    else if (c_poscominfo.ci_mkno == "69")
                    {
                        ProcessItemSale("9006", Convert.ToDouble(lbBagCount.Text)); // 쇼핑백 코드
                    }
                }
                else if(GintLocation == 2)              // 토론토 인 경우
                {
                    string strCurDate = DateTime.Now.ToString("yyyy-MM-dd");
                    if ((Convert.ToDateTime("2023-05-31") <= Convert.ToDateTime(strCurDate)))         
                        ProcessItemSale("5556", Convert.ToDouble(lbBagCount.Text)); // 쇼핑백 코드 
                    else
                        ProcessItemSale("5555", Convert.ToDouble(lbBagCount.Text)); // 쇼핑백 코드 
                }
                else                                    // 미국 인 경우
                {
                    if (c_poscominfo.ci_mkno != "86")                              // 2024.06.04 West Jordan 매장 쇼핑백 무료 제공으로 임시로 하드코딩 함 by Robin
                    {
                        if (c_poscominfo.ci_mkno != "55")                              // 2025.06.11 Aurora 매장 쇼핑백 코드 변경. Aurora Bag FoodStamp 적용 해제.
                        {
                            ProcessItemSale("090", Convert.ToDouble(lbBagCount.Text)); // 쇼핑백 코드 
                        }
                        else
                        {
                            ProcessItemSale("093", Convert.ToDouble(lbBagCount.Text)); // 쇼핑백 코드 
                        }                            
                    }                        
                }
                
                // 음성 실행.
                //GssAddBag.SelectVoice(GstrVoice);
                //GssAddBag.SpeakAsync(lbBagCount.Text + "...Bags Added.");
                
                //Thread.Sleep(1500);
                
                ProcessAddtoBagToSelectPayment();
            }
        }
        private void btnNoBag_Click(object sender, EventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            g_sMessage = string.Format("[{0}] Push No bag Button (ctrlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            //Timer Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();

            if (GssHowManyUseBag.GetCurrentlySpokenPrompt() != null)                // 음성 중복 재생 방지.
            {
                GssHowManyUseBag.SpeakAsyncCancel(GssHowManyUseBag.GetCurrentlySpokenPrompt());
            }
            // 스캔된 상품이 없을때 진행 하지 않게..
            if(lbTotalValCS.Text != "0.00")
            {
                ProcessAddtoBagToSelectPayment();
            }
            else
            {
                // Error Message
                DisplayErrorMessageBox("Add Bag", "No Scaned Items. \nPlease Scan your Item first.", 1, sMethod);
            }
        }

        private void ProcessAddtoBagToSelectPayment()
        {
            pnSelectPayment.Visible = true;
            btnSelectCreditCard.Enabled = true;

            if (Convert.ToDouble(lblPayHmoney.Text) > 0) { btnSelectPointCard.Enabled = false; pbSelectpaymentPointCardApplied.Visible = true; }
            else                                         { btnSelectPointCard.Enabled = true; pbSelectpaymentPointCardApplied.Visible = false; }

            //if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                        // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
            //{
                if (Convert.ToDouble(lblPayEBT.Text) > 0) { btnEBT.Enabled = false; pbSelectpaymentEBTApplied.Visible = true; }
                else { btnEBT.Enabled = true; pbSelectpaymentEBTApplied.Visible = false; }
            //}

            btnSelectGiftCard.Enabled = true;
            
            transitionDoublePage(pnSelectPayment, pnAddBag, 584, 1 * GROUP_BOX_LEFT, 300);

            tbStep1Name.ForeColor = System.Drawing.Color.Silver;
            tbStep2Name.ForeColor = System.Drawing.Color.Silver;
            tbStep3Name.ForeColor = System.Drawing.Color.DimGray;
            st_ProcessStatus.CurrentStep = 3;
            //btnNext.Text = "PAYMENT";
            //btnNext.Font = new System.Drawing.Font("Helvetica85-Heavy", 15F, FontStyle.Bold);
            btnNext.Enabled = false;
            gbHelp.Visible = false;
            lbBagCount.Text = "1";

            if (g_HelpModeOn == true)
            {
                btnHelp.Enabled = false;
                txtNumCS.Enabled = true;
                KeyInReady();
            }
            else
            {
                btnHelp.Enabled = true;
                txtNumCS.Enabled = false;       // 아이템 스캔 방지.
            }

            //// Scale Scanner Disable
            //if (OPOSScanner.DeviceEnabled)
            //{
            //    OPOSScanner.DataEvent -= ScannerDataEvent;

            //    OPOSScanner.DeviceEnabled = false;
            //    OPOSScanner.ReleaseDevice();
            //    OPOSScanner.Close();
            //}

            // 음성 실행.
            GssSelectPayment.SelectVoice(GstrVoice);
            GssSelectPayment.SpeakAsync("Select your Payment Type.");
        }

        private void btnHelpExit_Click(object sender, EventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            g_sMessage = string.Format("[{0}] Push Help Exit Button (ctrlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            if (GssHelp.GetCurrentlySpokenPrompt() != null)                // 음성 중복 재생 방지.
            {
                GssHelp.SpeakAsyncCancel(GssHelp.GetCurrentlySpokenPrompt());
            }

            lbHelpMessage3.Text = "";
            transitionSiglePage(gbHelp, 1023, 200);

            // Beep Sound Stop.
            g_bBreakBeepSound = false;
            ProcessBeepSoundTimer.Stop();

            gbHelp.Visible = false;

            ctrlOnScreen = ctrTemp;

            btnSearch.Enabled = true;
            btnSearch_Category.Enabled = true;
            if (ctrlOnScreen == pnAddBag)
            {
                btnMinus.Enabled = true;
                btnBagPlus.Enabled = true;
                btnAddBagToCart.Enabled = true;
                btnNoBag.Enabled = true;
                btnNext.Enabled = true;
                txtNumCS.Enabled = false;
                btnBack.Enabled = true;

            }
            else if (ctrlOnScreen == pnSelectPayment || ctrlOnScreen == gbProcessPointCard)
            {
                btnSelectCreditCard.Enabled = true;

                if (Convert.ToDouble(lblPayHmoney.Text) > 0) { btnSelectPointCard.Enabled = false; pbSelectpaymentPointCardApplied.Visible = true; }
                else { btnSelectPointCard.Enabled = true; pbSelectpaymentPointCardApplied.Visible = false; }

                //if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                            // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
                //{
                    if (Convert.ToDouble(lblPayEBT.Text) > 0) { btnEBT.Enabled = false; pbSelectpaymentEBTApplied.Visible = true; }
                    else { btnEBT.Enabled = true; pbSelectpaymentEBTApplied.Visible = false; }
                //}

                btnSelectGiftCard.Enabled = true;
                txtNumCS.Enabled = false;
                btnBack.Enabled = true;

            }
            else if (ctrlOnScreen == pn_ItemScan || ctrlOnScreen == pnItemScanSearchBtn)
            {
                if (ItemCSView.Items.Count != 0)
                {
                    btnNext.Enabled = true;
                }
                txtNumCS.Focus();
                txtNumCS.Enabled = true;
                
            }
            
            //else if (ctrlOnScreen == gbBackToStartExtension)
            //{
            //    btnSelectCreditCard.Enabled = true;
            //    if (Convert.ToDouble(lblPayHmoney.Text) > 0) { btnSelectPointCard.Enabled = false; pbSelectpaymentPointCardApplied.Visible = true; }
            //    else { btnSelectPointCard.Enabled = true; pbSelectpaymentPointCardApplied.Visible = false; }

                //    if (Convert.ToDouble(lblPayEBT.Text) > 0) { btnEBT.Enabled = false; pbSelectpaymentEBTApplied.Visible = true; }
                //    else { btnEBT.Enabled = true; pbSelectpaymentEBTApplied.Visible = false; }

                //    btnSelectGiftCard.Enabled = true;

                //    if (ItemCSView.Items.Count != 0)
                //    {
                //        btnNext.Enabled = true;
                //    }

                //    btnMinus.Enabled = true;
                //    btnBagPlus.Enabled = true;
                //    btnAddBagToCart.Enabled = true;
                //    btnNoBag.Enabled = true;

                //    btnSearch.Enabled = true;
                //    btnSearch_Category.Enabled = true;

                //    txtNumCS.Enabled = false;
                //}
            else
            {
                txtNumCS.Enabled = false;
            }
            
            btnHelp.Enabled = true;
            g_HelpModeReady = false;
            cpgbHelp.IsRunning = false;
            
            // Light ON
            ProcessQLightControl("a0");     // all Light Off
            ProcessQLightControl("o1");     // Orange Light On

            ProcessQLightControl("b0");     // Beep Off

            // Time Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();

        }
        
        private void btnKeyboardN_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "N";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardM_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "M";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardColon_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += ",";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardColon2_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += ".";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardBS_Click(object sender, EventArgs e)
        {
            if (txtSearchCode.Text.Length > 0)
            {
                txtSearchCode.Text = txtSearchCode.Text.Remove(txtSearchCode.Text.Length - 1, 1);

                if (txtSearchCode.Text.Length == 0)
                {
                    //Pasge Left, Right Disable
                    btnSearchPageLeft.Enabled = false;
                    btnSearchPageRight.Enabled = false;
                }
            }
            
            txtSearchCode.Focus();
            txtSearchCode.Select(txtSearchCode.Text.Length, 0);

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboardSP_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += " ";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }
         private void prtOrderSlip(string pPrtNo)
        {
            // 오더슬립 프린트 로직
        }
        private void PrtReceipt_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            string strAddress = string.Empty;
            string strPhoneNumber = string.Empty;
            string strMarketName = string.Empty;

            string strInvoNum = string.Empty;
            string strcolDate = string.Empty;
            string strcolTime = string.Empty;
            string strcolCashierPassword = string.Empty;
            string strcolStation = string.Empty;
            string strcolHMoney = string.Empty;
            string strcolCash = string.Empty;
            string strcolPennyRounded = string.Empty;
            string strcolChange = string.Empty;
            string strcolDebit = string.Empty;
            string strcolVisa = string.Empty;
            string strcolMaster = string.Empty;
            string strcolAmex = string.Empty;
            string strcolDisc = string.Empty;
            string strcolDiners = string.Empty;
            string strcolCardETC = string.Empty;
            string strcolAli = string.Empty;
            string strcolWechat = string.Empty;
            string strcolUnion = string.Empty;
            string strcolEBT = string.Empty;
            string strcolGiftCard = string.Empty;
            string strcolGiftCertification = string.Empty;
            string strcolCheck = string.Empty;
            string strcolCredit = string.Empty;

            string strEpFname = string.Empty;

            string strtprod = string.Empty;
            string strtPtype = string.Empty;
            string strtType = string.Empty;
            string strtPunit = string.Empty;
            string strtIUprice = string.Empty;
            string strtOUPrice = string.Empty;
            string strtQty = string.Empty;
            string strtAmt = string.Empty;
            string strtWprice = string.Empty;
            string strtNative = string.Empty;
            string strtTax = string.Empty;
            string strtGst = string.Empty;
            string strtPst = string.Empty;
            string strtHst = string.Empty;
            string strtPromo = string.Empty;
            string strtspecial = string.Empty;
            string strprodName = string.Empty;
            string strprodKname = string.Empty;
            string strprodDeposit = string.Empty;
            string strprodCrf = string.Empty;
            string strprodPromoPrice = string.Empty;
            string strCustomerReceipt = string.Empty;

            string strwtTransType = string.Empty;
            string strwtStatus = string.Empty;
            string strwtPayorderid = string.Empty;
            string strwtPayType = string.Empty;
            string strwtMchid = string.Empty;

            string strcolCust = string.Empty;
            string strcolCStore = string.Empty;
            string strcolCustNo = string.Empty;

            string strcFirst = string.Empty;
            string strcName = string.Empty;
            string strcTelNo = string.Empty;

            string strcolHPPrev = string.Empty;
            string strcolHPEarn = string.Empty;
            string strcolHPBonus = string.Empty;
            string strcolHPUsed = string.Empty;
            string strcolHPBal = string.Empty;

            string strMKGstNo = string.Empty;

            string strtDate = string.Empty;
            string strtTime = string.Empty;

            string strSurveySDate = string.Empty;
            string strSurveyEDate = string.Empty;

            //int iItemQty = 0;
            //int iItemCount = 0;
            int itType = 0;
            int iFontHeightGap = 3;

            double dItemQty = 0;
            double dItemCount = 0;
            double dGSTTotal = 0;
            double dPSTTotal = 0;
            double dHSTTotal = 0;
            double dSubTotal = 0;
            double dTotalDue = 0;

            double dcolCash = 0;

            bool blTotalDC = false;
            string strDCtype = "";
            string strDCrate = "";
            string strDCTotal = "";

            string sQBuff = string.Empty;
            long lReturn = 0;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            g_sMessage = string.Format("[{0}] Start Receipt Print (ctrlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);


            e.Graphics.PageUnit = GraphicsUnit.Millimeter;
            StringBuilder sbHead = new StringBuilder();
            StringBuilder sbListInfo = new StringBuilder();
            StringFormat strformat = new StringFormat();
            int MaxWidth = 70;

            SizeF stringSize;
            RectangleF recHeadoutline;
            RectangleF recDetailInfo;

            Font font = new Font("Arial", 8, FontStyle.Regular);
            float fontHeight = font.GetHeight();
            int iStartX = 0;
            int iStartY = 0;
            int Offset = 0;
            Font Barcodefont;

            ////////////////////////////////////////////////////////Receipt Header START/////////////////////////////////////////////////////////////////////////
           
            //Market Address & information
            try
            {
                lReturn = c_localdb.DBConnection();
                if (lReturn < 0)
                {
                    g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    return;
                }

                sQBuff = "Select bh_lename, bh_street, bh_city, bh_province, bh_postal, bh_phone "
                       + "From HANAMART.dbo.tb_Branch "
                       + "Where bh_cd = " + c_poscominfo.ci_mkno;                // Branch Code 추가되는 코드 필요.

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
                        strAddress = Convert.ToString(c_localdb.rs.Fields["bh_street"].Value) + ", ";
                        strAddress += Convert.ToString(c_localdb.rs.Fields["bh_city"].Value) + ", ";
                        strAddress += Convert.ToString(c_localdb.rs.Fields["bh_province"].Value) + ", ";
                        strAddress += Convert.ToString(c_localdb.rs.Fields["bh_postal"].Value);
                        strPhoneNumber = Convert.ToString(c_localdb.rs.Fields["bh_phone"].Value);
                        strMarketName = Convert.ToString(c_localdb.rs.Fields["bh_lename"].Value);
                    }
                    else
                    {
                        g_sMessage = string.Format("[{0}] Market Information is not found (Branch Code: {1}).", sMethod, c_poscominfo.ci_mkno);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                        c_localdb.RsClose();
                        return;
                    }
                }
                c_localdb.RsClose();

                if (GstrReprint == "S")
                {
                    // Suspend Slip
                    iStartX = 0;
                    iStartY = 0;
                    Offset = 0;
                    // Suspend Start
                    Offset = Offset + iFontHeightGap;
                    e.Graphics.DrawString("This is no Receipt.", font, new SolidBrush(Color.Black), iStartX + 7, iStartY + Offset);
                    Offset = Offset + iFontHeightGap;
                    Offset = Offset + iFontHeightGap;
                    e.Graphics.DrawString("SUSPEND TRANSACTION", new Font("Arial", 12, FontStyle.Bold), new SolidBrush(Color.Black), iStartX + 7, iStartY + Offset);
                    Offset = Offset + iFontHeightGap;
                    Offset = Offset + iFontHeightGap;
                    e.Graphics.DrawString(strMarketName, font, new SolidBrush(Color.Black), iStartX + 3, iStartY + Offset);
                    Offset = Offset + iFontHeightGap;
                    e.Graphics.DrawString(strAddress, font, new SolidBrush(Color.Black), iStartX + 3, iStartY + Offset);
                    Offset = Offset + iFontHeightGap;
                    e.Graphics.DrawString(strPhoneNumber, font, new SolidBrush(Color.Black), iStartX + 3, iStartY + Offset);
                    Offset = Offset + iFontHeightGap;
                    Offset = Offset + iFontHeightGap;

                    // Search Suspend Trans Table
                    // Date 변환
                    string strCurDate = DateTime.Now.ToString("yyyy-MM-dd");
                    string strCurTime = DateTime.Now.ToString("h:mm:ss tt");

                    DateTime dDateTime = DateTime.ParseExact(strCurDate, "yyyy-MM-dd", null);
                    strCurDate = dDateTime.ToString("MMM dd, yyyy");

                    e.Graphics.DrawString(strCurDate + " " + strCurTime, font, new SolidBrush(Color.Black), iStartX, iStartY + Offset);
                    Offset = Offset + iFontHeightGap;
                    e.Graphics.DrawString("-----------------------------------------------------------------------", font, new SolidBrush(Color.Black), iStartX, iStartY + Offset);
                    Offset = Offset + iFontHeightGap;

                    e.Graphics.DrawString("Qty", font, new SolidBrush(Color.Black), iStartX, iStartY + Offset);
                    e.Graphics.DrawString("Description", font, new SolidBrush(Color.Black), iStartX + 7, iStartY + Offset);
                    e.Graphics.DrawString("Amount", font, new SolidBrush(Color.Black), 60 - 6, iStartY + Offset);
                    Offset = Offset + iFontHeightGap;
                    Offset = Offset + iFontHeightGap;

                }
                else if (GstrReprint == "N" || GstrReprint == "R")
                {
                    // Logo
                    Image igReceiptImg = global::HanaSales_SelfCheckOut.Properties.Resources.Rpt;
                    e.Graphics.DrawImage(igReceiptImg, 3, 0, 67, 16);

                    sbHead.AppendLine();
                    sbHead.AppendLine();
                    sbHead.AppendLine(strAddress);
                    if(GintLocation == 1 || GintLocation == 2)              // 벤쿠버, 토론토 인경우
                    {
                        sbHead.AppendLine("Tel. " + strPhoneNumber + " / www.hmart.ca");
                    }
                    else                                                    // 미국인 경우
                    {
                        sbHead.AppendLine("Tel. " + strPhoneNumber + " / www.hmartus.com");
                    }
                    
                    // BBQ 매장 전화번호 추가 필요.
                    //sb.AppendLine("Tel." + strPhoneNumber + "/ www.hmart.ca");
                    // Reprint Notice 추가 필요.
                    sbHead.AppendLine();

                    if (GstrReprint == "R")
                    {
                        //Reprint Message 
                        string strCurDate = DateTime.Now.ToString("yyyy-MM-dd");
                        string strCurTime = DateTime.Now.ToString("h:mm:ss tt");
                        sbHead.AppendLine("Receipt Reprinted " + strCurDate + " " + strCurTime);
                    }

                    // 출력 부분
                    strformat.LineAlignment = StringAlignment.Center;
                    strformat.Alignment = StringAlignment.Center;

                    stringSize = e.Graphics.MeasureString(sbHead.ToString(), new Font("Arial", 8, FontStyle.Regular), MaxWidth, strformat);
                    recHeadoutline = new RectangleF(0, Convert.ToInt16(igReceiptImg.Height * 72 / 300 / 2.83465), MaxWidth, stringSize.Height);
                    e.Graphics.DrawString(sbHead.ToString(), new Font("Arial", 8, FontStyle.Regular), Brushes.Black, recHeadoutline, strformat);


                    //Date & Cashier name
                    strInvoNum = GstrPrtInvno;
                    //strInvoNum = "6199210000076";  // 테스트 용 임시
                    sQBuff = "Select * From HANAMART.dbo.tb_Payment where colinvno = '" + strInvoNum + "'";        // Branch Code 추가되는 코드 필요.
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
                            strcolDate = Convert.ToString(c_localdb.rs.Fields["colDate"].Value);
                            strcolTime = Convert.ToString(c_localdb.rs.Fields["colTime"].Value);
                            strcolCashierPassword = Convert.ToString(c_localdb.rs.Fields["colPassword"].Value);
                            strcolStation = Convert.ToString(c_localdb.rs.Fields["colStation"].Value);
                            strcolHMoney = Convert.ToString(c_localdb.rs.Fields["colHmoney"].Value);
                            strcolCash = Convert.ToString(c_localdb.rs.Fields["colCash"].Value);
                            if(GintLocation == 1)                           // 벤쿠버인 경우
                            {
                                strcolPennyRounded = string.Format("{0:0.00}", double.Parse(Convert.ToString(c_localdb.rs.Fields["colPennyRounded"].Value))); //////Convert.ToString(c_localdb.rs.Fields["colPennyRounded"].Value); 
                                strcolAli = Convert.ToString(c_localdb.rs.Fields["colAli"].Value);
                                strcolWechat = Convert.ToString(c_localdb.rs.Fields["colWechat"].Value);
                                strcolUnion = Convert.ToString(c_localdb.rs.Fields["colUnion"].Value);
                            }
                            else if(GintLocation == 2)              // 토론토 인경우
                            {
                                strcolPennyRounded = "0.0000";
                                strcolAli = "0.0000";
                                strcolWechat = "0.0000";
                                strcolUnion = "0.0000";
                            }
                            else                                    // 미국 인경우
                            {
                                strcolPennyRounded = "0.0000";
                                strcolAli = "0.0000";
                                strcolWechat = "0.0000";
                                strcolUnion = "0.0000";
                            }

                            strcolChange = Convert.ToString(c_localdb.rs.Fields["colChange"].Value);
                            strcolDebit = Convert.ToString(c_localdb.rs.Fields["colDebit"].Value);
                            strcolVisa = Convert.ToString(c_localdb.rs.Fields["colVisa"].Value);
                            strcolMaster = Convert.ToString(c_localdb.rs.Fields["colMaster"].Value);
                            strcolAmex = Convert.ToString(c_localdb.rs.Fields["colAmex"].Value);
                            strcolDisc = Convert.ToString(c_localdb.rs.Fields["colDisc"].Value);
                            strcolDiners = Convert.ToString(c_localdb.rs.Fields["colDiners"].Value);
                            strcolCardETC = Convert.ToString(c_localdb.rs.Fields["colCardETC"].Value);
                            strcolEBT = Convert.ToString(c_localdb.rs.Fields["colEBT"].Value);
                            strcolGiftCard = Convert.ToString(c_localdb.rs.Fields["colGiftCard"].Value);
                            strcolGiftCertification = Convert.ToString(c_localdb.rs.Fields["colGiftCertification"].Value);
                            strcolCheck = Convert.ToString(c_localdb.rs.Fields["colCheck"].Value);
                            strcolCredit = Convert.ToString(c_localdb.rs.Fields["colCredit"].Value);
                            strcolCust = Convert.ToString(c_localdb.rs.Fields["colCust"].Value);
                            strcolCStore = Convert.ToString(c_localdb.rs.Fields["colcStore"].Value);
                            strcolCustNo = Convert.ToString(c_localdb.rs.Fields["colCustNo"].Value);
                            strcolHPPrev = Convert.ToString(c_localdb.rs.Fields["colHPPrev"].Value);
                            strcolHPEarn = Convert.ToString(c_localdb.rs.Fields["colHPEarn"].Value);
                            strcolHPBonus = Convert.ToString(c_localdb.rs.Fields["colHPBonus"].Value);
                            strcolHPUsed = Convert.ToString(c_localdb.rs.Fields["colHPUsed"].Value);
                            strcolHPBal = Convert.ToString(c_localdb.rs.Fields["colHPBal"].Value);
                        }
                        else
                        {
                            g_sMessage = string.Format("[{0}] Payment Information is not found (Invoice Number : {1}).", sMethod, strInvoNum);
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                            // Error Message
                            DisplayErrorMessageBox("Receipt Print", "Wrong Payment Information. \n(Invoice Number: " + strInvoNum + ")", 1, sMethod);
                            txtReceiptReprintBarcode.Clear();
                            c_localdb.RsClose();
                            return;
                        }
                    }
                    c_localdb.RsClose();

                    //// Cashier Name
                    //sQBuff = "Select ep_Fname From HANAMART.dbo.tb_Employee where ep_No = '" + strcolCashierPassword + "'";
                    //lReturn = c_localdb.RsOpen(sQBuff);
                    //if (lReturn != 1)
                    //{
                    //    g_sMessage = string.Format("[{0}] Data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    //    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                    //    c_localdb.DBClose();
                    //    return;
                    //}
                    //else
                    //{
                    //    if (c_localdb.rs.RecordCount == 1) { strEpFname = Convert.ToString(c_localdb.rs.Fields["ep_Fname"].Value); }
                    //    else
                    //    {
                    //        g_sMessage = string.Format("[{0}] Employee Information is not found (Cashier Password : {1}).", sMethod, strcolCashierPassword);
                    //        c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                    //        c_localdb.RsClose();
                    //        return;
                    //    }
                    //}
                    //c_localdb.RsClose();

                    strEpFname = " ";

                    // Date 변환
                    strcolDate = strcolDate.Substring(0, 10);
                    DateTime dDateTime = DateTime.ParseExact(strcolDate, "yyyy-MM-dd", null);

                    strcolDate = dDateTime.ToString("MMM dd, yyyy");

                    sbListInfo.AppendLine();
                    sbListInfo.AppendLine(String.Format("{0, -10} {1, 1}{2, 34}", strcolDate, strcolTime, strEpFname + "(" + strcolStation + ")"));
                    sbListInfo.AppendLine("-----------------------------------------------------------------------");

                    // Qty Description Amount
                    if(GintLocation != 3)                   // 벤쿠버, 토론토 인경우
                    {
                        sbListInfo.AppendLine(String.Format("{0, -3}{1, 15}{2, 46}", "Qty", "Description", "Amount"));
                        sbListInfo.AppendLine();
                    }
                    else                                    // 미국인 경우
                    {
                        sbListInfo.AppendLine(String.Format("{0, -3}{1, 15}{2, 35}{3, 10}", "Qty", "Description", "U/Price", "Amount"));
                        sbListInfo.AppendLine();
                    }
                    
                    // 출력 부분
                    strformat.LineAlignment = StringAlignment.Near;
                    strformat.Alignment = StringAlignment.Near;

                    stringSize = e.Graphics.MeasureString(sbListInfo.ToString(), new Font("Arial", 8, FontStyle.Regular), MaxWidth, strformat);
                    recDetailInfo = new RectangleF(0, recHeadoutline.Y + recHeadoutline.Size.Height, MaxWidth, stringSize.Height);

                    e.Graphics.DrawString(sbListInfo.ToString(), new Font("Arial", 8, FontStyle.Regular), Brushes.Black, recDetailInfo, strformat);

                    iStartX = 0;
                    iStartY = (int)(recHeadoutline.Y + recHeadoutline.Size.Height + stringSize.Height);
                    Offset = 0;
                }

                ////////////////////////////////////////////////////////Receipt Header END/////////////////////////////////////////////////////
                //////////////////////////////////////////////////////Receipt Detail START/////////////////////////////////////////////////////////////////////////
                //Offset X, Y
                //Detail Order List

                strInvoNum = GstrPrtInvno;

                if (GstrReprint == "S")
                {
                    if (GintLocation == 1)                          // 벤쿠버인 경우
                    {
                        sQBuff = "Select tprod, prodName, prodKname, prodDeposit, prodCrf, tPtype, tType, tPunit, tIUprice, " +
                        "tOUPrice, tQty, tAmt, tWprice, tNative, tTax, tGst, tPst, tHst, (CASE WHEN tPromo = '' THEN '0' ELSE tPromo END) AS tPromo, tspecial, promoPrice "
                        + "From HANAMART.dbo.tb_SuspendTrans left join HANAMART.dbo.mfProd ON tprod = prodId "
                        + "where tInvNo = '" + strInvoNum + "' order by tID ";
                    }
                    else if(GintLocation == 2)                  // 토론토 인경우
                    {
                        sQBuff = "Select tprod, prodName, prodKname, prodDeposit, tPtype, tType, tPunit, tIUprice, " +
                        "tOUPrice, tQty, tAmt, tWprice, tNative, tTax, tGst, tPst, tHst, (CASE WHEN tPromo = '' THEN '0' ELSE tPromo END) AS tPromo, tspecial, promoPrice "
                        + "From HANAMART.dbo.tb_SuspendTrans left join HANAMART.dbo.mfProd ON tprod = prodId "
                        + "where tInvNo = '" + strInvoNum + "' order by tID ";
                    }
                    else                                        // 미국 인 경우
                    {
                        sQBuff = "Select tprod, prodName, prodKname, prodDeposit, tPtype, tType, tPunit, tIUprice, " +
                        "tOUPrice, tQty, tAmt, tWprice, tNative, tTax, tGst, tPst, (CASE WHEN tPromo = '' THEN '0' ELSE tPromo END) AS tPromo, tspecial, promoPrice "
                        + "From HANAMART.dbo.tb_SuspendTrans left join HANAMART.dbo.mfProd ON tprod = prodId "
                        + "where tInvNo = '" + strInvoNum + "' order by tID ";
                    }
                }
                else if (GstrReprint == "N" || GstrReprint == "R")
                {
                    if (GintLocation == 1)                          // 벤쿠버인 경우
                    {                   
                        sQBuff = "Select tprod, prodName, prodKname, prodDeposit, prodCrf, tPtype, tType, tPunit, tIUprice, " +
                            "tOUPrice, tQty, tAmt, tWprice, tNative, tTax, tGst, tPst, tHst, (CASE WHEN tPromo = '' THEN '0' ELSE tPromo END) AS tPromo, tspecial, promoPrice "
                          + "From HANAMART.dbo.tb_solditem left join HANAMART.dbo.mfProd ON tprod = prodId "
                          + "where tInvNo = '" + strInvoNum + "' order by tID ";
                    }
                    else if (GintLocation == 2)                          // 토론토 인 경우
                    {
                        sQBuff = "Select tprod, prodName, prodKname, prodDeposit, tPtype, tType, tPunit, tIUprice, " +
                            "tOUPrice, tQty, tAmt, tWprice, tNative, tTax, tGst, tPst, tHst, (CASE WHEN tPromo = '' THEN '0' ELSE tPromo END) AS tPromo, tspecial, promoPrice "
                          + "From HANAMART.dbo.tb_solditem left join HANAMART.dbo.mfProd ON tprod = prodId "
                          + "where tInvNo = '" + strInvoNum + "' order by tID ";
                    }
                    else                                            // 미국 인 경우
                    {
                        sQBuff = "Select tprod, prodName, prodKname, prodDeposit, tPtype, tType, tPunit, tIUprice, " +
                        "tOUPrice, tQty, tAmt, tWprice, tNative, tTax, tGst, tPst, (CASE WHEN tPromo = '' THEN '0' ELSE tPromo END) AS tPromo, tspecial, promoPrice "
                      + "From HANAMART.dbo.tb_solditem left join HANAMART.dbo.mfProd ON tprod = prodId "
                      + "where tInvNo = '" + strInvoNum + "' order by tID ";
                    }
                }
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
                    while (c_localdb.rs.EOF != true)
                    {
                        if (c_localdb.rs.RecordCount != 0)
                        {
                            strtprod = Convert.ToString(c_localdb.rs.Fields["tprod"].Value);
                            strtPtype = Convert.ToString(c_localdb.rs.Fields["tPtype"].Value);
                            strtType = Convert.ToString(c_localdb.rs.Fields["tType"].Value);
                            strtPunit = Convert.ToString(c_localdb.rs.Fields["tPunit"].Value);
                            strtIUprice = Convert.ToString(c_localdb.rs.Fields["tIUprice"].Value);
                            strtOUPrice = string.Format("{0:0.00}", double.Parse(Convert.ToString(c_localdb.rs.Fields["tOUPrice"].Value))); 
                            strtQty = Convert.ToString(c_localdb.rs.Fields["tQty"].Value);
                            strtAmt = string.Format("{0:0.00}", double.Parse(Convert.ToString(c_localdb.rs.Fields["tAmt"].Value)));  /////////////////////// strtAmt = string.Format("{0:0.00}", double.Parse(rs["tAmt"].ToString()));
                            strtWprice = Convert.ToString(c_localdb.rs.Fields["tWprice"].Value);
                            strtNative = Convert.ToString(c_localdb.rs.Fields["tNative"].Value);
                            strtTax = Convert.ToString(c_localdb.rs.Fields["tTax"].Value);
                            strtGst = Convert.ToString(c_localdb.rs.Fields["tGst"].Value);
                            strtPst = Convert.ToString(c_localdb.rs.Fields["tPst"].Value);
                            if(GintLocation != 3)               // 벤쿠버, 토론토 인 경우
                            {
                                strtHst = Convert.ToString(c_localdb.rs.Fields["tHst"].Value);
                            }
                            else
                            {
                                strtHst = "0.00";
                            }
                            strtPromo = Convert.ToString(c_localdb.rs.Fields["tPromo"].Value);
                            strtspecial = Convert.ToString(c_localdb.rs.Fields["tspecial"].Value);
                            strprodName = Convert.ToString(c_localdb.rs.Fields["prodName"].Value);
                            strprodKname = Convert.ToString(c_localdb.rs.Fields["prodKname"].Value);
                            strprodDeposit = Convert.ToString(c_localdb.rs.Fields["prodDeposit"].Value);
                            if(GintLocation == 1)               // 벤쿠버인 경우
                            {
                                strprodCrf = Convert.ToString(c_localdb.rs.Fields["prodCrf"].Value);
                            }
                            dItemQty = double.Parse(strtQty);

                            if (strtType == "") { itType = 0; }
                            else { itType = Int32.Parse(strtType); }

                            //Bottle Deposit 상품
                            if (strprodDeposit != "" && strtType == "21")
                            {
                                strprodName = "Bottle Deposit-" + strprodName;
                                strprodKname = "공병값";
                            }

                            //CRF
                            if (strprodCrf != "" && strtType == "20")
                            {
                                strprodName = "CRF-" + strprodName;
                                strprodKname = "용기재활용수수료";
                            }

                            //Eco Fee
                            if (strtType == "22")
                            {
                                strprodName = "E.H.F - " + strprodName;
                                strprodKname = "환경부담금 ";
                            }

                            // Item Discount 표시
                            if (strtType == "48")
                            {
                                strprodName = "D.C-" + strprodName;
                                strprodKname = "할인-" + strprodKname;
                            }

                            // Mix & Match Record 표시
                            if (strtType == "47" && strtprod == "2995200000033")
                            {
                                strprodName = "Mix & Match Promo";
                                strprodKname = "믹스 앤 매치 프로모션";
                            }

                            // Reusable Bag Exemption 표시
                            if(GintLocation == 3)                   // 미국 인 경우
                            {
                                if (strtType == "49" && strtprod == "2995000000009")
                                {
                                    strprodName = "REUSABLE BAG EXEMPTION";
                                    strprodKname = "재활용봉투 면제";
                                }
                            }
                            
                            //Description Limit
                            if (strprodName.Length >= 20)
                            {
                                strprodName = strprodName.Substring(0, 20);
                            }

                            if (strtType == "41" || strtType == "42" || strtType == "43" || strtType == "44" || strtType == "45")
                            {
                                blTotalDC = true;
                                strDCtype = strtType;
                                strDCrate = strtNative;
                                strDCTotal = c_poscomlibs.getDoubleFormat(Convert.ToDouble(strtAmt) + Convert.ToDouble(strtGst) + Convert.ToDouble(strtPst) + Convert.ToDouble(strtHst));
                            }

                            //Item Detail
                            if (strtType != "41" && strtType != "42" && strtType != "43" && strtType != "44" && strtType != "45")
                            {
                                string tmpTax = string.Empty;
                                if (strtTax == "" || strtTax == "0") { tmpTax = ""; }
                                else                                 { tmpTax = "T";}
                                                                    
                                e.Graphics.DrawString(strtQty, font, new SolidBrush(Color.Black), iStartX, iStartY + Offset);           // QTY 수량, 무게 값
                                
                                if (GintLocation != 3)                       // 벤쿠버 토론토 인경우
                                {
                                    // 상품 명 가격 리스트 찍기
                                    if (Convert.ToInt16(strtPromo) >= 1)                           // Promotion 제품인 경우
                                    {
                                        strprodPromoPrice = Convert.ToString(c_localdb.rs.Fields["promoPrice"].Value);

                                        if (strtPunit == "LB" || strtPunit == "KG")
                                        {
                                            strprodName = strprodName + " " + "@$" + strprodPromoPrice + "/" + strtPunit;
                                        }
                                        else
                                        {
                                            if (strtType != "21" && strtType != "20")                    // Bottle Deposit & CRF 값이 아닌 경우.
                                            {
                                                strprodName = strprodName + " " + "@$" + strprodPromoPrice;
                                            }
                                            else
                                            {
                                                strprodName = strprodName + " " + "@$" + strtOUPrice;
                                            }
                                        }
                                        e.Graphics.DrawString(strprodName, font, new SolidBrush(Color.Black), iStartX + 7, iStartY + Offset);

                                        e.Graphics.DrawString(strtAmt, font, new SolidBrush(Color.Black), 62 - strtAmt.Length, iStartY + Offset);
                                        e.Graphics.DrawString(tmpTax, font, new SolidBrush(Color.Black), 64, iStartY + Offset);

                                        Offset = Offset + iFontHeightGap;

                                        // 한글 명
                                        if (strprodKname != "")
                                        {
                                            e.Graphics.DrawString(strprodKname, font, new SolidBrush(Color.Black), iStartX + 7, iStartY + Offset);
                                            Offset = Offset + iFontHeightGap;
                                        }

                                        // Promotion 표시
                                        if(strtType != "21" && strtType != "20")                    // Bottle Deposit & CRF 값이 아닌 경우.
                                        {
                                            e.Graphics.DrawString("# Promotion Item - Reg $" + strtIUprice + "/" + strtPunit, new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 7, iStartY + Offset);
                                            Offset = Offset + iFontHeightGap;
                                        }
                                    }
                                    else                                                    // Promotion 제품 아닌 경우
                                    {
                                        strprodName = strprodName + " " + "@$" + strtOUPrice;
                                        e.Graphics.DrawString(strprodName, font, new SolidBrush(Color.Black), iStartX + 7, iStartY + Offset);
                                        e.Graphics.DrawString(strtAmt, font, new SolidBrush(Color.Black), 62 - strtAmt.Length, iStartY + Offset);
                                        e.Graphics.DrawString(tmpTax, font, new SolidBrush(Color.Black), 64, iStartY + Offset);

                                        Offset = Offset + iFontHeightGap;

                                        // 한글 명
                                        if (strprodKname != "")
                                        {
                                            e.Graphics.DrawString(strprodKname, font, new SolidBrush(Color.Black), iStartX + 7, iStartY + Offset);
                                            Offset = Offset + iFontHeightGap;
                                        }
                                    }
                                }
                                else if(GintLocation == 3)      // 미국 인 경우
                                {
                                    e.Graphics.DrawString(strprodName, font, new SolidBrush(Color.Black), iStartX + 7, iStartY + Offset);
                                    e.Graphics.DrawString(strtOUPrice, font, new SolidBrush(Color.Black), 52 - strtOUPrice.Length, iStartY + Offset);

                                    e.Graphics.DrawString(strtAmt, font, new SolidBrush(Color.Black), 62 - strtAmt.Length, iStartY + Offset);
                                    e.Graphics.DrawString(tmpTax, font, new SolidBrush(Color.Black), 64, iStartY + Offset);

                                    Offset = Offset + iFontHeightGap;

                                    if (strprodKname != "")
                                    {
                                        e.Graphics.DrawString(strprodKname, font, new SolidBrush(Color.Black), iStartX + 7, iStartY + Offset);
                                        Offset = Offset + iFontHeightGap;
                                    }
                                }
                            }

                            //Item Count, Total, GST, PST, HST Sum
                            if (strtType == "49" || strtType == "31" || strtType == "32" || itType < 20)
                            {
                                if (strtPunit == "LB" || strtPunit == "KG") { dItemCount = dItemCount + 1; }
                                else { dItemCount = dItemCount + dItemQty; }
                            }

                            dGSTTotal = dGSTTotal + Convert.ToDouble(strtGst);
                            dPSTTotal = dPSTTotal + Convert.ToDouble(strtPst);
                            if(GintLocation != 3)                   // 벤쿠버, 토론토 인 경우
                            {
                                dHSTTotal = dHSTTotal + Convert.ToDouble(strtHst);
                            }
                            else
                            {
                                dHSTTotal = 0.00;
                            }
                            
                            dSubTotal = dSubTotal + Convert.ToDouble(strtAmt);

                            c_localdb.rs.MoveNext();
                        }
                        else
                        {
                            g_sMessage = string.Format("[{0}] Sold Items Information is not found (Invoice Number : {1}).", sMethod, strInvoNum);
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                            c_localdb.RsClose();
                            return;
                        }
                    }
                }
                c_localdb.RsClose();

                //Total info, Discount, Sub Total, GST / PST / HST, Total Due, Payment(Debit, Visa, Master..), Change Due

                Offset = Offset + iFontHeightGap;
                //Total Item count
                    e.Graphics.DrawString("Total " + string.Format("{0:0}", double.Parse(dItemCount.ToString())) + " Items", font, new SolidBrush(Color.Black), iStartX + 7, iStartY + Offset);
                Offset = Offset + iFontHeightGap;
                Offset = Offset + iFontHeightGap;

                //Total DC Amount
                if (blTotalDC == true)
                {
                    string strDCname = "";
                    if (strDCtype == "41")
                    {
                        strDCname = "Total Discount " + strDCrate + "%";
                    }
                    else if (strDCtype == "42")
                    {
                        strDCname = "VIP Discount " + strDCrate + "%";
                    }
                    else if (strDCtype == "43")
                    {
                        strDCname = "Employee Discount " + strDCrate + "%";
                    }
                    else if (strDCtype == "44")
                    {
                        ///strDCname = "FoodStamp Tax Exemption " + strDCrate + "%";
                        strDCname = "FoodStamp Tax Exemption";
                    }
                    else if (strDCtype == "45")
                    {
                        strDCname = "Internal Discount " + strDCrate + "%";
                    }

                    e.Graphics.DrawString(strDCname, font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                    e.Graphics.DrawString(strDCTotal, font, new SolidBrush(Color.Black), 62 - strDCTotal.Length, iStartY + Offset);
                    Offset = Offset + iFontHeightGap;
                    Offset = Offset + iFontHeightGap;
                }

                //Sub Total
                e.Graphics.DrawString("Sub Total : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(dSubTotal.ToString())), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(dSubTotal.ToString())).Length, iStartY + Offset);
                Offset = Offset + iFontHeightGap;

                // USA Tax 표시
                string strTax1Name = "";
                string strTax2Name = "";
                if (GintLocation == 3)                  // 미국 인 경우
                {
                    strTax1Name = "Tax1";
                    strTax2Name = "Tax2";
                }
                else                                    // 캐나다, 토론토 인 경우.
                {
                    strTax1Name = "GST";
                    strTax2Name = "PST";
                }

                //Gst
                if (dGSTTotal != 0)
                {
                    e.Graphics.DrawString(strTax1Name + " : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                    e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(dGSTTotal.ToString())), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(dGSTTotal.ToString())).Length, iStartY + Offset);
                    Offset = Offset + iFontHeightGap;
                }
                //Pst
                if (dPSTTotal != 0)
                {
                    e.Graphics.DrawString(strTax2Name + " : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                    e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(dPSTTotal.ToString())), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(dPSTTotal.ToString())).Length, iStartY + Offset);
                    Offset = Offset + iFontHeightGap;
                }
                //Hst
                if (dHSTTotal != 0)
                {
                    e.Graphics.DrawString("HST : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                    e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(dHSTTotal.ToString())), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(dHSTTotal.ToString())).Length, iStartY + Offset);
                    Offset = Offset + iFontHeightGap;
                }
                //Total Due
                dTotalDue = dSubTotal + dGSTTotal + dPSTTotal + dHSTTotal;
                e.Graphics.DrawString("Total Due : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(dTotalDue.ToString())), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(dTotalDue.ToString())).Length, iStartY + Offset);
                Offset = Offset + iFontHeightGap;


                if (GstrReprint == "N" || GstrReprint == "R")
                {
                    //Penny Rounded
                    if (strcolPennyRounded != "0.0000")
                    {
                        e.Graphics.DrawString("Penney Rounded : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                        e.Graphics.DrawString(string.Format("{0:0.00}", strcolPennyRounded), font, new SolidBrush(Color.Black), 62 - strcolPennyRounded.Length, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                    }
                    Offset = Offset + iFontHeightGap;
                
                    //Payment Summary Result, 
                    if (strcolHMoney != "" && strcolHMoney != "0.0000")
                    {
                        e.Graphics.DrawString("H-Money : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                        e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(strcolHMoney)), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(strcolHMoney)).Length, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                    }
                    dcolCash = Convert.ToDouble(strcolCash) + Convert.ToDouble(strcolChange);
                    if (dcolCash != 0)
                    {
                        e.Graphics.DrawString("Cash : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                        e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(dcolCash.ToString())), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(dcolCash.ToString())).Length, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                    }
                    if (strcolDebit != "" && strcolDebit != "0.0000")
                    {
                        e.Graphics.DrawString("Debit : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                        e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(strcolDebit)), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(strcolDebit)).Length, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                    }
                    if (strcolVisa != "" && strcolVisa != "0.0000")
                    {
                        e.Graphics.DrawString("Visa : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                        e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(strcolVisa)), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(strcolVisa)).Length, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                    }
                    if (strcolMaster != "" && strcolMaster != "0.0000")
                    {
                        e.Graphics.DrawString("Master : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                        e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(strcolMaster)), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(strcolMaster)).Length, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                    }
                    if (strcolAmex != "" && strcolAmex != "0.0000")
                    {
                        e.Graphics.DrawString("AMEX : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                        e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(strcolAmex)), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(strcolAmex)).Length, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                    }
                    if (strcolDisc != "" && strcolDisc != "0.0000")
                    {
                        e.Graphics.DrawString("Disc : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                        e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(strcolDisc)), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(strcolDisc)).Length, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                    }
                    if (strcolDiners != "" && strcolDiners != "0.0000")
                    {
                        e.Graphics.DrawString("UnionPay : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                        e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(strcolDiners)), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(strcolDiners)).Length, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                    }
                    if (strcolCardETC != "" && strcolCardETC != "0.0000")
                    {
                        e.Graphics.DrawString("Credit Card : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                        e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(strcolCardETC)), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(strcolCardETC)).Length, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                    }
                    if (strcolAli != "" && strcolAli != "0.0000")
                    {
                        e.Graphics.DrawString("Ali : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                        e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(strcolAli)), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(strcolAli)).Length, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                    }
                    if (strcolWechat != "" && strcolWechat != "0.0000")
                    {
                        e.Graphics.DrawString("WeChat : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                        e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(strcolWechat)), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(strcolWechat)).Length, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                    }
                    if (strcolUnion != "" && strcolUnion != "0.0000")
                    {
                        e.Graphics.DrawString("UnionQR : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                        e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(strcolUnion)), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(strcolUnion)).Length, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                    }
                    if (strcolEBT != "" && strcolEBT != "0.0000")
                    {
                        e.Graphics.DrawString("EBT : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                        e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(strcolEBT)), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(strcolEBT)).Length, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                    }
                    if (strcolGiftCard != "" && strcolGiftCard != "0.0000")
                    {
                        e.Graphics.DrawString("Gift Card : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                        e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(strcolGiftCard)), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(strcolGiftCard)).Length, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                    }
                    if (strcolGiftCertification != "" && strcolGiftCertification != "0.0000")
                    {
                        e.Graphics.DrawString("Gift Certificate : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                        e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(strcolGiftCertification)), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(strcolGiftCertification)).Length, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                    }
                    if (strcolCheck != "" && strcolCheck != "0.0000")
                    {
                        e.Graphics.DrawString("Delivery : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                        e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(strcolCheck)), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(strcolCheck)).Length, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                    }
                    if (strcolCredit != "" && strcolCredit != "0.0000")
                    {
                        e.Graphics.DrawString("Credit : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                        e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(strcolCredit)), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(strcolCredit)).Length, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                    }

                    e.Graphics.DrawString("Change Due : ", font, new SolidBrush(Color.Black), iStartX + 14, iStartY + Offset);
                    e.Graphics.DrawString(string.Format("{0:0.00}", double.Parse(strcolChange)), font, new SolidBrush(Color.Black), 62 - string.Format("{0:0.00}", double.Parse(strcolChange)).Length, iStartY + Offset);
                    Offset = Offset + iFontHeightGap;
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //Transaction Result (Card Trans, WeChat Trans)
                    //Card Trans
                    sQBuff = "Select ct_CustomerReceipt From HANAMART.dbo.tb_cardtrans where ct_InvNo = '" + strInvoNum + "' " + "And ct_ActionCode = 'A' ";
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
                            strCustomerReceipt = Convert.ToString(c_localdb.rs.Fields["ct_CustomerReceipt"].Value);
                            if (strCustomerReceipt != "")
                            {
                                //strCustomerReceipt = "++++++++++++++++++++++++++++\n" + strCustomerReceipt.Substring(0, strCustomerReceipt.Length - 25) + "\n++++++++++++++++++++++++++++";
                                //e.Graphics.DrawString(strCustomerReceipt, font, new SolidBrush(Color.Black), iStartX + 10, iStartY + Offset);
                                //Offset = Offset + iFontHeightGap * 17;

                                Offset = Offset + iFontHeightGap;
                                e.Graphics.DrawString("++++++++++++++++++++++++++++", font, new SolidBrush(Color.Black), iStartX + 10, iStartY + Offset);
                                Offset = Offset + iFontHeightGap;

                                int i = 0;
                                strCustomerReceipt = strCustomerReceipt.Substring(0, strCustomerReceipt.Length - 25);
                                strCustomerReceipt.Replace("\n", "\r\n");
                                string[] strSplitCustomerReceipt = strCustomerReceipt.Split(new string[] {"\r\n"}, StringSplitOptions.None);

                                for(i = 0; i < strSplitCustomerReceipt.Length; i++)
                                {
                                    e.Graphics.DrawString(strSplitCustomerReceipt[i], font, new SolidBrush(Color.Black), iStartX + 10, iStartY + Offset);
                                    Offset = Offset + iFontHeightGap;
                                }
                                e.Graphics.DrawString("++++++++++++++++++++++++++++", font, new SolidBrush(Color.Black), iStartX + 10, iStartY + Offset);
                                Offset = Offset + iFontHeightGap;
                            }
                            else
                            {
                                g_sMessage = string.Format("[{0}] CardTrans Information is not found (Invoice Number : {1}).", sMethod, strInvoNum);
                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                                c_localdb.RsClose();
                                return;
                            }
                        }
                    }
                    c_localdb.RsClose();

                    //WeChat Trans
                    if(GintLocation == 1)                       // 벤쿠버경우
                    {
                        sQBuff = "Select wt_transtype, wt_status, wt_payorderid, wt_channelId, wt_mchid From HANAMART.dbo.tb_wechatpaytrans where wt_InvNo = '"
                            + strInvoNum + "' " + "And wt_status = 'SUCCESS'  Order by wt_seq";
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
                            while (c_localdb.rs.EOF != true)
                            {
                                if (c_localdb.rs.RecordCount != 0)
                                {
                                    strwtTransType = Convert.ToString(c_localdb.rs.Fields["wt_transtype"].Value);
                                    strwtStatus = Convert.ToString(c_localdb.rs.Fields["wt_status"].Value);
                                    strwtPayorderid = Convert.ToString(c_localdb.rs.Fields["wt_payorderid"].Value);
                                    strwtPayType = Convert.ToString(c_localdb.rs.Fields["wt_channelId"].Value);
                                    strwtMchid = Convert.ToString(c_localdb.rs.Fields["wt_mchid"].Value);

                                    if ((strwtTransType == "CONFIRM" || strwtTransType == "GET_RETURN_RESPONSE") && strwtStatus == "SUCCESS" && strwtPayorderid != "")
                                    {
                                        //QR Code Image 생성모듈
                                        Bitmap btQR;
                                        QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
                                        btQR = qrCodeEncoder.Encode(strwtPayorderid, Encoding.UTF8);
                                        btQR.Save("C:\\Users\\Glen Kim\\Desktop\\Project\\hanas.hanapos\\hanas.hanapos\\hanapos.sales\\" + strwtPayorderid + ".jpg");   // 임시 저장 경로 변경 필요.

                                        Offset = Offset + iFontHeightGap;
                                        e.Graphics.DrawString("****************************************************************", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 2, iStartY + Offset);
                                        Offset = Offset + iFontHeightGap;
                                        e.Graphics.DrawString("PAY TYPE : " + strwtPayType, font, new SolidBrush(Color.Black), iStartX + 3, iStartY + Offset);
                                        Offset = Offset + iFontHeightGap;
                                        e.Graphics.DrawString("MERCHANT ID : " + strwtMchid, font, new SolidBrush(Color.Black), iStartX + 3, iStartY + Offset);
                                        Offset = Offset + iFontHeightGap;
                                        e.Graphics.DrawString("ORDER ID : " + strwtPayorderid, font, new SolidBrush(Color.Black), iStartX + 3, iStartY + Offset);
                                        Offset = Offset + iFontHeightGap;

                                        //QR Code Image 출력
                                        Image igQRImg = System.Drawing.Image.FromFile("C:\\Users\\Glen Kim\\Desktop\\Project\\hanas.hanapos\\hanas.hanapos\\hanapos.sales\\" + strwtPayorderid + ".jpg"); // 임시 저장 경로 변경 필요.
                                                                                                                                                                                                          ///////////경로 추가 필요.
                                        e.Graphics.DrawImage(igQRImg, 25, iStartY + Offset, 15, 15);
                                        Offset = Offset + iFontHeightGap * 4;
                                        e.Graphics.DrawString("****************************************************************", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 2, iStartY + Offset);
                                        Offset = Offset + iFontHeightGap;
                                    }
                                    c_localdb.rs.MoveNext();
                                }
                                else
                                {
                                    g_sMessage = string.Format("[{0}] WeChat Trans Information is not found (Invoice Number : {1}).", sMethod, strInvoNum);
                                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                                    c_localdb.RsClose();
                                    return;
                                }
                            }
                        }
                        c_localdb.RsClose();
                    }
                    

                    //HPoint Info(Previous H Point, Earned H Point Today, Balance)
                    sQBuff = "Select cFirst, cName, cTelNo From HANAMART.dbo.mfCust "
                            + "where cStore = '" + strcolCStore + "' and cCustNo = '" + strcolCustNo + "' and cID = '" + strcolCust + "'";
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
                        if (c_localdb.rs.RecordCount != 0)
                        {
                            strcFirst = Convert.ToString(c_localdb.rs.Fields["cFirst"].Value);
                            strcName = Convert.ToString(c_localdb.rs.Fields["cName"].Value);
                            strcTelNo = Convert.ToString(c_localdb.rs.Fields["cTelNo"].Value);

                            if (strcFirst != "" || strcName != "" || strcTelNo != "")
                            {
                                e.Graphics.DrawString("****************************************************************", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 2, iStartY + Offset);
                                Offset = Offset + iFontHeightGap;
                                e.Graphics.DrawString("YOUR SAVING & H-POINT SUMMARY", new Font("Arial", 8, FontStyle.Bold), new SolidBrush(Color.Black), iStartX + 7, iStartY + Offset);
                                Offset = Offset + iFontHeightGap;

                                if(GintLocation == 1)                           // 벤쿠버 인 경우
                                {
                                    e.Graphics.DrawString("Member : " + strcFirst + " " + strcName + " (" + strcolCust.Substring((strcolCust.Length) - 8, 8) + ")", new Font("Arial", 8, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 3, iStartY + Offset);
                                    Offset = Offset + iFontHeightGap;
                                }
                                else if(GintLocation == 2)                      // 토론토 인 경우
                                {
                                    e.Graphics.DrawString("Member : " + strcolCust.Substring((strcolCust.Length) - 8, 8), new Font("Arial", 8, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 3, iStartY + Offset);
                                    Offset = Offset + iFontHeightGap;
                                }
                                else                                            // 미국 인 경우
                                {
                                    e.Graphics.DrawString("Member : " + strcFirst + " " + strcName + " (" + strcolCust.Substring((strcolCust.Length) - 8, 8) + ")", new Font("Arial", 8, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 3, iStartY + Offset);
                                    Offset = Offset + iFontHeightGap;
                                }
                                
                                e.Graphics.DrawString("....................................................................................", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 2, iStartY + Offset);
                                Offset = Offset + iFontHeightGap;
                                e.Graphics.DrawString("Previous H-Point : ", font, new SolidBrush(Color.Black), iStartX + 3, iStartY + Offset);
                                //e.Graphics.DrawString(string.Format("{0:0,0}", double.Parse(strcolHPPrev)) + " P", font, new SolidBrush(Color.Black), 60 - string.Format("{0:0,0}", double.Parse(strcolHPPrev)).Length, iStartY + Offset);
                                e.Graphics.DrawString(string.Format("{0:0,0}", double.Parse(strcolHPPrev)) + " P", font, new SolidBrush(Color.Black), 60 - (string.Format("{0:0,0}", double.Parse(strcolHPPrev)) + " P ($" + string.Format("{0:0.00}", double.Parse(strcolHPBal) / 500) + ")").Length, iStartY + Offset);
                                
                                Offset = Offset + iFontHeightGap;
                                if (strcolHPEarn != "0")
                                {
                                    e.Graphics.DrawString("Earned H-Point Today : ", font, new SolidBrush(Color.Black), iStartX + 3, iStartY + Offset);
                                    //e.Graphics.DrawString(string.Format("{0:0,0}", double.Parse(strcolHPEarn)) + " P", font, new SolidBrush(Color.Black), 60 - string.Format("{0:0,0}", double.Parse(strcolHPEarn)).Length, iStartY + Offset);
                                    e.Graphics.DrawString(string.Format("{0:0,0}", double.Parse(strcolHPEarn)) + " P", font, new SolidBrush(Color.Black), 60 - (string.Format("{0:0,0}", double.Parse(strcolHPEarn)) + " P ($" + string.Format("{0:0.00}", double.Parse(strcolHPBal) / 500) + ")").Length, iStartY + Offset);
                                    
                                    Offset = Offset + iFontHeightGap;
                                }
                                if (strcolHPBonus != "0")
                                {
                                    e.Graphics.DrawString("** Bonus H-Point ** : ", font, new SolidBrush(Color.Black), iStartX + 3, iStartY + Offset);
                                    //e.Graphics.DrawString(string.Format("{0:0,0}", double.Parse(strcolHPBonus)) + " P", font, new SolidBrush(Color.Black), 60 - string.Format("{0:0,0}", double.Parse(strcolHPBonus)).Length, iStartY + Offset);
                                    e.Graphics.DrawString(string.Format("{0:0,0}", double.Parse(strcolHPBonus)) + " P", font, new SolidBrush(Color.Black), 60 - (string.Format("{0:0,0}", double.Parse(strcolHPBonus)) + " P ($" + string.Format("{0:0.00}", double.Parse(strcolHPBal) / 500) + ")").Length, iStartY + Offset);
                                    
                                    Offset = Offset + iFontHeightGap;
                                }
                                if (strcolHPUsed != "0")
                                {
                                    e.Graphics.DrawString("Used H-Point Today : ", font, new SolidBrush(Color.Black), iStartX + 3, iStartY + Offset);
                                    //e.Graphics.DrawString(string.Format("{0:0,0}", double.Parse(strcolHPUsed)) + " P", font, new SolidBrush(Color.Black), 60 - string.Format("{0:0,0}", double.Parse(strcolHPUsed)).Length, iStartY + Offset);
                                    e.Graphics.DrawString(string.Format("{0:0,0}", double.Parse(strcolHPUsed)) + " P", font, new SolidBrush(Color.Black), 60 - (string.Format("{0:0,0}", double.Parse(strcolHPUsed)) + " P ($" + string.Format("{0:0.00}", double.Parse(strcolHPBal) / 500) + ")").Length, iStartY + Offset);                                    

                                    Offset = Offset + iFontHeightGap;
                                }

                                e.Graphics.DrawString("Balance : ", font, new SolidBrush(Color.Black), iStartX + 3, iStartY + Offset);

                                if (GintLocation == 1)                               // 벤쿠버 인 경우
                                {
                                    //e.Graphics.DrawString(string.Format("{0:0,0}", double.Parse(strcolHPBal)) + " P ($" + string.Format("{0:0.00}", double.Parse(strcolHPBal) / 500) + ")", font, new SolidBrush(Color.Black), 60 - (string.Format("{0:0,0}", double.Parse(strcolHPBal)) + " P ($" + string.Format("{0:0.00}", double.Parse(strcolHPBal) / 500) + ")").Length, iStartY + Offset);

                                    e.Graphics.DrawString(string.Format("{0:0,0}", double.Parse(strcolHPBal)) + " P", font, new SolidBrush(Color.Black), 60 - (string.Format("{0:0,0}", double.Parse(strcolHPBal)) + " P ($" + string.Format("{0:0.00}", double.Parse(strcolHPBal) / 500) + ")").Length, iStartY + Offset);
                                    e.Graphics.DrawString("   ($" + string.Format("{0:0.00}", double.Parse(strcolHPBal) / 500) + ")", new Font("Arial", 8, FontStyle.Bold), new SolidBrush(Color.Black), 60 - ("($" + string.Format("{0:0.00}", double.Parse(strcolHPBal) / 500) + ")").Length, iStartY + Offset);
                                    Offset = Offset + iFontHeightGap;
                                    e.Graphics.DrawString("****************************************************************", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 2, iStartY + Offset);
                                    Offset = Offset + iFontHeightGap;

                                    // 추가 멘트 추가 요청
                                    e.Graphics.DrawString("다음 번 결제시, ", new Font("Arial", 6, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 3, iStartY + Offset);
                                    e.Graphics.DrawString(" $" + string.Format("{0:0.00}", double.Parse(strcolHPBal) / 500), new Font("Arial", 6, FontStyle.Bold), new SolidBrush(Color.Black), iStartX + 19, iStartY + Offset);
                                    e.Graphics.DrawString("를 현금 같이 사용하실 수 있습니다.", new Font("Arial", 6, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 27, iStartY + Offset);
                                    Offset = Offset + iFontHeightGap;

                                    e.Graphics.DrawString("Cash value of ", new Font("Arial", 6, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 8, iStartY + Offset);
                                    e.Graphics.DrawString(" $" + string.Format("{0:0.00}", double.Parse(strcolHPBal) / 500), new Font("Arial", 6, FontStyle.Bold), new SolidBrush(Color.Black), iStartX + 22, iStartY + Offset);
                                    e.Graphics.DrawString("can be used on next purchase.", new Font("Arial", 6, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 30, iStartY + Offset);
                                    Offset = Offset + iFontHeightGap;
                                }
                                else
                                {
                                    e.Graphics.DrawString(string.Format("{0:0,0}", double.Parse(strcolHPBal)) + " P", font, new SolidBrush(Color.Black), 60 - string.Format("{0:0,0}", double.Parse(strcolHPBal)).Length, iStartY + Offset);
                                    Offset = Offset + iFontHeightGap;
                                    e.Graphics.DrawString("****************************************************************", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 2, iStartY + Offset);
                                    Offset = Offset + iFontHeightGap;
                                }
                                //Offset = Offset + iFontHeightGap;
                                //e.Graphics.DrawString("****************************************************************", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 2, iStartY + Offset);
                                //Offset = Offset + iFontHeightGap;
                            }
                            else
                            {
                                g_sMessage = string.Format("[{0}] HPoint Information is not found (Invoice Number : {1}).", sMethod, strInvoNum);
                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                                //c_localdb.RsClose();
                                //return;
                            }
                        }
                    }
                    c_localdb.RsClose();

                    //    //Market Notice
                    ///////////////////////////////////// 매장 코드 별로 메세지 구분 필요//////////////

                    if (GintLocation == 1)                               // 벤쿠버 인 경우
                    {
                        //Survey QR URL 
                        sQBuff = "Select sm_id, sm_title_eng, sm_sdate, sm_edate, sm_url, sm_status FROM tb_SurveyMaster where sm_status = 1";                                
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
                            if (c_localdb.rs.RecordCount != 0)
                            {
                                string strCurDate = DateTime.Now.ToString("yyyy-MM-dd");

                                strSurveySDate = Convert.ToString(c_localdb.rs.Fields["sm_sdate"].Value);
                                strSurveyEDate = Convert.ToString(c_localdb.rs.Fields["sm_edate"].Value);

                                // Start Date check.
                                if ((Convert.ToDateTime(strSurveySDate) <= Convert.ToDateTime(strCurDate)) && (Convert.ToDateTime(strSurveyEDate) >= Convert.ToDateTime(strCurDate)))   // Survey 기간일때
                                {
                                    //QR Code Image 생성모듈
                                    Bitmap btQR;
                                    QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
                                    btQR = qrCodeEncoder.Encode(Convert.ToString(c_localdb.rs.Fields["sm_url"].Value), Encoding.UTF8);
                                    btQR.Save(Application.StartupPath + "\\QRCode\\" + Convert.ToString(c_localdb.rs.Fields["sm_id"].Value) + "_Survey" +".jpg");   // QR Image 저장

                                    Offset = Offset + iFontHeightGap;
                                    e.Graphics.DrawString("****************************************************************", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 2, iStartY + Offset);
                                    Offset = Offset + iFontHeightGap;
                                    e.Graphics.DrawString(Convert.ToString(c_localdb.rs.Fields["sm_title_eng"].Value), new Font("Arial", 8, FontStyle.Bold), new SolidBrush(Color.Black), iStartX + 18, iStartY + Offset);
                                    Offset = Offset + iFontHeightGap;
                                    e.Graphics.DrawString("Scan QR Code to access the Survey", new Font("Arial", 8, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 13, iStartY + Offset);
                                    Offset = Offset + iFontHeightGap;
                                    Offset = Offset + iFontHeightGap;

                                    //QR Code Image 출력
                                    Image igQRImg = System.Drawing.Image.FromFile(Application.StartupPath + "\\QRCode\\" + Convert.ToString(c_localdb.rs.Fields["sm_id"].Value) + "_Survey" + ".jpg"); 
                                    e.Graphics.DrawImage(igQRImg, 26, iStartY + Offset, 19, 19);
                                    Offset = Offset + iFontHeightGap * 7;
                                    e.Graphics.DrawString("****************************************************************", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 2, iStartY + Offset);
                                    Offset = Offset + iFontHeightGap;
                                }

                                //e.Graphics.DrawString("****************************************************************", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 2, iStartY + Offset);
                                //Offset = Offset + iFontHeightGap;
                                //e.Graphics.DrawString("For the safety of customers, employees and public,", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                                //Offset = Offset + iFontHeightGap;
                                //e.Graphics.DrawString("we are temporarily NOT accepting any returns until", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                                //Offset = Offset + iFontHeightGap;
                                //e.Graphics.DrawString("further notice. We apologize any inconvenience ", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                                //Offset = Offset + iFontHeightGap;
                                //e.Graphics.DrawString("this may cause.", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                                //Offset = Offset + iFontHeightGap;
                                //e.Graphics.DrawString("****************************************************************", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 2, iStartY + Offset);
                                //Offset = Offset + iFontHeightGap;
                            }
                        }
                    }
                    else if (GintLocation == 2)                          // 토론토 인 경우
                    {
                        e.Graphics.DrawString("****************************************************************", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 2, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                        e.Graphics.DrawString("General merchandise must be returned within 7 days.", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                        e.Graphics.DrawString("Meat, Fish, Produce, Bakery and frozen/daily products", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                        e.Graphics.DrawString("must be returned or exchanged within 24 hours.Opened", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                        e.Graphics.DrawString("or used cosmetics cannot be exchanged or returned.", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                        e.Graphics.DrawString("All merchandise must be in original condition with", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                        e.Graphics.DrawString("the original receipt for return or exchange.", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                        Offset = Offset + iFontHeightGap;
                        e.Graphics.DrawString("**영수증을 반드시 지참하셔야 합니다.미 지참 시 교환/환불이 불가합니다.**", new Font("Arial", 5, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                        e.Graphics.DrawString("- 구매일로부터 7일이내에 반품 또는 교환하셔야 합니다.단,육류,수산,청과,", new Font("Arial", 5, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                        e.Graphics.DrawString("  냉동/냉장 조리식품은  24시간 이내에 반품 또는 교환하셔야 합니다.", new Font("Arial", 5, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                        e.Graphics.DrawString("- 신용카드 취소는 결제하신 카드로만 처리 가능합니다.", new Font("Arial", 5, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                        e.Graphics.DrawString("- 구입 후 패키지 개봉/훼손 시에는 교환/환불이 불가합니다.", new Font("Arial", 5, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                        e.Graphics.DrawString("****************************************************************", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 2, iStartY + Offset);
                        Offset = Offset + iFontHeightGap;
                    }
                    else                                                 // 미국 인 경우
                    {
                        if (c_poscominfo.ci_mkno == "55")          // 오로라 매장인 경우
                        {
                            e.Graphics.DrawString("****************************************************************", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 2, iStartY + Offset);
                            Offset = Offset + iFontHeightGap;
                            e.Graphics.DrawString("Returns must be made within 7 days of the original", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                            Offset = Offset + iFontHeightGap;
                            e.Graphics.DrawString("purchase date on the receipt. Meat, fish, produce,", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                            Offset = Offset + iFontHeightGap;
                            e.Graphics.DrawString("frozen products and dairy products must be returned", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                            Offset = Offset + iFontHeightGap;
                            e.Graphics.DrawString("within 24 hours.", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                            Offset = Offset + iFontHeightGap;
                            e.Graphics.DrawString("Products must be in original condition for return.", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                            Offset = Offset + iFontHeightGap;
                            e.Graphics.DrawString("****************************************************************", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 2, iStartY + Offset);
                            Offset = Offset + iFontHeightGap;

                        }
                        else
                        {
                            e.Graphics.DrawString("****************************************************************", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 2, iStartY + Offset);
                            Offset = Offset + iFontHeightGap;
                            e.Graphics.DrawString("For the safety of customers, employees and public,", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                            Offset = Offset + iFontHeightGap;
                            e.Graphics.DrawString("we are temporarily NOT accepting any returns until", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                            Offset = Offset + iFontHeightGap;
                            e.Graphics.DrawString("further notice. We apologize any inconvenience ", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                            Offset = Offset + iFontHeightGap;
                            e.Graphics.DrawString("this may cause.", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 4, iStartY + Offset);
                            Offset = Offset + iFontHeightGap;
                            e.Graphics.DrawString("****************************************************************", new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 2, iStartY + Offset);
                            Offset = Offset + iFontHeightGap;
                        }
                    }
                    //Tax number, Barcode Num
                    /////////////////////////////// 매장 GST 번호 받아오는 부분 필요/////////////
                    //strMKGstNo = "R899391346"; /// 임시
                    strMKGstNo = c_poscominfo.si_MarketGstNum;
                    e.Graphics.DrawString("TAX Reg.# " + strMKGstNo, new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 2, iStartY + Offset);
                    e.Graphics.DrawString(strInvoNum, new Font("Arial", 7, FontStyle.Regular), new SolidBrush(Color.Black), 60 - strInvoNum.Length, iStartY + Offset);
                    Offset = Offset + iFontHeightGap;
                }

                //Receipt Barcode
                //////////////////////////// Hold, Suspend Receipt일 경우 구분하는 부분 필요.
                Barcodefont = new Font("IDAutomationHC39M", 12, FontStyle.Regular);

                if (GstrReprint == "N" || GstrReprint == "R")
                {
                    e.Graphics.DrawString("*" + strInvoNum + "*", Barcodefont, new SolidBrush(Color.Black), iStartX + 7, iStartY + Offset);
                    //Thank you for Shopping.
                    Offset = Offset + iFontHeightGap * 3;
                    e.Graphics.DrawString("\nThank You For Shopping", new Font("Arial", 12, FontStyle.Bold), new SolidBrush(Color.Black), iStartX + 7, iStartY + Offset);
                    Offset = Offset + iFontHeightGap;
                }
                else if (GstrReprint == "S")
                {
                    e.Graphics.DrawString("-----------------------------------------------------------------------", font, new SolidBrush(Color.Black), iStartX, iStartY + Offset);
                    Offset = Offset + iFontHeightGap;
                    if(GintLocation != 3)                   // 미국이 아닌 경우
                    {
                        e.Graphics.DrawString("*1." + strInvoNum + "*", Barcodefont, new SolidBrush(Color.Black), iStartX + 3, iStartY + Offset);
                    }
                    else                                    // 미국인 경우
                    {
                        e.Graphics.DrawString("*1Z2" + strInvoNum + "*", Barcodefont, new SolidBrush(Color.Black), iStartX + 3, iStartY + Offset);
                    }
                    
                    Offset = Offset + iFontHeightGap;
                }

                //Coupon 출력 로직 .
                if (GstrReprint == "N")       // 신규 영수증인 때만 출력,, Reprint시 출력 안되게.
                {
                    if(GintLocation == 1)                   // 벤쿠버 인 경우
                    {
                        

                    }
                    else if (GintLocation == 2)             // 토론토 인 경우
                    {
                        if (c_poscominfo.ci_mkno == "11" && strcolStation != "2")
                        {
                            string strCurDate = DateTime.Now.ToString("yyyy-MM-dd");

                            //날짜 조건 토론토 리치몬드힐 매장 꼬리표 2022-11-18 ~ 2022-12-01 경품 추첨권 출력 $30 당 1장 출력 Limit 없음.
                            if ((Convert.ToDateTime("2022-11-18") <= Convert.ToDateTime(strCurDate)) && (Convert.ToDateTime("2022-12-01") >= Convert.ToDateTime(strCurDate)))         // Promo End Date가 현재 날짜보다 큰(늦은) 경우 
                            {
                                if (dTotalDue >= 29.99)
                                {
                                    for (int i = 1; i < dTotalDue / 30; i++)
                                    {
                                        PrtCoupon.PrintController = new System.Drawing.Printing.StandardPrintController();
                                        PrtCoupon.Print();
                                    }
                                }
                            }
                            //날짜 조건 토론토 리치몬드힐 매장 꼬리표 2023-05-19 ~ 2023-05-22 $100 구매시 $5할인, $200 구매시 $10할인, $200 구매시 $15할인.
                            if ((Convert.ToDateTime("2023-05-19") <= Convert.ToDateTime(strCurDate)) && (Convert.ToDateTime("2023-05-22") >= Convert.ToDateTime(strCurDate)))         // Promo End Date가 현재 날짜보다 큰(늦은) 경우 
                            {
                                if (dTotalDue > 99.99  && dTotalDue < 200)
                                {
                                    imgCouponFilesPath = Directory.GetFiles(Application.StartupPath + "\\Coupon\\", "dc_5_can_tor_20230519_RichmondHill_Steeles.jpg", SearchOption.TopDirectoryOnly);
                                    PrtCoupon.PrintController = new System.Drawing.Printing.StandardPrintController();
                                    PrtCoupon.Print();                                    
                                }
                                else if (dTotalDue > 199.99 && dTotalDue < 300)
                                {
                                    imgCouponFilesPath = Directory.GetFiles(Application.StartupPath + "\\Coupon\\", "dc_10_can_tor_20230519_RichmondHill_Steeles.jpg", SearchOption.TopDirectoryOnly);
                                    PrtCoupon.PrintController = new System.Drawing.Printing.StandardPrintController();
                                    PrtCoupon.Print();
                                }
                                else if (dTotalDue > 299.99)
                                {
                                    imgCouponFilesPath = Directory.GetFiles(Application.StartupPath + "\\Coupon\\", "dc_15_can_tor_20230519_RichmondHill_Steeles.jpg", SearchOption.TopDirectoryOnly);
                                    PrtCoupon.PrintController = new System.Drawing.Printing.StandardPrintController();
                                    PrtCoupon.Print();
                                }
                            }
                        }
                    }
                    else                                    // 미국 인 경우
                    {
                        // Discount Coupon Table에서 확인 필요.
                        string strCurDate = DateTime.Now.ToString("yyyy-MM-dd");
                        sQBuff = "Select * From tb_Discountcoupon Where dc_Deleted = 0 And dc_Void = 0 And dc_PrtCoupon <> 0 And dc_sPrtDate <= '" + strCurDate + "' and dc_ePrtDate >= '" + strCurDate + "' Order by dc_Seq";        // 현재 생성되어 있는 Coupon 리스트 조회.
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
                            if (c_localdb.rs.RecordCount != 0)
                            {
                                PrtCoupon.PrintController = new System.Drawing.Printing.StandardPrintController();
                                PrtCoupon.Print();
                            }
                        }
                        
                        //if (c_poscominfo.ci_mkno == "85")
                        //{
                        //    string strCurDate = DateTime.Now.ToString("yyyy-MM-dd");

                        //    //날짜 조건 미국 레드몬드 꼬리표 2023-09-28 ~ 2023-10-01 상품 증정권 출력. $50 이상 "1", $100 이상 "2", $200 이상 "3", $300 이상 "4".
                        //    if ((Convert.ToDateTime("2023-09-28") <= Convert.ToDateTime(strCurDate)) && (Convert.ToDateTime("2023-10-01") >= Convert.ToDateTime(strCurDate)))         // Promo End Date가 현재 날짜보다 큰(늦은) 경우 
                        //    {
                        //        if (dTotalDue > 49.99 && dTotalDue < 100)
                        //        {
                        //            imgCouponFilesPath = Directory.GetFiles(Application.StartupPath + "\\Coupon\\", "exchange_1_usa_20230928_Redmond.jpg", SearchOption.TopDirectoryOnly);
                        //            //setUseCoupon(true);
                        //            PrtCoupon.PrintController = new System.Drawing.Printing.StandardPrintController();
                        //            PrtCoupon.Print();
                        //        }
                        //        else if (dTotalDue > 99.99 && dTotalDue < 200)
                        //        {
                        //            imgCouponFilesPath = Directory.GetFiles(Application.StartupPath + "\\Coupon\\", "exchange_2_usa_20230928_Redmond.jpg", SearchOption.TopDirectoryOnly);
                        //            PrtCoupon.PrintController = new System.Drawing.Printing.StandardPrintController();
                        //            PrtCoupon.Print();
                        //        }
                        //        else if (dTotalDue > 199.99 && dTotalDue < 300)
                        //        {
                        //            imgCouponFilesPath = Directory.GetFiles(Application.StartupPath + "\\Coupon\\", "exchange_3_usa_20230928_Redmond.jpg", SearchOption.TopDirectoryOnly);
                        //            PrtCoupon.PrintController = new System.Drawing.Printing.StandardPrintController();
                        //            PrtCoupon.Print();
                        //        }
                        //        else if (dTotalDue > 299.99)
                        //        {
                        //            imgCouponFilesPath = Directory.GetFiles(Application.StartupPath + "\\Coupon\\", "exchange_4_usa_20230928_Redmond.jpg", SearchOption.TopDirectoryOnly);
                        //            PrtCoupon.PrintController = new System.Drawing.Printing.StandardPrintController();
                        //            PrtCoupon.Print();
                        //        }
                        //    }
                        //}
                    }
                }

                g_sMessage = string.Format("[{0}] Receipt Print Done. (ctrlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                ////////////////////////////////////////////////////////Receipt Detail END/////////////////////////////////////////////////////////////////////////
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

        private void PrtCoupon_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            string sQBuff = string.Empty;
            long lReturn = 0;
            string[] imgFiles;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            g_sMessage = string.Format("[{0}] Start Coupon Print (ctrlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            Font font = new Font("Arial", 9, FontStyle.Regular);
            float fontHeight = font.GetHeight();
            int iStartX = 0;
            int iStartY = 50;
            int Offset = 0;
            int iFontHeightGap = (int)fontHeight;

            string strcolDate = string.Empty;
            string strcolCust = string.Empty;
            string strcolCStore = string.Empty;
            string strcolCustNo = string.Empty;
            string strcFirst = string.Empty;
            string strcName = string.Empty;
            string strcTelNo = string.Empty;
            string strTemp = string.Empty;

            string strCondition = string.Empty;
            string strCondition1 = string.Empty;
            string strCurDate = DateTime.Now.ToString("yyyy-MM-dd");
            string strUSCurDate = DateTime.Now.ToString("MMM dd, yyyy");
            string strSalesAmt = string.Empty;
            int iItemCnt = 0;
            int iPrtGB = 0;
            bool bLsw = false;
            int iCouponCnt = 0;
            double dCouponAmt = 0;
            Font Barcodefont;


            // Market Number 로 로직 구분.
            //c_poscominfo.ci_mkno

            // Coupon Path
            if (GintLocation != 3)             // 미국이 아닌 경우
            {
                if (GintLocation != 1)           // 토론토 인 경우
                {
                    imgFiles = imgCouponFilesPath;
                    Image igCouponImg = Image.FromFile(imgFiles[0]);
                    e.Graphics.DrawImage(igCouponImg, 2, 0, 275, 300);
                }
                else                            // 벤쿠버 인 경우
                {
                    imgFiles = imgCouponFilesPath;
                    Image igCouponImg = Image.FromFile(imgFiles[0]);
                    e.Graphics.DrawImage(igCouponImg, 2, 0, 275, 300);
                    
                    Barcodefont = new Font("3 of 9 Barcode", 25, FontStyle.Regular);
                    Offset = Offset + (iFontHeightGap * 9);
                    e.Graphics.DrawString("*62.10005*", Barcodefont, new SolidBrush(Color.Black), iStartX + 30, iStartY + Offset);
                    Offset = Offset + (iFontHeightGap * 3);
                    e.Graphics.DrawString("03/27/2025 ~ 04/30/2025" , new Font("Arial", 8, FontStyle.Regular), new SolidBrush(Color.Black), iStartX + 135, iStartY + Offset);
                    Offset = Offset + iFontHeightGap;
                }
            }
            else                                // 미국인 경우
            {
                try
                {
                    lReturn = c_localdb.DBConnection();
                    if (lReturn < 0)
                    {
                        g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                        return;
                    }
                    // Discount Coupon Table에서 확인 필요.
                    sQBuff = "Select * From tb_Discountcoupon Where dc_Deleted = 0 And dc_Void = 0 And dc_PrtCoupon <> 0 And dc_sPrtDate <= '" + strCurDate + "' and dc_ePrtDate >= '" + strCurDate + "' Order by dc_Seq";        // 현재 생성되어 있는 Coupon 리스트 조회.

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
                        while (!c_localdb.rs.EOF)
                        {
                            // Condition Check
                            if (Convert.ToBoolean(c_localdb.rs.Fields["dc_PromoAvoid"].Value) == true)           //'세일상품 제외
                                strCondition = " And Rtrim(Ltrim(tPromo)) = '' ";

                            if (Convert.ToBoolean(c_localdb.rs.Fields["dc_MemberAvoid"].Value) == true)           //'맴버 할인상품 제외
                                strCondition = strCondition + " And Rtrim(Ltrim(tSpecial)) = '' ";

                            if (Convert.ToBoolean(c_localdb.rs.Fields["dc_DCItemAvoid"].Value) == true)           //'캐쉬어 할인상품 제외
                                strCondition = strCondition + " And tType Not In ('11','48') ";

                            if (Convert.ToBoolean(c_localdb.rs.Fields["dc_ExceptionalAvoid"].Value) == true)           //'상품정보에 제외상품으로 등록한 상품 제외
                                strCondition = strCondition + " ";

                            if (Convert.ToBoolean(c_localdb.rs.Fields["dc_IncludeTax"].Value) == true)           //'상품정보에 제외상품으로 등록한 상품 제외
                                strCondition1 = " Sum(tAmt+tGst+tPst) ";
                            else
                                strCondition1 = " Sum(tAmt) ";

                            if (Convert.ToBoolean(c_localdb.rs.Fields["dc_UsePrtPeriod"].Value) == false ||
                              (Convert.ToDateTime(c_localdb.rs.Fields["dc_sprtdate"].Value) <= Convert.ToDateTime(strCurDate) &&
                              Convert.ToDateTime(strCurDate) <= Convert.ToDateTime(c_localdb.rs.Fields["dc_eprtdate"].Value)))
                            {
                                switch (Convert.ToInt32(c_localdb.rs.Fields["dc_IssueGB"].Value))
                                {
                                    case 1:                                 //'조건없이 쿠폰 발행
                                    case 2:                                 //'총 구매액이 범위에 들어갔을 경우
                                        try
                                        {
                                            lReturn = c_localdb2.DBConnection();

                                            if (lReturn < 0)
                                            {
                                                g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb2.error_message);
                                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                                            }

                                            sQBuff = "Select " + strCondition1 + " as SalesAmt From tb_SoldItem " +
                                                    "Where tInvno = '" + GstrPrtInvno + "' And tUpCode <> '3' " + strCondition;

                                            lReturn = c_localdb2.RsOpen(sQBuff);

                                            if (lReturn == 1)
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(c_localdb2.rs.Fields["SalesAmt"].Value)))            // 합계가 Null 아닌 경우
                                                {
                                                    strSalesAmt = Convert.ToString(c_localdb2.rs.Fields["SalesAmt"].Value);

                                                    if (Convert.ToDouble(c_localdb.rs.Fields["dc_LowAmt"].Value) > Convert.ToDouble(strSalesAmt) ||
                                                        Convert.ToDouble(strSalesAmt) > Convert.ToDouble(c_localdb.rs.Fields["dc_HighAmt"].Value))
                                                    {
                                                        bLsw = true;
                                                    }
                                                }
                                                else
                                                {
                                                    strSalesAmt = "0";
                                                    bLsw = true;
                                                }
                                            }
                                            else
                                            {
                                                g_sMessage = string.Format("[{0}] Sales Amount query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb2.error_message);
                                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                                                c_localdb2.DBClose();
                                            }
                                            c_localdb2.RsClose();
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
                                            c_localdb2.DBClose();
                                        }
                                        break;
                                    case 3:                                 //'요구 부서 구매액이 범위에 들어갔을 경우, tUpCode=3인 것은 dc_treadascash가 False인 쿠폰임
                                        try
                                        {
                                            lReturn = c_localdb2.DBConnection();

                                            if (lReturn < 0)
                                            {
                                                g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb2.error_message);
                                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                                            }

                                            sQBuff = "Select " + strCondition1 + " as SalesAmt From tb_SoldItem Left Join tb_ProdType on tPType = pType Left Join tb_Product on tProd = prodid " +
                                                    "Where tInvno = " + GstrPrtInvno + "  And tUpCode <> '3' And ptcode = '" + Convert.ToString(c_localdb.rs.Fields["dc_RelationCode"].Value) + "' " + strCondition;

                                            lReturn = c_localdb2.RsOpen(sQBuff);

                                            if (lReturn == 1)
                                            {
                                                if (!string.IsNullOrEmpty(Convert.ToString(c_localdb2.rs.Fields["SalesAmt"].Value)))            // 합계가 Null 아닌 경우
                                                {
                                                    strSalesAmt = Convert.ToString(c_localdb2.rs.Fields["SalesAmt"].Value);
                                                    if (Convert.ToDouble(c_localdb.rs.Fields["dc_LowAmt"].Value) > Convert.ToDouble(strSalesAmt) ||
                                                             Convert.ToDouble(strSalesAmt) > Convert.ToDouble(c_localdb.rs.Fields["dc_HighAmt"].Value))
                                                    {
                                                        bLsw = true;
                                                    }
                                                }
                                                else
                                                {
                                                    strSalesAmt = "0";
                                                    bLsw = true;
                                                }
                                            }
                                            else
                                            {
                                                g_sMessage = string.Format("[{0}] Sales Amount query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb2.error_message);
                                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                                                c_localdb2.DBClose();
                                            }
                                            c_localdb2.RsClose();
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
                                            c_localdb2.DBClose();
                                        }
                                        break;

                                    case 4:                                 //'요구 상품이 구매가 되었을 경우
                                        try
                                        {
                                            lReturn = c_localdb2.DBConnection();

                                            if (lReturn < 0)
                                            {
                                                g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb2.error_message);
                                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                                            }

                                            sQBuff = "Select Count(*) as ItemCnt From tb_SoldItem " +
                                                     "Where tInvno = " + GstrPrtInvno + " And tProd = '" + Convert.ToString(c_localdb.rs.Fields["dc_RelationCode"].Value) + "' And tQty > 0 And tAmt > 0 And tType <= '40'";

                                            lReturn = c_localdb2.RsOpen(sQBuff);

                                            if (lReturn == 1)
                                            {
                                                if (Convert.ToInt32(c_localdb2.rs.Fields["ItemCnt"].Value) <= 0)
                                                {
                                                    bLsw = true;
                                                }

                                                iItemCnt = Convert.ToInt32(c_localdb2.rs.Fields["ItemCnt"].Value);
                                            }
                                            else
                                            {
                                                g_sMessage = string.Format("[{0}] Item Count query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb2.error_message);
                                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                                                c_localdb2.DBClose();
                                                break;
                                            }
                                            c_localdb2.RsClose();
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
                                            c_localdb2.DBClose();
                                        }
                                        break;
                                    default:
                                        bLsw = true;
                                        c_localdb2.RsClose();
                                        c_localdb2.DBClose();
                                        break;
                                }
                            }

                            if (bLsw == false)
                            {
                                if (Convert.ToInt32(c_localdb.rs.Fields["dc_PrtGB"].Value) != 0)
                                    iPrtGB = Convert.ToInt32(c_localdb.rs.Fields["dc_PrtGB"].Value);
                                else
                                    iPrtGB = 0;

                                switch (Convert.ToInt32(c_localdb.rs.Fields["dc_IssueGB"].Value))
                                {
                                    case 1:
                                        iCouponCnt = 1;
                                        dCouponAmt = Convert.ToDouble(c_localdb.rs.Fields["dc_Amt"].Value);
                                        break;
                                    case 2:
                                        iCouponCnt = Convert.ToInt32(Convert.ToDouble(strSalesAmt) / Convert.ToDouble(c_localdb.rs.Fields["dc_LowAmt"].Value));
                                        dCouponAmt = Convert.ToDouble(c_localdb.rs.Fields["dc_Amt"].Value);
                                        break;
                                    case 3:
                                        iCouponCnt = 1;
                                        dCouponAmt = Convert.ToDouble(c_localdb.rs.Fields["dc_Amt"].Value) * Convert.ToInt32(Convert.ToDouble(strSalesAmt) / Convert.ToDouble(c_localdb.rs.Fields["dc_LowAmt"].Value));
                                        break;
                                    default:
                                        iCouponCnt = 0;
                                        dCouponAmt = 0;
                                        break;
                                }

                                // Coupon Image 출력
                                if (Convert.ToBoolean(c_localdb.rs.Fields["dc_useimg"].Value) == true &&
                                  !string.IsNullOrEmpty(Convert.ToString(c_localdb.rs.Fields["dc_couponimg"].Value)))
                                {
                                    byte[] byImage = (byte[])(c_localdb.rs.Fields["dc_couponimg"].Value);
                                    Image igCouponImg = BinaryToImage(byImage);
                                    e.Graphics.DrawImage(igCouponImg, 2, 0, 275, 300);
                                }

                                // 해당되는 쿠폰 갯수 만틈 출력
                                for (int i = 1; i <= iCouponCnt; i++)
                                {
                                    // 쿠폰 이미지가 있는 경우.
                                    if (Convert.ToBoolean(c_localdb.rs.Fields["dc_useimg"].Value) == true &&
                                       !string.IsNullOrEmpty(Convert.ToString(c_localdb.rs.Fields["dc_couponimg"].Value)))
                                    {
                                        e.Graphics.DrawString("DATE : " + strUSCurDate, new Font("Arial", 10, FontStyle.Regular), new SolidBrush(Color.Black), 100 - ("DATE : " + strUSCurDate).Length, 1);
                                        Offset = Offset + (iFontHeightGap);
                                        Offset = Offset + (iFontHeightGap);
                                    }
                                    else
                                    {
                                        e.Graphics.DrawString(Convert.ToString(c_localdb.rs.Fields["dc_ename"].Value), new Font("Arial", 15, FontStyle.Bold), new SolidBrush(Color.Black), iStartX, iStartY + Offset);
                                        Offset = Offset + iFontHeightGap;
                                        e.Graphics.DrawString(Convert.ToString(c_localdb.rs.Fields["dc_kname"].Value), new Font("Arial", 15, FontStyle.Bold), new SolidBrush(Color.Black), iStartX, iStartY + Offset);
                                        Offset = Offset + iFontHeightGap;
                                    }

                                    //쿠폰 Amount 가 있는 경우
                                    if (dCouponAmt > 0)
                                    {
                                        e.Graphics.DrawString("$ " + string.Format("{0:0.00}", double.Parse(Convert.ToString(dCouponAmt))), new Font("Arial", 40, FontStyle.Bold), new SolidBrush(Color.Black), iStartX + 20, iStartY + Offset);
                                        Offset = Offset + (iFontHeightGap * 14);
                                    }
                                    else
                                    {
                                        Offset = Offset + (iFontHeightGap * 5);
                                    }

                                    // Coupon Barcode 출력
                                    Barcodefont = new Font("IDAutomationHC39M", 12, FontStyle.Regular);
                                    if (Convert.ToBoolean(c_localdb.rs.Fields["dc_RtnChk"].Value) == false)
                                    {
                                        e.Graphics.DrawString("*" + (Convert.ToString(c_localdb.rs.Fields["dc_code"].Value)) + "*", Barcodefont, new SolidBrush(Color.Black), iStartX + 15, iStartY + Offset);
                                        Offset = Offset + iFontHeightGap;
                                    }
                                    else
                                    {
                                        //ADD Issue Coupon
                                        AddIssueCoupon(dCouponAmt);

                                        e.Graphics.DrawString("*C" + (Convert.ToString(c_localdb.rs.Fields["dc_code"].Value)) + "*", Barcodefont, new SolidBrush(Color.Black), iStartX + 15, iStartY + Offset);
                                        Offset = Offset + iFontHeightGap;
                                    }
                                }                            
                            }
                            c_localdb.rs.MoveNext();
                        }
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


                //try
                //{
                //    lReturn = c_localdb.DBConnection();
                //    if (lReturn < 0)
                //    {
                //        g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                //        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                //        return;
                //    }

                //    // Customoer Information
                //    sQBuff = "Select * From HANAMART.dbo.tb_Payment where colinvno = '" + GstrPrtInvno + "'";        // Branch Code 추가되는 코드 필요.
                //    lReturn = c_localdb.RsOpen(sQBuff);
                //    if (lReturn != 1)
                //    {
                //        g_sMessage = string.Format("[{0}] Data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                //        c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                //        c_localdb.DBClose();
                //        return;
                //    }
                //    else
                //    {
                //        if (c_localdb.rs.RecordCount == 1)
                //        {
                //            strcolDate = Convert.ToString(c_localdb.rs.Fields["colDate"].Value);
                //            strcolCust = Convert.ToString(c_localdb.rs.Fields["colCust"].Value);
                //            strcolCStore = Convert.ToString(c_localdb.rs.Fields["colcStore"].Value);
                //            strcolCustNo = Convert.ToString(c_localdb.rs.Fields["colCustNo"].Value);                        
                //        }
                //        else
                //        {
                //            g_sMessage = string.Format("[{0}] Payment Information is not found (Invoice Number : {1}).", sMethod, GstrPrtInvno);
                //            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                //            // Error Message
                //            DisplayErrorMessageBox("Coupon Print", "Wrong Payment Information. \n(Invoice Number: " + GstrPrtInvno + ")", 1, sMethod);
                //            txtReceiptReprintBarcode.Clear();
                //            c_localdb.RsClose();
                //            return;
                //        }
                //    }
                //    c_localdb.RsClose();


                //    //HPoint Info(Previous H Point, Earned H Point Today, Balance)
                //    sQBuff = "Select cFirst, cName, cTelNo From HANAMART.dbo.mfCust "
                //            + "where cStore = '" + strcolCStore + "' and cCustNo = '" + strcolCustNo + "' and cID = '" + strcolCust + "'";
                //    lReturn = c_localdb.RsOpen(sQBuff);
                //    if (lReturn != 1)
                //    {
                //        g_sMessage = string.Format("[{0}] Data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                //        c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                //        c_localdb.DBClose();
                //        return;
                //    }
                //    else
                //    {
                //        if (c_localdb.rs.RecordCount != 0)
                //        {
                //            strcFirst = Convert.ToString(c_localdb.rs.Fields["cFirst"].Value);
                //            strcName = Convert.ToString(c_localdb.rs.Fields["cName"].Value);
                //            strcTelNo = Convert.ToString(c_localdb.rs.Fields["cTelNo"].Value);
                //            strTemp = strcTelNo.Substring(0, 3) +"-"+ strcTelNo.Substring(3, 3) + "-" + strcTelNo.Substring(6, 4);
                //            strcTelNo = strTemp;

                //            if (strcFirst != "" || strcName != "" || strcTelNo != "")
                //            {
                //                e.Graphics.DrawString(strcFirst + " " + strcName, new Font("Arial", 10, FontStyle.Bold), new SolidBrush(Color.Black), iStartX + 60, iStartY + Offset);
                //                Offset = Offset + iFontHeightGap;
                //                Offset = Offset + iFontHeightGap;

                //                e.Graphics.DrawString(strcTelNo, new Font("Arial", 10, FontStyle.Bold), new SolidBrush(Color.Black), iStartX + 40, iStartY + Offset);
                //                Offset = Offset + iFontHeightGap;
                //                Offset = Offset + iFontHeightGap;

                //                e.Graphics.DrawString(strcolCust, new Font("Arial", 10, FontStyle.Bold), new SolidBrush(Color.Black), iStartX + 130, iStartY + Offset);
                //                Offset = Offset + iFontHeightGap;
                //            }
                //            else
                //            {
                //                g_sMessage = string.Format("[{0}] HPoint Information is not found (Invoice Number : {1}).", sMethod, GstrPrtInvno);
                //                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                //                c_localdb.RsClose();
                //                return;
                //            }
                //        }
                //    }

                //    g_sMessage = string.Format("[{0}] Coupon Print Done(ctrlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
                //    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                //    c_localdb.DBClose();                
                //}
                //catch (SqlException ex)
                //{
                //    g_sMessage = string.Format("[{0}] Databasae error caused while operating (line: {1})\n[{0}] {2}", sMethod, ex.Source.ToString(), ex.Message.ToString());
                //    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                //}
                //catch (Exception ex)
                //{
                //    g_sMessage = string.Format("[{0}] Error caused while operating (line: {1})\n[{0}] {2}", sMethod, ex.Source.ToString(), ex.Message.ToString());
                //    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                //}
                //finally
                //{
                //    c_localdb.DBClose();
                //}
            }
        }
        private string AddIssueCoupon(double dCouponAmt)
        {
            string strCouponBarcode = string.Empty;
            int icSeq = 0;
            string strCode = string.Empty;
            string strCurDate = DateTime.Now.ToString("yyyyMMdd");
            int iChkDigit = 0;

            string sQBuff = string.Empty;
            long lReturn = 0;            

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            g_sMessage = string.Format("[{0}] AddIssueCoupon (ctrlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            lReturn = c_localdb2.DBConnection();

            if (lReturn < 0)
            {
                g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb2.error_message);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            }

            sQBuff = "Select IsNull(Max(ci_seq), 0) as ciSeq From tb_CouponIssue " +
                    "Where ci_mkgb = " + c_poscominfo.ci_mkno + " And ci_date = '" + strCurDate + "' And ci_station = " + c_poscominfo.si_counternum;
            
            lReturn = c_localdb2.RsOpen(sQBuff);

            if (lReturn == 1)
            {
                if (Convert.ToInt32(c_localdb2.rs.Fields["ciSeq"].Value) != 0)
                {
                    icSeq = Convert.ToInt32(c_localdb2.rs.Fields["ciSeq"].Value);
                }
                else
                {
                    icSeq = 101;
                }                
            }
            else
            {
                g_sMessage = string.Format("[{0}] Coupon Issue Max Seq query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb2.error_message);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                c_localdb2.DBClose();
                return strCouponBarcode;                
            }
            c_localdb2.RsClose();

            sQBuff = "Select * From tb_CouponIssue " +
                    "Where ci_mkgb = " + c_poscominfo.ci_mkno + " And ci_date = '" + strCurDate + "' And " +
                    "ci_station = " + c_poscominfo.si_counternum + " And ci_seq = " + icSeq;
            
            lReturn = c_localdb2.RsOpen(sQBuff);

            if (lReturn == 1)
            {
                if (c_localdb2.record_count == 0)
                {
                    c_localdb2.rs.AddNew();

                    c_localdb2.rs.Fields["ci_mkgb"].Value = c_poscominfo.ci_mkno;
                    c_localdb2.rs.Fields["ci_Date"].Value = DateTime.Now.ToString("yyyy-MM-dd"); ;
                    c_localdb2.rs.Fields["ci_Station"].Value = c_poscominfo.si_counternum;
                    c_localdb2.rs.Fields["ci_seq"].Value = icSeq;
                    c_localdb2.rs.Fields["ci_dcseq"].Value = c_localdb.rs.Fields["dc_seq"].Value;
                    c_localdb2.rs.Fields["ci_dccode"].Value = c_localdb.rs.Fields["dc_code"].Value;
                    c_localdb2.rs.Fields["ci_IssueInvno"].Value = GstrPrtInvno;
                    c_localdb2.rs.Fields["ci_CouponAmt"].Value = dCouponAmt;
                    c_localdb2.rs.Fields["ci_RtnChk"].Value = 0;
                    c_localdb2.rs.Fields["ci_RtnMkGb"].Value = 0;
                    c_localdb2.rs.Fields["ci_RtnInvNo"].Value = "";
                    c_localdb2.rs.Fields["ci_upflag"].Value = 3;                
                }
                c_localdb2.rs.Update();
            }
            else
            {
                g_sMessage = string.Format("[{0}] Coupon Issue query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                c_localdb2.DBClose();
                return strCouponBarcode;
            }
            c_localdb2.RsClose();
            c_localdb2.DBClose();

            //Make check Digit
            strCode = c_poscominfo.ci_mkno + strCurDate + c_poscominfo.si_counternum + icSeq;

            for(int i = 1; i < strCode.Length; i++)
            {
                iChkDigit = iChkDigit + Convert.ToInt32(strCode.Substring(i, 1));
            }
            strCouponBarcode = strCode + Convert.ToString(iChkDigit).Substring(Convert.ToString(iChkDigit).Length - 1);         //// 확인 필요.

            return strCouponBarcode;
        }

        private Image BinaryToImage(byte[] b)
        {
            if (b == null)
                return null;

            MemoryStream memStream = new MemoryStream();
            memStream.Write(b, 0, b.Length);

            return Image.FromStream(memStream);
        }

        private void btnKeyboard1_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "1";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboard2_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "2";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboard4_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "4";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboard5_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "5";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboard6_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "6";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboard7_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "7";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboard8_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "8";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboard9_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "9";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnKeyboard0_Click(object sender, EventArgs e)
        {
            txtSearchCode.Text += "0";

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }
        
        private void btnReviewToStart_Click(object sender, EventArgs e)
        {
            // Review To Start 화면 전환.
            ReviewToStartTimer.Stop();
            ProcessReviewToStart();
        }

        private void ProcessReviewToStart()
        {
            //pn_Start
            Control ctrlTempScreen;
            pnSelectPayment.Visible = false;
            pn_Start.Visible = true;
            ctrlTempScreen = pnSelectPayment;
            ctrlOnScreen = pn_Start;
            ctrlOffScreen = pnReview;

            ctrlOnScreen.BringToFront();
            ctrlOffScreen.SendToBack();

            Transition t = new Transition(new TransitionType_EaseInEaseOut(300));

            t.add(ctrlTempScreen, "Left", GROUP_BOX_LEFT);
            t.add(ctrlOnScreen, "Left", 0);
            t.add(ctrlOffScreen, "Left", GROUP_BOX_LEFT);
            t.run();

            st_ProcessStatus.Visible = false;
            tbStep1Name.Visible = false;
            tbStep2Name.Visible = false;
            tbStep3Name.Visible = false;
            tbStep4Name.Visible = false;
            txtInvNo.Visible = false;

            gbHelp.Visible = false;

            if(g_HelpModeOn == true)
            {
                g_HelpModeOn = false;
                lbHelpMode.Visible = false;
                lbHelpMode2.Visible = false;
                lbHelpMode3.Visible = false;
                lbHelpMode4.Visible = false;
                lbHelpMode5.Visible = false;

                btnHelp.Visible = true;
                btnHelp.BringToFront();
                btnHelp.Enabled = true;
                
                btnManualETCKey.Visible = false;
                btnManualETCKey.SendToBack();
                btnManualETCKey.Enabled = true;
            }
            
            //New Invoice No
            GetNewInvNo();

            // OPEN/CLOSE 버튼 활성화.
            //swbOpenClose.Visible = true;

            // 음성 실행.
            GssWelcome.SelectVoice(GstrVoice);
            GssWelcome.SpeakAsync("Welcome to H Mart.");

            // Light ON
            ProcessQLightControl("a0");     // all Light Off
            ProcessQLightControl("g1");     // Green Light On

            if(GintLocation == 1)       //벤쿠버 인 경우
            {
                if (OPOSScanner.DeviceEnabled == false)
                {
                    _EnableScannerDevice();
                }
                //// 저울에서 스캔 되는거 방지.
                //if (OPOSScanner.DeviceEnabled)
                //{
                //    OPOSScanner.DataEvent -= ScannerDataEvent;
                //    OPOSScanner.DeviceEnabled = false;
                //}
            }
            else if(GintLocation == 2)      // 토론토 인경우
            {
                if (OPOSScanner.DeviceEnabled == false)
                {
                    _EnableScannerDevice();
                }
            }
            else                            // 미국 인 경우
            {
                if (OPOSScanner.DeviceEnabled == false)
                {
                    _EnableScannerDevice();
                }
                //// 저울에서 스캔 되는거 방지.
                //if (OPOSScanner.DeviceEnabled)
                //{
                //    OPOSScanner.DataEvent -= ScannerDataEvent;
                //    OPOSScanner.DeviceEnabled = false;
                //}
            }

            // Adult Check initialize
            g_CertifiedAdult = false;
            g_CertifiedAdultReady = false;
            g_iAdultLimit = 0;
            g_iStoredAdultLimit = 0;
            g_strAgeCheckforProdID = string.Empty;

            // Printer Status Check Timer Start
            if(c_poscominfo.si_sPrinter1 == "CUSTOM K80")
            {
                PrintStatusCheckTimer.Interval = g_iPrinterStatusInterval;
                PrintStatusCheckTimer.Start();
            }
            else
            {
                // Update Timer Start.
                UpdateCheckTimer.Interval = g_iUpdateInterval;
                UpdateCheckTimer.Start();
            }
        }
        private void UpdateCheckTimer_Tick(object sender, EventArgs e)
        {
            UpdateCheckTimer.Stop();
            // Process Check Update.
            ProcessUpdateCheck();
        }

        private void ReviewToStartTimer_Tick(object sender, EventArgs e)
        {
            ReviewToStartTimer.Stop();
            ProcessReviewToStart();
        }
        public void ProcessUpdateCheck()
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string sQBuff = string.Empty;
            long lReturn = 0;
            string strINIUpdateTime = string.Empty;
            string strINIPgmVersion = string.Empty;

            // Update Time File 과 LocalDB의 Time Stamp와 비교.
            strINIUpdateTime = this.getIni("UPDATE", "Modified Time", "", g_strUpdateCheckFolder + "\\UpdateTime.ini");
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
                sQBuff = "SELECT st_timestamp, st_pgmVersion " +
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
                        g_strServerUpdateTime = Convert.ToString(c_localdb.rs.Fields["st_timestamp"].Value);
                        g_strServerPgmVersion = Convert.ToString(c_localdb.rs.Fields["st_pgmVersion"].Value);
                    }
                    else
                    {
                        DisplayErrorMessageBox("Update Check", "Update File Check Failed.", 1, sMethod);

                        g_sMessage = string.Format("[{0}] Please check local DB Timestamp. (error code: {1}).", sMethod, g_strServerUpdateTime);
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

            // 비교
            if(strINIUpdateTime != g_strServerUpdateTime)
            {
                // Update 화면 전환.
                // 화면 전환
                ctrTemp = ctrlOnScreen;
                gbUpdate.BringToFront();
                cPUpdate.IsRunning = true;
                btnStart.Enabled = false;
                transitionSiglePage(gbUpdate, 318, 100);

                // 상태 표시등 변경.
                // Light ON
                ProcessQLightControl("a0");     // All Light Off
                ProcessQLightControl("o1");     // Orange Light On

                // Update Process
                bgw_ProcessUpdate.RunWorkerAsync();
            }
            else
            {
                if(strINIPgmVersion != g_strServerPgmVersion)
                {
                    // Version Update logic
                    Process.Start(Application.StartupPath + "\\HanaSales_SelfCheckOut_Startup.exe");
                }
                else
                {
                    // 같으면 Timer Start.
                    UpdateCheckTimer.Interval = g_iUpdateInterval;
                    UpdateCheckTimer.Start();
                }
            }
        }

        public void ProcessUpdateImage()
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            bool bChangeFolerNameStatusDone = false;
            bool bCopyFolderStatusDone = false;
            bool bFolderDeleteStatusDone = false;

            string strServerImageFolder = string.Empty;
            string strLocalImageFolder = string.Empty;
            string strOriginalImageFolder = string.Empty;

            // Market GB 정보에 있는 서버로 접속하기.
            strServerImageFolder = @"\\" + c_poscominfo.si_StationIPGroup + c_poscominfo.si_StationDBSvr + "\\hanaro\\SelfCheckImage";            // 각 매장 서버 경로로 바꿔야 함.
            strOriginalImageFolder = Application.StartupPath + "\\Image";          //Original Image Folder
            strLocalImageFolder = Application.StartupPath + "\\TempImage";          //임시폴더에 카피

            g_sMessage = string.Format("[{0}] Image Update Server Folder : {1}", sMethod, strServerImageFolder);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            g_sMessage = string.Format("[{0}] Image Update Original Folder : {1}", sMethod, strOriginalImageFolder);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            g_sMessage = string.Format("[{0}] Image Update Local Folder : {1}", sMethod, strLocalImageFolder);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            bCopyFolderStatusDone = CopyFolder(strServerImageFolder, strLocalImageFolder);                  // 폴더 카피.

            if (bCopyFolderStatusDone == false)
            {
                // Error Message
                g_sMessage = string.Format("[{0}] Image Update Error : {1} -> {2}", sMethod, strServerImageFolder, strLocalImageFolder);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                return;
            }

            // Original Image Folder Name Change.
            bChangeFolerNameStatusDone = FolderNameChange(strOriginalImageFolder, strOriginalImageFolder +"_old");
            if (bChangeFolerNameStatusDone == false)
            {
                // Error Message
                g_sMessage = string.Format("[{0}] Image Update Error (FolderNameChange) : {1} -> {2}", sMethod, strOriginalImageFolder, strOriginalImageFolder + "_old");
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                return;
            }
            // Temp Image Folder -> Image Folder Name Change
            bChangeFolerNameStatusDone = FolderNameChange(strLocalImageFolder, strOriginalImageFolder);

            if (bChangeFolerNameStatusDone == false)
            {
                // Error Message
                g_sMessage = string.Format("[{0}] Image Update Error (FolderNameChange) : {1} -> {2}", sMethod, strLocalImageFolder, strOriginalImageFolder);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                return;
            }
            // Image_old Folder Delete
            bFolderDeleteStatusDone = FolderDelete(strOriginalImageFolder + "_old");
            if (bFolderDeleteStatusDone == false)
            {
                // Error Message
                g_sMessage = string.Format("[{0}] Image Update Error (Folder DELETE) : {1}", sMethod, strOriginalImageFolder + "_old");
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                return;
            }

            bgw_ProcessUpdate.CancelAsync();
        }

        public bool CopyFile(string strOriginFile, string strCopyFile)
        {
            FileInfo fi = new FileInfo(strOriginFile);
            long lSize = 0;
            long lTotalSize = fi.Length;

            byte[] bBuf = new byte[1024];

            // 동일파일이 존재하면 삭제하고 다시 
            if(File.Exists(strCopyFile))
            {
                File.Delete(strCopyFile);
            }

            //원본 파일 열기
            FileStream fsIn = new FileStream(strOriginFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            //대상 파일 열기
            FileStream fsOut = new FileStream(strCopyFile, FileMode.Create, FileAccess.Write);

            while(lSize < lTotalSize)
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
                    if(File.Exists(strCopyFile))
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

        public bool CopyFolder(string strOriginFolder, string strCopyFolder)
        {
            // 폴더가 없으면 만듬.
            if(!Directory.Exists(strCopyFolder))
            {
                Directory.CreateDirectory(strCopyFolder);
            }
            // 파일 목록 불러오기
            string[] files = Directory.GetFiles(strOriginFolder);
            // 폴더 목록 불러오기
            string[] folders = Directory.GetDirectories(strOriginFolder);

            foreach(string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(strCopyFolder, name);
                //파일 복사 부분
                CopyFile(file, dest);
            }

            // foreach 안에서 재귀함수를 통해서 폴더 복사 및 파일 복사 진행완료.
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(strCopyFolder, name);
                CopyFolder(folder, dest);
            }
            
            return true;
        }
        
        public static bool FolderDelete(string path)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);

                FileInfo[] files = dir.GetFiles("*.*", SearchOption.AllDirectories);

                foreach (FileInfo file in files)

                    file.Attributes = FileAttributes.Normal;

                Directory.Delete(path, true);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool FolderNameChange(string path, string changepath)             // path : TempImage Folder, changepath : Image Folder
        {
            try
            {
                DirectoryInfo Dir = new DirectoryInfo(path);
                DirectoryInfo CDir = new DirectoryInfo(changepath);
                if (CDir.Exists)
                {
                    FolderDelete(changepath);
                }
                if (Dir.Exists)
                {
                    Dir.MoveTo(changepath);
                    Dir = new DirectoryInfo(changepath);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void bgw_ProcessUpdate_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(500);
            ProcessUpdateImage();
        }

        private void bgw_ProcessUpdate_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // 화면 전환.
            cPUpdate.IsRunning = false;
            transitionSiglePage(gbUpdate, 1023, 200);
            ctrlOnScreen = ctrTemp;
            btnStart.Enabled = true;

            // 경광봉 로직 추가 필요. 화면에 따라 등 색깔 변경.
            // Light ON
            if (ctrlOnScreen == pn_Start)
            {
                ProcessQLightControl("a0");     // all Light Off
                ProcessQLightControl("g1");     // Green Light On
            }
            else
            {
                ProcessQLightControl("a0");     // all Light Off
                ProcessQLightControl("r1");     // Red Light On
            }
            
            // INI 파일에 Time Stamp 기록 업데이트 한다.
            this.setIni("UPDATE", "Modified Time", g_strServerUpdateTime, g_strUpdateCheckFolder + "\\UpdateTime.ini");

            //Program Version Check
            string strINIPgmVersion = string.Empty;
            // Program Version File 과 LocalDB의 Pgversion와 비교.
            strINIPgmVersion = this.getIni("UPDATE", "Program Version", "", g_strUpdateCheckFolder + "\\UpdateTime.ini");

            if(strINIPgmVersion != g_strServerPgmVersion)
            {
                // Version Update logic
                Process.Start(Application.StartupPath + "\\HanaSales_SelfCheckOut_Startup.exe");
            }
            else
            {
                UpdateCheckTimer.Interval = g_iUpdateInterval;
                UpdateCheckTimer.Start();
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            g_sMessage = string.Format("[{0}] Push Next Button (ctlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();

            if (ctrlOnScreen == pnItemScanSearchBtn || ctrlOnScreen == pn_Start || ctrlOnScreen == pn_ItemScan || ctrlOnScreen == gbHelp 
                || ctrlOnScreen == pnSelectPayment || ctrlOnScreen == gbScanPointCard || ctrlOnScreen == gbSearchBox || ctrlOnScreen == gbBackToStartExtension || ctrlOnScreen == gbMessageBox
                || ctrlOnScreen == pn_ManualETCKey || ctrlOnScreen == gbAgeCheck)
            {
                ctrTemp = ctrlOnScreen;

                btnSearch.Enabled = false;
                btnSearch_Category.Enabled = false;
                btnHelp.Enabled = false;
                btnNext.Enabled = false;
                btnBack.Enabled = false;
                
                btnItemCorrect.Enabled = false;
                btnVoid.Enabled = false;
                btnItemDiscount.Enabled = false;
                btnReprint.Enabled = false;
                btnSuspend.Enabled = false;
                btnManualETCKey.Enabled = false;

                transitionSiglePage(gbMessageBox, 240, 200);

                // 음성 실행.
                GssMessageBox.SelectVoice(GstrVoice);
                GssMessageBox.SpeakAsync("Do you finish Scanning all your items?");
            }
            else if (ctrlOnScreen == pnAddBag || ctrlOnScreen == gbHelp)
            {
                pnSelectPayment.Visible = true;
                btnSelectCreditCard.Enabled = true;
                btnSelectPointCard.Enabled = true;
                btnSelectGiftCard.Enabled = true;

                //if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                            // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
                btnEBT.Enabled = true;

                transitionDoublePage(pnSelectPayment, pnAddBag, 584, 1 * GROUP_BOX_LEFT, 300);

                tbStep2Name.ForeColor = System.Drawing.Color.Silver;
                tbStep3Name.ForeColor = System.Drawing.Color.DimGray;
                st_ProcessStatus.CurrentStep = 3;
                //btnNext.Text = "PAYMENT";
                //btnNext.Font = new System.Drawing.Font("Helvetica85-Heavy", 15F, FontStyle.Bold);

                btnNext.Enabled = false;
                gbHelp.Visible = false;

                btnBack.Image = global::HanaSales_SelfCheckOut.Properties.Resources.new_Back;

                // 음성 실행.
                GssSelectPayment.SelectVoice(GstrVoice);
                GssSelectPayment.SpeakAsync("Select your Payment Type.");
            }
        }
        private void GetMembershipPrefix()
        {
            string sQBuff = string.Empty;

            long lReturn = 0;
            int iIndex = 0;

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

                sQBuff = "SELECT cc_code " +
                           "FROM hanamart.dbo.tb_comcodes " +
                          "WHERE cc_category = 1 " +
                            "AND cc_area = '" + GintLocation + "' " +
                            "AND cc_active = 'Y'";

                lReturn = c_localdb.RsOpen(sQBuff);

                if (lReturn == 1)
                {
                    c_poscominfo.saMemberPrefix = new string[c_localdb.record_count];

                    while (!c_localdb.rs.EOF)
                    {
                        c_poscominfo.saMemberPrefix[iIndex] = Convert.ToString(c_localdb.rs.Fields["cc_code"].Value);

                        iIndex++;
                        c_localdb.rs.MoveNext();

                    }

                    c_localdb.RsClose();
                }
                else
                {
                    g_sMessage = string.Format("[{0}] Membership card number prefix data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
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
        }

        private void GetVIPMembership()
        {
            string sQBuff = string.Empty;

            long lReturn = 0;
            int iIndex = 0;

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

                sQBuff = "SELECT cc_code " +
                           "FROM hanamart.dbo.tb_comcodes " +
                          "WHERE cc_category = 2 " +
                            "AND cc_area = '" + GintLocation + "' " +
                            "AND cc_mkno = '" + c_poscominfo.ci_mkno + "' " +
                            "AND cc_active = 'Y'";

                lReturn = c_localdb.RsOpen(sQBuff);

                if (lReturn == 1)
                {
                    c_poscominfo.saVIPMembership = new string[c_localdb.record_count];

                    while (!c_localdb.rs.EOF)
                    {
                        c_poscominfo.saVIPMembership[iIndex] = Convert.ToString(c_localdb.rs.Fields["cc_code"].Value);

                        iIndex++;
                        c_localdb.rs.MoveNext();
                    }

                    c_localdb.RsClose();
                }
                else
                {
                    g_sMessage = string.Format("[{0}] Membership card number prefix data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
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
        }
        private void ProcessCardPayment()
        {
            bool blCardApproved = false;
            string strPaid = txtNumCS.Text;                   // Self Check에선 부분 결제되는 거 방지 차원. Glen.
            string strTRcode = "";
            double dblPaid = 0;
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            //string strTotVal = lbTotalValCS.Text;
            string strTotVal = lbSelectPaymentBalanceNum.Text;
            //string strTotVal = lblPayBalance.Text;
            

            //if (GPinPadReady == false)
            //{
            //    DisplayErrorMessageBox("PIN Pad", "Pin Pad connection failed.", 1, sMethod);
            //    KeyInReady();
            //    return;
            //}

            //if (strTotVal.Length > 6)
            //{
            //    DisplayErrorMessageBox("PIN Pad", "Please input the pay amount less than $9,999.", 1, sMethod);
            //    KeyInReady();
            //    return;
            //}

            //if (strTotVal == "0")
            //{
            //    DisplayErrorMessageBox("PIN Pad", "Please input the correct pay amount.", 1, sMethod);
            //    KeyInReady();
            //    return;
            //}

            if (strPaid == "")
            {
                strPaid = lblPayBalance.Text;
                //strPaid = lbSelectPaymentBalanceNum.Text;
                dblPaid = Convert.ToDouble(strPaid);
            }
            if (dblPaid == 0)
            {
                DisplayErrorMessageBox("PIN Pad", "Payment Incomplete.", 1, sMethod);
                GPayFinish = false;

                transitionSiglePage(gbProcessCreditCard, 1024, 200);

                //Timer Start
                BackToStartTimerFromItemScan.Stop();
                BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
                BackToStartTimerFromItemScan.Start();
                return;
            }
            //else
            //{
            //    dblPaid = Convert.ToDouble(strPaid) / 100;

            //}

            if (dblPaid > 0)
            {
                strTRcode = "01";
            }
            else if (dblPaid < 0)
            {
                strTRcode = "04";
            }

            g_sMessage = string.Format("[{0}] Pinpad Send Code : {1}, TotalVal : {2})", sMethod, strTRcode, strTotVal);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            // Card 결재 창 출력 프로세스 표시
            blCardApproved = pinpadprocess(strTRcode, strTotVal);
            //blCardApproved = true;      // 임시
           
            GintAuthSeq += 1;

            if (blCardApproved == true)
            {
                //MessageBox.Show("Card payment approved.");

                // 아래 Logic 추가 필요
                // If wCardTranType = 6 And wgGiftTranCode >= 5 Then   'Gift Card Issue or Reload에서 돌아왔을때

                calcPaymentTotal();
                //lblPayBalance.Text = "0.0"; // 임시

                if (Convert.ToDouble(lblPayBalance.Text) <= 0)  // 
                //if (Convert.ToDouble(lbSelectPaymentBalanceNum.Text) <= 0)  // Pay할 금액이 남아 있는 경우 Pay 계속 진행
                {
                    //if (strPaid != "" && strTotVal != "0.00")
                    //{
                    //    dblBalance = (Convert.ToDouble(strPaid) / 100) - (Convert.ToDouble(strTotVal));
                    //    lblPayBalance.Text = c_poscomlibs.getDoubleFormat(dblBalance);
                    //}
                    GPayFinish = true;

                    calcPaymentTotal(); // 결제 내역 모두 합산한 뒤 완료되었으면 Transaction 종료 처리
                }
                else
                {
                    DisplayErrorMessageBox("PIN Pad", "Payment Incomplete.", 1, sMethod);

                    GPayFinish = false;

                    transitionSiglePage(gbProcessCreditCard, 1024, 200);
                    //ctrlOnScreen = ctrTemp;

                    ctrlOnScreen = pnSelectPayment;     // 강제로 컨트롤 화면 설정함.

                    btnSelectCreditCard.Enabled = true;

                    if (Convert.ToDouble(lblPayHmoney.Text) > 0) { btnSelectPointCard.Enabled = false; pbSelectpaymentPointCardApplied.Visible = true; }
                    else { btnSelectPointCard.Enabled = true; pbSelectpaymentPointCardApplied.Visible = false; }

                    //if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                                // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
                    //{
                        if (Convert.ToDouble(lblPayEBT.Text) > 0) { btnEBT.Enabled = false; pbSelectpaymentEBTApplied.Visible = true; }
                        else { btnEBT.Enabled = true; pbSelectpaymentEBTApplied.Visible = false; }
                    //}
                    
                    btnSelectGiftCard.Enabled = true;
                    btnHelp.Enabled = true;

                    //Timer Start
                    BackToStartTimerFromItemScan.Stop();
                    BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
                    BackToStartTimerFromItemScan.Start();
                }
            }
            else
            {
                DisplayErrorMessageBox("PIN Pad", "Payment Incomplete.", 1, sMethod);
                GPayFinish = false;

                transitionSiglePage(gbProcessCreditCard, 1024, 200);

                //Timer Start
                BackToStartTimerFromItemScan.Stop();
                BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
                BackToStartTimerFromItemScan.Start();

                //ctrlOnScreen = ctrTemp;

                //ctrlOnScreen = pnSelectPayment;     // 강제로 컨트롤 화면 설정함.
                //btnSelectCreditCard.Enabled = true;
                //btnSelectGiftCard.Enabled = true;
                //btnSelectPointCard.Enabled = true;
                //btnSelectCash.Enabled = true;
                //btnHelp.Enabled = true;
            }
        }

        private void ProcessEBTPayment()
        {
            bool blCardApproved = false;
            string strPaid = txtNumCS.Text;                   // Self Check에선 부분 결제되는 거 방지 차원. Glen.
            string strTRcode = "";
            double dblPaid = 0;
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string strTotVal = lbSelectPaymentBalanceNum.Text;

            if (strPaid == "")
            {
                //strPaid = lbSelectPaymentBalanceNum.Text;
                //dblPaid = Convert.ToDouble(strPaid);

                strPaid = lblPayBalance.Text;
                //strPaid = lbSelectPaymentBalanceNum.Text;
                dblPaid = Convert.ToDouble(strPaid);
            }

            if (dblPaid == 0)
            {
                DisplayErrorMessageBox("PIN Pad", "Payment Incomplete.", 1, sMethod);
                GPayFinish = false;

                transitionSiglePage(gbProcessEBT, 1024, 200);
                return;
            }

            //else
            //{
            //    dblPaid = Convert.ToDouble(strPaid) / 100;

            //}

            if (dblPaid > 0)
            {
                strTRcode = "01";
            }
            else if (dblPaid < 0)
            {
                strTRcode = "04";
            }

            g_sMessage = string.Format("[{0}] Pinpad EBT Send Code : 60, Totalval: {1}", sMethod, strTotVal);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            // EBT 검증용 Pre Swipe
            blCardApproved = pinpadprocess("60", strTotVal);

            // Card 결재 창 출력 프로세스 표시
            //blCardApproved = pinpadprocess(strTRcode, strTotVal);
            GintAuthSeq += 1;

            if (blCardApproved == true)
            {
 
                calcPaymentTotal();
                if (Convert.ToDouble(lblPayBalance.Text) <= 0)
                    //if (Convert.ToDouble(lbSelectPaymentBalanceNum.Text) <= 0)  // Pay할 금액이 남아 있는 경우 Pay 계속 진행
                {
                    GPayFinish = true;

                    calcPaymentTotal(); // 결제 내역 모두 합산한 뒤 완료되었으면 Transaction 종료 처리
                }
                else
                {
                    //DisplayErrorMessageBox("PIN Pad", "Payment Incomplete.", 1, sMethod);
                    DisplayErrorMessageBox("Payment", "PAYMENT INCOMPLETE. \n Please Select another Payment Type.", 1, sMethod);
                    GPayFinish = false;
                    transitionSiglePage(gbProcessEBT, 1023, 200);
                
                    ctrlOnScreen = pnSelectPayment;     // 강제로 컨트롤 화면 설정함.

                    //btnSelectCreditCard.Enabled = true;
                    //btnSelectPointCard.Enabled = true;
                    //btnSelectGiftCard.Enabled = true;
                    //btnEBT.Enabled = true;
                    //btnHelp.Enabled = true;
                }

            }
            else
            {
                //DisplayErrorMessageBox("PIN Pad", "Payment Incomplete.", 1, sMethod);
                DisplayErrorMessageBox("Payment", "PAYMENT INCOMPLETE. \n Please Select another Payment Type.", 1, sMethod);
                GPayFinish = false;
                transitionSiglePage(gbProcessEBT, 1023, 200);

                ctrlOnScreen = pnSelectPayment;     // 강제로 컨트롤 화면 설정함.

                // Cancel Pre-swipe
                string strCmdText;
                strCmdText = "/C " + Application.StartupPath + "\\CancelPreSwipe.cmd -i " + txtInvNo.Text;
                Process.Start("CMD.exe", strCmdText);

                if (File.Exists(Application.StartupPath + "\\TK" + txtInvNo.Text + ".dat"))
                {
                    File.Delete(Application.StartupPath + "\\TK" + txtInvNo.Text + ".dat");
                }


                //transitionSiglePage(gbProcessCreditCard, 1023, 200);

                //ctrlOnScreen = ctrTemp;

                //ctrlOnScreen = pnSelectPayment;     // 강제로 컨트롤 화면 설정함.
                //btnSelectCreditCard.Enabled = true;
                //btnSelectGiftCard.Enabled = true;
                //btnSelectPointCard.Enabled = true;
                //btnSelectCash.Enabled = true;
                //btnHelp.Enabled = true;
            }
        }

        private bool pinpadprocess(string pTRcode, string pAmount)
        {
            bool ret = false;
            string strMessage = "";
            string strwMessage1 = "";
            string strwMessage2 = "";
            string strwTK = "";
            string strAccountType = "";
            string strwApprovalCd = "";
            string strwDisplayMsg = "";
            //double payAmount = Convert.ToDouble(pAmount == "" ? "0" : pAmount) / 100;
            bool blEbtTrans = false;
            double payAmount = Convert.ToDouble(pAmount == "" ? "0" : pAmount);

            clsHanaMiraX HanaMiraX = new clsHanaMiraX();

            string sQBuff = string.Empty;
            long lReturn = 0;
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            
            if (GblTestMode == true)
            {
                HanaMiraX.StationID = "EIGEN";
            }
            else
            {
                //HanaMiraX.StationID = g_strCounterNum;
                HanaMiraX.StationID = c_poscominfo.si_PinpadStationID;

                g_sMessage = string.Format("[{0}] HanaMiraX.StationID : {1}", sMethod, HanaMiraX.StationID);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            }
            HanaMiraX.Invoice_Num = txtInvNo.Text;

            //if (pTRcode == "01")
            //{
            //    string strPurchase = HanaMiraX.fncMakePurchase(payAmount.ToString());
            //}

            if (pTRcode == "01")
            {
                string tmpAmount = "";
                tmpAmount = c_poscomlibs.getDoubleFormat(payAmount);
                string strPurchase = HanaMiraX.fncMakePurchase(tmpAmount);
            }
            // EBT
            else if (pTRcode == "60")
            {
                string strPurchase = HanaMiraX.fncInitPreSwipe("");
                double dblEBTpayTotal = 0;
                double dblEBTTaxExRate = 0;

                if (HanaMiraX.ActionCode == "A")
                //if (HanaMiraX.ActionCode == "")
                {
                    strwTK = HanaMiraX.TK;

                    // 추가 => file 생성해서 strTK 기록. HanaMiraX에서 fncReadDatFile에서 처리됨 - "\TK" + txtInvNo.Text + ".dat"
                    FileStream fs = new FileStream(Application.StartupPath + "\\TK" + txtInvNo.Text + ".dat", FileMode.Append, FileAccess.Write, FileShare.Write);
                    fs.Close();
                    StreamWriter sw = new StreamWriter(Application.StartupPath + "\\TK" + txtInvNo.Text + ".dat", true, Encoding.ASCII);
                    sw.Write(strwTK);
                    sw.Close();

                    strAccountType = HanaMiraX.AccountType;

                    if (strAccountType == "FoodStamp") // Foodstamp인 경우만 Tax Exemption 적용
                    {
                        /* 기존 수식 참조
                            //Taxable/Non-Taxable Amount Rate: Total Taxable Payable (Tax 포함 금액) / Total EBT Payable  (Tax 포함 금액)
                            wgFoodStampTaxableAmtRate = Format((wgFoodStampTaxableItemAmt + wgFoodStampTax) / (wgFoodStampAmt + wgFoodStampTax), "#0.00")
                
                            //Total EBT Paid Amount에 대한 각 Taxable/Non-Taxable 별 결재된 금액 산출: Total Paid Amount x Taxable Amount Rate
                            wgFoodStampTaxableEBTPaidAmt = Format(wgFoodStampPaidAmt * wgFoodStampTaxableAmtRate, "#0.00")             'Format(wgFoodStampPaidAmt / (wgFoodStampAmt - wgFoodStampTax), "#0.00")
                            wgFoodStampNonTaxableEBTPaidAmt = Format(wgFoodStampPaidAmt * (1 - wgFoodStampTaxableAmtRate), "#0.00")           'Format(wgFoodStampPaidAmt / (wgFoodStampAmt - wgFoodStampTax), "#0.00")
                
                            //Payment Rate
                            wTotDCRate = wgFoodStampTaxableEBTPaidAmt / IIf(wgFoodStampTaxableItemAmt + wgFoodStampTax = 0, 1, wgFoodStampTaxableItemAmt + wgFoodStampTax)       'Format(wgFoodStampPaidAmt / (wgFoodStampAmt - wgFoodStampTax), "#0.00")
                            wTotNonTxRate = wgFoodStampNonTaxableEBTPaidAmt / IIf(wgFoodStampAmt - wgFoodStampTaxableItemAmt = 0, 1, wgFoodStampAmt - wgFoodStampTaxableItemAmt)       'Format(wgFoodStampPaidAmt / (wgFoodStampAmt - wgFoodStampTax), "#0.00")


                        */
                        
                        /* EBT Tax Exemption 계산 */
                        dblEBTpayTotal = GdblEBTAmountTotal + GdblEBTTax1Total + GdblEBTTax2Total + GdblEBTTax3Total;
                        //dblEBTTaxExRate = dblEBTpayTotal / (GdblEBTAmountTotal + GdblEBTTax1Total); // 수식 검증 필요
                        dblEBTTaxExRate = dblEBTpayTotal / (payAmount); // 수식 검증 필요
                        //dblEBTTaxExRate = 100; // 임시 할당

                        // 1. 아이템 스캔 시 합산된 Foodstamp 밸런스에서 이미 EBT로 결제된 금액이 있으면 그 금액을 빼줌
                        if (lblPayEBT.Text != "0.00")
                        {
                            // 이미 결제된 EBT 금액 빼줄 때 + / - 값 적용되는 것 신경써서 검토 필요
                            payAmount = dblEBTpayTotal - Convert.ToDouble(lblPayEBT.Text);
                        }

                        // 2. EBT 합산 밸런스와 전체 밸런스 비교하여 전체 밸런스가 같거나 큰 경우 카드 결제 대상은 EBT밸런스. 전체 밸런스가 작은 경우는 전체 밸런스가 카드 결제 대상 금액.
                        if (payAmount >= dblEBTpayTotal)
                        {
                            payAmount = dblEBTpayTotal;
                        }

                        payAmount = Math.Round(payAmount, 2);
                        
                        // 3. EBT 밸런스와 카드 결제금액 비교. EBT 밸런스가 크거나 같을 때
                        //  - 카드 결제 금액이 0인 경우 => EBT 대상 아이템이 없는 경우. 메시지 띄우고 Exit
                        if (payAmount == 0)
                        {
                            //MessageBox.Show("No EBT Amount is available!");
                            DisplayErrorMessageBox("Payment", "No EBT Amount is available!. \n Please Select another Payment Type.", 1, sMethod);

                            HanaMiraX.fncCancelPreSwipe();                  // Cancel PreSwipe
                           
                            // Cancel Pre-Swipe
                            string strCmdText;
                            strCmdText = "/C " + Application.StartupPath + "\\CancelPreSwipe.cmd -i " + txtInvNo.Text;
                            Process.Start("CMD.exe", strCmdText);

                            if (File.Exists(Application.StartupPath + "\\TK" + txtInvNo.Text + ".dat"))
                            {
                                File.Delete(Application.StartupPath + "\\TK" + txtInvNo.Text + ".dat");
                            }

                            return false;
                        }
                        //  - Else => $xx.xx Available with Foodstamp 메시지 띄우고 결제 계속 진행
                        else
                        {
                            //MessageBox.Show("$" + payAmount.ToString() + " Available with EBT Foodstamp!");
                            DisplayErrorMessageBox("Payment", "$" + payAmount.ToString() + " Available with EBT Foodstamp!", 1, sMethod);
                        }

                        // 4. EBT Reusable Bag Exemption 추가. ProcessFoodStampReusableBagExemption 참조.
                        // - 스캔된 상품 중 "090" 상품이 있는지 확인 후 있으면 해당 금액만큼 minus 시켜주는 아이템 추가
                        int iCnt = 0;

                        lReturn = c_localdb.DBConnection();

                        if (lReturn < 0)
                        {
                            g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                            return false;
                        }

                        sQBuff = "SELECT count(*) as cnt FROM tb_OrderItem WHERE tInvNo = '" + txtInvNo.Text + "' AND tProd = '090' ";
                        lReturn = c_localdb.RsOpen(sQBuff);

                        if (lReturn == 1)
                        {
                            iCnt = Convert.ToInt32(c_localdb.rs.Fields["cnt"].Value);
                        }
                        else
                        {
                            g_sMessage = string.Format("[{0}] EBT Reusable bag Count query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                            c_localdb.DBClose();

                            return false;
                        }
                        c_localdb.RsClose();


                        // 이미 Bag Exemption 받은 경우 중복 실행되지 않도록
                        if (iCnt > 0 && GblEBTReusableBag == false)
                        {
                            int maxseq = 0;
                            sQBuff = "SELECT max(tID) as maxseq FROM tb_OrderItem WHERE tInvNo = '" + txtInvNo.Text + "' ";
                            lReturn = c_localdb.RsOpen(sQBuff);

                            if (lReturn == 1)
                            {
                                if (c_localdb.rs.Fields["maxseq"].Value == null)
                                {
                                    maxseq = 1;
                                }
                                else
                                {
                                    maxseq = Convert.ToInt32(c_localdb.rs.Fields["maxseq"].Value) + 1;
                                }
                            }
                            else
                            {
                                g_sMessage = string.Format("[{0}] EBT Reusable bag Max Seq query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                                c_localdb.DBClose();

                                return false;
                            }
                            c_localdb.RsClose();

                            sQBuff = "SELECT sum(tQty) as tQty, avg(tIUprice) as tIUprice, avg(tOUprice) as tOUprice, sum(tAmt) as tAmt, sum(tGst) as tGst, sum(tPst) as tPst FROM tb_OrderItem " +
                                     " WHERE tInvNo = '" + txtInvNo.Text + "' AND tProd = '090' ";
                            lReturn = c_localdb.RsOpen(sQBuff);

                            string strtQty = "";
                            string tIUprice = "";
                            string tOUprice = "";
                            string tAmt = "";
                            string tGst = "";
                            string tPst = "";

                            if (lReturn == 1)
                            {
                                strtQty = Convert.ToString(c_localdb.rs.Fields["tQty"].Value);
                                tIUprice = Convert.ToString(c_localdb.rs.Fields["tIUprice"].Value);
                                tOUprice = Convert.ToString(c_localdb.rs.Fields["tOUprice"].Value);
                                tAmt = Convert.ToString(c_localdb.rs.Fields["tAmt"].Value);
                                tGst = Convert.ToString(c_localdb.rs.Fields["tGst"].Value);
                                tPst = Convert.ToString(c_localdb.rs.Fields["tPst"].Value);
                            }
                            else
                            {
                                g_sMessage = string.Format("[{0}] EBT Reusable bag product info query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                                c_localdb.DBClose();

                                return false;
                            }
                            c_localdb.RsClose();

                            if (strtQty != "") // Reusable Bag을 구매한 경우에만 데이터 추가
                            {
                                sQBuff = "INSERT INTO hanamart.dbo.tb_orderitem " +
                                          "SELECT '" + txtInvNo.Text + "', " + maxseq + ",'" + DateTime.Now.ToString("yyyy-MM-dd") + "','" + DateTime.Now.ToString("h:mm:ss tt") + "' , '" +
                                                 "2995000000009" + "', '999', '02', " + "ISNULL(prodCat1,0), ISNULL(prodCat2,0), ISNULL(prodCat3,0), ISNULL(prodCat4,0), ISNULL(prodCat5,0), " +
                                                 "-" + strtQty + ", prodUnit, " + tIUprice + ", " + tOUprice + ", 0, " + "100" + ", '', " +
                                                  "-" + tAmt + ", " + "-" + tGst + ", " + tPst + ", " + "1" + ", " +
                                                  "0,0" + ",'0', '" + c_poscominfo.mi_cardno + "','" + c_poscominfo.ui_epno + "', '" + c_poscominfo.si_counternum +
                                                  "','','','',''," +  // UpCode, Special, Free, Supp,
                                                  "'49','',0,'',0,'',0,'' " +
                                           "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.mfPtype ON prodType = pType " +
                                           "WHERE prodId = '090'";

                                if (c_localdb.DBExcute(sQBuff) != 1)
                                {
                                    g_sMessage = string.Format("[{0}] Item insert failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                                    c_localdb.DBClose();

                                    return false;
                                }
                                else
                                {
                                    g_sMessage = string.Format("[{0}] ({1} item added ({2}).", sMethod, "2995000000009", c_poscominfo.ui_epno);
                                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                                }

                                //calcTotalDisplay("0.00", "-" + tAmt, "-" + tGst, "0.00"); // Bag Exemption 받는 금액 반영
                                calcTotalDisplay("-" + tAmt, "-" + tGst, "0.00", "0.00"); // Bag Exemption 받는 금액 반영 순서 수정함.

                                GblEBTReusableBag = true;
                            }
                        }

                        // 5. EBT Tax Exemption 금액 계산. ProcessFoodStampTaxExemption 참조.
                        // - 스캔된 상품 중 Foodstamp Tax Exemption 되는 아이템만 추려내어 해당 Tax 금액만큼 minus 시켜주는 아이템 추가

                        // 이미 Exemption 계산되어 있던 항목이 있는지 검사 후 총 Exemption 금액에서 제외시킴
                        double dblExistEBTGst = 0;

                        sQBuff = "SELECT tGst FROM tb_OrderItem WHERE tInvNo = '" + txtInvNo.Text + "' AND tType = '44' ";
                        lReturn = c_localdb.RsOpen(sQBuff);

                        while (!c_localdb.rs.EOF)
                        {
                            if (c_localdb.rs.Fields["tGst"].Value == null)
                            {
                                dblExistEBTGst = 0;
                            }
                            else
                            {
                                dblExistEBTGst = Convert.ToDouble(c_localdb.rs.Fields["tGst"].Value);
                            }

                            c_localdb.rs.MoveNext();
                        }
                        c_localdb.RsClose();

                        if (GdblEBTTax1Total != 0) // Exenmtion 받을 Tax가 있는 경우에만 추가
                        {
                            GdblEBTTax1Total = Math.Round(GdblEBTTax1Total - dblExistEBTGst,2);

                            // Tax Exemption 항목 추가
                            int maxseq2 = 0;
                            sQBuff = "SELECT max(tID) as maxseq FROM tb_OrderItem WHERE tInvNo = '" + txtInvNo.Text + "' ";
                            lReturn = c_localdb.RsOpen(sQBuff);

                            while (!c_localdb.rs.EOF)
                            {
                                maxseq2 = Convert.ToInt32(c_localdb.rs.Fields["maxseq"].Value) + 1;
                                c_localdb.rs.MoveNext();
                            }

                            c_localdb.RsClose();

                            if (maxseq2 == 0)
                            {
                                maxseq2 = 1;
                            }

                            sQBuff = "INSERT INTO hanamart.dbo.tb_orderitem " +
                                          "SELECT '" + txtInvNo.Text + "', " + maxseq2 + ",'" + DateTime.Now.ToString("yyyy-MM-dd") + "','" + DateTime.Now.ToString("h:mm:ss tt") + "' , '" +
                                                 "2995000000008" + "', '099', '99', " + "0,0,0,0,0, '', " +
                                                 "1" + ", '', " + "0" + ", " + "0" + ", 0, " + dblEBTTaxExRate.ToString() + ", '', '', " +
                                                  "0" + ", " + GdblEBTTax1Total.ToString() + ", " + "0" + ", '0'" + ", '', '0', '" + c_poscominfo.ui_epno + "', '" + c_poscominfo.si_counternum +
                                                 "','','','','','','','',0,'',0,'','','' ";

                            if (GintLocation == 3) // 미국일 경우 테이블 컬럼 구성 다름
                            {
                                sQBuff = "INSERT INTO hanamart.dbo.tb_orderitem " +
                                          "SELECT '" + txtInvNo.Text + "', " + maxseq2 + ",'" + DateTime.Now.ToString("yyyy-MM-dd") + "','" + DateTime.Now.ToString("h:mm:ss tt") + "' , '" +
                                                 "2995000000008" + "', '099', '99', " + "0,0,0,0,0, " +
                                                 "1" + ", '', " + "0" + ", " + "0" + ", 0, " + dblEBTTaxExRate.ToString() + ", '', " +
                                                  "0" + ", " + (GdblEBTTax1Total * -1).ToString() + ", " + "0" +  ", '', " +  // Amt, Tax1, Tax2, Tax
                                                  "0,0" + ",'0', '" + c_poscominfo.mi_cardno + "','" + c_poscominfo.ui_epno + "', '" + c_poscominfo.si_counternum +
                                                 "','','','',''," +  // UpCode, Special, Free, Supp,
                                                 "'44','',0,'',0,'',0,'' ";
                            }

                            if (c_localdb.DBExcute(sQBuff) != 1)
                            {
                                g_sMessage = string.Format("[{0}] EBT Tax Exemption Data insert failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                                c_localdb.DBClose();

                                return false;
                            }
                            else
                            {
                                g_sMessage = string.Format("[{0}] ({1} item added ({2}).", sMethod, "2995000000008", c_poscominfo.ui_epno);
                                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                            }

                            // Loop 돌면서 FoodStamp 대상 상품에 대해 tTxExem과 tTxExemAmt에 값 업데이트
                            sQBuff = "SELECT tID, tAmt, tGst, tTxExem, tTxExemAmt FROM tb_OrderItem WHERE tInvNo = '" + txtInvNo.Text + "' AND tType <> '44' AND tFoodStamp = '1' ";
                            lReturn = c_localdb.RsOpen(sQBuff);

                            double dblItemtAmt = 0;
                            double dblItemtGst = 0;
                            double dblItemTxExem = 0;
                            double dblItemTxExemAmt = 0;

                            while (!c_localdb.rs.EOF)
                            {
                                dblItemtAmt = Math.Round(Convert.ToDouble(c_localdb.rs.Fields["tAmt"].Value),2);
                                dblItemtGst = Math.Round(Convert.ToDouble(c_localdb.rs.Fields["tGst"].Value),2);
                                dblItemTxExem = Math.Round(Convert.ToDouble(c_localdb.rs.Fields["tTxExem"].Value),2);
                                dblItemTxExemAmt = Math.Round(Convert.ToDouble(c_localdb.rs.Fields["tTxExemAmt"].Value),2);

                                sQBuff = "UPDATE tb_OrderItem SET tTxExemAmt = " + dblItemtAmt + ", tTxExem = " + dblItemtGst + " WHERE tInvNo = '" + txtInvNo.Text + "' AND tID = " + c_localdb.rs.Fields["tID"].Value;
                                if (c_localdb.DBExcute(sQBuff) != 1)
                                {
                                    g_sMessage = string.Format("[{0}] EBT Tax Exemption Value update failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                                    c_localdb.DBClose();

                                    return false;
                                }
                                else
                                {
                                    g_sMessage = string.Format("[{0}] ({1} Exemption Value Updated ({2}).", sMethod, "2995000000008", c_poscominfo.ui_epno);
                                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                                }

                                /* 
                                    If !tAmt < 0 Then
                                        If !tTxExem <> 0 Then
                                            // Do nothing   
                                        End If
                                    Else
                                        If (!tGst = 0) Then
                                            !tTxExem = 0
                                            !tTxExemAmt = Format(IIf(wgFoodStampPaidAmt = Val(lblFoodStamp), !tAmt * -1, (!tAmt * wTotNonTxRate) * (-1)), "#0.00")
                                        Else
                                            !tTxExem = Format(IIf(wgFoodStampPaidAmt = Val(lblFoodStamp), !tGst * -1, (!tGst * wPrevTotDCRate) * (-1)), "#0.00")
                                            !tTxExemAmt = Format(IIf(wgFoodStampPaidAmt = Val(lblFoodStamp), !tAmt * -1, (!tAmt * wPrevTotDCRate) * (-1)), "#0.00")
                                        End If
                                    End If
                                */

                                c_localdb.rs.MoveNext();
                            }

                            c_localdb.RsClose();
                        }

                        // 6. Tax 재계산하여 payAmount 금액 결정
                        // 해당 Invno로 재계산하여 Amt, Tax 합산하여 금액 결정
                        double dblRecalcEBTAmt = 0;
                        double dblRecalcEBTTax1 = 0;
                        double dblRecalcEBTTax2 = 0;

                        sQBuff = "SELECT sum(tAmt) as tAmt, sum(tGst) as tGst, sum(tPst) as tPst, sum(tTxExem) as TxExem FROM tb_OrderItem WHERE tInvNo = '" + txtInvNo.Text + "' AND (tFoodStamp = '1' or tType = '44') ";
                        lReturn = c_localdb.RsOpen(sQBuff);

                        if (lReturn == 1)
                        {
                            dblRecalcEBTAmt = Math.Round(Convert.ToDouble(c_localdb.rs.Fields["tAmt"].Value),2);
                            dblRecalcEBTTax1 = Math.Round(Convert.ToDouble(c_localdb.rs.Fields["tGst"].Value), 2);
                            dblRecalcEBTTax2 = Math.Round(Convert.ToDouble(c_localdb.rs.Fields["tPst"].Value), 2);
                            GdblEBTTaxExemptTotal = Math.Round(Convert.ToDouble(c_localdb.rs.Fields["TxExem"].Value), 2);

                            calcTotalDisplay("0.00", (GdblEBTTaxExemptTotal * -1).ToString(), "0.00", "0.00");

                        }
                        else
                        {
                            g_sMessage = string.Format("[{0}] EBT PayAmount ReCalc query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                            c_localdb.DBClose();

                            return false;
                        }
                        c_localdb.RsClose();

                        payAmount = Math.Round(dblRecalcEBTAmt + dblRecalcEBTTax1 + dblRecalcEBTTax2,2);

                    }

                    // 최종 결제 금액으로 결제 진행
                    blEbtTrans = HanaMiraX.fncEBTTrans(payAmount.ToString(), strAccountType); // FoodStamp or CashBenefit

                }
                else
                {
                    return ret;
                }
                
            }
            else if (pTRcode == "95")
            {            
                bool blHandshake;
                //bool blHandshake = true;
                if(HanaMiraX.StationID == "EIGEN")
                {
                    blHandshake = true;
                    g_sMessage = string.Format("[{0}] Pinpad Hand Shaking {1}", sMethod, HanaMiraX.StationID, blHandshake, c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                }
                else
                {
                    blHandshake = HanaMiraX.fncMiraServHandShaking();
                    g_sMessage = string.Format("[{0}] Pinpad Hand Shaking {1}", sMethod, blHandshake, c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                }
                
                if (blHandshake == true)
                {
                    GPinPadReady = true;
                    return false;
                }
                else
                {
                    if (GblTestMode == true)
                    {
                        GPinPadReady = true;
                        return true;
                    }
                    else
                    {
                        GPinPadReady = false;
                    }
                }
            }

            strwMessage1 = HanaMiraX.ShowMessage;
            strwMessage2 = HanaMiraX.MsgDescription;
            strwTK = HanaMiraX.TK;
            strwApprovalCd = HanaMiraX.Approval_CD;

            strwDisplayMsg = HanaMiraX.Display_Msg;

            strMessage = "InvNo: " + HanaMiraX.Invoice_Num + "\n" +
                     "StationID: " + HanaMiraX.StationID + "\n" +
                     "ActionCode: " + HanaMiraX.ActionCode + "\n" +
                     "Receipt_Lang: " + HanaMiraX.Receipt_Lang + "\n" +
                     "Response_Code: " + HanaMiraX.Response_Code + "\n" +
                     "Response_Msg: " + HanaMiraX.Response_Msg + "\n" +
                     "ISOResponse_Code: " + HanaMiraX.ISOResponse_Code + "\n" +
                     "Approval_Cd: " + HanaMiraX.Approval_CD + "\n" +
                     "Amount: " + HanaMiraX.Amount + "\n" +
                     "OperatorMessage: " + HanaMiraX.OperatorMessage + "\n" +
                     "Display_Msg: " + HanaMiraX.Display_Msg + "\n" +
                     "Receipt_Msg_Action: " + HanaMiraX.Receipt_Msg_Action + "\n" +
                     "Receipt_Msg_Account: " + HanaMiraX.Receipt_Msg_Account + "\n" +
                     "ReceiptRefNum: " + HanaMiraX.ReceiptRefNum + "\n" +
                     "Invoice_Num: " + HanaMiraX.Invoice_Num + "\n" +
                     "Echo_Data: " + HanaMiraX.Echo_Data + "\n" +
                     "CardType: " + HanaMiraX.CardType + "\n" +
                     "HostDateTime: " + HanaMiraX.HostDateTime + "\n" +
                     "AccountType: " + HanaMiraX.AccountType + "\n" +
                     "CVV_Response: " + HanaMiraX.CVV_Response + "\n" +
                     "AVS_Response_C: " + HanaMiraX.AVS_Response_C + "\n" +
                     "AVS_Response_M: " + HanaMiraX.AVS_Response_M + "\n";
            strMessage = strMessage +
                     "TK: " + HanaMiraX.TK + "\n" +
                     "SO: " + HanaMiraX.SO + "\n" +
                     "AA: " + HanaMiraX.AmtOfAdj + "\n" +
                     "CR: " + HanaMiraX.AmtOfCrd + "\n" +
                     "AD: " + HanaMiraX.AmtOfDbt + "\n" +
                     "NA: " + HanaMiraX.NumOfAdj + "\n" +
                     "NC: " + HanaMiraX.NumOfCrd + "\n" +
                     "ND: " + HanaMiraX.NumOfDbt + "\n" +
                     "Signature Required: " + HanaMiraX.SignatureRequired + "\n";

            //MessageBox.Show(strMessage);

            g_sMessage = string.Format("[{0}] HanaMiraX Message {1}", sMethod, strMessage);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            
            // EBT
            if (pTRcode == "01") 
            {
                ReceiptReady(HanaMiraX, pTRcode, "S");
            }
            else if (pTRcode == "60")
            {
                //if (blEbtTrans == true)
                //{
                //    ReceiptReady(HanaMiraX, "01", "S");
                ReceiptReady(HanaMiraX, pTRcode, "S");
                //}
            }

            if (HanaMiraX.ActionCode == "A")
            {
                ret = true;
            }
            else
            {
                ret = false;
            }

            return ret;
        }
        private void ReceiptReady(clsHanaMiraX HanaMiraX, string pTranCode, string pSwipeManual)
        {
            bool blApproved = false;
            int wCardTranType = 0;
            int wCreditCardCode = 0;
            int wPaycode = 0;
            string wAccountType = "";
            bool wSignatureRequired = false;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            g_sMessage = string.Format("[{0}] HanaMiraX.ActionCode : {1}, HanaMiraX.CardType : {2}, HanaMiraX.HostDateTime :{3})", sMethod, HanaMiraX.ActionCode, HanaMiraX.CardType, HanaMiraX.HostDateTime);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            switch (HanaMiraX.ActionCode)
            {
                case "A":
                    blApproved = true;
                    break;
                case "D":
                    blApproved = false;
                    break;
                default:
                    blApproved = false;
                    break;
            }

            switch (HanaMiraX.CardType)
            {
                case "I":
                    wCardTranType = 1;
                    break;
                case "V":            //VISA
                    wCardTranType = 2;
                    wCreditCardCode = 0;
                    wAccountType = "4";
                    break;
                case "M":            //MASTER
                    wCardTranType = 2;
                    wCreditCardCode = 1;
                    wAccountType = "4";
                    break;
                case "A":           //AMEX
                    wCardTranType = 2;
                    wCreditCardCode = 2;
                    wAccountType = "4";
                    break;
                case "AX":     //AMEX
                    wCardTranType = 2;
                    wCreditCardCode = 2;
                    wAccountType = "4";
                    break;
                case "D":            //DISCOVER
                    wCardTranType = 2;
                    wCreditCardCode = 3;
                    wAccountType = "4";
                    break;
                case "UP":            //UNION PAY
                    wCardTranType = 2;

                    if(GintLocation != 3)               // 미국이 아닌 경우 ColUnion 컬럼이 있음.
                    {
                        wCreditCardCode = 4;
                    }
                    else                                // 미국인 경우 ColUnion 컬럼없음. OTHER로 빠지게 수정.
                    {
                        wCreditCardCode = 5;
                    }
                    
                    wAccountType = "4";
                    break;
                case "G":           //GIFT CARD
                    wCardTranType = 8;
                    wAccountType = "7";
                    break;
                case "B":           //EBT
                    wCardTranType = 7;
                    wAccountType = "6";
                    break;
                case "E":           //Diners
                    wCardTranType = 2;
                    wCreditCardCode = 9;
                    wAccountType = "4";
                    break;
                default:           //OTHER
                    wCardTranType = 2;
                    wCreditCardCode = 5;
                    wAccountType = "4";
                    break;
            }

            if (wCardTranType == 1)
            {
                wPaycode = 2; // Debit
            }
            else if (wCardTranType == 2)
            {
                wPaycode = 3;
            }
            else if (wCardTranType == 5)
            {
                wPaycode = 9;
            }
            else if (wCardTranType == 7)
            {
                wPaycode = 7;
            }
            else if (wCardTranType == 8)
            {
                wPaycode = 8;
            }
            else
            {
                wPaycode = 9;
            }

            switch (HanaMiraX.SignatureRequired)
            {
                case "":
                    wSignatureRequired = false;
                    break;
                case "N":
                    wSignatureRequired = false;
                    break;
                case "P":
                    wSignatureRequired = false;
                    break;
                default:
                    wSignatureRequired = true;
                    break;
            }

            string sQBuff = string.Empty;
            long lReturn = 0;
            //string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            try
            {
                lReturn = c_localdb.DBConnection();

                if (lReturn < 0)
                {
                    g_sMessage = string.Format("[{0}] Remote database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    return;
                }

                // Add - Table 삭제 
                sQBuff = "Select ISNULL(Max(ct_seq),1) as ctseqmax From tb_CardTrans Where ct_year = '" + DateTime.Now.ToString("yy") + "' ";
                lReturn = c_localdb.RsOpen(sQBuff);

                int iSeq = 0;
                if (lReturn == 1)
                {
                    iSeq = Convert.ToInt32(c_localdb.rs.Fields["ctseqmax"].Value) + 1;
                }
                else
                {
                    iSeq = 1;
                }
                c_localdb.RsClose();

                string strTranDate = c_poscomlibs.Left(HanaMiraX.HostDateTime, 8);
                strTranDate = c_poscomlibs.Left(strTranDate, 4) + "-" + strTranDate.Substring(4, 2) + "-" + strTranDate.Substring(6, 2);

                string strTranTime = c_poscomlibs.Right(HanaMiraX.HostDateTime, 6);
                strTranTime = c_poscomlibs.Left(strTranTime, 2) + ":" + strTranTime.Substring(2, 2) + ":" + strTranTime.Substring(4, 2);

                string strCustomerCopy = HanaMiraX.Cust_Receipt_Msg_Txt.Replace("'", "`");  // Customer Copy에서 French로 들어왔을 경우 작은 따음표 처리.

                // tb_CardTrans - 저장
                //2-1. tb_SoldItem
                sQBuff = "Insert Into tb_CardTrans (ct_year, ct_Seq, ct_InvNo, ct_OrderSeq, ct_Station, ct_TranType, ct_Amount, ct_balance, ct_CardType, ct_Pan, ct_exp," +
                            "ct_Language, ct_RefNum, ct_SwipeManual, ct_AccountType, ct_AuthCode, ct_TranNo, ct_TranDate, ct_TranTime, ct_ActionCode, ct_ResponseCode," +
                            "ct_Response, ct_Track2, ct_MerchantReceipt, ct_CustomerReceipt, ct_Token, ct_upflag)";
                sQBuff += "Values ('" + DateTime.Now.ToString("yy") + "'," + iSeq.ToString() + ",'" + txtInvNo.Text + "'," + GintAuthSeq.ToString() +
                            "," + c_poscominfo.si_counternum + ",'" + pTranCode.Trim() + "'," + HanaMiraX.Amount + "," + "0" + ",'" + HanaMiraX.CardType + "','','',0,'" +
                            HanaMiraX.ReceiptRefNum + "','" + pSwipeManual + "','" + HanaMiraX.AccountType + "','" + HanaMiraX.Approval_CD + "','','" +
                            strTranDate + "','" + strTranTime + "','" + HanaMiraX.ActionCode.Trim() + "','" + HanaMiraX.Response_Code.Trim() + "','" + HanaMiraX.Display_Msg +
                            "','" + HanaMiraX.TK + "','" + HanaMiraX.Receipt_Msg_Txt + "','" + strCustomerCopy + "','" + HanaMiraX.TK + "',1)";

                g_sMessage = string.Format("[{0}] Insert tb_cardTrans SQL Query {1}", sMethod, sQBuff);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                if (c_localdb.DBExcute(sQBuff) != 1)
                {
                    g_sMessage = string.Format("[{0}] tb_CardTrans data insert failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    c_localdb.DBClose();

                    return;
                }
                else
                {
                    if (blApproved == true)
                    {
                        if (wPaycode == 2)
                        {
                            lblPayDebit.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(lblPayCredit.Text) + Convert.ToDouble(HanaMiraX.Amount));
                        }
                        if (wPaycode == 3 || wPaycode == 9)
                        {
                            lblPayCredit.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(lblPayCredit.Text) + Convert.ToDouble(HanaMiraX.Amount));
                            CardPay[wCreditCardCode] = CardPay[wCreditCardCode] + Convert.ToDouble(HanaMiraX.Amount);
                        }
                        if (wPaycode == 7)
                        {
                            lblPayEBT.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(lblPayEBT.Text) + Convert.ToDouble(HanaMiraX.Amount));
                            CardPay[7] = CardPay[7] + Convert.ToDouble(HanaMiraX.Amount);
                        }
                        if (wPaycode == 8)
                        {
                            lblPayGiftCard.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(lblPayGiftCard.Text) + Convert.ToDouble(HanaMiraX.Amount));
                            CardPay[8] = CardPay[8] + Convert.ToDouble(HanaMiraX.Amount);
                        }
                    }

                    PrtCardReceipt(HanaMiraX.Receipt_Msg_Txt);
                    
                    g_sMessage = string.Format("[{0}] (tb_CardTrans data saved ({1}).", sMethod, c_poscominfo.ui_epno);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
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
        }
        private void btnItemCorrect_Click(object sender, EventArgs e)
        {
            string sQBuff = string.Empty;
            //long lReturn = 0;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            g_sMessage = string.Format("[{0}] Push Item Correct Button (ctrlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            
            // Button Control
            btnBack.Enabled = false;
            btnItemCorrect.Enabled = false;
            btnVoid.Enabled = false;
            btnItemDiscount.Enabled = false;
            btnReprint.Enabled = false;
            btnSuspend.Enabled = false;
            btnNext.Enabled = false;
            ItemCSView.Enabled = false;

            btnSearch.Enabled = false;
            btnSearch_Category.Enabled = false;

            btnAddBagToCart.Enabled = false;
            btnNoBag.Enabled = false;
            btnBagPlus.Enabled = false;
            btnMinus.Enabled = false;

            btnSelectCreditCard.Enabled = false;
            btnSelectPointCard.Enabled = false;
            btnSelectGiftCard.Enabled = false;
            btnEBT.Enabled = false;
            btnManualETCKey.Enabled = false;

            if (ItemCSView.Items.Count <= 0 || ItemCSView.SelectedItems.Count <= 0)
            {
                // Error Message
                DisplayErrorMessageBox("Item Correct", "Please Select the Item.", 1, sMethod);
                return;
            }
            // 결제 내역이 있는 경우
            if (lblPayTotal.Text != "0.00")
            {
                DisplayErrorMessageBox("Item Correct", "ITEM CORRECT NOT ALLOWED\n WHEN PAYMENT EXIST", 1, sMethod);
                return;
            }
            
            g_ManagerCardScanReady = true;
            g_mMaterFunctionVal = ManualMasterFunction.ItemCorrect;

            if (c_poscominfo.ci_mkno != "55" && c_poscominfo.ci_mkno != "62")                // 55 오로라, 62 다운타운 매장이 아닌 경우 Manager Key Scan 필요.
            {
                ctrTemp = ctrlOnScreen;
                transitionSiglePage(gbScanManagerCard, 308, 200);
                cpScanManagerCard.IsRunning = true;                
            }
            else                                            // 55 오로라, 62 다운타운 매장인 경우 Manager Key Scan 필요 없음.
            {
                ProcessEntryCode(g_strManagerKey, 0);
            }            
            KeyInReady();            
        }

        public void ProcessItemCorrect()
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string sQBuff = string.Empty;
            long lReturn = 0;

            string strProdId = "", strSeq = "", strType = "";
            int iListViewIndex = 0;
            
            if (ItemCSView.Items.Count > 0 && ItemCSView.SelectedItems.Count > 0)
            {
                iListViewIndex = ItemCSView.FocusedItem.Index;
                strProdId = ItemCSView.SelectedItems[0].Text;
                strSeq = ItemCSView.SelectedItems[0].SubItems[1].Text;
                strType = ItemCSView.SelectedItems[0].SubItems[7].Text;
            }
            //else
            //{
            //    // Button Control
            //    btnBack.Enabled = false;
            //    btnItemCorrect.Enabled = false;
            //    btnVoid.Enabled = false;
            //    btnItemDiscount.Enabled = false;
            //    btnReprint.Enabled = false;
            //    btnSuspend.Enabled = false;
            //    btnNext.Enabled = false;
            //    ItemCSView.Enabled = false;

            //    if (ctrTemp == pn_ItemScan || ctrTemp == pnItemScanSearchBtn)
            //    {
            //        btnSearch.Enabled = false;
            //        btnSearch_Category.Enabled = false;
            //    }
            //    else if (ctrTemp == pnAddBag)
            //    {
            //        btnAddBagToCart.Enabled = false;
            //        btnNoBag.Enabled = false;
            //        btnBagPlus.Enabled = false;
            //        btnMinus.Enabled = false;
            //    }
            //    else if (ctrTemp == pnSelectPayment)
            //    {
            //        btnSelectCreditCard.Enabled = false;
            //        btnSelectPointCard.Enabled = false;
            //        btnSelectGiftCard.Enabled = false;
            //        btnEBT.Enabled = false;
            //    }
            //    // Error Message
            //    DisplayErrorMessageBox("Item Correct", "Please Select the Item.", 1, sMethod);
            //    return;
            //}

            try
            {
                // CRF, Bottle Deposit, EHF 선택인 경우 Pass
                if (strType == "20" || strType == "21" || strType == "22")
                {
                    DisplayErrorMessageBox("Item Correct", "Please Select the Item without Deposit.", 1, sMethod);
                    return;
                }

                // tb_OderItem 에서 삭제하기.
                if (strProdId != "" && strSeq != "")
                {
                    lReturn = c_localdb.DBConnection();

                    if (lReturn < 0)
                    {
                        g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                        return;
                    }


                    // 입력된 상품코드(UPC) 검색하여 존재 여부 확인.
                    sQBuff = "DELETE FROM hanamart.dbo.tb_OrderItem " +
                                   "WHERE tProd = '" + strProdId + "' " +
                                     "AND (tID = " + strSeq + " OR tRelatedID = " + strSeq + ") ";

                    if (c_localdb.DBExcute(sQBuff) != 1)
                    {
                        g_sMessage = string.Format("[{0}] Item correction failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                        c_localdb.DBClose();

                        return;
                    }
                    else
                    {
                        g_sMessage = string.Format("[{0}] ({1} item(s) deleted ({2}).", sMethod, c_localdb.affectedrecords.ToString(), c_poscominfo.ui_epno);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                    }

                    initListView();
                    initSalesTotal();
                    PopulateListViewProdItem("ItemCorrect");

                    if (ItemCSView.Items.Count > 1)
                    {
                        //if (ItemCSView.Items.Count > iListViewIndex)
                        //{
                        //    ItemCSView.Items[iListViewIndex].Selected = true;
                        //    ItemCSView.Items[iListViewIndex].Focused = true;
                        //    ItemCSView.Select();
                        //}
                        //else
                        //{
                        //    ItemCSView.Items[iListViewIndex - 1].Selected = true;
                        //    ItemCSView.Items[iListViewIndex - 1].Focused = true;
                        //    ItemCSView.Select();
                        //}
                    }
                    else if (ItemCSView.Items.Count == 0 || lbTotalValCS.Text == "0.00")
                    {
                        btnNext.Enabled = false;
                    }
                    KeyInReady();

                    ProcessTotalDC();
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
        }
        
        private void btnVoid_Click(object sender, EventArgs e)
        {
            string sQBuff = string.Empty;
            
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            g_sMessage = string.Format("[{0}] Push Void Button (ctrlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            btnBack.Enabled = false;
            btnItemCorrect.Enabled = false;
            btnVoid.Enabled = false;
            btnItemDiscount.Enabled = false;
            btnHelp.Enabled = false;
            btnReprint.Enabled = false;
            btnSuspend.Enabled = false;
            btnSearch.Enabled = false;
            btnSearch_Category.Enabled = false;
            btnNext.Enabled = false;

            btnAddBagToCart.Enabled = false;
            btnNoBag.Enabled = false;
            btnBagPlus.Enabled = false;
            btnMinus.Enabled = false;

            btnSelectCreditCard.Enabled = false;
            btnSelectPointCard.Enabled = false;
            btnSelectGiftCard.Enabled = false;
            btnEBT.Enabled = false;
            btnManualETCKey.Enabled = false;

            // 결제 내역이 있는 경우
            if (lblPayTotal.Text != "0.00")
            {
                DisplayErrorMessageBox("VOID", "VOID NOT ALLOWED\n WHEN PAYMENT EXIST", 1, sMethod);
                return;
            }

            g_ManagerCardScanReady = true;
            g_mMaterFunctionVal = ManualMasterFunction.ItemBtnVoid;
            
            if (c_poscominfo.ci_mkno != "55" && c_poscominfo.ci_mkno != "62")                // 55 오로라, 62 다운타운 매장이 아닌 경우 Manager Key Scan 필요.
            {
                ctrTemp = ctrlOnScreen;
                transitionSiglePage(gbScanManagerCard, 308, 200);
                cpScanManagerCard.IsRunning = true;
            }
            else                                            // 55 오로라, 62 다운타운 매장인 경우 Manager Key Scan 필요 없음.
            {
                ProcessEntryCode(g_strManagerKey, 0);
            }
            
            KeyInReady();

            // Item Void Error Message
            //DisplayErrorMessageBox("VOID", "Warning!! All item list will be deleted.\nAre you sure? ", 2, sMethod);
        }

        private void btnItemDiscount_Click(object sender, EventArgs e)
        {
            string strProdId = string.Empty;
            string strSeq = string.Empty;
            string strType = string.Empty;
            string strSelectedItemName = string.Empty;

            int iListViewIndex = 0;

            string sQBuff = string.Empty;
            long lReturn = 0;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            g_sMessage = string.Format("[{0}] Push Item Discount Button (ctlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            // Button Control
            btnBack.Enabled = false;
            btnItemCorrect.Enabled = false;
            btnVoid.Enabled = false;
            btnItemDiscount.Enabled = false;
            btnReprint.Enabled = false;
            btnSuspend.Enabled = false;
            btnNext.Enabled = false;
            ItemCSView.Enabled = false;
            btnManualETCKey.Enabled = false;

            if (ctrTemp == pn_ItemScan || ctrTemp == pnItemScanSearchBtn)
            {
                btnSearch.Enabled = false;
                btnSearch_Category.Enabled = false;
            }
            else if (ctrTemp == pnAddBag)
            {
                btnAddBagToCart.Enabled = false;
                btnNoBag.Enabled = false;
                btnBagPlus.Enabled = false;
                btnMinus.Enabled = false;
            }
            else if (ctrTemp == pnSelectPayment)
            {
                btnSelectCreditCard.Enabled = false;
                btnSelectPointCard.Enabled = false;
                btnSelectGiftCard.Enabled = false;
                btnEBT.Enabled = false;
            }

            if (ItemCSView.Items.Count > 0 && ItemCSView.SelectedItems.Count > 0)                   // 선택된 아이템이 있는지 체크
            {
                iListViewIndex = ItemCSView.FocusedItem.Index;
                strProdId = ItemCSView.SelectedItems[0].Text;
                strSeq = ItemCSView.SelectedItems[0].SubItems[1].Text;
                strSelectedItemName = ItemCSView.SelectedItems[0].SubItems[2].Text;
                strType = ItemCSView.SelectedItems[0].SubItems[7].Text;
            }
            else
            {
                // Error Message
                DisplayErrorMessageBox("Item Discount", "Please Select the Item.", 1, sMethod);
                return;
            }

            if(strType =="11" || strType =="48")                    //Dicount Item 항목이거나, Discount 이미 된 상품인 경우
            {
                // Error Message
                DisplayErrorMessageBox("Item Discount", "This Item is already dicounted or discount Item.", 1, sMethod);
                return;
            }

            // Discount Barcode Scan
            // Barcode 입력 창 출력
            ctrTemp = ctrlOnScreen;
            ctrTemp2 = ctrlOnScreen;

            cpItemDiscountBarcode.IsRunning = true;
            transitionSiglePage(gbItemDiscount, 284, 200);

            //Discount Barcode Mode On
            g_ItemDiscountModeOn = true;

            txtItemDiscountBarcode.Clear();
            txtItemDiscountBarcode.Focus();

        }
        private void txtItemDiscountBarcode_KeyDown(object sender, KeyEventArgs e)
        {
          string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                if (txtItemDiscountBarcode.Text != "")
                {
                    //g_ItemDiscountModeOn = false;
                    // 바코드에서 DC Rate 가져오기.
                    GdblItemDCRate = Convert.ToDouble(txtItemDiscountBarcode.Text.Substring(txtItemDiscountBarcode.Text.LastIndexOf('.') + 1, 3));          // Rate 파싱에서 넣기.

                    ProcessItemDiscountBarcode();
                }
                else
                {
                    // Error Message
                    DisplayErrorMessageBox("Item Discount", "Please Scan your Item Discount Barcode.", 1, sMethod);

                    txtItemDiscountBarcode.Clear();
                    txtItemDiscountBarcode.Focus();
                }
            }

        }

        private void btnItemDiscountBarcodeExit_Click(object sender, EventArgs e)
        {
            transitionSiglePage(gbItemDiscount, 1023, 200);
            ctrlOnScreen = ctrTemp;
            cpItemDiscountBarcode.IsRunning = false;

            g_ItemDiscountModeOn = false;

            if (ctrTemp != pn_ItemScan && ctrTemp != pnItemScanSearchBtn)
            {
                // 저울에서 스캔 되는거 방지.
                if (OPOSScanner.DeviceEnabled)
                {
                    OPOSScanner.DataEvent -= ScannerDataEvent;
                    OPOSScanner.DeviceEnabled = false;
                }
            }

            ProcessReprintAfterButtonControl();
        }

        private void btnReprint_Click(object sender, EventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            g_sMessage = string.Format("[{0}] Push Reprint Button (ctlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            btnBack.Enabled = false;
            btnItemCorrect.Enabled = false;
            btnVoid.Enabled = false;
            btnItemDiscount.Enabled = false;
            btnHelp.Enabled = false;
            btnReprint.Enabled = false;
            btnSuspend.Enabled = false;
            btnSearch.Enabled = false;
            btnSearch_Category.Enabled = false;
            btnNext.Enabled = false;

            btnAddBagToCart.Enabled = false;
            btnNoBag.Enabled = false;
            btnBagPlus.Enabled = false;
            btnMinus.Enabled = false;

            btnSelectCreditCard.Enabled = false;
            btnSelectPointCard.Enabled = false;
            btnSelectGiftCard.Enabled = false;
            btnEBT.Enabled = false;
            btnManualETCKey.Enabled = false;

            g_ManagerCardScanReady = true;
            g_mMaterFunctionVal = ManualMasterFunction.ItemReprint;

            if (c_poscominfo.ci_mkno != "55" && c_poscominfo.ci_mkno != "62")                // 55 오로라, 62 다운타운 매장이 아닌 경우 Manager Key Scan 필요.
            {
                ctrTemp = ctrlOnScreen;
                transitionSiglePage(gbScanManagerCard, 308, 200);
                cpScanManagerCard.IsRunning = true;
            }
            else                                            // 55 오로라, 62 다운타운 매장인 경우 Manager Key Scan 필요 없음.
            {
                ProcessEntryCode(g_strManagerKey, 0);
            }

            KeyInReady();            
        }

        private void ProcessReprint()
        {
            // Barcode 입력 창 출력
            ctrTemp = ctrlOnScreen;
            ctrTemp2 = ctrlOnScreen;

            transitionSiglePage(gbReceiptReprint, 284, 200);

            txtReceiptReprintBarcode.Clear();
            txtReceiptReprintBarcode.Focus();

            // Change Status
            g_ReprintModeOn = true;

            // Button Control
            btnBack.Enabled = false;
            btnItemCorrect.Enabled = false;
            btnVoid.Enabled = false;
            btnItemDiscount.Enabled = false;
            btnReprint.Enabled = false;
            btnSuspend.Enabled = false;
            btnNext.Enabled = false;
            ItemCSView.Enabled = false;
            //btnSearch.Enabled = false;
            //btnSearch_Category.Enabled = false;


            if (ctrTemp == pn_ItemScan || ctrTemp == pnItemScanSearchBtn)
            {
                btnSearch.Enabled = false;
                btnSearch_Category.Enabled = false;
            }
            else if (ctrTemp == pnAddBag)
            {
                btnAddBagToCart.Enabled = false;
                btnNoBag.Enabled = false;
                btnBagPlus.Enabled = false;
                btnMinus.Enabled = false;
            }
            else if (ctrTemp == pnSelectPayment)
            {
                btnSelectCreditCard.Enabled = false;
                btnSelectPointCard.Enabled = false;
                btnSelectGiftCard.Enabled = false;
                btnEBT.Enabled = false;
            }
            // 저울 스캔 ON
            if (OPOSScanner.DeviceEnabled == false)
            {
                _EnableScannerDevice();
            }
        }
        private void btnReceiptReprintBack_Click(object sender, EventArgs e)
        {
            transitionSiglePage(gbReceiptReprint, 1023, 200);
            ctrlOnScreen = ctrTemp;

            g_ReprintModeOn = false;

            if (ctrTemp != pn_ItemScan && ctrTemp != pnItemScanSearchBtn)
            {
                // 저울에서 스캔 되는거 방지.
                if (OPOSScanner.DeviceEnabled)
                {
                    OPOSScanner.DataEvent -= ScannerDataEvent;
                    OPOSScanner.DeviceEnabled = false;
                }
            }
            
            ProcessReprintAfterButtonControl();
        }
        private void btnReceiptReprintLast_Click(object sender, EventArgs e)
        {
            // Receipt
            GstrPrtInvno = (Convert.ToInt64(txtInvNo.Text) - 1).ToString();
            GstrReprint = "R"; // Reprint시 R

            transitionSiglePage(gbReceiptReprint, 1023, 200);
            ctrlOnScreen = ctrTemp;

            g_ReprintModeOn = false;

            if (ctrTemp != pn_ItemScan && ctrTemp != pnItemScanSearchBtn)
            {
                // 저울에서 스캔 되는거 방지.
                if (OPOSScanner.DeviceEnabled)
                {
                    OPOSScanner.DataEvent -= ScannerDataEvent;
                    OPOSScanner.DeviceEnabled = false;
                }
            }

            ProcessReprintAfterButtonControl();

            PrtReceipt.PrintController = new System.Drawing.Printing.StandardPrintController();
            PrtReceipt.Print();
            
            txtReceiptReprintBarcode.Clear();
        }
        
        private void txtReceiptReprintBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                if (txtReceiptReprintBarcode.Text != "" && txtReceiptReprintBarcode.Text.Length == 13)
                {
                    // Receipt
                    GstrPrtInvno = txtReceiptReprintBarcode.Text;
                    GstrReprint = "R"; // Reprint시 R

                    PrtReceipt.PrintController = new System.Drawing.Printing.StandardPrintController();
                    PrtReceipt.Print();

                    txtReceiptReprintBarcode.Clear();
                    
                    transitionSiglePage(gbReceiptReprint, 1023, 200);
                    ctrlOnScreen = ctrTemp;

                    g_ReprintModeOn = false;

                    if (ctrTemp != pn_ItemScan && ctrTemp != pnItemScanSearchBtn)
                    {
                        // 저울에서 스캔 되는거 방지.
                        if (OPOSScanner.DeviceEnabled)
                        {
                            OPOSScanner.DataEvent -= ScannerDataEvent;
                            OPOSScanner.DeviceEnabled = false;
                        }
                    }

                    ProcessReprintAfterButtonControl();
                }
                else
                {
                    // Error Message
                    DisplayErrorMessageBox("Receipt Print", "Please Scan your Receipt Barcode.", 1, sMethod);
                    txtReceiptReprintBarcode.Clear();
                }
            }
        }

        private void ProcessReprintAfterButtonControl()
        {
            // Button Control
            ItemCSView.Enabled = true;
            
            btnSearch.Enabled = true;
            btnSearch_Category.Enabled = true;
            btnBack.Enabled = true;
            btnItemCorrect.Enabled = true;
            btnVoid.Enabled = true;
            btnItemDiscount.Enabled = true;
            btnReprint.Enabled = true;
            btnSuspend.Enabled = true;

            btnAddBagToCart.Enabled = true;
            btnNoBag.Enabled = true;
            btnBagPlus.Enabled = true;
            btnMinus.Enabled = true;

            g_CertifiedAdult = false;
            g_CertifiedAdultReady = false;
            g_iAdultLimit = 0;
            g_iStoredAdultLimit = 0;

            btnSelectCreditCard.Enabled = true;
            btnSelectPointCard.Enabled = true;
            btnSelectGiftCard.Enabled = true;

            //if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                        // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
            btnEBT.Enabled = true;

            btnManualETCKey.Enabled = true;

            if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
            {
                btnNext.Enabled = false;
            }
            else
            {
                btnNext.Enabled = true;
            }

            // EBT 결제 내역이 있는 경우
            if (Convert.ToDouble(lblPayEBT.Text) > 0)
            {
                btnEBT.Enabled = false;
            }
            else
            {
                //if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                            // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
                btnEBT.Enabled = true;
            }


            //if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
            //{
            //    btnNext.Enabled = false;
            //}
            //else
            //{
            //    btnNext.Enabled = true;
            //}

            //if (ctrlOnScreen == pn_ItemScan || ctrTemp == pnItemScanSearchBtn)
            //{
            //    btnSearch.Enabled = true;
            //    btnSearch_Category.Enabled = true;
            //}
            //else if (ctrTemp == pnAddBag)
            //{
            //    btnAddBagToCart.Enabled = true;
            //    btnNoBag.Enabled = true;
            //    btnBagPlus.Enabled = true;
            //    btnMinus.Enabled = true;
            //}
            //else if (ctrTemp == pnSelectPayment)
            //{
            //    btnSelectCreditCard.Enabled = true;
            //    btnSelectPointCard.Enabled = true;
            //    btnSelectGiftCard.Enabled = true;
            //    btnEBT.Enabled = true;
            //}
            KeyInReady();
        }

        private void btnSuspend_Click(object sender, EventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            g_sMessage = string.Format("Transaction Suspend Start");
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            
            // SUSPEND 하여 처리 할 수 있도록 ERROR 해제.
            //if (lblPayTotal.Text != "0.00")
            //{
            //    DisplayErrorMessageBox("Suspend", "SUSPEND NOT ALLOWED\n WHEN PAYMENT EXIST", 1, sMethod);
            //    return;
            //}

            if (txtNumCS.Text == "" && ItemCSView.Items.Count > 0 && lbTotalValCS.Text != "0.00")
            {
                btnBack.Enabled = false;
                btnItemCorrect.Enabled = false;
                btnVoid.Enabled = false;
                btnItemDiscount.Enabled = false;
                btnHelp.Enabled = false;
                btnReprint.Enabled = false;
                btnSuspend.Enabled = false;
                btnSearch.Enabled = false;
                btnSearch_Category.Enabled = false;
                btnNext.Enabled = false;
                ItemCSView.Enabled = false;

                btnAddBagToCart.Enabled = false;
                btnNoBag.Enabled = false;
                btnBagPlus.Enabled = false;
                btnMinus.Enabled = false;

                btnSelectCreditCard.Enabled = false;
                btnSelectPointCard.Enabled = false;
                btnSelectGiftCard.Enabled = false;
                btnEBT.Enabled = false;
                btnManualETCKey.Enabled = false;

                g_ManagerCardScanReady = true;
                g_mMaterFunctionVal = ManualMasterFunction.ItemSuspend;

                if (c_poscominfo.ci_mkno != "55" && c_poscominfo.ci_mkno != "62")                // 55 오로라, 62 다운타운 매장이 아닌 경우 Manager Key Scan 필요.
                {
                    ctrTemp = ctrlOnScreen;
                    transitionSiglePage(gbScanManagerCard, 308, 200);
                    cpScanManagerCard.IsRunning = true;
                }
                else                                            // 55 오로라, 62 다운타운 매장인 경우 Manager Key Scan 필요 없음.
                {
                    ProcessEntryCode(g_strManagerKey, 0);
                }

                KeyInReady();

                //DisplayErrorMessageBox("Suspend", "Transaction Will be Suspended. \nAre you Sure?", 2, sMethod);
//                TransactionSuspend();
                //MessageBox.Show("SUSPEND CANCELLED");
            }
            else
            {
                DisplayErrorMessageBox("Suspend", "Transaction Suspend Failed.", 1, sMethod);
                return;
            }
        }

        private void ProcessTransactionSuspend()
        {
            GstrPrtInvno = txtInvNo.Text;
            GstrReprint = "S"; // Suspend Slip

            DumpSuspendTrans();

            PrtReceipt.PrintController = new System.Drawing.Printing.StandardPrintController();
            PrtReceipt.Print();

            initListView();
            initSalesTotal();
            initPayment();
            initScale();
            InitMemberPointInfo();
            InitMemberDisplay();

            //5-3. New Invoice No
            GetNewInvNo();

            //5-4. New Item Scan 
            KeyInReady();

            // Sync Process 실행
            if(GblTestMode == false)
            {
                ProcessHanaSyncData();
            }
            //Process.Start(Application.StartupPath + "\\HanaSyncData.bat");
        }

        private void DumpSuspendTrans()
        {
            string sQBuff = string.Empty;

            long lReturn = 0;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            try
            {
                lReturn = c_remotedb.DBConnection();

                if (lReturn < 0)
                {
                    g_sMessage = string.Format("[{0}] Remote database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    DisplayErrorMessageBox("Suspend", "Remote database connection failed", 1, sMethod);

                    return;
                }

                //1. 서버 tb_SuspendTrans Table
                // Add - Table 삭제 
                sQBuff = "Delete From tb_SuspendTrans Where tInvNo = '" + txtInvNo.Text + "' ";
                if (c_remotedb.DBExcute(sQBuff) != 1)
                {
                    g_sMessage = string.Format("[{0}] Remotoe Item Delete failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    DisplayErrorMessageBox("Suspend", "Remotoe Item Delete failed", 1, sMethod);

                    c_remotedb.DBClose();

                    return;
                }
                else
                {
                    g_sMessage = string.Format("[{0}] (Remote tb_SuspendTrans data deleted successfully ({1}).", sMethod, c_poscominfo.ui_epno);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                }

                lReturn = c_localdb.DBConnection();

                if (lReturn < 0)
                {
                    g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    DisplayErrorMessageBox("Suspend", "Local database connection failed", 1, sMethod);

                    return;
                }

                // 2. 로컬 Local Suspend 삭제
                // Add - Table 삭제 
                sQBuff = "Delete From tb_SuspendTrans Where tInvNo = '" + txtInvNo.Text + "' ";
                if (c_localdb.DBExcute(sQBuff) != 1)
                {
                    g_sMessage = string.Format("[{0}] Local Item Delete failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    DisplayErrorMessageBox("Suspend", "Local Item Delete failed", 1, sMethod);

                    c_remotedb.DBClose();

                    return;
                }
                else
                {
                    g_sMessage = string.Format("[{0}] (Local tb_SuspendTrans data deleted successfully ({1}).", sMethod, c_poscominfo.ui_epno);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                }


                //2-1. tb_SoldItem
                if(GintLocation == 1)                           // 벤쿠버 일때
                {
                    sQBuff = "INSERT INTO HANAMART.dbo.tb_SuspendTrans ";
                    sQBuff += "SELECT tInvNo,tID,tDate,tTime,tProd,tPtype,tPtype2,tCat1,tCat2,tCat3,tCat4,tCat5,tCatCode,tQty,tPunit,tIUprice";
                    sQBuff += ",tOUprice,tWprice,tNative,tPromo,tPromoCode,tAmt,tGst,tPst,tHst,tTax,tCust,tPassWord,tPassStation,tUpCode,tSpecial,tFree";
                    sQBuff += ",tMMBC,tSupp,tType,tEntryCode,tFoodStamp,tGiftCardRef,tRelatedID,tMixMatch,tShift,'" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "','" + txtEmpNo.Text + "','" + c_poscominfo.si_counternum + "',0,1 ";
                    sQBuff += "FROM HANAMART.dbo.tb_OrderItem ";

                }
                else if(GintLocation == 2)                      // 토론토 일때
                {
                    sQBuff = "INSERT INTO HANAMART.dbo.tb_SuspendTrans ";
                    sQBuff += "SELECT tInvNo,tID,tDate,tTime,tProd,tPtype,tPtype2,tCat1,tCat2,tCat3,tCat4,tCat5,tQty,tPunit,tIUprice";
                    sQBuff += ",tOUprice,tWprice,tNative,tPromo,tAmt,tGst,tPst,tHst,tTax,tCust,tPassWord,tPassStation,tUpCode,tSpecial,tFree";
                    sQBuff += ",tSupp,tType,tEntryCode,tFoodStamp,tGiftCardRef,tRelatedID,tMixMatch,tShift,tMemo,'" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "','" + txtEmpNo.Text + "','" + c_poscominfo.si_counternum + "',0,1 ";
                    sQBuff += "FROM HANAMART.dbo.tb_OrderItem ";
                }
                else                                        // 미국 일때
                {
                    sQBuff = "INSERT INTO HANAMART.dbo.tb_SuspendTrans ";
                    sQBuff += "SELECT tInvNo,tID,tDate,tTime,tProd,tPtype,tPtype2,tCat1,tCat2,tCat3,tCat4,tCat5,tQty,tPunit,";
                    sQBuff += "tIUprice,tOUprice,tWprice,tNative,tPromo,tAmt,tGst,tPst,tTax,'','','',tCust,tPassWord,tPassStation,tUpCode,tSpecial,tFree,";
                    sQBuff += "tSupp,tType,tEntryCode,tFoodStamp,tGiftCardRef,tRelatedID,tShift,'" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "','" + txtEmpNo.Text + "','" + c_poscominfo.si_counternum + "',0,1 ";
                    sQBuff += "FROM HANAMART.dbo.tb_OrderItem ";
                }
                if (c_localdb.DBExcute(sQBuff) != 1)
                {
                    g_sMessage = string.Format("[{0}] Suspend Item insert failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    c_localdb.DBClose();

                    DisplayErrorMessageBox("Suspend", "Suspend Item insert failed", 1, sMethod);

                    return;
                }
                else
                {

                    g_sMessage = string.Format("[{0}] (SuspendTrans data saved ({1}).", sMethod, c_poscominfo.ui_epno);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    // Suspend Histroy 
                    sQBuff = "Insert into tb_SuspendHist (sh_datetime,sh_invno,sh_TranGb,sh_ItemNum,sh_Amt,sh_Password,sh_station,sh_UpFlag) Values ";
                    sQBuff += "('" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "','" + txtInvNo.Text + "'," + "1" + "," + lbItemCountValCS.Text + "," + lbTotalValCS.Text + ",'" + txtEmpNo.Text + "'," + c_poscominfo.si_counternum + ",1)";

                    if (c_localdb.DBExcute(sQBuff) != 1)
                    {
                        g_sMessage = string.Format("[{0}] Suspend History insert failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                        DisplayErrorMessageBox("Suspend", "Suspend History insert failed", 1, sMethod);

                        c_localdb.DBClose();

                        return;
                    }
                    else
                    {
                        g_sMessage = string.Format("[{0}] (tb_Suspendhist data saved ({1}).", sMethod, c_poscominfo.ui_epno);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                        sQBuff = "DELETE HANAMART.dbo.tb_OrderItem WHERE tInvNo = '" + txtInvNo.Text + "' ";

                        if (c_localdb.DBExcute(sQBuff) != 1)
                        {
                            g_sMessage = string.Format("[{0}] tb_OrderItem delete failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                            DisplayErrorMessageBox("Suspend", "tb_OrderItem delete failed", 1, sMethod);

                            c_localdb.DBClose();

                            return;
                        }
                        else
                        {
                            g_sMessage = string.Format("[{0}] (tb_OrderItem data deleted ({1}).", sMethod, c_poscominfo.ui_epno);
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                        }
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
        }

        private void btnSearchOrKeyinItem_Click(object sender, EventArgs e)
        {
            iLSearchImage.Images.Clear();
            lvSearchImage_Item.Clear();
            lvSearchImage_Category.Clear();
            lvSearchImage_Additional.Clear();

            lvSearchImage_Item.BringToFront();
            lvSearchImage_Category.SendToBack();

            pn_Keyboard.BringToFront();
            pn_Keyboard.Visible = true;
            btnKeyboardClear.Text = "CLEAR";
            btnKeyboardClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(46)))), ((int)(((byte)(95)))));
            
            // Page bug 
            g_SearchItemPageNum = 0;
            g_SearchItemMaxPageNum = 0;

            g_SearchCategory = false;

            g_SearchAdditional = false;

            txtSearchCode.Clear();
            txtSearchCode.Focus();

            btnSearchPageLeft.Visible = true;
            btnSearchPageRight.Visible = true;
            btnSearchPageLeft.Enabled = false;
            btnSearchPageRight.Enabled = false;

            btnSearchCategory_TZ.Enabled = false;
            btnSearchCategory_QS.Enabled = false;
            btnSearchCategory_P.Enabled = false;
            btnSearchCategory_NO.Enabled = false;
            btnSearchCategory_KM.Enabled = false;
            btnSearchCategory_DJ.Enabled = false;
            btnSearchCategory_BC.Enabled = false;
            btnSearchCategory_A.Enabled = false;
            btnSearchCategory_All.Enabled = false;

            // 초기 화면 출력
            GetimageListView("INIT", ImageType.Item);

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnAdditionalSearch_Click(object sender, EventArgs e)
        {
            iLSearchImage.Images.Clear();
            lvSearchImage_Item.Clear();
            lvSearchImage_Category.Clear();
            lvSearchImage_Additional.Clear();

            lvSearchImage_Item.SendToBack();
            lvSearchImage_Category.SendToBack();
            lvSearchImage_Additional.BringToFront();

            pn_Keyboard.BringToFront();
            pn_Keyboard.Visible = true;

            btnSearchPageLeft.Visible = false;
            btnSearchPageRight.Visible = false;

            g_SearchCategory = false;

            g_SearchAdditional = true;

            txtSearchCode.Clear();
            txtSearchCode.Focus();
            
            btnSearchCategory_TZ.Enabled = false;
            btnSearchCategory_QS.Enabled = false;
            btnSearchCategory_P.Enabled = false;
            btnSearchCategory_NO.Enabled = false;
            btnSearchCategory_KM.Enabled = false;
            btnSearchCategory_DJ.Enabled = false;
            btnSearchCategory_BC.Enabled = false;
            btnSearchCategory_A.Enabled = false;
            btnSearchCategory_All.Enabled = false;

            btnKeyboardClear.Text = "ENTER";
            btnKeyboardClear.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(23)))), ((int)(((byte)(24)))));

            initlvSearchImage_Additional();
            // 초기 화면 출력
            GetimageListView("INIT", ImageType.Additional_Data);

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void initlvSearchImage_Additional()
        {
            lvSearchImage_Additional.Columns.Add("PRODUCT ID", 260, HorizontalAlignment.Left);
            lvSearchImage_Additional.Columns.Add("ITEM NAME", 350, HorizontalAlignment.Left);
            lvSearchImage_Additional.Columns.Add("PRICE", 160, HorizontalAlignment.Right);
            lvSearchImage_Additional.Columns.Add("TAX", 80, HorizontalAlignment.Right);
        }

        private void btnKeyboardInvisable_Click(object sender, EventArgs e)
        {
            pn_Keyboard.SendToBack();
            pn_Keyboard.Visible = false;

            // 내렸을때 Category 화면으로 이동.
            iLSearchImage.Images.Clear();
            lvSearchImage_Item.Clear();
            lvSearchImage_Category.Clear();
            lvSearchImage_Additional.Clear();

            g_SearchCategory_Clicked = false;
            g_SearchKeyInputPLU_Number = false;

            g_SearchAdditional = false;

            btnSearchPageLeft.Visible = false;
            btnSearchPageRight.Visible = false;

            btnSearchCategory_TZ.Enabled = true;
            btnSearchCategory_QS.Enabled = true;
            btnSearchCategory_P.Enabled = true;
            btnSearchCategory_NO.Enabled = true;
            btnSearchCategory_KM.Enabled = true;
            btnSearchCategory_DJ.Enabled = true;
            btnSearchCategory_BC.Enabled = true;
            btnSearchCategory_A.Enabled = true;
            btnSearchCategory_All.Enabled = true;

            if (GintLocation == 1)               // 벤쿠버인  경우
            {
                btnSearchOrKeyinItem.Text = "Search Key In Item";
                btnBackToCategory.Text = "Back";
            }
            else if (GintLocation == 2)               // 토론토인 경우
            {
                btnSearchOrKeyinItem.Text = "Search Key In Category";
                btnBackToCategory.Text = "Back to Category";
            }
            else if (GintLocation == 3)               // 미국인 경우
            {
                btnSearchOrKeyinItem.Text = "Search Key In Item";
                btnBackToCategory.Text = "Back";
                g_SearchCategory = true;
            }

            // All Image 출력
            if (g_SearchCategory == true)
            {
                lvSearchImage_Item.SendToBack();
                lvSearchImage_Category.BringToFront();
                GetimageListView("ALL", ImageType.Category);
            }
            else
            {
                lvSearchImage_Item.BringToFront();
                lvSearchImage_Category.SendToBack();
                GetimageListView("ALL", ImageType.Item);
            }

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnBackToCategory_Click(object sender, EventArgs e)
        {
            pn_Keyboard.Visible = false;

            iLSearchImage.Images.Clear();
            lvSearchImage_Item.Clear();
            lvSearchImage_Category.Clear();
            lvSearchImage_Additional.Clear();

            g_SearchAdditional = false;

            //g_SearchCategory_Clicked = false;
            g_SearchKeyInputPLU_Number = false;

            btnSearchPageLeft.Visible = false;
            btnSearchPageRight.Visible = false;

            g_SearchItemPageNum = 0;
            g_SearchItemMaxPageNum = 0;

            btnSearchCategory_TZ.Enabled = true;
            btnSearchCategory_QS.Enabled = true;
            btnSearchCategory_P.Enabled = true;
            btnSearchCategory_NO.Enabled = true;
            btnSearchCategory_KM.Enabled = true;
            btnSearchCategory_DJ.Enabled = true;
            btnSearchCategory_BC.Enabled = true;
            btnSearchCategory_A.Enabled = true;
            btnSearchCategory_All.Enabled = true;

            //if (GintLocation != 3)               // 미국이 아닌 경우
            //{
            //    btnSearchOrKeyinItem.Text = "Search Key In Category";
            //    btnBackToCategory.Text = "Back to Category";
            //}
            //else
            //{
            //    btnSearchOrKeyinItem.Text = "Search Key In Item";
            //    btnBackToCategory.Text = "Back to Category";
            //    g_SearchCategory = true;
            //}

            // All Image 출력
            if (GintLocation != 3)               // 미국이 아닌 경우
            {
                if (g_SearchCategory == true)
                {
                    lvSearchImage_Item.SendToBack();
                    lvSearchImage_Category.BringToFront();
                    GetimageListView("ALL", ImageType.Category);
                }
                else
                {
                    lvSearchImage_Item.BringToFront();
                    lvSearchImage_Category.SendToBack();
                    GetimageListView("ALL", ImageType.Item);
                }
            }
            else
            {
                if (g_SearchCategory == true && g_SearchCategory_Clicked != true)
                {
                    // Exit Search
                    if (GssSearchItem.GetCurrentlySpokenPrompt() != null)                // 음성 중복 재생 방지.
                    {
                        GssSearchItem.SpeakAsyncCancel(GssSearchItem.GetCurrentlySpokenPrompt());
                    }

                    //ctrlOnScreen = ctrTemp;

                    gbSearchBox.Visible = false;
                    txtSearchCode.Clear();
                    btnBack.Enabled = true;
                    btnItemCorrect.Enabled = true;
                    btnVoid.Enabled = true;
                    btnItemDiscount.Enabled = true;
                    btnReprint.Enabled = true;
                    btnSuspend.Enabled = true;

                    if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
                    {
                        btnNext.Enabled = false;
                    }
                    else
                    {
                        btnNext.Enabled = true;
                    }

                    btnSearch.Enabled = true;
                    btnSearch_Category.Enabled = true;

                    if (g_HelpModeOn == true)
                        btnHelp.Enabled = false;
                    else
                        btnHelp.Enabled = true;

                    iLSearchImage.Images.Clear();
                    lvSearchImage_Item.Clear();
                    lvSearchImage_Category.Clear();
                    g_SearchCategory = false;
                    g_SearchCategory_Clicked = false;
                    g_SearchKeyInputPLU_Number = false;

                    KeyInReady();

                    if (OPOSScanner.DeviceEnabled == false)
                    {
                        _EnableScannerDevice();
                    }

                    //Timer Stop & Start
                    BackToStartTimerFromItemScan.Stop();
                    BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
                    BackToStartTimerFromItemScan.Start();
                    //lvSearchImage_Item.SendToBack();
                    //lvSearchImage_Category.BringToFront();
                    //GetimageListView("ALL", ImageType.Category);
                }
                else
                {
                    g_SearchCategory_Clicked = false;
                    btnBackToCategory.Text = "Back";
                    lvSearchImage_Item.SendToBack();
                    lvSearchImage_Category.BringToFront();
                    GetimageListView("ALL", ImageType.Category);
                }
            }

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnSearchCategory_A_Click(object sender, EventArgs e)
        {
            pn_Keyboard.Visible = false;
            if(g_SearchCategory == true)
            {
                if (g_SearchCategory_Clicked == true)                   // 카테고리 검색에서 카테고리를 누른 상태에서 스피드 키를 눌렀을때.
                {
                    lvSearchImage_Item.BringToFront();
                    lvSearchImage_Category.SendToBack();

                    // Page Number 초기화
                    g_SearchItemPageNum = 0;
                    g_SearchItemMaxPageNum = 0;

                    GetimageListView("A", ImageType.Item);
                }
                else
                {
                    iLSearchImage.Images.Clear();
                    lvSearchImage_Item.Clear();
                    lvSearchImage_Category.Clear();

                    lvSearchImage_Item.SendToBack();
                    lvSearchImage_Category.BringToFront();
                    GetimageListView("A", ImageType.Category);
                }
            }
            else
            {
                iLSearchImage.Images.Clear();
                lvSearchImage_Item.Clear();
                lvSearchImage_Category.Clear();

                lvSearchImage_Item.BringToFront();
                lvSearchImage_Category.SendToBack();

                // Page Number 초기화
                g_SearchItemPageNum = 0;
                g_SearchItemMaxPageNum = 0;

                GetimageListView("A", ImageType.Item);
            }

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();     
        }

        private void btnSearchCategory_All_Click(object sender, EventArgs e)
        {
            pn_Keyboard.Visible = false;
            
            iLSearchImage.Images.Clear();
            lvSearchImage_Item.Clear();
            lvSearchImage_Category.Clear();

            g_SearchCategory_Clicked = false;
            g_SearchKeyInputPLU_Number = false;

            // All Image 출력
            if (GintLocation != 3)               // 미국이 아닌 경우
            {
                if (g_SearchCategory == true)
                {
                    btnSearchOrKeyinItem.Text = "Search Key In Item";
                    btnBackToCategory.Text = "Back to Category";

                    lvSearchImage_Item.SendToBack();
                    lvSearchImage_Category.BringToFront();

                    btnSearchPageLeft.Visible = false;
                    btnSearchPageRight.Visible = false;

                    // Page Number 초기화
                    g_SearchItemPageNum = 0;
                    g_SearchItemMaxPageNum = 0;

                    GetimageListView("ALL", ImageType.Category);
                }
                else
                {
                    lvSearchImage_Item.BringToFront();
                    lvSearchImage_Category.SendToBack();
                    GetimageListView("ALL", ImageType.Item);
                }
            }
            else
            {
                btnSearchOrKeyinItem.Text = "Search Key In Item";
                btnBackToCategory.Text = "Back";
                
                lvSearchImage_Item.SendToBack();
                lvSearchImage_Category.BringToFront();
                
                btnSearchPageLeft.Visible = false;
                btnSearchPageRight.Visible = false;
                
                // Page Number 초기화
                g_SearchItemPageNum = 0;
                g_SearchItemMaxPageNum = 0;
                
                GetimageListView("ALL", ImageType.Category);
            }

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }
        
        public void GetimageListView(string strContents, ImageType eImageType)
        {
            string strTemp, strprodId, strPLU, strProdName, strSplitChar;
            string strCategory, strCategoryName;
            
            string[] imgFiles, strSplitContents;
            
            var list = new List<string>();

            int iTempImageIndex = 0;
            
            //iLSearchImage.ImageSize = new Size(112, 100);            // Image Size 설정.
            iLSearchImage.ImageSize = new Size(115, 80);            // Image Size 설정.
            iLSearchImage.ColorDepth = ColorDepth.Depth32Bit;       // Image Depth 설정.
            
            switch (strContents)
            {
                case "ALL":
                    iLSearchImage.Images.Clear();
                    lvSearchImage_Item.Clear();
                    lvSearchImage_Category.Clear();

                    if (eImageType == ImageType.Item)
                        if(GintLocation == 3)                   // 미국인 경우
                            imgFiles = Directory.GetFiles(Application.StartupPath + "\\Image\\", "*.jpg", SearchOption.TopDirectoryOnly); // 경로 변경 필요. 프로젝트내부로 이동
                        else
                            imgFiles = Directory.GetFiles(Application.StartupPath + "\\Image\\", "*.png", SearchOption.TopDirectoryOnly); // 경로 변경 필요. 프로젝트내부로 이동
                    else if (eImageType == ImageType.Category)
                        if (GintLocation == 3)                   // 미국인 경우
                            imgFiles = Directory.GetFiles(Application.StartupPath + "\\Image\\Category\\", "*.jpg", SearchOption.AllDirectories); // 경로 변경 필요. 프로젝트내부로 이동
                        else
                            imgFiles = Directory.GetFiles(Application.StartupPath + "\\Image\\Category\\", "*.png", SearchOption.AllDirectories); // 경로 변경 필요. 프로젝트내부로 이동
                    else
                        imgFiles = Directory.GetFiles(Application.StartupPath + "\\Image\\", "*.*", SearchOption.TopDirectoryOnly); // 경로 변경 필요. 프로젝트내부로 이동
                    break;
                case "A":
                case "B_C":
                case "P":
                case "D_E_F_G_H_I_J":
                case "K_L_M":
                case "N_O":
                case "Q_R_S":
                case "T_U_V_W_X_Y_Z":
                    
                    strSplitContents = strContents.Split('_');

                    for (int i = 0; i < strSplitContents.Length; i++)
                    {
                        strSplitChar = strSplitContents[i];
                        if (eImageType == ImageType.Item)
                        {
                            if(g_SearchCategory_Clicked == true)                                // Category Search에서 Category선택 후에 상단 스피트키를 눌렀을때.
                            {
                                iLSearchImage.Images.Clear();
                                lvSearchImage_Item.Clear();

                                string strCategoryNumber = string.Empty;
                                ListView.SelectedListViewItemCollection items = lvSearchImage_Category.SelectedItems;
                                if (items.Count < 1)
                                {
                                    g_sMessage = "아이템을 찾을 수 없습니다.[0]";
                                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                                    iLSearchImage.Images.Clear();
                                    lvSearchImage_Item.Clear();
                                    lvSearchImage_Category.Clear();

                                    lvSearchImage_Category.SendToBack();

                                    changeToItemScan();

                                    //list.AddRange(Directory.GetFiles(Application.StartupPath + "\\Image\\", "*_*_*_" + strSplitChar + "*.*", SearchOption.TopDirectoryOnly)); // 경로 변경 필요. 프로젝트내부로 이동

                                    return;
                                }

                                ListViewItem lvItem = items[0];
                                string strSelectedCategoryName = lvItem.Text;       // 선택된 카테고리의 번호 받기.
                                strCategoryNumber = GetCategoryNumberInSearchImage(strSelectedCategoryName);          // 카테고리 이름으로 카테고리 번호 가져오기.

                                list.AddRange(Directory.GetFiles(Application.StartupPath + "\\Image\\", strCategoryNumber + "_*_*_" + strSplitChar + "*.*", SearchOption.TopDirectoryOnly)); // 경로 변경 필요. 프로젝트내부로 이동
                            }
                            else
                            {
                                iLSearchImage.Images.Clear();
                                lvSearchImage_Item.Clear();
                                lvSearchImage_Category.Clear();

                                list.AddRange(Directory.GetFiles(Application.StartupPath + "\\Image\\", "*_*_*_" + strSplitChar + "*.*", SearchOption.TopDirectoryOnly)); // 경로 변경 필요. 프로젝트내부로 이동
                            }
                            
                        }
                            
                        else if (eImageType == ImageType.Category)
                            if (GintLocation == 3)                   // 미국인 경우
                                list.AddRange(Directory.GetFiles(Application.StartupPath + "\\Image\\Category\\", "*_" + strSplitChar + "*.jpg", SearchOption.AllDirectories)); // 경로 변경 필요. 프로젝트내부로 이동
                            else
                                list.AddRange(Directory.GetFiles(Application.StartupPath + "\\Image\\Category\\", "*_" + strSplitChar + "*.png", SearchOption.AllDirectories)); // 경로 변경 필요. 프로젝트내부로 이동
                        else
                            list.AddRange(Directory.GetFiles(Application.StartupPath + "\\Image\\", "*_*_*_*" + strSplitChar + "*.*", SearchOption.TopDirectoryOnly)); // 경로 변경 필요. 프로젝트내부로 이동
                    }
                    imgFiles = list.ToArray();
                    break;
                default:
                    if (eImageType == ImageType.Category)                           // Category에서 Key 입력
                    {
                        if (strContents.All(char.IsDigit))      //숫자인지 확인 True 면 숫자, False 면 문자.
                        {
                            // 숫자 PLU 검색
                            if (GintLocation == 3)                   // 미국인 경우
                                imgFiles = Directory.GetFiles(Application.StartupPath + "\\Image\\Category\\", strContents + "*_" + "*.jpg", SearchOption.AllDirectories); // 경로 변경 필요. 프로젝트내부로 이동
                            else
                                imgFiles = Directory.GetFiles(Application.StartupPath + "\\Image\\Category\\", strContents + "*_" + "*.png", SearchOption.AllDirectories); // 경로 변경 필요. 프로젝트내부로 이동
                        }
                        else
                        {
                            // 문자 Category Name 검색
                            if (GintLocation == 3)                   // 미국인 경우
                                imgFiles = Directory.GetFiles(Application.StartupPath + "\\Image\\Category\\", "*_" + strContents + "*.jpg", SearchOption.AllDirectories); // 경로 변경 필요. 프로젝트내부로 이동
                            else
                                imgFiles = Directory.GetFiles(Application.StartupPath + "\\Image\\Category\\", "*_" + strContents + "*.png", SearchOption.AllDirectories); // 경로 변경 필요. 프로젝트내부로 이동
                        }
                    }
                    else if (eImageType == ImageType.Item && strContents != "INIT")                                                   // Item 에서 Key 입력
                    {
                        if (g_SearchCategory == true)
                        {
                            iLSearchImage.Images.Clear();
                            lvSearchImage_Item.Clear();

                            string strCategoryNumber = string.Empty;
                            //ListView.SelectedListViewItemCollection items = lvSearchImage_Category.SelectedItems;
                            //ListViewItem lvItem = items[0];
                            //string strSelectedCategoryName = lvItem.Text;       // 선택된 카테고리의 번호 받기.
                            string strSelectedCategoryName = g_Temp_lvItem;       // 선택된 카테고리의 번호 받기.
                            strCategoryNumber = GetCategoryNumberInSearchImage(strSelectedCategoryName);          // 카테고리 이름으로 카테고리 번호 가져오기

                            if (strContents.All(char.IsDigit))      //숫자인지 확인 True 면 숫자, False 면 문자.
                            {
                                if(g_SearchKeyInputPLU_Number == true)
                                {
                                    // 숫자 PLU 검색
                                    imgFiles = Directory.GetFiles(Application.StartupPath + "\\Image\\", strCategoryNumber + "_*_" + strContents + "*_*.*", SearchOption.TopDirectoryOnly); // 경로 변경 필요. 프로젝트내부로 이동
                                }
                                else
                                {
                                    if (strContents == "")
                                    {
                                        strContents = strCategoryNumber;
                                    }
                                    // 숫자 PLU 검색
                                    imgFiles = Directory.GetFiles(Application.StartupPath + "\\Image\\", strContents + "_*_*_*.*", SearchOption.TopDirectoryOnly); // 경로 변경 필요. 프로젝트내부로 이동
                                }
                            }
                            else                                    // Category -> Item 클릭 -> keyboard 입력.
                            {
                                // 문자 Prod Name 검색
                                imgFiles = Directory.GetFiles(Application.StartupPath + "\\Image\\", strCategoryNumber + "_*_*_" + strContents + "*.*", SearchOption.TopDirectoryOnly); // 경로 변경 필요. 프로젝트내부로 이동
                            }

                            //lvSearchImage_Category.Clear();
                        }
                        else
                        {
                            if (strContents.All(char.IsDigit))      //숫자인지 확인 True 면 숫자, False 면 문자.
                            {
                                // 숫자 PLU 검색
                                imgFiles = Directory.GetFiles(Application.StartupPath + "\\Image\\", "*_*_" + strContents + "*_" + "*.*", SearchOption.TopDirectoryOnly); // 경로 변경 필요. 프로젝트내부로 이동
                            }
                            else
                            {
                                // 문자 Prod Name 검색
                                imgFiles = Directory.GetFiles(Application.StartupPath + "\\Image\\", "*_*_*_*" + strContents + "*.*", SearchOption.TopDirectoryOnly); // 경로 변경 필요. 프로젝트내부로 이동
                            }
                        }                        
                    }
                    else
                    {
                        imgFiles = new string[0];                        
                    }                    
                    break;
            }

            string[] FilterdimgFiles = new string[imgFiles.Length];
            int j = 0;
            for (int i = 0; i < imgFiles.Length; i++)
            {
                if (eImageType == ImageType.Item && GintLocation == 3)
                //if (eImageType == ImageType.Item )
                {
                    strTemp = imgFiles[i];
                    strTemp = strTemp.Substring(strTemp.LastIndexOf('\\') + 1);
                    strTemp = strTemp.Substring(0, strTemp.LastIndexOf('.'));

                    strprodId = strTemp.Substring(strTemp.IndexOf('_') + 1);           // Prod ID
                    strprodId = strprodId.Substring(0, strprodId.IndexOf('_'));         // Prod ID 

                    if (GetImageDisplaySettingStatus(strprodId) == true)             // ProdCat3 Image Display Enable/Disable 설정 확인.
                    {
                        iLSearchImage.Images.Add(Image.FromFile(imgFiles[i]));
                        FilterdimgFiles[j] = imgFiles[i];
                        j++;
                    }                    
                }
                else
                {
                    iLSearchImage.Images.Add(Image.FromFile(imgFiles[i]));
                    FilterdimgFiles[j] = imgFiles[i];
                    j++;
                }                
            }
            
            if (iLSearchImage.Images.Count != 0)
            {
                if (eImageType == ImageType.Category)   // Category로 검색하는 경우.
                {
                    lvSearchImage_Category.LargeImageList = iLSearchImage;
                    for (int i = 0; i < iLSearchImage.Images.Count; i++)
                    {
                        strTemp = imgFiles[i];
                        strTemp = strTemp.Substring(strTemp.LastIndexOf('\\') + 1);
                        strTemp = strTemp.Substring(0, strTemp.LastIndexOf('.'));                        
                        strCategory = strTemp.Substring(0, strTemp.IndexOf('_'));         //Category Number
                        strCategoryName = strTemp.Substring(strTemp.LastIndexOf('_') + 1);  // Category Name

                        ListViewItem item = new ListViewItem(strCategoryName, i);     // 표시 형식 (Category Name)

                        item.ImageIndex = i;
                        lvSearchImage_Category.Items.Add(item);
                    }
                }
                else if (eImageType == ImageType.Item)                               // Item으로 검색하는 경우
                {
                    lvSearchImage_Item.LargeImageList = iLSearchImage;
                    g_SearchItemMaxPageNum = iLSearchImage.Images.Count / 10;             // Max Page 

                    //if (iLSearchImage.Images.Count % 10 != 0)                       // Page 나머지 표시하는 Page 추가 +1
                    //{
                    //    g_SearchItemMaxPageNum = g_SearchItemMaxPageNum + 1;
                    //}

                    int iPageLimitCount = 0;
                    if (g_SearchItemPageNum == g_SearchItemMaxPageNum)                  // Max Page 바로 이전 Page 인 경우
                    {
                        iPageLimitCount = (g_SearchItemPageNum * 10) + (iLSearchImage.Images.Count % 10);        // 나머지 만큼 더해서 Page 표시
                    }
                    else
                    {
                        iPageLimitCount = (g_SearchItemPageNum + 1) * 10;                                              // Page Num에 10개씩 표시.
                    }

                    if (iLSearchImage.Images.Count > 10)                        // Page Select 버튼 활성화
                    {
                        btnSearchPageLeft.Visible = true;
                        btnSearchPageRight.Visible = true;
                        btnSearchPageLeft.Enabled = true;
                        btnSearchPageRight.Enabled = true;
                    }
                    else
                    {
                        btnSearchPageLeft.Visible = true;
                        btnSearchPageRight.Visible = true;
                        btnSearchPageLeft.Enabled = false;
                        btnSearchPageRight.Enabled = false;
                    }

                    iTempImageIndex = g_SearchItemPageNum * 10;

                    for (int i = g_SearchItemPageNum * 10; i < iPageLimitCount; i++)
                    {
                        strTemp = FilterdimgFiles[i];
                        strTemp = strTemp.Substring(strTemp.LastIndexOf('\\') + 1);
                        strTemp = strTemp.Substring(0, strTemp.LastIndexOf('.'));

                        strCategory = strTemp.Substring(0, strTemp.IndexOf('_'));         //Categoty 

                        strprodId = strTemp.Substring(strTemp.IndexOf('_') + 1);           // Prod ID
                        strprodId = strprodId.Substring(0, strprodId.IndexOf('_'));         // Prod ID 

                        strPLU = strTemp.Substring(strTemp.IndexOf('_') + 1);            // Prod PLU
                        strPLU = strPLU.Substring(strPLU.IndexOf('_') + 1);           // Prod PLU
                        strPLU = strPLU.Substring(0, strPLU.IndexOf('_'));

                        strProdName = strTemp.Substring(strTemp.LastIndexOf('_') + 1);  // Prod Name

                        ListViewItem item;

                        if (GintLocation == 3)                                                 // 미국 인 경우
                        {
                            if (GetImageDisplaySettingStatus(strprodId) == true)             // ProdCat3 Image Display Enable/Disable 설정 확인.
                            {
                                //item = new ListViewItem(strPLU + " " + strProdName, i);     // 표시 형식 (PLU  + Prod Name)
                                //item.ImageIndex = i;

                                item = new ListViewItem(strPLU + " " + strProdName, iTempImageIndex);     // 표시 형식 (PLU  + Prod Name)
                                item.ImageIndex = iTempImageIndex;

                                lvSearchImage_Item.Items.Add(item);
                                iTempImageIndex++;
                            }
                        }
                        else                                                                    // 벤쿠버, 토론토 인 경우. HANARO에서 ProdCat3 설정하는 파트가 추가 될때 까지 임시로 구분.
                        {
                            item = new ListViewItem(strPLU + " " + strProdName, iTempImageIndex);     // 표시 형식 (PLU  + Prod Name)
                            item.ImageIndex = iTempImageIndex;

                            lvSearchImage_Item.Items.Add(item);
                            iTempImageIndex++;
                        }                                                                                   
                    }

                    //for (int i = 0; i < iLSearchImage.Images.Count; i++)
                    //{
                    //    strTemp = imgFiles[i];
                    //    strTemp = strTemp.Substring(strTemp.LastIndexOf('\\') + 1);
                    //    strTemp = strTemp.Substring(0, strTemp.LastIndexOf('.'));

                    //    strCategory = strTemp.Substring(0, strTemp.IndexOf('_'));         //Categoty 

                    //    strprodId = strTemp.Substring(strTemp.IndexOf('_') + 1);           // Prod ID
                    //    strprodId = strprodId.Substring(0, strprodId.IndexOf('_'));         // Prod ID 

                    //    strPLU = strTemp.Substring(strTemp.IndexOf('_') + 1);           // Prod PLU
                    //    strPLU = strPLU.Substring(strPLU.IndexOf('_') + 1);           // Prod PLU
                    //    strPLU = strPLU.Substring(0, strPLU.IndexOf('_'));

                    //    strProdName = strTemp.Substring(strTemp.LastIndexOf('_') + 1);  // Prod Name

                    //    ListViewItem item = new ListViewItem(strPLU + " " + strProdName, i);     // 표시 형식 (PLU  + Prod Name)

                    //    item.ImageIndex = i;
                    //    lvSearchImage_Item.Items.Add(item);
                    //}
                }
            }
            else
            {
                if(g_SearchCategory == true)
                {
                    if(g_SearchCategory_Clicked == true)
                    {
                        //lvSearchImage_Category.Clear();

                        iLSearchImage.ImageSize = new Size(256, 10);            // Image Size 설정.
                        lvSearchImage_Item.LargeImageList = iLSearchImage;
                        ListViewItem item = new ListViewItem("No Search Result.");
                        lvSearchImage_Item.Items.Add(item);
                    }
                    else
                    {
                        iLSearchImage.ImageSize = new Size(256, 10);            // Image Size 설정.
                        lvSearchImage_Category.LargeImageList = iLSearchImage;
                        ListViewItem item = new ListViewItem("No Search Result.");
                        lvSearchImage_Category.Items.Add(item);
                    }
                }
                else
                {
                    if(g_SearchAdditional == true)                              //Additional Search 버튼 눌렀을때
                    {             
                        // 상품 정보 조회 하여 List View에 업데이트.
                        GetProdInfo(strContents);
                    }
                    else                                                        // Search Or Key In 버튼 눌렀을때
                    {
                        if (strContents != "INIT")
                        {
                            iLSearchImage.ImageSize = new Size(256, 10);            // Image Size 설정.
                            lvSearchImage_Item.LargeImageList = iLSearchImage;
                            ListViewItem item = new ListViewItem("No Search Result.");
                            lvSearchImage_Item.Items.Add(item);
                        }
                        else
                        {
                            iLSearchImage.ImageSize = new Size(256, 10);            // Image Size 설정.
                            lvSearchImage_Item.LargeImageList = iLSearchImage;
                            lvSearchImage_Item.Items.Clear();
                            ListViewItem item = new ListViewItem("Please input your Item Name or PLU Number.");
                            lvSearchImage_Item.Items.Add(item);

                            //g_SearchCategory = true;
                        }
                    }                                        
                }
            }

            //const uint LVM_SETICONSPACING = 0x1025;
            //SendMessage(lvSearchImage_Item.Handle, LVM_SETICONSPACING, IntPtr.Zero, new IntPtr(170));    // 리스트뷰에 나오는 사진들 간격 조절 

            this.Refresh();

            ListView_SetSpacing(lvSearchImage_Item, 170, 130);
            ListView_SetSpacing(lvSearchImage_Category, 170, 130);
            
            txtSearchCode.Focus();
        }

        public int MakeLong(short lowPart, short highPart)
        {
            return (int)(((ushort)lowPart) | (uint)(highPart << 16));
        }

        public void ListView_SetSpacing(ListView listview, short cx, short cy)
        {
            const int LVM_FIRST = 0x1000;
            const int LVM_SETICONSPACING = LVM_FIRST + 53;
            // minimum spacing = 4
            SendMessage(listview.Handle, LVM_SETICONSPACING,
            IntPtr.Zero, (IntPtr)MakeLong(cx, cy));
        }

        private void btnSearchPageLeft_Click(object sender, EventArgs e)
        {
            string strInputText = string.Empty;

            strInputText = txtSearchCode.Text;
            if (strInputText.All(char.IsDigit))      //숫자인지 확인 True 면 숫자, False 면 문자.
                g_SearchKeyInputPLU_Number = true;
            else
                g_SearchKeyInputPLU_Number = false;

            if (strInputText != "" && strInputText.Length >= 2)              // 두자 이상 칠 경우에만 출력 되도록.
            {
                if(g_SearchItemPageNum > 0) {   g_SearchItemPageNum = g_SearchItemPageNum - 1;  }
                else                        {   g_SearchItemPageNum = 0;                        }

                iLSearchImage.Images.Clear();
                lvSearchImage_Item.Clear();

                GetimageListView(strInputText, ImageType.Item);
            }
            else
            {
                if (g_SearchItemPageNum > 0) { g_SearchItemPageNum = g_SearchItemPageNum - 1; }
                else { g_SearchItemPageNum = 0; }

                iLSearchImage.Images.Clear();
                lvSearchImage_Item.Clear();

                GetimageListView(strInputText, ImageType.Item);
            }
            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnSearchPageRight_Click(object sender, EventArgs e)
        {
            string strInputText = string.Empty;

            strInputText = txtSearchCode.Text;
            if (strInputText.All(char.IsDigit))      //숫자인지 확인 True 면 숫자, False 면 문자.
                g_SearchKeyInputPLU_Number = true;
            else
                g_SearchKeyInputPLU_Number = false;

            if (strInputText != "" && strInputText.Length >= 2)              // 두자 이상 칠 경우에만 출력 되도록.
            {
                if (g_SearchItemPageNum < g_SearchItemMaxPageNum) { g_SearchItemPageNum = g_SearchItemPageNum + 1;  }
                else                                               { g_SearchItemPageNum = g_SearchItemMaxPageNum;   }

                iLSearchImage.Images.Clear();
                lvSearchImage_Item.Clear();

                GetimageListView(strInputText, ImageType.Item);
            }
            else
            {
                if (g_SearchItemPageNum < g_SearchItemMaxPageNum) { g_SearchItemPageNum = g_SearchItemPageNum + 1; }
                else { g_SearchItemPageNum = g_SearchItemMaxPageNum; }

                iLSearchImage.Images.Clear();
                lvSearchImage_Item.Clear();

                GetimageListView(strInputText, ImageType.Item);
            }
            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        public bool GetImageDisplaySettingStatus(string strprodId)
        {
            bool bStatus = false;
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

                    return bStatus;
                }

                // 입력된 상품코드(UPC) 검색하여 존재 여부 확인.
                if (GintLocation == 3)                                          // 미국인 경우
                {
                    sQBuff = "SELECT prodCat3 FROM hanamart.dbo.mfProd " +
                             "WHERE prodId = '" + strprodId + "'";
                }
                else if (GintLocation == 2)                          // 토론토 인 경우
                {
                }

                else if (GintLocation == 1)                          // 벤쿠버 인 경우
                {
                    sQBuff = "SELECT prodCat3 FROM hanamart.dbo.mfProd " +
                             "WHERE prodId = '" + strprodId + "'";
                }

                lReturn = c_localdb.RsOpen(sQBuff);

                if (lReturn != 1)
                {
                    g_sMessage = string.Format("[{0}] Data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    c_localdb.DBClose();

                    return bStatus;
                }
                else
                {
                    if (c_localdb.rs.RecordCount == 1)
                    {
                        bStatus = Convert.ToBoolean(c_localdb.rs.Fields["prodCat3"].Value);
                    }
                    else
                    {
                        bStatus = false;
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
                //c_localdb.DBClose();
            }
            return bStatus;
        }

        private void GetProdInfo(string strProdId)
        {
            string strSearchedProdId = string.Empty;
            string strSearchedName = string.Empty;
            double dSearchedPrice = 0;
            string strSearchedTax = string.Empty;

            string sQBuff = string.Empty;
            long lReturn = 0;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            lvSearchImage_Additional.Clear();
            initlvSearchImage_Additional();

            try
            {
                lReturn = c_localdb.DBConnection();

                if (lReturn < 0)
                {
                    g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    return;
                }

                // 입력된 상품코드(UPC) 검색하여 존재 여부 확인.

                if (strProdId != "" && strProdId != "INIT")                                                // 입력 값이 빈것아 아니면,
                {
                    if(g_SearchKeyInputPLU_Number == true)                          // True 면 숫자
                    {
                        //sQBuff = "SELECT prodId, prodName, prodOUprice, prodTax " +
                        // "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.mfPtype ON prodType = pType " +
                        // "WHERE LEN(prodId) = 4 and ptCode = '21' and prodId like '" + strProdId + "%%' " +
                        // "ORDER BY prodId";

                        sQBuff = "SELECT prodId, prodName, prodOUprice, prodTax " +
                         "FROM hanamart.dbo.mfProd " +
                         "WHERE prodId = '" + strProdId + "' " +
                         "ORDER BY prodId";
                    }
                    else                                                            // False 면 문자
                    {
                        //sQBuff = "SELECT prodId, prodName, prodOUprice, prodTax " +
                        // "FROM hanamart.dbo.mfProd LEFT JOIN hanamart.dbo.mfPtype ON prodType = pType " +
                        // "WHERE LEN(prodId) = 4 and ptCode = '21' and prodName like '%%" + strProdId + "%%' " +
                        // "ORDER BY prodId";

                        sQBuff = "SELECT prodId, prodName, prodOUprice, prodTax " +
                         "FROM hanamart.dbo.mfProd " +
                         "WHERE prodName like '%%" + strProdId + "%%' " +
                         "ORDER BY prodId";
                    }
                    
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
                        while (c_localdb.rs.EOF != true)
                        {
                            //if (c_localdb.rs.RecordCount != 0)
                            //{
                            //    strSearchedProdId = Convert.ToString(c_localdb.rs.Fields["prodId"].Value);
                            //    strSearchedName = Convert.ToString(c_localdb.rs.Fields["prodName"].Value);
                            //    dSearchedPrice = Convert.ToDouble(c_localdb.rs.Fields["prodOUprice"].Value);
                            //    strSearchedTax = Convert.ToString(c_localdb.rs.Fields["prodTax"].Value);

                            //    // List View에 항목 추가.
                            //    var prodItem = new ListViewItem(new[] { strSearchedProdId, strSearchedName, String.Format("{0:#,##0.00}", dSearchedPrice, 2), strSearchedTax });
                            //    lvSearchImage_Additional.Items.Add(prodItem);
                            //}
                            //else
                            //{
                            //    g_sMessage = string.Format("[{0}] Searched Items Information is not found (Invoice Number : {1}).", sMethod, txtInvNo.Text);
                            //    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                            //    c_localdb.RsClose();
                            //    return;
                            //}

                            strSearchedProdId = Convert.ToString(c_localdb.rs.Fields["prodId"].Value);
                            strSearchedName = Convert.ToString(c_localdb.rs.Fields["prodName"].Value);
                            dSearchedPrice = Convert.ToDouble(c_localdb.rs.Fields["prodOUprice"].Value);
                            strSearchedTax = Convert.ToString(c_localdb.rs.Fields["prodTax"].Value);

                            // List View에 항목 추가.
                            var prodItem = new ListViewItem(new[] { strSearchedProdId, strSearchedName, String.Format("{0:#,##0.00}", dSearchedPrice, 2), strSearchedTax });
                            lvSearchImage_Additional.Items.Add(prodItem);

                            c_localdb.rs.MoveNext();
                        }

                        if (c_localdb.rs.RecordCount == 0)
                        {
                            var prodItem = new ListViewItem(new[] { "No Search Results.", "", "", "" });
                            lvSearchImage_Additional.Items.Add(prodItem);
                        }
                    }
                }
                else                                                                                    // 입력 값이 빈 것이면.
                {
                    // List View에 항목 추가.
                    var prodItem = new ListViewItem(new[] { "Please input product info.", "", "", ""});
                    lvSearchImage_Additional.Items.Add(prodItem);
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
        }
        private void btnSearchCategory_BC_Click(object sender, EventArgs e)
        {
            pn_Keyboard.Visible = false;
           
            if (g_SearchCategory == true)
            {
                if (g_SearchCategory_Clicked == true)                   // 카테고리 검색에서 카테고리를 누른 상태에서 스피드 키를 눌렀을때.
                {
                    lvSearchImage_Item.BringToFront();
                    lvSearchImage_Category.SendToBack();

                    // Page Number 초기화
                    g_SearchItemPageNum = 0;
                    g_SearchItemMaxPageNum = 0;

                    GetimageListView("B_C", ImageType.Item);
                }
                else
                {
                    iLSearchImage.Images.Clear();
                    lvSearchImage_Item.Clear();
                    lvSearchImage_Category.Clear();

                    lvSearchImage_Item.SendToBack();
                    lvSearchImage_Category.BringToFront();
                    GetimageListView("B_C", ImageType.Category);
                }
            }
            else
            {
                iLSearchImage.Images.Clear();
                lvSearchImage_Item.Clear();
                lvSearchImage_Category.Clear();

                lvSearchImage_Item.BringToFront();
                lvSearchImage_Category.SendToBack();
                GetimageListView("B_C", ImageType.Item);
            }

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }
        
        private void btnSearchCategory_P_Click(object sender, EventArgs e)
        {
            pn_Keyboard.Visible = false;
            
            if (g_SearchCategory == true)
            {
                if (g_SearchCategory_Clicked == true)                   // 카테고리 검색에서 카테고리를 누른 상태에서 스피드 키를 눌렀을때.
                {
                    lvSearchImage_Item.BringToFront();
                    lvSearchImage_Category.SendToBack();

                    // Page Number 초기화
                    g_SearchItemPageNum = 0;
                    g_SearchItemMaxPageNum = 0;

                    GetimageListView("P", ImageType.Item);
                }
                else
                {
                    iLSearchImage.Images.Clear();
                    lvSearchImage_Item.Clear();
                    lvSearchImage_Category.Clear();

                    lvSearchImage_Item.SendToBack();
                    lvSearchImage_Category.BringToFront();
                    GetimageListView("P", ImageType.Category);
                }
            }
            else
            {
                iLSearchImage.Images.Clear();
                lvSearchImage_Item.Clear();
                lvSearchImage_Category.Clear();

                lvSearchImage_Item.BringToFront();
                lvSearchImage_Category.SendToBack();
                GetimageListView("P", ImageType.Item);
            }

            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnSearchCategory_DJ_Click(object sender, EventArgs e)
        {
            pn_Keyboard.Visible = false;
            
            if (g_SearchCategory == true)
            {
                if (g_SearchCategory_Clicked == true)                   // 카테고리 검색에서 카테고리를 누른 상태에서 스피드 키를 눌렀을때.
                {
                    lvSearchImage_Item.BringToFront();
                    lvSearchImage_Category.SendToBack();

                    // Page Number 초기화
                    g_SearchItemPageNum = 0;
                    g_SearchItemMaxPageNum = 0;

                    GetimageListView("D_E_F_G_H_I_J", ImageType.Item);
                }
                else
                {
                    iLSearchImage.Images.Clear();
                    lvSearchImage_Item.Clear();
                    lvSearchImage_Category.Clear();

                    lvSearchImage_Item.SendToBack();
                    lvSearchImage_Category.BringToFront();
                    GetimageListView("D_E_F_G_H_I_J", ImageType.Category);
                }
            }
            else
            {
                iLSearchImage.Images.Clear();
                lvSearchImage_Item.Clear();
                lvSearchImage_Category.Clear();

                lvSearchImage_Item.BringToFront();
                lvSearchImage_Category.SendToBack();
                GetimageListView("D_E_F_G_H_I_J", ImageType.Item);
            }
            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnSearchCategory_KM_Click(object sender, EventArgs e)
        {
            pn_Keyboard.Visible = false;
            
            if (g_SearchCategory == true)
            {
                if (g_SearchCategory_Clicked == true)                   // 카테고리 검색에서 카테고리를 누른 상태에서 스피드 키를 눌렀을때.
                {
                    lvSearchImage_Item.BringToFront();
                    lvSearchImage_Category.SendToBack();

                    // Page Number 초기화
                    g_SearchItemPageNum = 0;
                    g_SearchItemMaxPageNum = 0;

                    GetimageListView("K_L_M", ImageType.Item);
                }
                else
                {
                    iLSearchImage.Images.Clear();
                    lvSearchImage_Item.Clear();
                    lvSearchImage_Category.Clear();

                    lvSearchImage_Item.SendToBack();
                    lvSearchImage_Category.BringToFront();
                    GetimageListView("K_L_M", ImageType.Category);
                }
            }
            else
            {
                iLSearchImage.Images.Clear();
                lvSearchImage_Item.Clear();
                lvSearchImage_Category.Clear();

                lvSearchImage_Item.BringToFront();
                lvSearchImage_Category.SendToBack();
                GetimageListView("K_L_M", ImageType.Item);
            }
            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnSearchCategory_NO_Click(object sender, EventArgs e)
        {
            pn_Keyboard.Visible = false;
            
            if (g_SearchCategory == true)
            {
                if (g_SearchCategory_Clicked == true)                   // 카테고리 검색에서 카테고리를 누른 상태에서 스피드 키를 눌렀을때.
                {
                    lvSearchImage_Item.BringToFront();
                    lvSearchImage_Category.SendToBack();

                    // Page Number 초기화
                    g_SearchItemPageNum = 0;
                    g_SearchItemMaxPageNum = 0;

                    GetimageListView("N_O", ImageType.Item);
                }
                else
                {
                    iLSearchImage.Images.Clear();
                    lvSearchImage_Item.Clear();
                    lvSearchImage_Category.Clear();

                    lvSearchImage_Item.SendToBack();
                    lvSearchImage_Category.BringToFront();
                    GetimageListView("N_O", ImageType.Category);
                }
            }
            else
            {
                iLSearchImage.Images.Clear();
                lvSearchImage_Item.Clear();
                lvSearchImage_Category.Clear();

                lvSearchImage_Item.BringToFront();
                lvSearchImage_Category.SendToBack();
                GetimageListView("N_O", ImageType.Item);
            }
            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnSearchCategory_QS_Click(object sender, EventArgs e)
        {
            pn_Keyboard.Visible = false;
            
            if (g_SearchCategory == true)
            {
                if (g_SearchCategory_Clicked == true)                   // 카테고리 검색에서 카테고리를 누른 상태에서 스피드 키를 눌렀을때.
                {
                    lvSearchImage_Item.BringToFront();
                    lvSearchImage_Category.SendToBack();

                    // Page Number 초기화
                    g_SearchItemPageNum = 0;
                    g_SearchItemMaxPageNum = 0;

                    GetimageListView("Q_R_S", ImageType.Item);
                }
                else
                {
                    iLSearchImage.Images.Clear();
                    lvSearchImage_Item.Clear();
                    lvSearchImage_Category.Clear();

                    lvSearchImage_Item.SendToBack();
                    lvSearchImage_Category.BringToFront();
                    GetimageListView("Q_R_S", ImageType.Category);
                }
            }
            else
            {
                iLSearchImage.Images.Clear();
                lvSearchImage_Item.Clear();
                lvSearchImage_Category.Clear();

                lvSearchImage_Item.BringToFront();
                lvSearchImage_Category.SendToBack();
                GetimageListView("Q_R_S", ImageType.Item);
            }
            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnSearchCategory_TZ_Click(object sender, EventArgs e)
        {
            pn_Keyboard.Visible = false;
            
            if (g_SearchCategory == true)
            {
                if (g_SearchCategory_Clicked == true)                   // 카테고리 검색에서 카테고리를 누른 상태에서 스피드 키를 눌렀을때.
                {
                    lvSearchImage_Item.BringToFront();
                    lvSearchImage_Category.SendToBack();

                    // Page Number 초기화
                    g_SearchItemPageNum = 0;
                    g_SearchItemMaxPageNum = 0;

                    GetimageListView("T_U_V_W_X_Y_Z", ImageType.Item);
                }
                else
                {
                    iLSearchImage.Images.Clear();
                    lvSearchImage_Item.Clear();
                    lvSearchImage_Category.Clear();

                    lvSearchImage_Item.SendToBack();
                    lvSearchImage_Category.BringToFront();
                    GetimageListView("T_U_V_W_X_Y_Z", ImageType.Category);
                }
            }
            else
            {
                iLSearchImage.Images.Clear();
                lvSearchImage_Item.Clear();
                lvSearchImage_Category.Clear();

                lvSearchImage_Item.BringToFront();
                lvSearchImage_Category.SendToBack();
                GetimageListView("T_U_V_W_X_Y_Z", ImageType.Item);
            }
            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void txtSearchCode_TextChanged(object sender, EventArgs e)
        {
            string strInputText = string.Empty;
            
            strInputText = txtSearchCode.Text;
            if (strInputText.All(char.IsDigit))      //숫자인지 확인 True 면 숫자, False 면 문자.
                g_SearchKeyInputPLU_Number = true;
            else
                g_SearchKeyInputPLU_Number = false;
                      
            if (g_SearchCategory == true)
            {
                if(g_SearchCategory_Clicked == true)
                {
                    //lvSearchImage_Item.BringToFront();
                    //lvSearchImage_Category.BringToFront();
                    GetimageListView(strInputText, ImageType.Item);
                }
                else
                {
                    iLSearchImage.Images.Clear();
                    lvSearchImage_Item.Clear();
                    lvSearchImage_Category.Clear();
                    lvSearchImage_Additional.Clear();

                    //lvSearchImage_Item.SendToBack();
                    //lvSearchImage_Category.BringToFront();
                    GetimageListView(strInputText, ImageType.Category);
                }
            }
            else
            {
                iLSearchImage.Images.Clear();
                lvSearchImage_Item.Clear();
                lvSearchImage_Category.Clear();
                //lvSearchImage_Additional.Clear();

                // Page Number 초기화
                g_SearchItemPageNum = 0;
                g_SearchItemMaxPageNum = 0;

                if (g_SearchAdditional == true)                         // Additional Search 버튼 눌러진 상태.
                {
                    //if (strInputText != "" && strInputText.Length >= 2)              // 두자 이상 칠 경우에만 출력 되도록.
                    //{
                    //    GetimageListView(strInputText, ImageType.Additional_Data);
                    //}
                    //else
                    //{
                    //    GetimageListView("INIT", ImageType.Additional_Data);
                    //}

                    if (strInputText == "" )              // 없으면 INIT 멘트 출력 되도록.
                    {
                        GetimageListView("INIT", ImageType.Additional_Data);
                    }
                }
                else                                                    // Search Or Key in 버튼 눌러진 상태.
                {
                    //lvSearchImage_Item.BringToFront();
                    lvSearchImage_Category.SendToBack();
                    if (strInputText != "" && strInputText.Length >= 2)              // 두자 이상 칠 경우에만 출력 되도록.
                    {
                        GetimageListView(strInputText, ImageType.Item);
                    }
                    else                                // 공백이 들어올 경우 초기 화면으로 출력되도록..
                    {
                        GetimageListView("INIT", ImageType.Item);
                    }
                }                
            }
            pn_Keyboard.Visible = true;
            pn_Keyboard.BringToFront();
        }

        private void lvSearchImage_MouseClick(object sender, MouseEventArgs e)
        {
            if (GssSearchItem.GetCurrentlySpokenPrompt() != null)                // 음성 중복 재생 방지.
            {
                GssSearchItem.SpeakAsyncCancel(GssSearchItem.GetCurrentlySpokenPrompt());
            }

            if (lvSearchImage_Item.SelectedItems.Count == 1)
            {
                ProcessAddSearchScaleItem();
            }
            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }
        private void lvSearchImage_Category_MouseClick(object sender, MouseEventArgs e)
        {
            string strCategoryNumber = string.Empty;
            ListView.SelectedListViewItemCollection items = lvSearchImage_Category.SelectedItems;
            if (items.Count < 1)
            {
                g_sMessage = "아이템을 찾을 수 없습니다.[1]";
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                iLSearchImage.Images.Clear();
                lvSearchImage_Item.Clear();
                lvSearchImage_Category.Clear();

                lvSearchImage_Category.SendToBack();

                changeToItemScan();

                //list.AddRange(Directory.GetFiles(Application.StartupPath + "\\Image\\", "*_*_*_" + strSplitChar + "*.*", SearchOption.TopDirectoryOnly)); // 경로 변경 필요. 프로젝트내부로 이동

                return;
            }
            ListViewItem lvItem = items[0];

            //string strSelectedCategoryName = string.Empty;
            string strSelectedCategoryName = lvItem.Text;       // 선택된 카테고리의 번호 받기.

            if (GssSearchItem.GetCurrentlySpokenPrompt() != null)                // 음성 중복 재생 방지.
            {
                GssSearchItem.SpeakAsyncCancel(GssSearchItem.GetCurrentlySpokenPrompt());
            }

            if (lvSearchImage_Category.SelectedItems.Count == 1)
            {
                txtSearchCode.Clear();
                g_SearchCategory_Clicked = true;
                g_Temp_lvItem = strSelectedCategoryName;

                // Item 출력되는 창에서 해당 아이템 조회해서 출력하기.
                iLSearchImage.Images.Clear();
                lvSearchImage_Item.Clear();
                //lvSearchImage_Category.Clear();

                // Item Image List 앞으로 나옴
                lvSearchImage_Item.BringToFront();
                lvSearchImage_Category.SendToBack();
                pn_Keyboard.SendToBack();
                pn_Keyboard.Visible = false;
                g_SearchKeyInputPLU_Number = false;
                
                btnSearchOrKeyinItem.Text = "Search Or Key in Item";
                btnBackToCategory.Text = "Back to Category";
                //g_SearchCategory = false;

                // 카테고리 이름으로 카테고리 번호 가져오기.
                strCategoryNumber = GetCategoryNumberInSearchImage(strSelectedCategoryName);          // 카테고리 이름으로 카테고리 번호 가져오기.
                g_SearchCategory_Clicked = true;
                GetimageListView(strCategoryNumber, ImageType.Item);
            }
            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void lvSearchImage_Additional_MouseClick(object sender, MouseEventArgs e)
        {
            if (GssSearchItem.GetCurrentlySpokenPrompt() != null)                // 음성 중복 재생 방지.
            {
                GssSearchItem.SpeakAsyncCancel(GssSearchItem.GetCurrentlySpokenPrompt());
            }

            if (lvSearchImage_Additional.SelectedItems.Count == 1)
            {
                ProcessAddSearchScaleItem();
            }
            //Timer Stop & Start
            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }


        private void ProcessAddSearchScaleItem()
        {
            string strProdUnit = string.Empty;
            double dblWeight = 0;
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            ListView.SelectedListViewItemCollection items;
            
            if(g_SearchAdditional == true)
            {
                items = lvSearchImage_Additional.SelectedItems;
            }
            else
            {
                items = lvSearchImage_Item.SelectedItems;
            }
            
            if (items.Count < 1)
            {
                g_sMessage = "아이템을 찾을 수 없습니다.[2]";
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                iLSearchImage.Images.Clear();
                lvSearchImage_Item.Clear();
                lvSearchImage_Category.Clear();

                lvSearchImage_Category.SendToBack();

                changeToItemScan();

                //list.AddRange(Directory.GetFiles(Application.StartupPath + "\\Image\\", "*_*_*_" + strSplitChar + "*.*", SearchOption.TopDirectoryOnly)); // 경로 변경 필요. 프로젝트내부로 이동

                return;
            }
            ListViewItem lvItem = items[0];
            string strSelectedPLU = string.Empty;
            string strSelectedItem = lvItem.Text;       // 선택된 아이템의 이름 받기.
            string strSelectedProdID = string.Empty;

            if (g_SearchAdditional == true)
            {
                strSelectedProdID = strSelectedItem.Substring(0);     // ProdID 
                // 갯수 상품인 경우 갯수 입력 할 수 있는 창 출력.
                strProdUnit = GetProdUnit(strSelectedProdID);          // Prod Unit 값 미리 가져오기.
            }
            else
            {
                strSelectedPLU = strSelectedItem.Substring(0, strSelectedItem.IndexOf(' '));     // PLU 
                // 갯수 상품인 경우 갯수 입력 할 수 있는 창 출력.
                strProdUnit = GetProdUnit(GetProdIdInSearchImage(strSelectedPLU));          // Prod Unit 값 미리 가져오기.
            }
                        
            //if (strProdUnit != "PK" && strProdUnit != "EA")                              // LB, KG 상품
            if (strProdUnit == "LB" || strProdUnit == "KG")                              // LB, KG 상품
            {
                dblWeight = Convert.ToDouble(lbWeightCS.Text);
                //dblWeight = 0.69;       // test

                if (dblWeight != 0)  // 무게를 올려 놓은 경우
                {
                    if (g_SearchAdditional == true)
                    {
                        ProcessItemSale(strSelectedProdID, dblWeight);                         // 아이템 등록
                    }
                    else
                    {
                        ProcessItemSale(GetProdIdInSearchImage(strSelectedPLU), dblWeight);                         // 아이템 등록
                    }

                    ctrlOnScreen = pn_ItemScan;

                    txtSearchCode.Clear();
                    //gbSearchBox.Visible = false;
                    lvSearchImage_Item.Enabled = true;
                    lvSearchImage_Category.Enabled = true;
                    lvSearchImage_Additional.Enabled = true;
                    pn_Keyboard.Enabled = true;

                    btnSearch.Enabled = true;
                    btnSearch_Category.Enabled = true;
                    btnSearchOrKeyinItem.Enabled = true;
                    btnBackToCategory.Enabled = true;
                    btnSearchCategory_TZ.Enabled = true;
                    btnSearchCategory_QS.Enabled = true;
                    btnSearchCategory_P.Enabled = true;
                    btnSearchCategory_NO.Enabled = true;
                    btnSearchCategory_KM.Enabled = true;
                    btnSearchCategory_DJ.Enabled = true;
                    btnSearchCategory_BC.Enabled = true;
                    btnSearchCategory_A.Enabled = true;
                    btnSearchCategory_All.Enabled = true;
                    btnSearchExit.Enabled = true;
                    btnReprint.Enabled = true;
                    btnSuspend.Enabled = true;
                    btnItemCorrect.Enabled = true;
                    btnVoid.Enabled = true;
                    btnItemDiscount.Enabled = true;
                    btnBack.Enabled = true;

                    if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
                    {
                        btnNext.Enabled = false;
                    }
                    else
                    {
                        btnNext.Enabled = true;
                    }
                    
                    iLSearchImage.Images.Clear();
                    lvSearchImage_Item.Clear();
                    lvSearchImage_Category.Clear();
                    lvSearchImage_Additional.Clear();

                    g_SearchAdditional = false;

                    g_SearchCategory = false;
                    g_SearchCategory_Clicked = false;

                    if (OPOSScanner.DeviceEnabled == false)
                    {
                        _EnableScannerDevice();
                    }

                    KeyInReady();

                    //if (OPOSScanner.DeviceEnabled == false)
                    //{
                    //    // OPOS Scanner Start
                    //    if (_OpenScannerDevice() != OPOS_RESPONSE.OPOS_SUCCESS)
                    //        return;

                    //    if (_ClaimScannerDevice() != OPOS_RESPONSE.OPOS_SUCCESS)
                    //        return;

                    //    if (_EnableScannerDevice() != OPOS_RESPONSE.OPOS_SUCCESS)
                    //        return;
                    //}
                }
                else            // 무게를 올려 놓지 않은 경우
                {
                    DisplayErrorMessageBox("Notice", "Please put your Items on the scale. \n\n Do you want to retry? ", 2, sMethod);

                    if (g_SearchAdditional == true)
                    {
                        g_sMessage = string.Format("[{0}] Scale Weight Data Error (error code: {1})\n[{0}] {2}", sMethod, strSelectedProdID, c_localdb.error_message);
                    }
                    else
                    {
                        g_sMessage = string.Format("[{0}] Scale Weight Data Error (error code: {1})\n[{0}] {2}", sMethod, GetProdIdInSearchImage(strSelectedPLU), c_localdb.error_message);
                    }
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    lvSearchImage_Item.Enabled = false;
                    lvSearchImage_Category.Enabled = false;
                    lvSearchImage_Additional.Enabled = false;
                    pn_Keyboard.Enabled = false;
                    btnSearchOrKeyinItem.Enabled = false;
                    btnBackToCategory.Enabled = false;
                    btnSearchCategory_TZ.Enabled = false;
                    btnSearchCategory_QS.Enabled = false;
                    btnSearchCategory_P.Enabled = false;
                    btnSearchCategory_NO.Enabled = false;
                    btnSearchCategory_KM.Enabled = false;
                    btnSearchCategory_DJ.Enabled = false;
                    btnSearchCategory_BC.Enabled = false;
                    btnSearchCategory_A.Enabled = false;
                    btnSearchCategory_All.Enabled = false;
                    btnSearchExit.Enabled = false;

                    //Timer Stop & Start
                    BackToStartTimerFromItemScan.Stop();
                    BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
                    BackToStartTimerFromItemScan.Start();
                }
            }
            else if (strProdUnit == "PK" || strProdUnit == "BAG" || strProdUnit == "EA" || strProdUnit == "BOX" || strProdUnit == "BN")                                        // LB, KG가 아닌경우 PK,BAG,EA, BN 상품인 경우.
            {
                // 상품 갯수 추가 하는 창 출력
                ctrTemp = ctrlOnScreen;
                transitionSiglePage(gbSearchItemAddCount, 284, 200);

                lvSearchImage_Item.Enabled = false;
                lvSearchImage_Category.Enabled = false;
                lvSearchImage_Additional.Enabled = false;
                pn_Keyboard.Enabled = false;
                btnSearchOrKeyinItem.Enabled = false;
                btnBackToCategory.Enabled = false;
                btnSearchCategory_TZ.Enabled = false;
                btnSearchCategory_QS.Enabled = false;
                btnSearchCategory_P.Enabled = false;
                btnSearchCategory_NO.Enabled = false;
                btnSearchCategory_KM.Enabled = false;
                btnSearchCategory_DJ.Enabled = false;
                btnSearchCategory_BC.Enabled = false;
                btnSearchCategory_A.Enabled = false;
                btnSearchCategory_All.Enabled = false;
                btnSearchExit.Enabled = false;
            }
            else if (strProdUnit == "")
            {
                DisplayErrorMessageBox("Notice", "Please Check Product Information. There is no Unit Per Sale Information.", 1, sMethod);

                g_sMessage = string.Format("[{0}] No Product Unit Information. (Item code: {1})\n[{0}] {2}", sMethod, GetProdIdInSearchImage(strSelectedPLU), c_localdb.error_message);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                txtSearchCode.Clear();
                lvSearchImage_Item.Enabled = false;
                lvSearchImage_Category.Enabled = false;
                lvSearchImage_Additional.Enabled = false;
                pn_Keyboard.Enabled = false;
                btnSearchOrKeyinItem.Enabled = false;
                btnBackToCategory.Enabled = false;
                btnSearchCategory_TZ.Enabled = false;
                btnSearchCategory_QS.Enabled = false;
                btnSearchCategory_P.Enabled = false;
                btnSearchCategory_NO.Enabled = false;
                btnSearchCategory_KM.Enabled = false;
                btnSearchCategory_DJ.Enabled = false;
                btnSearchCategory_BC.Enabled = false;
                btnSearchCategory_A.Enabled = false;
                btnSearchCategory_All.Enabled = false;
                btnSearchExit.Enabled = false;

                //Timer Stop & Start
                BackToStartTimerFromItemScan.Stop();
                BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
                BackToStartTimerFromItemScan.Start();
            }
        }
        
        private string GetProdUnit(string strProdId)
        {
            string strProdUnit = string.Empty;
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

                    return strProdUnit;
                }

                // 입력된 상품코드(UPC) 검색하여 존재 여부 확인.

                if(strProdId != "")
                {
                    sQBuff = "SELECT prodUnit " +
                         "FROM hanamart.dbo.mfProd " +
                         "WHERE prodId = '" + strProdId + "'";

                    lReturn = c_localdb.RsOpen(sQBuff);

                    if (lReturn != 1)
                    {
                        g_sMessage = string.Format("[{0}] Data query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                        c_localdb.DBClose();

                        return strProdUnit;
                    }
                    else
                    {
                        if (c_localdb.rs.RecordCount == 1)
                        {
                            strProdUnit = Convert.ToString(c_localdb.rs.Fields["prodUnit"].Value);
                        }
                        else
                        {
                            DisplayErrorMessageBox("ITEM SCAN", "Cannot find the product.\nPlease ask for help. \n\n Barcode : " + strProdId, 1, sMethod);

                            g_sMessage = string.Format("[{0}] Product not found (Item code: {1}).", sMethod, strProdId);
                            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                            c_localdb.RsClose();

                            return strProdUnit;
                        }
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

            return strProdUnit;
        }

        private string GetCategoryNumberInSearchImage(string strSelectedCategoryName)
        {
            string[] imgFiles;
            string strTemp;
            string strCategoryNumber = string.Empty;
            // 숫자 PLU 검색
            if(GintLocation == 3)                   // 미국 인 경우
                imgFiles = Directory.GetFiles(Application.StartupPath + "\\Image\\Category\\", "*_" + strSelectedCategoryName + "*.jpg", SearchOption.AllDirectories); // 경로 변경 필요. 프로젝트내부로 이동
            else
                imgFiles = Directory.GetFiles(Application.StartupPath + "\\Image\\Category\\", "*_" + strSelectedCategoryName + "*.png", SearchOption.AllDirectories); // 경로 변경 필요. 프로젝트내부로 이동

            for (int i = 0; i < imgFiles.Count(); i++)
            {
                strTemp = imgFiles[i].ToString();
                strTemp = strTemp.Substring(strTemp.LastIndexOf('\\') + 1);
                strTemp = strTemp.Substring(0, strTemp.LastIndexOf('.'));

                strCategoryNumber = strTemp.Substring(0, strTemp.IndexOf('_'));         //Prod ID 
            }
            return strCategoryNumber;
        }
        private void btnSearchItemCountMinus_Click(object sender, EventArgs e)
        {
            int tempCount = 0;
            tempCount = int.Parse(lbSearchItemAddCount.Text);
            tempCount -= 1;
            if (tempCount <= 0) { tempCount = 1; }

            lbSearchItemAddCount.Text = tempCount.ToString();
        }

        private void btnSearchItemCountPlus_Click(object sender, EventArgs e)
        {
            int tempCount = 0;
            tempCount = int.Parse(lbSearchItemAddCount.Text);
            if (tempCount == 99)
            {
                tempCount = 1;
            }
            else
            {
                tempCount += 1;
            }
            lbSearchItemAddCount.Text = tempCount.ToString();
        }

        private void btnSearchItemCountConFirm_Click(object sender, EventArgs e)
        {
            if (GssSearchItem.GetCurrentlySpokenPrompt() != null)                // 음성 중복 재생 방지.
            {
                GssSearchItem.SpeakAsyncCancel(GssSearchItem.GetCurrentlySpokenPrompt());
            }

            ListView.SelectedListViewItemCollection items;
            if (g_SearchAdditional == true)
            {
                items = lvSearchImage_Additional.SelectedItems;
            }
            else
            {
                items = lvSearchImage_Item.SelectedItems;
            }
            
            if (items.Count < 1)
            {
                g_sMessage = "아이템을 찾을 수 없습니다.[3]";
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                iLSearchImage.Images.Clear();
                lvSearchImage_Item.Clear();
                lvSearchImage_Category.Clear();

                lvSearchImage_Category.SendToBack();

                changeToItemScan();

                //list.AddRange(Directory.GetFiles(Application.StartupPath + "\\Image\\", "*_*_*_" + strSplitChar + "*.*", SearchOption.TopDirectoryOnly)); // 경로 변경 필요. 프로젝트내부로 이동

                return;
            }

            ListViewItem lvItem = items[0];
            string strSelectedPLU = string.Empty;
            string strSelectedItem = lvItem.Text;       // 선택된 아이템의 이름 받기.
            string strSelectedProdID = string.Empty;

            if(g_SearchAdditional == true)
            {
                strSelectedProdID = strSelectedItem.Substring(0);     // ProdID 
            }
            else
            {
                strSelectedPLU = strSelectedItem.Substring(0, strSelectedItem.IndexOf(' '));     // PLU 
            }
            
            transitionSiglePage(gbSearchItemAddCount, 1023, 200);
            changeToItemScan();
            //ctrlOnScreen = pn_ItemScan;  // 강제로 페널 입력

            g_SearchCategory = false;
            g_SearchCategory_Clicked = false;

            if (g_SearchAdditional == true)
            {
                gbSearchBox.Visible = false;
                ProcessItemSale(strSelectedProdID, Convert.ToDouble(lbSearchItemAddCount.Text));                         // 아이템 등록
            }
            else
            {
                ProcessItemSale(GetProdIdInSearchImage(strSelectedPLU), Convert.ToDouble(lbSearchItemAddCount.Text));                         // 아이템 등록
            }

            txtSearchCode.Clear();
            //gbSearchBox.Visible = false;

            lvSearchImage_Item.Enabled = true;
            lvSearchImage_Category.Enabled = true;
            lvSearchImage_Additional.Enabled = true;

            g_SearchAdditional = false;

            pn_Keyboard.Enabled = true;
            btnSearchOrKeyinItem.Enabled = true;
            btnBackToCategory.Enabled = true;
            btnSearchCategory_TZ.Enabled = true;
            btnSearchCategory_QS.Enabled = true;
            btnSearchCategory_P.Enabled = true;
            btnSearchCategory_NO.Enabled = true;
            btnSearchCategory_KM.Enabled = true;
            btnSearchCategory_DJ.Enabled = true;
            btnSearchCategory_BC.Enabled = true;
            btnSearchCategory_A.Enabled = true;
            btnSearchCategory_All.Enabled = true;
            btnSearchExit.Enabled = true;

            btnReprint.Enabled = true;
            btnSuspend.Enabled = true;
            btnItemCorrect.Enabled = true;
            btnVoid.Enabled = true;
            btnItemDiscount.Enabled = true;

            iLSearchImage.Images.Clear();
            lvSearchImage_Item.Clear();

            if(g_HelpModeOn == true)
            {
                btnBack.Enabled = true;
                btnSearch.Enabled = true;
                btnSearch_Category.Enabled = true;
                txtNumCS.Enabled = true;
                btnReprint.Enabled = true;
                btnSuspend.Enabled = true;
            }
            if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
            {
                btnNext.Enabled = false;
            }
            else
            {
                btnNext.Enabled = true;
            }

            lbSearchItemAddCount.Text = "1";

            if (OPOSScanner.DeviceEnabled == false)
            {
                _EnableScannerDevice();
            }

            KeyInReady();

            //if (OPOSScanner.DeviceEnabled == false)
            //{
            //    // OPOS Scanner Start
            //    if (_OpenScannerDevice() != OPOS_RESPONSE.OPOS_SUCCESS)
            //        return;

            //    if (_ClaimScannerDevice() != OPOS_RESPONSE.OPOS_SUCCESS)
            //        return;

            //    if (_EnableScannerDevice() != OPOS_RESPONSE.OPOS_SUCCESS)
            //        return;
            //}
        }
        private void btnSearchItemAddCountExit_Click(object sender, EventArgs e)
        {
            transitionSiglePage(gbSearchItemAddCount, 1023, 200);
            ctrlOnScreen = ctrTemp;
            // Focus Search 
            lvSearchImage_Item.Enabled = true;
            lvSearchImage_Category.Enabled = true;
            lvSearchImage_Additional.Enabled = true;
            pn_Keyboard.Enabled = true;
            btnSearchOrKeyinItem.Enabled = true;
            btnBackToCategory.Enabled = true;
            btnSearchCategory_TZ.Enabled = true;
            btnSearchCategory_QS.Enabled = true;
            btnSearchCategory_P.Enabled = true;
            btnSearchCategory_NO.Enabled = true;
            btnSearchCategory_KM.Enabled = true;
            btnSearchCategory_DJ.Enabled = true;
            btnSearchCategory_BC.Enabled = true;
            btnSearchCategory_A.Enabled = true;
            btnSearchCategory_All.Enabled = true;
            btnSearchExit.Enabled = true;

        }

        private string GetProdIdInSearchImage(string strPLU)
        {
            string[] imgFiles;
            string strTemp;
            string strprodId = string.Empty;
            // 숫자 PLU 검색
            imgFiles = Directory.GetFiles(Application.StartupPath + "\\Image\\", "*_*_" + strPLU + "_*.*", SearchOption.AllDirectories); // 경로 변경 필요. 프로젝트내부로 이동
            
            for(int i = 0; i < imgFiles.Count(); i++)
            {
                strTemp = imgFiles[i].ToString();
                strTemp = strTemp.Substring(strTemp.LastIndexOf('\\') + 1);
                strTemp = strTemp.Substring(0, strTemp.LastIndexOf('.'));
                strTemp = strTemp.Substring(strTemp.IndexOf('_') + 1);

                strprodId = strTemp.Substring(0, strTemp.IndexOf('_'));         //Prod ID 
            }
            return strprodId;
        }

        private void btnMessageBoxBack_Click(object sender, EventArgs e)
        {
            if (GssMessageBox.GetCurrentlySpokenPrompt() != null)                // 음성 중복 재생 방지.
            {
                GssMessageBox.SpeakAsyncCancel(GssMessageBox.GetCurrentlySpokenPrompt());
            }

            btnSearch.Enabled = true;
            btnSearch_Category.Enabled = true;

            if (g_HelpModeOn == true)
            {
                btnHelp.Enabled = false;                
            }
            else
            {
                btnHelp.Enabled = true;
            }
            if(ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
            {
                btnNext.Enabled = false;
            }
            else
            {
                btnNext.Enabled = true;
            }

            btnBack.Enabled = true;

            btnItemCorrect.Enabled = true;
            btnVoid.Enabled = true;
            btnItemDiscount.Enabled = true;
            btnReprint.Enabled = true;
            btnSuspend.Enabled = true;
            btnManualETCKey.Enabled = true;

            KeyInReady();

            transitionSiglePage(gbMessageBox, 1023, 200);
            ctrlOnScreen = ctrTemp;

            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }

        private void btnMessageBoxYes_Click(object sender, EventArgs e)
        {
            if (GssMessageBox.GetCurrentlySpokenPrompt() != null)                // 음성 중복 재생 방지.
            {
                GssMessageBox.SpeakAsyncCancel(GssMessageBox.GetCurrentlySpokenPrompt());
            }

            BackToStartTimerFromItemScan.Stop();
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();

            gbProcessCreditCard.Visible = false;

            transitionSiglePage(gbMessageBox, 1023, 200);
            ctrlOnScreen = ctrTemp;

            if (c_poscominfo.ci_mkno != "86")                                       // 86 West Jordan 매장이 아닌 경우
            {
                transitionDoublePage(pnAddBag, pnItemScanSearchBtn, 584, 1 * GROUP_BOX_LEFT, 300);
                ctrlOnScreen = pnAddBag;

                tbStep1Name.ForeColor = System.Drawing.Color.Silver;
                tbStep2Name.ForeColor = System.Drawing.Color.DimGray;
                st_ProcessStatus.CurrentStep = 2;
                //btnNext.Text = "PAYMENT";
                //btnNext.Font = new System.Drawing.Font("Helvetica85-Heavy", 15F, FontStyle.Bold);
                
                // 음성 실행.
                GssHowManyUseBag.SelectVoice(GstrVoice);
                if (GintLocation == 1)               // 벤쿠버
                {
                    GssHowManyUseBag.SpeakAsync("How many bags did you use? ");
                }
                else if (GintLocation == 2)          // 토론토
                {
                    GssHowManyUseBag.SpeakAsync("How many bags did you use? ");
                }
                else                                // 미국
                {
                    GssHowManyUseBag.SpeakAsync("How many bags did you use? ");
                }
            }
            else                                                                                // 86 West Jordan 매장인 경우
            {
                transitionDoublePage(pnSelectPayment, pnItemScanSearchBtn, 584, 1 * GROUP_BOX_LEFT, 300);
                ctrlOnScreen = pnSelectPayment;
                
                ProcessAddtoBagToSelectPayment();
            }

            btnMinus.Enabled = true;
            btnBagPlus.Enabled = true;
            btnAddBagToCart.Enabled = true;
            btnNoBag.Enabled = true;
            btnSearch.Enabled = true;
            btnSearch_Category.Enabled = true;

            if (GintLocation == 1 || GintLocation == 3)               // 벤쿠버, 미국인 경우
            {
                btnBack.Visible = true;
                btnBack.Enabled = true;
            }
            else                                // Pn_Item Scan 이후에 미국이 아닌 경우 저울 스캔되는 거 방지.
            {
                // 저울에서 스캔 되는거 방지.
                if (OPOSScanner.DeviceEnabled)
                {
                    OPOSScanner.DataEvent -= ScannerDataEvent;
                    OPOSScanner.DeviceEnabled = false;
                }
            }

            if (g_HelpModeOn == true)
            {
                btnHelp.Enabled = false;
                txtNumCS.Enabled = true;

                btnItemCorrect.Enabled = true;
                btnVoid.Enabled = true;
                btnItemDiscount.Enabled = true;
                btnReprint.Enabled = true;
                btnSuspend.Enabled = true;
                btnManualETCKey.Enabled = true;

                KeyInReady();
            }
            else
            {
                btnManualETCKey.Enabled = true;

                btnHelp.Enabled = true;
                btnHelp.BringToFront();

                if (GintLocation != 3)               // 미국이 아닌 경우
                {
                    txtNumCS.Enabled = false;       // 아이템 스캔 방지.
                }
                else
                {
                    txtNumCS.Enabled = true;
                    KeyInReady();
                }
            }
            //btnNext.Enabled = true;
            btnNext.Visible = false;
            btnBack.Image = global::HanaSales_SelfCheckOut.Properties.Resources.new_Back;

            lbBagCount.Text = "1";
            gbHelp.Visible = false;

            //// 저울에서 스캔 되는거 방지.
            //if (OPOSScanner.DeviceEnabled)
            //{
            //    OPOSScanner.DataEvent -= ScannerDataEvent;
            //    OPOSScanner.DeviceEnabled = false;
            //}
            //// Scale Scanner Disable
            //if (OPOSScanner.DeviceEnabled)
            //{
            //    OPOSScanner.DataEvent -= ScannerDataEvent;

            //    OPOSScanner.DeviceEnabled = false;
            //    OPOSScanner.ReleaseDevice();
            //    OPOSScanner.Close();
            //}            
        }

        private void PrtCardReceipt(string pReceiptText)
        {
            // 임시로 화면에 표시
            //MessageBox.Show(pReceiptText);
        }
        
        private void calcPaymentTotal()
        {
            double dblTotal = 0;
            dblTotal = Convert.ToDouble(lblPayCash.Text);
            dblTotal += Convert.ToDouble(lblPayDebit.Text);
            dblTotal += Convert.ToDouble(lblPayCredit.Text);
            dblTotal += Convert.ToDouble(lblPayGiftCard.Text);
            dblTotal += Convert.ToDouble(lblPayCerti.Text);
            dblTotal += Convert.ToDouble(lblPayWechat.Text);
            dblTotal += Convert.ToDouble(lblPayHmoney.Text);
            dblTotal += Convert.ToDouble(lblPayCheck.Text);
            dblTotal += Convert.ToDouble(lblPayEBT.Text);
            dblTotal += Convert.ToDouble(lblPayCoupon.Text);
            dblTotal += Convert.ToDouble(lblPayETC.Text);
            //dblTotal += Convert.ToDouble(lblPayPennyRounded.Text);
            lblPayTotal.Text = c_poscomlibs.getDoubleFormat(dblTotal);
            lblPayBalance.Text             = c_poscomlibs.getDoubleFormat(Convert.ToDouble(lbTotalValCS.Text) - dblTotal + Convert.ToDouble(lblPayPennyRounded.Text));
            lbSelectPaymentBalanceNum.Text = c_poscomlibs.getDoubleFormat(Convert.ToDouble(lbTotalValCS.Text) - dblTotal + Convert.ToDouble(lblPayPennyRounded.Text));

            double dblPayAmt = Math.Round((dblTotal + Convert.ToDouble(lblPayBalance.Text) - Convert.ToDouble(lblPayPennyRounded.Text)), 2);
            if ((GPayFinish == true) && (Convert.ToDouble(lbTotalValCS.Text) == dblPayAmt))
            {
                completeTransaction();
            }
        }
        
        private bool chkPinpadInit()
        {
            bool ret = false;
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            g_sMessage = string.Format("[{0}] Pinpadprocess : 95", sMethod);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            pinpadprocess("95", "0");

            return ret;
        }

        // OPOS Device Modules Start
        private int EnableOPOSDevices()
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string sMessage = string.Empty;

            // OPOS Scale Start
            if (_OpenScaleDevice() != OPOS_RESPONSE.OPOS_SUCCESS)
            {
                sMessage = "Success";
                g_sMessage = string.Format("[{0}] Scale Opened (Code: {1})", sMethod, sMessage);

                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                return -1;
            }
            else
            {
                sMessage = "Fail";
                g_sMessage = string.Format("[{0}] Scale Opened (Code: {1})", sMethod, sMessage);

                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            }

            if (_ClaimScaleDevice() != OPOS_RESPONSE.OPOS_SUCCESS)
            {
                sMessage = "Success";
                g_sMessage = string.Format("[{0}] Scale Claimed (Code: {1})", sMethod, sMessage);

                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                return -1;
            }
            else
            {
                sMessage = "Fail";
                g_sMessage = string.Format("[{0}] Scale Claimed (Code: {1})", sMethod, sMessage);

                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            }

            if (_EnableScaleDevice() != OPOS_RESPONSE.OPOS_SUCCESS)
            {
                sMessage = "Success";
                g_sMessage = string.Format("[{0}] Scale Enabled (Code: {1})", sMethod, sMessage);

                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                return -1;
            }
            else
            {
                sMessage = "Fail";
                g_sMessage = string.Format("[{0}] Scale Enabled (Code: {1})", sMethod, sMessage);

                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            }

            // OPOS Scanner Start
            if (_OpenScannerDevice() != OPOS_RESPONSE.OPOS_SUCCESS)
                return -1;

            if (_ClaimScannerDevice() != OPOS_RESPONSE.OPOS_SUCCESS)
                return -1;

            if (_EnableScannerDevice() != OPOS_RESPONSE.OPOS_SUCCESS)
                return -1;

            return 1;
        }
        
        private void DisableOPOSDevices()
        {
            if (OPOSScale.DeviceEnabled)
            {
                OPOSScale.StatusUpdateEvent -= StatusUpdateEvent;

                OPOSScale.DeviceEnabled = false;
                OPOSScale.ReleaseDevice();
                OPOSScale.Close();
            }

            if (OPOSScanner.DeviceEnabled)
            {
                OPOSScanner.DataEvent -= ScannerDataEvent;

                OPOSScanner.DeviceEnabled = false;
                OPOSScanner.ReleaseDevice();
                OPOSScanner.Close();
            }
        }

        private long _OpenScaleDevice()
        {
            // OPOS Scale Process
            return OPOSScale.Open(s_scale_devicename);          // DATALOGIC PSC Single-Cable RS232 Scanner-Scale on the registry
        }

        private long _OpenScannerDevice()
        {
            // OPOS Scanner Process
            return OPOSScanner.Open(s_scanner_devicename);            // DATALOGIC PSC Single-Cable RS232 Scanner-Scale on the registry
        }
        private long _ClaimScaleDevice()
        {
            return OPOSScale.ClaimDevice(1000);
        }

        private long _ClaimScannerDevice()
        {
            return OPOSScanner.ClaimDevice(1000);
        }

        private int _EnableScaleDevice()
        {
            object gResponse = "";

            if (c_poscominfo.si_scaletype == 9)
            {
                if (OPOSScale.CapStatusUpdate)
                {
                    OPOSScale.StatusNotify = (int)OPOS_SCALE_VALUES.SCAL_SN_ENABLED;

                    if (OPOSScale.ResultCode == OPOS_RESPONSE.OPOS_SUCCESS)
                    {
                        OPOSScale.StatusUpdateEvent += StatusUpdateEvent;

                        //btnGetWeight.Enabled = false;
                        gResponse = (OPOS_CODE_DEFINE)OPOSScale.ResultCode;
                        ShowScaleMessage(gResponse.ToString());
                    }
                    else
                    {
                        gResponse = (OPOS_CODE_DEFINE)OPOSScale.ResultCode;
                        ShowScaleMessage(gResponse.ToString());
                    }
                }
            }

            OPOSScale.DeviceEnabled = true;

            if (OPOSScale.DeviceEnabled)
            {
                OPOSScale.DataEventEnabled = true;
                OPOSScale.FreezeEvents = false;
            }

            return OPOSScale.ResultCode;
        }
        
        private long _EnableScannerDevice()
        {
            OPOSScanner.DeviceEnabled = true;

            if (OPOSScanner.DeviceEnabled)
            {
                OPOSScanner.DataEvent += ScannerDataEvent;
                OPOSScanner.DataEventEnabled = true;
                OPOSScanner.FreezeEvents = false;
            }

            return OPOSScanner.ResultCode;
        }
        
        private void StatusUpdateEvent(int value)
        {
            int status = (int)OPOSScale.ResultCode;
            //string sMessage = string.Format("Value: {0}, Status: {1}", value.ToString(), status.ToString());
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            string sMessage = string.Empty;

            if (value == (int)OPOS_SCALE_STATUS.SCAL_SUE_STABLE_WEIGHT)
            {
                sMessage = WeightFormat(OPOSScale.ScaleLiveWeight);
                ShowScaleData(sMessage);

                g_sMessage = string.Format("[{0}] Scale weight was captured (Code: {1})", sMethod, sMessage);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            }
            else if (value == (int)OPOS_SCALE_STATUS.SCAL_SUE_WEIGHT_UNSTABLE)
            {
                //sMessage = string.Format("Scale weight unstable");
                sMessage = WeightFormat(OPOSScale.ScaleLiveWeight);
                ShowScaleData(sMessage);
            }
            else if (value == (int)OPOS_SCALE_STATUS.SCAL_SUE_WEIGHT_ZERO)
            {
                sMessage = WeightFormat(OPOSScale.ScaleLiveWeight);
                ShowScaleData(sMessage);

                g_sMessage = string.Format("[{0}] Scale weight was captured (Code: {1})", sMethod, sMessage);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            }
            else if (value == (int)OPOS_SCALE_STATUS.SCAL_SUE_WEIGHT_OVERWEIGHT)
            {
                //sMessage = string.Format("Weight limit exceeded.");
                sMessage = string.Format("- - -");
                ShowScaleData(sMessage);

                g_sMessage = string.Format("[{0}] Scale weight is overweight (Code: {1})", sMethod, sMessage);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            }
            else if (value == (int)OPOS_SCALE_STATUS.SCAL_SUE_NOT_READY)
            {
                sMessage = string.Format("Scale not ready.");
            }
            else if (value == (int)OPOS_SCALE_STATUS.SCAL_SUE_WEIGHT_UNDER_ZERO)
            {
                // sMessage = string.Format("Scale under zero weight.");
                sMessage = string.Format(". . .");
                ShowScaleData(sMessage);

                g_sMessage = string.Format("[{0}] Scale weight is Under zero. (Code: {1})", sMethod, sMessage);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            }
            else
            {
                sMessage = string.Format("Unknown status [{0}]", value);
            }

            ShowScaleMessage(sMessage);

            OPOSScale.DataEventEnabled = true;
        }

        static private string WeightFormat(int vWeight)
        {
            string sWeight = string.Empty;

            string sUnits = UnitAbbreviation(OPOSScale.WeightUnits);

            if (sUnits == string.Empty)
            {
                sWeight = "N/A";
            }
            else
            {
                double dWeight = 0.001 * (double)vWeight;
                sWeight = string.Format((OPOSScale.WeightUnits == 2 ? "{0:0.000}" : "{0:0.00}"), dWeight);
            }

            return sWeight;
        }

        static private string UnitAbbreviation(int vUnits)
        {
            string sUnit = string.Empty;

            switch ((OPOS_SCALE_UNITS)vUnits)
            {
                case OPOS_SCALE_UNITS.SCAL_WU_GRAM: sUnit = "gr."; break;
                case OPOS_SCALE_UNITS.SCAL_WU_KILOGRAM: sUnit = "kg."; break;
                case OPOS_SCALE_UNITS.SCAL_WU_OUNCE: sUnit = "oz."; break;
                case OPOS_SCALE_UNITS.SCAL_WU_POUND: sUnit = "lb."; break;
            }

            return sUnit;
        }

        private void ScannerDataEvent(int value)
        {
            string sMessage = string.Format("{0}", OPOSScanner.ScanData);
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            // Temp
            g_sMessage = string.Format("[{0}] Scale Barcode was scanned.(Code: {1})", sMethod, sMessage);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            ShowScannerData(sMessage);
            OPOSScanner.DataEventEnabled = true;
        }

        private void btnscalescanTest_Click(object sender, EventArgs e)
        {
            ShowScannerData("FF83800030");
        }

        private void btnscalescanTest2_Click(object sender, EventArgs e)
        {
            if(GintLocation == 1)
                ShowScannerData("822222342481");
            else if (GintLocation == 3)
                ShowScannerData("822282315302");            
        }
        private void btnscalescanTest3_Click(object sender, EventArgs e)
        {
            if(GintLocation == 1)
                ShowScannerData("011152036458");
            else if (GintLocation == 3)
                ShowScannerData("FF83800030");
        }

        private void ShowScaleData(string vMessage)
        {
            lbWeightCS.Text = vMessage;
        }

        private void ShowScaleMessage(string vMessage)
        {
            string sMessage = string.Format("OPOSScale Device [{0}]", vMessage);
        }

        private void ShowScannerData(string vMessage)
        {
            //txtNumCS.Text = vMessage;

            int x, y;
            string strInStr;
            string strScanData;
            string strTempScanData = string.Empty;
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            strInStr = vMessage;
            x = vMessage.Length / 2;
            y = x - 7;

            strScanData = vMessage;

            if(ctrlOnScreen == gbErrorBox)                              // Errorbox 화면 출력 중에는 저울에서 스캔되는거 막음.
            {
                return;
            }

            if(ctrlOnScreen == gbScanPointCard)
            {
                g_sMessage = string.Format("[{0}] Scale Barcode was scanned for membership (Code: {1})", sMethod, strScanData);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                //strTempScanData = strScanData.Substring(1, strScanData.Length - 1).Trim();
                strTempScanData = strScanData;

                g_sMessage = string.Format("[{0}] Insert membership Check (Code: {1})", sMethod, strTempScanData);

                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                if (chkMemebership(strTempScanData))
                {
                    ScanPointCardProcess(strTempScanData);
                    return;
                }
                //return;
            }
            
            //if (gbScanPointCard.Visible == true)
            //{
            //    g_sMessage = string.Format("[{0}] Scale Barcode was scanned for membership (Code: {1})", sMethod, strScanData);
            //    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            //    ScanPointCardProcess(strScanData.Substring(1, strScanData.Length - 1).Trim());
            //    return;
            //}

            /*
            // 원래 Logic은 Reprint 입력창에 Reprint Invoice 값 넣어서 처리하도록 되어 있으나 Self Checkout은 Reprint 필요없어서 추가할 필요 없는 부분임
            //if (pnlReprint.Visible == true)
            //{
            //    GstrPrtInvno = strScanData;
            //    GstrReprint = "R"; // 판매 후 일반 Print

            //    PrtReceipt.PrintController = new System.Drawing.Printing.StandardPrintController();
            //    PrtReceipt.Print();
            //}
            //else
            //{
            */

            string strEntrycode = "";

            int iPrefix = Convert.ToChar(strScanData.Substring(0, 1));

            if (48 > iPrefix || iPrefix > 123)
            {
                //strScanData = strScanData.Substring(1).Replace(Convert.ToChar(0x0D), Convert.ToChar(0x00)).Replace(Convert.ToChar(0x0A), Convert.ToChar(0x00));
                strScanData = strScanData.Substring(1);
            }

            for (x = 0; x < strScanData.Length; x++)
            {
                if (Char.IsNumber(Convert.ToChar(strScanData.Substring(x, 1))) == true || strScanData.Substring(x, 1) == ".")
                {
                    strEntrycode = strEntrycode + strScanData.Substring(x, 1);
                }
                else
                {
                    x = strScanData.Length + 1;
                }
            }

            strScanData = strScanData.Trim();
            g_sMessage = string.Format("[{0}] ScanData : {1})", sMethod, strScanData);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            // Close 화면이면 상품 스캔되었을때 동작되지 않도록 수정. Manager Key 입력시 Open 되도록 수정.
            if (ctrlOnScreen == pn_TempClosed)
            {
                if (strScanData == g_strManagerKey || (strScanData.Length == 10 && strScanData.Substring(0, 1) == "F"))
                {
                    // Manager Key 스캔시 Close 해제.
                    g_ManagerCardScanReady = true;

                    if (strScanData.Length == 10 && strScanData.Substring(0, 1) == "F")
                    {
                        strEntrycode = c_poscomlibs.Right(strScanData, 8);
                    }
                    else
                    {
                        strEntrycode = strScanData;
                    }
                        
                    g_sMessage = string.Format("[{0}] Scale Barcode was scanned (Code: {1}) - ctrlOnScreen : pn_TempClosed", sMethod, strEntrycode);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    ProcessEntryCode(strEntrycode, 1);

                    return;                
                }
                else
                {
                    return;
                }
            }
            else if(ctrlOnScreen == pn_Start)
            {
                if (strScanData == g_strManagerKey || (strScanData.Length == 10 && strScanData.Substring(0, 1) == "F"))
                {
                    // Manager Key 스캔시 Close 해제.
                    g_ManagerCardScanReady = true;

                    if (strScanData.Length == 10 && strScanData.Substring(0, 1) == "F")
                    {
                        strEntrycode = c_poscomlibs.Right(strScanData, 8);
                    }
                    else
                    {
                        strEntrycode = strScanData;
                    }

                    g_sMessage = string.Format("[{0}] Scale Barcode was scanned (Code: {1}) - ctrlOnScreen : pn_TempClosed", sMethod, strEntrycode);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    ProcessEntryCode(strEntrycode, 1);

                    return;
                }                
            }


            if (strEntrycode.Length == 9 && strEntrycode.Substring(0, 1) == "F")
            {
                strEntrycode = c_poscomlibs.Right(strEntrycode, 8);
                g_sMessage = string.Format("[{0}] Scale Barcode was scanned. - F(Code: {1})", sMethod, strEntrycode);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            }
            // Manager Barcode 스캔시.
            if (strScanData.Length == 10 && strScanData.Substring(0, 2) == "FF" && g_ReprintModeOn == false && g_ItemDiscountModeOn == false)                // 벤쿠버 마스터 키. 저울 설정 다시 한 후에 변경될 예정.
            {
                strEntrycode = c_poscomlibs.Right(strScanData, 8);
                g_sMessage = string.Format("[{0}] Scale Barcode was scanned. - FF(Code: {1})", sMethod, strEntrycode);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                ProcessEntryCode(strEntrycode, 1);
                return;
            }
            else if (strScanData.Length == 9 && strScanData.Substring(0, 2) == "FF" && g_ReprintModeOn == false && g_ItemDiscountModeOn == false)            // 토론토 마스터 키. 벤쿠버 마스터 키도 변경할 예정.
            {
                strEntrycode = strScanData.Substring(2).Trim();

                g_sMessage = string.Format("[{0}] Scale Barcode was scanned. - FF, String Length = 9(Code: {1})", sMethod, strEntrycode);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                ProcessEntryCode(strEntrycode, 1);
                return;
            }
            // Reprint시 영수증 바코드 스캔시
            else if (strScanData.Length == 14 && ctrlOnScreen == gbReceiptReprint && g_ReprintModeOn == true)
            {
                strEntrycode = strScanData.Substring(1, strScanData.Length - 1).Trim();
                g_sMessage = string.Format("[{0}] Scale Barcode was scanned.(Code: {1}) - Receipt Barcode Scaned.", sMethod, strEntrycode);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                // Receipt
                GstrPrtInvno = strEntrycode;
                GstrReprint = "R"; // Reprint시 R

                PrtReceipt.PrintController = new System.Drawing.Printing.StandardPrintController();
                PrtReceipt.Print();

                txtReceiptReprintBarcode.Clear();

                transitionSiglePage(gbReceiptReprint, 1023, 200);
                ctrlOnScreen = ctrTemp;

                g_ReprintModeOn = false;

                ProcessReprintAfterButtonControl();
            }
            else if (strEntrycode == "" && g_ReprintModeOn == false && g_ItemDiscountModeOn == false && (strScanData.Substring(0, 1) == "A" || strScanData.Substring(0, 1) == "B" || strScanData.Substring(0, 1) == "E" || strScanData.Substring(0, 1) == "F" || strScanData.Substring(0, 1) == "C"))
            {
                if (strScanData.Substring(0, 1) == "B" && strScanData.Length == 12)
                {
                    strEntrycode = strScanData.Substring(2).Trim();
                }
                else if (strScanData.Substring(0, 2) == "B3" && (strScanData.Length == 7 || strScanData.Length == 15))                     // 토론토 Case // Length ==15 코퀴틀람 케이스
                {
                    strEntrycode = strScanData.Substring(2).Trim();
                }
                else
                {
                    strEntrycode = strScanData.Substring(1, strScanData.Length - 1).Trim();
                }
                g_sMessage = string.Format("[{0}] Scale Barcode was scanned.(Code: {1})", sMethod, strEntrycode);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                ProcessEntryCode(strEntrycode, 1);
            }
            else if (strEntrycode == "" && g_ReprintModeOn == false && g_ItemDiscountModeOn == false && strScanData.Length == 7)             // 벤쿠버 case
            {
                strEntrycode = strScanData.Substring(1, strScanData.Length - 1).Trim();

                g_sMessage = string.Format("[{0}] Scale Barcode was scanned.(Code: {1}) 20xxxx digit", sMethod, strEntrycode);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                ProcessEntryCode(strEntrycode, 1);
            }
            else if (strEntrycode == "" && g_ReprintModeOn == false && g_ItemDiscountModeOn == false && strScanData.Length == 13)            // 벤쿠버 Case
            {
                strEntrycode = strScanData.Substring(3).Trim();

                g_sMessage = string.Format("[{0}] Scale Barcode was scanned.(Code: {1}) 777777xxxx digit", sMethod, strEntrycode);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                ProcessEntryCode(strEntrycode, 1);
            }
            else if (strScanData.Length <= 8 && (strScanData.Substring(0, 3) == "332" || strScanData == "5640") && g_ReprintModeOn == false && g_ItemDiscountModeOn == false)               // 토론토 Case
            {
                strEntrycode = strScanData;

                g_sMessage = string.Format("[{0}] Scale Barcode was scanned.(Code: {1}) 332xxxx Or 5640 digit", sMethod, strEntrycode);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                ProcessEntryCode(strEntrycode, 1);
            }
            else if (strScanData.Length == 7 && ctrlOnScreen == gbItemDiscount && g_ItemDiscountModeOn == true)               //벤쿠버 Case  Item Discount Barcode 저울에서 스캔 되었을 때. B39.xxx
            {
                strEntrycode = strScanData.Substring(2).Trim();

                g_sMessage = string.Format("[{0}] Scale Barcode was scanned.(Code: {1}) B39.xxx digit", sMethod, strEntrycode);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                ProcessEntryCode(strEntrycode, 1);
            }
            else
            {
                strEntrycode = strScanData;

                g_sMessage = string.Format("[{0}] Scale Barcode was scanned (Code: {1}) - Check Barcode Type or Mode Check", sMethod, strEntrycode);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                ProcessEntryCode(strEntrycode, 1);                              
            }
        }

        private void ShowScannerMessage(string vMessage)
        {
            string sMessage = string.Format("OPOSScanner Device [{0}]", vMessage);
        }
        private void DisplayErrorMessageBox(string strTitle, string strContent, int ibtnCount, string sMethod)
        {
            g_sMessage = string.Format("[{0}] Error Message : {1}", sMethod, strContent);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            InitErrorMessageBox();

            lbErrorBoxMsgTitle.Text = strTitle;
            lbErrorBoxContent.Text = strContent;
            lbSmethod.Text = sMethod;
            
            if (ibtnCount == 1)
            {
                btnErrorBoxBtn1.Dock = System.Windows.Forms.DockStyle.Bottom;
                btnErrorBoxBtn2.Visible = false;
                btnErrorBoxBtn1.Text = "Confirm";
                btnErrorBoxBtn1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                btnErrorBoxBtn1.Image = global::HanaSales_SelfCheckOut.Properties.Resources.Next_S_new;
                btnErrorBoxBtn1.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;                
            }
            else if(ibtnCount == 2)
            {
                btnErrorBoxBtn1.Text = "No";
                btnErrorBoxBtn1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                btnErrorBoxBtn1.Image = global::HanaSales_SelfCheckOut.Properties.Resources.Back_S_new;
                btnErrorBoxBtn1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;

                btnErrorBoxBtn2.Text = "Yes";
                btnErrorBoxBtn2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                btnErrorBoxBtn2.Image = global::HanaSales_SelfCheckOut.Properties.Resources.Next_S_new;
                btnErrorBoxBtn2.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            }

            // Transition.
            if(ctrlOnScreen != gbErrorBox)
            {
                ctrTemp = ctrlOnScreen;
                transitionSiglePage(gbErrorBox, 288, 200);
            }
            else
            {
                transitionSiglePage(gbErrorBox, 288, 200);
            }
            //ctrTemp = ctrlOnScreen;            
        }
        public void InitErrorMessageBox()
        {
            gbErrorBox.BringToFront();
            lbErrorBoxMsgTitle.Text = "";
            lbErrorBoxContent.Text = "";
            lbSmethod.Text = "";

            btnErrorBoxBtn1.Dock = System.Windows.Forms.DockStyle.None;
            btnErrorBoxBtn2.Dock = System.Windows.Forms.DockStyle.None;

            btnErrorBoxBtn1.Visible = true;
            btnErrorBoxBtn2.Visible = true;
        }
        private void btnErrorBoxBtn1_Click(object sender, EventArgs e)
        {
            if (lbErrorBoxMsgTitle.Text == "Membership")
            {
                btnSkipPointCard.Enabled = true;
                btnBackToStart.Enabled = true;
                btnPointPhoneNumber1.Enabled = true;
                btnPointPhoneNumber2.Enabled = true;
                btnPointPhoneNumber3.Enabled = true;
                btnPointPhoneNumber4.Enabled = true;
                btnPointPhoneNumber5.Enabled = true;
                btnPointPhoneNumber6.Enabled = true;
                btnPointPhoneNumber7.Enabled = true;
                btnPointPhoneNumber8.Enabled = true;
                btnPointPhoneNumber9.Enabled = true;
                btnPointPhoneNumber0.Enabled = true;
                btnPointPhoneNumberBackSpace.Enabled = true;
                btnPointPhoneNumberEnter.Enabled = true;

                txtScanPointCard.Clear();
                txtScanPointCard.Focus();
            }
            else if (lbErrorBoxMsgTitle.Text == "Point Card")
            {
                transitionSiglePage(gbProcessPointCard, 1023, 200);
                //ctrlOnScreen = ctrTemp;
                ctrlOnScreen = pnSelectPayment;     // 강제로 컨트롤 화면 설정함.

                btnSelectCreditCard.Enabled = true;

                if (Convert.ToDouble(lblPayHmoney.Text) > 0) { btnSelectPointCard.Enabled = false; pbSelectpaymentPointCardApplied.Visible = true; }
                else                                         { btnSelectPointCard.Enabled = true; pbSelectpaymentPointCardApplied.Visible = false; }

                //if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                            // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
                //{
                    if (Convert.ToDouble(lblPayEBT.Text) > 0) { btnEBT.Enabled = false; pbSelectpaymentEBTApplied.Visible = true; }
                    else { btnEBT.Enabled = true; pbSelectpaymentEBTApplied.Visible = false; }
                //}
                         
                btnSelectGiftCard.Enabled = true;
                btnBack.Enabled = true;

                if (g_HelpModeOn == true)
                {
                    btnHelp.Enabled = false;
                    //txtNumCS.Enabled = true;
                    KeyInReady();
                }
                else
                {
                    btnHelp.Enabled = true;
                    txtNumCS.Enabled = false;       // 아이템 스캔 방지.
                }
            }
            else if (lbErrorBoxMsgTitle.Text == "Payment")
            {
                //transitionSiglePage(gbProcessPointCard, 1023, 200);
                transitionSiglePage(ctrTemp, 1023, 200);
                //ctrlOnScreen = ctrTemp;
                ctrTemp = pnSelectPayment;     // 강제로 컨트롤 화면 설정함.

                btnSelectCreditCard.Enabled = true;

                if(Convert.ToDouble(lblPayHmoney.Text) > 0) {    btnSelectPointCard.Enabled = false; pbSelectpaymentPointCardApplied.Visible = true; }
                else                                        {    btnSelectPointCard.Enabled = true; pbSelectpaymentPointCardApplied.Visible = false; }

                //if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                            // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
                //{
                    if (Convert.ToDouble(lblPayEBT.Text) > 0) { btnEBT.Enabled = false; pbSelectpaymentEBTApplied.Visible = true; }
                    else { btnEBT.Enabled = true; pbSelectpaymentEBTApplied.Visible = false; }
                //}

                btnSelectGiftCard.Enabled = true;
                btnBack.Enabled = true;

                if (g_HelpModeOn == true)
                {
                    btnHelp.Enabled = false;
                    //txtNumCS.Enabled = true;
                    KeyInReady();
                }
                else
                {
                    btnHelp.Enabled = true;
                    txtNumCS.Enabled = false;       // 아이템 스캔 방지.
                }
            }            
            else if (lbErrorBoxMsgTitle.Text == "PIN Pad")
            {
                transitionSiglePage(gbProcessCreditCard, 1023, 200);
                //ctrlOnScreen = ctrTemp;
                ctrTemp = pnSelectPayment;    // 강제로 컨트롤 화면 설정함.

                btnSelectCreditCard.Enabled = true;

                if (Convert.ToDouble(lblPayHmoney.Text) > 0) { btnSelectPointCard.Enabled = false; pbSelectpaymentPointCardApplied.Visible = true; }
                else                                         { btnSelectPointCard.Enabled = true; pbSelectpaymentPointCardApplied.Visible = false; }

                //if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                            // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
                //{
                    if (Convert.ToDouble(lblPayEBT.Text) > 0) { btnEBT.Enabled = false; pbSelectpaymentEBTApplied.Visible = true; }
                    else { btnEBT.Enabled = true; pbSelectpaymentEBTApplied.Visible = false; }
                //}

                btnSelectGiftCard.Enabled = true;
                btnBack.Enabled = true;

                if (g_HelpModeOn == true)
                {
                    btnHelp.Enabled = false;
                    txtNumCS.Enabled = true;
                    KeyInReady();
                }
                else
                {
                    btnHelp.Enabled = true;
                    txtNumCS.Enabled = false;       // 아이템 스캔 방지.
                }
            }
            else if (lbErrorBoxMsgTitle.Text == "Search Item")
            {
                if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
                {
                    btnNext.Enabled = false;
                }
                else
                {
                    btnNext.Enabled = true;
                }
                if (g_HelpModeOn == true)
                {
                    btnHelp.Enabled = false;
                    txtNumCS.Enabled = true;
                }
                else
                {
                    btnHelp.Enabled = true;
                    txtNumCS.Enabled = false;       // 아이템 스캔 방지.
                }

                btnBack.Enabled = true;
                btnSearch.Enabled = true;
                btnSearch_Category.Enabled = true;
                KeyInReady();
            }
            else if (lbErrorBoxMsgTitle.Text == "Notice")
            {
                lvSearchImage_Item.Enabled = true;
                lvSearchImage_Category.Enabled = true;
                lvSearchImage_Additional.Enabled = true;
                pn_Keyboard.Enabled = true;
                btnSearchOrKeyinItem.Enabled = true;
                btnBackToCategory.Enabled = true;
                btnSearchCategory_TZ.Enabled = true;
                btnSearchCategory_QS.Enabled = true;
                btnSearchCategory_P.Enabled = true;
                btnSearchCategory_NO.Enabled = true;
                btnSearchCategory_KM.Enabled = true;
                btnSearchCategory_DJ.Enabled = true;
                btnSearchCategory_BC.Enabled = true;
                btnSearchCategory_A.Enabled = true;
                btnSearchCategory_All.Enabled = true;
                btnSearchExit.Enabled = true;

                txtSearchCode.Focus();
            }
            else if (lbErrorBoxMsgTitle.Text == "VOID" || lbErrorBoxMsgTitle.Text == "Suspend")
            {
                btnSearch.Enabled = true;
                btnSearch_Category.Enabled = true;
                btnBack.Enabled = true;
                btnItemCorrect.Enabled = true;
                btnVoid.Enabled = true;
                btnItemDiscount.Enabled = true;
                btnReprint.Enabled = true;
                btnSuspend.Enabled = true;
                ItemCSView.Enabled = true;

                btnAddBagToCart.Enabled = true;
                btnNoBag.Enabled = true;
                btnBagPlus.Enabled = true;
                btnMinus.Enabled = true;

                btnSelectCreditCard.Enabled = true;
                btnSelectPointCard.Enabled = true;
                btnSelectGiftCard.Enabled = true;

                //if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                            // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
                btnEBT.Enabled = true;

                btnManualETCKey.Enabled = true;

                KeyInReady();

                // EBT 결제 내역이 있는 경우
                if (Convert.ToDouble(lblPayEBT.Text) > 0)
                {
                    btnEBT.Enabled = false;
                }
                else
                {
                    //if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                                // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
                    btnEBT.Enabled = true;
                }

                if (g_HelpModeOn == true)
                {
                    btnHelp.Enabled = false;
                }
                else
                {
                    btnHelp.Enabled = true;
                }

                if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
                {
                    btnNext.Enabled = false;
                }
                else
                {
                    btnNext.Enabled = true;
                }
            }
            else if (lbErrorBoxMsgTitle.Text == "Item Correct")
            {
                // Button Control
                btnSearch.Enabled = true;
                btnSearch_Category.Enabled = true;
                btnBack.Enabled = true;
                btnItemCorrect.Enabled = true;
                btnVoid.Enabled = true;
                btnItemDiscount.Enabled = true;
                btnSuspend.Enabled = true;
                ItemCSView.Enabled = true;
                btnReprint.Enabled = true;
                btnManualETCKey.Enabled = true;

                btnAddBagToCart.Enabled = true;
                btnNoBag.Enabled = true;
                btnBagPlus.Enabled = true;
                btnMinus.Enabled = true;

                btnSelectCreditCard.Enabled = true;
                btnSelectPointCard.Enabled = true;
                btnSelectGiftCard.Enabled = true;

                // EBT 결제 내역이 있는 경우
                if (Convert.ToDouble(lblPayEBT.Text) > 0)
                {
                    btnEBT.Enabled = false;
                }
                else
                {
//                    if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                                // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
                    btnEBT.Enabled = true;
                }

                if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
                {
                    btnNext.Enabled = false;
                }
                else
                {
                    btnNext.Enabled = true;
                }
                KeyInReady();
            }
            else if (lbErrorBoxMsgTitle.Text == "Receipt Print")
            {
                if (ctrTemp != ctrTemp2)
                {
                    ctrTemp = ctrTemp2;
                }

                txtReceiptReprintBarcode.Clear();
                txtReceiptReprintBarcode.Focus();
            }
            else if (lbErrorBoxMsgTitle.Text == "Help Mode")
            {
                if (ctrTemp != ctrTemp2)
                {
                    ctrTemp = ctrTemp2;
                }
                KeyInReady();
            }
            else if (lbErrorBoxMsgTitle.Text == "Age Check")
            {
                if (ctrTemp != ctrTemp2)
                {
                    ctrTemp = ctrTemp2;
                }

                tbAgeCheckNum.Clear();
                tbAgeCheckNum.Focus();
            }
            else if (lbErrorBoxMsgTitle.Text == "Age Check Decline")
            {
                if (ctrTemp != ctrTemp2)
                {
                    ctrTemp = ctrTemp2;
                }

                // Help Mode 강제로 해제함.
                ProcessEntryCode(g_strManagerKey, 0);                    //임시로 Manager Password 입력

                tbAgeCheckNum.Clear();
                KeyInReady();
            }
            else if (lbErrorBoxMsgTitle.Text == "Item Discount")
            {
                if (ctrTemp != ctrTemp2)
                {
                    ctrTemp = ctrTemp2;
                }

                // Button Control
                btnSearch.Enabled = true;
                btnSearch_Category.Enabled = true;
                btnBack.Enabled = true;
                btnItemCorrect.Enabled = true;
                btnVoid.Enabled = true;
                btnItemDiscount.Enabled = true;
                btnSuspend.Enabled = true;
                ItemCSView.Enabled = true;
                btnReprint.Enabled = true;
                btnManualETCKey.Enabled = true;

                if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
                {
                    btnNext.Enabled = false;
                }
                else
                {
                    btnNext.Enabled = true;
                }
                KeyInReady();
            }
            else if (lbErrorBoxMsgTitle.Text == "Printer Status")
            {
                btnStart.Enabled = true;

                transitionSiglePage(gbErrorBox, 1023, 200);
                ctrlOnScreen = ctrTemp;

                // Light ON
                ProcessQLightControl("a0");     // All Light Off
                ProcessQLightControl("g1");     // Green Light On
                ProcessQLightControl("b0");     // Beep Off

                PrintStatusCheckTimer.Interval = g_iPrinterStatusInterval;
                PrintStatusCheckTimer.Start();
            }
            else if (lbErrorBoxMsgTitle.Text == "ITEM SCAN")
            {
                // Button Control
                btnBack.Enabled = true;
                btnItemCorrect.Enabled = true;
                btnVoid.Enabled = true;
                btnItemDiscount.Enabled = true;
                btnReprint.Enabled = true;
                btnSuspend.Enabled = true;
                btnNext.Enabled = true;
                //ItemCSView.Enabled = true;

                btnSearch.Enabled = true;
                btnSearch_Category.Enabled = true;

                btnAddBagToCart.Enabled = true;
                btnNoBag.Enabled = true;
                btnBagPlus.Enabled = true;
                btnMinus.Enabled = true;

                btnSelectCreditCard.Enabled = true;
                btnSelectPointCard.Enabled = true;
                btnSelectGiftCard.Enabled = true;

                //if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                            // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
                btnEBT.Enabled = true;

                btnManualETCKey.Enabled = true;
                btnHelp.Enabled = true;

                KeyInReady();
            }
            else if (lbErrorBoxMsgTitle.Text == "Manual ETC")
            {
                // 결제 내역이 있는 경우
                if (lblPayTotal.Text != "0.00")
                {
                    // Button Control
                    btnSearch.Enabled = true;
                    btnSearch_Category.Enabled = true;
                    btnBack.Enabled = true;
                    btnItemCorrect.Enabled = true;
                    btnVoid.Enabled = true;
                    btnItemDiscount.Enabled = true;
                    btnSuspend.Enabled = true;
                    ItemCSView.Enabled = true;
                    btnReprint.Enabled = true;
                    btnManualETCKey.Enabled = true;

                    btnAddBagToCart.Enabled = true;
                    btnNoBag.Enabled = true;
                    btnBagPlus.Enabled = true;
                    btnMinus.Enabled = true;

                    btnSelectCreditCard.Enabled = true;
                    btnSelectPointCard.Enabled = true;
                    btnSelectGiftCard.Enabled = true;

                    // EBT 결제 내역이 있는 경우
                    if (Convert.ToDouble(lblPayEBT.Text) > 0)
                    {
                        btnEBT.Enabled = false;
                    }
                    else
                    {
//                        if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                                    // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
                        btnEBT.Enabled = true;
                    }

                    if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
                    {
                        btnNext.Enabled = false;
                    }
                    else
                    {
                        btnNext.Enabled = true;
                    }
                }

                pn_ManualETCKey.Enabled = true;
                txtManualETCKey_Amount.Clear();
                txtManualETCKey_Amount.Focus();
            }
            else
            {
                KeyInReady();
            }
            
            btnErrorBoxBtn1.Image = global::HanaSales_SelfCheckOut.Properties.Resources.Back_S_new;
           // btnErrorBoxBtn1.Symbol = "";

            //ctrTemp = ctrlOnScreen;
            transitionSiglePage(gbErrorBox, 1023, 200);
            ctrlOnScreen = ctrTemp;
        }
        
        private void btnErrorBoxBtn2_Click(object sender, EventArgs e)
        {
            if (lbErrorBoxMsgTitle.Text == "VOID")
            {
                // Item Void
                ProcessItemVoid();

                if (lbSmethod.Text == "btnBack_Click")
                {
                    transitionSiglePage(gbErrorBox, 1023, 200);
                    pn_Start.Visible = true;
                    transitionDoublePage(pn_Start, pn_ItemScan, 0, 1 * GROUP_BOX_LEFT, 300);
                    transitionSiglePage(pnSelectPayment, 1023, 200);
                    transitionSiglePage(pnAddBag, 1023, 200);

                    ctrlOnScreen = pn_Start;

                    // OPEN/CLOSE 버튼 활성화.
                    //swbOpenClose.Visible = true;

                    // Help BUTTON
                    btnHelp.Visible = true;
                    btnHelp.BringToFront();
                    btnHelp.Enabled = true;

                    //ETC ITEM KEY
                    btnManualETCKey.Visible = false;
                    btnManualETCKey.Enabled = true;
                    btnManualETCKey.SendToBack();
                    
                    st_ProcessStatus.Visible = false;
                    tbStep1Name.Visible = false;
                    tbStep2Name.Visible = false;
                    tbStep3Name.Visible = false;
                    tbStep4Name.Visible = false;
                    lbHelpMode.Visible = false;
                    lbHelpMode2.Visible = false;
                    lbHelpMode3.Visible = false;
                    lbHelpMode4.Visible = false;
                    lbHelpMode5.Visible = false;
                    lbBalancePointTop.Visible = false;
                    lbCurrentCusBal.Visible = false;

                    lbSavingCS.Visible = false;
                    lbSavingValueCS.Visible = false;

                    st_ProcessStatus.CurrentStep = 1;

                    g_HelpModeOn = false;
                    g_HelpModeReady = false;
                    g_CertifiedAdult = false;
                    g_CertifiedAdultReady = false;
                    g_iAdultLimit = 0;
                    g_iStoredAdultLimit = 0;

                    ItemCSView.Enabled = false;
                    txtInvNo.Visible = false;

                    btnSelectPointCard.Enabled = true;                  // Point Card 초기화.

                    //if (GintLocation == 1)       // 벤쿠버 일 경우
                    //{
                    //    저울에서 스캔 되는거 방지.
                    //    if (OPOSScanner.DeviceEnabled)
                    //    {
                    //        OPOSScanner.DataEvent -= ScannerDataEvent;
                    //        OPOSScanner.DeviceEnabled = false;
                    //    }
                    //}

                    // 음성 실행.
                    GssWelcome.SelectVoice(GstrVoice);
                    GssWelcome.SpeakAsync("Welcome to H Mart.");

                    // Light ON
                    ProcessQLightControl("a0");     // all Light Off
                    ProcessQLightControl("g1");     // Green Light On

                    // Printer Status Check Timer Start
                    if (c_poscominfo.si_sPrinter1 == "CUSTOM K80")
                    {
                        PrintStatusCheckTimer.Interval = g_iPrinterStatusInterval;
                        PrintStatusCheckTimer.Start();
                    }
                    else
                    {
                        // Update Timer Start.
                        UpdateCheckTimer.Interval = g_iUpdateInterval;
                        UpdateCheckTimer.Start();
                    }

                    // Timer Stop
                    BackToStartTimerFromItemScan.Stop();
                    // Timer Stop
                    BackToStartExtension.Stop();

                    //UpdateCheckTimer.Interval = g_iUpdateInterval;
                    //UpdateCheckTimer.Start();
                }
                else if(lbSmethod.Text == "btnVoid_Click")
                {
                    transitionSiglePage(gbErrorBox, 1023, 200);
                    ctrlOnScreen = ctrTemp;

                    btnSearch.Enabled = true;
                    btnSearch_Category.Enabled = true;
                    btnBack.Enabled = true;
                    btnItemCorrect.Enabled = true;
                    btnVoid.Enabled = true;
                    btnItemDiscount.Enabled = true;
                    btnReprint.Enabled = true;
                    btnSuspend.Enabled = true;

                    btnAddBagToCart.Enabled = true;
                    btnNoBag.Enabled = true;
                    btnBagPlus.Enabled = true;
                    btnMinus.Enabled = true;

                    g_CertifiedAdult = false;
                    g_CertifiedAdultReady = false;
                    g_iAdultLimit = 0;
                    g_iStoredAdultLimit = 0;

                    btnSelectCreditCard.Enabled = true;
                    btnSelectPointCard.Enabled = true;
                    btnSelectGiftCard.Enabled = true;

//                    if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                                // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
                    btnEBT.Enabled = true;

                    btnManualETCKey.Enabled = true;

                    if (ItemCSView.Items.Count < 1 || lbTotalValCS.Text == "0.00")
                    {
                        btnNext.Enabled = false;
                    }
                    else
                    {
                        btnNext.Enabled = true;
                    }
                    
                    if (g_HelpModeOn == true)
                    {
                        ItemCSView.Enabled = true;
                        btnHelp.Enabled = false;
                    }
                    else
                    {
                        ItemCSView.Enabled = false;
                        btnHelp.Enabled = true;
                    }
                }
            }
            else if (lbErrorBoxMsgTitle.Text == "Suspend")
            {
                // Transaction Suspend
                ProcessTransactionSuspend();

                // 화면 처리
                transitionSiglePage(gbErrorBox, 1023, 200);
                transitionSiglePage(pnSelectPayment, 1023, 200);
                transitionSiglePage(pnAddBag, 1023, 200);
                transitionSiglePage(pn_ItemScan, 1023, 200);
                pn_Start.Visible = true;
                transitionSiglePage(pn_Start, 0, 200);

                // 기타 처리
                st_ProcessStatus.Visible = false;
                tbStep1Name.Visible = false;
                tbStep2Name.Visible = false;
                tbStep3Name.Visible = false;
                tbStep4Name.Visible = false;
                lbHelpMode.Visible = false;
                lbHelpMode2.Visible = false;
                lbHelpMode3.Visible = false;
                lbHelpMode4.Visible = false;
                lbHelpMode5.Visible = false;
                lbBalancePointTop.Visible = false;
                lbCurrentCusBal.Visible = false;
                
                st_ProcessStatus.CurrentStep = 1;

                g_HelpModeOn = false;
                g_HelpModeReady = false;
                g_CertifiedAdult = false;
                g_CertifiedAdultReady = false;
                g_iAdultLimit = 0;
                g_iStoredAdultLimit = 0;

                ItemCSView.Enabled = false;
                txtInvNo.Visible = false;

                // OPEN/CLOSE 버튼 활성화.
                //swbOpenClose.Visible = true;

                // Help BUTTON
                btnHelp.Visible = true;
                btnHelp.BringToFront();
                btnHelp.Enabled = true;

                //ETC ITEM KEY
                btnManualETCKey.Visible = false;
                btnManualETCKey.Enabled = true;
                btnManualETCKey.SendToBack();
                
                // 음성 실행.
                GssWelcome.SelectVoice(GstrVoice);
                GssWelcome.SpeakAsync("Welcome to H Mart.");

                // Light ON
                ProcessQLightControl("a0");     // all Light Off
                ProcessQLightControl("g1");     // Green Light On

                // Printer Status Check Timer Start
                if (c_poscominfo.si_sPrinter1 == "CUSTOM K80")
                {
                    PrintStatusCheckTimer.Interval = g_iPrinterStatusInterval;
                    PrintStatusCheckTimer.Start();
                }
                else
                {
                    // Update Timer Start.
                    UpdateCheckTimer.Interval = g_iUpdateInterval;
                    UpdateCheckTimer.Start();
                }
            }
            else if (lbErrorBoxMsgTitle.Text == "Notice")
            {
                transitionSiglePage(gbErrorBox, 1023, 200);
                ctrlOnScreen = ctrTemp;

                ProcessAddSearchScaleItem();
            }
        }

        private void btnPointPhoneNumber1_Click(object sender, EventArgs e)
        {
            txtScanPointCard.Text += "1";
        }

        private void btnPointPhoneNumber2_Click(object sender, EventArgs e)
        {
            txtScanPointCard.Text += "2";
        }

        private void btnPointPhoneNumber3_Click(object sender, EventArgs e)
        {
            txtScanPointCard.Text += "3";
        }

        private void btnPointPhoneNumber4_Click(object sender, EventArgs e)
        {
            txtScanPointCard.Text += "4";
        }

        private void btnPointPhoneNumber5_Click(object sender, EventArgs e)
        {
            txtScanPointCard.Text += "5";
        }

        private void btnPointPhoneNumber6_Click(object sender, EventArgs e)
        {
            txtScanPointCard.Text += "6";
        }

        private void btnPointPhoneNumber7_Click(object sender, EventArgs e)
        {
            txtScanPointCard.Text += "7";
        }

        private void btnPointPhoneNumber8_Click(object sender, EventArgs e)
        {
            txtScanPointCard.Text += "8";
        }

        private void btnPointPhoneNumber9_Click(object sender, EventArgs e)
        {
            txtScanPointCard.Text += "9";
        }

        private void btnPointPhoneNumber0_Click(object sender, EventArgs e)
        {
            txtScanPointCard.Text += "0";
        }
        
        private void btnPointPhoneNumberBackSpace_Click(object sender, EventArgs e)
        {
            if (txtScanPointCard.Text.Length > 0)
            {
                txtScanPointCard.Text = txtScanPointCard.Text.Remove(txtScanPointCard.Text.Length - 1, 1);
            }

            txtScanPointCard.Focus();
            txtScanPointCard.Select(txtScanPointCard.Text.Length, 0);
        }

        private void btnPointPhoneNumberEnter_Click(object sender, EventArgs e)
        {
            BackToStartTimer.Stop();
            ScanPointCardProcess(txtScanPointCard.Text.Trim());
        }

        private void btnAdultCheckNo_Click(object sender, EventArgs e)
        {
            g_CertifiedAdultReady = false;
            g_CertifiedAdult = false;

            transitionSiglePage(gbAgeCheckConfirmation, 1023, 200);
            ctrlOnScreen = ctrTemp;

            // Help Mode 강제로 해제함.
            ProcessEntryCode(g_strManagerKey, 0);                    //임시로 Manager Password 입력
        }

        private void btnAdultCheckYes_Click(object sender, EventArgs e)
        {
            g_CertifiedAdultReady = false;
            g_CertifiedAdult = true;

            // 입력창 사라지고 Add Item 다시 실행 시킨다. Help Mode 강제로 해제함.
            transitionSiglePage(gbAgeCheckConfirmation, 1023, 200);
            ctrlOnScreen = ctrTemp;

            // Help Mode 강제로 해제함.
            ProcessEntryCode(g_strManagerKey, 0);                    //임시로 Manager Password 입력

            // 아이템 입력.
            ProcessItemSale(g_strAgeCheckforProdID, 1);
        }
        
        private void BackToStartTimerFromItemScan_Tick(object sender, EventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            g_sMessage = string.Format("[{0}] Timer Tick Enter", sMethod);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            // Timer Stop
            BackToStartTimerFromItemScan.Stop();

            // Back ground Bring Front
            pn_BackGround.BringToFront();

            // Back to Start Extension 표시
            g_iBackToStartExtensionCurrent = 0;
            txtBackToStartExtensionTime.Text = (g_iBackToStartExtensionInterval - g_iBackToStartExtensionCurrent).ToString();

            ctrTemp = ctrlOnScreen;
            transitionSiglePage(gbBackToStartExtension, 240, 200);

            // Extension 대기 시간 start
            BackToStartExtension.Interval = 1000;
            BackToStartExtension.Start();

        }

        private void BackToStartExtension_Tick(object sender, EventArgs e)
        {
            // 설정된 시간 만큼 지나면 Back to Start 실행. Text 화면에 초 변화되는거 표시.
            BackToStartExtension.Stop();

            if (g_iBackToStartExtensionCurrent != g_iBackToStartExtensionInterval)
            {
                //현재 카운트랑 설정된 카운트와 다를때 숫자 표시 하고 타이머 다시 시작.
                g_iBackToStartExtensionCurrent += 1;
               
                txtBackToStartExtensionTime.Text = (g_iBackToStartExtensionInterval - g_iBackToStartExtensionCurrent).ToString();
                
                BackToStartExtension.Interval = 1000;
                BackToStartExtension.Start();
            }
            else
            {
                string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
                g_sMessage = string.Format("[{0}] Extension Time limit is Over [{1} second]", sMethod, g_iBackToStartExtensionInterval);
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                // Void 후 Start 화면으로 이동.
                g_iBackToStartExtensionCurrent = 0;
                txtBackToStartExtensionTime.Text = (g_iBackToStartExtensionInterval - g_iBackToStartExtensionCurrent).ToString();
                ProcessBackToStartTimerFromItemScan();
            }            
        }

        private void btnTimeBackToMain_Click(object sender, EventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            g_sMessage = string.Format("[{0}] Back TO Main Customer Click", sMethod);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            // Void 후 Start 화면으로 이동.
            BackToStartExtension.Stop();
            g_iBackToStartExtensionCurrent = 0;
            txtBackToStartExtensionTime.Text = (g_iBackToStartExtensionInterval - g_iBackToStartExtensionCurrent).ToString();
            ProcessBackToStartTimerFromItemScan();
        }

        private void btnBacktoStartTimeExtension_Click(object sender, EventArgs e)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            g_sMessage = string.Format("[{0}] Time Extension Customer Click", sMethod);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            // BacktoStartTimeExtension 초기화.
            g_iBackToStartExtensionCurrent = 0;
            txtBackToStartExtensionTime.Text = (g_iBackToStartExtensionInterval - g_iBackToStartExtensionCurrent).ToString();

            //Time Extension 화면 이동.
            // Back to Start Extension 표시
            transitionSiglePage(gbBackToStartExtension, 1024, 200);
            ctrlOnScreen = ctrTemp;

            //Back Ground 화면 뒤로 이동.
            pn_BackGround.SendToBack();

            // KeyinReady
            KeyInReady();

            // Timer Stop
            BackToStartExtension.Stop();

            //Timer Start
            BackToStartTimerFromItemScan.Interval = g_iBackToStartIntervalFromItemScan;
            BackToStartTimerFromItemScan.Start();
        }
        private void ProcessBackToStartTimerFromItemScan()
        {
            // 아이템 Void 하고 Start 화면으로 이동.
            BackToStartTimerFromItemScan.Stop();

            // Timer Stop
            BackToStartExtension.Stop();

            // Search Box Invisible
            gbSearchBox.Visible = false;

            // Back ground Send Back
            pn_BackGround.SendToBack();
            // Back to Start Extension 이동
            transitionSiglePage(gbBackToStartExtension, 1024, 200);

            // Error box 이동
            transitionSiglePage(gbErrorBox, 1024, 200);

            if (lbErrorBoxMsgTitle.Text == "Notice")
            {
                lvSearchImage_Item.Enabled = true;
                lvSearchImage_Category.Enabled = true;
                pn_Keyboard.Enabled = true;
                btnSearchOrKeyinItem.Enabled = true;
                btnBackToCategory.Enabled = true;
                btnSearchCategory_TZ.Enabled = true;
                btnSearchCategory_QS.Enabled = true;
                btnSearchCategory_P.Enabled = true;
                btnSearchCategory_NO.Enabled = true;
                btnSearchCategory_KM.Enabled = true;
                btnSearchCategory_DJ.Enabled = true;
                btnSearchCategory_BC.Enabled = true;
                btnSearchCategory_A.Enabled = true;
                btnSearchCategory_All.Enabled = true;
                btnSearchExit.Enabled = true;
            }
            
            // Item Void
            ProcessItemVoid();

            pn_Start.Visible = true;
            transitionDoublePage(pn_Start, pn_ItemScan, 0, 1 * GROUP_BOX_LEFT, 300);
            transitionSiglePage(pnSelectPayment, 1024, 300);
            transitionSiglePage(pnAddBag, 1024, 300);
            pn_ManualETCKey.SendToBack();
            transitionSiglePage(pn_ManualETCKey, 1024, 200);
            
            ctrlOnScreen = pn_Start;

            st_ProcessStatus.Visible = false;
            tbStep1Name.Visible = false;
            tbStep2Name.Visible = false;
            tbStep3Name.Visible = false;
            tbStep4Name.Visible = false;
            lbHelpMode.Visible = false;
            lbHelpMode2.Visible = false;
            lbHelpMode3.Visible = false;
            lbHelpMode4.Visible = false;
            lbHelpMode5.Visible = false;
            lbBalancePointTop.Visible = false;
            lbCurrentCusBal.Visible = false;

            btnHelp.Visible = true;
            btnHelp.BringToFront();
            btnHelp.Enabled = true;
            // ETC KEY BUTTON Send Back
            btnManualETCKey.Visible = false;
            btnManualETCKey.SendToBack();
            btnManualETCKey.Enabled = true;

            st_ProcessStatus.CurrentStep = 1;

            g_HelpModeOn = false;
            g_HelpModeReady = false;
            g_CertifiedAdult = false;
            g_CertifiedAdultReady = false;
            g_iAdultLimit = 0;
            g_iStoredAdultLimit = 0;

            ItemCSView.Enabled = false;
            txtInvNo.Visible = false;

            // OPEN/CLOSE 버튼 활성화.
            //swbOpenClose.Visible = true;

            //if (GintLocation == 1 || GintLocation == 3)       // 벤쿠버, 미국 일 경우
            //{
            //    // 저울에서 스캔 되는거 방지.
            //    if (OPOSScanner.DeviceEnabled)
            //    {
            //        OPOSScanner.DataEvent -= ScannerDataEvent;
            //        OPOSScanner.DeviceEnabled = false;
            //    }
            //}

            // 음성 실행.
            GssWelcome.SelectVoice(GstrVoice);
            GssWelcome.SpeakAsync("Welcome to H Mart.");

            // Light ON
            ProcessQLightControl("a0");     // all Light Off
            ProcessQLightControl("g1");     // Green Light On

            // Printer Status Check Timer Start
            if (c_poscominfo.si_sPrinter1 == "CUSTOM K80")
            {
                PrintStatusCheckTimer.Interval = g_iPrinterStatusInterval;
                PrintStatusCheckTimer.Start();
            }
            else
            {
                // Update Timer Start.
                UpdateCheckTimer.Interval = g_iUpdateInterval;
                UpdateCheckTimer.Start();
            }

            //UpdateCheckTimer.Interval = g_iUpdateInterval;
            //UpdateCheckTimer.Start();
        }

        private void ProcessItemVoid()
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
                sQBuff = "DELETE FROM hanamart.dbo.tb_OrderItem ";

                if (c_localdb.DBExcute(sQBuff) != 1)
                {
                    g_sMessage = string.Format("[{0}] Void transaction failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    c_localdb.DBClose();

                    return;
                }
                else
                {
                    g_sMessage = string.Format("[{0}] ({1} item(s) voided ({2}).", sMethod, c_localdb.affectedrecords.ToString(), c_poscominfo.ui_epno);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                }

                InitMemberDisplay();
                InitMemberInfo();
                InitMemberPointInfo();

                initDClabel();

                initListView();
                initSalesTotal();
                PopulateListViewProdItem("Void");
                btnNext.Enabled = false;
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
        
        private void ProcessPointCardPayment()
        {
            string strPaid = txtNumCS.Text;
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

             
            double dblPayAmt = Convert.ToDouble(strPaid == "" ? "0" : strPaid);
            double dblHPNow = Convert.ToDouble(c_poscominfo.mi_pointbalance);
            GdblHPTodayUsed = Convert.ToDouble(lbUsedPointCS.Text);

//            if (Convert.ToDouble(lblPayBalance.Text) > 0)  // 판매 시
            if (Convert.ToDouble(lbSelectPaymentBalanceNum.Text) > 0)  // 판매 시
            {
                if (dblPayAmt == 0)
                {
                    //dblPayAmt = Convert.ToDouble(lblPayBalance.Text) * 500;
                    dblPayAmt = Convert.ToDouble(lbSelectPaymentBalanceNum.Text) * 500;
                    

                    if (dblPayAmt > (dblHPNow - GdblHPTodayUsed))
                    {
                        dblPayAmt = (dblHPNow - GdblHPTodayUsed);
                    }
                }
                else
                {
                    //if ((dblPayAmt / 500) > Convert.ToDouble(lblPayBalance.Text))
                    if ((dblPayAmt / 500) > Convert.ToDouble(lbSelectPaymentBalanceNum.Text))
                    {
                        //dblPayAmt = Convert.ToDouble(lblPayBalance.Text) * 500;
                        dblPayAmt = Convert.ToDouble(lbSelectPaymentBalanceNum.Text) * 500;
                    }

                    if (dblPayAmt > (dblHPNow - GdblHPTodayUsed))
                    {
                        dblPayAmt = (dblHPNow - GdblHPTodayUsed);
                    }
                }
            }
            else
            {
                if (dblPayAmt == 0)
                {
                    dblPayAmt = dblPayAmt * 500;

                }
                else
                {
                    if ((Convert.ToDouble(strPaid) / 500) < dblPayAmt)
                    {
                        dblPayAmt = dblPayAmt * 500;
                    }
                }
            }

            GdblHPTodayUsed = GdblHPTodayUsed + dblPayAmt;
                    //c_poscominfo.mi_pointbalance = Convert.ToInt64(dblHPNow - dblPayAmt);
                    //lbUsedPointCS.Text = GdblHPTodayUsed.ToString();
                    //lbBalancePointCS.Text = c_poscominfo.mi_pointbalance.ToString();

            dblPayAmt = dblPayAmt / 500;
            lblPayHmoney.Text = c_poscomlibs.getDoubleFormat(dblPayAmt);

            //double dblBalance = (Convert.ToDouble(lbTotalValCS.Text)) - dblPayAmt;
            double dblBalance = (Convert.ToDouble(lbSelectPaymentBalanceNum.Text)) - dblPayAmt;

            if (dblBalance > 0)
            {
                DisplayErrorMessageBox("Payment", "PAYMENT INCOMPLETE. \n Please Select another Payment Type.", 1, sMethod);
                GPayFinish = false;
                
                calcPaymentTotal();

                //KeyInReady();
                return;
            }

            // Pay 완료 상태.
            GPayFinish = true;
            calcPaymentTotal();  
        }

        private void btnPointUseNo_Click(object sender, EventArgs e)
        {
            transitionSiglePage(gbProcessPointCard, 1023, 200);
            ctrlOnScreen = ctrTemp;
            btnSelectCreditCard.Enabled = true;
            btnSelectPointCard.Enabled = true;
            btnSelectGiftCard.Enabled = true;

//            if (c_poscominfo.ci_mkno != "86")           // 2024.06.04 West Jordan 매장 임시로 하드코딩 함 by Robin
                                                        // 2024.06.12 West Jordan 매징 임시 하드코딩 해제함. 처음 로딩 할때 처리함.
            btnEBT.Enabled = true;

            btnHelp.Enabled = true;
            //KeyInReady();
        }

        private void btnPointUseYes_Click(object sender, EventArgs e)
        {
            // Point card 결제
            transitionSiglePage(gbProcessPointCard, 1023, 200);
            ProcessPointCardPayment();
        }

        private void ProcessPrtReceipt()
        {
            PrtReceipt.PrintController = new System.Drawing.Printing.StandardPrintController();
            PrtReceipt.Print();

            //PrtCoupon.PrintController = new System.Drawing.Printing.StandardPrintController();
            //PrtCoupon.Print();

            bgw_ReceiptPrinting.CancelAsync();
        }
        
        private void bgw_ReceiptPrinting_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1000);
            ProcessPrtReceipt();
        }

        private void bgw_ReceiptPrinting_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            gbProcessCreditCard.Visible = false;
            gbProcessEBT.Visible = false;

            transitionSiglePage(gbReceiptPrinting, 1023, 200);
            transitionSiglePage(gbProcessCreditCard, 1023, 200);
            transitionSiglePage(gbProcessEBT, 1023, 200);
            transitionSiglePage(gbProcessPointCard, 1023, 200);
            transitionDoublePage(pnReview, pn_ItemScan, 0, 1 * GROUP_BOX_LEFT, 300);

            tbStep3Name.ForeColor = System.Drawing.Color.Silver;
            tbStep4Name.ForeColor = System.Drawing.Color.DimGray;
            st_ProcessStatus.CurrentStep = 4;

            //btnNext.Text = "NEXT";
            btnNext.Font = new System.Drawing.Font("Helvetica85-Heavy", 24F, FontStyle.Bold);

            lbBagCount.Text = "1";
            lbBalancePointTop.Visible = false;
            lbCurrentCusBal.Visible = false;
            
            //cpCreditCard.IsRunning = false;
            gbHelp.Visible = false;
            btnSearch.Enabled = true;
            btnSearch_Category.Enabled = true;

            // 음성 실행.
            if (GssThankyou.GetCurrentlySpokenPrompt() == null)                // 음성 중복 재생 방지.
            {
                //GssThankyou.SpeakAsyncCancel(GssThankyou.GetCurrentlySpokenPrompt());
                GssThankyou.SelectVoice(GstrVoice);
                GssThankyou.SpeakAsync("Thank you for using Self Check Out.");
            }

            //GssThankyou.SelectVoice(GstrVoice);
            //GssThankyou.SpeakAsync("Thank you for using Self Check Out.");
            ////////////////////////////
            // Sync Process 실행
            //Process.Start(Application.StartupPath + "\\HanaSyncData.bat");
            if (GblTestMode == false)
            {
                ProcessHanaSyncData();
            }
            // 화면 전화 이후 Start 화면으로 자동으로 변경되기 위한 Timer. 
            ReviewToStartTimer.Interval = 5000;
            ReviewToStartTimer.Start();
        }

        //private void ProcessQLightControl(string strCommandColor)
        public void ProcessQLightControl(string strCommandColor)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            g_sMessage = string.Format("[{0}] Enter processQligtControl", sMethod);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);
            string strBatchFile_Command = Application.StartupPath + "\\Script\\qlight.bat " + strCommandColor;

            // Self Check Out 장비가 아닌 캐셔대 인 경우
            if (c_poscominfo.ci_mklocation == 1 && c_poscominfo.ci_mkno == "61" && (c_poscominfo.si_counternum == "1" || c_poscominfo.si_counternum == "10" || c_poscominfo.si_counternum == "2" || c_poscominfo.si_counternum == "11"))            
            //if (c_poscominfo.ci_mklocation == 3 && c_poscominfo.ci_mkno == "52")            // 임시 테스트 용
            {
                //bool bLampCommandResult = false;                                      // 테스트 완료 경광봉 설치시 사용할 코드.
                //byte* btLampCommand = stackalloc byte[6];

                //if(strCommandColor != "rb1")
                //{
                //    btLampCommand = SetCommandByte(strCommandColor);
                //}
                
                return;
            }
            
            ProcessStartInfo psQlightCommand = new ProcessStartInfo("cmd.exe", "/C " + strBatchFile_Command);
            psQlightCommand.WindowStyle = ProcessWindowStyle.Hidden;
            psQlightCommand.CreateNoWindow = true;
            psQlightCommand.UseShellExecute = false;
            psQlightCommand.RedirectStandardOutput = true;

            Process pQlightCommand = new Process();

            pQlightCommand.StartInfo = psQlightCommand;
            pQlightCommand.Start();
            pQlightCommand.WaitForExit();
            pQlightCommand.Close();
        }

        //unsafe public byte* SetCommandByte(string strCommandColor)
        //{
        //    string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
        //    bool bLampCommandResult = false;

        //    g_sMessage = string.Format("[{0}] SetColor", strCommandColor);
        //    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

        //    byte* btLampCommand = stackalloc byte[6];
        //    btLampCommand[5] = 0; // sound OFF
        //    int i = 0;
        //    switch (strCommandColor)
        //    {
        //        case "a0":  for (i = 0; i < 5; i++) {   btLampCommand[i] = g_Lampoff;   }   break;
        //        case "a1":  for (i = 0; i < 5; i++) {   btLampCommand[i] = g_Lampon;    }   break;
        //        case "r0":                          {   btLampCommand[0] = g_Lampoff;   }   break;
        //        case "r1":                          {   btLampCommand[0] = g_Lampon;    }   break;
        //        case "r2":  for (i = 0; i < 5; i++) {   btLampCommand[i] = g_Lampblink; }   break;
        //        case "o0":                          {   btLampCommand[1] = g_Lampoff;   }   break;
        //        case "o1":                          {   btLampCommand[1] = g_Lampon;    }   break;
        //        case "o2":  for (i = 0; i < 5; i++) {   btLampCommand[i] = g_Lampblink; }   break;
        //        case "g0":                          {   btLampCommand[2] = g_Lampoff;   }   break;
        //        case "g1":                          {   btLampCommand[2] = g_Lampon;    }   break;
        //        case "g2":  for (i = 0; i < 5; i++) {   btLampCommand[i] = g_Lampblink; }   break;
        //        //default:   for (i = 0; i < 5; i++)  {   btLampCommand[2] = g_Lampoff;   }   break;
        //    }

        //    // USB Connect Open
        //    Usb_Qu_Open();
        //    // Writh to USB QLight Lamp             
        //    bLampCommandResult = Usb_Qu_write(0, 0, btLampCommand);             // Usb_Qu_write(0번 Light, 0번 Type, Command Byte)

        //    g_sMessage = string.Format("[{0}] processQligtControl Write Result", bLampCommandResult);
        //    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

        //    return btLampCommand;
        //}

        //private bool Usb_Qu_write(int v1, int v2, char c_char)
        //{
        //    throw new NotImplementedException();
        //}
        
        public void ProcessHanaSyncData()
        {
            string strBatchFile_Command = Application.StartupPath + "\\HanaSyncData.bat";

            ProcessStartInfo psHanaSyncDataCommand = new ProcessStartInfo("cmd.exe", "/C " + strBatchFile_Command);
            psHanaSyncDataCommand.WindowStyle = ProcessWindowStyle.Hidden;
            psHanaSyncDataCommand.CreateNoWindow = true;
            psHanaSyncDataCommand.UseShellExecute = false;
            psHanaSyncDataCommand.RedirectStandardOutput = true;

            Process pHanaSyncDataCommand = new Process();

            pHanaSyncDataCommand.StartInfo = psHanaSyncDataCommand;
            pHanaSyncDataCommand.Start();
            pHanaSyncDataCommand.WaitForExit();
            pHanaSyncDataCommand.Close();
        }

        private void ProcessDiscountCoupon(string strCouponCode)
        {
            string sQBuff = string.Empty;
            long lReturn = 0;

            string strdccode = string.Empty;
            string strdcprodid = string.Empty;
            int iDCusegb = 0;
            decimal dDCAmt = 0;
            decimal dDCRate = 0;
            int iDConeforhouse = 1;
            bool bDcmatchprod = false;
            bool bDCwithTax = false;
            bool bDCtreatascash = false;
            bool bCoupon = false;
            bool bUseValidPeriod = false;
            string strCouponProdID = string.Empty;
            decimal dProdIUprice = 0;
            decimal dProdTax = 0;
            decimal dCouponDCAmt = 0;
            decimal dCouponDCGst = 0;
            decimal dCouponDCPst = 0;
            decimal dCouponDCHst = 0;
            decimal dtIUprice = 0;
            decimal dtOUPrice = 0;
            string lblMessage = "";
            string strTempProdName = string.Empty;
            decimal value = 0;
            string strseq = string.Empty;
            string strtID = string.Empty;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            //g_sMessage = string.Format("[{0}] Process Start Discount Coupon : (ctrlOnScreen : {1}).", sMethod, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            try
            {
                OrderItem itemdc;

                // Dicount Coupon Table 에서 현재 스캔된 아이템 중에 해당되는 아이템이 있는지 확인.
                lReturn = c_localdb.DBConnection();

                if (lReturn < 0)
                {
                    g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);
                    c_localdb.RsClose();
                    return;
                }
                  
                sQBuff = "SELECT * FROM tb_DiscountCoupon WHERE dc_void <> 1 AND dc_deleted <> 1 AND dc_code = '" + strCouponCode + "'";
                lReturn = c_localdb.RsOpen(sQBuff);

                if (!c_localdb.rs.EOF)
                {           // 설정된 Coupon이 있는 경우
                    //bUseValidPeriod = Convert.ToBoolean(Convert.ToInt32(c_localdb.rs.Fields["dc_UseValidPeriod"]));
                    bUseValidPeriod = c_localdb.rs.Fields["dc_UseValidPeriod"].Value != DBNull.Value && Convert.ToBoolean(c_localdb.rs.Fields["dc_UseValidPeriod"].Value);

                    DateTime today = DateTime.Today;
                    object sValidObj = c_localdb.rs.Fields["dc_svaliddate"].Value;
                    object eValidObj = c_localdb.rs.Fields["dc_evaliddate"].Value;

                    DateTime sValid;
                    DateTime eValid;

                    if (bUseValidPeriod)
                    {
                        sValid = Convert.ToDateTime(sValidObj);
                        eValid = Convert.ToDateTime(eValidObj);

                        if (today < sValid || today > eValid)
                        {
                            DisplayErrorMessageBox("Item Discount", "This Coupon is not validated.", 1, sMethod);
                            //c_localdb.RsClose();
                            return; // 유효기간 초과
                        }                            
                    }

                    bCoupon = true;
                    // 변수 세팅
                    strdccode = Convert.ToString(c_localdb.rs.Fields["dc_code"].Value);
                    strdcprodid = Convert.ToString(c_localdb.rs.Fields["dc_prodid"].Value);
                    dDCAmt = Convert.ToDecimal(c_localdb.rs.Fields["dc_Amt"].Value);
                    iDConeforhouse = Convert.ToInt32(c_localdb.rs.Fields["dc_oneforhouse"].Value) != 0 ? Convert.ToInt32(c_localdb.rs.Fields["dc_oneforhouse"].Value) : 1;
                    bDcmatchprod = Convert.ToInt32(c_localdb.rs.Fields["dc_MatchProd"].Value) != 0;
                    dDCRate = Convert.ToDecimal(c_localdb.rs.Fields["dc_rate"].Value);
                    iDCusegb = string.IsNullOrEmpty(c_localdb.rs.Fields["dc_UseGb"].ToString()) ? 0 : Convert.ToInt32(c_localdb.rs.Fields["dc_UseGb"].Value);
                    bDCwithTax = Convert.ToInt32(c_localdb.rs.Fields["dc_withtax"].Value) != 0;
                    bDCtreatascash = c_localdb.rs.Fields["dc_treatascash"].Value != DBNull.Value && Convert.ToBoolean(c_localdb.rs.Fields["dc_treatascash"].Value);
                    
                    switch (iDCusegb)
                    {
                        case 1:
                            dDCAmt = 0;
                            break;
                        case 2:
                            dDCRate = 0;
                            break;
                        case 3:
                            //dDCAmt = couponBaseAmount;
                            dDCRate = 0;
                            break;
                        //case 4:
                        //    strCouponProdID = strdcprodid;
                        //    wprodIUprice = GetProductPrice(wProdID); // 구현 필요
                        //    intQty = (int)wDCAmt;
                        //    wDCAmt = wprodIUprice;
                        //    wdcRate = 0;
                        //    break;
                        default:
                            dDCAmt = 0;
                            dDCRate = 0;
                            break;
                    }

                    if ((dDCAmt == 0 && dDCRate == 0) || iDCusegb == 0)
                    {
                        DisplayErrorMessageBox("Item Discount", "This Coupon Setting is not validated.", 1, sMethod);
                        //c_localdb.RsClose();
                        return;
                    }
                    c_localdb.RsClose();

                    // 2. 쿠폰 사용 제한 확인
                    if (iDConeforhouse > 0)
                    {
                        sQBuff = "SELECT COUNT(tEntryCode)AS 'CouponUseCount' FROM tb_OrderItem Where tInvNo = '" + txtInvNo.Text + "' And tEntryCode = '" + strdccode + "'";
                        lReturn = c_localdb.RsOpen(sQBuff);

                        if (lReturn == 1)
                        {
                            int iUsedCount = Convert.ToInt32(c_localdb.rs.Fields["CouponUseCount"].Value);
                            if (iDConeforhouse <= iUsedCount)
                            {
                                DisplayErrorMessageBox("Item Discount", "This Coupon is already used until coupon limit on this sale", 1, sMethod);
                                iDCusegb = 0;
                                //c_localdb.RsClose();
                                return;
                            }
                        }
                    }
                    c_localdb.RsClose();

                    // 3. 해당 상품 구매 여부 확인
                    sQBuff = "SELECT * , tx_tax1, tx_tax2, tx_tax3 FROM tb_OrderItem LEFT JOIN tb_Tax ON tx_cd = tTax AND tTax != '' LEFT JOIN mfProd P ON tProd = prodId WHERE tInvNo = '" + txtInvNo.Text + "' AND tProd = '" + strdcprodid + "' AND tType = ''";
                    lReturn = c_localdb.RsOpen(sQBuff);

                    if (lReturn >= 1 && !c_localdb.rs.EOF)
                    {
                        //dProdTax = Convert.ToDecimal(c_localdb.rs.Fields["tTax"].Value);
                        string strUnit = c_localdb.rs.Fields["tPunit"].Value.ToString();

                        strtID = c_localdb.rs.Fields["tID"].Value.ToString();

                        value = (strUnit == "LB" || strUnit == "KG" || strUnit == "100G" || strUnit == "OZ")
                                ? Convert.ToDecimal(c_localdb.rs.Fields["tAmt"].Value)
                                : Convert.ToDecimal(c_localdb.rs.Fields["tOUPrice"].Value);

                        if (iDCusegb == 1)
                        {
                            dCouponDCAmt = -value * (dDCRate / 100);
                            
                            if (bDCwithTax)
                            {
                                dCouponDCGst = Convert.ToDecimal(c_localdb.rs.Fields["tGst"].Value) * (dDCRate / 100);
                                dCouponDCPst = Convert.ToDecimal(c_localdb.rs.Fields["tPst"].Value) * (dDCRate / 100);
                                dCouponDCHst = Convert.ToDecimal(c_localdb.rs.Fields["tHst"].Value) * (dDCRate / 100);

                                //dCouponDCAmt = dCouponDCAmt + dCouponDCGst + dCouponDCPst + dCouponDCHst;
                            }
                            else
                            {
                                dCouponDCGst = 0;
                                dCouponDCPst = 0;
                                dCouponDCHst = 0;
                            }
                        }
                        else
                        {
                            dCouponDCAmt = -dDCAmt;

                            if (bDCwithTax)
                            {
                                dCouponDCGst = Convert.ToDecimal(c_localdb.rs.Fields["tGst"].Value) * (dDCAmt / value);
                                dCouponDCPst = Convert.ToDecimal(c_localdb.rs.Fields["tPst"].Value) * (dDCAmt / value);
                                dCouponDCHst = Convert.ToDecimal(c_localdb.rs.Fields["tHst"].Value) * (dDCAmt / value);

                                dCouponDCAmt = dCouponDCAmt + dCouponDCGst + dCouponDCPst + dCouponDCHst;
                            }
                            else
                            {
                                dCouponDCGst = 0;
                                dCouponDCPst = 0;
                                dCouponDCHst = 0;
                            }
                        }
                    }
                    else
                    {
                        if (bDcmatchprod)
                        {
                            DisplayErrorMessageBox("Item Discount", "Related Item Not Found or Already discounted.", 1, sMethod);
                            iDCusegb = 0;
                            //c_localdb.RsClose();
                            return;
                        }
                        if (iDCusegb != 1)
                        {
                            dCouponDCAmt = -dDCAmt;
                        }
                        else
                        {
                            c_localdb.RsClose();
                            sQBuff = "SELECT prodOUprice FROM mfProd WHERE prodid = '" + strdcprodid + "'";
                            lReturn = c_localdb.RsOpen(sQBuff);

                            if (lReturn == 1)
                            {
                                decimal price = Convert.ToDecimal(c_localdb.rs.Fields["prodOUprice"].Value);
                                dCouponDCAmt = -price * (dDCRate / 100);
                            }
                            else
                            {
                                DisplayErrorMessageBox("Item Discount", "Related Item Not Found.", 1, sMethod);
                                iDCusegb = 0;

                                //c_localdb.RsClose();
                                return;
                            }
                        }
                    }
                    c_localdb.RsClose();

                    // 4. Item Discount Coupon 금액 리스트에 입력.
                   
                    sQBuff = "SELECT O.*, p.prodName, p.prodKname, T.tx_tax1, T.tx_tax2, T.tx_tax3 " +
                         "FROM tb_OrderItem O LEFT JOIN tb_Tax T ON tx_cd = tTax AND tTax != '' LEFT JOIN mfProd P ON tProd = prodId " +
                         "WHERE tInvno = '" + txtInvNo.Text + "' AND tProd = '" + strdcprodid + "'";

                    lReturn = c_localdb.RsOpen(sQBuff);

                    if (lReturn == 1)
                    {
                        while (!c_localdb.rs.EOF)
                        {
                            itemdc.tInvNo = txtInvNo.Text;
                            itemdc.tID = "";
                            itemdc.tProd = strdcprodid;

                            strTempProdName = "D.C - " + Convert.ToString(c_localdb.rs.Fields["prodName"].Value);
                            if (strTempProdName.Length >= 20)
                            {
                                strTempProdName = strTempProdName.Substring(0, 20);
                            }
                            itemdc.tProdName = strTempProdName + "-Coupon";
                            itemdc.tProdKname = "";

                            itemdc.tPtype = Convert.ToString(c_localdb.rs.Fields["tPtype"].Value);
                            itemdc.tPtype2 = Convert.ToString(c_localdb.rs.Fields["tPtype2"].Value);
                            itemdc.tCat1 = Convert.ToString(c_localdb.rs.Fields["tCat1"].Value);
                            itemdc.tCat2 = Convert.ToString(c_localdb.rs.Fields["tCat2"].Value);
                            itemdc.tCat3 = Convert.ToString(c_localdb.rs.Fields["tCat3"].Value);
                            itemdc.tCat4 = Convert.ToString(c_localdb.rs.Fields["tCat4"].Value);
                            itemdc.tCat5 = Convert.ToString(c_localdb.rs.Fields["tCat5"].Value);

                            itemdc.tQty = Convert.ToString(c_localdb.rs.Fields["tQty"].Value);
                            itemdc.tIUprice = Convert.ToString(c_localdb.rs.Fields["tIUprice"].Value);
                            itemdc.tOUprice = (Math.Round((double)dCouponDCAmt * Convert.ToDouble(c_localdb.rs.Fields["tQty"].Value), 2)).ToString(); // DC Rate 반영해서 계산
                            itemdc.tTax = "0";
                            
                            itemdc.tAmt = itemdc.tOUprice;

                            itemdc.tTax = Convert.ToString(c_localdb.rs.Fields["tTax"].Value);
                            if (itemdc.tTax != "" && Convert.ToDouble(c_localdb.rs.Fields["tGst"].Value) != 0)
                            {
                                itemdc.tGst = "-" + (Math.Round(dCouponDCGst,2)).ToString();
                            }
                            else
                            {
                                itemdc.tGst = "0";
                            }

                            if (itemdc.tTax != "" && Convert.ToDouble(c_localdb.rs.Fields["tPst"].Value) != 0)
                            {
                                itemdc.tPst = "-" + (Math.Round(dCouponDCPst, 2)).ToString();
                            }
                            else
                            {
                                itemdc.tPst = "0";
                            }

                            if (itemdc.tTax != "" && Convert.ToDouble(c_localdb.rs.Fields["tHst"].Value) != 0)
                            {
                                itemdc.tHst = "-" + (Math.Round(dCouponDCHst, 2)).ToString();
                            }
                            else
                            {
                                itemdc.tHst = "0";
                            }

                            itemdc.tType = "48";
                            if (iDCusegb == 1)
                            {
                                itemdc.tNative = (dDCRate / 100).ToString();
                            }
                            else
                            {
                                itemdc.tNative = (dDCAmt / value).ToString();
                            }
                            
                            itemdc.tPromo = "";
                            itemdc.tPromoCode = "";
                            itemdc.tCust = Convert.ToString(c_localdb.rs.Fields["tCust"].Value);
                            itemdc.tPassWord = Convert.ToString(c_localdb.rs.Fields["tPassWord"].Value);
                            itemdc.tPassStation = Convert.ToString(c_localdb.rs.Fields["tPassStation"].Value);
                            itemdc.tUpCode = "";
                            itemdc.tSpecial = "";
                            itemdc.tFree = "";
                            itemdc.tMMBC = "";
                            itemdc.tSupp = "";
                            itemdc.tEntryCode = "";
                            itemdc.tFoodStamp = "0";
                            itemdc.tGiftCardRef = "";
                            itemdc.tRelatedID = Convert.ToString(c_localdb.rs.Fields["tID"].Value);
                            strseq = Convert.ToString(c_localdb.rs.Fields["tID"].Value);
                            itemdc.tMixMatch = "";
                            itemdc.tShift = "";
                            itemdc.tMemo = "";

                            if (itemdc.tAmt != "0")
                            {
                                AddOrderItem(itemdc);
                            }
                            c_localdb.rs.MoveNext();
                        }
                    }
                    else
                    {
                        g_sMessage = string.Format("[{0}] Item Discount Order Item query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                        c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                        c_localdb.DBClose();

                        return;
                    }
                }
                else
                {           // 설정된 Coupon이 없는 경우
                    DisplayErrorMessageBox("Item Discount", "This Coupon is not validated.", 1, sMethod);
                    return;
                }

                c_localdb.RsClose();

                // Origial Item tType에 11로 표시                
                sQBuff = "UPDATE tb_OrderItem set tType = '11' WHERE tInvno = '" + txtInvNo.Text + "' AND tProd = '" + strdcprodid + "' and tID = '" + strtID + "'";
                c_localdb.DBExcute(sQBuff);

                if (strdcprodid != "" && strseq != "")
                {
                    initListView();
                    initSalesTotal();

                    PopulateListViewProdItem("ItemDiscount"); // Parameter 구분해서 별도 적용 필요한지 확인 필요..

                    KeyInReady();

                    ProcessTotalDC(); // 실행 필요 여부 확인
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
        }

        private void ProcessItemDiscountBarcode()
        {
            string strProdId = "", strSeq = "", strType = "";
            string strTempProdName = string.Empty;
            int iListViewIndex = 0;
            double dblDCRate = 0;

            string sQBuff = string.Empty;
            long lReturn = 0;

            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;
            g_sMessage = string.Format("[{0}] Item Discount : ({1} %) (ctrlOnScreen : {2}).", sMethod, GdblItemDCRate, ctrlOnScreen.Name);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            if (ItemCSView.Items.Count > 0 && ItemCSView.SelectedItems.Count > 0)
            {
                iListViewIndex = ItemCSView.FocusedItem.Index;
                strProdId = ItemCSView.SelectedItems[0].Text;
                strSeq = ItemCSView.SelectedItems[0].SubItems[1].Text;
                strType = ItemCSView.SelectedItems[0].SubItems[7].Text;
            }
            else
            {
                return;
            }

            try
            {
                // 1. CRF. Bottle Deposit, EHF Type인 경우 DC 안됨
                if (strType == "20" || strType == "21" || strType == "22")
                {
                    DisplayErrorMessageBox("Item Discount", "Cannot apply Item Discount.", 1, sMethod);
                    return;
                }

                // 2. 변수 Clear 확인

                // 3. Total DC 적용된 경우 Exit
                if (GdblTotalDCRate > 0)
                {
                    DisplayErrorMessageBox("Item Discount", "Total DC was already applied.", 1, sMethod);
                    return;
                }

                // 4. Gift Card 구매 시 Exit - 판매 시 Block 하는 것이 맞을 것 같으므로 Item DC에서는 불필요

                // 5. Type에 Code가 들어가 있는 경우 Exit
                if (strType != "")
                {
                    DisplayErrorMessageBox("Item Discount", "Cannot apply Item DC.", 1, sMethod);
                    return;
                }

                // 6. DC Rate 할당 (바코드 파싱해서 가져옴)
                dblDCRate = GdblItemDCRate;
                GdblItemDCRate = 0; // Item Add 전에 조건에 의해 빠져나갈 경우를 대비 미리 변수 초기화

                // 7. Item DC 50% 넘어가면 Exit
                if (dblDCRate > 50)
                {
                    DisplayErrorMessageBox("Item Discount", "Cannot apply over 50% for Item DC.", 1, sMethod);
                    return;
                }

                // 8. 변수 Clear 확인
                OrderItem itemdc;

                /* 9. DC Rate 값이 있을 경우
                 - Item 추가
                 - tType : 11 (Org Item에 Update)
                 - wtOUPrice = CDbl(wtIUprice) * (wItemDCRate / 100) * -1
                 - tType : 48
                 - tNative : DC Rate
                 - RelatedId = Org Item Seq ID
                 - Amt : tOUPrice * tQty
                 - Tax 계산
                 - Amt가 0이 아닌 경우 AddOrderItem

                */

                lReturn = c_localdb.DBConnection();

                if (lReturn < 0)
                {
                    g_sMessage = string.Format("[{0}] Local database connection failed (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    return;
                }

                sQBuff = "SELECT O.*, p.prodName, p.prodKname, T.tx_tax1, T.tx_tax2, T.tx_tax3 " +
                         "FROM tb_OrderItem O LEFT JOIN tb_Tax T ON tx_cd = tTax AND tTax != '' LEFT JOIN mfProd P ON tProd = prodId " +
                         "WHERE tInvno = '" + txtInvNo.Text + "' AND tProd = '" + strProdId + "' and tID = " + strSeq;

                lReturn = c_localdb.RsOpen(sQBuff);
                
                if (lReturn == 1)
                {
                    while (!c_localdb.rs.EOF)
                    {
                        itemdc.tInvNo = txtInvNo.Text;
                        itemdc.tID = "";
                        itemdc.tProd = strProdId;

                        strTempProdName = "D.C - " + Convert.ToString(c_localdb.rs.Fields["prodName"].Value);
                        if (strTempProdName.Length >= 20)
                        {
                            strTempProdName = strTempProdName.Substring(0, 20);
                        }
                        itemdc.tProdName = strTempProdName +"@"+ dblDCRate.ToString() + "%";
                        itemdc.tProdKname = "";

                        itemdc.tPtype = Convert.ToString(c_localdb.rs.Fields["tPtype"].Value);
                        itemdc.tPtype2 = Convert.ToString(c_localdb.rs.Fields["tPtype2"].Value);
                        itemdc.tCat1 = Convert.ToString(c_localdb.rs.Fields["tCat1"].Value);
                        itemdc.tCat2 = Convert.ToString(c_localdb.rs.Fields["tCat2"].Value);
                        itemdc.tCat3 = Convert.ToString(c_localdb.rs.Fields["tCat3"].Value);
                        itemdc.tCat4 = Convert.ToString(c_localdb.rs.Fields["tCat4"].Value);
                        itemdc.tCat5 = Convert.ToString(c_localdb.rs.Fields["tCat5"].Value);

                        itemdc.tQty = Convert.ToString(c_localdb.rs.Fields["tQty"].Value); 
                        itemdc.tIUprice = Convert.ToString(c_localdb.rs.Fields["tIUprice"].Value);
                        itemdc.tOUprice = "-" + (Math.Round((Convert.ToDouble(c_localdb.rs.Fields["tOUprice"].Value) * dblDCRate / 100) * Convert.ToDouble(c_localdb.rs.Fields["tQty"].Value), 2)).ToString(); // DC Rate 반영해서 계산
                        itemdc.tAmt = itemdc.tOUprice;

                        itemdc.tTax = Convert.ToString(c_localdb.rs.Fields["tTax"].Value);
                        if (itemdc.tTax != "" && Convert.ToDouble(c_localdb.rs.Fields["tGst"].Value) != 0)
                        {
                            itemdc.tGst = "-" + (Math.Round(Convert.ToDouble(c_localdb.rs.Fields["tGst"].Value) * (dblDCRate / 100), 2)).ToString();
                        }
                        else
                        {
                            itemdc.tGst = "0";
                        }

                        if (itemdc.tTax != "" && Convert.ToDouble(c_localdb.rs.Fields["tPst"].Value) != 0)
                        {
                            itemdc.tPst = "-" + (Math.Round(Convert.ToDouble(c_localdb.rs.Fields["tPst"].Value) * (dblDCRate / 100), 2)).ToString();
                        }
                        else
                        {
                            itemdc.tPst = "0";
                        }

                        if (itemdc.tTax != "" && Convert.ToDouble(c_localdb.rs.Fields["tHst"].Value) != 0)
                        {
                            itemdc.tHst = "-" + (Math.Round(Convert.ToDouble(c_localdb.rs.Fields["tHst"].Value) * (dblDCRate / 100), 2)).ToString();
                        }
                        else
                        {
                            itemdc.tHst = "0";
                        }

                        itemdc.tType = "48";
                        itemdc.tNative = dblDCRate.ToString();

                        itemdc.tPromo = "";
                        itemdc.tPromoCode = "";
                        itemdc.tCust = Convert.ToString(c_localdb.rs.Fields["tCust"].Value);
                        itemdc.tPassWord = Convert.ToString(c_localdb.rs.Fields["tPassWord"].Value);
                        itemdc.tPassStation = Convert.ToString(c_localdb.rs.Fields["tPassStation"].Value);
                        itemdc.tUpCode = "";
                        itemdc.tSpecial = "";
                        itemdc.tFree = "";
                        itemdc.tMMBC = "";
                        itemdc.tSupp = "";
                        itemdc.tEntryCode = "";
                        itemdc.tFoodStamp = "0";
                        itemdc.tGiftCardRef = "";
                        itemdc.tRelatedID = strSeq;
                        itemdc.tMixMatch = "";
                        itemdc.tShift = "";
                        itemdc.tMemo = "";

                        if (itemdc.tAmt != "0")
                        {
                            AddOrderItem(itemdc);
                        }
                        c_localdb.rs.MoveNext();
                    }

                }
                else
                {
                    g_sMessage = string.Format("[{0}] Item Discount Order Item query error (error code: {1})\n[{0}] {2}", sMethod, lReturn.ToString(), c_localdb.error_message);
                    c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                    c_localdb.DBClose();

                    return;
                }
                c_localdb.RsClose();

                // Origial Item tType에 11로 표시
                sQBuff = "UPDATE tb_OrderItem set tType = '11' WHERE tInvno = '" + txtInvNo.Text + "' AND tProd = '" + strProdId + "' and tID = " + strSeq;
                c_localdb.DBExcute(sQBuff);

                // 10. 변수 Clear 확인 & New Item 스캔 준비
                GdblItemDCRate = 0;
                
                // ItemDiscount 창 삭제.
                transitionSiglePage(gbItemDiscount, 1023, 200);
                ctrlOnScreen = ctrTemp;
                cpItemDiscountBarcode.IsRunning = false;

                g_ItemDiscountModeOn = false;

                if (ctrTemp != pn_ItemScan && ctrTemp != pnItemScanSearchBtn)
                {
                    // 저울에서 스캔 되는거 방지.
                    if (OPOSScanner.DeviceEnabled)
                    {
                        OPOSScanner.DataEvent -= ScannerDataEvent;
                        OPOSScanner.DeviceEnabled = false;
                    }
                }

                ProcessReprintAfterButtonControl();
                
                if (strProdId != "" && strSeq != "")
                {

                    initListView();
                    initSalesTotal();

                    PopulateListViewProdItem("ItemDiscount"); // Parameter 구분해서 별도 적용 필요한지 확인 필요..

                    KeyInReady();

                    ProcessTotalDC(); // 실행 필요 여부 확인
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

        }
    

    private void btnAgeCheckNum1_Click(object sender, EventArgs e)
        {
            if(tbAgeCheckNum.TextLength != 8)
            {
                tbAgeCheckNum.Text += "1";
            }
        }

        private void btnAgeCheckNum2_Click(object sender, EventArgs e)
        {
            if (tbAgeCheckNum.TextLength != 8)
            {
                tbAgeCheckNum.Text += "2";
            }
        }

        private void btnAgeCheckNum3_Click(object sender, EventArgs e)
        {
            if (tbAgeCheckNum.TextLength != 8)
            {
                tbAgeCheckNum.Text += "3";
            }
        }

        private void btnAgeCheckNum4_Click(object sender, EventArgs e)
        {
            if (tbAgeCheckNum.TextLength != 8)
            {
                tbAgeCheckNum.Text += "4";
            }
        }

        private void btnAgeCheckNum5_Click(object sender, EventArgs e)
        {
            if (tbAgeCheckNum.TextLength != 8)
            {
                tbAgeCheckNum.Text += "5";
            }
        }

        private void btnAgeCheckNum6_Click(object sender, EventArgs e)
        {
            if (tbAgeCheckNum.TextLength != 8)
            {
                tbAgeCheckNum.Text += "6";
            }
        }

        private void btnAgeCheckNum7_Click(object sender, EventArgs e)
        {
            if (tbAgeCheckNum.TextLength != 8)
            {
                tbAgeCheckNum.Text += "7";
            }
        }

        private void btnAgeCheckNum8_Click(object sender, EventArgs e)
        {
            if (tbAgeCheckNum.TextLength != 8)
            {
                tbAgeCheckNum.Text += "8";
            }
        }

        private void btnAgeCheckNum9_Click(object sender, EventArgs e)
        {
            if (tbAgeCheckNum.TextLength != 8)
            {
                tbAgeCheckNum.Text += "9";
            }
        }

        private void btnAgeCheckNum0_Click(object sender, EventArgs e)
        {
            if (tbAgeCheckNum.TextLength != 8)
            {
                tbAgeCheckNum.Text += "0";
            }
        }

        private void btnAgeCheckDeleteNum_Click(object sender, EventArgs e)
        {
            if (tbAgeCheckNum.Text.Length > 0)
            {
                tbAgeCheckNum.Text = tbAgeCheckNum.Text.Remove(tbAgeCheckNum.Text.Length - 1, 1);
            }

            tbAgeCheckNum.Focus();
            tbAgeCheckNum.Select(tbAgeCheckNum.Text.Length, 0);
        }

        private void btnAgeCheckExit_Click(object sender, EventArgs e)
        {
            g_CertifiedAdultReady = false;
            g_iAdultLimit = 0;

            transitionSiglePage(gbAgeCheck, 1023, 200);
            ctrlOnScreen = ctrTemp;

            // Help Mode 강제로 해제함.
            ProcessEntryCode(g_strManagerKey, 0);                    //임시로 Manager Password 입력

            tbAgeCheckNum.Clear();
            KeyInReady();
        }

        private void btnAgeCheckEnter_Click(object sender, EventArgs e)
        {
            // 입력한 숫자 Date 형태로 변경하기.
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            DateTime dtInputDateforAge, dtDateToday = DateTime.Today.AddYears(g_iAdultLimit * -1);
            string strInputDateforAge = string.Empty;
            int iCompareDateforAge = 0;

            g_sMessage = string.Format("[{0}] Age Check Key in Enter Date of Birth : {1}", sMethod, tbAgeCheckNum.Text);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            strInputDateforAge = tbAgeCheckNum.Text;

            // 입력한 값이 6자리가 아니면 Return.
            if(strInputDateforAge.Length != 6)
            {
                DisplayErrorMessageBox("Age Check", "Please Check input date. \n (EX. DEC-10-1999 => 121099", 1, sMethod);
                return;
            }

            //8자리 형태로 변환 MMDDYY -> YYYYMMDD
            if(Convert.ToInt16(strInputDateforAge.Substring(4,2)) > 20)
            {
                strInputDateforAge = "19" + strInputDateforAge.Substring(4, 2) + strInputDateforAge.Substring(0, 4);
            }
            else
            {
                strInputDateforAge = "20" + strInputDateforAge.Substring(4, 2) + strInputDateforAge.Substring(0, 4);
            }

            g_sMessage = string.Format("[{0}] Transform MMDDYY to YYYYMMDD {1}", sMethod, strInputDateforAge);
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            if (strInputDateforAge.Length == 8)
            {
                //Verified Date
                if (Convert.ToInt16(strInputDateforAge.Substring(0, 4)) <= 1900
                    || Convert.ToInt16(strInputDateforAge.Substring(4, 2)) == 00 || Convert.ToInt16(strInputDateforAge.Substring(4, 2)) > 12
                    || Convert.ToInt16(strInputDateforAge.Substring(6, 2)) > 31)
                {
                    // Error Message
                    //DisplayErrorMessageBox("Age Check", "Please Check input date. \n (EX. 1999-DEC-10 => 19991210", 1, sMethod);
                    DisplayErrorMessageBox("Age Check", "Please Check input date. \n (EX. DEC-10-1999 => 121099", 1, sMethod);
                    return;
                }

                // 날짜형태로 변환.
                strInputDateforAge = strInputDateforAge.Substring(0, 4) + "-" + strInputDateforAge.Substring(4, 2) + "-" + strInputDateforAge.Substring(6, 2);
                dtInputDateforAge = Convert.ToDateTime(strInputDateforAge);
                iCompareDateforAge = DateTime.Compare(dtInputDateforAge, dtDateToday);

                if(iCompareDateforAge <= 0)                         // 성인 인 경우
                {
                    g_CertifiedAdultReady = false;
                    g_CertifiedAdult = true;

                    g_iStoredAdultLimit = g_iAdultLimit;

                    tbAgeCheckNum.Clear();
                    // 입력창 사라지고 Add Item 다시 실행 시킨다. Help Mode 강제로 해제함.
                    transitionSiglePage(gbAgeCheck, 1023, 200);
                    ctrlOnScreen = ctrTemp;

                    // Help Mode 강제로 해제함.
                    ProcessEntryCode(g_strManagerKey, 0);                    //임시로 Manager Password 입력

                    // 아이템 입력.
                    ProcessItemSale(g_strAgeCheckforProdID, 1);
                }
                else                                        // 나이가 어린 경우
                {
                    //Error Message 
                    DisplayErrorMessageBox("Age Check Decline", "It is not eligible age in order to buy this item.(Age Limit : " + Convert.ToString(g_iAdultLimit)+ ")", 1, sMethod);
                    g_CertifiedAdultReady = false;
                    //g_CertifiedAdult = false;
                    g_iAdultLimit = 0;

                    transitionSiglePage(gbAgeCheck, 1023, 200);
                    ctrlOnScreen = ctrTemp;
                }
            }
            else                        // 생년월일을 잘못입력한 경우.
            {
                // Error Message
                //DisplayErrorMessageBox("Age Check", "Please Check input date. \n (EX. 1999-DEC-10 => 19991210", 1, sMethod);
                DisplayErrorMessageBox("Age Check", "Please Check input date. \n (EX. DEC-10-1999 => 121099", 1, sMethod);
                return;
            }
        }

        private void ProcessBeepSoundTimer_Tick(object sender, EventArgs e)
        {
            ProcessBeepSoundTimer.Stop();
            
            ProcessBeepSoundTimer.Interval = g_iProcessBeepSoundTimer;
            ProcessBeepSoundTimer.Start();

            bgw_ProcessBeepSound.RunWorkerAsync();
        }

        private void bgw_ProcessBeepSound_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(2300);
            ProcessPlayBeep();
        }

        private void bgw_ProcessBeepSound_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void ProcessPlayBeep()
        {
            if(g_bBreakBeepSound)
            {
                Console.Beep(800, 1000);
            }               
            bgw_ProcessBeepSound.CancelAsync();
        }
        private void PrintStatusCheckTimer_Tick(object sender, EventArgs e)
        {
            PrintStatusCheckTimer.Stop();

            ChkPrintStatus();
        }

        private void ChkPrintStatus()
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            g_sMessage = string.Format("[{0}] CUSTOM K80 Printer ChkPrintStatus function enter : {1}", sMethod, g_bPrinterStatusInit.ToString());
            c_colib.cWriteLogs(g_sProcessor, g_sMessage);

            if (g_bPrinterStatusInit)        // Printer Init Done
            {
                g_sMessage = string.Format("[{0}] CUSTOM K80 Printer ChkPrintStatus No Paper Status : {1}", sMethod, CustomReceiptPrt.GetPrinterFullStatus().StsNOPAPER.ToString());
                c_colib.cWriteLogs(g_sProcessor, g_sMessage);

                // Printer Status Check.
                if (CustomReceiptPrt.GetPrinterFullStatus().StsNOPAPER)
                {
                    btnStart.Enabled = false;
                    
                    // Light On
                    ProcessQLightControl("a0");     // All Light Off
                    ProcessQLightControl("o2");     // All Light Flash
                    ProcessQLightControl("rb1");     // Beep

                    //Error Message
                    DisplayErrorMessageBox("Printer Status", "Please Check Roll Paper for Receipt.", 1, sMethod);
                    return;
                }
                else                // Update Check Timer Start
                {
                    // Update Timer Start.
                    UpdateCheckTimer.Interval = g_iUpdateInterval;
                    UpdateCheckTimer.Start();
                }
            }
            else
            {
                InitPrinterStatus();

                PrintStatusCheckTimer.Interval = g_iPrinterStatusInterval;
                PrintStatusCheckTimer.Start();
            }
        }

        private void ShowErrorMessage(Exception ex)
        {
            string sMethod = System.Reflection.MethodBase.GetCurrentMethod().Name;

            string strErrorDescription = "";
            //string strErrorOrigin = "";

            if (ex.GetType() == typeof(CuCustomWndAPIWrapException))
            {
                //strErrorOrigin = "CuCustomWndAPIWrapException Error";
                strErrorDescription = ((CuCustomWndAPIWrapException)ex).ErrorDescription;
            }
            else
            {
                //strErrorOrigin = "Exception Error";
                strErrorDescription = ex.ToString();
            }
            
            // Error Message
            DisplayErrorMessageBox("Printer Status", strErrorDescription, 1, sMethod);            
        }
    }
}
