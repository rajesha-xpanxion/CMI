
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace CMI.Common.Imaging
{
    public class Imager : IImager
    {
        public double ConvertBytesToMegaBytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        public byte[] ResizeImage(byte[] inputBytes, int targetMaxSize)
        {
            int outputWidth = targetMaxSize, outputHeight = targetMaxSize;

            using (var inputImage = new Bitmap(ConvertBytesToImage(inputBytes)))
            {
                //derive possible width & height if output image
                if (inputImage.Width > inputImage.Height)
                {
                    outputWidth = targetMaxSize;
                    outputHeight = Convert.ToInt32(inputImage.Height * targetMaxSize / (double)inputImage.Width);
                }
                else
                {
                    outputWidth = Convert.ToInt32(inputImage.Width * targetMaxSize / (double)inputImage.Height);
                    outputHeight = targetMaxSize;
                }

                //draw new resized image
                using (var outputImage = new Bitmap(outputWidth, outputHeight))
                {
                    using (var outputImageGraphics = Graphics.FromImage(outputImage))
                    {
                        //set required properties
                        outputImageGraphics.CompositingQuality = CompositingQuality.HighSpeed;
                        outputImageGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        outputImageGraphics.CompositingMode = CompositingMode.SourceCopy;

                        //draw actual imae
                        outputImageGraphics.DrawImage(inputImage, 0, 0, outputWidth, outputHeight);

                        //return resized image bytes
                        return ConvertImageToBytes(outputImage);
                    }
                }
            }
        }

        private Image ConvertBytesToImage(byte[] inputBytes)
        {
            using (var ms = new MemoryStream(inputBytes))
            {
                return Image.FromStream(ms);
            }
        }

        private byte[] ConvertImageToBytes(Image inputImage)
        {
            using (var ms = new MemoryStream())
            {
                inputImage.Save(ms, inputImage.RawFormat);

                return ms.ToArray();
            }
        }
    }
}
