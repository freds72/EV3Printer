using EV3PrinterDriver.Robots;
using MonoBrickFirmware.Movement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EV3PrinterDriver.Commands
{
    struct MoveCommand : IRobotCommand
    {
        public static readonly string Token = "MOV";

        public int X;
        public int Y;

        public void Do(IRobot robot)
        {
            WaitHandle[] handles = new WaitHandle[2];
            // ------------------------------------
            // main motor rotation
            handles[0] = robot.CreateRotateTask(RobotSetup.XPort, X);

            // -----------------------------------------------------
            // secondary motor rotation 
            handles[1] = robot.CreateRotateTask(RobotSetup.YPort, Y);

            // sync motors
            WaitHandle.WaitAll(handles);
        }

        public override string ToString()
        {
            return string.Format("TO: {0}/{1}", X, Y);
        }
    }
}
