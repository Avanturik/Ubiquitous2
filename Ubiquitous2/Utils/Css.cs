using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UB.Utils
{
    public class Css
    {
        public static CssBackground GetBackground( string cssDefinition )
        {
            if (String.IsNullOrWhiteSpace(cssDefinition))
                return null;

            var url = Re.GetSubString(cssDefinition, @"url\('([^']+)");
            var x = Re.GetSubString(cssDefinition, @"url\('[^']+.*?(-*\d+)");
            if (String.IsNullOrWhiteSpace(x))
                x = "0";

            var y = Re.GetSubString(cssDefinition, @"url\('[^']+.*?\d+.*?(-*\d+)");
            if (String.IsNullOrWhiteSpace(x))
                y = "0";

            var width = Re.GetSubString(cssDefinition, @"width.*?(\d+)");
            var height = Re.GetSubString(cssDefinition, @"height.*?(\d+)");

            int numWidth = 0;
            int numHeight = 0;

            Int32.TryParse(width, out numWidth);
            Int32.TryParse(height, out numHeight);

            return new CssBackground()
            {
                url = url,
                x = x,
                y = y,
                width = numWidth,
                height = numHeight,
            };
        }
    }

    public class CssBackground 
    {
        public string url { get; set; }
        public string x { get; set; }
        public string y { get;set; }
        public int width { get;set; }
        public int height {get;set; }
    }
}
