using EV3PrinterDriver.Commands;
using EV3PrinterDriver.Robots;
using MonoBrickFirmware.Display;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.Sensors;
using MonoBrickFirmware.UserInput;
using System;
using System.Collections.Generic;
using System.IO;
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

            LcdConsole.WriteLine("EV3Scanner 3.0");

            float mainRatio = 1.667f;
            float secondaryRatio = 3.0f;
            float handRatio = 1f;
#if DUMPLOGS
            StringBuilder logs = new StringBuilder();
#endif
            // stop here
            if (!run)
                return;

            // main loop
            LcdConsole.WriteLine("Starting...");

            try
            {
                using (IRobot robot = new ScannerRobot() { })
                {
                    // apply settings
                    robot.RatioSettings[RobotSetup.XPort] = mainRatio;
                    robot.RatioSettings[RobotSetup.YPort] = secondaryRatio;
                    robot.RatioSettings[RobotSetup.PenPort] = handRatio;

                    // printer
                    // robot.SpeedSettings[RobotSetup.XPort] = 64;
                    // robot.SpeedSettings[RobotSetup.YPort] = 113; // adjust Y motor speed as X motor produces almost twice as much speed (0.56)
                    // robot.SpeedSettings[RobotSetup.PenPort] = 127;

                    // scanner
                    robot.SpeedSettings[RobotSetup.XPort] = 16;
                    robot.SpeedSettings[RobotSetup.YPort] = 113; // adjust Y motor speed as X motor produces almost twice as much speed (0.56)
                    robot.SpeedSettings[RobotSetup.PenPort] = 127;

                    // calibrate robot
                    LcdConsole.WriteLine("Calibrating...");
                    robot.Calibrate(() => { return !run; });

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

                        // blinking green
                        Buttons.LedPattern(4);

                        // Perform a blocking call to accept requests.
                        // You could also user server.AcceptSocket() here.
                        client = server.AcceptTcpClient();
                        LcdConsole.WriteLine("Connected!");

                        // turn off
                        Buttons.LedPattern(0);

                        // Get a stream object for reading and writing
                        NetworkStream stream = client.GetStream();

                        // if robot is sending data, publish to network channel
                        EventHandler<DataEventArgs> dataCB = (o, e) => {
                            try
                            {
                                byte[] msg = System.Text.Encoding.ASCII.GetBytes(e.Data);
                                stream.Write(msg, 0, msg.Length);
                            }
                            catch(IOException)
                            {
                                // disconnected
                                robot.Off();
                            }
                        };

                        robot.OnData += dataCB;

#if DUMPLOGS
                        // DEBUG
                        byte[] logBuffer = Encoding.ASCII.GetBytes(logs.ToString());
                        client.GetStream().Write(logBuffer, 0, logBuffer.Length);
#endif
                        try
                        {
                            string data = null;

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
                                        message = "";
                                    }
                                }
                            }
                        }
                        catch(IOException)
                        {
                            LcdConsole.Clear();
                            LcdConsole.WriteLine("Disconnected!");

                            // stop sending data
                            robot.OnData -= dataCB;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                /*
#if DEBUG
                // Set the TcpListener on port 13000.
                int port = 13000;

                // TcpListener server = new TcpListener(port);
                TcpListener debugServer = new TcpListener(IPAddress.Any, port);

                // Start listening for client requests.
                debugServer.Start();

                LcdConsole.WriteLine("Wait debugger");

                // Perform a blocking call to accept requests.
                // You could also user server.AcceptSocket() here.
                using (TcpClient debugClient = debugServer.AcceptTcpClient())
                {
                    // Get a stream object for reading and writing
                    NetworkStream stream = debugClient.GetStream();

                    foreach (String it in Assembly.GetExecutingAssembly().GetManifestResourceNames())
                    {
                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(it + "\n");
                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                    }

                    while (ex != null)
                    {
                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(ex.Message + "\n");
                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);

                        msg = System.Text.Encoding.ASCII.GetBytes(ex.StackTrace + "\n");
                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        ex = ex.InnerException;
                    }
                }

                // Stop listening for new clients.
                debugServer.Stop();
#else
                throw;
#endif
        */
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

