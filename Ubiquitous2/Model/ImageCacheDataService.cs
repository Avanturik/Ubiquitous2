using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Cache;
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

        public void GetImage(Uri uri, int width, int height, Action<Image> callback, Action<Image> downloadComplete)
        {
            lock (getLock)
            {
                if( width ==0 || height == 0)
                {
                    ImageInfo.GetWebImageSize(uri.AbsoluteUri, (size) => {
                        width = size.Width;
                        height = size.Height;
                    });
                }
                GetImageSource(uri, width, height, (imageSource) =>
                {
                    Image image = new Image() { Width = width, Height = height };
                    image.Source = imageSource;
                    imageSource.DownloadCompleted += (o, e) =>
                    {
                        var handler = downloadComplete;
                        if (downloadComplete != null)
                            downloadComplete(image);
                    };
                    UI.Dispatch(() => callback(image));
                });
            }
        }


        public void GetImageSource(Uri uri, int width, int height, Action<BitmapImage> callback)
        {
            if (!bitmapImageCache.ContainsKey(uri.AbsoluteUri))
            {
                var bitmap = new BitmapImage();
                using (var webClient = new WebClientBase())
                using( var stream = webClient.DownloadToStream(uri.AbsoluteUri))
                {                    
                    
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.CreateOptions = BitmapCreateOptions.None;
                    bitmap.UriCachePolicy = new HttpRequestCachePolicy(HttpCacheAgeControl.MaxAge, new TimeSpan(0), new TimeSpan(0));
                    bitmap.UriSource = uri;
                    bitmap.EndInit();

                    bitmapImageCache.Add(uri.AbsoluteUri, bitmap);
                }
            }
            callback(bitmapImageCache[uri.AbsoluteUri]);
        }
    }
}
