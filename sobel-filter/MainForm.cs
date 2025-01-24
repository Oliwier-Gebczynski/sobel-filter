using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace sobel_filter
{
    public partial class MainForm : Form
    {
        private ProgressBar progressBar;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Detekcja Krawędzi - Sobel Filter";
            this.Size = new System.Drawing.Size(1200, 1000);

            Label lblImage = new Label { Text = "Wybierz zdjęcie:", Left = 20, Top = 20, AutoSize = true };
            TextBox txtImagePath = new TextBox { Left = 150, Top = 20, Width = 400 };
            Button btnBrowse = new Button { Text = "Przeglądaj", Left = 560, Top = 18 };
            PictureBox pictureBox = new PictureBox { Left = 150, Top = 60, Width = 400, Height = 300, BorderStyle = BorderStyle.Fixed3D };

            Button btnProcess = new Button { Text = "Przetwarzaj", Left = 20, Top = 400 };

            Label lblExecutionTime = new Label { Text = "Czas wykonania: ", Left = 20, Top = 450, AutoSize = true };
            progressBar = new ProgressBar { Left = 200, Top = 450, Width = 600, Height = 20 };

            Label lblResult = new Label { Text = "Bitmapa:", Left = 20, Top = 500, AutoSize = true };
            PictureBox pictureBoxResult = new PictureBox { Left = 150, Top = 500, Width = 400, Height = 300, BorderStyle = BorderStyle.Fixed3D };



            this.Controls.Add(lblImage);
            this.Controls.Add(txtImagePath);
            this.Controls.Add(btnBrowse);
            this.Controls.Add(pictureBox);
            this.Controls.Add(btnProcess);
            this.Controls.Add(lblExecutionTime);
            this.Controls.Add(lblResult);
            this.Controls.Add(pictureBoxResult);
            this.Controls.Add(progressBar);

            btnBrowse.Click += (sender, args) =>
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Obrazy|*.jpg;*.jpeg;*.png";
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        txtImagePath.Text = openFileDialog.FileName;
                        pictureBox.ImageLocation = openFileDialog.FileName;
                    }
                }
            };

            btnProcess.Click += async (sender, args) =>
            {
                string imagePath = txtImagePath.Text;

                if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                {
                    MessageBox.Show("Wybierz poprawny plik obrazu.", "Błąd");
                    return;
                }

                try
                {
                    var stopwatch = Stopwatch.StartNew();
                    progressBar.Value = 0;

                    // Wczytaj obraz do tablicy bajtów
                    using (var image = new Bitmap(imagePath))
                    {
                        var width = image.Width;
                        var height = image.Height;
                        var data = image.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        try
                        {
                            var stride = data.Stride;
                            var scan0 = data.Scan0;
                            var bufferSize = stride * height;
                            byte[] bytes = new byte[bufferSize];
                            Marshal.Copy(scan0, bytes, 0, bytes.Length);

                            // Wywołaj funkcję z DLL
                            string outputPath = Path.Combine(Path.GetDirectoryName(imagePath), "sobel_result.bmp");
                            NativeMethods.ApplySobelFilter(bytes, outputPath, width, height);

                            // Pokaż wynik
                            if (File.Exists(outputPath))
                            {
                                pictureBoxResult.Image = new Bitmap(outputPath);
                            }
                        }
                        finally
                        {
                            image.UnlockBits(data);
                        }
                    }

                    stopwatch.Stop();
                    lblExecutionTime.Text = $"Czas wykonania: {stopwatch.ElapsedMilliseconds} ms";

                    MessageBox.Show($"Plik wynikowy został zapisany w: {Path.GetDirectoryName(imagePath)}", "Sukces");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Wystąpił błąd: {ex.Message}", "Błąd");
                }
                finally
                {
                    progressBar.Value = 0; // Resetowanie paska po zakończeniu
                }
            };

            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxResult.SizeMode = PictureBoxSizeMode.Zoom;
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }

    public class NativeMethods
    {
        [DllImport("", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ApplySobelFilter(byte[] inputImage, string outputPath, int width, int height);
    }
}
