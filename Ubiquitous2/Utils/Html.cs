using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Windows;
using HtmlAgilityPack;
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
            var appConfig = (Application.Current as App).AppConfig;
            if (!appConfig.IsShortURLEnabled)
                return url;

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

        public static HtmlAgilityPack.HtmlNode GetDocNode(String content)
        {
            HtmlDocument doc = new HtmlDocument();
            try
            {
                doc.LoadHtml(content);
            }
            catch { }

            return doc.DocumentNode;
        }
        public static String GetSiblingInnerText(String xpath, String content)
        {
            String result = String.Empty;
            try
            {
                var docNode = GetDocNode(content);
                HtmlNode node = docNode.SelectSingleNode(xpath);
                result = node.NextSibling.InnerText;
            }
            catch
            {
            }
            return result;
        }
        public static String GetInnerHtml(String xpath, String content)
        {
            String result = String.Empty;
            try
            {
                var docNode = GetDocNode(content);
                HtmlNode node = docNode.SelectSingleNode(xpath);
                result = node.InnerHtml;
            }
            catch
            {
            }
            return result;
        }
        public static String GetInnerText(String xpath, String content)
        {
            String result = String.Empty;
            try
            {
                var docNode = GetDocNode(content);
                HtmlNode node = docNode.SelectSingleNode(xpath);
                result = node.InnerText;
            }
            catch
            {
            }
            return result;
        }
        public static List<KeyValuePair<String, String>> FormParams(String xpath, String content)
        {
            List<KeyValuePair<String, String>> result = new List<KeyValuePair<String, String>>();
            try
            {
                var docNode = GetDocNode(content);
                var inputNodes = docNode.SelectNodes(xpath + "//input");
                if (inputNodes != null)
                {
                    foreach (HtmlNode node in inputNodes)
                    {
                        var id = node.GetAttributeValue("id", "");
                        if (String.IsNullOrEmpty(id))
                            id = node.GetAttributeValue("name", "");

                        var value = node.GetAttributeValue("value", "");
                        result.Add(new KeyValuePair<String, String>(id, value));
                    }
                }
                var selectedOptions = docNode.SelectNodes(xpath + @"//select/option[@selected='selected']");
                if (selectedOptions != null)
                {
                    foreach (HtmlNode node in selectedOptions)
                    {
                        var id = node.GetAttributeValue("value", "");
                        var value = node.NextSibling.InnerText;
                        result.Add(new KeyValuePair<String, String>(id, value));
                    }
                }

                var textAreas = docNode.SelectNodes(xpath + "//textarea");
                if (textAreas != null)
                {
                    foreach (HtmlNode node in textAreas)
                    {
                        var id = node.GetAttributeValue("id", "");
                        if (String.IsNullOrEmpty(id))
                            id = node.GetAttributeValue("name", "");

                        var value = node.InnerText;
                        result.Add(new KeyValuePair<String, String>(id, value));
                    }
                }

            }
            catch { }
            return result;
        }
        public static List<KeyValuePair<String, String>> GetOptions(String xpath, String content)
        {
            List<KeyValuePair<String, String>> result = new List<KeyValuePair<String, String>>();
            try
            {
                var docNode = GetDocNode(content);
                foreach (HtmlNode node in docNode.SelectNodes(xpath))
                {
                    result.Add(new KeyValuePair<String, String>(node.GetAttributeValue("value", ""), node.NextSibling.InnerText));
                }
            }
            catch { }
            return result;
        }
        public static String GetAttribute(String xpath, String attribName, String content)
        {
            String result = String.Empty;

            try
            {
                var docNode = GetDocNode(content);
                HtmlNode node = docNode.SelectSingleNode(xpath);
                result = node.Attributes[attribName].Value;
            }
            catch
            {
            }
            return result;
        }
    }
}
