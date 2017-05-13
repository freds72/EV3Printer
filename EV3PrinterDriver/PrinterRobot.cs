using EV3PrinterDriver.Robots;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EV3PrinterDriver
{
    class PrinterRobot : RobotBase, IPrinterRobot
    {
        readonly EV3TouchSensor _resetSensor;

        public PrinterRobot():
            base(new MotorPort[] { RobotSetup.XPort, RobotSetup.YPort, RobotSetup.PenPort })
        {
            _resetSensor = new EV3TouchSensor(RobotSetup.XResetPort);
        }

        public enum CalibrationSteps
        {
            X,
            Y,
            Hand,
            Test,
            Pause
        }

        public void Calibrate(Func<CalibrationSteps, bool> calibrated, bool skip = false)
        {
            Motor motor = null;

            ResetTachos();

            motor = Motors[RobotSetup.XPort];
            motor.SpeedProfile(16, 0, (uint)Math.Abs(1800 * RatioSettings[RobotSetup.XPort]), 0, false);
            while (!calibrated(CalibrationSteps.X)) ;
            Off();
            ResetTachos();
        }

        protected override void PostCommand()
        {
            // did we touch limit?
            if (_resetSensor.IsPressed())
                ResetTacho(RobotSetup.XPort);
        }
    }
}
