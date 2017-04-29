using EV3Printer.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EV3Printer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private IEV3Brick _brick;

        private RelayCommand<String> _sendCommand;
        public RelayCommand<String> SendCommand => _sendCommand ?? (_sendCommand = new RelayCommand<String>(
            command => { _brick.Send(command); },
            command => { return string.IsNullOrEmpty(command) == false && _brick.IsConnected; }
        ));
        private RelayCommand<String> _connectCommand;
        public RelayCommand<String> ConnectCommand => _connectCommand ?? (_connectCommand = new RelayCommand<String>(
            address => { _brick.Connect(address); }, 
            address => { return string.IsNullOrEmpty(address) == false && _brick != null; }
        ));

        string _serverAddress;
        public string ServerAddress
        {
            get { return _serverAddress; }
            set
            {
                Set(ref _serverAddress, value);
            }
        }

        string _command;
        public string RawCommand
        {
            get { return _command; }
            set
            {
                Set(ref _command, value);
            }
        }

        string _text2Draw;
        public string Text2Draw
        {
            get { return _text2Draw; }
            set
            {
                Set(ref _text2Draw, value);
            }
        }

        string _lastLog;
        public string LastLog
        {
            get { return _lastLog; }
            set
            {
                Set(ref _lastLog, value);
            }
        }

        bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                Set(ref _isConnected, value);
            }
        }

        public MainViewModel(IEV3Brick brick)
        {
            _brick = brick;
            _brick.OnLog += Brick_OnLog;
            _brick.OnConnect += Brick_OnConnect;
        }

        private void Brick_OnConnect(object sender, ConnectedEventArgs e)
        {
            IsConnected = e.State;
        }

        private void Brick_OnLog(object sender, LogEventArgs e)
        {
            LastLog = e.Message;
        }
    }
}
