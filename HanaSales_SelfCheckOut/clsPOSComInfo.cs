using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HanaSales_SelfCheckOut
{
    class clsPOSComInfo
    {
        public static CompInfo ciCompInfo;
        public static StationInfo siStationInfo;
        public static UserInfo uiLoginInfo;
        public static MemberInfo miMembership;

        public string[] saMemberPrefix;
        public string[] saVIPMembership;

        public string ci_mkno
        {
            get { return ciCompInfo.sCompMkNo; }
            set { ciCompInfo.sCompMkNo = value; }
        }

        public int ci_mklocation
        {
            get { return ciCompInfo.iCompArea; }
            set { ciCompInfo.iCompArea = value; }
        }

        public void ClearCompInfo()
        {
            ciCompInfo.iCompArea = 0;                  // Company location (Area). 1: 밴쿠버, 2: 토론토, 3: USA
            ciCompInfo.sCompMkNo = string.Empty;       // Market No
            ciCompInfo.sCompName = string.Empty;
            ciCompInfo.sCompAddr = string.Empty;
            ciCompInfo.sCompTelNo = string.Empty;
            ciCompInfo.sCompFaxNo = string.Empty;
            ciCompInfo.sCompGSTNo = string.Empty;
            ciCompInfo.sCompPSTNo = string.Empty;
        }

        public string si_counternum
        {
            get { return siStationInfo.sCounterNum; }
            set { siStationInfo.sCounterNum = value; }
        }

        public bool si_touchuse
        {
            get { return siStationInfo.bTouchUse; }
            set { siStationInfo.bTouchUse = value; }
        }

        public bool si_customermonitor
        {
            get { return siStationInfo.bCustomerMonitor; }
            set { siStationInfo.bCustomerMonitor = value; }
        }

        public int si_scaletype
        {
            get { return siStationInfo.iScaleType; }
            set { siStationInfo.iScaleType = value; }
        }

        public int si_scaleport
        {
            get { return siStationInfo.iScalePort; }
            set { siStationInfo.iScalePort = value; }
        }

        public bool si_scaleuse
        {
            get { return siStationInfo.bScaleUse; }
            set { siStationInfo.bScaleUse = value; }
        }
        public int si_StationEnabled
        {
            get { return siStationInfo.iStationEnabled; }
            set { siStationInfo.iStationEnabled = value; }
        }
        public string si_StationIPGroup
        {
            get { return siStationInfo.sIPGroup; }
            set { siStationInfo.sIPGroup = value; }
        }

        public string si_StationDBSvr
        {
            get { return siStationInfo.sDBSvr; }
            set { siStationInfo.sDBSvr = value; }
        }
        public string si_PinpadStationID
        {
            get { return siStationInfo.sPinpadStationID; }
            set { siStationInfo.sPinpadStationID = value; }
        }
        public string si_MarketGstNum
        {
            get { return siStationInfo.sMarketGstNum; }
            set { siStationInfo.sMarketGstNum = value; }
        }
        public string si_sPrinter1
        {
            get { return siStationInfo.sPrinter1; }
            set { siStationInfo.sPrinter1 = value; }
        }
        
        public void ClearStationInfo()
        {
            siStationInfo.sCounterNum = string.Empty;
            siStationInfo.bTouchUse = false;
            siStationInfo.bCustomerMonitor = false;
            siStationInfo.iScaleType = 0;          // 1: Datalogics, 2: NCR, 3: Datalogics w8500xt 0 (Type 1 과 동일), 4: Datalogics w8500xt 1, 9: OPOS
            siStationInfo.iScalePort = 0;
            siStationInfo.iPinPort = 0;
            siStationInfo.iStationEnabled = 0;
            siStationInfo.sPrinter1 = string.Empty;
            siStationInfo.sPrinter2 = string.Empty;
            siStationInfo.sPrinter3 = string.Empty;
            siStationInfo.sIPGroup = string.Empty;
            siStationInfo.sDBSvr = string.Empty;
            siStationInfo.sPinpadStationID = string.Empty;
            siStationInfo.sMarketGstNum = string.Empty;
            siStationInfo.sPrinter1 = string.Empty;
        }

        public string ui_epno
        {
            get { return uiLoginInfo.sEpNo; }
            set { uiLoginInfo.sEpNo = value; }
        }

        public string ui_epname
        {
            get { return uiLoginInfo.sEpName; }
            set { uiLoginInfo.sEpName = value; }
        }

        public void ClearUserInfo()
        {
            uiLoginInfo.sEpNo = string.Empty;
            uiLoginInfo.sEpName = string.Empty;
        }

        public string mi_store
        {
            get { return miMembership.sStore; }
            set { miMembership.sStore = value; }
        }

        public int mi_custno
        {
            get { return miMembership.iCustNo; }
            set { miMembership.iCustNo = value; }
        }

        public string mi_cardno
        {
            get { return miMembership.sCardNo; }
            set { miMembership.sCardNo = value; }
        }

        public string mi_name
        {
            get { return miMembership.sName; }
            set { miMembership.sName = value; }
        }

        public string mi_first
        {
            get { return miMembership.sFirst; }
            set { miMembership.sFirst = value; }
        }

        public string mi_telno
        {
            get { return miMembership.sTelNo; }
            set { miMembership.sTelNo = value; }
        }

        public string mi_email
        {
            get { return miMembership.sEmail; }
            set { miMembership.sEmail = value; }
        }

        public string mi_membertype
        {
            get { return miMembership.sMemberType; }
            set { miMembership.sMemberType = value; }
        }

        public long mi_pointbalance
        {
            get { return miMembership.lPointBalance; }
            set { miMembership.lPointBalance = value; }
        }

        public long mi_pointearned
        {
            get { return miMembership.lPointEarned; }
            set { miMembership.lPointEarned = value; }
        }

        public long mi_pointused
        {
            get { return miMembership.lPointUsed; }
            set { miMembership.lPointUsed = value; }
        }

        public int mi_origin
        {
            get { return miMembership.iOrigin; }
            set { miMembership.iOrigin = value; }
        }

        public double mi_staffbal
        {
            get { return miMembership.dStaffBal; }
            set { miMembership.dStaffBal = value; }
        }

        public string mi_nph
        {
            get { return miMembership.sNPH; }
            set { miMembership.sNPH = value; }
        }

        public string mi_asla
        {
            get { return miMembership.sASLA; }
            set { miMembership.sASLA = value; }
        }

        public string mi_apno
        {
            get { return miMembership.sAPNo; }
            set { miMembership.sAPNo = value; }
        }

        public string mi_tpno
        {
            get { return miMembership.sTPNo; }
            set { miMembership.sTPNo = value; }
        }

        public bool mi_void
        {
            get { return miMembership.bVoid; }
            set { miMembership.bVoid = value; }
        }

        public void ClearMemberInfo()
        {
            miMembership.sStore = string.Empty;
            miMembership.iCustNo = 0;
            miMembership.sCardNo = string.Empty;
            miMembership.sName = string.Empty;
            miMembership.sFirst = string.Empty;
            miMembership.sTelNo = string.Empty;
            miMembership.sEmail = string.Empty;
            miMembership.sMemberType = string.Empty;
            miMembership.lPointBalance = 0;
            miMembership.lPointEarned = 0;
            miMembership.lPointUsed = 0;
            miMembership.iOrigin = 0;
            miMembership.dStaffBal = 0;
            miMembership.sNPH = string.Empty;
            miMembership.sASLA = string.Empty;
            miMembership.sAPNo = string.Empty;
            miMembership.sTPNo = string.Empty;
            miMembership.bVoid = false;
        }
    }
}
