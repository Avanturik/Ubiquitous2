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
        public static string HtmlToPlainText( string message)
        {
            string result = message;
            result = Regex.Replace(result, @"<img.*src=""(.*?)"".*?\/>", "$1");
            result = Regex.Replace(result, @"<a[^>]*>(.*?)<\/a>", "$1");
            result = Regex.Replace(result, @"<a.*href=""(.*?)"".*>", "$1");
            result = Regex.Replace(result, @"<(http.*?)>", "$1");
            result = Regex.Replace(result, @"<http[^>]*>", "");
            if (String.IsNullOrWhiteSpace(result))
                return message;
            else
                return result;
        }
        public static void ConvertToPlainText( ChatMessage message, IChat chat)
        {
            message.Text = HtmlToPlainText(message.Text);
        }
        public static void RemoveRedundantTags( ChatMessage message, IChat chat)
        {
            string regex = @"</*(div|iframe)[^>]*>";
            message.Text = Regex.Replace(message.Text, regex, "");
        }
        public static void ParseURLs( ChatMessage message, IChat chat )
        {
            message.Text = ConvertUrlsToLinks(message.Text);
        }
        public static void ParseImageUrlsAsImages( ChatMessage message, IChat chat )
        {
            ParseSimpleImageTags(message, chat, false);
        }
        public static void ParseSimpleImageTags( ChatMessage message, IChat chat )
        {
            ParseSimpleImageTags(message, chat, true);
        }
        public static void ParseSimpleImageTags(ChatMessage message, IChat chat, bool isInsideTag = true)
        {
            string regex = @"(<((https?:)?\/\/?[^'""<>]+?\.(jpg|jpeg|gif|png)).*?>)";
            if( !isInsideTag )
                regex = @"(((https?:)?\/\/?[^'""<>]+?\.(jpg|jpeg|gif|png)).*)";
            Regex r = new Regex(regex, RegexOptions.IgnoreCase);
            var matches = r.Matches(message.Text);
            using( var webClient = new WebClientBase())
            {
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1)
                    {
                        message.Text = r.Replace(message.Text, @"<img src=""$2""/>").Replace("href=\"www", "href=\"http://www");
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
        public static void ParseSpaceSeparatedEmoticons(ChatMessage message, IChat chat)
        {
            if (chat == null || chat.Emoticons == null)
                return;
            var emoticons = chat.Emoticons.ToList();
            bool containsNonAlpha = Regex.IsMatch(message.Text, @"\W");
            
            HashSet<string> words = null;

            if (containsNonAlpha)
                words = new HashSet<string>(Regex.Split(message.Text, @"\s").Where(s => s != String.Empty));
            else
                words = new HashSet<string>(new string[] { message.Text });


            foreach (var emoticon in emoticons)
            {
                if ((words != null || !containsNonAlpha) && emoticon.ExactWord != null)
                {
                    if (words.Contains(emoticon.ExactWord))
                    {
                        if (message.Text == emoticon.ExactWord)
                        {
                            message.Text = emoticon.HtmlCode;
                            break;
                        }
                        message.Text = message.Text.Replace(" " + emoticon.ExactWord, emoticon.HtmlCode);
                        message.Text = message.Text.Replace(emoticon.ExactWord + " ", emoticon.HtmlCode);
                    }
                }
            }
        }
        public static string ConvertUrlsToLinks(string msg)
        {
            var result = msg;
            string removeAnchors = @"<\/*a.*?>";
            result = Regex.Replace(msg, removeAnchors, "", RegexOptions.IgnoreCase);

            string regex = @"(?<!<[^>]*)((www\.|(http|https|ftp|news|file)+\:\/\/)[_.\p{L}0-9-]+\.[\p{L}0-9\/_;:@=.+?,##%&~-]*[^.|\'|\# |!|\(|?|,| |>|<|;|\)])";
            Regex r = new Regex(regex, RegexOptions.IgnoreCase);
            var matches = r.Matches(result);
            foreach (Match match in matches)
            {
                result = result.Replace(match.Value, String.Format("<a href=\"{0}\" title=\"{1}\" target=\"_blank\">{0}</a>", Html.GetTinyUrl(match.Value), match.Value));
            }

            return result.Replace("href=\"www", "href=\"http://www");
        }

    }
}
