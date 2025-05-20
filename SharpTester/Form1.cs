namespace SharpTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Bitmap originalImage;

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                originalImage = (Bitmap)Bitmap.FromFile(openFileDialog1.FileName);
                pictureBox1.Image = originalImage;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = ProcessImage(originalImage);
            label1.Text = "";
        }

        Bitmap ProcessImage(Bitmap imageToProcess)
        {

            return null;
        }
    }
}
