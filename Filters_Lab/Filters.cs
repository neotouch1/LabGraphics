using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Filters_Lab
{
    abstract class Filters
    {

        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);


        public virtual Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;

                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }

        public int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

    };



    //Классы фильтров


    // Инверсия
    class InvertFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);

            Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);
            return resultColor;
        }

    };


    // Матричные фильтры

    class MatrixFilters : Filters
    {

        protected float[,] kernel = null;
        protected MatrixFilters() { }
        public MatrixFilters(float[,] kernel)
        {
            this.kernel = kernel;
        }

        //вычислять цвет пикселя на основании своих соседей.
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            float resultR = 0;
            float resultG = 0;
            float resultB = 0;

            for (int l = -radiusY; l <= radiusX; l++)
            {
                for (int k = -radiusX; k <= radiusY; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);

                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];

                }
            }
            return Color.FromArgb(Clamp((int)resultR, 0, 255),
                                    Clamp((int)resultG, 0, 255),
                                    Clamp((int)resultB, 0, 255));
        }

    };

    // Двуматричные фильтры

    class MatrixFilters2 : Filters
    {

        protected float[,] kernel_1 = null;
        protected float[,] kernel_2 = null;
        protected MatrixFilters2() { }
        public MatrixFilters2(float[,] kernels, float[,] kernel)
        {
            this.kernel_1 = kernels;
            this.kernel_2 = kernel;
        }

        //вычислять цвет пикселя на основании своих соседей.
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel_1.GetLength(0) / 2;
            int radiusY = kernel_1.GetLength(1) / 2;

            float resultR = 0;
            float resultG = 0;
            float resultB = 0;

            for (int l = -radiusY; l <= radiusX; l++)
            {
                for (int k = -radiusX; k <= radiusY; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);

                    resultR += neighborColor.R * kernel_1[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel_1[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel_1[k + radiusX, l + radiusY];

                }
            }


            int radiusX2 = kernel_2.GetLength(0) / 2;
            int radiusY2 = kernel_2.GetLength(1) / 2;

            float resultR2 = 0;
            float resultG2 = 0;
            float resultB2 = 0;

            for (int l = -radiusY; l <= radiusX; l++)
            {
                for (int k = -radiusX; k <= radiusY; k++)
                {
                    int idX2 = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY2 = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX2, idY2);

                    resultR2 += neighborColor.R * kernel_1[k + radiusX, l + radiusY];
                    resultG2 += neighborColor.G * kernel_1[k + radiusX, l + radiusY];
                    resultB2 += neighborColor.B * kernel_1[k + radiusX, l + radiusY];

                }
            }
            return Color.FromArgb(
             Clamp((int)Math.Sqrt((resultR * resultR + resultR2 * resultR2)), 0, 255),
             Clamp((int)Math.Sqrt((resultG * resultG + resultG2 * resultG2)), 0, 255),
             Clamp((int)Math.Sqrt((resultB * resultB + resultB2 * resultB2)), 0, 255));
        }

    }



    //Blur размытие

    class BlurFilter : MatrixFilters
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;

            kernel = new float[sizeX, sizeY];

            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
                }
            }
        }
    }


    //Gaus

    class GaussianFilter : MatrixFilters
    {

        public void createGaussianKernel(int radius, float sigma)
        {
            int size = 2 * radius + 1;  //size

            kernel = new float[size, size];

            //коэф

            float norm = 0;

            // рассчитываем ядро линейного фильтра
            for (int i = -radius; i <= radius; i++)
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    kernel[i, j] /= norm;
                }
            }
        }
        public GaussianFilter()
        {
            createGaussianKernel(7, 2);
        }
    }


    // "Это фигня. Вот, классика, адидас, черно-белый!..."
    class GrayScaleFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceIntencity = sourceImage.GetPixel(x, y);

            int intencity = (int)((sourceIntencity.R * 0.36 + sourceIntencity.B * 0.51 + sourceIntencity.G * 0.11));

            Color resultIntencity = Color.FromArgb(intencity, intencity, intencity);
            return resultIntencity;
        }
    }

    // Сепия
    class Sepia : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceIntencity = sourceImage.GetPixel(x, y);
            float k = 50f;

            int intencity = (int)((sourceIntencity.R * 0.36 + sourceIntencity.B * 0.51 + sourceIntencity.G * 0.11));

            int R_ = Clamp((int)((intencity) + (2 * k)), 0, 255);
            int G_ = Clamp((int)((intencity) + (0.5 * k)), 0, 255);
            int B_ = Clamp((int)((intencity) - (1 * k)), 0, 255);

            Color resultIntencity = Color.FromArgb(R_, G_, B_);
            return resultIntencity;
        }
    }

    // Яркость

    class Brightness : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceBrightness = sourceImage.GetPixel(x, y);

            int k = 20;  // Коэффициент яркости

            int R_ = Clamp(sourceBrightness.R + k, 0, 255);
            int G_ = Clamp(sourceBrightness.G + k, 0, 255);
            int B_ = Clamp(sourceBrightness.B + k, 0, 255);

            Color resultIntencity = Color.FromArgb(R_, G_, B_);
            return resultIntencity;
        }

    }


    //Собель
    class SobelFilter : MatrixFilters2
    {
        public SobelFilter()
        {
            kernel_1 = new float[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };

            kernel_2 = new float[,] { { -1, -2, -1 }, { -0, 0, 0 }, { 1, 2, 1 } };
        }
    }

    class Sharpness : MatrixFilters
    {
        public Sharpness()
        {
            kernel = new float[,] { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };
        }
    }



    // Медианный фильтр с ядром в 4х4
    class MedianFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            List<int> AllR = new List<int>();
            List<int> AllG = new List<int>();
            List<int> AllB = new List<int>();

            int radiusX = 3;
            int radiusY = 3;

            for (int k = -radiusX; k <= radiusX; k++)
            {
                for (int l = -radiusY; l <= radiusY; l++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);

                    Color color = sourceImage.GetPixel(idX, idY);

                    AllR.Add(color.R);
                    AllG.Add(color.G);
                    AllB.Add(color.B);
                }
            }

            AllR.Sort();
            AllG.Sort();
            AllB.Sort();

            return Color.FromArgb(AllR[AllR.Count() / 2], AllG[AllG.Count() / 2], AllB[AllB.Count() / 2]);
           
        }
    }
}
