using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EV3Printer.Services
{
    public interface IEV3Brick
    {
        event EventHandler<LogEventArgs> OnLog;
        event EventHandler<DataEventArgs> OnData;
        event EventHandler<ConnectedEventArgs> OnConnect;

        bool IsConnected { get; }
        void Connect(string address);
        void Send(string command);
    }    
}
