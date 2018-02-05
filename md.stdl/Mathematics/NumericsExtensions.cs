using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VVVV.Utils.VMath;
using VMatrix = VVVV.Utils.VMath.Matrix4x4;
using SMatrix = System.Numerics.Matrix4x4;

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

        public static Vector2D AsVVector(this Vector2 v) { return new Vector2D(v.X, v.Y); }
        public static Vector3D AsVVector(this Vector3 v) { return new Vector3D(v.X, v.Y, v.Z); }
        public static Vector4D AsVVector(this Vector4 v) { return new Vector4D(v.X, v.Y, v.Z, v.W); }

        public static Vector2 AsSystemVector(this Vector2D v) { return new Vector2((float)v.x, (float)v.y); }
        public static Vector3 AsSystemVector(this Vector3D v) { return new Vector3((float)v.x, (float)v.y, (float)v.z); }
        public static Vector4 AsSystemVector(this Vector4D v) { return new Vector4((float)v.x, (float)v.y, (float)v.z, (float)v.w); }
        public static Quaternion AsSystemQuaternion(this Vector4D v) { return new Quaternion((float)v.x, (float)v.y, (float)v.z, (float)v.w); }

        public static VMatrix AsVMatrix4X4(this SMatrix m)
        {
            return new VMatrix(
                m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24,
                m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44);
        }

        public static SMatrix AsSystemMatrix4X4(this VMatrix m)
        {
            return new SMatrix(
                (float)m.m11, (float)m.m12, (float)m.m13, (float)m.m14,
                (float)m.m21, (float)m.m22, (float)m.m23, (float)m.m24,
                (float)m.m31, (float)m.m32, (float)m.m33, (float)m.m34,
                (float)m.m41, (float)m.m42, (float)m.m43, (float)m.m44);
        }
    }
}
