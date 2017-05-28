using EV3Printer.Converters;
using EV3Printer.Models;
using EV3Printer.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input.Inking;

namespace EV3Printer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IEV3Brick _brick;
        private readonly SystemSettings _sysSettings;
        private readonly PrinterSettings _printSettings;
        private readonly ScannerSettings _scannerSettings;
        private readonly ILogger _log;

        private RelayCommand<string> _sendCommand;
        public RelayCommand<string> SendCommand => _sendCommand ?? (_sendCommand = new RelayCommand<string>(
            command => { _brick.Send(command); },
            command => { return string.IsNullOrEmpty(command) == false; }
        ));
        private RelayCommand<string> _connectCommand;
        public RelayCommand<string> ConnectCommand => _connectCommand ?? (_connectCommand = new RelayCommand<string>(
            address => { _brick.Connect(address); }, 
            address => { return string.IsNullOrEmpty(address) == false && _brick != null; }
        ));
        

        public ObservableCollection<string> Logs { get { return _log.Logs; } }

        public double SimplificationFactor
        {
            get { return _printSettings.SimplificationFactor; }
            set
            {
                Set(ref _printSettings.SimplificationFactor, value);
            }
        }

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

        bool _highDef = false;
        public bool HighDefSimplification
        {
            get { return _highDef; }
            set
            {
                Set(ref _highDef, value);
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

        public bool SimplificationPreview
        {
            get { return _printSettings.SimplificationPreview; }
            set
            {
                Set(ref _printSettings.SimplificationPreview, value);
            }
        }

        public MainViewModel(IEV3Brick brick, ILogger log, PrinterSettings printSettings, ScannerSettings scannerSettings, SystemSettings systemSettings)
        {
            _log = log;
            _printSettings = printSettings;
            _sysSettings = systemSettings;
            _scannerSettings = scannerSettings;

            // default values
            ServerAddress = "10.0.1.1:13000";

            _brick = brick;
            _brick.OnLog += Brick_OnLog;
            _brick.OnConnect += Brick_OnConnect;
        }

        private void Brick_OnConnect(object sender, ConnectedEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () => IsConnected = e.State);
        }

        private void Brick_OnLog(object sender, LogEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () => _log.Log(e.Message));
        }       
    }
}
