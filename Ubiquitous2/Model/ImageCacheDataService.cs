using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Threading;

namespace UB.Model
{
    public class ImageCacheDataService : IImageDataSource
    {
        private object cacheLock = new object();
        private object getLock = new object();

        private Dictionary<String, BitmapImage> bitmapImageCache = new Dictionary<string, BitmapImage>();
        private List<Image> imageCache = new List<Image>();
        
        public void GetImage(Uri uri, int width, int height, Action<Image> callback)
        {
            lock(getLock)
            {
                imageCache.Add(new Image() { Width = width, Height = height });
                Image image = imageCache.LastOrDefault();

                if (!bitmapImageCache.ContainsKey(uri.AbsoluteUri))
                {
                    bitmapImageCache.Add(uri.AbsoluteUri, new BitmapImage(uri));
                }
                
                image.Source = bitmapImageCache[uri.AbsoluteUri];
                callback(image);
            }
        }
    }
}
