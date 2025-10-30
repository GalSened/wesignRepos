using Common.Enums;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using System;
using System.CodeDom;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Common.Handlers
{
    public class DataUriSchemeHandler : IDataUriScheme
    {
        private const string IMAGE_DATA_TYPE = "image";
        private const string APPLICATION_DATA_TYPE = "application";
        private const string TEXT_DATA_TYPE = "text";

        public byte[] GetBytes(string base64string)
        {
            string content = Getbase64Content(base64string);

            return Convert.FromBase64String(content);
        }

        public string Getbase64Content(string base64string)
        {
            string content = base64string?.Split(new char[] { ',' })?.LastOrDefault();
            if (string.IsNullOrWhiteSpace(content) || !base64string.StartsWith("data:") ||
                !base64string.Contains(";base64,") || base64string.Count(x => x == ',') > 1)
            {
                throw new InvalidOperationException(ResultCode.InvalidBase64StringFormat.GetNumericString());
            }

            return content;
        }

        public bool IsValidFileType(string base64string, out FileType fileType)
        {
            //refactor return content if valid ? result.Content
            var result = ExtractDetailsFromBase64String(base64string, APPLICATION_DATA_TYPE, out _);
            Enum.TryParse(result.Type?.ToUpper(), out fileType);
            bool isValid = fileType == FileType.DOCX ||
                           fileType == FileType.PDF ||
                           fileType == FileType.PNG ||
                           fileType == FileType.JPG ||
                           fileType == FileType.JPEG ||
                           fileType == FileType.CSV ||
                           result.Type == MimeFileType.MSG ||
                           result.Type == MimeFileType.DOCX ||
                           result.Type == MimeFileType.DOC ||
                           result.Type == MimeFileType.XLSX ||
                           result.Type.ToUpper() == MimeFileType.CSV ||
                           result.Type.ToUpper() == MimeFileType.CSV_2 ||
                           result.Type.ToUpper() == MimeFileType.CSV_3;

            fileType =
                result.Type == MimeFileType.DOCX ?
                FileType.DOCX :
                result.Type == MimeFileType.DOC ?
                FileType.DOC :
                result.Type == MimeFileType.XLSX ?
                FileType.XLSX :
                result.Type == MimeTypes.Xls.Replace("application/", "") ?
                FileType.XLS :
                result.Type.ToUpper() == MimeFileType.CSV || result.Type.ToUpper() == MimeFileType.CSV_2
                || result.Type.ToUpper() == MimeFileType.CSV_3 ?
                FileType.CSV : result.Type.ToLower() == MimeFileType.MSG ? FileType.MSG : fileType;

            return isValid;
        }

        public bool IsValidSignatureImageType(string base64string, out SignatureImageType imageType)
        {
            var result = ExtractDetailsFromBase64String(base64string, IMAGE_DATA_TYPE, out bool isValidFormat);

            bool isvalid = Enum.TryParse(result.Type?.ToUpper(), out imageType);
            return isvalid;
        }
        public bool IsValidImageType(string base64string, out ImageType imageType)
        {
            var result = ExtractDetailsFromBase64String(base64string, IMAGE_DATA_TYPE, out bool isValidFormat);

            bool isvalid = Enum.TryParse(result.Type?.ToUpper(), out imageType);
            return isvalid;
        }

        public bool IsOctetStreamIsValidWord(string base64string, out FileType fileType)
        {
            var result = ExtractDetailsFromBase64String(base64string, MimeTypes.Octet, out bool isValidFormat);
            if (isValidFormat)
            {
                if (result.Content.StartsWith("UEsDBBQABgAIAAAA"))
                {
                    fileType = FileType.DOCX;
                    return true;
                }
                if (result.Content.StartsWith("0M8R4KGxGuE"))
                {
                    fileType = FileType.DOC;
                    return true;
                }
            }

            //only for compilation - there is no use of file type while return false
            fileType = FileType.HTML;
            return false;
        }

        public bool IsValidFile(string base64string)
        {
            var applicationTypeResult = ExtractDetailsFromBase64String(base64string, APPLICATION_DATA_TYPE, out bool isValidApplicationFormat);
            var textTypeResult = ExtractDetailsFromBase64String(base64string, TEXT_DATA_TYPE, out bool isValidTextFormat);

            if ((IsValidBase64File(textTypeResult.Content) && isValidTextFormat) ||
                (IsValidBase64File(applicationTypeResult.Content) && isValidApplicationFormat))
            {
                return true;
            }

            return false;

        }

        public bool IsValidImage(string base64string)
        {
            if (string.IsNullOrEmpty(base64string))
            {
                return false;
            }
            var result = ExtractDetailsFromBase64String(base64string, IMAGE_DATA_TYPE, out bool isValidFormat);
            if (!isValidFormat || !IsValidImage(Convert.FromBase64String(result.Content)))
            {
                return false;
            }
            return true;
        }

        #region Private Functions

        private bool IsValidBase64File(string base64file)
        {
            try
            {
                var bytes = Convert.FromBase64String(base64file);
                //TODO check it
                //base64string = base64string.Trim();
                //return (base64string.Length % 4 == 0) && Regex.IsMatch(base64string, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private (string Type, string Content) ExtractDetailsFromBase64String(string base64string, string dataType, out bool isValidFormat)
        {
            // string content = Getbase64Content(base64string);
            string[] array = base64string?.Split(new char[] { ',' }, 2);
            isValidFormat = array != null && array.Count() == 2 && array[0].Contains($"data:{dataType}") && array[0].Contains(";base64");

            return array != null && array.Count() == 2 ?
                    (array[0].Replace(";base64", "").Substring(array[0].IndexOf('/') + 1), array[1]) :
                    ("", array != null ? array[0] : "");
        }

        private bool IsValidImage(byte[] bytes)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    using (Image.FromStream(ms))
                    {

                    };
                }
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        #endregion
    }
}
