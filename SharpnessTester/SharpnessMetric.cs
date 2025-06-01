using MathNet.Numerics.Statistics;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestObjectForm
{
    public class SharpnessMetric
    {

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
