using System.Numerics;

namespace SharpTester
{
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
            label1.Text = "";
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
