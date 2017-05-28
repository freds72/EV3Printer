using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EV3Printer.Services
{
    class EV3Brick : IEV3Brick
    {
        Socket _socket;

        public event EventHandler<LogEventArgs> OnLog;
        public event EventHandler<DataEventArgs> OnData;
        public event EventHandler<ConnectedEventArgs> OnConnect;

        public string ServerAddress { get; private set; }

        public bool IsConnected { get { return _socket != null && _socket.Connected; } }

        public EV3Brick()
        {
        }

        private void ResetBuffer(SocketAsyncEventArgs e)
        {
            var buffer = new Byte[1024];

            e.SetBuffer(buffer, 0, 1024);
        }

        private void SocketReceive(Object sender, SocketAsyncEventArgs e)
        {
            ProcessData(e.Buffer, e.BytesTransferred);

            ResetBuffer(e);

            _socket.ReceiveAsync(e);
        }

        private void ProcessData(Byte[] data, Int32 count)
        {
            if (count > 0)
            {
                string errorLog = System.Text.Encoding.ASCII.GetString(data, 0, count);
                OnData?.Invoke(this, new DataEventArgs(errorLog));

                // System.Diagnostics.Debug.WriteLine(errorLog);
            }
        }

        public void Connect(string serverAddress = "10.0.1.1:13000")
        {
            ServerAddress = serverAddress;
            string result = string.Empty;

            // Create DnsEndPoint. The hostName and port are passed in to this method.
            string[] address = ServerAddress.Split(':');
            int port = 13000;
            DnsEndPoint hostEntry = new DnsEndPoint(address[0], address.Length == 2 ? int.Parse(address[1], System.Globalization.NumberFormatInfo.InvariantInfo) : port);

            // cleanup any previous connection
            if (_socket != null && _socket.Connected)
            {
                _socket.Dispose();
                OnConnect?.Invoke(this, new ConnectedEventArgs(false));
            }

            // Create a stream-based, TCP socket using the InterNetwork Address Family. 
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Create a SocketAsyncEventArgs object to be used in the connection request
            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
            socketEventArg.RemoteEndPoint = hostEntry;

            // Inline event handler for the Completed event.
            // Note: This event handler was implemented inline in order to make this method self-contained.
            socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate (object s, SocketAsyncEventArgs e)
            {
                if (e.SocketError == SocketError.Success)
                    OnConnect?.Invoke(this, new ConnectedEventArgs(true));

                OnLog?.Invoke(this, new LogEventArgs("Server response: " + e.SocketError.ToString()));

                // listen 
                SocketAsyncEventArgs traceEventArg = new SocketAsyncEventArgs();
                traceEventArg.RemoteEndPoint = hostEntry;
                ResetBuffer(traceEventArg);
                traceEventArg.Completed += SocketReceive;                
                _socket.ReceiveAsync(traceEventArg);
            });

            OnLog?.Invoke(this, new LogEventArgs(string.Format("Connecting {0}:{1}", hostEntry.Host, hostEntry.Port)));

            // Make an asynchronous Connect request over the socket
            _socket.ConnectAsync(socketEventArg);
        }

        public void Send(string command)
        {
            if ( IsConnected )
            {
                OnLog?.Invoke(this, new LogEventArgs(string.Format("Cmd: {0}", command)));

                SocketAsyncEventArgs completeArgs = new SocketAsyncEventArgs();
                byte[] buffer = Encoding.ASCII.GetBytes(string.Format("{0}\0", command));
                completeArgs.SetBuffer(buffer, 0, buffer.Length);
                completeArgs.UserToken = _socket;
                completeArgs.RemoteEndPoint = _socket.RemoteEndPoint;
                _socket.SendAsync(completeArgs);
            }
            else
            {
                OnLog?.Invoke(this, new LogEventArgs(string.Format("Ignored: {0}", command)));
            }
        }
    }
}
