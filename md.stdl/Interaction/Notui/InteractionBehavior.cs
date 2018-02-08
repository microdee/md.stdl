using System;
using System.Linq;
using System.Numerics;

namespace md.stdl.Interaction.Notui
{
    /// <summary>
    /// Attribute telling some metadata about the behavior's parameter for the host application
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class BehaviorParameterAttribute : Attribute { }

    /// <summary>
    /// Abstract class for defining per-frame behavior for any IGuiElement.
    /// </summary>
    /// <remarks>
    /// Unlike IGuiElements behaviors should not be stateful. You can store states in 
    /// </remarks>
    public abstract class InteractionBehavior
    {
        public const string BehaviorStatePrefix = "Internal.Behavior:";

        /// <summary>
        /// A unique identifier used comparing elements
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// The method which will be executed for the given element every frame.
        /// </summary>
        public abstract void Behave(IGuiElement element);

        public T GetState<T>(IGuiElement element)
        {
            try
            {
                return (T)element.Value.Auxiliary[BehaviorStatePrefix + Id];
            }
            catch (Exception e)
            {
                var we = new Exception("Getting behavior state object failed.", e);
                throw we;
            }
        }

        public void SetState(IGuiElement element, ICloneable value)
        {
            if (IsStateAvailable(element))
                element.Value.Auxiliary[BehaviorStatePrefix + Id] = value;
            else element.Value.Auxiliary.Add(BehaviorStatePrefix + Id, value);
        }

        public bool IsStateAvailable(IGuiElement element) => element.Value.Auxiliary.ContainsKey(BehaviorStatePrefix + Id);
    }

    /// <inheritdoc />
    /// <summary>
    /// Move this element closest to the viewer
    /// </summary>
    public class MoveToTopOnTouchBehavior : InteractionBehavior
    {
        /// <summary>
        /// Distance to move to
        /// </summary>
        [BehaviorParameter]
        public float Distance { get; set; } = 0.0f;

        /// <summary>
        /// Use interaction transform to get element current depth
        /// </summary>
        [BehaviorParameter]
        public bool UseInteractionTransform { get; set; }

        private ElementTransformation SelectTransform(IGuiElement element)
        {
            return UseInteractionTransform ? element.InteractionTransformation : element.DisplayTransformation;
        }

        public override void Behave(IGuiElement element)
        {
            if(!element.Touched) return;
            var mindist = element.Children.Values.Min(child => SelectTransform(child).GetViewPosition(child.Context).Z);
            var moveby = Math.Max(mindist - Distance, 0);
            var movedir = Vector3.Normalize(element.Context.ViewPosition - SelectTransform(element).Position);
            element.DisplayTransformation.Translate(movedir * moveby);
            element.InteractionTransformation.Translate(movedir * moveby);
            foreach (var el in element.Context.Elements.Values)
            {
                el.DisplayTransformation.Translate(movedir * mindist * -1);
                el.InteractionTransformation.Translate(movedir * mindist * -1);
            }
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Specifying a behavior where the element can be dragged, rotated and scaled freely or within constraints
    /// </summary>
    public class MultitouchCardsBehavior : InteractionBehavior
    {
        public class BehaviorState : ICloneable
        {
            public object Clone()
            {
                return new BehaviorState
                {

                };
            }
        }

        /// <summary>
        /// Can the element be dragged in the parent context?
        /// </summary>
        [BehaviorParameter]
        public bool Draggable { get; set; }

        /// <summary>
        /// Can multiple touches scale this element?
        /// </summary>
        [BehaviorParameter]
        public bool Scalable { get; set; }

        /// <summary>
        /// Can multiple touches rotate this element?
        /// </summary>
        [BehaviorParameter]
        public bool Pivotable { get; set; }

        public override void Behave(IGuiElement element)
        {
            throw new NotImplementedException();
        }
    }
}
