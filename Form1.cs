using OpenCvSharp;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string imagePath = "";

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Фильтр для изображений
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                openFileDialog.Title = "Выберите изображение";

                // Показываем диалог и проверяем, нажата ли кнопка OK
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    imagePath = openFileDialog.FileName;     
                }
            }

            // Загрузка изображения
            Bitmap original = new Bitmap(imagePath);

            using Mat src = BitmapConverter.ToMat(original);
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

            using Mat src_ = BitmapConverter.ToMat(original);
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
            chart1.ChartAreas[0].AxisY.Maximum = 1000;

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
            textBox1.Text = $"Результаты анализа верхней линии:\n" +
                                 $"- Сумма градиента: {gradientSumTop}\n" +
                                 $"- Среднее значение яркости: {meanTop}, Стандартное отклонение: {stdDevTop}\n" +
                                 $"- Максимальная яркость: {maxBrightnessTop}, Минимальная яркость: {minBrightnessTop}, Контрастность: {contrastTop}\n" +
                                 $"- Длины черных полос: {string.Join(", ", blackStripesTop)}\n" +
                                 $"- Длины белых полос: {string.Join(", ", whiteStripesTop)}\n" +
                                 $"- Коэффициент корреляции: {correlationCoefficientTop}\n\n" +

                                 $"Результаты анализа нижней линии:\n" +
                                 $"- Сумма градиента: {gradientSumBottom}\n" +
                                 $"- Среднее значение яркости: {meanBottom}, Стандартное отклонение: {stdDevBottom}\n" +
                                 $"- Максимальная яркость: {maxBrightnessBottom}, Минимальная яркость: {minBrightnessBottom}, Контрастность: {contrastBottom}\n" +
                                 $"- Длины черных полос: {string.Join(", ", blackStripesBottom)}\n" +
                                 $"- Длины белых полос: {string.Join(", ", whiteStripesBottom)}\n" +
                                 $"- Коэффициент корреляции: {correlationCoefficientBottom}";

            ;
        }

        // Метод получения точек на линии
        private List<Point> GetPointsOnLine(Point start, Point end)
        {
            List<Point> points = new List<Point>();
            int steps = (int)Math.Max(Math.Abs(end.X - start.X), Math.Abs(end.Y - start.Y));
            for (int i = 0; i <= steps; i++)
            {
                double t = i / (double)steps;
                int x = (int)(start.X + (end.X - start.X) * t);
                int y = (int)(start.Y + (end.Y - start.Y) * t);
                points.Add(new Point(x, y));
            }
            return points;
        }

        // Метод сбора яркости
        private void AddBrightnessData(Mat src, List<Point> points, Series series)
        {
            for (int i = 0; i < points.Count; i++)
            {
                Point p = points[i];
                if (p.X >= 0 && p.Y >= 0 && p.X < src.Cols && p.Y < src.Rows)
                {
                    Vec4b pixel = src.At<Vec4b>(p.Y, p.X);
                    int brightness = (pixel.Item0 + pixel.Item1 + pixel.Item2) / 3;
                    series.Points.AddXY(i, brightness);
                }
            }
        }

        static System.Collections.Generic.List<Rect> DetectColorStrips(Mat src, string color)
        {
            // Копируем исходное изображение
            using Mat hsv = new Mat();
            Cv2.CvtColor(src, hsv, ColorConversionCodes.BGR2HSV); // BGRA2HSV

            // Диапазоны цветов HSV
            Scalar lowerBound, upperBound;

            switch (color.ToLower())
            {
                case "red":
                    // Red может быть в двух диапазонах в HSV
                    var lower1 = new Scalar(0, 100, 100);
                    var upper1 = new Scalar(10, 255, 255);
                    var lower2 = new Scalar(160, 100, 100);
                    var upper2 = new Scalar(180, 255, 255);

                    Mat mask1 = new Mat();
                    Mat mask2 = new Mat();
                    Cv2.InRange(hsv, lower1, upper1, mask1);
                    Cv2.InRange(hsv, lower2, upper2, mask2);

                    Mat redMask = new Mat();
                    Cv2.BitwiseOr(mask1, mask2, redMask); // ← Исправленная строка
                    return FindStrips(redMask);

                case "green":
                    lowerBound = new Scalar(40, 40, 40);
                    upperBound = new Scalar(80, 255, 255);
                    break;

                default:
                    throw new ArgumentException("Поддерживаемые цвета: red, green");
            }

            using Mat mask = new Mat();
            Cv2.InRange(hsv, lowerBound, upperBound, mask);
            return FindStrips(mask);
        }

        static System.Collections.Generic.List<Rect> FindStrips(Mat mask)
        {
            // Морфологические операции для улучшения маски
            using Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(5, 5));
            Cv2.MorphologyEx(mask, mask, MorphTypes.Close, kernel);
            Cv2.MorphologyEx(mask, mask, MorphTypes.Open, kernel);

            // Поиск контуров
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(mask, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            var rectangles = new System.Collections.Generic.List<Rect>();

            foreach (var contour in contours)
            {
                Rect rect = Cv2.BoundingRect(contour);
                double area = Cv2.ContourArea(contour);

                // Фильтруем маленькие области
                if (area < 1000) continue;

                // Определяем соотношение сторон
                double width = rect.Width;
                double height = rect.Height;

                double ratio = Math.Max(width / height, height / width); // Чтобы не зависеть от ориентации

                // Проверяем, что соотношение приблизительно 1:10
                if (ratio >= 9.0 && ratio <= 11.0)
                {
                    rectangles.Add(rect);
                }
            }

            return rectangles;
        }

        // Метод для расчета суммы градиента (качество фокусировки)
        private double CalculateGradientSum(Series series)
        {
            double gradientSum = 0;
            for (int i = 0; i < series.Points.Count - 1; i++)
            {
                double x1 = series.Points[i].XValue;
                double y1 = series.Points[i].YValues[0];
                double x2 = series.Points[i + 1].XValue;
                double y2 = series.Points[i + 1].YValues[0];
                double gradient = Math.Abs(y2 - y1);
                gradientSum += gradient;
            }
            return gradientSum;
        }

        // Метод для расчета стандартного отклонения (шум)
        private (double mean, double stdDev) CalculateMeanAndStdDev(List<double> values)
        {
            double mean = values.Average();
            double variance = values.Average(x => Math.Pow(x - mean, 2));
            double stdDev = Math.Sqrt(variance);
            return (mean, stdDev);
        }

        // Метод для расчета контрастности
        private (double maxBrightness, double minBrightness, double contrast) CalculateContrast(Series series)
        {
            double maxBrightness = series.Points.Max(p => p.YValues[0]);
            double minBrightness = series.Points.Min(p => p.YValues[0]);
            double contrast = maxBrightness - minBrightness;
            return (maxBrightness, minBrightness, contrast);
        }

        // Метод для анализа равномерности полос
        private (List<int> blackStripeLengths, List<int> whiteStripeLengths) AnalyzeStripeUniformity(Series series, double threshold)
        {
            List<int> blackStripeLengths = new List<int>();
            List<int> whiteStripeLengths = new List<int>();
            int currentStripeLength = 0;
            bool isBlackStripe = true;

            foreach (DataPoint point in series.Points)
            {
                if (point.YValues[0] < threshold) // Черная полоса
                {
                    if (!isBlackStripe)
                    {
                        if (currentStripeLength > 0)
                        {
                            whiteStripeLengths.Add(currentStripeLength);
                        }
                        currentStripeLength = 0;
                        isBlackStripe = true;
                    }
                    currentStripeLength++;
                }
                else // Белая полоса
                {
                    if (isBlackStripe)
                    {
                        if (currentStripeLength > 0)
                        {
                            blackStripeLengths.Add(currentStripeLength);
                        }
                        currentStripeLength = 0;
                        isBlackStripe = false;
                    }
                    currentStripeLength++;
                }
            }

            if (currentStripeLength > 0)
            {
                if (isBlackStripe)
                {
                    blackStripeLengths.Add(currentStripeLength);
                }
                else
                {
                    whiteStripeLengths.Add(currentStripeLength);
                }
            }

            return (blackStripeLengths, whiteStripeLengths);
        }

        // Метод для расчета коэффициента корреляции (линейность)
        private double CalculateCorrelationCoefficient(Series series)
        {
            double sumXY = 0, sumX = 0, sumY = 0, sumX2 = 0, sumY2 = 0;

            foreach (DataPoint point in series.Points)
            {
                double x = point.XValue;
                double y = point.YValues[0];
                sumXY += x * y;
                sumX += x;
                sumY += y;
                sumX2 += x * x;
                sumY2 += y * y;
            }

            int n = series.Points.Count;
            double numerator = n * sumXY - sumX * sumY;
            double denominator = Math.Sqrt((n * sumX2 - sumX * sumX) * (n * sumY2 - sumY * sumY));
            double correlationCoefficient = numerator / denominator;

            return correlationCoefficient;
        }
    }

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
                Console.WriteLine("Ошибка при создании Bitmap: " + ex.Message);
                return null;
            }
        }
    }
}
