
namespace CMI.Common.Imaging
{
    public interface IImager
    {
        double ConvertBytesToMegaBytes(long bytes);

        byte[] ResizeImage(byte[] inputBytes, int targetMaxSize);
    }
}
