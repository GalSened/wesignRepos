using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.PDF
{
    public class SignautreImagesList
    {
        public Dictionary<int, SignautreImagesInPage> ImagesInPage { get; set; }
        public SignautreImagesList()
        {
            ImagesInPage = new Dictionary<int, SignautreImagesInPage>();
        }

    }
}
