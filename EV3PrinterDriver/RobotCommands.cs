using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EV3PrinterDriver.RoboCommands
{
    interface IRobotCommand
    {
    }

    struct HandCommand : IRobotCommand
    {
        public const string UpToken = "UP";
        public const string DownToken = "DWN";

        public bool Up;
        public HandCommand(bool up)
        { Up = up; }
    }

    struct FeedCommand : IRobotCommand
    {
        public const string Token = "FEED";
        public int Y;
    }

    struct MoveCommand : IRobotCommand
    {
        public const string Token = "MOV";
        public int X;
        public int Y;
    }
}
