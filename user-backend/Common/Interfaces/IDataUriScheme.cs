using Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public interface IDataUriScheme
    {
        bool IsValidImage(string base64string);
        bool IsValidFile(string base64string);
        byte[] GetBytes(string base64string);
        string Getbase64Content(string base64string);
        bool IsValidImageType(string base64string, out ImageType imageType);
        bool IsValidFileType(string base64string, out FileType fileType);
        bool IsOctetStreamIsValidWord(string base64string, out FileType fileType);
        bool IsValidSignatureImageType(string base64string, out SignatureImageType imageType);

    }
}
