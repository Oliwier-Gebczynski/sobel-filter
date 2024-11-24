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

                return outputFolder;
            }
        }
    }
}
