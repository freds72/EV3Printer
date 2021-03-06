﻿using EV3PrinterDriver.Robots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EV3PrinterDriver.Commands
{
    struct ScanCommand : IRobotCommand
    {
        public const string Token = "SCN";
        public int Delay;

        public void Do(IRobot robot)
        {
            IScannerRobot scanner = (IScannerRobot)robot;
            scanner.Scan(Delay);
        }

        public override string ToString()
        {
            return string.Format("SCAN: {0}", Delay);
        }
    }
}
