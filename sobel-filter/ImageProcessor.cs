using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace sobel_filter
{
    public static class ImageProcessor
    {
        public static string ConvertToBitmap(string imagePath)
        {
            string baseFolder = Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName).FullName).FullName).FullName).FullName, "results");

            if (!Directory.Exists(baseFolder))
            {
                Directory.CreateDirectory(baseFolder);
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string outputFolder = Path.Combine(baseFolder, timestamp);
            Directory.CreateDirectory(outputFolder);

            using (Image originalImage = Image.FromFile(imagePath))
            {
                string originalFileName = Path.Combine(outputFolder, "original_" + Path.GetFileName(imagePath));
                string bitmapFileName = Path.Combine(outputFolder, "bitmap_" + Path.GetFileNameWithoutExtension(imagePath) + ".bmp");

                originalImage.Save(originalFileName);

                Bitmap bitmap = new Bitmap(originalImage);

                bitmap.Save(bitmapFileName, ImageFormat.Bmp);

                // Convert to grayscale
                string grayscaleFileName = Path.Combine(outputFolder, "grayscale_" + Path.GetFileNameWithoutExtension(imagePath) + ".bmp");
                Bitmap grayscaleBitmap = ConvertToGrayscale(bitmap);
                grayscaleBitmap.Save(grayscaleFileName, ImageFormat.Bmp);

                return outputFolder;
            }
        }

        private static Bitmap ConvertToGrayscale(Bitmap bitmap)
        {
            Bitmap grayscaleBitmap = new Bitmap(bitmap.Width, bitmap.Height);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color originalColor = bitmap.GetPixel(x, y);

                    int intensity = (originalColor.R + originalColor.G + originalColor.B) / 3;

                    Color grayscaleColor = Color.FromArgb(intensity, intensity, intensity);

                    grayscaleBitmap.SetPixel(x, y, grayscaleColor);
                }
            }

            return grayscaleBitmap;
        }
    }
}
