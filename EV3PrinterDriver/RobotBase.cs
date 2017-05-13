using EV3PrinterDriver.Collections;
using EV3PrinterDriver.Commands;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Sensors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EV3PrinterDriver
{
    abstract class RobotBase : IDisposable, IRobot
    {
        bool disposed = false;
        readonly EventWaitHandle _changedWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        readonly WaitHandle[] _motorTasks;
        static readonly ManualResetEvent _completedTask = new ManualResetEvent(true);
        public MotorSettingCollection<Motor> Motors { get; private set; }
        public MotorSettingCollection<sbyte> SpeedSettings { get; private set; }
        public MotorSettingCollection<float> RatioSettings { get; private set; }
        readonly List<IRobotCommand> _commands = new List<IRobotCommand>();

        Thread _thread;
        bool _run = true;

        public RobotBase(MotorPort[] motorPorts)
        {
            _motorTasks = new WaitHandle[8];
            Motors = new MotorSettingCollection<Motor>(motorPorts.Length);
            SpeedSettings = new MotorSettingCollection<sbyte>(motorPorts.Length);
            RatioSettings = new MotorSettingCollection<float>(motorPorts.Length);
            for (int i = 0; i < motorPorts.Length; i++)
            {
                MotorPort port = motorPorts[i];
                Motors[port] = new Motor(port);
                SpeedSettings[port] = SByte.MaxValue;
                RatioSettings[port] = 1;
            }

            // 
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

        protected List<IRobotCommand> DequeueAll()
        {
            // clone the current command stack
            lock (_commands)
            {
                var temp = new List<IRobotCommand>(_commands);
                _commands.Clear();
                return temp;
            }
        }

        void Do(Action<Motor> action)
        {
            foreach (Motor motor in Motors)
                if (motor != null)
                    action(motor);
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
                Motors.Clear();
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

        protected void ResetTacho(MotorPort port)
        {
            lock (_commands)
                _commands.Clear();
            Motors[port].ResetTacho();
        }

        public WaitHandle CreateRotateTask(MotorPort port, int targetTacho)
        {
            Motor motor = Motors[port];            
            // current pos
            int motorTacho = motor.GetTachoCount();

            // get target value            
            targetTacho = (int)Math.Round(targetTacho * RatioSettings[port], MidpointRounding.AwayFromZero);
            int err = (targetTacho - motorTacho);
            if (err != 0)
            {
                sbyte speed = SpeedSettings[port];
                return motor.SpeedProfile((err > 0) ? speed : (sbyte)-speed, 0, (uint)Math.Abs(err), 0, true);
            }

            return _completedTask;
        }

        int _taskCounter = 0;
        public void Queue(WaitHandle taskHandle)
        {
            _motorTasks[_taskCounter++] = taskHandle;
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
                    // clear task list
                    for (int t = 0; t < _motorTasks.Length; t++)
                        _motorTasks[t] = _completedTask;
                    _taskCounter = 0;

                    // do it!
                    command.Do(this);

                    // wait for all motors to finish
                    WaitHandle.WaitAll(_motorTasks);

                    // anything to do after a command executed?
                    PostCommand();
                }
            }
        }

        protected virtual void PostCommand()
        {
            // does nothing
        }
    }
}
