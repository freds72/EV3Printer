using EV3PrinterDriver.Robots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EV3PrinterDriver.Commands
{
    struct StopCommand : IRobotCommand
    {
        public const string Token = "STP";

        public void Do(IRobot robot)
        {
            robot.Off();
        }

        public override string ToString()
        {
            return "STOP";
        }
    }
}
