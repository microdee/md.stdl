﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Math;


namespace md.stdl.Mathematics
{
    public static class Filters
    {
        #region Lowpass
        /// <summary>
        /// Simple static low pass filter
        /// </summary>
        /// <param name="previous">The previous value</param>
        /// <param name="target">The target to be reached</param>
        /// <param name="alpha">1: Don't filter at all; 0: Filter everything</param>
        /// <returns></returns>
        public static float Lowpass(float previous, float target, float alpha)
        {
            float a = Min(Max(0, alpha), 1);
            return alpha * target + (1 - alpha) * previous;
        }
        /// <summary>
        /// Simple static low pass filter for Vector2
        /// </summary>
        /// <param name="previous">The previous value</param>
        /// <param name="target">The target to be reached</param>
        /// <param name="alpha">1: Don't filter at all; 0: Filter everything</param>
        /// <returns></returns>
        public static Vector2 Lowpass(Vector2 previous, Vector2 target, Vector2 alpha)
        {
            var a = Vector2.Min(Vector2.Max(Vector2.Zero, alpha), Vector2.One);
            return alpha * target + (Vector2.One - alpha) * previous;
        }
        /// <summary>
        /// Simple static low pass filter for Vector3
        /// </summary>
        /// <param name="previous">The previous value</param>
        /// <param name="target">The target to be reached</param>
        /// <param name="alpha">1: Don't filter at all; 0: Filter everything</param>
        /// <returns></returns>
        public static Vector3 Lowpass(Vector3 previous, Vector3 target, Vector3 alpha)
        {
            var a = Vector3.Min(Vector3.Max(Vector3.Zero, alpha), Vector3.One);
            return alpha * target + (Vector3.One - alpha) * previous;
        }
        /// <summary>
        /// Simple static low pass filter for Vector4
        /// </summary>
        /// <param name="previous">The previous value</param>
        /// <param name="target">The target to be reached</param>
        /// <param name="alpha">1: Don't filter at all; 0: Filter everything</param>
        /// <returns></returns>
        public static Vector4 Lowpass(Vector4 previous, Vector4 target, Vector4 alpha)
        {
            var a = Vector4.Min(Vector4.Max(Vector4.Zero, alpha), Vector4.One);
            return alpha * target + (Vector4.One - alpha) * previous;
        }
        /// <summary>
        /// Simple static low pass filter for Vector4
        /// </summary>
        /// <param name="previous">The previous value</param>
        /// <param name="target">The target to be reached</param>
        /// <param name="alpha">1: Don't filter at all; 0: Filter everything</param>
        /// <returns></returns>
        public static Quaternion Lowpass(Quaternion previous, Quaternion target, float alpha)
        {
            float a = Min(Max(0, alpha), 1);
            return Quaternion.Slerp(previous, target, a);
        }
        #endregion

        #region Velocity
        /// <summary>
        /// Simple velocity driven filter
        /// </summary>
        /// <param name="prevpos">Previous position</param>
        /// <param name="target">Target position</param>
        /// <param name="velocity">Maximum velocity</param>
        /// <returns>New position</returns>
        public static float Velocity(float prevpos, float target, float velocity)
        {
            var d = target - prevpos;
            return prevpos + Min(Abs(d), velocity) * Sign(d);
        }
        /// <summary>
        /// Simple velocity driven filter for Vector2
        /// </summary>
        /// <param name="prevpos">Previous position</param>
        /// <param name="target">Target position</param>
        /// <param name="velocity">Maximum velocity</param>
        /// <returns>New position</returns>
        public static Vector2 Velocity(Vector2 prevpos, Vector2 target, float velocity)
        {
            var d = target - prevpos;
            return prevpos + Vector2.Normalize(d) * Min(velocity, d.Length());
        }
        /// <summary>
        /// Simple velocity driven filter for Vector3
        /// </summary>
        /// <param name="prevpos">Previous position</param>
        /// <param name="target">Target position</param>
        /// <param name="velocity">Maximum velocity</param>
        /// <returns>New position</returns>
        public static Vector3 Velocity(Vector3 prevpos, Vector3 target, float velocity)
        {
            var d = target - prevpos;
            return prevpos + Vector3.Normalize(d) * Min(velocity, d.Length());
        }
        /// <summary>
        /// Simple velocity driven filter for Vector4
        /// </summary>
        /// <param name="prevpos">Previous position</param>
        /// <param name="target">Target position</param>
        /// <param name="velocity">Maximum velocity</param>
        /// <returns>New position</returns>
        public static Vector4 Velocity(Vector4 prevpos, Vector4 target, float velocity)
        {
            var d = target - prevpos;
            return prevpos + Vector4.Normalize(d) * Min(velocity, d.Length());
        }
        /// <summary>
        /// Simple velocity driven filter for Quaternion
        /// </summary>
        /// <param name="prevpos">Previous position</param>
        /// <param name="target">Target position</param>
        /// <param name="velocity">Maximum velocity</param>
        /// <returns>New position</returns>
        public static Quaternion Velocity(Quaternion prevpos, Quaternion target, float velocity)
        {
            var d = target - prevpos;
            return prevpos + Quaternion.Normalize(d) * Min(velocity, d.Length());
        }
        #endregion

        #region Inertial
        /// <summary>
        /// Inertial, force driven filter
        /// </summary>
        /// <param name="prevpos">Previous position</param>
        /// <param name="prevvel">Previous velocity</param>
        /// <param name="target">Target position</param>
        /// <param name="force">Maximum force</param>
        /// <param name="newpos">New position</param>
        /// <param name="newvel">New velocity</param>
        public static void Inertial(float prevpos, float prevvel, float target, float force, out float newpos, out float newvel)
        {
            var d = target - prevpos;
            var f = Min(Abs(d), force)*Sign(d);
            newvel = prevvel + f;
            newpos = prevpos + newvel;
        }
        /// <summary>
        /// Inertial, force driven filter for Vector2
        /// </summary>
        /// <param name="prevpos">Previous position</param>
        /// <param name="prevvel">Previous velocity</param>
        /// <param name="target">Target position</param>
        /// <param name="force">Maximum force</param>
        /// <param name="newpos">New position</param>
        /// <param name="newvel">New velocity</param>
        public static void Inertial(Vector2 prevpos, Vector2 prevvel, Vector2 target, float force, out Vector2 newpos, out Vector2 newvel)
        {
            var d = target - prevpos;
            var f = Vector2.Normalize(d) * Min(force, d.Length());
            newvel = prevvel + f;
            newpos = prevpos + newvel;
        }
        /// <summary>
        /// Inertial, force driven filter for Vector3
        /// </summary>
        /// <param name="prevpos">Previous position</param>
        /// <param name="prevvel">Previous velocity</param>
        /// <param name="target">Target position</param>
        /// <param name="force">Maximum force</param>
        /// <param name="newpos">New position</param>
        /// <param name="newvel">New velocity</param>
        public static void Inertial(Vector3 prevpos, Vector3 prevvel, Vector3 target, float force, out Vector3 newpos, out Vector3 newvel)
        {
            var d = target - prevpos;
            var f = Vector3.Normalize(d) * Min(force, d.Length());
            newvel = prevvel + f;
            newpos = prevpos + newvel;
        }
        /// <summary>
        /// Inertial, force driven filter for Vector4
        /// </summary>
        /// <param name="prevpos">Previous position</param>
        /// <param name="prevvel">Previous velocity</param>
        /// <param name="target">Target position</param>
        /// <param name="force">Maximum force</param>
        /// <param name="newpos">New position</param>
        /// <param name="newvel">New velocity</param>
        public static void Inertial(Vector4 prevpos, Vector4 prevvel, Vector4 target, float force, out Vector4 newpos, out Vector4 newvel)
        {
            var d = target - prevpos;
            var f = Vector4.Normalize(d) * Min(force, d.Length());
            newvel = prevvel + f;
            newpos = prevpos + newvel;
        }
        /// <summary>
        /// Inertial, force driven filter for Quaternion
        /// </summary>
        /// <param name="prevpos">Previous position</param>
        /// <param name="prevvel">Previous velocity</param>
        /// <param name="target">Target position</param>
        /// <param name="force">Maximum force</param>
        /// <param name="newpos">New position</param>
        /// <param name="newvel">New velocity</param>
        public static void Inertial(Quaternion prevpos, Quaternion prevvel, Quaternion target, float force, out Quaternion newpos, out Quaternion newvel)
        {
            var d = target - prevpos;
            var f = Quaternion.Normalize(d) * Min(force, d.Length());
            newvel = prevvel + f;
            newpos = prevpos + newvel;
        }
        #endregion
    }
}