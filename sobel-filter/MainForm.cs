using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sobel_filter
{
    public partial class MainForm : Form
    {
        [DllImport("", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CppSobelFunction(byte[] inputImage, byte[] outputImage, int width, int height);

        // Modyfikacja: używamy wskaźników byte* (podobnie jak w działającym przykładzie)
        [DllImport("",
            EntryPoint = "AsmSobelFunction", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern void AsmSobelFunction(byte* inputImage, byte* outputImage, int width, int height);

        public MainForm()
        {
            InitializeComponent();
        }

        private TextBox txtFilePath;
        private Button btnBrowse;
        private GroupBox groupBoxDll;
        private RadioButton rbCpp;
        private RadioButton rbAsm;
        private Button btnStart;
        private PictureBox pictureBoxResult;
        private Label lblImage;
        private Label lblTime;
        private GroupBox threadOptions;
        private RadioButton thread1;
        private RadioButton thread2;
        private RadioButton thread4;
        private RadioButton thread8;
        private RadioButton thread16;
        private RadioButton thread32;
        private RadioButton thread64;

        private void InitializeComponent()
        {
            this.Text = "Detekcja Krawędzi - Filtr Scharra";
            this.Size = new Size(1000, 1000);
            this.StartPosition = FormStartPosition.CenterScreen;

            int formWidth = this.ClientSize.Width;
            int formHeight = this.ClientSize.Height;

            // TextBox do wyświetlania ścieżki do pliku
            txtFilePath = new TextBox();
            txtFilePath.Size = new Size(600, 25);
            txtFilePath.Location = new Point(20, 20);
            txtFilePath.ReadOnly = true;

            // Label "Wybierz plik" umieszczony nad TextBoxem
            lblImage = new Label();
            lblImage.Text = "Wybierz plik";
            lblImage.AutoSize = true;
            lblImage.Location = new Point(20, 0);

            // Przycisk do przeglądania pliku
            btnBrowse = new Button();
            btnBrowse.Text = "Przeglądaj...";
            btnBrowse.Size = new Size(90, 25);
            btnBrowse.Location = new Point(txtFilePath.Right + 10, 20);
            btnBrowse.Click += BtnBrowse_Click;

            // GroupBox do wyboru biblioteki (C++/ASM)
            groupBoxDll = new GroupBox();
            groupBoxDll.Text = "Wybierz bibliotekę";
            groupBoxDll.Size = new Size(200, 80);
            groupBoxDll.Location = new Point(20, txtFilePath.Bottom + 20);

            rbCpp = new RadioButton();
            rbCpp.Text = "C++";
            rbCpp.Location = new Point(10, 20);
            rbCpp.Checked = true;

            rbAsm = new RadioButton();
            rbAsm.Text = "ASM";
            rbAsm.Location = new Point(10, 50);

            groupBoxDll.Controls.Add(rbCpp);
            groupBoxDll.Controls.Add(rbAsm);

            // GroupBox do wyboru liczby wątków
            threadOptions = new GroupBox();
            threadOptions.Text = "Wybierz ilość wątków:";
            threadOptions.Size = new Size(150, 200);
            threadOptions.Location = new Point(formWidth - threadOptions.Width - 20, 65);

            thread1 = new RadioButton();
            thread1.Text = "1";
            thread1.Location = new Point(10, 15);
            thread1.Checked = true;

            thread2 = new RadioButton();
            thread2.Text = "2";
            thread2.Location = new Point(10, 40);

            thread4 = new RadioButton();
            thread4.Text = "4";
            thread4.Location = new Point(10, 65);

            thread8 = new RadioButton();
            thread8.Text = "8";
            thread8.Location = new Point(10, 90);

            thread16 = new RadioButton();
            thread16.Text = "16";
            thread16.Location = new Point(10, 115);

            thread32 = new RadioButton();
            thread32.Text = "32";
            thread32.Location = new Point(10, 140);

            thread64 = new RadioButton();
            thread64.Text = "64";
            thread64.Location = new Point(10, 165);

            threadOptions.Controls.Add(thread1);
            threadOptions.Controls.Add(thread2);
            threadOptions.Controls.Add(thread4);
            threadOptions.Controls.Add(thread8);
            threadOptions.Controls.Add(thread16);
            threadOptions.Controls.Add(thread32);
            threadOptions.Controls.Add(thread64);

            // Przycisk START - umieszczony na środku pod groupBox'em z biblioteką
            btnStart = new Button();
            btnStart.Text = "START";
            btnStart.Size = new Size(90, 40);
            btnStart.Location = new Point((formWidth - btnStart.Width) / 2, groupBoxDll.Bottom + 20);
            btnStart.Click += BtnStart_Click;

            // PictureBox do wyświetlania obrazu wynikowego
            pictureBoxResult = new PictureBox();
            pictureBoxResult.Size = new Size(650, 650);
            pictureBoxResult.Location = new Point((formWidth - pictureBoxResult.Width) / 2, btnStart.Bottom + 20);
            pictureBoxResult.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxResult.SizeMode = PictureBoxSizeMode.StretchImage;

            // Label do wyświetlania czasu wykonania algorytmu
            lblTime = new Label();
            lblTime.AutoSize = true;
            lblTime.Location = new Point((formWidth / 2) - 50, formHeight - 40);

            // Dodawanie kontrolek do formy
            this.Controls.Add(lblImage);
            this.Controls.Add(txtFilePath);
            this.Controls.Add(btnBrowse);
            this.Controls.Add(groupBoxDll);
            this.Controls.Add(threadOptions);
            this.Controls.Add(btnStart);
            this.Controls.Add(pictureBoxResult);
            this.Controls.Add(lblTime);
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.png;*bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = openFileDialog.FileName;
                    Bitmap image = new Bitmap(txtFilePath.Text);
                    pictureBoxResult.Image = image;
                }
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtFilePath.Text) && System.IO.File.Exists(txtFilePath.Text))
            {
                try
                {
                    Bitmap grayImage = ImageProcessing.ConvertToGrayscale(txtFilePath.Text);
                    pictureBoxResult.Image = grayImage;

                    int width = grayImage.Width;
                    int height = grayImage.Height;
                    int bufferSize = width * height;

                    // Przygotowanie danych wejściowych i bufora wyjściowego
                    byte[] inputPixels = ImageProcessing.BitmapToByteArray(grayImage);
                    byte[] outputPixels = new byte[bufferSize];

                    Stopwatch stopWatch = new Stopwatch();

                    if (rbAsm.Checked)
                    {
                        // Używamy fixed do uzyskania wskaźników z tablic (podobnie jak w działającym przykładzie)
                        unsafe
                        {
                            fixed (byte* inputPtr = inputPixels)
                            fixed (byte* outputPtr = outputPixels)
                            {
                                stopWatch.Start();
                                AsmSobelFunction(inputPtr, outputPtr, width, height);
                                stopWatch.Stop();
                            }
                        }
                        lblTime.Text = $"{stopWatch.Elapsed.TotalMilliseconds:F2} ms";
                    }
                    else
                    {
                        stopWatch.Start();
                        Array.Clear(outputPixels, 0, outputPixels.Length);

                        int numThreads = GetSelectedThreadCount();
                        int innerHeight = height - 2;
                        if (innerHeight <= 0)
                            innerHeight = 0;
                        numThreads = Math.Min(numThreads, Math.Max(innerHeight, 1));

                        if (numThreads == 1)
                        {
                            CppSobelFunction(inputPixels, outputPixels, width, height);
                        }
                        else
                        {
                            int chunkSize = innerHeight / numThreads;
                            int remainder = innerHeight % numThreads;
                            int currentStart = 1;
                            var chunks = new List<Tuple<int, int>>();

                            for (int i = 0; i < numThreads; i++)
                            {
                                int currentChunkSize = chunkSize + (i < remainder ? 1 : 0);
                                int currentEnd = currentStart + currentChunkSize - 1;
                                currentEnd = Math.Min(currentEnd, height - 2);
                                chunks.Add(Tuple.Create(currentStart, currentEnd));
                                currentStart = currentEnd + 1;
                            }

                            Parallel.ForEach(chunks, new ParallelOptions { MaxDegreeOfParallelism = numThreads }, chunk =>
                            {
                                int outputStartRow = chunk.Item1;
                                int outputEndRow = chunk.Item2;

                                int inputStartRow = Math.Max(0, outputStartRow - 1);
                                int inputEndRow = Math.Min(height - 1, outputEndRow + 1);
                                int sliceHeight = inputEndRow - inputStartRow + 1;

                                if (sliceHeight < 3)
                                    return;

                                byte[] inputSlice = new byte[sliceHeight * width];
                                Array.Copy(inputPixels, inputStartRow * width, inputSlice, 0, inputSlice.Length);

                                byte[] outputSlice = new byte[sliceHeight * width];
                                CppSobelFunction(inputSlice, outputSlice, width, sliceHeight);

                                int validStart = 1;
                                int validEnd = sliceHeight - 2;
                                int validRowCount = validEnd - validStart + 1;

                                for (int i = 0; i < validRowCount; i++)
                                {
                                    int mainRow = outputStartRow + i;
                                    if (mainRow >= height) break;

                                    Array.Copy(
                                        outputSlice,
                                        (validStart + i) * width,
                                        outputPixels,
                                        mainRow * width,
                                        width
                                    );
                                }
                            });
                        }
                        stopWatch.Stop();
                        lblTime.Text = $"{stopWatch.Elapsed.TotalMilliseconds:F2} ms";
                    }

                    Bitmap edgeImage = ImageProcessing.ByteArrayToBitmap(outputPixels, width, height);
                    ImageProcessing.SaveBitmapToResults(edgeImage, "processed_");
                    pictureBoxResult.Image = edgeImage;

                    MessageBox.Show("Processing complete!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Wystąpił błąd podczas przetwarzania obrazu: {ex.Message}", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Wybierz poprawny plik przed uruchomieniem algorytmu!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private int GetSelectedThreadCount()
        {
            if (thread1.Checked) return 1;
            if (thread2.Checked) return 2;
            if (thread4.Checked) return 4;
            if (thread8.Checked) return 8;
            if (thread16.Checked) return 16;
            if (thread32.Checked) return 32;
            if (thread64.Checked) return 64;
            return 1;
        }
    }
}

