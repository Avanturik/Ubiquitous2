using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UB.Utils
{
    public class Rnd
    {
        public static string RandomWebSocketServerNum(int maxServerNumber)
        {
            var number = new Random();
            return (number.Next(0, maxServerNumber)).ToString("000");
        }
        public static string RandomWebSocketString()
        {
            var chars = "abcdefghijklmnopqrstuvwxyz0123456789_";
            StringBuilder builder = new StringBuilder();
            var random = new Random();

            for (var i = 0; i < 8; i++)
                builder.Append(chars[random.Next(0, chars.Length - 1)]);

            return builder.ToString();
        }
    }
}
