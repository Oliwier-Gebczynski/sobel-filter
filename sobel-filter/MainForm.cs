using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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

                    // Asynchroniczne przetwarzanie obrazu
                    await Task.Run(() =>
                    {
                        for (int i = 0; i <= 100; i++)
                        {
                            Thread.Sleep(1); // Symulacja przetwarzania
                            Invoke(new Action(() => progressBar.Value = i));
                        }
                    });

                    string resultFolder = ImageProcessor.ConvertToBitmap(imagePath);

                    stopwatch.Stop();
                    lblExecutionTime.Text = $"Czas wykonania: {stopwatch.ElapsedMilliseconds} ms";

                    string bitmapFilePath = Path.Combine(resultFolder, "bitmap_" + Path.GetFileNameWithoutExtension(imagePath) + ".bmp");
                    if (File.Exists(bitmapFilePath))
                    {
                        pictureBoxResult.Image = new Bitmap(bitmapFilePath);
                    }

                    MessageBox.Show($"Pliki zostały zapisane w folderze: {resultFolder}", "Sukces");
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
}
