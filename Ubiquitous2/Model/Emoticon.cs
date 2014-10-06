using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UB.Utils;

namespace UB.Model
{
    public class Emoticon
    {
        public Emoticon(string pattern, string url, int width, int height)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            if (!Re.IsMatch(pattern, @"\W"))
                ExactWord = pattern;
            else if( pattern != null )
                Pattern = pattern;

            Width = width;
            Height = height;


            Uri uri;
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
            {
                HtmlCode = Html.CreateImageTag(url, width, height, String.IsNullOrWhiteSpace(ExactWord)?Pattern:ExactWord);
                Uri = uri;
            }

        }
        public Uri Uri { get; set; }
        public string Pattern { get; set; }
        public string ExactWord { get;set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public readonly string HtmlCode;
    }
}
