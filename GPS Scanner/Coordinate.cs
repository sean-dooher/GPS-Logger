using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO.Ports;

namespace GPS_Scanner
{
    public class Coordinate
    {
        #region Enums
        public enum CoordinateStyle
        { DegreeDecimal, DegreeMinuteDecimal, DegreeMinuteSecond }
        #endregion

        #region Properties
        public CoordinateComponent Latitude
        {
            get;
            private set;
        }
        public CoordinateComponent Longitude
        {
            get;
            private set;
        }
        #endregion

        #region Variables
        #endregion

        #region Constructors
        public Coordinate(double lat, CoordinateComponent.CardinalDirection latDir, double lon, CoordinateComponent.CardinalDirection longDir):
            this(new CoordinateComponent(lat,latDir),new CoordinateComponent(lon,longDir)) { }

        public Coordinate(double lat, double lon):
            this(new CoordinateComponent(lat,CoordinateComponent.ComponentType.Latitude), new CoordinateComponent(lon, CoordinateComponent.ComponentType.Longitude)) { }

        public Coordinate(CoordinateComponent lat, CoordinateComponent lon)
        {
            Latitude = lat;
            Longitude = lon;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Retreives a GPS Coordinate from the specified GPS
        /// </summary>
        /// <param name="GPS">SerialPort that represents GPS</param>
        public static Coordinate GetCoordinates(SerialPort GPS)
        {
            if (!GPS.IsOpen)
                GPS.Open();
            string gpsReadout = String.Empty;
            while (!gpsReadout.StartsWith("$GPGGA") && !gpsReadout.StartsWith("$GPRMC"))
            {
                gpsReadout = GPS.ReadLine();
            }
            GPS.Close();

            double latitude;
            CoordinateComponent.CardinalDirection longDir;
            double longitude;
            CoordinateComponent.CardinalDirection latDir;
            string[] readout = gpsReadout.Split(',');

            try
            {
                if (gpsReadout.StartsWith("$GPGGA"))
                {
                    latitude = CoordinateComponent.DegreeMinutesToDecimal(double.Parse(readout[2]));
                    latDir = (CoordinateComponent.CardinalDirection)readout[3][0];
                    longitude = CoordinateComponent.DegreeMinutesToDecimal(double.Parse(readout[4]));
                    longDir = (CoordinateComponent.CardinalDirection)readout[5][0];
                }
                else
                {
                    latitude = CoordinateComponent.DegreeMinutesToDecimal(double.Parse(readout[3]));
                    latDir = (CoordinateComponent.CardinalDirection)readout[4][0];
                    longitude = CoordinateComponent.DegreeMinutesToDecimal(double.Parse(readout[5]));
                    longDir = (CoordinateComponent.CardinalDirection)readout[6][0];
                }
                return new Coordinate(latitude, latDir, longitude, longDir);
            }
            catch
            {
                return GetCoordinates(GPS);
            }

        }

        public override string ToString()
        {
            return ToString(CoordinateStyle.DegreeDecimal);
        }
        public string ToString(Coordinate.CoordinateStyle style)
        {
            return Latitude.ToString(style) + ", " + Longitude.ToString(style);
        }
        #endregion
    }

    public class CoordinateComponent
    {
        #region Enums
        public enum ComponentType
        { Latitude, Longitude }
        public enum CardinalDirection
        { North='N',East='E',South='S',West='W' }
        #endregion
        #region Properties

        /// <summary>
        /// Gets whether the component is latitude or longitude
        /// </summary>
        public ComponentType DirectionType
        {
            get;
            private set;
        }

        public CardinalDirection Direction
        {
            get;
            private set;
        }

        public double DecimalDegrees
        {
            get
            { return lat; }
        }

        public int Degrees
        {
            get
            {
                return (int)Math.Abs(lat);
            }

        }

        public int Minutes
        {
            get
            {
                return (int)Math.Abs(lat % 1 * 60);
            }
        }

        public double Seconds
        {
            get
            {
                return Math.Round((Math.Abs(lat % 1) * 60 - Minutes) * 60, 2);
            }
        }
        #endregion
        #region Variables
        private double lat;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new CoordinateComponent object with specified degrees, direction and style
        /// </summary>
        /// <param name="degrees">Latitude/Longitude in decimal degree form</param>
        public CoordinateComponent(double degrees, ComponentType type)
        {
            lat = degrees;
            DirectionType = type;
            if (type == ComponentType.Latitude && Math.Sign(lat) == -1)
                Direction = CardinalDirection.South;
            else if (type == ComponentType.Latitude)
                Direction = CardinalDirection.North;
            else if (type == ComponentType.Longitude && Math.Sign(lat) == -1)
                Direction = CardinalDirection.West;
            else if (type == ComponentType.Longitude)
                Direction = CardinalDirection.East;
        }

        public CoordinateComponent(double degrees, CardinalDirection direction)
        {
            lat = (direction == CardinalDirection.South || direction == CardinalDirection.West ? -1 : 1) * degrees;
            Direction = direction;
            if (direction == CardinalDirection.North || direction == CardinalDirection.South)
                DirectionType = ComponentType.Latitude;
            else
                DirectionType = ComponentType.Longitude;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Converts a coordinate in the form of DDDMM.mmmm to DDD.dddd
        /// </summary>
        /// <param name="degreeMinutes">Coordinate in the form of DDDMM.mmmm </param>
        /// <returns>Coordinate in DDD.dddd form</returns>
        public static double DegreeMinutesToDecimal(double degreeMinutes)
        {
            return (int)degreeMinutes / 100 + Math.Round(degreeMinutes % 100 / 60, 4);
        }
      
        public override string ToString()
        {
            return ToString(Coordinate.CoordinateStyle.DegreeDecimal);
        }

        /// <summary>
        /// Formats a latitude/longitude in a certain style
        /// </summary>
        /// <param name="style">Style to return in</param>
        /// <returns>String of latitude/longitude in style</returns>
        public string ToString(Coordinate.CoordinateStyle style)
        {
            switch (style)
            {
                case Coordinate.CoordinateStyle.DegreeDecimal:
                    return Math.Abs(DecimalDegrees).ToString() + (char)Direction;
                case Coordinate.CoordinateStyle.DegreeMinuteDecimal:
                    return Degrees + "° " + Math.Round((DecimalDegrees % 100), 4) + "\" " + (char)Direction;
                case Coordinate.CoordinateStyle.DegreeMinuteSecond:
                    return Degrees + "° " + Minutes + "\" " + Seconds + "\' " + (char)Direction;
                default:
                    return Math.Abs(DecimalDegrees).ToString() + (char)Direction;
            }
        }
        #endregion
    }
}
