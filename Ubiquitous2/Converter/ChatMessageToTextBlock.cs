using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using GalaSoft.MvvmLight.Threading;
using HtmlAgilityPack;
using Microsoft.Practices.ServiceLocation;
using UB.Model;

namespace UB.Converter
{
    public class ChatMessageToInlines : IValueConverter
    {
        private IImageDataSource dataService;
        private object lockConvert = new object();
        public ChatMessageToInlines()
        {
            dataService = ServiceLocator.Current.GetInstance<IImageDataSource>();
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            lock( lockConvert )
            {
                String url = String.Empty;
                TextBlock textBlock = new TextBlock();
                textBlock.AllowDrop = false;
                textBlock.Focusable = false;
                var message = value as ChatMessage;

                if (!String.IsNullOrEmpty(message.Text))
                {
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(message.Text);
                    var nodes = htmlDoc.DocumentNode.ChildNodes.ToList();
                    foreach (var node in nodes)
                    {
                        switch (node.NodeType)
                        {
                            case HtmlNodeType.Text:
                                textBlock.Inlines.Add(new Run(node.InnerText));
                                break;
                            case HtmlNodeType.Element:
                                switch (node.OriginalName.ToLower())
                                {
                                    case "img":
                                        int width;
                                        int height;
                                        url = node.Attributes["src"].Value;
                                        int.TryParse(node.Attributes["width"].Value, out width);
                                        int.TryParse(node.Attributes["height"].Value, out height);
                                        width = width <= 0 ? 16 : width;
                                        height = height <= 0 ? 16 : height;
                                        
                                        dataService.GetImage(new Uri(url), width, height, (image) =>
                                        {
                                            image.Focusable = false;
                                            textBlock.Inlines.Add(image);
                                        });
                                        break;
                                    case "a":
                                        Hyperlink link = new Hyperlink(new Run(node.Attributes["href"].Value));
                                        url = node.Attributes["href"].Value;
                                        link.IsEnabled = true;
                                        if (!url.Contains("://"))
                                            url = "http://" + url;
                                        link.NavigateUri = new Uri(url);
                                        link.Focusable = false;
                                        link.RequestNavigate += (sender, e) =>
                                        {
                                            Process.Start(e.Uri.ToString());
                                        };
                                        textBlock.Inlines.Add(link);
                                        break;
                                }
                                break;
                        }
                    }
                }
                return textBlock.Inlines;

            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
