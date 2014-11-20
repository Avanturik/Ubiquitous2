using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public static class Log
    {
        public static void WriteError(String message)
        {
            var text = String.Format("[{1}] Error: {0}", message, DateTime.Now.ToLongTimeString());
            AppendToLogFile(text);
        }
        public static void WriteWarning(String message)
        {
            var text = String.Format("[{1}] Warning: {0}", message, DateTime.Now.ToLongTimeString());
            AppendToLogFile(text);
        }
        public static void WriteInfo(String message)
        {
            var text = String.Format("[{1}] Info: {0}", message, DateTime.Now.ToLongTimeString());
            AppendToLogFile(text);
        }

        public static void WriteError(String format, params object[] args)
        {
            var text = String.Format("[" + DateTime.Now.ToLongTimeString() + "] Error: " + format, args);
            AppendToLogFile(text);
        }
        public static void WriteWarning(String format, params object[] args)
        {
            var text = String.Format("[" + DateTime.Now.ToLongTimeString() + "] Warning: " + format, args);
            AppendToLogFile(text);
        }
        public static void WriteInfo(String format, params object[] args)
        {
            var text = String.Format("[" + DateTime.Now.ToLongTimeString() + "] Info: " + format, args);
            AppendToLogFile(text);
        }

        private static void AppendToLogFile(string text)
        {
            //try
            //{
            //    File.AppendAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\ubiquitous_log.txt", text + Environment.NewLine);
            //}
            //catch (Exception e)
            //{
            //    Debug.Print("Error adding record to the log file");
            //}
            Debug.Print(text);
        }
    }
}
