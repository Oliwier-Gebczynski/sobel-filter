using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace sobel_filter
{
    public static class ImageProcessing
    {
        public static Bitmap ConvertToGrayscale(string inputPath)
        {
            Bitmap colorImage = new Bitmap(inputPath);

            Bitmap grayImage = new Bitmap(colorImage.Width, colorImage.Height);

            for (int y = 0; y < grayImage.Height; y++)
            {
                for (int x = 0; x < grayImage.Width; x++)
                {
                    Color pixelColor = colorImage.GetPixel(x, y);

                    int grayValue = (int)(0.3 * pixelColor.R + 0.59 * pixelColor.G + 0.11 * pixelColor.B);

                    Color grayColor = Color.FromArgb(grayValue, grayValue, grayValue);

                    grayImage.SetPixel(x, y, grayColor);
                }
            }
            return grayImage;
        }

        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            int bytes = Math.Abs(bmpData.Stride) * bitmap.Height;
            byte[] pixelData = new byte[bytes];
            Marshal.Copy(bmpData.Scan0, pixelData, 0, bytes);
            bitmap.UnlockBits(bmpData);
            return pixelData;
        }

        public static Bitmap ByteArrayToBitmap(byte[] pixels, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            ColorPalette palette = bitmap.Palette;
            for (int i = 0; i < 256; i++)
                palette.Entries[i] = Color.FromArgb(i, i, i);
            bitmap.Palette = palette;
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
            bitmap.UnlockBits(bmpData);
            return bitmap;
        }

        public static string SaveBitmapToResults(Bitmap bitmap, string imagePath, string prefix)
        {
            string baseFolder = Path.Combine(
                Directory.GetParent(
                    Directory.GetParent(
                        Directory.GetParent(
                            Directory.GetParent(
                                Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName
                            ).FullName
                        ).FullName
                    ).FullName
                ).FullName,
                "results"
            );

            if (!Directory.Exists(baseFolder))
            {
                Directory.CreateDirectory(baseFolder);
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string outputFolder = Path.Combine(baseFolder, timestamp);
            Directory.CreateDirectory(outputFolder);

            string originalFileName = Path.GetFileNameWithoutExtension(imagePath);
            string outputFileName = $"{prefix}{originalFileName}.bmp";
            string outputPath = Path.Combine(outputFolder, outputFileName);

            bitmap.Save(outputPath, ImageFormat.Bmp);

            return outputFolder;
        }

        public static string SaveBitmapToResults(Bitmap bitmap, string prefix)
        {
            // Używamy domyślnej nazwy "result" jako podstawy do nazewnictwa pliku wynikowego.
            string defaultFileName = "result";
            return SaveBitmapToResults(bitmap, defaultFileName, prefix);
        }
    }
}