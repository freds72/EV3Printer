using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EV3Printer.Services
{
    public class LogEventArgs
    {
        public string Message { get; private set; }
        public LogEventArgs(string msg)
        {
            Message = msg;
        }
    }
}
