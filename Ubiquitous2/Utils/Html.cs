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
        public static string GetTinyUrl( string url )
        {
            Uri uri;
            if( Uri.TryCreate( url, UriKind.Absolute, out uri ))
            {
                var query = uri.Query;
                if( uri.PathAndQuery.Length > 1 )
                {
                   var result = webClient.Download(String.Format(@"http://tinyurl.com/api-create.php?url={0}", url));
                   if( !String.IsNullOrWhiteSpace(result) && result.StartsWith("http://") && result.Length < url.Length)
                       return result;
                }
            }

            return url;
        }
    }
}
