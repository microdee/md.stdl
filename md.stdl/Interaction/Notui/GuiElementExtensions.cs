using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace md.stdl.Interaction.Notui
{
    public static class GuiElementExtensions
    {
        public static Matrix4x4 GetDisplayTransform(this IGuiElement element)
        {
            var parent = Matrix4x4.Identity;
            if (element.Parent != null) parent = element.Parent.GetDisplayTransform();
            return element.DisplayTransformation.Matrix * parent;
        }
        public static Matrix4x4 GetInteractionTransform(this IGuiElement element)
        {
            var parent = Matrix4x4.Identity;
            if (element.Parent != null) parent = element.Parent.GetDisplayTransform();
            return element.InteractionTransformation.Matrix * parent;
        }

        public static void FlattenElements(this IGuiElement element, List<IGuiElement> flatlist)
        {
            foreach (var child in element.Children)
            {
                child.FlattenElements(flatlist);
                flatlist.Add(child);
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

        public static void CopyTo(this IGuiElement a, IGuiElement b,
            bool sameParent = true,
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

            if(sameParent) b.Parent = a.Parent;
            if (copyChildren)
            {
                b.Children = a.Children.Select(child => child.Copy()).ToList();
            }
        }
    }
}
