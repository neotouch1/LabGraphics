using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Filters_Lab;
using System.ComponentModel;

namespace Filters_Lab
{
    class MatMorphology : Filters
    {
        protected static float[,] mask = new float[,] {
                { 1, 1, 1, 1, 1 },
                { 1, 1, 1, 1, 1 },
                { 1, 1, 1, 1, 1 },
                { 1, 1, 1, 1, 1 },
                { 1, 1, 1, 1, 1 }};


        protected int radiusX = mask.GetLength(0) / 2;
        protected int radiusY = mask.GetLength(1) / 2;
   


        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            return Color.FromArgb(0, 0, 0);
        }
    }

    //Расширение
    class DilationFilter : MatMorphology
    {
        
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int maxR = 0;
            int maxG = 0;
            int maxB = 0;

            for (int k = -radiusX; k <= radiusX; k++)
            {
                for (int l = -radiusY; l <= radiusY; l++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);

                    if (mask[k + radiusX, l + radiusY] == 1)
                    {
                        Color color = sourceImage.GetPixel(idX, idY);
                        maxR = Math.Max(maxR, color.R);
                        maxG = Math.Max(maxG, color.G);
                        maxB = Math.Max(maxB, color.B);
                    }
                }
            }
            return Color.FromArgb(maxR, maxG, maxB);
        }
    }


class ErosionFilter : MatMorphology
{

    protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
    {
        int minR = 255;
        int minG = 255;
        int minB = 255;

        for (int k = -radiusX; k <= radiusX; k++)
        {
            for (int l = -radiusY; l <= radiusY; l++)
            {
                int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                int idY = Clamp(y + l, 0, sourceImage.Height - 1);

                if (mask[k + radiusX, l + radiusY] == 1)
                {
                    Color color = sourceImage.GetPixel(idX, idY);
                    minR = Math.Min(minR, color.R);
                    minG = Math.Min(minG, color.G);
                    minB = Math.Min(minB, color.B);
                }
            }
        }
        return Color.FromArgb(minR, minG, minB);
     }
    }

// открытие  "подавляет "белый" шум на объектах"
    class OpeningFilter : MatMorphology
    {
    public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Filters first = new ErosionFilter();
            Filters second = new DilationFilter();

            return first.processImage(second.processImage(sourceImage, worker), worker);
        }
    }


    class ClosingFilter : MatMorphology
    {
    public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
        Filters first = new DilationFilter();
        Filters second = new ErosionFilter();

        return first.processImage(second.processImage(sourceImage, worker), worker);
        }
    }


    class GradFilter : MatMorphology
    {
        protected Bitmap ErosionImage;
        protected Bitmap DilationImage;

        public override Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            Filters Erosion = new ErosionFilter();
            Filters Dilation = new DilationFilter();

            ErosionImage = Erosion.processImage(sourceImage, worker);
            DilationImage = Dilation.processImage(sourceImage, worker);

            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));

                if (worker.CancellationPending) { return null; }

                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color colorErosion = ErosionImage.GetPixel(x, y);
            Color colorDilation = DilationImage.GetPixel(x, y);

            return Color.FromArgb(
                        Clamp(colorDilation.R - colorErosion.R, 0, 255),
                        Clamp(colorDilation.G - colorErosion.G, 0, 255),
                        Clamp(colorDilation.B - colorErosion.B, 0, 255));
        }
    }

}