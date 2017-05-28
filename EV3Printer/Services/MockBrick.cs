using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace EV3Printer.Services
{
    class MockBrick : IEV3Brick
    {
        public bool IsConnected
        {
            get
            {
                return true;
            }
        }

        public event EventHandler<ConnectedEventArgs> OnConnect;
        public event EventHandler<DataEventArgs> OnData;
        public event EventHandler<LogEventArgs> OnLog;

        public void Connect(string address)
        {
            // does nothing
            OnConnect(this, new ConnectedEventArgs(true));
        }

        public void Send(string command)
        {
            if (command.StartsWith("SCN"))
            {
                OnLog(this, new LogEventArgs("Mock scanner..."));
                Task.Factory.StartNew(() =>
                {
                    Byte[] color = new byte[3];
                    Random rnd = new Random();
                    for (int x = 0; x < PrinterSettings.PageWidth; x++)
                        for (int y = 0; y < PrinterSettings.PageHeight; y++)
                        {
                            rnd.NextBytes(color);
                            string msg = string.Format("PIX;{0};{1};{2};{3};{4}\0", color[0], color[1], color[2], x, y);
                            OnData(this, new DataEventArgs(msg));
                        }
                });
            }
        }

        public MockBrick()
        {
        }
    }
}
