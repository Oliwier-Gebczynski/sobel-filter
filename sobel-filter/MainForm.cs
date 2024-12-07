﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace sobel_filter
{
    public partial class MainForm : Form
    {
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

            Label lblResult = new Label { Text = "Bitmapa:", Left = 20, Top = 500, AutoSize = true };
            PictureBox pictureBoxResult = new PictureBox { Left = 150, Top = 500, Width = 400, Height = 300, BorderStyle = BorderStyle.Fixed3D };

            Label lblMode = new Label { Text = "Wybierz tryb:", Left = 20, Top = 370, AutoSize = true };
            ComboBox comboBoxMode = new ComboBox { Left = 150, Top = 370, Width = 200 };
            comboBoxMode.Items.AddRange(new string[]
            {
                "Debug",
                "Release optimized for size",
                "Release optimized for speed"
            });
            comboBoxMode.SelectedIndex = 0;

            this.Controls.Add(lblImage);
            this.Controls.Add(txtImagePath);
            this.Controls.Add(btnBrowse);
            this.Controls.Add(pictureBox);
            this.Controls.Add(btnProcess);
            this.Controls.Add(lblExecutionTime);
            this.Controls.Add(lblResult);
            this.Controls.Add(pictureBoxResult);
            this.Controls.Add(lblMode);
            this.Controls.Add(comboBoxMode);

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

            btnProcess.Click += (sender, args) =>
            {
                string imagePath = txtImagePath.Text;

                if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                {
                    MessageBox.Show("Wybierz poprawny plik obrazu.", "Błąd");
                    return;
                }

                try
                {
                    var selectedMode = comboBoxMode.SelectedItem.ToString();
                    var stopwatch = Stopwatch.StartNew();

                    string resultFolder = ImageProcessor.ConvertToBitmap(imagePath);

                    stopwatch.Stop(); 
                    lblExecutionTime.Text = $"Czas wykonania: {stopwatch.ElapsedMilliseconds} ms";

                    string bitmapFilePath = Path.Combine(resultFolder, "bitmap_" + Path.GetFileNameWithoutExtension(imagePath) + ".bmp");
                    if (File.Exists(bitmapFilePath))
                    {
                        pictureBoxResult.Image = new Bitmap(bitmapFilePath);
                    }

                    MessageBox.Show($"Pliki zostały zapisane w folderze: {resultFolder}\nWybrany tryb: {selectedMode}", "Sukces");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Wystąpił błąd: {ex.Message}", "Błąd");
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
