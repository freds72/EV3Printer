using EV3PrinterDriver.Collections;
using EV3PrinterDriver.Commands;
using MonoBrickFirmware.Movement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EV3PrinterDriver
{
    interface IRobot
    {
        MotorSettingCollection<Motor> Motors { get;  }
        MotorSettingCollection<sbyte> SpeedSettings { get; }
        MotorSettingCollection<float> RatioSettings { get;  }

        void ResetTachos();        
        void Queue(IRobotCommand command);
        void Queue(WaitHandle taskHandle);
        WaitHandle CreateRotateTask(MotorPort port, int targetTacho);
        void Off();
    }
}
