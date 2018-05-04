using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VVVV.Utils.VMath;

namespace md.stdl.Mathematics
{
    /// <summary>
    /// Simple angle mode
    /// </summary>
    public enum AngleMode
    {
        Degrees,
        Radians,
        Cycles
    }

    /// <summary>
    /// Sphere related functions
    /// </summary>
    public static class Sphere
    {
        /// <summary>
        /// Distance between 2 points on the surface of a sphere.
        /// </summary>
        /// <param name="longlat1">First point in longitude / latitude</param>
        /// <param name="longlat2">Second point in longitude / latitude</param>
        /// <param name="R">Radius of the sphere (default is the radius of Earth in meters)</param>
        /// <param name="anglemode">Angle unit mode of the input</param>
        /// <returns>The distance value</returns>
        public static double DistanceOnSurface(Vector2 longlat1, Vector2 longlat2, double R = 6371e3, AngleMode anglemode = AngleMode.Degrees)
        {
            var lon1 = longlat1.X;
            var lat1 = longlat1.Y;
            var lon2 = longlat2.X;
            var lat2 = longlat2.Y;

            var am = 1.0;
            switch (anglemode)
            {
                case AngleMode.Degrees:
                    am = VMath.DegToRad;
                    break;
                case AngleMode.Cycles:
                    am = VMath.CycToRad;
                    break;
            }
            
            var φ1 = lat1 * am;
            var φ2 = lat2 * am;
            var Δφ = (lat2 - lat1) * am;
            var Δλ = (lon2 - lon1) * am;

            var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                    Math.Cos(φ1) * Math.Cos(φ2) *
                    Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }
    }
}
