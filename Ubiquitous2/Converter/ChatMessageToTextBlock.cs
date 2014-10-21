using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using HtmlAgilityPack;
using Microsoft.Practices.ServiceLocation;
using UB.Model;
using UB.Utils;

namespace UB.Converter
{
    public class ChatMessageToInlines : IValueConverter, IMultiValueConverter
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
                return ChatMessageToInlinesCollection(value as ChatMessage);
            }
        }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            lock (lockConvert)
            {
                TextBlock textBlock = new TextBlock();

                if (values == null || values.Count() <= 0)
                    return textBlock.Inlines;

                foreach (object value in values.ToList())
                {

                    if( value is ChatMessage )
                    {
                        var messageInlines = ChatMessageToInlinesCollection(value as ChatMessage);
                        if( messageInlines != null )
                        {
                            foreach (var inline in messageInlines.ToList())
                            {
                                textBlock.Inlines.Add(inline);
                            }
                        }
                    }
                    else if( value is FrameworkElement)
                    {
                        var element = value as FrameworkElement;
                        if( element.Parent is Grid)
                        {
                            (element.Parent as Grid).Children.Remove(element);
                            textBlock.Inlines.Add(element);
                        }
                    }
                }

                return textBlock.Inlines;
            }
        }

        private InlineCollection ChatMessageToInlinesCollection( ChatMessage message)
        {
            String url = String.Empty;
            TextBlock textBlock = new TextBlock();
            textBlock.AllowDrop = false;
            textBlock.Focusable = false;

            if (message == null)
                return textBlock.Inlines;

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
                                    int width = 0;
                                    int height = 0;
                                    url = "http://localhost/";
                                    if (node.Attributes["src"] != null)
                                        url = node.Attributes["src"].Value;

                                    if (node.Attributes["width"] != null)
                                        int.TryParse(node.Attributes["width"].Value, out width);

                                    if (node.Attributes["height"] != null)
                                        int.TryParse(node.Attributes["height"].Value, out height);

                                    var imgTooltip = this.With(x => node.Attributes["alt"])
                                                    .With(x => x.Value);

                                    Uri imageUri;
                                    if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out imageUri))
                                    {
                                        if( dataService != null )
                                            dataService.GetImage(imageUri, width, height, (image) =>
                                            {
                                                image.ToolTip = imgTooltip;
                                                textBlock.Inlines.Add(image);
                                            }, null);
                                    }
                                    else
                                    {
                                        Log.WriteError("Converter got invalid Url:{0}", url);
                                        textBlock.Inlines.Add(url);
                                    }

                                    break;
                                case "a":
                                    Hyperlink link = new Hyperlink(new Run(node.Attributes["href"].Value));
                                    url = node.Attributes["href"].Value;
                                    var tooltip = this.With(x => node.Attributes["title"])
                                                    .With(x => x.Value);

                                    link.IsEnabled = true;
                                    if (!url.Contains("://"))
                                        url = "http://" + url;
                                    Uri linkUri;

                                    if (Uri.TryCreate(url, UriKind.Absolute, out linkUri))
                                    {
                                        link.NavigateUri = linkUri;
                                        link.Focusable = false;
                                        link.ToolTip = tooltip;
                                        link.RequestNavigate += (sender, e) =>
                                        {
                                            Process.Start(e.Uri.ToString());
                                        };
                                        textBlock.Inlines.Add(link);
                                    }
                                    else
                                    {
                                        Log.WriteError("Can't parse url: {0}", url);
                                        textBlock.Inlines.Add(url);
                                    }
                                    break;
                            }
                            break;
                    }
                }
            }
            return textBlock.Inlines;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
