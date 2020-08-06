using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

/// <summary>
/// Rotates Files if necessary
/// </summary>
public static class ImageRotator
{
    public static RotateFlipType RotateImageByExifOrientationData(string sourceFilePath, string targetFilePath, ImageFormat targetFormat, bool updateExifData = true)
    {
        // Rotate the image according to EXIF data
        var bmp = new Bitmap(sourceFilePath);
        RotateFlipType fType = RotateImageByExifOrientationData(bmp, updateExifData);
        if (fType != RotateFlipType.RotateNoneFlipNone)
        {
            bmp.Save(targetFilePath, targetFormat);
        }
        return fType;
    }

    public static RotateFlipType RotateImageByExifOrientationData(Image img, bool updateExifData = true)
    {
        int orientationId = 0x0112;
        var fType = RotateFlipType.RotateNoneFlipNone;
        if (img.PropertyIdList.Contains(orientationId))
        {
            var pItem = img.GetPropertyItem(orientationId);
            fType = GetRotateFlipTypeByExifOrientationData(pItem.Value[0]);
            if (fType != RotateFlipType.RotateNoneFlipNone)
            {
                img.RotateFlip(fType);
                // Remove Exif orientation tag (if requested)
                if (updateExifData) img.RemovePropertyItem(orientationId);
            }
        }
        return fType;
    }

    public static RotateFlipType GetRotateFlipTypeByExifOrientationData(int orientation)
    {
        switch (orientation)
        {
            case 1:
            default:
                return RotateFlipType.RotateNoneFlipNone;
            case 2:
                return RotateFlipType.RotateNoneFlipX;
            case 3:
                return RotateFlipType.Rotate180FlipNone;
            case 4:
                return RotateFlipType.Rotate180FlipX;
            case 5:
                return RotateFlipType.Rotate90FlipX;
            case 6:
                return RotateFlipType.Rotate90FlipNone;
            case 7:
                return RotateFlipType.Rotate270FlipX;
            case 8:
                return RotateFlipType.Rotate270FlipNone;
        }
    }
}
