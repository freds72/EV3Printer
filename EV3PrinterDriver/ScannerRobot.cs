using EV3PrinterDriver.Robots;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Sensors;
using System.Threading;

namespace EV3PrinterDriver
{
    class ScannerRobot : RobotBase, IScannerRobot
    {
        readonly EV3TouchSensor _resetSensor;

        public ScannerRobot():
            base(new MotorPort[] { RobotSetup.XPort, RobotSetup.YPort, RobotSetup.PenPort })
        {
            _resetSensor = new EV3TouchSensor(RobotSetup.XResetPort);
        }

        Thread _scanThread;
        bool _scanRun = true;
        ManualResetEvent _scanWait = new ManualResetEvent(false);
        void ScanPollThread()
        {
            while (_scanRun)
            {
                // is scanner active?
                WaitHandle.WaitAll(new WaitHandle[] { _scanWait });

            }
        }

        public void Scan(bool isActive)
        {
            if (isActive)
                _scanWait.Set();
            else
                _scanWait.Reset();
        }
    }
}
