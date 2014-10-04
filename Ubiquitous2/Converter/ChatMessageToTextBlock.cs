using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;
using HtmlAgilityPack;
using Microsoft.Practices.ServiceLocation;
using UB.Model;
using UB.Utils;
using WpfAnimatedGif;

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
                                        int width = 0;
                                        int height = 0;
                                        url = "http://localhost/";
                                        if( node.Attributes["src"] != null)
                                            url = node.Attributes["src"].Value;

                                        if (node.Attributes["width"] != null)
                                            int.TryParse(node.Attributes["width"].Value, out width);

                                        if (node.Attributes["height"] != null)
                                            int.TryParse(node.Attributes["height"].Value, out height);

                                        Uri imageUri;
                                        if( Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out imageUri) )
                                        {
                                            dataService.GetImage(imageUri, width, height, (image) => {
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
                                        link.IsEnabled = true;
                                        if (!url.Contains("://"))
                                            url = "http://" + url;
                                        Uri linkUri;
                                        
                                        if( Uri.TryCreate(url, UriKind.Absolute, out linkUri) )
                                        {
                                            link.NavigateUri = linkUri;
                                            link.Focusable = false;
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
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
