using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace EV3Printer.Models
{
    struct Pixel
    {
        public int X;
        public int Y;
        public Color Color;
        public Pixel(int x, int y, Color c)
        {
            X = x;
            Y = y;
            Color = c;
        }
    }
}
