using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HanaSales_SelfCheckOut
{
    public class commonClass
    {
        public static string constringLocal = "Data Source = localhost; Initial Catalog = HANAMART; User ID = sa; Password=gkskgksk;MultipleActiveResultSets=true;";
        public static string constringServer = "Data Source = localhost; Initial Catalog = HANAMART; User ID = sa; Password=gkskgksk;MultipleActiveResultSets=true;";
        public static string scaleUnitLB = "LB";
        public static string scaleUnitKG = "KG";

        public string Left(string input, int count)
        {
            return input.Substring(0, Math.Min(input.Length, count));
        }

        public string Right(string input, int count)
        {
            return input.Substring(Math.Max(input.Length - count, 0), Math.Min(count, input.Length));
        }

        public string getDoubleFormat(double pInput)
        {
            string strConv = String.Format("{0:#,##0.00}", pInput, 2);
            return strConv;
        }

        public string getPennyRounded(double pAmount)
        {
            double dblAmount = Math.Abs(pAmount);
            string strPennyRounded = "0.00";

            string strTmpAmount = String.Format("{0:#,##0.00}", pAmount, 2);
            strTmpAmount = Right(strTmpAmount, 1);

            switch (strTmpAmount)
            {
                case "0": case "1": case "2":
                    //strPennyRounded = String.Format("{0:#,##0.00}", 0 - (Convert.ToDouble(strTmpAmount) * 0.01), 2);
                    strPennyRounded = getDoubleFormat(0 - (Convert.ToDouble(strTmpAmount) * 0.01));

                    break;
                case "3": case "4": case "5":
                    //strPennyRounded = String.Format("{0:#,##0.00}", 0.05 - (Convert.ToDouble(strTmpAmount) * 0.01), 2);
                    strPennyRounded = getDoubleFormat(0.05 - (Convert.ToDouble(strTmpAmount) * 0.01));
                    break;
                case "6": case "7": 
                    //strPennyRounded = String.Format("{0:#,##0.00}", 0.05 - (Convert.ToDouble(strTmpAmount) * 0.01), 2);
                    strPennyRounded = getDoubleFormat(0.05 - (Convert.ToDouble(strTmpAmount) * 0.01));
                    break;
                case "8": case "9":
                    //strPennyRounded = String.Format("{0:#,##0.00}", 0.1 - (Convert.ToDouble(strTmpAmount) * 0.01), 2);
                    strPennyRounded = getDoubleFormat(0.1 - (Convert.ToDouble(strTmpAmount) * 0.01));
                    break;
                default:
                    strPennyRounded = "0.00";
                    break;
            }

            if (pAmount < 0)
            {
                //strPennyRounded = String.Format("{0:#,##0.00}", Convert.ToDouble(strPennyRounded) * -1, 2);
                strPennyRounded = getDoubleFormat(Convert.ToDouble(strPennyRounded) * -1);
            }
            return strPennyRounded;
        }

        public bool GetTotalDCType(string tType)
        {
            bool ret = false;

            if (tType == "41" || tType == "42" || tType == "43" || tType == "44" || tType == "45" || tType == "46" || tType == "49") //|| tType == "48"   //|| tType == "47" Mix & Match 사용으로 제외 
            {
                ret = true;
            }
            else
            {
                ret = false;
            }
            return ret;
        }

        public void OpenCashDrawer()
        {
            // Cash Drawer Open Logic
            MessageBox.Show("Cash Drawer Opened");
        }

        public double[] GetTaxRate(string pTax)
        {
            double[] dblRate = new double[3];

            // Tax Table에서 값을 가져옴

            dblRate[0] = 0.05; //GST
            dblRate[1] = 0.07; //PST
            dblRate[2] = 0.13; //HST

            return dblRate;
        }
    }
}
