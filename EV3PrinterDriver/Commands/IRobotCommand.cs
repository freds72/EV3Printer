using EV3PrinterDriver.Robots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EV3PrinterDriver.Commands
{
    interface IRobotCommand
    {
        void Do(IRobot robot);
    }
}
