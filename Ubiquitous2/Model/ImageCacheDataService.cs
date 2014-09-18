using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Threading;
using UB.Utils;
using WpfAnimatedGif;

namespace UB.Model
{
    public class ImageCacheDataService : IImageDataSource
    {
        private object cacheLock = new object();
        private object getLock = new object();

        private Dictionary<String, BitmapImage> bitmapImageCache = new Dictionary<string, BitmapImage>();
        
        public void GetImage(Uri uri, int width, int height, Action<Image> callback)
        {
            lock(getLock)
            {
                
                if (!bitmapImageCache.ContainsKey(uri.AbsoluteUri))
                {
                    var bitmap = new BitmapImage(uri);
                    if (bitmap == null)
                        return;

                    bitmapImageCache.Add(uri.AbsoluteUri, bitmap);
                }
                Image image = new Image() { Width = width, Height = height };
                if (uri.OriginalString.ToLower().Contains(".gif"))
                {
                    image.Source = bitmapImageCache[uri.AbsoluteUri];
                    //ImageBehavior.SetRepeatBehavior(image, RepeatBehavior.Forever);
                    //using (var webClient = new WebClientBase())
                    //{
                    //    var frame = BitmapFrame.Create(webClient.DownloadToStream(uri.AbsoluteUri), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    //    ImageBehavior.SetAnimatedSource(image, frame);
                    //}
                }
                else
                {
                    image.Source = bitmapImageCache[uri.AbsoluteUri];
                }
                UI.Dispatch(() => callback(image));
            }
        }
    }
}
