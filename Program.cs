using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DiscFit
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

            // Global exception handlers to catch and log any unhandled crashes
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, args) =>
            {
                LogCrash(args.Exception);
                MessageBox.Show("An error occurred: " + args.Exception.Message + "\n\nDetails logged to: " + 
                    System.IO.Path.Combine(System.IO.Path.GetTempPath(), "DiscFit_crash.log"),
                    "DiscFit Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                    LogCrash(ex);
            };

            Application.Run(new Form1());
        }

        private static void LogCrash(Exception ex)
        {
            try
            {
                string logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "DiscFit_crash.log");
                string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex.GetType().Name}: {ex.Message}\r\n{ex.StackTrace}\r\n\r\n";
                System.IO.File.AppendAllText(logPath, entry);
            }
            catch { }
        }
    }
}

