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
            Debug.Print("Error: {0}", message);
        }
        public static void WriteWarning(String message)
        {
            Debug.Print("Warning: {0}",message);
        }
        public static void WriteInfo(String message)
        {
            Debug.Print("Info: {0}",message);
        }

        public static void WriteError(String format, params object[] args)
        {
            Debug.Print("Error: " + format, args);
        }
        public static void WriteWarning(String format, params object[] args)
        {
            Debug.Print("Warning: " + format, args);
        }
        public static void WriteInfo(String format, params object[] args)
        {
            Debug.Print("Info: " + format, args);
        }

    }
}
