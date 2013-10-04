using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Quotopia
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            try
            {
                //QuoteLegacy q = new QuoteLegacy();
                QuotopiaMain q = new QuotopiaMain();
                
                Application.Run(q);
            }
            catch (Exception ex) { TradeLink.AppKit.CrashReport.Report(QuotopiaMain.PROGRAM, ex); }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            TradeLink.AppKit.CrashReport.Report(QuotopiaMain.PROGRAM, (Exception)e.ExceptionObject); 
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            TradeLink.AppKit.CrashReport.Report(QuotopiaMain.PROGRAM, e);
        }
    }
}