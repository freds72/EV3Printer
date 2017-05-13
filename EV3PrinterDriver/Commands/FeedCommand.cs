using EV3PrinterDriver.Robots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EV3PrinterDriver.Commands
{
    struct FeedCommand : IRobotCommand
    {
        public static readonly string Token = "FEED";
        public int Y;

        public void Do(IRobot robot)
        {
            // "eat" paper
            robot.Motors[RobotSetup.YPort].ResetTacho();
            robot.CreateRotateTask(RobotSetup.YPort, Y).WaitOne();
            robot.Motors[RobotSetup.YPort].ResetTacho();
        }
    }
}
