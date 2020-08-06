# .NET-Image-Rotator-Helper
Sometimes Files rotated on Windows (not sure of the version differences yet) are not correctly displayed when uploading them to an Application, this Code allows to check the images metadata and Rotate the File accordingly


This repository also includes an Extension-Helper Class with ASP.NET Image-Upload helper, which validates the uploaded File (to be an Image) and also uses the same principles as the Rotator-Helper to directly Rotate the Files accordingly.
To use this Extension call "IsImage" with the HttpPostedFile-Object, it will out metadata or the exception if thrown.
