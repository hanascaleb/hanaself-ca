using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;


namespace hanas.com.colibs
{
    public class cls_colibs00
    {
        private string c_app_path = AppDomain.CurrentDomain.BaseDirectory;      // System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

        public string app_path
        {
            get { return this.c_app_path; }
        }

        public void cWriteLogs(string Processor, string Message)
        {
            string sLogPath = c_app_path + "\\Logs\\";
            string sFileName = Processor + "_" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";

            string sFileFullName = sLogPath + sFileName;

            if (!Directory.Exists(sLogPath))
            {
                Directory.CreateDirectory(sLogPath);
            }

            if (!File.Exists(sFileFullName))
            {
                // Create a file to write to.   
                using (StreamWriter swLogFile = File.CreateText(sFileFullName))
                {
                    swLogFile.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(sFileFullName))
                {
                    sw.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " + Message);
                }
            }
        }

        public int cReadFile(string vFileName, ref List<string> vList)
        {
            string sReadLine = string.Empty;

            string sFilePath = c_app_path + "\\";
            string sFileName = vFileName;

            string sFileFullName = sFilePath + sFileName;
            int iReturn = 0;

            try
            {
                if (!File.Exists(sFileFullName))
                {
                    iReturn = -1;
                }
                else
                {
                    StreamReader sr = new StreamReader(sFileFullName);
                    while ((sReadLine = sr.ReadLine()) != null)
                    {
                        vList.Add(sReadLine);
                    }

                    sr.Close();

                    iReturn = 1;
                }

                return iReturn;
            }
            catch
            {
                return -1;
            }
        }

        public static string HashBytes(string valueToHash)
        {
            HashAlgorithm hasher = new MD5CryptoServiceProvider();
            Byte[] valueToHashAsByte = Encoding.UTF8.GetBytes(valueToHash);
            Byte[] returnBytes = hasher.ComputeHash(valueToHashAsByte);
            StringBuilder hex = new StringBuilder(returnBytes.Length * 2);
            foreach (byte b in returnBytes) hex.AppendFormat("{ 0:x2}", b);
            return "0x" + hex.ToString().ToUpper();
        }

    }
}
