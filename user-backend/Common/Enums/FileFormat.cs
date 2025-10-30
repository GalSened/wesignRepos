namespace Common.Enums
{
    public static class MimeFileType
    {
        public const string DOCX = "vnd.openxmlformats-officedocument.wordprocessingml.document";
        public const string DOC = "msword";
        public const string CSV = "VNDMS-EXCEL";
        public const string CSV_2 = "VNDMSEXCEL";
        public const string CSV_3 = "VND.MS-EXCEL";
        public const string XLSX = "vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string MSG = "vnd.ms-outlook";
    }

    public enum FileType
    {
        PDF = 1,
        DOC = 2,
        DOCX = 3,
        PNG = 4,
        JPG = 5,
        JPEG = 6,
        HTML = 7,
        CSV = 8,
        XLSX = 9,
        MSG = 10,
        XLS = 11,
        XML = 12,
        RTF = 13
    }

}
