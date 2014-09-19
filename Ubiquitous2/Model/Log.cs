using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Model
{
    public static class Log
    {
        public static void WriteError(String message)
        {
            Debug.Print("[{1}] Error: {0}", message, DateTime.Now.ToLongTimeString());
        }
        public static void WriteWarning(String message)
        {
            Debug.Print("[{1}] Warning: {0}", message, DateTime.Now.ToLongTimeString());
        }
        public static void WriteInfo(String message)
        {
            Debug.Print("[{1}] Info: {0}", message, DateTime.Now.ToLongTimeString());
        }

        public static void WriteError(String format, params object[] args)
        {
            Debug.Print("[" + DateTime.Now.ToLongTimeString() + "] Error: " + format, args);
        }
        public static void WriteWarning(String format, params object[] args)
        {
            Debug.Print("[" + DateTime.Now.ToLongTimeString() + "] Warning: " + format, args);
        }
        public static void WriteInfo(String format, params object[] args)
        {
            Debug.Print("[" + DateTime.Now.ToLongTimeString() + "] Info: " + format, args );
        }

    }
}
