using EV3Printer.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EV3Printer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public enum EditModeType
        {
            FreeForm,
            Text,
            None
        };

        private IEV3Brick _brick;
        
        private RelayCommand<string> _sendCommand;
        public RelayCommand<string> SendCommand => _sendCommand ?? (_sendCommand = new RelayCommand<string>(
            command => { _brick.Send(command); },
            command => { return string.IsNullOrEmpty(command) == false && _brick.IsConnected; }
        ));
        private RelayCommand<string> _connectCommand;
        public RelayCommand<string> ConnectCommand => _connectCommand ?? (_connectCommand = new RelayCommand<string>(
            address => { _brick.Connect(address); }, 
            address => { return string.IsNullOrEmpty(address) == false && _brick != null; }
        ));
        
        private RelayCommand<bool> _drawCommand;
        public RelayCommand<bool> DrawCommand => _drawCommand ?? (_drawCommand = new RelayCommand<bool>(
            isChecked => {
                /* todo */
            },
            isChecked => { return true; }
        ));

        private RelayCommand<bool> _textCommand;
        public RelayCommand<bool> TextCommand => _textCommand ?? (_textCommand = new RelayCommand<bool>(
            isChecked => {
                /* todo */
            },
            isChecked => { return true; }
        ));
        private RelayCommand _clearCommand;
        public RelayCommand ClearCommand => _clearCommand ?? (_clearCommand = new RelayCommand(
            () => { /* todo */ },
            () => { return true; }
        ));

        private RelayCommand<PenCommandArgs> _penCommand;
        public RelayCommand<PenCommandArgs> PenCommand => _penCommand ?? (_penCommand = new RelayCommand<PenCommandArgs>(
            args => {
            System.Diagnostics.Debug.WriteLine(string.Format("{0} - {1}", args.Location, args.IsInContact));
            },
            args => { return true; }
        ));

        public ObservableCollection<string> Logs { get; private set; }

        private EditModeType _mode = EditModeType.None;
        public EditModeType EditMode
        {
            get { return _mode; }
            set
            {
                Set(ref _mode, value);
                // related properties
                // RaisePropertyChanged("IsInDrawMode");
                // RaisePropertyChanged("IsInTextMode");
            }
        }

        public bool IsInDrawMode
        {
            get { return EditMode == EditModeType.FreeForm; }
            set {
                EditMode = value ? EditModeType.FreeForm : EditModeType.None;
                RaisePropertyChanged("IsInTextMode");
            }
        }

        public bool IsInTextMode
        {
            get { return EditMode == EditModeType.Text; }
            set
            {
                EditMode = value ? EditModeType.Text : EditModeType.None;
                RaisePropertyChanged("IsInDrawMode");
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

        string _text2Draw;
        public string Text2Draw
        {
            get { return _text2Draw; }
            set
            {
                Set(ref _text2Draw, value);
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
            // default values
            Logs = new ObservableCollection<string>();
            ServerAddress = "10.0.1.1:13000";

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
            Logs.Add(e.Message);
        }
    }
}
