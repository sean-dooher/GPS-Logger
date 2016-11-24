using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace GPS_Scanner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;
            StreamWriter writer = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\GPSLogger-" + DateTime.Now.ToUniversalTime().Date.ToString("yyyy-dd-MM") + ".txt",true);
            SerialPort GPS = ScanForGPS();

            if (GPS == null)
            {
                Console.Write("Please plug in the GPS or close any other program using the port");
            }
            else
            {
                Console.WriteLine("Now using {0}", GPS.PortName);

                Console.OutputEncoding = GPS.Encoding;
                List<Tuple<Coordinate, DateTime>> coordinates = new List<Tuple<Coordinate, DateTime>>();

                bool exit = false;
                DateTime check = DateTime.Now;
                while (!exit)
                {
                    if ((DateTime.Now - check).Seconds >= 5)
                    {
                        
                        Coordinate test = Coordinate.GetCoordinates(GPS);
                        coordinates.Add(new Tuple<Coordinate, DateTime>(test, DateTime.Now));
                        writer.Write(coordinates.Last().Item1.Latitude + ":" + coordinates.Last().Item1.Longitude + ":" + coordinates.Last().Item2.ToFileTimeUtc() + ",");
                        writer.Flush();
                        check = DateTime.Now;
                    }
                    if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.Q)
                    {
                        exit = true;
                        writer.Close();
                    }
                    if ((DateTime.Now - check).Milliseconds % 1000 <= 10)
                    {
                        Console.Clear();
                        Console.WriteLine("Time until next coordinate: {0}s", 5 - (DateTime.Now - check).Seconds);
                        foreach (Tuple<Coordinate, DateTime> tuple in coordinates)
                            Console.WriteLine(tuple.Item1.Latitude.DecimalDegrees + ", " + tuple.Item1.Longitude.DecimalDegrees);
                    }
                        
                }
            }
        }

        static SerialPort ScanForGPS()
        {
            SerialPort GPS = null;
            //scan and detect GPS
            foreach (string name in SerialPort.GetPortNames())
            {
                Console.WriteLine("Scanning Port: {0}", name);
                SerialPort test = new SerialPort(name, 4800, Parity.None, 8);
                test.ReadTimeout = 1000;
                try
                {
                    test.Open();
                    for (int i = 0; i < 50; i++)
                    {
                        Console.Clear();
                        Console.WriteLine("Scanning Port: {0} \n Try: {1}/50", name, i + 1);
                        if (test.ReadLine().StartsWith("$GP"))
                        {
                            GPS = test;
                            i = 50;
                            test.Close();
                        }
                    }
                }
                catch
                {
                    test.Close();
                }
            }
            Console.Clear();
            return GPS;
        }
    }
}
