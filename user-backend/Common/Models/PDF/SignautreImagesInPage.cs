using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.PDF
{

    public class SignautreImagesInPage
    {
        public int PageNumber { get; set; }
        public List<SignautreImageInPage> signautreImageInPages { get; set; } 
        public SignautreImagesInPage()
        {
            signautreImageInPages = new List<SignautreImageInPage>();
        }
    }
}
