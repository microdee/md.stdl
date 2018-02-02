using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace md.stdl.Mathematics
{
    public static class NumericsExtensions
    {
        public static Vector2 xy(this Vector3 a) { return new Vector2(a.X, a.Y); }
        public static Vector2 yx(this Vector3 a) { return new Vector2(a.Y, a.X); }
        public static Vector2 xz(this Vector3 a) { return new Vector2(a.X, a.Z); }
        public static Vector2 zx(this Vector3 a) { return new Vector2(a.Z, a.X); }
        public static Vector2 yz(this Vector3 a) { return new Vector2(a.Y, a.Z); }
        public static Vector2 zy(this Vector3 a) { return new Vector2(a.Z, a.Y); }

        public static Vector2 xy(this Vector4 a) { return new Vector2(a.X, a.Y); }
        public static Vector2 yx(this Vector4 a) { return new Vector2(a.Y, a.X); }
        public static Vector2 xz(this Vector4 a) { return new Vector2(a.X, a.Z); }
        public static Vector2 zx(this Vector4 a) { return new Vector2(a.Z, a.X); }
        public static Vector2 yz(this Vector4 a) { return new Vector2(a.Y, a.Z); }
        public static Vector2 zy(this Vector4 a) { return new Vector2(a.Z, a.Y); }

        public static Vector2 xw(this Vector4 a) { return new Vector2(a.X, a.W); }
        public static Vector2 wx(this Vector4 a) { return new Vector2(a.W, a.X); }
        public static Vector2 wz(this Vector4 a) { return new Vector2(a.W, a.Z); }
        public static Vector2 zw(this Vector4 a) { return new Vector2(a.Z, a.W); }
        public static Vector2 yw(this Vector4 a) { return new Vector2(a.Y, a.W); }
        public static Vector2 wy(this Vector4 a) { return new Vector2(a.W, a.Y); }
        
        public static Vector3 xyz(this Vector4 a) { return new Vector3(a.X, a.Y, a.Z); }
        public static Vector3 xzy(this Vector4 a) { return new Vector3(a.X, a.Z, a.Y); }
        public static Vector3 yxz(this Vector4 a) { return new Vector3(a.Y, a.X, a.Z); }
        public static Vector3 yzx(this Vector4 a) { return new Vector3(a.Y, a.X, a.Z); }
        public static Vector3 zxy(this Vector4 a) { return new Vector3(a.Z, a.X, a.Y); }
        public static Vector3 zyx(this Vector4 a) { return new Vector3(a.Z, a.Y, a.X); }

        public static Vector3 xyw(this Vector4 a) { return new Vector3(a.X, a.Y, a.W); }
        public static Vector3 xwy(this Vector4 a) { return new Vector3(a.X, a.W, a.Y); }
        public static Vector3 yxw(this Vector4 a) { return new Vector3(a.Y, a.X, a.W); }
        public static Vector3 ywx(this Vector4 a) { return new Vector3(a.Y, a.X, a.W); }
        public static Vector3 wxy(this Vector4 a) { return new Vector3(a.W, a.X, a.Y); }
        public static Vector3 wyx(this Vector4 a) { return new Vector3(a.W, a.Y, a.X); }

        public static Vector3 xwz(this Vector4 a) { return new Vector3(a.X, a.W, a.Z); }
        public static Vector3 xzw(this Vector4 a) { return new Vector3(a.X, a.Z, a.W); }
        public static Vector3 wxz(this Vector4 a) { return new Vector3(a.W, a.X, a.Z); }
        public static Vector3 wzx(this Vector4 a) { return new Vector3(a.W, a.X, a.Z); }
        public static Vector3 zxw(this Vector4 a) { return new Vector3(a.Z, a.X, a.W); }
        public static Vector3 zwx(this Vector4 a) { return new Vector3(a.Z, a.W, a.X); }

        public static Vector3 wyz(this Vector4 a) { return new Vector3(a.W, a.Y, a.Z); }
        public static Vector3 wzy(this Vector4 a) { return new Vector3(a.W, a.Z, a.Y); }
        public static Vector3 ywz(this Vector4 a) { return new Vector3(a.Y, a.W, a.Z); }
        public static Vector3 yzw(this Vector4 a) { return new Vector3(a.Y, a.W, a.Z); }
        public static Vector3 zwy(this Vector4 a) { return new Vector3(a.Z, a.W, a.Y); }
        public static Vector3 zyw(this Vector4 a) { return new Vector3(a.Z, a.Y, a.W); }
    }
}
