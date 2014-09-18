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
            if (string.IsNullOrWhiteSpace(url) || width <= 0 || height <= 0)
                return;

            if (!Re.IsMatch(pattern, @"\W"))
                ExactWord = pattern;
            else
                Pattern = pattern;

            Width = width;
            Height = height;
            HtmlCode = Html.CreateImageTag(url, width, height);
            Uri = new Uri(url);

        }
        public Uri Uri { get; set; }
        public List<string> ExactWords { get; set; }
        public string Pattern { get; set; }
        public string ExactWord { get;set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public readonly string HtmlCode;
    }
}
