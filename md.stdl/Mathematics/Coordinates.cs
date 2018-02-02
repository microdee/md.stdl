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
    }
}
