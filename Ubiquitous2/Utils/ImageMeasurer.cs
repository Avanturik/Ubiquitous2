using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UB.Model;

namespace UB.Utils
{
    public class ImageInfo
    {
        public ImageInfo()
        {
            Initialize();
        }
        public ImageInfo( ImageFormat format, Size dimensions)
        {
            Initialize();
            Format = format;
            Dimensions = dimensions;
        }
        private void Initialize()
        {
            FileSize = -1;
            Dimensions = new Size();
        }

        public long FileSize { get; set; }
        public Size Dimensions { get; set; }
        public ImageFormat Format { get; set; }
    }
    public static class ImageMeasurer
    {
        private static WebClientBase webClient = new WebClientBase() { KeepAlive = true };

        private static Dictionary<byte[], Func<BinaryReader, ImageInfo>> imageFormatDecoders = new Dictionary<byte[], Func<BinaryReader, ImageInfo>>()
        {
            { new byte[]{ 0x42, 0x4D }, DecodeBitmap},
            { new byte[]{ 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, DecodeGif },
            { new byte[]{ 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, DecodeGif },
            { new byte[]{ 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, DecodePng },
            { new byte[]{ 0xff, 0xd8 }, DecodeJfif },
        };

        public static void GetWebImageDimensions(string url, Action<ImageInfo> callback)
        {
            webClient.KeepAlive = true;
            var fileSize = webClient.GetContentLength(url);
            if( fileSize > 0 )
            {
                using( var stream = webClient.DownloadPartial(url, 0, 30) )
                using( var binaryReader = new BinaryReader(stream))
                {
                    if (callback != null)
                    {
                        ImageInfo imageInfo = null;
                        try
                        {
                            imageInfo = GetDimensions(binaryReader);
                            imageInfo.FileSize = fileSize;
                        }
                        catch
                        {
                            Log.WriteError("measuring image {0}", url);
                        }
                        callback(imageInfo??new ImageInfo());
                    }
                }
            }
            else 
            {
                Image testImage = Image.FromStream(webClient.DownloadToStream(url));
                var imageInfo = new ImageInfo()
                {
                    Format = testImage.RawFormat,
                    Dimensions = testImage.Size,
                };
                callback(imageInfo);
            }
        }
        public static long GetWebImageSize( string url )
        {
            return webClient.GetContentLength(url);
        }

        public static ImageInfo GetDimensions(BinaryReader binaryReader)
        {
            int maxMagicBytesLength = imageFormatDecoders.Keys.OrderByDescending(x => x.Length).First().Length;

            byte[] magicBytes = new byte[maxMagicBytesLength];

            for (int i = 0; i < maxMagicBytesLength; i += 1)
            {
                magicBytes[i] = binaryReader.ReadByte();

                foreach (var kvPair in imageFormatDecoders)
                {
                    if (magicBytes.StartsWith(kvPair.Key))
                    {
                        return kvPair.Value(binaryReader);
                    }
                }
            }

            return new ImageInfo();
        }

        private static bool StartsWith(this byte[] thisBytes, byte[] thatBytes)
        {
            for (int i = 0; i < thatBytes.Length; i += 1)
            {
                if (thisBytes[i] != thatBytes[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static short ReadLittleEndianInt16(this BinaryReader binaryReader)
        {
            byte[] bytes = new byte[sizeof(short)];
            for (int i = 0; i < sizeof(short); i += 1)
            {
                bytes[sizeof(short) - 1 - i] = binaryReader.ReadByte();
            }
            return BitConverter.ToInt16(bytes, 0);
        }

        private static int ReadLittleEndianInt32(this BinaryReader binaryReader)
        {
            byte[] bytes = new byte[sizeof(int)];
            for (int i = 0; i < sizeof(int); i += 1)
            {
                bytes[sizeof(int) - 1 - i] = binaryReader.ReadByte();
            }
            return BitConverter.ToInt32(bytes, 0);
        }

        private static ImageInfo DecodeBitmap(BinaryReader binaryReader)
        {
            binaryReader.ReadBytes(16);
            int width = binaryReader.ReadInt32();
            int height = binaryReader.ReadInt32();
            return new ImageInfo(ImageFormat.Bmp, new Size(width, height));
        }

        private static ImageInfo DecodeGif(BinaryReader binaryReader)
        {
            int width = binaryReader.ReadInt16();
            int height = binaryReader.ReadInt16();
            return new ImageInfo(ImageFormat.Gif, new Size(width, height));
        }

        private static ImageInfo DecodePng(BinaryReader binaryReader)
        {
            binaryReader.ReadBytes(8);
            int width = binaryReader.ReadLittleEndianInt32();
            int height = binaryReader.ReadLittleEndianInt32();
            return new ImageInfo(ImageFormat.Png, new Size(width, height));
        }

        private static ImageInfo DecodeJfif(BinaryReader binaryReader)
        {
            while (binaryReader.ReadByte() == 0xff)
            {
                byte marker = binaryReader.ReadByte();
                short chunkLength = binaryReader.ReadLittleEndianInt16();

                if (marker == 0xc0)
                {
                    binaryReader.ReadByte();

                    int height = binaryReader.ReadLittleEndianInt16();
                    int width = binaryReader.ReadLittleEndianInt16();
                    new ImageInfo(ImageFormat.Jpeg, new Size(width, height));
                }

                binaryReader.ReadBytes(chunkLength - 2);
            }
            return new ImageInfo();
        }
    }
}
