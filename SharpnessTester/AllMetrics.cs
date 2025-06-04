using MathNet.Numerics.Statistics;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using Point = OpenCvSharp.Point;

namespace TestObjectForm
{
    public class BitmapConverter
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
    public class AllMetrics
    {
        // Метод получения точек на линии
        public static List<Point> GetPointsOnLine(Point start, Point end)
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
        public static void AddBrightnessData(Mat src, List<Point> points, Series series)
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

        public static System.Collections.Generic.List<Rect> DetectColorStrips(Mat src, string color)
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

        public static System.Collections.Generic.List<Rect> FindStrips(Mat mask)
        {
            // Морфологические операции для улучшения маски
            using Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(5, 5));
            Cv2.MorphologyEx(mask, mask, MorphTypes.Close, kernel);
            Cv2.MorphologyEx(mask, mask, MorphTypes.Open, kernel);

            // Поиск контуров
            OpenCvSharp.Point[][] contours;
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
        public static double CalculateGradientSum(Series series)
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
        public static (double mean, double stdDev) CalculateMeanAndStdDev(List<double> values)
        {
            double mean = values.Average();
            double variance = values.Average(x => Math.Pow(x - mean, 2));
            double stdDev = Math.Sqrt(variance);
            return (mean, stdDev);
        }

        // Метод для расчета контрастности
        public static (double maxBrightness, double minBrightness, double contrast) CalculateContrast(Series series)
        {
            double maxBrightness = series.Points.Max(p => p.YValues[0]);
            double minBrightness = series.Points.Min(p => p.YValues[0]);
            double contrast = maxBrightness - minBrightness;
            return (maxBrightness, minBrightness, contrast);
        }

        // Метод для анализа равномерности полос
        public static (List<int> blackStripeLengths, List<int> whiteStripeLengths) AnalyzeStripeUniformity(Series series, double threshold)
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
        public static double CalculateCorrelationCoefficient(Series series)
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

        // Расчёт PSNR
        public static double CalculatePSNR(Mat matOriginal, Mat matProcessed)
        {
            double psnr = Cv2.PSNR(matOriginal, matProcessed);
            return psnr;
        }
        public static double Variance(Mat image)
        {
            using (var laplacian = new Mat())
            {
                int kernel_size = 3;
                int scale = 1;
                int delta = 0;
                int ddepth = image.Type().Depth;
                Cv2.Laplacian(image, laplacian, ddepth, kernel_size, scale, delta);
                Cv2.MeanStdDev(laplacian, out var mean, out var stddev);
                return stddev.Val0 * stddev.Val0;
            }
        }

        public static double VarianceOfLaplacian(Mat image)
        {
            using (var laplacian = new Mat())
            {
                Cv2.Laplacian(image, laplacian, MatType.CV_64FC1);
                Cv2.MeanStdDev(laplacian, out var mean, out var stddev);
                return stddev.Val0 * stddev.Val0;
            }
        }

        //public static double CalculateNoise(Mat image)
        //{
        //    if (image.Empty())
        //        throw new ArgumentException("Изображение пустое.");
        //    Mat outimage = null;
        //    Cv2.Laplacian(image, outimage, new MatType(1));

        //}

        // Возвращает численную оценку резкости изображения
        public static double CalculateSharpness(Mat image)
        {
            if (image.Empty())
                throw new ArgumentException("Изображение пустое.");

            // Преобразуем в оттенки серого
            Mat gray = new Mat();
            Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

            // Вычисляем градиент по X (вертикальные границы)
            Mat gradX = new Mat();
            Cv2.Sobel(gray, gradX, MatType.CV_32F, 1, 0);

            // Нормализуем до 0..255
            Mat absGradX = new Mat();
            Cv2.ConvertScaleAbs(gradX, absGradX);

            // Получаем данные как массив byte[]
            byte[] data = new byte[absGradX.Rows * absGradX.Cols];
            unsafe
            {
                byte* ptr = (byte*)absGradX.DataPointer;
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = ptr[i];
                }
            }

            // Конвертируем в double[]
            double[] gradientValues = data.Select(x => (double)x).ToArray();

            // Считаем дисперсию
            double sharpnessScore = Statistics.Variance(gradientValues);
            return sharpnessScore;
        }
    }
}
