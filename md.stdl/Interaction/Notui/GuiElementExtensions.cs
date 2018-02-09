using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using md.stdl.Coding;

namespace md.stdl.Interaction.Notui
{
    public static class GuiElementExtensions
    {
        /// <summary>
        /// Pure function for getting the matrix of the display transformation
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static Matrix4x4 GetDisplayTransform(this IGuiElement element)
        {
            var parent = Matrix4x4.Identity;
            if (element.Parent != null) parent = element.Parent.GetDisplayTransform();
            return element.DisplayTransformation.Matrix * parent;
        }

        /// <summary>
        /// Pure function for getting the matrix of the interaction transformation
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static Matrix4x4 GetInteractionTransform(this IGuiElement element)
        {
            var parent = Matrix4x4.Identity;
            if (element.Parent != null) parent = element.Parent.GetDisplayTransform();
            return element.InteractionTransformation.Matrix * parent;
        }

        /// <summary>
        /// Pure function for flattening the element hiararchy into a single list
        /// </summary>
        /// <param name="element"></param>
        /// <param name="flatlist">The list containing the result</param>
        public static void FlattenElements(this IGuiElement element, List<IGuiElement> flatlist)
        {
            flatlist.Add(element);
            foreach (var child in element.Children.Values)
            {
                child.FlattenElements(flatlist);
            }
        }

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

        public static void FollowDisplay(this IGuiElement element)
        {
            element.DisplayTransformation.CopyTo(element.InteractionTransformation);
        }
        public static void FollowInteraction(this IGuiElement element)
        {
            element.DisplayTransformation.CopyTo(element.InteractionTransformation);
        }

        /// <summary>
        /// Create a copy of an Element with specified ID
        /// </summary>
        /// <param name="el"></param>
        /// <param name="id"></param>
        /// <param name="copyAge">Copy the time of the Age stopwatch into the new element</param>
        /// <returns></returns>
        public static IGuiElement Copy(this IGuiElement el, Guid id, bool copyAge = false)
        {
            var newel = el.Copy();
            newel.Id = id;
            if(copyAge) newel.Age.SetTime(el.Age.Elapsed);
            return newel;
        }

        /// <summary>
        /// Copies element data into another instance
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="sameParent"></param>
        /// <param name="copyId"></param>
        /// <param name="copyChildren"></param>
        public static void CopyTo(this IGuiElement a, IGuiElement b,
            bool sameParent = true,
            bool copyId = false,
            bool copyChildren = true)
        {
            b.Name = a.Name;
            b.Active = a.Active;
            b.Transparent = a.Transparent;
            b.Context = a.Context;

            b.Depth = a.Depth;
            b.InteractionTransformation = a.InteractionTransformation.Copy();
            b.DisplayTransformation = a.DisplayTransformation.Copy();

            b.Behaviors = a.Behaviors.ToList();
            b.FadeInTime = a.FadeInTime;
            b.FadeOutTime = a.FadeOutTime;

            b.Value = a.Value.Copy();
            b.EnvironmentObject = a.EnvironmentObject?.Clone() as ICloneable;

            if (sameParent) b.Parent = a.Parent;
            if (copyId) b.Id = a.Id;

            if (copyChildren)
            {
                b.Children = a.Children.Select(cid => cid, child => child.Copy());
            }
        }

        /// <summary>
        /// Updates element data from another one which has the same Id
        /// </summary>
        /// <param name="a">Reference Element</param>
        /// <param name="b">Destination Element</param>
        /// <param name="updateChildren">Run this function recursively with children of the reference</param>
        /// <param name="updateTransform">Update the transform or not</param>
        public static void UpdateTo(this IGuiElement a, IGuiElement b, bool updateChildren = true, bool updateTransform = true)
        {
            if(a.Id != b.Id) return;

            b.Name = a.Name;
            b.Active = a.Active;
            b.Transparent = a.Transparent;
            b.Context = a.Context;

            b.Depth = a.Depth;
            if(updateTransform) b.InteractionTransformation = a.InteractionTransformation;
            if(updateTransform) b.DisplayTransformation = a.DisplayTransformation;


            b.Behaviors = a.Behaviors.ToList();
            b.FadeInTime = a.FadeInTime;
            b.FadeOutTime = a.FadeOutTime;

            b.Value = a.Value;
            b.Parent = a.Parent;
            
            if (updateChildren)
                b.AddOrUpdateChildren(true, updateTransform, a.Children.Values.ToArray());
        }
    }
}
