using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Drawing.Imaging;

namespace CMI.Common.Imaging
{
    public class Imager : IImager
    {
        public double ConvertBytesToMegaBytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        public byte[] ResizeImage(byte[] inputImageBytes, int targetMaxSize)
        {
            int outputWidth = targetMaxSize, outputHeight = targetMaxSize;

            using (var inputImage = ConvertBytesToImage(inputImageBytes))
            {
                ImageFormat inputImageFormat = inputImage.RawFormat;

                using (var inputImageBitmap = new Bitmap(inputImage))
                {
                    //derive possible width & height if output image
                    if (inputImageBitmap.Width > inputImageBitmap.Height)
                    {
                        outputWidth = targetMaxSize;
                        outputHeight = Convert.ToInt32(inputImageBitmap.Height * targetMaxSize / (double)inputImageBitmap.Width);
                    }
                    else
                    {
                        outputWidth = Convert.ToInt32(inputImageBitmap.Width * targetMaxSize / (double)inputImageBitmap.Height);
                        outputHeight = targetMaxSize;
                    }

                    //draw new resized image
                    using (var outputImageBitmap = new Bitmap(outputWidth, outputHeight))
                    {
                        using (var outputImageGraphics = Graphics.FromImage(outputImageBitmap))
                        {
                            //set required properties
                            outputImageGraphics.CompositingQuality = CompositingQuality.HighSpeed;
                            outputImageGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            outputImageGraphics.CompositingMode = CompositingMode.SourceCopy;

                            //draw actual imae
                            outputImageGraphics.DrawImage(inputImageBitmap, 0, 0, outputWidth, outputHeight);

                            //return resized image bytes
                            return ConvertImageToBytes(outputImageBitmap, inputImageFormat);
                        }
                    }
                }
            }
        }

        private Image ConvertBytesToImage(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {
                return Image.FromStream(memoryStream);
            }
        }

        private byte[] ConvertImageToBytes(Image image, ImageFormat imageFormat)
        {
            using (var memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, imageFormat);

                return memoryStream.ToArray();
            }
        }
    }
}
