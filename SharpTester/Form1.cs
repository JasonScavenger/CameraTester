using OpenCvSharp;
using System.Numerics;
using System.Windows.Forms;

namespace SharpTester
{
    class BitmapConverter
    {
        // Bitmap -> Mat
        public static Mat ToMat(Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return Cv2.ImDecode(ms.ToArray(), ImreadModes.Color);
            }
        }

        // Mat -> Bitmap
        public static Bitmap ToBitmap(Mat mat)
        {
            byte[] bytes = mat.ToBytes(); // БЕЗОПАСНЫЙ способ получить данные

            try
            {
                using (var ms = new MemoryStream(bytes))
                {
                    return new Bitmap(ms);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Ошибка при создании Bitmap: " + ex.Message);
                return null;
            }
        }
    }

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Bitmap originalImage;
        Bitmap black = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                originalImage = (Bitmap)Bitmap.FromFile(openFileDialog1.FileName);
                pictureBox1.Image = originalImage;
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = await ProcessImage(originalImage);

            var img = BitmapConverter.ToMat(originalImage);

            double sharpness = TestObjectForm.SharpnessMetric.CalculateSharpness(img);

            double variance = TestObjectForm.SharpnessMetric.Variance(img);

            Mat imgGaussed = BitmapConverter.ToMat(originalImage);
            Cv2.GaussianBlur(img, imgGaussed, new OpenCvSharp.Size(5, 5), 1, 1);

            double blur = TestObjectForm.SharpnessMetric.VarianceOfLaplacian(img);

            double psnr = TestObjectForm.SharpnessMetric.CalculatePSNR(img, imgGaussed);

            label1.Text = $"Резкость: {Math.Round(sharpness, 3)}\r\n" +
                $"\r\nДисперсия: {Math.Round(variance, 3)}\r\n" +
                $"\r\nБлюр: {Math.Round(blur, 3)}\r\n" +
                $"\r\nPSNR: {psnr} (30-40 dB norm)";
            if (psnr < 30) label1.Text += "\r\nИзображение слишком шумное ✕!\r\n";
            if (psnr > 40) label1.Text += "\r\nИзображение содержит мало шумов ✓\r\n";
        }

        async Task<Bitmap> ProcessImage(Bitmap imageToProcess)
        {
            AforgeService service = new AforgeService();

            bool isContains = await service.IsContains(imageToProcess, black);

            if (isContains)
            {
                Bitmap copy = new Bitmap(imageToProcess);
                var g = Graphics.FromImage(copy);
                foreach (var place in service._matchings)
                {
                    g.DrawRectangle(new Pen(Color.Red, 2.0f), place.Rectangle);
                }
                return copy;
            }
            else
            {
                return null;
            }
        }
    }
}
