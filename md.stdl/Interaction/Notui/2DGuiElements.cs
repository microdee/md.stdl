using System;
using md.stdl.Mathematics;
using VVVV.Utils.VMath;

namespace md.stdl.Interaction.Notui
{
    public abstract class PlanarElement<TElement> : BaseGuiElement<TElement> where TElement : IGuiElement, new()
    {
        protected IntersectionPoint PreparePlanarShapeHitTest(TouchContainer<IGuiElement[]> touch)
        {
            // when first hit consider the display transformation then
            // for the rest of the interaction consider the interaction transform
            var matrix = Hitting.ContainsKey(touch) ? InteractionMatrix : DisplayMatrix;
            var hit = Intersections.PlaneRay(
                touch.WorldPosition,
                touch.ViewDir,
                matrix,
                out var ispoint,
                out var planarpoint);
            return hit ? new IntersectionPoint
            {
                WorldSpace = ispoint,
                ElementSpace = planarpoint
            } : null;
        }
    }

    public class RectangleElement : PlanarElement<RectangleElement>
    {
        public override IntersectionPoint HitTest(TouchContainer<IGuiElement[]> touch)
        {
            var intersection = PreparePlanarShapeHitTest(touch);
            var phit = intersection != null;
            if (!phit) return null;
            var hit = intersection.ElementSpace.X <= 0.5 && intersection.ElementSpace.X >= -0.5 &&
                      intersection.ElementSpace.Y <= 0.5 && intersection.ElementSpace.Y >= -0.5;
            return hit ? intersection : null;
        }
    }
    public class CircleElement : PlanarElement<CircleElement>
    {
        public override IntersectionPoint HitTest(TouchContainer<IGuiElement[]> touch)
        {
            var intersection = PreparePlanarShapeHitTest(touch);
            var phit = intersection != null;
            if (!phit) return null;
            return intersection.ElementSpace.xy().Length() < 0.5 ? intersection : null;
        }
    }
    public class SegmentElement : PlanarElement<SegmentElement>
    {
        public float HoleRadius { get; set; } = 0;
        public float Cycles { get; set; } = 1;

        public override IntersectionPoint HitTest(TouchContainer<IGuiElement[]> touch)
        {
            var intersection = PreparePlanarShapeHitTest(touch);
            var phit = intersection != null;
            if (!phit) return null;
            var polar = Coordinates.RectToPolar(intersection.ElementSpace.xy());
            var hit = polar.Y * 2 < 1 && polar.Y * 2 >= HoleRadius && (polar.X + Math.PI) % VMath.TwoPi <= (Cycles * Math.PI * 2);
            return hit ? intersection : null;
        }
    }
}
