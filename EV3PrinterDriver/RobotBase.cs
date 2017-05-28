using EV3PrinterDriver.Collections;
using EV3PrinterDriver.Commands;
using EV3PrinterDriver.Robots;
using MonoBrickFirmware.Display;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Sensors;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EV3PrinterDriver
{
    abstract class RobotBase : IRobot
    {
        bool disposed = false;
        static readonly ManualResetEvent _completedTask = new ManualResetEvent(true);
        public MotorSettingCollection<Motor> Motors { get; private set; }
        public MotorSettingCollection<sbyte> SpeedSettings { get; private set; }
        public MotorSettingCollection<float> RatioSettings { get; private set; }

        BlockingCollection<IRobotCommand> _commands = new BlockingCollection<IRobotCommand>();

        Thread _thread;
        CancellationTokenSource _cancelSrc = new CancellationTokenSource();
        CancellationToken _cancel;

        public RobotBase(MotorPort[] motorPorts)
        {
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
            _cancel = _cancelSrc.Token;
            _thread = new Thread(MotorPollThread);
            _thread.IsBackground = true;
            _thread.Start();
        }

        public void Queue(IRobotCommand command)
        {
            _commands.Add(command);
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
        public virtual void Off()
        {
            // clear any pending commands
            var oldCommands = _commands;
            _commands = new BlockingCollection<IRobotCommand>();
            oldCommands.CompleteAdding(); // prevent any new commands
            oldCommands.Dispose();

            // 
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
                _cancelSrc.Cancel();
                _thread.Join();
                _commands.Dispose();
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
            {
                Do((m) => m.ResetTacho());
            }
        }

        protected void ResetTacho(MotorPort port)
        {
            lock (_commands)
            {
                Motors[port].ResetTacho();
            }
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

        public event EventHandler<DataEventArgs> OnData;
        protected void SendData(String data)
        {
            OnData?.Invoke(this, new DataEventArgs(data));
        }


        void MotorPollThread()
        {
            try
            {
                while (true)
                {
                    // take all pending comamnds
                    IRobotCommand command = null;
                    if (_commands.TryTake(out command, Timeout.Infinite, _cancel))
                    {
                        // do it!
                        command.Do(this);

                        // anything to do after a command executed?
                        PostCommand();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // normal exception when thread is stopped
            }
            catch (Exception ex)
            {
                // 
                while (ex != null)
                {
                    LcdConsole.WriteLine(ex.Message);
                    ex = ex.InnerException;
                }
                throw;
            }
        }

        protected virtual void PostCommand()
        {
            // does nothing
        }

        public abstract void Calibrate(Func<bool> stop);
    }
}
