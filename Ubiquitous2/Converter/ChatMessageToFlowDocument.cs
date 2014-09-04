using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using UB.Model;
using HtmlAgilityPack;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;


namespace UB.Converter
{
    public class ChatMessageToFlowDocument : IValueConverter
    {
        
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FlowDocument flowDocument = new FlowDocument();
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
                            paragraph.Inlines.Add(new Run(node.InnerText));
                            break;
                        case HtmlNodeType.Element:
                            switch( node.OriginalName.ToLower() )
                            {
                                case "img":
                                    int width;
                                    int height;
                                    int.TryParse(node.Attributes["width"].Value, out width);
                                    int.TryParse(node.Attributes["height"].Value, out height);

                                    BitmapImage bi = new BitmapImage(new Uri(node.Attributes["src"].Value));
                                    
                                    Image image = new Image();                                    
                                    image.Width = width;
                                    image.Height = height;
                                    image.SnapsToDevicePixels = true;
                                    image.Source = bi;

                                    paragraph.Inlines.Add( image );
                                    break;
                                case "a":
                                    Hyperlink link = new Hyperlink(new Run(node.Attributes["href"].Value));
                                    var url = new Uri(node.Attributes["href"].Value);
                                    link.IsEnabled = true;
                                    link.NavigateUri = url;
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
