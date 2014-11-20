using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfAnimatedGif
{
    public class CompressedBitmapSource
    {
        public CompressedBitmapSource(object source)
        {
            if (source == null)
                return;

            if( source is CompressedBitmapSource )
            {
                var compressedBitmap = (source as CompressedBitmapSource);
                Bytes = compressedBitmap.Bytes;
                Width = compressedBitmap.Width;
                Height = compressedBitmap.Height;
            }
            else if( source is BitmapSource )
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((source as BitmapSource)));
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    encoder.Save(memoryStream);
                    memoryStream.Flush();
                    Bytes = memoryStream.ToArray();
                }
                Width = encoder.Frames[0].PixelWidth;
                Height = encoder.Frames[0].PixelHeight;
            }
            else if( source is byte[])
            {
                using (MemoryStream stream = new MemoryStream((byte[])source))
                {
                    var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    Width = decoder.Frames[0].PixelWidth;
                    Height = decoder.Frames[0].PixelHeight;
                }
                Bytes = source as byte[];
            }
        }
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] Bytes { get; set; }

        public static implicit operator BitmapSource(CompressedBitmapSource compBitmap)
        {
            if (compBitmap == null)
                return null;

            using (MemoryStream stream = new MemoryStream(compBitmap.Bytes))
            {
                var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                return new FormatConvertedBitmap(decoder.Frames[0], PixelFormats.Pbgra32, null, 1);
            }
        }
        public static implicit operator byte[](CompressedBitmapSource compBitmap)
        {
            if (compBitmap == null)
                return null;

            using (MemoryStream stream = new MemoryStream(compBitmap.Bytes))
            {
                var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                return new FormatConvertedBitmap(decoder.Frames[0],PixelFormats.Pbgra32,null, 1).ConvertToByteArray();
            }
        }
        public static implicit operator CompressedBitmapSource(BitmapSource source)
        {
            return new CompressedBitmapSource(source);
        }

    }
}
