using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace md.stdl.Mathematics
{
    public static class Coordinates
    {
        /// <summary>
        /// Rectangular/Cartesian to polar coordinates
        /// </summary>
        /// <param name="cart">Cartesian coordinates</param>
        /// <returns>A vector where X is angle and Y is distance</returns>
        public static Vector2 RectToPolar(Vector2 cart)
        {
            var angle = (float)Atan2(cart.Y, cart.X);
            var dist = cart.Length();
            return new Vector2(angle, dist);
        }
        public static Vector2 PolarToRect(Vector2 polar)
        {
            return new Vector2(
                (float)Cos(polar.X) * polar.Y,
                (float)Sin(polar.X) * polar.Y
            );
        }

        public static void GetPointWorldPosDir(Vector2 point, Matrix4x4 invaspproj, Matrix4x4 invview, out Vector3 worldpos, out Vector3 viewdir)
        {
            var tpw = Vector4.Transform(new Vector4(point, 0, 1), invaspproj * invview);
            var tpdw = Vector4.Transform(new Vector4(point, 1, 1), invaspproj * invview);
            worldpos = tpw.xyz() / tpw.W;
            viewdir = Vector3.Normalize(tpdw.xyz() / tpdw.W - tpw.xyz() / tpw.W);
        }
    }
}
