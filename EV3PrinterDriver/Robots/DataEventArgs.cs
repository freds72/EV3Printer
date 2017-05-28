using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EV3PrinterDriver.Robots
{
    class DataEventArgs : EventArgs
    {
        public String Data { get; private set; }
        public DataEventArgs(string data)
        {
            this.Data = data;
        }
    }
}
