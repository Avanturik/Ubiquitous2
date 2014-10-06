using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using UB.Model;

namespace UB.Utils
{
    public static class Html
    {
        private static WebClientBase webClient = new WebClientBase();

        public static String CreateImageTag(String src, int width, int height, string altText = "")
        {
            using( StringWriter stringWriter = new StringWriter() )
            {
                using (HtmlTextWriter htmlWriter = new HtmlTextWriter(stringWriter, String.Empty))
                {
                    htmlWriter.AddAttribute(HtmlTextWriterAttribute.Src, src, false);
                    htmlWriter.AddAttribute(HtmlTextWriterAttribute.Width, width.ToString());
                    htmlWriter.AddAttribute(HtmlTextWriterAttribute.Height, height.ToString());
                    htmlWriter.AddAttribute(HtmlTextWriterAttribute.Alt, altText);
                    htmlWriter.RenderBeginTag(HtmlTextWriterTag.Img);
                    htmlWriter.RenderEndTag();
                }
                return stringWriter.ToString();
            }

        }
        public static string ConvertUrlsToLinks(string msg)
        {
            var result = msg;
            string removeAnchors = @"<\/*a.*?>";
            result = Regex.Replace(msg, removeAnchors, "", RegexOptions.IgnoreCase);

            string regex = @"(?<!<[^>]*)((www\.|(http|https|ftp|news|file)+\:\/\/)[_.a-z0-9-]+\.[a-z0-9\/_;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])";
            Regex r = new Regex(regex, RegexOptions.IgnoreCase);
            var matches = r.Matches(result);
            foreach( Match match in matches )
            {
                result = result.Replace(match.Value, String.Format("<a href=\"{0}\" title=\"{1}\" target=\"_blank\">{0}</a>",GetTinyUrl(match.Value),match.Value));
            }

            return result.Replace("href=\"www", "href=\"http://www");
        }

        public static string GetTinyUrl( string url )
        {
            Uri uri;
            if( Uri.TryCreate( url, UriKind.Absolute, out uri ))
            {
                var query = uri.Query;
                if( uri.PathAndQuery.Length > 1 )
                {
                   var result = webClient.Download(String.Format(@"http://tinyurl.com/api-create.php?url={0}", HttpUtility.UrlEncode(url)));
                   if( !String.IsNullOrWhiteSpace(result) && result.StartsWith("http://"))
                       return result;
                }
            }

            return url;
        }
    }
}
