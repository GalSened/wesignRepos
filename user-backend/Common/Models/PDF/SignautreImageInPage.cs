using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.PDF
{

        public class SignautreImageInPage
        {
            public int ImageListId { get; set; }
            public int ImageId { get; set; }
            public int ImageGID { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double W { get; set; }
            public double H { get; set; }
            public int ImageIndex { get;  set; }
        }
    
}
