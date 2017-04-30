using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace EV3Printer.ViewModels
{
    public class PenCommandArgs
    {
        public bool IsInContact { get; set; }
        public Point Location { get; set; }
    }
}
