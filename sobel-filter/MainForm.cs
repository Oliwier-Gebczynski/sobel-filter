using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sobel_filter
{
    public partial class MainForm : Form
    {
        // Import the C++ function from the DLL
        [DllImport("C:\\Users\\oliwi\\Documents\\GitHub\\sobel-filter\\sobel-filter\\x64\\Debug\\sobel-cpp-dll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CppSobelFunction(byte[] inputImage, byte[] outputImage, int width, int height);

        // Import the ASM function from the DLL – using byte* pointers (remember to use the unsafe context)
        [DllImport("C:\\Users\\oliwi\\Documents\\GitHub\\sobel-filter\\sobel-filter\\x64\\Debug\\sobel-asm-dll.dll",
            EntryPoint = "AsmSobelFunction", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern void AsmSobelFunction(byte* inputImage, byte* outputImage, int width, int height);

        // Declaration of controls – both existing and new ones
        private TextBox txtFilePath;
        private Button btnBrowse;
        private GroupBox groupBoxDll;
        private RadioButton rbCpp;
        private RadioButton rbAsm;
        private Button btnStart;
        private PictureBox pictureBoxResult;
        private PictureBox pictureBoxOriginal;
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

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Main form settings
            this.Text = "Filtr Sobela";
            this.ClientSize = new Size(1000, 1000);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int margin = 20;
            int formWidth = this.ClientSize.Width;
            int formHeight = this.ClientSize.Height;

            // --- TOP SECTION – file selection, library selection, thread count selection, START button ---

            // Label "Select file"
            lblImage = new Label();
            lblImage.Text = "Wybierz plik"; // "Select file"
            lblImage.AutoSize = true;
            lblImage.Location = new Point(margin, margin);

            // TextBox for displaying the file path
            txtFilePath = new TextBox();
            txtFilePath.Size = new Size(600, 25);
            txtFilePath.Location = new Point(margin, lblImage.Bottom + 5);
            txtFilePath.ReadOnly = true;

            // Button to browse for a file
            btnBrowse = new Button();
            btnBrowse.Text = "Przeglądaj..."; // "Browse..."
            btnBrowse.Size = new Size(100, 25);
            btnBrowse.Location = new Point(txtFilePath.Right + margin, txtFilePath.Top);
            btnBrowse.Click += BtnBrowse_Click;

            // GroupBox for selecting the library (C++/ASM)
            groupBoxDll = new GroupBox();
            groupBoxDll.Text = "Wybierz bibliotekę"; // "Select library"
            groupBoxDll.Size = new Size(200, 80);
            groupBoxDll.Location = new Point(margin, 100);

            rbCpp = new RadioButton();
            rbCpp.Text = "C++";
            rbCpp.Location = new Point(10, 20);
            rbCpp.Checked = true;

            rbAsm = new RadioButton();
            rbAsm.Text = "ASM";
            rbAsm.Location = new Point(10, 45);

            groupBoxDll.Controls.Add(rbCpp);
            groupBoxDll.Controls.Add(rbAsm);

            // GroupBox for selecting the number of threads
            threadOptions = new GroupBox();
            threadOptions.Text = "Wybierz ilość wątków:"; // "Select number of threads:"
            threadOptions.Size = new Size(150, 200);
            threadOptions.Location = new Point(groupBoxDll.Right + margin, 100);

            thread1 = new RadioButton();
            thread1.Text = "1";
            thread1.Location = new Point(10, 20);
            thread1.Checked = true;

            thread2 = new RadioButton();
            thread2.Text = "2";
            thread2.Location = new Point(10, 45);

            thread4 = new RadioButton();
            thread4.Text = "4";
            thread4.Location = new Point(10, 70);

            thread8 = new RadioButton();
            thread8.Text = "8";
            thread8.Location = new Point(10, 95);

            thread16 = new RadioButton();
            thread16.Text = "16";
            thread16.Location = new Point(10, 120);

            thread32 = new RadioButton();
            thread32.Text = "32";
            thread32.Location = new Point(10, 145);

            thread64 = new RadioButton();
            thread64.Text = "64";
            thread64.Location = new Point(10, 170);

            threadOptions.Controls.Add(thread1);
            threadOptions.Controls.Add(thread2);
            threadOptions.Controls.Add(thread4);
            threadOptions.Controls.Add(thread8);
            threadOptions.Controls.Add(thread16);
            threadOptions.Controls.Add(thread32);
            threadOptions.Controls.Add(thread64);

            // START button – placed below the group boxes
            btnStart = new Button();
            btnStart.Text = "START";
            btnStart.Size = new Size(100, 40);
            btnStart.Location = new Point(450, 900);
            btnStart.Click += BtnStart_Click;

            // --- MIDDLE SECTION – two PictureBoxes ---
            // Calculate the width of the available area (with margins)
            int pictureAreaX = margin;
            int pictureAreaY = groupBoxDll.Bottom + 2 * margin + 100;
            int pictureAreaWidth = formWidth - 3 * margin;
            int pictureBoxWidth = pictureAreaWidth / 2;
            int pictureBoxHeight = pictureAreaWidth / 2; // Reserve 80px for progress bar and possible labels

            // PictureBox for displaying the original image (left side)
            pictureBoxOriginal = new PictureBox();
            pictureBoxOriginal.Location = new Point(pictureAreaX, pictureAreaY);
            pictureBoxOriginal.Size = new Size(pictureBoxWidth, pictureBoxHeight);
            pictureBoxOriginal.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxOriginal.SizeMode = PictureBoxSizeMode.StretchImage;

            // PictureBox for displaying the processed image (right side)
            pictureBoxResult = new PictureBox();
            pictureBoxResult.Location = new Point(pictureBoxOriginal.Right + margin, pictureAreaY);
            pictureBoxResult.Size = new Size(pictureBoxWidth, pictureBoxHeight);
            pictureBoxResult.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxResult.SizeMode = PictureBoxSizeMode.StretchImage;

            // --- BOTTOM SECTION – label for execution time ---
            lblTime = new Label();
            lblTime.AutoSize = true;
            lblTime.Location = new Point(margin, formHeight - margin - 45);

            // Add all controls to the form
            this.Controls.Add(lblImage);
            this.Controls.Add(txtFilePath);
            this.Controls.Add(btnBrowse);
            this.Controls.Add(groupBoxDll);
            this.Controls.Add(threadOptions);
            this.Controls.Add(btnStart);
            this.Controls.Add(pictureBoxOriginal);
            this.Controls.Add(pictureBoxResult);
            this.Controls.Add(lblTime);
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            // Open file dialog to select an image file
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.png;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFilePath.Text = openFileDialog.FileName;
                    Bitmap image = new Bitmap(txtFilePath.Text);
                    // Display the original image on the left
                    pictureBoxOriginal.Image = image;
                    // Clear the processed image (right side)
                    pictureBoxResult.Image = null;
                }
            }
        }

        private unsafe void BtnStart_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtFilePath.Text) && System.IO.File.Exists(txtFilePath.Text))
            {
                try
                {
                    // Convert the image to grayscale
                    Bitmap grayImage = ImageProcessing.ConvertToGrayscale(txtFilePath.Text);

                    int width = grayImage.Width;
                    int height = grayImage.Height;
                    int bufferSize = width * height;

                    // Prepare input data and output buffer
                    byte[] inputPixels = ImageProcessing.BitmapToByteArray(grayImage);
                    byte[] outputPixels = new byte[bufferSize];

                    Stopwatch stopWatch = new Stopwatch();

                    // --- Processing ---
                    if (rbAsm.Checked)
                    {
                        unsafe
                        {
                            int numThreads = GetSelectedThreadCount();
                            int innerHeight = height - 2;
                            if (innerHeight <= 0)
                                innerHeight = 0;
                            numThreads = Math.Min(numThreads, Math.Max(innerHeight, 1));

                            if (numThreads == 1)
                            {
                                fixed (byte* inputPtr = inputPixels)
                                fixed (byte* outputPtr = outputPixels)
                                {
                                    stopWatch.Start();
                                    AsmSobelFunction(inputPtr, outputPtr, width, height);
                                    stopWatch.Stop();
                                }
                            }
                            else
                            {
                                stopWatch.Start();
                                fixed (byte* inputPtr = inputPixels)
                                fixed (byte* outputPtr = outputPixels)
                                {
                                    // Store pointer addresses to avoid capturing fixed variables in lambda expressions
                                    long inputPtrAddress = (long)inputPtr;
                                    long outputPtrAddress = (long)outputPtr;

                                    // Divide the image into segments – similar to the C++ version
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

                                        // To apply the filter, we also need the row above and below the segment
                                        int inputStartRow = Math.Max(0, outputStartRow - 1);
                                        int inputEndRow = Math.Min(height - 1, outputEndRow + 1);
                                        int sliceHeight = inputEndRow - inputStartRow + 1;
                                        if (sliceHeight < 3)
                                            return;

                                        // Temporary buffer for processing the segment
                                        byte[] tempOutputSlice = new byte[sliceHeight * width];
                                        fixed (byte* tempOutputSlicePtr = tempOutputSlice)
                                        {
                                            byte* localInputPtr = (byte*)inputPtrAddress;
                                            byte* localOutputPtr = (byte*)outputPtrAddress;
                                            byte* inputSlicePtr = localInputPtr + (inputStartRow * width);

                                            // Process the segment
                                            AsmSobelFunction(inputSlicePtr, tempOutputSlicePtr, width, sliceHeight);

                                            // Copy the processed "valid" rows (skip the first and last rows)
                                            for (int i = 0; i < sliceHeight - 2; i++)
                                            {
                                                int mainRow = outputStartRow + i;
                                                if (mainRow >= height)
                                                    break;
                                                Buffer.MemoryCopy(
                                                    tempOutputSlicePtr + ((i + 1) * width),
                                                    localOutputPtr + (mainRow * width),
                                                    width,
                                                    width
                                                );
                                            }
                                        }
                                    });
                                }
                                stopWatch.Stop();
                            }
                            lblTime.Text = $"{stopWatch.Elapsed.TotalMilliseconds:F2} ms";
                        }
                    }
                    else
                    {
                        // Processing for the C++ library
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

                    // Convert the processed byte array to a bitmap and display the result (on the right)
                    Bitmap edgeImage = ImageProcessing.ByteArrayToBitmap(outputPixels, width, height);
                    ImageProcessing.SaveBitmapToResults(edgeImage, "processed_");
                    pictureBoxResult.Image = edgeImage;

                    MessageBox.Show("Processing complete!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during image processing: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Select a valid file before running the algorithm!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
