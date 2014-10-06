using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace UB.Model
{
    interface IImageDataSource
    {
        void GetImage(Uri uri, int width, int height, Action<Image> callback, Action<Image> downloadComplete);
        void GetImageSource(Uri uri, int width, int height, Image image, Action<BitmapImage> callback);
        void AddImage(Uri uri, Stream stream);
    }
}
