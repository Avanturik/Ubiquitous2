using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using UB.Model;
using HtmlAgilityPack;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Microsoft.Practices.ServiceLocation;
using GalaSoft.MvvmLight.Threading;
using System.Windows.Threading;


namespace UB.Converter
{
    public class ChatMessageToFlowDocument : IValueConverter
    {
        private IImageDataSource dataService;
        public ChatMessageToFlowDocument()
        {
            dataService = ServiceLocator.Current.GetInstance<IImageDataSource>();
        }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String url = String.Empty;
            FlowDocument flowDocument = new FlowDocument();
            flowDocument.AllowDrop = false;
            flowDocument.Focusable = false;
            var message = value as ChatMessage;
            if (!String.IsNullOrEmpty(message.Text))
            {
                Paragraph paragraph = new Paragraph() { TextAlignment = TextAlignment.Justify };
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(message.Text);                
                var nodes = htmlDoc.DocumentNode.ChildNodes.ToList();
                foreach( var node in nodes )
                {
                    switch( node.NodeType )
                    {
                        case HtmlNodeType.Text:
                            paragraph.Inlines.Add(new TextBox() { Text = node.InnerText });
                            break;
                        case HtmlNodeType.Element:
                            switch( node.OriginalName.ToLower() )
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
                                        paragraph.Inlines.Add(image);
                                    });
                                    break;
                                case "a":
                                    Hyperlink link = new Hyperlink(new Run(node.Attributes["href"].Value));
                                    url = node.Attributes["href"].Value;
                                    link.IsEnabled = true;
                                    link.NavigateUri = new Uri(url);
                                    link.RequestNavigate += (sender, e) =>
                                    {
                                       Process.Start(e.Uri.ToString());
                                    };
                                    paragraph.Inlines.Add(link);
                                    break;
                            }
                            break;

                    }
                }
                
                flowDocument.Blocks.Add(paragraph);
            }

            return flowDocument;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
