// --------------------------------------------------------------------------
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// --------------------------------------------------------------------------

using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace UB.Utils
{
    internal class TextMeasurer
    {
        private static double _lineHeight = -1;

        private static double _charWidth = -1;

        public static double GetEstimatedHeight(string text, double width)
        {
            CheckInitialization();

            double height = 0;

            int startChar = 0;

            int charsPerLine = (int)(width / _charWidth) - 5; // 5 is rough average extra characters for word wrap
            charsPerLine = Math.Max(1, charsPerLine);

            while (startChar < text.Length)
            {
                int endChar = text.IndexOf('\n', startChar);
                if (endChar < 0)
                {
                    endChar = text.Length;
                }
                int charCount = Math.Max(1, endChar - startChar);
                startChar = endChar + 1;

                height += _lineHeight * Math.Ceiling((double)charCount / charsPerLine);
            }

            return height;
        }

        private static void CheckInitialization()
        {
            if (_lineHeight >= 0)
            {
                return;
            }

            var text = new StringBuilder();
            for (char ch = 'a'; ch <= 'z'; ch++)
            {
                text.Append(ch);
            }
            text.Append("\r\n");
            for (char ch = 'A'; ch <= 'Z'; ch++)
            {
                text.Append(ch);
            }

            var textBlock = new TextBlock();
            textBlock.Text = text.ToString();
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            _lineHeight = textBlock.DesiredSize.Height / 2;
            _charWidth = textBlock.DesiredSize.Width / 26;
        }
    }
}