using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace EV3Printer.Services
{
    public class PrinterSettings
    {
        // page size
        public static readonly int PageWidth = 970;
        public static readonly int PageHeight = 670;

        public double SimplificationFactor = 50;

        public bool HighDefSimplification = false;

        public bool SimplificationPreview = false;
    }
}
