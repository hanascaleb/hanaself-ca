using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HanaSales_SelfCheckOut
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Diagnostics.Process[] processes = null;
            string strCurrentProcess = System.Diagnostics.Process.GetCurrentProcess().ProcessName.ToUpper();
            processes = System.Diagnostics.Process.GetProcessesByName(strCurrentProcess);

            if (processes.Length > 1)
            {
                MessageBox.Show(String.Format("'[{0}]' : Self-Checkout Program is already started.", System.Diagnostics.Process.GetCurrentProcess().ProcessName));
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new HanaSales_SelfCheckOut());
        }
    }
}
