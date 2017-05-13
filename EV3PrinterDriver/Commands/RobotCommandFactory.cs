
using System;

namespace EV3PrinterDriver.Commands
{
    static class RobotCommandFactory
    {
        public static IRobotCommand Create(string rawCommand)
        {
            string[] tokens = rawCommand.Split(';');
            // get message type
            string token = tokens[0];
            if (token == HandCommand.UpToken)
                return new HandCommand(true);
            else if (token == HandCommand.DownToken)
                return new HandCommand(false);
            else if (token == MoveCommand.Token)
                return new MoveCommand
                {
                    X = (int)Math.Round(float.Parse(tokens[1]), MidpointRounding.AwayFromZero),
                    Y = (int)Math.Round(float.Parse(tokens[2]), MidpointRounding.AwayFromZero)
                };
            else if (token == FeedCommand.Token)
                return new FeedCommand() { Y = int.Parse(tokens[1]) };
            else if (token == ScanCommand.Token)
                return new ScanCommand() { IsActive = (int.Parse(tokens[1]) > 0) };

            // unknonw command
            return null;
        }
    }
}
