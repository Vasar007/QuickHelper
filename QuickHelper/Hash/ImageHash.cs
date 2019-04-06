using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media.Imaging;

namespace QuickHelper.Hash
{
    internal static class ImageHash
    {
        public static string GetMD5Hash(byte[] buffer)
        {
            using (var md5Hasher = MD5.Create())
            {

                var data = md5Hasher.ComputeHash(buffer);

                var sBuilder = new StringBuilder();
                foreach (var elem in data)
                {
                    sBuilder.Append(elem.ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        public static byte[] SaveImage(BitmapSource bitmap)
        {
            using (var ms = new MemoryStream())
            {
                var encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(ms);

                return ms.GetBuffer();
            }
        }

        public static BitmapSource LoadImage(byte[] imageData)
        {
            using (var ms = new MemoryStream(imageData))
            {
                var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.PreservePixelFormat,
                                                   BitmapCacheOption.OnLoad);
                return decoder.Frames[0];
            }
        }
    }
}
