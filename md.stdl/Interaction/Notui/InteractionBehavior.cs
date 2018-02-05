using System;

namespace md.stdl.Interaction.Notui
{
    /// <inheritdoc />
    /// <summary>
    /// Specifying a behavior where the element can be dragged, rotated and scaled freely or within constraints
    /// </summary>
    public class MultitouchCardsBehavior : IInteractionBehavior
    {
        /// <summary>
        /// Can the element be dragged in the parent context?
        /// </summary>
        public bool Draggable { get; set; }

        /// <summary>
        /// Can multiple touches scale this element?
        /// </summary>
        public bool Scalable { get; set; }

        /// <summary>
        /// Can multiple touches rotate this element?
        /// </summary>
        public bool Pivotable { get; set; }

        public Guid Id { get; set; } = Guid.NewGuid();
        public IGuiElement AttachedElement { get; set; }

        public void Behave()
        {
            throw new NotImplementedException();
        }

        public IInteractionBehavior Copy()
        {
            throw new NotImplementedException();
        }

        public void CopyTo(IInteractionBehavior b)
        {
            throw new NotImplementedException();
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
