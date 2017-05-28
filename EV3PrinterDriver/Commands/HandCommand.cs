using EV3PrinterDriver.Robots;
using MonoBrickFirmware.Movement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EV3PrinterDriver.Commands
{
    struct HandCommand : IRobotCommand
    {
        public static readonly string UpToken = "UP";
        public static readonly string DownToken = "DWN";

        public bool Up;
        public HandCommand(bool up)
        { Up = up; }

        public void Do(IRobot robot)
        {
            // Hand (must not be sync w/ motors)
            robot.Motors[RobotSetup.PenPort].SpeedProfile((sbyte)(Up ? 127 : -127), 0, 180, 0, true).WaitOne();
        }

        public override string ToString()
        {
            return Up ? "UP" : "DOWN";
        }
    }
}
