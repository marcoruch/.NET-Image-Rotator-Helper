using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

/// <summary>
/// Summary description for ImageUploadHelper
/// </summary>
public static class ImageUploadHelper
{

public static byte[] LoadUploadedFile(this HttpPostedFile postedFile)
    {
        using (BinaryReader binaryReader = new BinaryReader(postedFile.InputStream))
        {
            try
            {
                return binaryReader.ReadBytes(postedFile.ContentLength);
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                binaryReader.Dispose();
            }
        }
    }

    public static byte[] ImageToByteArray(this System.Drawing.Image image)
    {
        using (var ms = new MemoryStream())
        {
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }
    }

    /// <summary>
    /// Try Load fileName 
    /// </summary>
    /// <param name="postedFile"></param>
    /// <returns>Filename</returns>
    public static string LoadFileName(this HttpPostedFile postedFile)
    {
        return postedFile?.FileName;
    }

    /// <summary>
    /// Try Load Content Type
    /// </summary>
    /// <param name="postedFile"></param>
    /// <returns>Content Type</returns>
    public static string LoadFileType(this HttpPostedFile postedFile)
    {
        return postedFile?.ContentType;
    }

    /// <summary>
    /// Try get the Image Object from the FileContent
    /// </summary>
    /// <param name="byteArrayIn"></param>
    /// <returns>Image Object or null</returns>
    public static Image StreamToImage(this byte[] byteArrayIn)
    {
        MemoryStream buf = null;
        try
        {
            using (buf = new MemoryStream(byteArrayIn))
            {
                return Image.FromStream(buf, true);
            }

        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            buf?.Dispose();
        }
    }

    /// <summary>
    /// Validate if PostedFile is an Image and Provide Content
    /// </summary>
    /// <param name="postedFile"></param>
    /// <param name="fileData"></param>
    /// <param name="fileName"></param>
    /// <param name="fileType"></param>
    /// <param name="fileImage"></param>
    /// <returns>Posted File Information</returns>
    public static bool IsImage(this HttpPostedFile postedFile, out byte[] fileData, out string fileName, out string fileType, out Image fileImage, out string exception)
    {
        // outs are defined at validation
        fileImage = null;
        fileData = null;
        fileName = null;
        fileType = null;

        //-------------------------------------------
        //  Check the image mime types
        //-------------------------------------------
        if (!string.Equals(postedFile.ContentType, "image/jpg", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(postedFile.ContentType, "image/jpeg", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(postedFile.ContentType, "image/png", StringComparison.OrdinalIgnoreCase))
        {
            exception = "Ungültiger Dateityp";
            return false;
        }

        //-------------------------------------------
        //  Check the image extension
        //-------------------------------------------
        var postedFileExtension = Path.GetExtension(postedFile.FileName);
        if (!string.Equals(postedFileExtension, ".jpg", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(postedFileExtension, ".png", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(postedFileExtension, ".jpeg", StringComparison.OrdinalIgnoreCase))
        {
            exception = "Ungültiger Dateityp";
            return false;
        }
        else
        {
            fileName = postedFile.FileName.Replace(postedFileExtension, "");
            fileType = postedFileExtension;
        }

        //-------------------------------------------
        //  Attempt to read the file and check the first bytes
        //-------------------------------------------
        try
        {
            if (!postedFile.InputStream.CanRead)
            {
                exception = "Datei konnte nicht gelesen werden";
                return false;
            }
            //------------------------------------------
            //   Check whether the image size exceeding the limit or not
            //------------------------------------------ 
            if (postedFile.ContentLength < 512)
            {
                exception = "Datei zu klein, ungültige Datei?";
                return false;
            }

            fileData = postedFile.LoadUploadedFile();
            string content = System.Text.Encoding.UTF8.GetString(fileData);
            if (Regex.IsMatch(content, @"<script|<html|<head|<title|<body|<pre|<table|<a\s+href|<img|<plaintext|<cross\-domain\-policy",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline))
            {
                exception = "Ungültige Datei.";
                return false;
            }

        }
        catch (Exception)
        {
            exception = "Unbehandelter Fehler";
            return false;
        }

        //-------------------------------------------
        //  Try to instantiate new Bitmap, if .NET will throw exception
        //  we can assume that it's not a valid image
        //-------------------------------------------

        try
        {
            fileImage = fileData.StreamToImage();
            var temp = RotateImageByExifOrientationData(fileImage, true);
            
            // We don't need to reassign this when the Image wasn't being rotated anyways.
            if (RotateFlipType.RotateNoneFlipNone != temp)
            {
                fileData = ImageToByteArray(fileImage);
            }
        }
        catch (Exception e1)
        {

            exception = "Datei konnte nicht gelesen werden";
            return false;
        }
        finally
        {
            postedFile.InputStream.Position = 0;
        }

        exception = null;
        return true;
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
