using OpenCvSharp;
using System.Numerics;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Forms.DataVisualization.Charting;
using TestObjectForm;
using Point = OpenCvSharp.Point;
using static TestObjectForm.AllMetrics;

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

            var img = TestObjectForm.BitmapConverter.ToMat(originalImage);

            double sharpness = TestObjectForm.AllMetrics.CalculateSharpness(img);

            double variance = TestObjectForm.AllMetrics.Variance(img);

            Mat imgGaussed = TestObjectForm.BitmapConverter.ToMat(originalImage);
            Cv2.GaussianBlur(img, imgGaussed, new OpenCvSharp.Size(5, 5), 1, 1);

            double blur = TestObjectForm.AllMetrics.VarianceOfLaplacian(img);

            double psnr = TestObjectForm.AllMetrics.CalculatePSNR(img, imgGaussed);


            textBox1.Text = $"Резкость: {Math.Round(sharpness, 3)}\r\n" +
                $"\r\nДисперсия: {Math.Round(variance, 3)}\r\n" +
                $"\r\nБлюр: {Math.Round(blur, 3)}\r\n" +
                $"\r\nPSNR: {Math.Round(psnr, 3)} (30-40 dB norm)";
            if (psnr < 30)
            {
                textBox1.Text += "\r\nИзображение слишком шумное ✕!\r\n";
            }
            if (psnr > 40)
            {
                textBox1.Text += "\r\nИзображение содержит мало шумов ✓\r\n";
            }

            Mat src = BitmapConverter.ToMat(originalImage);
            Cv2.CvtColor(src, src, ColorConversionCodes.BGR2BGRA); // BGRA для работы с альфой

            // Поиск полос
            var redRects = DetectColorStrips(src, "red");
            var greenRects = DetectColorStrips(src, "green");

            if (redRects.Count == 0 || greenRects.Count == 0)
            {
                MessageBox.Show("Не найдены полосы нужного цвета.");
                return;
            }

            // Центры
            Point center1 = new Point(redRects[0].X + redRects[0].Width / 2, redRects[0].Y + redRects[0].Height / 2);
            Point center2 = new Point(greenRects[0].X + greenRects[0].Width / 2, greenRects[0].Y + greenRects[0].Height / 2);

            // Верхняя и нижняя линии
            Point center1_1 = new Point(redRects[0].X + redRects[0].Width / 2, redRects[0].Y + redRects[0].Height / 2 - redRects[0].Height / 4);
            Point center2_1 = new Point(greenRects[0].X + greenRects[0].Width / 2, greenRects[0].Y + greenRects[0].Height / 2 - greenRects[0].Height / 4);

            Point center1_2 = new Point(redRects[0].X + redRects[0].Width / 2, redRects[0].Y + redRects[0].Height / 2 + redRects[0].Height / 4);
            Point center2_2 = new Point(greenRects[0].X + greenRects[0].Width / 2, greenRects[0].Y + greenRects[0].Height / 2 + greenRects[0].Height / 4);

            // Рисуем линии
            Cv2.Line(src, center1, center2, Scalar.Yellow, 5);
            Cv2.Line(src, center1_1, center2_1, Scalar.Yellow, 5);
            Cv2.Line(src, center1_2, center2_2, Scalar.Yellow, 5);

            // Конвертируем обратно в Bitmap
            Bitmap resultBitmap = BitmapConverter.ToBitmap(src);
            pictureBox1.Image = resultBitmap;

            // Анализ яркости по линиям
            chart1.Series.Clear();

            var seriesTop = new Series("Верхняя линия")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 2,
                Color = Color.Red
            };

            var seriesBottom = new Series("Нижняя линия")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 2,
                Color = Color.Blue
            };

            // Получаем точки на линиях
            List<Point> topLinePoints = GetPointsOnLine(center1_1, center2_1);
            List<Point> bottomLinePoints = GetPointsOnLine(center1_2, center2_2);

            using Mat src_ = BitmapConverter.ToMat(originalImage);
            Cv2.CvtColor(src_, src_, ColorConversionCodes.BGR2BGRA); // BGRA для работы с альфой

            // Собираем яркость
            AddBrightnessData(src_, topLinePoints, seriesTop);
            AddBrightnessData(src_, bottomLinePoints, seriesBottom);

            chart1.Series.Add(seriesTop);
            chart1.Series.Add(seriesBottom);

            // Настройки графика
            chart1.ChartAreas[0].AxisX.Title = "Позиция";
            chart1.ChartAreas[0].AxisY.Title = "Яркость";
            chart1.ChartAreas[0].AxisY.Minimum = 0;
            chart1.ChartAreas[0].AxisY.Maximum = 255;

            // Легенда
            chart1.Legends.Clear();
            chart1.Legends.Add(new Legend());

            // Вычисляем метрики
            double gradientSumTop = CalculateGradientSum(seriesTop);
            double gradientSumBottom = CalculateGradientSum(seriesBottom);

            List<double> brightnessValuesTop = seriesTop.Points.Select(p => p.YValues[0]).ToList();
            List<double> brightnessValuesBottom = seriesBottom.Points.Select(p => p.YValues[0]).ToList();

            var (meanTop, stdDevTop) = CalculateMeanAndStdDev(brightnessValuesTop);
            var (meanBottom, stdDevBottom) = CalculateMeanAndStdDev(brightnessValuesBottom);

            var (maxBrightnessTop, minBrightnessTop, contrastTop) = CalculateContrast(seriesTop);
            var (maxBrightnessBottom, minBrightnessBottom, contrastBottom) = CalculateContrast(seriesBottom);

            var (blackStripesTop, whiteStripesTop) = AnalyzeStripeUniformity(seriesTop, 100);
            var (blackStripesBottom, whiteStripesBottom) = AnalyzeStripeUniformity(seriesBottom, 100);

            double correlationCoefficientTop = CalculateCorrelationCoefficient(seriesTop);
            double correlationCoefficientBottom = CalculateCorrelationCoefficient(seriesBottom);

            // Выводим результаты в TextBox
            textBox1.Text += $"\r\n\r\nРезультаты анализа верхней линии:\r\n" +
                                 $"- Сумма градиента: {gradientSumTop}\r\n" +
                                 $"- Среднее значение яркости: {meanTop}, Стандартное отклонение: {stdDevTop}\r\n" +
                                 $"- Максимальная яркость: {maxBrightnessTop}, Минимальная яркость: {minBrightnessTop}, Контрастность: {contrastTop}\r\n" +
                                 $"- Длины черных полос: {string.Join(", ", blackStripesTop)}\r\n" +
                                 $"- Длины белых полос: {string.Join(", ", whiteStripesTop)}\r\n" +
                                 $"- Коэффициент корреляции: {correlationCoefficientTop}\r\n\r\n" +

                                 $"Результаты анализа нижней линии:\r\n" +
                                 $"- Сумма градиента: {gradientSumBottom}\r\n" +
                                 $"- Среднее значение яркости: {meanBottom}, Стандартное отклонение: {stdDevBottom}\r\n" +
                                 $"- Максимальная яркость: {maxBrightnessBottom}, Минимальная яркость: {minBrightnessBottom}, Контрастность: {contrastBottom}\r\n" +
                                 $"- Длины черных полос: {string.Join(", ", blackStripesBottom)}\r\n" +
                                 $"- Длины белых полос: {string.Join(", ", whiteStripesBottom)}\r\n" +
                                 $"- Коэффициент корреляции: {correlationCoefficientBottom}";

            double psnrKoeff = psnr / 40.0;
            double sharpnessKoeff = sharpness / 900.0;
            double contrast = (contrastTop + contrastBottom) / 2.0;
            textBox1.Text += $"\r\nСредняя контрастность: {Math.Round(contrast, 3)}";


            double finalKoeff = psnrKoeff * sharpnessKoeff;
            textBox1.Text += $"\r\nКоэффициент крутости: {Math.Round(finalKoeff, 3)}";

            var focusWB = new Series("Белая снизу")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 2,
                Color = Color.Blue
            };
            var focusBB = new Series("Черная снизу")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 2,
                Color = Color.Red
            };
            double x = 0;
            double sum = whiteStripesBottom.Sum();
            double max = whiteStripesBottom.Max();
            for (int i = 0; i < whiteStripesBottom.Count; i++)
            {
                focusWB.Points.AddXY(x / sum * 1000, max - whiteStripesBottom[i]);
                x += whiteStripesBottom[i];
            }
            x = 0;
            sum = blackStripesBottom.Sum();
            max = blackStripesBottom.Max();
            for (int i = 0; i < blackStripesBottom.Count; i++)
            {
                focusBB.Points.AddXY(x / sum * 1000, max - blackStripesBottom[i]);
                x += blackStripesBottom[i];
            }

            // Анализ яркости по линиям
            chart2.Series.Clear();
            chart2.Series.Add(focusWB);
            chart2.Series.Add(focusBB);

            // Настройки графика
            chart2.ChartAreas[0].AxisX.Title = "Позиция";
            chart2.ChartAreas[0].AxisY.Title = "Фокус";
            chart2.ChartAreas[0].AxisY.Minimum = 0;
            chart2.ChartAreas[0].AxisY.Maximum = max;

            // Легенда
            chart2.Legends.Clear();
            chart2.Legends.Add(new Legend());
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
