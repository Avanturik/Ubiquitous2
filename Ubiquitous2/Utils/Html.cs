using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI;

namespace UB.Utils
{
    public static class Html
    {
        public static String CreateImageTag(String src, int width, int height)
        {
            using( StringWriter stringWriter = new StringWriter() )
            {
                using (HtmlTextWriter htmlWriter = new HtmlTextWriter(stringWriter, String.Empty))
                {
                    htmlWriter.AddAttribute(HtmlTextWriterAttribute.Src, src);
                    htmlWriter.AddAttribute(HtmlTextWriterAttribute.Width, width.ToString());
                    htmlWriter.AddAttribute(HtmlTextWriterAttribute.Height, height.ToString());
                    htmlWriter.RenderBeginTag(HtmlTextWriterTag.Img);
                    htmlWriter.RenderEndTag();
                }
                return stringWriter.ToString();
            }

        }
        public static string ConvertUrlsToLinks(string msg)
        {
            string regex = @"(?<!<[^>]*)((www\.|(http|https|ftp|news|file)+\:\/\/)[_.a-z0-9-]+\.[a-z0-9\/_;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])";
            Regex r = new Regex(regex, RegexOptions.IgnoreCase);
            return r.Replace(msg, "<a href=\"$1\" target=\"_blank\">$1</a>").Replace("href=\"www", "href=\"http://www");
        }
    }
}
