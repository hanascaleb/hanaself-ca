public struct CompInfo
{
    public int iCompArea;                  // Company location (Area). 1: 밴쿠버, 2: 토론토, 3: USA
    public string sCompMkNo;                  // Market No
    public string sCompName;
    public string sCompAddr;
    public string sCompTelNo;
    public string sCompFaxNo;
    public string sCompGSTNo;
    public string sCompPSTNo;
}

public struct StationInfo
{
    public string sCounterNum;
    public bool bTouchUse;
    public bool bCustomerMonitor;
    public int iScaleType;          // 0: Scale/Scanner 사용 안함, 1: Datalogics, 2: NCR, 3: Datalogics w8500xt 0 (Type 1 과 동일), 4: Datalogics w8500xt 1, 9: OPOS
    public int iScalePort;
    public bool bScaleUse;
    public int iPinPort;
    public string sPrinter1;
    public string sPrinter2;
    public string sPrinter3;
    public int iStationEnabled;
    public string sIPGroup;
    public string sDBSvr;
    public string sPinpadStationID;
    public string sMarketGstNum;
}

public struct UserInfo
{
    public string sEpNo;
    public string sEpName;
}

public struct OrderInfo
{
    public string sInvNo;
    public int iSeq;
    public string sProdId;
    public string sProdName;
    public string sProdKname;
    public double dProdRegPrice;
    public string sProdTax;
    //public int iTaxRate1;
    //public int iTaxRate2;
    //public int iTaxRate3;

    public double dTaxRate1;
    public double dTaxRate2;
    public double dTaxRate3;
    
    public double dQty;
    public string sProdUnit;
    public double dAmt;
    public double dGst;
    public double dPst;
    public double dHst;
    public double dDCRate;
    //public int iProdPromo;
    public double dProdPromo;
    public double dProdPromoPrice;
    public double dIUprice;
    public double dPromoQty;
    public string sPromoSdate;
    public string sPromoEdate;
    public string sCurDate;
    public string sCurTime;
    public double dProdDeposit;
    public double dProdCrf;
    public string sPromotioncd;

    public int iProdusehour;
    public string sProdBeginHour;
    public string sProdEndHour;
    public string sProdHourDCR;

    public string sWprice;
    public string sCust;
    public string sNative;
    public string sspecial;
    public string sFree;
    public string sEntryCode;
    public string sFoodStamp;
    public string sGiftCardRef;
    public string sType;
    public string sPtype;
    public string sPtype2;
    public string sRelatedid;
    public string sCat1;
    public string sCat2;
    public string sCat3;
    public string sCat4;
    public string sCat5;
    public double dCap;
    public string sCapUnit;
    public int iPackQty;
    public string sProdecid;

}

public struct OrderAmount
{
    public double dProdAmount;
    public double dTax1Amount;
    public double dTax2Amount;
    public double dTax3Amount;
    public double dProdPrice;
    public double dProdDeposit;
    public double dProdCrf;
    public double dProdEcofee;

    // EBT
    public double dEBTamount;
    public double dEBTTax1Amount;
    public double dEBTTax2Amount;
    public double dEBTTax3Amount;
}

public struct MemberInfo
{
    public string sStore;
    public int iCustNo;
    public string sCardNo;
    public string sName;
    public string sFirst;
    public string sTelNo;
    public string sEmail;
    public string sMemberType;
    public long lPointBalance;
    public long lPointEarned;
    public long lPointUsed;
    public int iOrigin;
    public double dStaffBal;
    public string sNPH;
    public string sASLA;
    public string sAPNo;
    public string sTPNo;
    public bool bVoid;
}