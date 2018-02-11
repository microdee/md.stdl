using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace md.stdl.Interaction.Notui.Behaviors
{
    /// <inheritdoc />
    /// <summary>
    /// Specifying a behavior where the element can be dragged, rotated and scaled freely or within constraints
    /// </summary>
    public class SlidingBehavior : InteractionBehavior
    {
        public class BehaviorState : AuxiliaryObject
        {
            public Vector3 DeltaPos = Vector3.Zero;
            public float DeltaAngle = 0;
            public float DeltaSize = 0;

            public override AuxiliaryObject Copy()
            {
                return new BehaviorState
                {
                    DeltaPos = DeltaPos,
                    DeltaAngle = DeltaAngle,
                    DeltaSize = DeltaSize
                };
            }

            public override void UpdateFrom(AuxiliaryObject other)
            {
                if (!(other is BehaviorState bs)) return;
                DeltaPos = bs.DeltaPos;
                DeltaAngle = bs.DeltaAngle;
                DeltaSize = bs.DeltaSize;
            }
        }

        /// <summary>
        /// Can the element be dragged in the parent context?
        /// </summary>
        [BehaviorParameter]
        public bool Draggable { get; set; } = true;

        /// <summary>
        /// Can the user scale this element?
        /// </summary>
        [BehaviorParameter]
        public bool Scalable { get; set; } = false;

        /// <summary>
        /// Can the user rotate this element?
        /// </summary>
        [BehaviorParameter]
        public bool Pivotable { get; set; } = false;

        /// <summary>
        /// Slide element in the plane of its parent. Otherwise on the plane parallel to the view and offset by the element's center position.
        /// </summary>
        /// <remarks>
        /// If this is true then constraints will be also relative to the element's parent, instead of the world space.
        /// </remarks>
        [BehaviorParameter]
        public bool SlideInParentPlane { get; set; }

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
        [BehaviorParameter]
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
        [BehaviorParameter]
        public float FlickTime { get; set; } = 0;

        public override void Behave(NotuiElement element)
        {
            throw new NotImplementedException();
        }
    }
}
