using EV3Printer.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using EV3Printer.Extensions;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.Concurrent;
using EV3Printer.Models;

namespace EV3Printer.ViewModels
{
    public class ScannerViewModel : PrinterViewModelBase
    {
        private readonly IEV3Brick _brick;
        private readonly ScannerSettings _settings;
        private readonly WriteableBitmap _bmp;
        private readonly DispatcherTimer _timer;
        private readonly ConcurrentQueue<Pixel> _commands = new ConcurrentQueue<Pixel>();

        int _scanSession = 0;
        private RelayCommand _stopScanCommand;
        public RelayCommand StopCommand => _stopScanCommand ?? (_stopScanCommand = new RelayCommand(
            () => {
                _scanSession = 0;
                _brick.Send("STP;");
            }));

        private RelayCommand _clearCommand;
        public RelayCommand ClearCommand => _clearCommand ?? (_clearCommand = new RelayCommand(
            () => {
                using (_bmp.GetBitmapContext())
                {
                    _bmp.Clear(Colors.White);
                }
            }));

        private RelayCommand _scanCommand;
        public RelayCommand ScanCommand => _scanCommand ?? (_scanCommand = new RelayCommand(
            () => {
                // init
                _scanSession++;

                // clear image
                using (_bmp.GetBitmapContext())
                {
                    _bmp.Clear(Colors.White);
                }

                //
                _brick.Send(string.Format("SCN;{0}", _settings.ScanDelay));
                bool _reverse = false;
                for (int i = 0; i < PrinterSettings.PageHeight; i += _settings.ScanResolution)
                {
                    // stop?
                    if (_scanSession == 0) break;

                    // move to position
                    _brick.Send(string.Format("MOV;{0};{1}", _reverse ? -PrinterSettings.PageWidth : 0, -i));
                    // move to the other side!
                    _brick.Send(string.Format("MOV;{0};{1}", _reverse ? 0 : -PrinterSettings.PageWidth, -i));

                    // flip direction
                    _reverse = !_reverse;
                }
            }));

        public ImageSource ImageSource
        {
            get { return _bmp; }
        }

        public override void Cleanup()
        {
            base.Cleanup();
            _timer.Stop();
            _brick.OnData -= _brick_OnData;
        }

        private readonly ILogger _log;
        public ScannerViewModel(IEV3Brick brick, ILogger log, ScannerSettings settings):
            base(brick)
        {
            _log = log;
            // see: https://github.com/teichgraf/WriteableBitmapEx
            _bmp = BitmapFactory.New(PrinterSettings.PageWidth, PrinterSettings.PageHeight);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += _timer_Tick;
            _timer.Start();

            _brick = brick;
            _settings = settings;
            _brick.OnData += _brick_OnData;
        }

        private void _timer_Tick(object sender, object e)
        {
            if (_commands.Count > 0)
            {
                using (_bmp.GetBitmapContext())
                {
                    Pixel p;
                    while (_commands.TryDequeue(out p))
                    {
                        _bmp.SetPixel(p.X, p.Y, p.Color);
                    }
                } // no need to raise a property change
            }
        }

        string message = "";
        private void _brick_OnData(object sender, DataEventArgs e)
        {
            for (int i = 0; i < e.Data.Length; i++)
            {
                char c = e.Data[i];
                if (c != '\0')
                    message += c;
                else // message ready
                {
                    string[] tokens = message.Split(';');
                    if (tokens[0] == "PIX") // pixel message
                    {
                        int t = 1;
                        int r = int.Parse(tokens[t++], System.Globalization.NumberFormatInfo.InvariantInfo);
                        int g = int.Parse(tokens[t++], System.Globalization.NumberFormatInfo.InvariantInfo);
                        int b = int.Parse(tokens[t++], System.Globalization.NumberFormatInfo.InvariantInfo);
                        string xtoken = tokens[t++];
                        int x = string.IsNullOrEmpty(xtoken)?0:(int)Math.Round(float.Parse(xtoken, System.Globalization.NumberFormatInfo.InvariantInfo), MidpointRounding.AwayFromZero);
                        string ytoken = tokens[t++];
                        int y = string.IsNullOrEmpty(ytoken)?0:(int)Math.Round(float.Parse(ytoken, System.Globalization.NumberFormatInfo.InvariantInfo), MidpointRounding.AwayFromZero);

                        _commands.Enqueue(new Pixel(
                            Math.Abs(x).Clamp(0, PrinterSettings.PageWidth),
                            Math.Abs(y).Clamp(0, PrinterSettings.PageHeight),
                            Color.FromArgb(255, (byte)r.Clamp(0,255), (byte)g.Clamp(0,255), (byte)b.Clamp(0,255))));
                    }
                    else
                    {
                        // unknown message
                    }
                    // 
                    message = "";
                }
            }
        }
    }
}
