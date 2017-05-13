using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EV3PrinterDriver
{
    /// <summary>
    /// Build-specific motor bindings
    /// </summary>
    static class RobotSetup
    {
        public static readonly MotorPort PenPort = MotorPort.OutC;
        public static readonly MotorPort XPort = MotorPort.OutA;
        public static readonly MotorPort YPort = MotorPort.OutB;
        public static readonly SensorPort XResetPort = SensorPort.In1;
    }
}
