using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EV3Printer.Services
{
    public class DataEventArgs : EventArgs
    {
        public string Data { get; private set; }
        public DataEventArgs(string data)
        {
            Data = data;
        }
    }
}
