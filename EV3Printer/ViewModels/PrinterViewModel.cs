using EV3Printer.Converters;
using EV3Printer.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using EV3Printer.Extensions;

namespace EV3Printer.ViewModels
{
    public class PrinterViewModel : PrinterViewModelBase
    {
        private readonly IEV3Brick _brick;
        private readonly PrinterSettings _settings;
        private readonly ILogger _log;

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

        private InkStrokesToPointsConverter _inkStrokeConverter = new InkStrokesToPointsConverter();
        private RelayCommand<InkStrokeContainer> _printCommand;
        public RelayCommand<InkStrokeContainer> PrintCommand => _printCommand ?? (_printCommand = new RelayCommand<InkStrokeContainer>(
            strokes =>
            {
                var pointStrokes = _inkStrokeConverter.Convert(strokes, _settings.SimplificationFactor / 100.0, _settings.HighDefSimplification);
                _log.Log(string.Format("Stroke Collections: {0}", pointStrokes.Count));
                // max X: 980
                // max Y: 670                
                foreach (List<Point> stroke in pointStrokes)
                {
                    _log.Log(string.Format("Strokes: {0}", stroke.Count));
                    for (int i = 0; i < stroke.Count; i++)
                    {
                        // move to position
                        _brick.Send(string.Format("MOV;{0:#.##};{1:#.##}",
                            -PrinterSettings.PageWidth * stroke[i].X.Clamp(20, 210 - 20) / 210,
                            -PrinterSettings.PageWidth * stroke[i].Y.Clamp(30, 297 - 30) / 297));
                        // first point = start drawing
                        //if (i == 0)
                        //    _brick.Send("DWN");
                    }
                    // done with segment
                    //_brick.Send("UP");
                }
            }
        ));

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
                _log.Clear();
                _brick.Send("CLR");
            },
            strokes => { return true; }
        ));

        public PrinterViewModel(IEV3Brick brick, ILogger log, PrinterSettings settings):
            base(brick)
        {
            _log = log;
            _brick = brick;
            _settings = settings;
        }
    }
}
