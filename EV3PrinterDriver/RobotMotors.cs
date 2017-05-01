using EV3PrinterDriver.RoboCommands;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EV3PrinterDriver
{
    class RobotMotors : IDisposable
    {
        bool disposed = false;
        readonly EventWaitHandle _changedWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        WaitHandle[] _motorTasks = new WaitHandle[3];
        static readonly ManualResetEvent _completedTask = new ManualResetEvent(true);
        readonly Motor[] _motors = new Motor[3];
        readonly EV3TouchSensor _resetSensor;
        readonly List<IRobotCommand> _commands = new List<IRobotCommand>();

        Thread _thread;
        bool _run = true;

        public SByte MainMotorSpeed { get; set; }
        public SByte SecondaryMotorSpeed { get; set; }
        public SByte HandMotorSpeed { get; set; }
        public float MainMotorRatio { get; set; }
        public float SecondaryMotorRatio { get; set; }
        public float HandMotorRatio { get; set; }

        public RobotMotors(MotorPort mainMotor, MotorPort secondaryMotor, MotorPort handMotor, SensorPort resetPort)
        {
            _motors[0] = new Motor(mainMotor);
            _motors[1] = new Motor(secondaryMotor);
            _motors[2] = new Motor(handMotor);

            _resetSensor = new EV3TouchSensor(resetPort);

            // default values
            MainMotorSpeed = SByte.MaxValue;
            SecondaryMotorSpeed = SByte.MaxValue;
            HandMotorSpeed = SByte.MaxValue;

            _thread = new Thread(MotorPollThread);
            _thread.IsBackground = true;
            _thread.Start();
        }

        public void Queue(IRobotCommand command)
        {
            // make sure a single thread is changing values
            lock (_commands)
            {
                _commands.Add(command);
            }
            // notify worker thread
            _changedWaitHandle.Set();
        }

        private List<IRobotCommand> DequeueAll()
        {
            // clone the current command stack
            lock (_commands)
            {
                var temp = new List<IRobotCommand>(_commands);
                _commands.Clear();
                return temp;
            }
        }

        public int GetRawTacho(MotorPort port)
        {
            return _motors[(int)port].GetTachoCount();
        }

        void Do(Action<Motor> action)
        {
            for (int i = 0; i < _motors.Length; i++)
                if (_motors[i] != null)
                    action(_motors[i]);
        }
        /// <summary>
        /// Turns off motor
        /// </summary>
        public void Off()
        {
            Do((m) => m.Off());
        }

        #region implements IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                _run = false;
                _changedWaitHandle.Set();
                _thread.Join();
                Do((m) => m.Off());
                for (int i = 0; i < _motors.Length; i++)
                    _motors[i] = null;                
            }
            // Free any unmanaged objects here.
            disposed = true;
        }
        #endregion

        /// <summary>
        /// Reset tachymeter
        /// </summary>
        public void ResetTachos()
        {
            lock (_commands)
                _commands.Clear();
            Do((m) => m.ResetTacho());
        }

        void ResetTacho(int i)
        {
            lock (_commands)
                _commands.Clear();
            _motors[i].ResetTacho();
        }

        static WaitHandle Rotate(Motor motor, int targetTacho, float ratio, sbyte speed)
        {
            // current pos
            int motorTacho = motor.GetTachoCount();

            // get target value
            targetTacho = (int)Math.Round(targetTacho * ratio, MidpointRounding.AwayFromZero);
            int err = (targetTacho - motorTacho);
            if (err != 0)
            {
                return motor.SpeedProfile((err > 0) ? speed : (sbyte)-speed, 0, (uint)Math.Abs(err), 0, true);
            }

            return _completedTask;
        }

        public enum CalibrationSteps
        {
            X,
            Y,
            Hand,
            Test,
            Pause
        }

        public void Calibrate(Func<CalibrationSteps, bool> calibrated, bool skip = false)
        {
            Motor motor = null;

            ResetTachos();

            motor = _motors[0];
            motor.SpeedProfile(16, 0, (uint)Math.Abs(1800 * MainMotorRatio), 0, false);
            while (!calibrated(CalibrationSteps.X)) ;
            Off();
            ResetTachos();
        }

        void Do(HandCommand command)
        {
            // ------------------------------------
            // main motor rotation
            _motorTasks[0] = _completedTask;

            // -----------------------------------------------------
            // secondary motor rotation 
            _motorTasks[1] = _completedTask;

            // Hand
            _motorTasks[2] = _motors[2].SpeedProfile((sbyte)(command.Up ? 127 : -127), 0, 180, 0, true);
        }

        void Do(MoveCommand command)
        {
            // ------------------------------------
            // main motor rotation
            _motorTasks[0] = Rotate(_motors[0], command.X, MainMotorRatio, MainMotorSpeed);

            // -----------------------------------------------------
            // secondary motor rotation 
            _motorTasks[1] = Rotate(_motors[1], command.Y, SecondaryMotorRatio, SecondaryMotorSpeed);

            // Hand (not used for now)
            _motorTasks[2] = _completedTask;
        }


        void Do(FeedCommand command)
        {
            // "eat" paper
            _motors[1].ResetTacho();
            Rotate(_motors[1], command.Y, SecondaryMotorRatio, SecondaryMotorSpeed).WaitOne();
            _motors[1].ResetTacho();

            // ------------------------------------
            // main motor rotation
            _motorTasks[0] = _completedTask;

            // -----------------------------------------------------
            // secondary motor rotation 
            _motorTasks[1] = _completedTask;

            // Hand (not used for now)
            _motorTasks[2] = _completedTask;
        }

        void MotorPollThread()
        {           
            while (_run)
            {
                // wait until there is something to do
                _changedWaitHandle.WaitOne();

                // take all pending comamnds
                List<IRobotCommand> commands = DequeueAll();
                for (int i = 0; i < commands.Count && _run; i++)
                {
                    IRobotCommand command = commands[i];

                    if (command.GetType() == typeof(HandCommand))
                    {
                        Do((HandCommand)command);
                    }
                    else if (command.GetType() == typeof(MoveCommand))
                    {
                        Do((MoveCommand)command);
                    }
                    else if(command.GetType() == typeof(FeedCommand))
                    {
                        Do((FeedCommand)command);
                    }
                    // unknown command

                    // wait for all motors to finish
                    WaitHandle.WaitAll(_motorTasks);

                    // did we touch limit?
                    if (_resetSensor.IsPressed())
                        _motors[0].ResetTacho();
                }
            }
        }
    }
}
