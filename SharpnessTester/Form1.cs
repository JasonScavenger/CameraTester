using System.IO;
using System.Linq;
using System.Windows.Forms;
using OpenCvSharp;
using MathNet.Numerics.Interpolation;
using System.Windows.Forms.DataVisualization.Charting;

namespace TestObjectForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            MessageBox.Show("Выберите папку с изображениями камеры 1");
            var folder1 = SelectFolder();
            if (folder1 == null) return;

            MessageBox.Show("Выберите папку с изображениями камеры 2");
            var folder2 = SelectFolder();
            if (folder2 == null) return;

            ProcessAndPlot(folder1, folder2);
        }

        private string SelectFolder()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
            }
        }

        private void ProcessAndPlot(string path1, string path2)
        {
            chart1.Series.Clear();

            var data1 = LoadDataFromFolder(path1);
            var data2 = LoadDataFromFolder(path2);

            PlotSeries(data1, "Камера 1", Color.Blue);
            PlotSeries(data2, "Камера 2", Color.Red);
        }

        private List<(double X, double Y)> LoadDataFromFolder(string folderPath)
        {
            var files = Directory.GetFiles(folderPath, "*.JPG")
                                 //.Where(f => Path.GetFileNameWithoutExtension(f).All(char.IsDigit))
                                 .ToList();

            var results = new List<(double X, double Y)>();

            //foreach (var file in files)
            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                double x = i;
                //if (double.TryParse(Path.GetFileNameWithoutExtension(file), out double x))
                {
                    Mat image = Cv2.ImRead(file, ImreadModes.Color);
                    if (!image.Empty())
                    {
                        double sharpness = AllMetrics.CalculateSharpness(image);
                        results.Add((x, sharpness));
                        image.Dispose();
                    }
                }
            }

            return results.OrderBy(p => p.X).ToList();
        }



        private void PlotSeries(List<(double X, double Y)> points, string name, Color color)
        {
            points = points.OrderBy(p => p.X).ToList();

            if (points.Count < 2)
            {
                var emptySeries = new Series(name) { ChartType = SeriesChartType.Line };
                chart1.Series.Add(emptySeries);
                return;
            }

            double[] xs = points.Select(p => p.X).ToArray();
            double[] ys = points.Select(p => p.Y).ToArray();

            var (xsInterpolated, ysInterpolated) = Cubic.InterpolateXY(xs, ys, 1000);

            var curveSeries = new Series(name)
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 2,
                Color = color
            };

            for (int i = 0; i < xsInterpolated.Length; i++)
            {
                curveSeries.Points.AddXY(xsInterpolated[i], ysInterpolated[i]);
            }

            chart1.Series.Add(curveSeries);
        }
    }
}
