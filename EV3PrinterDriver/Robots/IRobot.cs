using EV3PrinterDriver.Collections;
using EV3PrinterDriver.Commands;
using MonoBrickFirmware.Movement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EV3PrinterDriver.Robots
{
    interface IRobot : IDisposable
    {
        /// <summary>
        /// Raised when the robot is emitting data
        /// </summary>
        event EventHandler<DataEventArgs> OnData;

        MotorSettingCollection<Motor> Motors { get;  }
        MotorSettingCollection<sbyte> SpeedSettings { get; }
        MotorSettingCollection<float> RatioSettings { get;  }

        /// <summary>
        /// Start calibration routine.
        /// </summary>
        /// <param name="stop">Indicates whether to stop calibration</param>
        void Calibrate(Func<bool> stop);

        void ResetTachos();        
        /// <summary>
        /// Queue a robot command
        /// </summary>
        /// <param name="command"></param>
        void Queue(IRobotCommand command);

        /// <summary>
        /// Create a rotate task
        /// </summary>
        /// <param name="port">Motor identifer</param>
        /// <param name="targetTacho">Target rotation</param>
        /// <returns></returns>
        WaitHandle CreateRotateTask(MotorPort port, int targetTacho);

        /// <summary>
        /// Removes any pending commands and stops all motors
        /// </summary>
        void Off();
    }
}
