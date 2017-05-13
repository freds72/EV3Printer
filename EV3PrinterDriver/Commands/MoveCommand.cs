using EV3PrinterDriver.Robots;
using MonoBrickFirmware.Movement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EV3PrinterDriver.Commands
{
    struct MoveCommand : IRobotCommand
    {
        public static readonly string Token = "MOV";

        public int X;
        public int Y;

        public void Do(IRobot robot)
        {
            // ------------------------------------
            // main motor rotation
            robot.Queue(robot.CreateRotateTask(RobotSetup.XPort, X));

            // -----------------------------------------------------
            // secondary motor rotation 
            robot.Queue(robot.CreateRotateTask(RobotSetup.YPort, Y));
        }
    }
}
