using System.Collections.Generic;
using System.Linq;
using System.Numerics;

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
            foreach (var child in element.Children)
            {
                child.FlattenElements(flatlist);
            }
        }

        public static void Translate(this ElementTransformation tr, Vector3 diff)
        {
            tr.Position += diff;
        }
        public static void Resize(this ElementTransformation tr, Vector3 diff)
        {
            tr.Scale += diff;
        }
        public static void GlobalRotate(this ElementTransformation tr, Vector3 diff)
        {
            tr.Rotation = tr.Rotation * Quaternion.CreateFromYawPitchRoll(diff.Y, diff.X, diff.Z);
        }
        public static void LocalRotate(this ElementTransformation tr, Vector3 diff)
        {
            tr.Rotation = Quaternion.CreateFromYawPitchRoll(diff.Y, diff.X, diff.Z) * tr.Rotation;
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

            b.Behaviors = a.Behaviors.Select(behav => behav.Copy()).ToList();
            b.FadeInTime = a.FadeInTime;
            b.FadeOutTime = a.FadeOutTime;

            b.Value = a.Value.Copy();

            if (sameParent) b.Parent = a.Parent;
            if (copyId) b.Id = a.Id;

            if (copyChildren)
            {
                b.Children = a.Children.Select(child => child.Copy()).ToList();
            }
        }

        /// <summary>
        /// Updates element data from another one which has the same Id
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="updateChildren"></param>
        public static void UpdateTo(this IGuiElement a, IGuiElement b, bool updateChildren = true)
        {
            if(a.Id != b.Id) return;

            b.Name = a.Name;
            b.Active = a.Active;
            b.Transparent = a.Transparent;
            b.Context = a.Context;

            b.Depth = a.Depth;
            b.InteractionTransformation = a.InteractionTransformation;
            b.DisplayTransformation = a.DisplayTransformation;

            foreach (var behavior in a.Behaviors)
            {
                if (b.Behaviors.All(behav => behav.Id != behavior.Id))
                {
                    b.Behaviors.Add(behavior);
                }
                else
                {
                    var bbehav = b.Behaviors.First(behav => behav.Id != behavior.Id);
                    behavior.CopyTo(bbehav);
                }
            }
            b.FadeInTime = a.FadeInTime;
            b.FadeOutTime = a.FadeOutTime;

            b.Value = a.Value;
            b.Parent = a.Parent;

            b.Children = a.Children;
        }
    }
}
