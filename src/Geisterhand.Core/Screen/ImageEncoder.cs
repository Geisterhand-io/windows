using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Geisterhand.Core.Screen;

public static class ImageEncoder
{
    /// <summary>
    /// Encode a bitmap to a byte array in the specified format.
    /// </summary>
    public static byte[] Encode(Bitmap bitmap, string format, int quality = 85)
    {
        using var ms = new MemoryStream();
        switch (format.ToLowerInvariant())
        {
            case "jpeg":
            case "jpg":
                var jpegEncoder = GetEncoder(ImageFormat.Jpeg);
                var qualityParam = new EncoderParameters(1);
                qualityParam.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);
                bitmap.Save(ms, jpegEncoder!, qualityParam);
                break;
            case "png":
            default:
                bitmap.Save(ms, ImageFormat.Png);
                break;
        }
        return ms.ToArray();
    }

    /// <summary>
    /// Encode a bitmap to a Base64 string.
    /// </summary>
    public static string EncodeToBase64(Bitmap bitmap, string format, int quality = 85)
    {
        byte[] bytes = Encode(bitmap, format, quality);
        return Convert.ToBase64String(bytes);
    }

    private static ImageCodecInfo? GetEncoder(ImageFormat format)
    {
        var codecs = ImageCodecInfo.GetImageDecoders();
        return codecs.FirstOrDefault(c => c.FormatID == format.Guid);
    }
}
