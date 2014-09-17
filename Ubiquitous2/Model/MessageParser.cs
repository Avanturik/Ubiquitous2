using System;
using System.Collections.Generic;
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
