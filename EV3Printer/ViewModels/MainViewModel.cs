using EV3Printer.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        static double Clamp(double value, double min, double max)
        {
            return Math.Max(Math.Min(value, max), min);
        }

        private RelayCommand<InkStrokeContainer> _printCommand;
        public RelayCommand<InkStrokeContainer> PrintCommand => _printCommand ?? (_printCommand = new RelayCommand<InkStrokeContainer>(
            strokes => {                
                Logs.Add(string.Format("Send {0} stroke(s).", strokes.GetStrokes().Count));
                // max X: 980
                // max Y: 670                
                foreach(InkStroke stroke in strokes.GetStrokes())
                {
                    var points = simplify(stroke.GetInkPoints(), 0.5, false);
                    for (int i = 0; i < points.Count; i++)
                    {
                    // move to position
                    _brick.Send(string.Format("MOV;{0:#.##};{1:#.##}",
                        -980 * Clamp(points[i].X, 20, 210 - 20) / 210,
                        -670 * Clamp(points[i].Y, 30, 297 - 30) / 297));
                        // first point = start drawing
                        if ( i == 0 )
                            _brick.Send("DWN");
                    }
                    // done with segment
                    _brick.Send("UP");
                }
            },
            strokes => { return true; }
        ));

        private RelayCommand<InkStrokeContainer> _clearCommand;
        public RelayCommand<InkStrokeContainer> ClearCommand => _clearCommand ?? (_clearCommand = new RelayCommand<InkStrokeContainer>(
            strokes => {
                strokes.Clear();
            },
            strokes => { return true; }
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
            DispatcherHelper.CheckBeginInvokeOnUI(
                () => IsConnected = e.State);
        }

        private void Brick_OnLog(object sender, LogEventArgs e)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(
                () => Logs.Add(e.Message));
        }

        // taken from: http://mourner.github.io/simplify-js/
        // square distance between 2 points
        double getSqDist(Point p1, Point p2)
        {
            var dx = p1.X - p2.X;
            var dy = p1.Y - p2.Y;

            return dx * dx + dy * dy;
        }

        // square distance from a point to a segment
        double getSqSegDist(Point p, Point p1, Point p2)
        {

            var x = p1.X;
            var y = p1.Y;
            var dx = p2.X - x;
            var dy = p2.Y - y;

            if (dx != 0 || dy != 0)
            {
                var t = ((p.X - x) * dx + (p.Y - y) * dy) / (dx * dx + dy * dy);

                if (t > 1)
                {
                    x = p2.X;
                    y = p2.Y;
                }
                else if (t > 0)
                {
                    x += dx * t;
                    y += dy * t;
                }
            }

            dx = p.X - x;
            dy = p.Y - y;

            return dx * dx + dy * dy;
        }
        // rest of the code doesn't care about point format

        // basic distance-based simplification
        List<Point> simplifyRadialDist(IReadOnlyList<InkPoint> points, double sqTolerance)
        {
            var prevPoint = points[0].Position;
            var newPoints = new List<Point>();
            Point point;

            for(int i=1;i<points.Count;i++)
            {
                point = points[i].Position;

                if (getSqDist(point, prevPoint) > sqTolerance)
                {
                    newPoints.Add(point);
                    prevPoint = point;
                }
            }

            if (prevPoint != point) newPoints.Add(point);

            return newPoints;
        }

        void simplifyDPStep(IReadOnlyList<InkPoint> points, int first, int last, double sqTolerance, List<Point> simplified)
        {
            var maxSqDist = sqTolerance;
            int index = 0;

            for (int i = first + 1; i < last; i++)
            {
                var sqDist = getSqSegDist(points[i].Position, points[first].Position, points[last].Position);

                if (sqDist > maxSqDist)
                {
                    index = i;
                    maxSqDist = sqDist;
                }
            }

            if (maxSqDist > sqTolerance)
            {
                if (index - first > 1) simplifyDPStep(points, first, index, sqTolerance, simplified);
                simplified.Add(points[index].Position);
                if (last - index > 1) simplifyDPStep(points, index, last, sqTolerance, simplified);
            }
        }

        // simplification using Ramer-Douglas-Peucker algorithm
        List<Point> simplifyDouglasPeucker(IReadOnlyList<InkPoint> points, double sqTolerance)
        {
            var last = points.Count - 1;

            var simplified = new List<Point>();
            simplified.Add(points[0].Position);
            simplifyDPStep(points, 0, last, sqTolerance, simplified);
            simplified.Add(points[last].Position);

            return simplified;
        }

        // both algorithms combined for awesome performance
        List<Point> simplify(IReadOnlyList<InkPoint> points, double tolerance, bool highestQuality)
        {
            if (points.Count <= 2) return points.Select(p => p.Position).ToList();

            var sqTolerance = tolerance * tolerance;

            var tmp = highestQuality ? points.Select(p => p.Position).ToList() : simplifyRadialDist(points, sqTolerance);
            tmp = simplifyDouglasPeucker(points, sqTolerance);

            return tmp;
        }
    }
}
