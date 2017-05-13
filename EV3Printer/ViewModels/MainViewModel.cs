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
            command => { return string.IsNullOrEmpty(command) == false; }
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

        private RelayCommand _newPageCommand;
        public RelayCommand NewPageCommand => _newPageCommand ?? (_newPageCommand = new RelayCommand(
            () => {
                _brick.Send("FEED;-25");
            }
        ));

        private RelayCommand _dropPageCommand;
        public RelayCommand DropPageCommand => _dropPageCommand ?? (_dropPageCommand = new RelayCommand(
            () => {
                _brick.Send("FEED;-750");
            }
        ));

        static double Clamp(double value, double min, double max)
        {
            return Math.Max(Math.Min(value, max), min);
        }

        private InkStrokesToPointsConverter _inkStrokeConverter = new InkStrokesToPointsConverter();
        private RelayCommand<InkStrokeContainer> _printCommand;
        public RelayCommand<InkStrokeContainer> PrintCommand => _printCommand ?? (_printCommand = new RelayCommand<InkStrokeContainer>(
            strokes =>
            {
                var pointStrokes = _inkStrokeConverter.Convert(strokes, SimplificationFactor / 100.0, HighDefSimplification);
                Logs.Add(string.Format("Stroke Collections: {0}", pointStrokes.Count));
                // max X: 980
                // max Y: 670                
                foreach (List<Point> stroke in pointStrokes)
                {
                    Logs.Add(string.Format("Strokes: {0}", stroke.Count));
                    for (int i = 0; i < stroke.Count; i++)
                    {
                        // move to position
                        _brick.Send(string.Format("MOV;{0:#.##};{1:#.##}",
                            -980 * Clamp(stroke[i].X, 20, 210 - 20) / 210,
                            -670 * Clamp(stroke[i].Y, 30, 297 - 30) / 297));
                        // first point = start drawing
                        if (i == 0)
                            _brick.Send("DWN");
                    }
                    // done with segment
                    _brick.Send("UP");
                }                
            }
        ));

        int _scanSession = 0;
        private RelayCommand _stopScanCommand;
        public RelayCommand StopScanCommand => _stopScanCommand ?? (_stopScanCommand = new RelayCommand(
            () => { _scanSession = 0;
                _brick.Send("SCAN;0");
            }));

        private RelayCommand _scanCommand;
        public RelayCommand ScanCommand => _scanCommand ?? (_scanCommand = new RelayCommand(
            () => {
                // init
                _scanSession++;
                _brick.Send("SCAN;1");
                _brick.Send(string.Format("MOV;{0:#.##};{1:#.##}", 0, 0));
                bool _reverse = false;
                for (int i = 0; i < 670; i += _scanResolution)
                {
                    // stop?
                    if (_scanSession == 0) break;

                    _brick.Send(string.Format("MOV;{0:#.##};{1:#.##}", _reverse?0:-970, i));
                    _reverse = !_reverse;
                }
                _brick.Send("SCAN;0");
            }));


        private RelayCommand<InkStrokeContainer> _testCommand;
        public RelayCommand<InkStrokeContainer> TestCommand => _testCommand ?? (_testCommand = new RelayCommand<InkStrokeContainer>(
            strokes =>
            {
                InkStrokeBuilder builder = new InkStrokeBuilder();
                var tmpStroke = builder.CreateStrokeFromInkPoints(new Point[] { new Point(0, 0), new Point(210, 297) }.Select(p => new InkPoint(p, 1)), Matrix3x2.Identity);
                strokes.AddStroke(tmpStroke);
            }));

        private RelayCommand<InkStrokeContainer> _clearCommand;
        public RelayCommand<InkStrokeContainer> ClearCommand => _clearCommand ?? (_clearCommand = new RelayCommand<InkStrokeContainer>(
            strokes => {
                strokes.Clear();
            },
            strokes => { return true; }
        ));

        public ObservableCollection<string> Logs { get; private set; }

        private double _simplification = 50;
        public double SimplificationFactor
        {
            get { return _simplification; }
            set
            {
                Set(ref _simplification, value);
            }
        }

        private int _scanResolution = 1;
        public int ScanResolution
        {
            get { return _scanResolution; }
            set
            {
                Set(ref _scanResolution, value);
            }
        }

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

        bool _simplificationPreview;
        public bool SimplificationPreview
        {
            get { return _simplificationPreview; }
            set
            {
                Set(ref _simplificationPreview, value);
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
            DispatcherHelper.CheckBeginInvokeOnUI(
                () => IsConnected = e.State);
        }

        private void Brick_OnLog(object sender, LogEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () => Logs.Add(e.Message));
        }       
    }
}
