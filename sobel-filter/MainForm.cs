using System;
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

            Label lblMode = new Label { Text = "Wybierz tryb:", Left = 20, Top = 380, AutoSize = true };
            ComboBox cmbMode = new ComboBox
            {
                Left = 150,
                Top = 380,
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbMode.Items.AddRange(new string[]
            {
        "C++ Debug",
        "C++ Optimized for Size",
        "C++ Optimized for Speed",
        "Asembler"
            });

            Label lblThreshold = new Label { Text = "Podaj wartość T:", Left = 20, Top = 420, AutoSize = true };
            TextBox txtThreshold = new TextBox { Left = 150, Top = 420, Width = 100 };

            Button btnProcess = new Button { Text = "Rozpocznij przetwarzanie", Left = 20, Top = 460 };

            Label lblResult = new Label { Text = "Wynik:", Left = 20, Top = 500, AutoSize = true };
            PictureBox pictureBoxResult = new PictureBox { Left = 150, Top = 500, Width = 400, Height = 300, BorderStyle = BorderStyle.Fixed3D };

            this.Controls.Add(lblImage);
            this.Controls.Add(txtImagePath);
            this.Controls.Add(btnBrowse);
            this.Controls.Add(pictureBox);
            this.Controls.Add(lblMode);
            this.Controls.Add(cmbMode);
            this.Controls.Add(lblThreshold);
            this.Controls.Add(txtThreshold);
            this.Controls.Add(btnProcess);
            this.Controls.Add(lblResult);
            this.Controls.Add(pictureBoxResult);

            btnBrowse.Click += (sender, args) => // for tests
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Bitmap Files|*.bmp";
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        txtImagePath.Text = openFileDialog.FileName;
                        pictureBox.ImageLocation = openFileDialog.FileName;
                    }
                }
            };

            btnProcess.Click += (sender, args) => // for tests
            {
                try
                {
                    if (!int.TryParse(txtThreshold.Text, out int threshold))
                    {
                        MessageBox.Show("Podaj poprawną wartość liczbową dla T.", "Błąd");
                        return;
                    }

                    MessageBox.Show($"Rozpoczęto przetwarzanie z wartością T = {threshold}!", "Przetwarzanie");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Wystąpił błąd: {ex.Message}", "Błąd");
                }
            };
        }


        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }
}
