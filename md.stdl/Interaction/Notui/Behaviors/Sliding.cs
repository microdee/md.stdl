using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using md.stdl.Mathematics;
using static System.Math;

namespace md.stdl.Interaction.Notui.Behaviors
{
    /// <inheritdoc />
    /// <summary>
    /// Specifying a behavior where the element can be dragged, rotated and scaled freely or within constraints
    /// </summary>
    public class SlidingBehavior : InteractionBehavior
    {
        public enum SelectedPlane
        {
            /// <summary>
            /// The plane parallel to the view and offset by the element's center position.
            /// </summary>
            ViewAligned,

            /// <summary>
            /// The plane defined by the elements DisplayMatrix
            /// </summary>
            OwnPlane,

            /// <summary>
            /// If exists the plane defined by the DisplayMatrix of the element's parent. Otherwise use ViewAligned
            /// </summary>
            ParentPlane
        }

        public class BehaviorState : AuxiliaryObject
        {
            public Vector2 DeltaPos = Vector2.Zero;
            public float DeltaAngle = 0;
            public float DeltaSize = 0;

            public float TotalAngle = 0;

            public override AuxiliaryObject Copy()
            {
                return new BehaviorState
                {
                    DeltaPos = DeltaPos,
                    DeltaAngle = DeltaAngle,
                    DeltaSize = DeltaSize,
                    TotalAngle = TotalAngle
                };
            }

            public override void UpdateFrom(AuxiliaryObject other)
            {
                if (!(other is BehaviorState bs)) return;
                DeltaPos = bs.DeltaPos;
                DeltaAngle = bs.DeltaAngle;
                DeltaSize = bs.DeltaSize;
                TotalAngle = bs.TotalAngle;
            }
        }

        /// <summary>
        /// Can the element be dragged in the parent context?
        /// </summary>
        [BehaviorParameter]
        public bool Draggable { get; set; } = true;

        /// <summary>
        /// Dragging sensitivity per axis.
        /// </summary>
        /// <remarks>
        /// 0 on an axis means that axis is locked, below 0 means axis movement is reversed
        /// </remarks>
        [BehaviorParameter]
        public Vector2 DraggingCoeffitient { get; set; } = new Vector2(1.0f);

        /// <summary>
        /// Can the user scale this element?
        /// </summary>
        [BehaviorParameter]
        public bool Scalable { get; set; } = false;

        /// <summary>
        /// Scaling sensitivity
        /// </summary>
        /// <remarks>
        /// 0 means that scaling is locked (same as Scalable = false), below 0 means scaling movement is reversed
        /// </remarks>
        [BehaviorParameter]
        public float ScalingCoeffitient { get; set; } = 1.0f;

        /// <summary>
        /// Can the user rotate this element?
        /// </summary>
        [BehaviorParameter]
        public bool Pivotable { get; set; } = false;

        /// <summary>
        /// Rotation sensitivity
        /// </summary>
        /// <remarks>
        /// 0 means that rotation is locked (same as Pivotable = false), below 0 means rotation movement is reversed
        /// </remarks>
        [BehaviorParameter]
        public float RotationCoeffitient { get; set; } = 1.0f;

        /// <summary>
        /// Slide element in the selected plane.
        /// </summary>
        /// <remarks>
        /// If this is true then constraints will be also relative to the element's parent, instead of the world space.
        /// </remarks>
        [BehaviorParameter]
        public SelectedPlane SlideInSelectedPlane { get; set; } = SelectedPlane.ViewAligned;

        /// <summary>
        /// Slide when children are hit as well
        /// </summary>
        [BehaviorParameter]
        public bool SlideOnChildrenInteracting { get; set; }

        /// <summary>
        /// Slide only when this amount of touches interacting with the element
        /// </summary>
        [BehaviorParameter(Minimum = 1)]
        public int MinimumTouches { get; set; } = 1;

        /// <summary>
        /// Limit rotation to a minimum and maximum cycles
        /// </summary>
        [BehaviorParameter]
        public bool LimitRotation { get; set; }

        /// <summary>
        /// Limit the dragging to a bounding box
        /// </summary>
        [BehaviorParameter]
        public bool LimitTranslation { get; set; }

        /// <summary>
        /// Minimum and maximum cycles if rotation is limited.
        /// </summary>
        [BehaviorParameter]
        public Vector2 RotationMinMax { get; set; } = new Vector2(-1, 1);

        /// <summary>
        /// Minimum and maximum size of the element
        /// </summary>
        [BehaviorParameter(Minimum = 0)]
        public Vector2 ScaleMinMax { get; set; } = new Vector2(0.1f, 3);

        /// <summary>
        /// Minimum of bounding box in world or parent space
        /// </summary>
        [BehaviorParameter]
        public Vector3 BoundingBoxMin { get; set; } = new Vector3(-1, -1, -1);
        /// <summary>
        /// Maximum of bounding box in world or parent space
        /// </summary>
        [BehaviorParameter]
        public Vector3 BoundingBoxMax { get; set; } = new Vector3(1, 1, 1);

        /// <summary>
        /// After the interaction ended how long the element should continue sliding in seconds
        /// </summary>
        /// <remarks>
        /// While the element is flicking constraints are still applied. SlidingBehavior will attempt to approach constraint borders smoothly.
        /// </remarks>
        [BehaviorParameter(Minimum = 0)]
        public float FlickTime { get; set; } = 0;

        private SelectedPlane _actualPlaneSelection;

        private Vector3 GetTouchWorldVelocity(TouchContainer touch, Matrix4x4 plane, NotuiContext context, out Vector3 currpos, out Vector3 prevpos)
        {
            var hit = Intersections.PlaneRay(touch.WorldPosition, touch.ViewDir, plane, out var capos, out var crpos);
            currpos = crpos;
            var prevpoint = touch.Point - touch.Velocity;
            Coordinates.GetPointWorldPosDir(prevpoint, context.ProjectionWithAspectRatioInverse, context.ViewInverse, out var popos, out var pdir);
            var phit = Intersections.PlaneRay(popos, pdir, plane, out var papos, out var prpos);
            prevpos = prpos;
            return crpos - prpos;
        }

        private void Move(NotuiElement element, BehaviorState state, Matrix4x4 usedplane)
        {
            var disptr = element.DisplayTransformation;
            if (Draggable)
            {
                var worldvel = Vector4.Transform(new Vector4(state.DeltaPos * DraggingCoeffitient, 0, 0), usedplane).xyz();
                if (element.Parent != null)
                {
                    Matrix4x4.Invert(element.Parent.DisplayMatrix, out var invparenttr);
                    worldvel = Vector4.Transform(new Vector4(worldvel, 0), invparenttr).xyz();
                }

                disptr.Translate(worldvel);
                if (LimitTranslation)
                    disptr.Position = Intersections.BoxPointLimit(BoundingBoxMin, BoundingBoxMax, disptr.Position);

                element.UpdateFromDisplayToInteraction(element);
            }

            if (Scalable)
            {
                var sclvel = 1 + state.DeltaSize * ScalingCoeffitient;
                var scl = disptr.Scale;
                disptr.Resize(new Vector3(sclvel));
                if (disptr.Scale.Length() > ScaleMinMax.Y || disptr.Scale.Length() < ScaleMinMax.X)
                    disptr.Scale = scl;
                element.UpdateFromDisplayToInteraction(element);
            }

            if (Pivotable)
            {
                var targetrot = state.TotalAngle + state.DeltaAngle * RotationCoeffitient;
                if (!LimitRotation || RotationMinMax.X <= targetrot && targetrot <= RotationMinMax.Y)
                {
                    state.TotalAngle = targetrot;

                    var worldaxis = Vector3.TransformNormal(Vector3.UnitZ, usedplane);
                    var worldrot = Quaternion.CreateFromAxisAngle(worldaxis, state.DeltaAngle * RotationCoeffitient);
                    if (element.Parent != null)
                    {
                        Matrix4x4.Invert(element.Parent.DisplayMatrix, out var invparenttr);
                        Matrix4x4.Decompose(invparenttr, out var ipscale, out var iprot, out var ippos);
                        worldrot = worldrot * iprot;
                    }
                    element.DisplayTransformation.GlobalRotate(worldrot);
                    element.UpdateFromDisplayToInteraction(element);
                }
            }
        }

        private void FlickProgress(BehaviorState state, NotuiContext context)
        {
            var frametime = context.DeltaTime * FlickTime;
            if (FlickTime < context.DeltaTime)
            {
                state.DeltaPos = Vector2.Zero;
                state.DeltaAngle = 0;
                state.DeltaSize = 0;
            }
            else
            {
                state.DeltaPos = Vector2.Lerp(state.DeltaPos, Vector2.Zero, 1/frametime);
                state.DeltaAngle *= 1 / frametime;
                state.DeltaSize *= 1 / frametime;
            }
        }

        private void AddChildrenTouches(NotuiElement element, List<TouchContainer<NotuiElement[]>> touches)
        {
            foreach (var child in element.Children.Values)
            {
                touches.AddRange(child.Touching.Keys);
                AddChildrenTouches(child, touches);
            }
        }

        private Vector4 CalcDeltaMatrix(Vector2 curr0, Vector2 prev0, Vector2 curr1, Vector2 prev1, out Matrix4x4 tr)
        {
            var prev0x = prev0.X;
            var prev0y = prev0.Y;
            var prev1x = prev1.X;
            var prev1y = prev1.Y;
            var curr0x = curr0.X;
            var curr0y = curr0.Y;
            var curr1x = curr1.X;
            var curr1y = curr1.Y;
            var prevdiffx = prev1x - prev0x;
            var prevdiffy = prev1y - prev0y;
            var currdiffx = curr1x - curr0x;
            var currdiffy = curr1y - curr0y;
            //Sqrt((currdiffx * currdiffx + currdiffy * currdiffy) / (prevdiffx * prevdiffx + prevdiffy * prevdiffy));
            var rot1 = (prevdiffx * currdiffx + prevdiffy * currdiffy) / (prevdiffx * prevdiffx + prevdiffy * prevdiffy);
            var rot2 = (prevdiffy * currdiffx - prevdiffx * currdiffy) / (prevdiffx * prevdiffx + prevdiffy * prevdiffy);
            var tx = curr0x - (rot1 * prev0x + rot2 * prev0y);
            var ty = curr0y - (-rot2 * prev0x + rot1 * prev0y);

            var restr = new Matrix4x4(
                rot1, -rot2, 0.0f, 0.0f,
                rot2,  rot1, 0.0f, 0.0f,
                0.0f,  0.0f, 1.0f, 0.0f,
                tx,    ty,   0.0f, 1.0f
            );

            tr = restr;
            var refv = Vector4.Transform(Vector4.UnitX, restr).xy();
            var polar = Coordinates.RectToPolar(refv);
            return new Vector4(tx, ty, polar.X, polar.Y);
        }

        public override void Behave(NotuiElement element)
        {
            _actualPlaneSelection = SlideInSelectedPlane == SelectedPlane.ParentPlane ?
                (element.Parent != null ? SelectedPlane.ParentPlane : SelectedPlane.ViewAligned) :
                SlideInSelectedPlane;

            Matrix4x4 usedplane;
            switch (_actualPlaneSelection)
            {
                case SelectedPlane.ParentPlane:
                    usedplane = element.Parent.DisplayMatrix;
                    break;
                case SelectedPlane.OwnPlane:
                    usedplane = element.DisplayMatrix;
                    break;
                case SelectedPlane.ViewAligned:
                    usedplane = Matrix4x4.CreateFromQuaternion(element.Context.ViewOrientation) *
                                Matrix4x4.CreateTranslation(element.DisplayMatrix.Translation);
                    break;
                default:
                    // same as above
                    usedplane = Matrix4x4.CreateFromQuaternion(element.Context.ViewOrientation) *
                                Matrix4x4.CreateTranslation(element.DisplayMatrix.Translation);
                    break;
            }

            var hasstate = IsStateAvailable(element);
            var currstate = hasstate ? GetState<BehaviorState>(element) : new BehaviorState();

            var touches = element.Touching.Keys.ToList();
            if(SlideOnChildrenInteracting)
                AddChildrenTouches(element, touches);

            if(touches.Count >= MinimumTouches)
            {
                if (Draggable && element.Touching.Count == 1)
                {
                    var relvel = GetTouchWorldVelocity(element.Touching.Keys.First(), usedplane, element.Context,
                        out var crelpos, out var prelpos);
                    currstate.DeltaPos = relvel.xy();

                    Move(element, currstate, usedplane);
                    FlickProgress(currstate, element.Context);
                    SetState(element, currstate);
                    return;
                }
                

                if (!Draggable && element.Touching.Count == 1)
                {
                    GetTouchWorldVelocity(element.Touching.Keys.First(), usedplane, element.Context,
                        out var crelpos, out var prelpos);
                    var deltarn = CalcDeltaMatrix(crelpos.xy(), prelpos.xy(), crelpos.xy() * -1, prelpos.xy() * -1, out var deltarntr);
                    currstate.DeltaPos = Vector2.Zero;
                    currstate.DeltaAngle = deltarn.Z;
                    currstate.DeltaSize = deltarn.W;

                    Move(element, currstate, usedplane);
                    FlickProgress(currstate, element.Context);
                    SetState(element, currstate);
                    return;
                }

                var orderedbyfastest = touches.OrderByDescending(t => t.Velocity.LengthSquared()).ToArray();
                var t0 = orderedbyfastest[0];
                var t1 = orderedbyfastest[1];
                GetTouchWorldVelocity(t0, usedplane, element.Context, out var cp0, out var pp0);
                GetTouchWorldVelocity(t1, usedplane, element.Context, out var cp1, out var pp1);
                var delta = CalcDeltaMatrix(cp0.xy(), pp0.xy(), cp1.xy(), pp1.xy(), out var deltatr);

                currstate.DeltaPos = delta.xy();
                currstate.DeltaAngle = delta.Z;
                currstate.DeltaSize = delta.W;
            }
            Move(element, currstate, usedplane);
            FlickProgress(currstate, element.Context);
            SetState(element, currstate);
        }
    }
}
