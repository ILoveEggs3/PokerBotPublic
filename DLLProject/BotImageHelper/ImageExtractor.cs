using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotImageHelper
{
    public static class ImageExtractor
    {
        //            Rectangle zoneTypeCarte1 = new Rectangle(CGGPokerConsantes.GG_HAND_OFFSET1_WIDTH, CGGPokerConsantes.GG_HAND_OFFSET1_HEIGHT, CGGPokerConsantes.GG_HAND_TYPE_RECTANGLE_WIDTH, CGGPokerConsantes.GG_HAND_TYPE_RECTANGLE_HEIGHT);
        //            Bitmap bmp = FFCurrentImage;
        //Rectangle zoneTypeCarte1 = new Rectangle(CGGPokerConsantes.GG_HAND_OFFSET1_WIDTH, CGGPokerConsantes.GG_HAND_OFFSET1_HEIGHT, CGGPokerConsantes.GG_HAND_TYPE_RECTANGLE_WIDTH, CGGPokerConsantes.GG_HAND_TYPE_RECTANGLE_HEIGHT);
        //bmp = (Bitmap)bmp.Clone(zoneTypeCarte1, bmp.PixelFormat);
           // return bmp;

        private static Bitmap cropBitmap(Bitmap bmp, Tuple<int, int> coord, Tuple<int, int> dimension)
        {
            Rectangle zoneTypeCarte1 = new Rectangle(coord.Item1, coord.Item2, dimension.Item1, dimension.Item2);
            bmp = bmp.Clone(zoneTypeCarte1, bmp.PixelFormat);
            return bmp;
        }
        public static int cropImagesFromSources(string source, string destination, string name, Tuple<int,int> coord, Tuple<int, int> dimension)
        {
            int nbImg = 0;
            string ext = source.Substring(source.Length - 3, 3);
            if (ext.ToLower() == "bmp")
            {
                Bitmap bmp = new Bitmap(@source);
                bmp = cropBitmap(bmp, coord, dimension);
                bmp.Save(@destination + "\\" + name + ".bmp", ImageFormat.Bmp);
                nbImg = 1;
            }
            else if (ext.ToLower() == "zip")
            {
                // Normalizes the path.
                destination = Path.GetFullPath(destination);

                // Ensures that the last character on the extraction path
                // is the directory separator char. 
                // Without this, a malicious zip file could try to traverse outside of the expected
                // extraction path.
                if (!destination.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    destination += Path.DirectorySeparatorChar;
                using (ZipArchive archive = ZipFile.OpenRead(source))
                {
                    string tempFolderName = "temp\\";
                    string fullTempFolderPath = destination + tempFolderName;

                    System.IO.Directory.CreateDirectory(fullTempFolderPath);
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase))
                        {
                            // Gets the full path to ensure that relative segments are removed.
                            string destinationPath = Path.GetFullPath(Path.Combine(fullTempFolderPath, String.Format("temp{0}.bmp",nbImg)));

                            // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                            // are case-insensitive.
                            if (destinationPath.StartsWith(fullTempFolderPath, StringComparison.Ordinal))
                            {
                                entry.ExtractToFile(destinationPath);
                                Bitmap bmp2 = new Bitmap(destinationPath);
                                Bitmap bmp = cropBitmap(bmp2, coord, dimension);
                                string path = String.Format(@"{0}\\{1}{2}.bmp", destination, name, nbImg++);
                                bmp.Save(@path, ImageFormat.Bmp);
                                bmp.Dispose();
                                bmp2.Dispose();
                            }
                        }
                    }
                    if (Directory.Exists(@fullTempFolderPath))
                    {
                        Directory.Delete(@fullTempFolderPath, true);
                    }

                }
            }
            else
            {
                throw new Exception("Unrecognised exception");
            }
            return nbImg;
        }
    }
}
