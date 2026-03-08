using System.Drawing;
using System.Drawing.Imaging;

namespace Geisterhand.Core.Screen;

public class ImageDiffService
{
    /// <summary>
    /// Compare two bitmaps pixel-by-pixel and return diff statistics.
    /// </summary>
    public (bool match, double diffPercent, Bitmap? diffImage) Compare(Bitmap baseline, Bitmap current, double threshold = 0.01)
    {
        int width = Math.Min(baseline.Width, current.Width);
        int height = Math.Min(baseline.Height, current.Height);
        int totalPixels = width * height;
        int diffCount = 0;

        var diffBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var bPixel = baseline.GetPixel(x, y);
                var cPixel = current.GetPixel(x, y);

                if (bPixel.R != cPixel.R || bPixel.G != cPixel.G || bPixel.B != cPixel.B)
                {
                    diffCount++;
                    diffBitmap.SetPixel(x, y, Color.Red);
                }
                else
                {
                    // Dim the matching pixels
                    diffBitmap.SetPixel(x, y, Color.FromArgb(128, bPixel.R, bPixel.G, bPixel.B));
                }
            }
        }

        // Account for size differences
        if (baseline.Width != current.Width || baseline.Height != current.Height)
        {
            int maxPixels = Math.Max(baseline.Width, current.Width) * Math.Max(baseline.Height, current.Height);
            diffCount += maxPixels - totalPixels;
            totalPixels = maxPixels;
        }

        double diffPercent = totalPixels > 0 ? (double)diffCount / totalPixels : 0;
        bool isMatch = diffPercent <= threshold;

        return (isMatch, diffPercent, diffBitmap);
    }
}
