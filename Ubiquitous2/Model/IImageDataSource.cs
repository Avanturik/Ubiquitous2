using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UB.Model
{
    interface IImageDataSource
    {
        void GetImage(Uri uri, int width, int height, Action<Image> callback);
    }
}
