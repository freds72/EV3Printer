using EV3PrinterDriver.Robots;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Sensors;
using System.Threading;
using System;
using MonoBrickFirmware.Display;

namespace EV3PrinterDriver
{
    class ScannerRobot : RobotBase, IScannerRobot
    {
        readonly EV3TouchSensor _resetSensor;
        readonly EV3ColorSensor _colorSensor;
        Thread _scanThread;
        ManualResetEvent _scanWait = new ManualResetEvent(false);

        public ScannerRobot():
            base(new MotorPort[] { RobotSetup.XPort, RobotSetup.YPort, RobotSetup.PenPort })
        {
            _resetSensor = new EV3TouchSensor(RobotSetup.XResetPort);
            _colorSensor = new EV3ColorSensor(RobotSetup.ColorPort, ColorMode.RGB);

            _scanThread = new Thread(ScanPollThread);
            _scanThread.IsBackground = true;
            _scanThread.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if ( disposing )
            {
                _scanThread.Join();
                _scanThread = null;
            }
            base.Dispose(disposing);
        }

        public override void Off()
        {
            base.Off();

            // back to non-signaled state
            _scanWait.Reset();
        }

        private int _scanDelay = 100;
        public void Scan(int delay)
        {
            if (delay < 0)
                throw new ArgumentOutOfRangeException("Delay must be positive.");
            _scanDelay = delay;
            _scanWait.Set();
        }

        public override void Calibrate(Func<bool> stop)
        {
            Motor motor = null;

            ResetTachos();

            motor = Motors[RobotSetup.XPort];
            motor.SpeedProfile(16, 0, (uint)Math.Abs(1800 * RatioSettings[RobotSetup.XPort]), 0, false);
            while (_resetSensor.IsPressed() == false && stop() == false) ;
            Off();
            ResetTachos();
        }

        void ScanPollThread()
        {
            try
            {
                Motor xmotor = Motors[RobotSetup.XPort];
                Motor ymotor = Motors[RobotSetup.YPort];

                float xratio = RatioSettings[RobotSetup.XPort];
                float yratio = RatioSettings[RobotSetup.YPort];

                int prevx = -1;
                int prevy = -1;
                while (true)
                {
                    _scanWait.WaitOne();

                    // capture motor positions
                    int x = xmotor.GetTachoCount();
                    int y = ymotor.GetTachoCount();
                    if (prevx != x || prevy != y)
                    {
                        // send a data point
                        RGBColor color = _colorSensor.ReadRGB();

                        // a bit of formatting
                        SendData(string.Format("PIX;{0};{1};{2};{3:#.##};{4:#.##}\0", new object[] { color.Red, color.Green, color.Blue, x / xratio, y / yratio }));

                        //
                        // LcdConsole.WriteLine(string.Format("{0:#.##}:{1:#.##} = {2}", x / xratio, y / yratio, color.ToString()));

                        prevx = x;
                        prevy = y;
                    }
                    if ( _scanDelay > 0)
                        Thread.Sleep(_scanDelay);
                }
            }
            catch (OperationCanceledException)
            {
                // normal exception when thread is stopped
            }
            catch (Exception ex)
            {
                // 
                while (ex != null)
                {
                    LcdConsole.WriteLine(ex.Message);
                    ex = ex.InnerException;
                }
                throw;
            }
        }

        protected override void PostCommand()
        {
            // did we touch limit?
            if (_resetSensor.IsPressed())
                ResetTacho(RobotSetup.XPort);
        }
    }
}
