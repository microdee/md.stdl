using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using md.stdl.Coding;

namespace md.stdl.Interaction.Notui
{
    public static class NotuiExtensions
    {
        /// <summary>
        /// Translate transformation with a delta
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="diff">Delta</param>
        public static void Translate(this ElementTransformation tr, Vector3 diff)
        {
            tr.Position += diff;
        }

        /// <summary>
        /// Resize transformation with a delta
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="diff">Delta</param>
        public static void Resize(this ElementTransformation tr, Vector3 diff)
        {
            tr.Scale += diff;
        }

        /// <summary>
        /// Rotate transformation with global delta pitch yaw roll
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="dPitchYawRoll">Delta pitch yaw roll</param>
        public static void GlobalRotate(this ElementTransformation tr, Vector3 dPitchYawRoll)
        {
            tr.Rotation = tr.Rotation * Quaternion.CreateFromYawPitchRoll(dPitchYawRoll.Y, dPitchYawRoll.X, dPitchYawRoll.Z);
        }

        /// <summary>
        /// Rotate transformation with local delta pitch yaw roll
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="dPitchYawRoll">Delta pitch yaw roll</param>
        public static void LocalRotate(this ElementTransformation tr, Vector3 dPitchYawRoll)
        {
            tr.Rotation = Quaternion.CreateFromYawPitchRoll(dPitchYawRoll.Y, dPitchYawRoll.X, dPitchYawRoll.Z) * tr.Rotation;
        }

        /// <summary>
        /// Rotate transformation with global delta quaternion
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="q">Delta quaternion</param>
        public static void GlobalRotate(this ElementTransformation tr, Quaternion q)
        {
            tr.Rotation = tr.Rotation * q;
        }

        /// <summary>
        /// Rotate transformation with local delta pitch yaw roll
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="q">Delta quaternion</param>
        public static void LocalRotate(this ElementTransformation tr, Quaternion q)
        {
            tr.Rotation = q * tr.Rotation;
        }

        /// <summary>
        /// Update an IElementCommon from another IElementCommon
        /// </summary>
        /// <param name="element">Receiving element, Can be a prototype or an instance</param>
        /// <param name="prototype">Reference element, Can be a prototype or an instance</param>
        /// <param name="selectivetr"></param>
        public static void UpdateCommon(this IElementCommon element, IElementCommon prototype, ApplyTransformMode selectivetr)
        {
            element.Id = prototype.Id;

            if (element is NotuiElement elinst)
            {
                if (prototype is ElementPrototype prot) elinst.Prototype = prot;
                if (prototype is NotuiElement el) elinst.Prototype = el.Prototype;
            }

            element.Name = prototype.Name;
            element.Active = prototype.Active;
            element.Transparent = prototype.Transparent;
            element.FadeOutTime = prototype.FadeOutTime;
            element.FadeInTime = prototype.FadeInTime;
            element.Behaviors = prototype.Behaviors;
            element.InteractionTransformation.UpdateFrom(prototype.InteractionTransformation, selectivetr);
            element.DisplayTransformation.UpdateFrom(prototype.DisplayTransformation, selectivetr);
        }
    }
}
