using System;
using System.Drawing;

namespace Shared.Helpers
{
    public static class CImageHelper
    {
        private static readonly object lockObject = new object();
        // Source: http://stackoverflow.com/questions/6501797/resize-image-proportionally-with-maxheight-and-maxwidth-constraints
        public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            lock (lockObject)
            {
                var ratioX = (double)maxWidth / image.Width;
                var ratioY = (double)maxHeight / image.Height;
                var ratio = Math.Min(ratioX, ratioY);

                var newWidth = (int)(image.Width * ratio);
                var newHeight = (int)(image.Height * ratio);

                var newImage = new Bitmap(newWidth, newHeight);

                using (var graphics = Graphics.FromImage(newImage))
                    graphics.DrawImage(image, 0, 0, newWidth, newHeight);

                return newImage;
            }
        }
    }
}
