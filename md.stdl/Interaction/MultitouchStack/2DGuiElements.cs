using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using md.stdl.Mathematics;
using VVVV.Utils.VMath;
using static System.Math;
using static VVVV.Utils.VMath.VMath;

namespace md.stdl.Interaction.MultitouchStack
{
    public abstract class PlanarElement : BaseGuiElement
    {
        protected bool PreparePlanarShapeHitTest(TouchContainer<IGuiElement[]> touch, out Vector3 isPoint, out Vector3 planePoint)
        {
            var matrix = Hitting.Contains(touch) ? InteractionMatrix : DisplayMatrix;
            var hit = Intersections.PlaneRay(
                touch.WorldPosition,
                touch.ViewDir,
                matrix,
                out isPoint,
                out planePoint);
            return hit;
        }
    }

    public class RectangleElement : PlanarElement
    {
        public override bool HitTest(TouchContainer<IGuiElement[]> touch)
        {
            var hit = PreparePlanarShapeHitTest(touch, out var wPoint, out var planepoint);
            if (!hit) return false;
            return planepoint.X <= 1 && planepoint.X >= -1 && planepoint.Y <= 1 && planepoint.Y >= -1;
        }
        public override IGuiElement Copy()
        {
            var res = new RectangleElement();
            this.CopyTo(res);
            return res;
        }
    }
    public class CircleElement : PlanarElement
    {
        public override bool HitTest(TouchContainer<IGuiElement[]> touch)
        {
            var hit = PreparePlanarShapeHitTest(touch, out var wPoint, out var planepoint);
            if (!hit) return false;
            return planepoint.xy().Length() < 1;
        }
        public override IGuiElement Copy()
        {
            var res = new CircleElement();
            this.CopyTo(res);
            return res;
        }
    }
    public class SegmentElement : PlanarElement
    {
        public float HoleRadius { get; set; }
        public float Cycles { get; set; }

        public override bool HitTest(TouchContainer<IGuiElement[]> touch)
        {
            var hit = PreparePlanarShapeHitTest(touch, out var wPoint, out var planepoint);
            if (!hit) return false;
            var polar = Coordinates.RectToPolar(planepoint.xy());
            return (polar.Y < 1 && polar.X >= HoleRadius) &&
                ((polar.Y + PI) % TwoPi <= (Cycles*PI*2));
        }
        public override IGuiElement Copy()
        {
            var res = new SegmentElement();
            this.CopyTo(res);
            return res;
        }
    }
}
