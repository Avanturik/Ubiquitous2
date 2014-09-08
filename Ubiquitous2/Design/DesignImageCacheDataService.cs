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
        
        public void GetImage(Uri uri, int width, int height, Action<Image> callback)
        {
            var trace = new StackTrace(true);
            var frame = trace.GetFrame(0);
            var sourceCodeDir = Path.GetDirectoryName(frame.GetFileName());
            callback(new Image() { Width = 16, Height = 16 });
            return; 


            var imageUri = new Uri(sourceCodeDir + @"/favicon.ico");
            var bitmap = new BitmapImage(imageUri);
            Image image = new Image();
            image.Height = 16;
            image.Width = image.Height;
            image.Source = bitmap;

            callback(image);
        }
    }
}
