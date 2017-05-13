using EV3PrinterDriver.Commands;
using MonoBrickFirmware.Display;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Sensors;
using MonoBrickFirmware.UserInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace EV3PrinterDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            // for number conversion
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InstalledUICulture;

            ButtonEvents buttons = new ButtonEvents();
            TcpListener server = null;
            TcpClient client = null;
            bool run = true;
            buttons.EscapePressed += () =>
            {
                if (server != null)
                    server.Stop();
                if (client != null)
                    client.Close();

                run = false;
            };

            LcdConsole.WriteLine("EV3Printer 2.1");

            float mainRatio = 1.667f;
            float secondaryRatio = 3.0f;
            float handRatio = 1f;
#if DUMPLOGS
            StringBuilder logs = new StringBuilder();
#endif
            using (PrinterRobot robot = new PrinterRobot() { })
            {
                robot.RatioSettings[RobotSetup.XPort] = mainRatio;
                robot.RatioSettings[RobotSetup.YPort] = secondaryRatio;
                robot.RatioSettings[RobotSetup.PenPort] = handRatio;

                EV3TouchSensor sensor1 = new EV3TouchSensor(RobotSetup.XResetPort);
                
                int pause = 0;
                buttons.EnterReleased += () =>
                {
                    pause++;
                };
                int currentPause = 0;

                LcdConsole.WriteLine("Calibrating...");

                robot.Calibrate(step =>
                {
                    if (!run) return true;
                    switch (step)
                    {
                        case PrinterRobot.CalibrationSteps.X:
                            if (sensor1.IsPressed())
                                return true;
                            break;

                        case PrinterRobot.CalibrationSteps.Pause:
                            if (currentPause != pause)
                            {
                                currentPause = pause;
                                return true;
                            }
                            break;
                    }
                    /*
                    Lcd.Clear();
                    int line = 0;
                    Lcd.WriteText(Font.MediumFont, new Point(0, line), "Calibrating...", true);
                    line += (int)(Font.MediumFont.maxHeight);
                    Lcd.WriteText(Font.MediumFont, new Point(0, line), string.Format("Refl.: {0} / {1}", sensor2.ReadRaw(), sensor2max), true);
                    line += (int)(Font.MediumFont.maxHeight);
                    Lcd.WriteText(Font.MediumFont, new Point(0, line), string.Format("A: {0}", motors.GetRawTacho(MotorPort.OutA)), true);
                    line += (int)(Font.MediumFont.maxHeight);
                    Lcd.WriteText(Font.MediumFont, new Point(0, line), string.Format("B: {0}", motors.GetRawTacho(MotorPort.OutB)), true);
                    line += (int)(Font.MediumFont.maxHeight);
                    Lcd.Update();
                    */

                    return false;
                });
            }
            // stop here
            if (!run)
                return;

            // main loop
            LcdConsole.WriteLine("Starting...");

            try
            {
                using (PrinterRobot robot = new PrinterRobot() { })
                {
                    // Set the TcpListener on port 13000.
                    Int32 port = 13000;

                    // TcpListener server = new TcpListener(port);
                    server = new TcpListener(IPAddress.Any, port);

                    // Start listening for client requests.
                    server.Start();

                    // Buffer for reading data
                    Byte[] bytes = new Byte[256];

                    // Enter the listening loop.
                    while (run)
                    {
                        LcdConsole.WriteLine("Waiting for a connection... ");

                        // Perform a blocking call to accept requests.
                        // You could also user server.AcceptSocket() here.
                        client = server.AcceptTcpClient();
                        LcdConsole.WriteLine("Connected!");

#if DUMPLOGS
                        // DEBUG
                        byte[] logBuffer = Encoding.ASCII.GetBytes(logs.ToString());
                        client.GetStream().Write(logBuffer, 0, logBuffer.Length);
#endif
                        // apply settings
                        robot.RatioSettings[RobotSetup.XPort] = mainRatio;
                        robot.RatioSettings[RobotSetup.YPort] = secondaryRatio;
                        robot.RatioSettings[RobotSetup.PenPort] = handRatio;

                        robot.SpeedSettings[RobotSetup.XPort] = 64;
                        robot.SpeedSettings[RobotSetup.YPort] = 113; // adjust Y motor speed as X motor produces almost twice as much speed (0.56)
                        robot.SpeedSettings[RobotSetup.PenPort] = 127;

                        string data = null;

                        // Get a stream object for reading and writing
                        NetworkStream stream = client.GetStream();

                        int read;
                        string message = "";
                        // Loop to receive all the data sent by the client.
                        while (run && (read = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            // Translate data bytes to a ASCII string.
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, read);
                            for (int i = 0; i < read; i++)
                            {
                                char c = data[i];
                                if (c != '\0')
                                    message += c;
                                else
                                {
                                    // get message type
                                    IRobotCommand command = RobotCommandFactory.Create(message);
                                    if (command != null)
                                        robot.Queue(command);
                                    
                                    //
                                    Lcd.Clear();
                                    int line = 0;
                                    Lcd.WriteText(Font.MediumFont, new Point(0, line), string.Format("X:{0}", robot.Motors[RobotSetup.XPort].GetTachoCount()), true);
                                    line += (int)(Font.MediumFont.maxHeight);
                                    Lcd.WriteText(Font.MediumFont, new Point(0, line), string.Format("Y:{0}", robot.Motors[RobotSetup.YPort].GetTachoCount()), true);
                                    line += (int)(Font.MediumFont.maxHeight);
                                    Lcd.Update();

                                    // 
                                    message = "";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }
    }
}

