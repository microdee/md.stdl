using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace md.stdl.Interaction.Notui
{
    public class TouchInteractionEventArgs : EventArgs
    {
        public TouchContainer<IGuiElement[]> Touch;
        public IntersectionPoint IntersectionPoint;
    }

    public class ChildrenAddedEventArgs : EventArgs
    {
        public IGuiElement[] Elements;
    }

    public class IntersectionPoint
    {
        public Vector3 WorldSpace { get; set; }
        public Vector3 ElementSpace { get; set; }
        public IGuiElement Element { get; set; }
    }

    /// <inheritdoc />
    /// <summary>
    /// Simple strongly typed wrapper for ICloneable
    /// </summary>
    /// <typeparam name="T">Any type</typeparam>
    public interface ICopy<out T> : ICloneable
    {
        /// <summary>
        /// Clone the object with strong typing
        /// </summary>
        /// <returns>The clone of the original object</returns>
        T Copy();
    }

    /// <inheritdoc />
    /// <summary>
    /// Interface for defining per-frame behavior for any IGuiElement
    /// </summary>
    public interface IInteractionBehavior : ICopy<IInteractionBehavior>
    {
        /// <summary>
        /// The element this behavior will affect
        /// </summary>
        IGuiElement AttachedElement { get; set; }

        /// <summary>
        /// The method which will be executed for the given element every frame.
        /// </summary>
        void Behave();
    }

    /// <inheritdoc />
    /// <summary>
    /// A general purpose parameter holder for any IGuiElement
    /// </summary>
    public class AttachedValues : ICopy<AttachedValues>
    {
        /// <summary>
        /// N axis float values
        /// </summary>
        public float[] Values = new float[1];
        /// <summary>
        /// N number of strings
        /// </summary>
        public string[] Texts = new string[1];
        /// <summary>
        /// Whatever you want as long as it's clonable
        /// </summary>
        public ICloneable Auxiliary;

        public AttachedValues Copy()
        {
            return new AttachedValues
            {
                Values = Values.ToArray(),
                Texts = Texts.ToArray(),
                Auxiliary = (ICloneable)Auxiliary.Clone()
            };
        }

        public object Clone()
        {
            return Copy();
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Interface for an element inside a Notui Context
    /// </summary>
    public interface IGuiElement : ICopy<IGuiElement>
    {
        /// <summary>
        /// General name used for identification
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// A unique identifier used comparing elements
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// The context which this element is assigned to.
        /// </summary>
        NotuiContext Context { get; set; }

        /// <summary>
        /// Is element reacting to touches?
        /// </summary>
        bool Active { get; set; }

        /// <summary>
        /// Whether this element blocks touches
        /// </summary>
        bool Transparent { get; set; }

        /// <summary>
        /// Are there touches over this element?
        /// </summary>
        bool Hit { get; set; }

        /// <summary>
        /// Does this element has touches interacting with it?
        /// </summary>
        bool Touched { get; set; }

        /// <summary>
        /// The depth of this element in the touch context. Lowest depth value is the top element, highest is the bottom one.
        /// </summary>
        /// <remarks>
        /// This is not the same as the Z component of an element's position. When processing the elements only this value is used to determine their order from lowest to highest.
        /// </remarks>
        float Depth { get; set; }

        /// <summary>
        /// Delete this element after this amount of seconds passed in the Dethklok
        /// </summary>
        float FadeOutTime { get; set; }

        /// <summary>
        /// Seconds to fade in this element based on the Age
        /// </summary>
        float FadeInTime { get; set; }

        /// <summary>
        /// Element fading from 0 (faded out) to 1 (faded in)
        /// </summary>
        float ElementFade { get; set; }

        /// <summary>
        /// Transformation to work with during user manipulation like smoothed dragging. This is used for hittesting touches already interacting with this element.
        /// </summary>
        ElementTransformation InteractionTransformation { get; set; }

        /// <summary>
        /// Transformation to be used during the display of the element. This is used for hittesting newly interacting touches, and for the parent transformation for children.
        /// </summary>
        ElementTransformation DisplayTransformation { get; set; }

        /// <summary>
        /// List of touches interacting with this element which is managed by this element
        /// </summary>
        ConcurrentDictionary<TouchContainer<IGuiElement[]>, IntersectionPoint> Touching { get; set; }

        /// <summary>
        /// List of touches directly over this element which is managed by this element
        /// </summary>
        ConcurrentDictionary<TouchContainer<IGuiElement[]>, IntersectionPoint> Hitting { get; set; }

        /// <summary>
        /// List of touches hovering this element which is managed by the context
        /// </summary>
        ConcurrentDictionary<TouchContainer<IGuiElement[]>, IntersectionPoint> Hovering { get; set; }

        /// <summary>
        /// The element this element inherits its transformation from. Null if this element is directly in a context.
        /// </summary>
        IGuiElement Parent { get; set; }

        /// <summary>
        /// Was interaction matrix already calculated since last request.
        /// </summary>
        bool InteractionMatrixCached { get; set; }
        /// <summary>
        /// Was display matrix already calculated since last request.
        /// </summary>
        bool DisplayMatrixCached { get; set; }

        /// <summary>
        /// Absolute world interaction transformation.
        /// </summary>
        Matrix4x4 InteractionMatrix { get; }

        /// <summary>
        /// Absolute world display transformation.
        /// </summary>
        Matrix4x4 DisplayMatrix { get; }

        /// <summary>
        /// Elements which will inherit the transformation of this element
        /// </summary>
        List<IGuiElement> Children { get; set; }

        /// <summary>
        /// Set of interaction behaviors assigned to this element executed in the order of from first to last
        /// </summary>
        List<IInteractionBehavior> Behaviors { get; set; }

        /// <summary>
        /// Requests context to delete this element and its children
        /// </summary>
        bool DeleteMe { get; set; }
        
        /// <summary>
        /// Element age since creation
        /// </summary>
        Stopwatch Age { get; set; }

        /// <summary>
        /// Start this stopwatch to fade out this element before deletion.
        /// </summary>
        /// <remarks>
        /// Metalocalypse
        /// </remarks>
        Stopwatch Dethklok { get; set; }

        /// <summary>
        /// Optional value to be manipulated with the element
        /// </summary>
        AttachedValues Value { get; set; }

        /// <summary>
        /// Event on the first of multiple touches interacting with this element until the last touch is released
        /// </summary>
        event EventHandler<TouchInteractionEventArgs> OnInteractionBegin;

        /// <summary>
        /// Event on the last touch interacting with this element is released
        /// </summary>
        event EventHandler<TouchInteractionEventArgs> OnInteractionEnd;

        /// <summary>
        /// Event fired when a touch started interacting with this element
        /// </summary>
        event EventHandler<TouchInteractionEventArgs> OnTouchBegin;

        /// <summary>
        /// Event on the release of a touch interacting with this element
        /// </summary>
        event EventHandler<TouchInteractionEventArgs> OnTouchEnd;

        /// <summary>
        /// Event fired when a touch got over this element
        /// </summary>
        event EventHandler<TouchInteractionEventArgs> OnHitBegin;

        /// <summary>
        /// Event fired when a touch left this element
        /// </summary>
        event EventHandler<TouchInteractionEventArgs> OnHitEnd;

        /// <summary>
        /// Event fired on every frame while the element is being interacted with
        /// </summary>
        event EventHandler OnInteracting;

        /// <summary>
        /// Event fired when a child element is added
        /// </summary>
        event EventHandler<ChildrenAddedEventArgs> OnChildrenAdded;

        /// <summary>
        /// Event fired when the Dethklok is started. Except when FadeOutTime is set to 0.
        /// </summary>
        event EventHandler OnDeletionStarted;

        /// <summary>
        /// Event fired when the Element requested its deletion
        /// </summary>
        event EventHandler OnDeleting;

        /// <summary>
        /// Event fired when the Element finished fading
        /// </summary>
        event EventHandler OnFadedIn;

        /// <summary>
        /// Implementer should do the children addition and updating and call OnChildAdded to notify context
        /// </summary>
        /// <param name="children">children to be added</param>
        /// <param name="removeNotPresent">remove Children from elements not present in the input</param>
        void AddOrUpdateChildren(bool removeNotPresent, params IGuiElement[] children);

        /// <summary>
        /// Pure hittest function used by the context
        /// </summary>
        /// <param name="touch">Current touch</param>
        /// <returns>Return null when the element is not hit by the touch and return the intersection coordinates otherwise</returns>
        IntersectionPoint HitTest(TouchContainer<IGuiElement[]> touch);

        /// <summary>
        /// Used for managing side effects of touch interaction
        /// </summary>
        /// <param name="touch">Current touch</param>
        void ProcessTouch(TouchContainer<IGuiElement[]> touch);

        /// <summary>
        /// This is called every frame by the context
        /// </summary>
        /// <remarks>
        /// The context call this function of all flattened elements in parallel regardless of the element hierarchy, you should take this into account when overriding this function or developing behaviors. You MUST NOT call the Mainloop method of the children elements yourself because the context already does so (unless you are really desperate).
        /// </remarks>
        void MainLoop();

        /// <summary>
        /// Notify the need for recomputing absolute world matrices
        /// </summary>
        void InvalidateMatrices();

        /// <summary>
        /// Start the Dethklok or if FadeOutTime is 0 just set DeleteMe true and invoke OnDeleting
        /// </summary>
        void StartDeletion();
    }
}
