using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
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
        private WebClientBase webClient = new WebClientBase();        
        private Dictionary<String, ImageCacheItem> bitmapImageCache = new Dictionary<string,ImageCacheItem>();
        public void AddImage( Uri uri, Stream stream )
        {
            if (bitmapImageCache.ContainsKey(uri.OriginalString))
                return;

            var item = new ImageCacheItem(stream);
            
            lock (cacheLock)
                bitmapImageCache.Add(uri.OriginalString, item);

        }
        public void GetImage(Uri uri, int width, int height, Action<Image> callback, Action<Image> downloadComplete)
        {
            lock (getLock)
            {
                Image image = new Image();

                GetImageSource(uri, width, height, image, (imageSource) =>
                {
                    int x = -1;
                    int y = -1;
                    int w = -1;
                    int h = -1;
                    //URL contains offset parameter ?
                    if (uri.OriginalString.Contains("ubx="))
                    {
                        var ubx = Url.GetParameter(uri, "ubx");
                        var uby = Url.GetParameter(uri, "uby");
                        var ubw = Url.GetParameter(uri, "ubw");
                        var ubh = Url.GetParameter(uri, "ubh");
                        if( !Int32.TryParse(ubx, out x) ||
                            !Int32.TryParse(uby, out y) ||
                            !Int32.TryParse(ubw, out w) ||
                            !Int32.TryParse(ubh, out h))
                        {
                            x = -1;
                            y = -1;
                            w = -1;
                            h = -1;
                            callback(null);
                        }
                        else
                        {
                            x = Math.Abs(x);
                            y = Math.Abs(y);
                        }
                        image.Width = w;
                        image.Height = h;

                        if (imageSource.IsDownloading)
                        {
                            imageSource.DownloadCompleted += (o, e) =>
                            {
                                var cropped = new CroppedBitmap(imageSource, new Int32Rect(x, y, w, h));
                                image.Source = cropped;
                            };
                            UI.Dispatch(() => callback(image));
                        }
                        else
                        {
                            if( imageSource.PixelWidth > 1 && imageSource.PixelHeight > 1 && w > 0 && h > 0 )
                            {
                                var cropped = new CroppedBitmap(imageSource, new Int32Rect(x, y, w, h));
                                image.Source = cropped;
                                UI.Dispatch(() => callback(image));
                            }
                        }
                    }
                    else
                    {
                        image.Source = imageSource;
                        UI.Dispatch(() => callback(image));
                    }

                });
            }
        }
        private void SetupGifAnimation( Image image, BitmapImage bitmap)
        {
            UI.Dispatch(() =>
            {
                if (ImageBehavior.GetAnimatedSource(image) == ImageBehavior.AnimatedSourceProperty.DefaultMetadata.DefaultValue)
                {
                    ImageBehavior.SetRepeatBehavior(image, RepeatBehavior.Forever);
                    ImageBehavior.SetAutoStart(image, true);
                    ImageBehavior.SetAnimatedSource(image, bitmap);
                }
            });
        }

        private void FixImageSize(Image image, double newWidth)
        {
 
            image.MaxWidth = newWidth;
            if (newWidth >= 1 && newWidth < (Application.Current as App).ChatBoxWidth)
            {
                image.Width = newWidth;
            }
            else
            {
                image.Width = double.NaN;
            }

        }
        public void GetImageSource(Uri uri, int width, int height, Image image, Action<BitmapImage> callback)
        {
            if (String.IsNullOrWhiteSpace(uri.OriginalString))
                return;

            bool forceSize = false;

            if (width <= 1)
            {
                image.Width = 64;
            }
            else if (width <= (Application.Current as App).ChatBoxWidth )
            {
                image.Width = width;
                forceSize = true;
            }
            else
            {
                FixImageSize(image, width);
            }

            if( uri.OriginalString.Contains("uburl="))
            {
                Uri.TryCreate(HttpUtility.UrlDecode( Url.GetParameter(uri,"uburl")), UriKind.RelativeOrAbsolute, out uri);               
            }
            
            if (!bitmapImageCache.ContainsKey(uri.OriginalString) )
            {
                var item = new ImageCacheItem(uri, width, height) { 
                    DownloadComplete = (cacheItem) =>
                    {
                        UI.Dispatch(() => {
                            if (!forceSize || width > (Application.Current as App).ChatBoxWidth)
                                FixImageSize(image, cacheItem.Width);
                            if (uri.OriginalString.ToLower().Contains(".gif"))
                            {
                                SetupGifAnimation(image, cacheItem.Bitmap);
                            }                    
                        });
                    }
                };
            
                lock (cacheLock)
                    bitmapImageCache.Add(uri.OriginalString, item);

                callback(item.Bitmap);

            }
            else
            {
                var item = bitmapImageCache[uri.OriginalString];

                if (item == null)
                {
                    bitmapImageCache.Remove(uri.OriginalString);
                    return;
                }

                item.LastAccessed = DateTime.Now;
                if (!item.IsDownloading)
                {
                    if (!forceSize || width > (Application.Current as App).ChatBoxWidth)
                        FixImageSize(image, item.Width);

                    if (uri.OriginalString.ToLower().Contains(".gif"))
                    {
                        SetupGifAnimation(image, item.Bitmap);
                    }
                }
                callback(item.Bitmap);
            }

            //foreach (var item in bitmapImageCache.Keys.ToList())
            //{
            //    ImageCacheItem cacheItem;
            //    cacheItem = bitmapImageCache[item];
            //    if (cacheItem == null || DateTime.Now.Subtract(cacheItem.LastAccessed).Seconds > 120)
            //        lock (cacheLock)
            //            bitmapImageCache.Remove(item);
            //}

            
        }
    }
    public class ImageCacheItem
    {
        public ImageCacheItem( Stream inputStream )
        {
            LastAccessed = DateTime.Now;

            Bitmap = new BitmapImage();
            
            IsDownloading = true;

            Bitmap.BeginInit();
            Bitmap.CacheOption = BitmapCacheOption.OnLoad;
            Bitmap.CreateOptions = BitmapCreateOptions.None;
            Bitmap.StreamSource = inputStream;
            Bitmap.EndInit();
            Width = Bitmap.PixelWidth;
            Height = Bitmap.PixelHeight;

            IsDownloading = false;
            if (DownloadComplete != null)
                DownloadComplete(this);

            inputStream.Close();

        }

        public ImageCacheItem(Uri uri, int width, int height)
        {
            LastAccessed = DateTime.Now;

            Bitmap = new BitmapImage();
            IsDownloading = true;
            Bitmap.DownloadCompleted += (o, e) =>
            {
                Width = Bitmap.PixelWidth;
                Height = Bitmap.PixelHeight;
                if (DownloadComplete != null) 
                    DownloadComplete(this);
                IsDownloading = false;
            };
            Bitmap.BeginInit();
            Bitmap.CacheOption = BitmapCacheOption.OnLoad;
            Bitmap.CreateOptions = BitmapCreateOptions.None;
            Bitmap.UriCachePolicy = new HttpRequestCachePolicy(HttpCacheAgeControl.MaxAge, new TimeSpan(0), new TimeSpan(0));
            Bitmap.UriSource = uri;
            Bitmap.EndInit();
            Width = width;
            Height = height;
        }
        public DateTime LastAccessed { get; set; }
        public BitmapImage Bitmap { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsDownloading { get; set; }
        public Action<ImageCacheItem> DownloadComplete { get; set; }
    }
}
