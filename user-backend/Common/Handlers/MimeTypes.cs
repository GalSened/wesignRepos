using System.Collections.Generic;
using System.Net.Mime;

namespace Common.Handlers
{
    public static class MimeTypes
	{
		public static Dictionary<string, string> Base64TypeFormatToExtentionType = new Dictionary<string, string>()
		{

			{"application/vnd.ms-outlook", "msg" },
			{"audio/aiff", "aiff" },
			{"application/unknown","octet" },
			{"video/x-msvideo", "avi" },
			{"text/css", "css" },
			{"text/csv", "csv" },
			{"application/msword", "doc" },
			{"application/vnd.openxmlformats-officedocument.wordprocessingml.document", "docx" },
			{"image/gif", "gif" },
			{"application/x-gzip", "gz" },
			{System.Net.Mime.MediaTypeNames.Text.Html, "html" },
			{System.Net.Mime.MediaTypeNames.Image.Jpeg, "jpeg" },
			{"application/x-javascript", "js" },
			{"audio/mid", "mid" },
			{"video/quicktime", "mov" },
			{"audio/mpeg", "mp3" },
			{System.Net.Mime.MediaTypeNames.Application.Octet, "octet" },
			{System.Net.Mime.MediaTypeNames.Application.Pdf, "pdf" },
			{"image/png", "png" },
			{"application/vnd.ms-powerpoint", "ppt" },
			{"application/vnd.openxmlformats-officedocument.presentationml.presentation", "pptx" },
			{System.Net.Mime.MediaTypeNames.Application.Rtf, "rtf" },
			{System.Net.Mime.MediaTypeNames.Image.Tiff, "tiff" },
			{System.Net.Mime.MediaTypeNames.Text.Plain, "txt" },
			{"application/vnd.ms-excel", "xls" },
			{"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx" },
			{System.Net.Mime.MediaTypeNames.Text.Xml, "xml" },
			{System.Net.Mime.MediaTypeNames.Application.Zip, "zip" },
			{"image/svg+xml", "svg" }
		};
		public static string Aiff
		{
			get { return "audio/aiff"; }
		}

		public static string Avi
		{
			get { return "video/x-msvideo"; }
		}

		public static string Css
		{
			get { return "text/css"; }
		}

        public static string Msg
        {
			get { return "application/vnd.ms-outlook"; }

        }
    
    
        public static string Csv
		{
			get { return "text/csv"; }
		}

		public static string Doc
		{
			get { return "application/msword"; }
		}

		public static string Docx
		{
			get { return "application/vnd.openxmlformats-officedocument.wordprocessingml.document"; }
		}

		public static string Dot
		{
			get { return "application/msword"; }
		}

		public static string Gif
		{
			get { return "image/gif"; }
		}

		public static string Gz
		{
			get { return "application/x-gzip"; }
		}

		public static string Htm
		{
			get { return System.Net.Mime.MediaTypeNames.Text.Html; }
		}

		public static string Html
		{
			get { return System.Net.Mime.MediaTypeNames.Text.Html; }
		}

		public static string Jpeg
		{
			get { return System.Net.Mime.MediaTypeNames.Image.Jpeg; }
		}

		public static string Jpg
		{
			get { return System.Net.Mime.MediaTypeNames.Image.Jpeg; }
		}

		public static string Js
		{
			get { return "application/x-javascript"; }
		}

		public static string Mid
		{
			get { return "audio/mid"; }
		}

		public static string Mov
		{
			get { return "video/quicktime"; }
		}

		public static string Mp3
		{
			get { return "audio/mpeg"; }
		}

		public static string Mpeg
		{
			get { return "video/mpeg"; }
		}

		public static string Mpg
		{
			get { return "video/mpeg"; }
		}

		public static string Octet
		{
			get { return System.Net.Mime.MediaTypeNames.Application.Octet; }
		}

		public static string Pdf
		{
			get { return System.Net.Mime.MediaTypeNames.Application.Pdf; }
		}

		public static string Pps
		{
			get { return "application/vnd.ms-powerpoint"; }
		}

		public static string Ppt
		{
			get { return "application/vnd.ms-powerpoint"; }
		}

		public static string Pptx
		{
			get { return "application/vnd.openxmlformats-officedocument.presentationml.presentation"; }
		}

		public static string Rtf
		{
			get { return System.Net.Mime.MediaTypeNames.Application.Rtf; }
		}

		public static string Tiff
		{
			get { return System.Net.Mime.MediaTypeNames.Image.Tiff; }
		}

		public static string Txt
		{
			get { return System.Net.Mime.MediaTypeNames.Text.Plain; }
		}

		public static string Xls
		{
			get { return "application/vnd.ms-excel"; }
		}

		public static string Xlsx
		{
			get { return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; }
		}

		public static string Xlt
		{
			get { return "application/vnd.ms-excel"; }
		}

		public static string Xml
		{
			get { return System.Net.Mime.MediaTypeNames.Text.Xml; }
		}

		public static string Zip
		{
			get { return System.Net.Mime.MediaTypeNames.Application.Zip; }
		}

		/// <summary>
		/// MIME type for unknown files
		/// </summary>
		public static string Unknown
		{
			get { return "application/unknown"; }
		}

		/// <summary>
		/// Try to determine MIME type based on the extension. If a type cannot be inferred, application/unknown will be returned
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static string Auto(string fileName)
		{
			string mimeType = "application/unknown";
			string extension = System.IO.Path.GetExtension(fileName).ToLower();

			Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(extension);

			if (regKey != null && regKey.GetValue("Content Type") != null)
			{
				mimeType = regKey.GetValue("Content Type").ToString();
			}
            

            if (mimeType == "application/unknown")
			{
				mimeType = FixMediaType(extension);

            }
			return mimeType;
		}

        private static string FixMediaType(string filePathExtension)
        {
            string mimeType = "application/unknown";
            switch (filePathExtension)
            {
                case ".msg":
                    {
                        mimeType = MimeTypes.Msg;
                        break;
                    }
                case ".doc":
                    {
                        mimeType = MimeTypes.Doc;
                        break;
                    }
                case ".docx":
                    {
                        mimeType = MimeTypes.Docx;
                        break;
                    }
                case ".pdf":
                    {
                        mimeType = MimeTypes.Pdf;
                        break;
                    }
                case ".ppt":
                    {
                        mimeType = MimeTypes.Ppt;
                        break;
                    }
                case ".pptx":
                    {
                        mimeType = MimeTypes.Pptx;
                        break;
                    }
                case ".xls":
                    {
                        mimeType = MimeTypes.Xls;
                        break;
                    }

                case ".xlsx":
                    {
                        mimeType = MimeTypes.Xlsx;
                        break;
                    }
                case ".csv":
                    {
                        mimeType = MimeTypes.Csv;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }


			return mimeType;

        }
    }
}
