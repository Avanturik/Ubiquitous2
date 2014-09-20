using System;
using System.Collections.Generic;
using UB.Utils;

namespace UB.Model
{
    public class ChatMessage    
    {
        private string text;

        public ChatMessage()
        {
            Initialize();
        }
        public ChatMessage (String text)
        {
            Text = text;
            OriginalText = text;
            Initialize();
        }
        private void Initialize()
        {
            Id = Guid.NewGuid();
            TimeStamp = DateTime.Now.ToLongTimeString();
            UnixTimeStamp = Time.UnixTimestamp();
            HighlyImportant = false;
            FormatString = "%from@%chatname: %text";
        }
        public String ChatName { get; set; }
        public string FormatString { get; set; }
        public string OriginalText { get; set; }
        public String FormattedText
        {
            get
            {
                if (String.IsNullOrWhiteSpace(FormatString) || String.IsNullOrWhiteSpace(OriginalText))
                    return String.Empty;

                //%text - text, %from - from name, %to - to name, %chatname - chat name %time - timestamp"
                Dictionary<string, string> replaceList = new Dictionary<string, string>()
                {
                    {"%text", this.OriginalText},
                    {"%from", this.FromUserName},
                    {"%to", String.Empty},
                    {"%chatname", this.ChatName},
                    {"%time", this.TimeStamp}
                };            

                var result = FormatString;
                foreach (var pair in replaceList)
                {
                     result = result.Replace(pair.Key, pair.Value);
                }
                return result;
            }
        }
        public String Text{
            get { return text; }
            set
            {
                if (text == value)
                    return;

                text = value;

                if (String.IsNullOrWhiteSpace(OriginalText))
                    OriginalText = text;

            }
        }
        public Guid Id { get; set; }
        public String TimeStamp { get; set; }
        public String ChatIconURL { get; set; }
        public String FromUserName { get; set; }
        public String Channel { get; set; }
        public bool HighlyImportant { get; set; }
        public bool IsSentByMe { get; set; }
        public long UnixTimeStamp { get; set; }
    }
}
