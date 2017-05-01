using EV3PrinterDriver.RoboCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EV3PrinterDriver
{
    class RobotCommandFactory
    {
        public IRobotCommand Create(string rawCommand)
        {
            string[] tokens = rawCommand.Split(';');
            // get message type
            switch (tokens[0])
            {
                case HandCommand.UpToken:
                    return new HandCommand(true);
                case HandCommand.DownToken:
                    return new HandCommand(false);
                case MoveCommand.Token:
                    return new MoveCommand
                    {
                        X = (int)Math.Round(float.Parse(tokens[1]), MidpointRounding.AwayFromZero),
                        Y = (int)Math.Round(float.Parse(tokens[2]), MidpointRounding.AwayFromZero)
                    };
                case FeedCommand.Token:
                    return new FeedCommand() { Y = int.Parse(tokens[1]) };
            }
            // unknonw command
            return null;
        }
    }
}
