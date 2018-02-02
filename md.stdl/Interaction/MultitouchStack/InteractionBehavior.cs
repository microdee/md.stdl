﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace md.stdl.Interaction.MultitouchStack
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

        public IGuiElement AttachedElement { get; set; }
        public void Behave()
        {
            throw new NotImplementedException();
        }

        public IInteractionBehavior Copy()
        {
            throw new NotImplementedException();
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
