using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UB.Model;

namespace UB.Utils
{
    public static class Text
    {
        public static string ReplaceUTF32Character(this string input, string what, string replacement)
        {
            if (what.Length > 8 || what.Length < 4)
                return input;

            what = what.PadLeft(8, '0').ToUpper();

            int j = 0;
            int length = what.Length;

            string whatFormatted = String.Empty;

            for (int i = 2; i < length + 2; i += 2)
                whatFormatted += what.Substring(length - i, 2) + "-";

            whatFormatted = whatFormatted.Substring(0, whatFormatted.Length - 1);

            string inputUtf32 = Encoding.UTF32.GetString( Encoding.Convert(Encoding.UTF8, Encoding.UTF32, Encoding.UTF8.GetBytes(input)) );
            return inputUtf32.Replace(whatFormatted.ConvertFromUTF32Hex(), replacement);
        }

        public static string ConvertFromUTF32Hex(this string input)
        {
            String[] hexNumbers = input.Split('-');
            byte[] hexBytes = new byte[hexNumbers.Length];
            for (int i = 0; i < hexNumbers.Length; i++)
                hexBytes[i] = Convert.ToByte(hexNumbers[i], 16);

            return Encoding.UTF32.GetString(hexBytes);
        }
        public static string ConvertToUTF32Hex(this string input)
        {
            var result = BitConverter.ToString(Encoding.UTF32.GetBytes(input));
            return result;
        }
    }
}
