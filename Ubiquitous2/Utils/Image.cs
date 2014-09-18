using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UB.Model;

namespace UB.Utils
{
    public static class ImageInfo
    {
        public static void GetWebImageSize( string url, Action<Size> callback )
        {
            const int maxHeight = 256;
            const int maxWidth = 256;
            using( var webClient = new WebClientBase())
            {
                Image image = GetImageFromUrl(url);
                
                if (image != null)
                {
                
                    var width = image.Width;
                    var height = image.Height;
                    if (width > maxWidth)
                    {
                        height = height * maxWidth / width;
                        width = maxWidth;
                    }

                    if (height > maxHeight)
                    {
                        width  = width * maxHeight / height;
                        height = maxHeight;
                    }

                    if (width == 0)
                        width = 16;
                    if (height == 0)
                        height = 16;

                    callback( new Size(width, height));
                }
                else
                    callback( new Size(16, 16));
            }
        }
        public static Image GetImageFromUrl(string url)
        {
            using (var webClient = new WebClientBase())
            {
                return ByteArrayToImage(webClient.DownloadData(url));
            }
        }

        public static Image ByteArrayToImage(byte[] fileBytes)
        {
            using (var stream = new MemoryStream(fileBytes))
            {
                return Image.FromStream(stream);
            }
        }
    }
}
