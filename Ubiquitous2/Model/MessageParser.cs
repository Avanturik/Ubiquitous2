using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UB.Utils;

namespace UB.Model
{
    public class MessageParser
    {
        public static void ParseURLs( ChatMessage message, IChat chat )
        {
            message.Text = Html.ConvertUrlsToLinks(message.Text);
        }
        public static void ParseSimpleImageTags( ChatMessage message, IChat chat )
        {
            string regex = @"(<((https?:)?\/\/?[^'""<>]+?\.(jpg|jpeg|gif|png)).*?>)";
            Regex r = new Regex(regex, RegexOptions.IgnoreCase);
            var matches = r.Matches(message.Text);
            using( var webClient = new WebClientBase())
            {
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1)
                    {
                        string url = match.Groups[2].Value;
                        try
                        {

                            Image image = Image.FromStream(webClient.DownloadToStream(url));
                            if (image != null)
                            {
                                var width = image.Width;
                                var height = image.Height;
                                if (width > 256)
                                {
                                    height = height * 256 / width;
                                    width = 256;
                                }

                                if (height > 256)
                                {
                                    width  = width * 256 / height;
                                    height = 256;
                                }

                                message.Text = r.Replace(message.Text, @"<img width=""" + width + @""" height=""" + height + @""" src=""$2""/>").Replace("href=\"www", "href=\"http://www");
                            }
                        }
                        catch { }
                    }
                }
            }
        }
        public static void ParseEmoticons(ChatMessage message, IChat chat)
        {
            if (chat == null || chat.Emoticons == null)
                return;
            var emoticons = chat.Emoticons.ToList();
            bool containsNonAlpha = Regex.IsMatch(message.Text, @"\W");
            HashSet<string> words = null;

            if (containsNonAlpha)
                words = new HashSet<string>(Regex.Split(message.Text, @"\W").Where(s => s != String.Empty));
            else
                words = new HashSet<string>(new string[] { message.Text });


            foreach (var emoticon in emoticons)
            {
                if ((words != null || !containsNonAlpha) && emoticon.ExactWord != null)
                {
                    if (words.Contains(emoticon.ExactWord))
                        message.Text = message.Text.Replace(emoticon.ExactWord, emoticon.HtmlCode);
                }
                else if (emoticon.Pattern != null && containsNonAlpha)
                {
                    message.Text = Regex.Replace(message.Text, emoticon.Pattern, emoticon.HtmlCode, RegexOptions.Singleline);
                }
            }
        }
    }
}
