using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Common.Models.XMLModels
{
    public class ColorType
    {
        public ColorType(int red, int green, int blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }


        public override bool Equals(object placeHolderColor)
        {
            return this.Red == Color.FromName(placeHolderColor.ToString()).R && this.Blue == Color.FromName(placeHolderColor.ToString()).B
                            && this.Green == Color.FromName(placeHolderColor.ToString()).G;
        }

    }
}
