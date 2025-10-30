using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.Documents
{
    public class Appendix
    {
        public string Name { get; set; }
        public string Base64File { get; set; }
        public string FileExtention { get; set; }
        public byte[] FileContent { get; set; }
    }
}
