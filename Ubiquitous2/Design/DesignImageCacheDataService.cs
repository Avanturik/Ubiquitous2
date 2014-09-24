using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight.Threading;
using UB.Model;

namespace UB.Design
{
    public class DesignImageCacheDataService : IImageDataSource
    {
        private object cacheLock = new object();
        private object getLock = new object();

        private Dictionary<String, BitmapImage> bitmapImageCache = new Dictionary<string, BitmapImage>();
        private List<Image> imageCache = new List<Image>();

        public void GetImage(Uri uri, int width, int height, Action<Image> callback, Action<Image> downloadComplete)
        {
            var trace = new StackTrace(true);
            var frame = trace.GetFrame(0);
            var sourceCodeDir = Path.GetDirectoryName(frame.GetFileName());

            Image image = new Image();
            image.Height = 16;
            image.Width = image.Height;

            callback(image);
            return;


        }


        public void GetImageSource(Uri uri, int width, int height, Action<BitmapImage> callback)
        {
            
        }


        public void GetImageSource(Uri uri, int width, int height, Image image, Action<BitmapImage> callback)
        {
            throw new NotImplementedException();
        }
    }
}
