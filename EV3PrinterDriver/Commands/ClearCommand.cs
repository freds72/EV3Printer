using EV3PrinterDriver.Robots;
using MonoBrickFirmware.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EV3PrinterDriver.Commands
{
    struct ClearCommand : IRobotCommand
    {
        public static readonly string Token = "CLR";

        public void Do(IRobot robot)
        {
            LcdConsole.Clear();
        }

        public override string ToString()
        {
            return "CLEAR";
        }
    }
}
