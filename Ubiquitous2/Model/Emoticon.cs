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
        public Emoticon(String pattern, String url, int width, int height)
        {
            if (String.IsNullOrWhiteSpace(url) || width <= 0 || height <= 0)
                return;

            Pattern = pattern;
            Width = width;
            Height = height;
            HtmlCode = Html.CreateImageTag(url, width, height);
            Uri = new Uri(url);

        }
        public Uri Uri { get; set; }
        public String Pattern { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public readonly String HtmlCode;
    }
}
