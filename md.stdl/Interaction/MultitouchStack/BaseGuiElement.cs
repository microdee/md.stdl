﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace md.stdl.Interaction.MultitouchStack
{
    /// <inheritdoc cref="IGuiElement" />
    /// <summary>
    /// Simple element base implementing some useful management functions
    /// </summary>
    public abstract class BaseGuiElement : IGuiElement
    {
        private Matrix4x4 _interactionMatrix;
        private Matrix4x4 _displayMatrix;
        private ElementTransformation _displayTransformation;

        public string Name { get; set; }
        public MultitouchContext Context { get; set; }
        public bool Active { get; set; }
        public bool Transparent { get; set; }

        public bool Draggable { get; set; }
        public bool Scalable { get; set; }
        public bool Pivotable { get; set; }

        public bool Hit { get; set; }
        public bool Touched { get; set; }
        public float Depth { get; set; }
        public float FadeOutTime { get; set; }
        public float FadeInTime { get; set; }
        public float ElementFade { get; set; }

        public HashSet<TouchContainer<IGuiElement[]>> Touching { get; set; } =
            new HashSet<TouchContainer<IGuiElement[]>>(new TouchEqualityComparer());
        public HashSet<TouchContainer<IGuiElement[]>> Hitting { get; set; } =
            new HashSet<TouchContainer<IGuiElement[]>>(new TouchEqualityComparer());
        public HashSet<TouchContainer<IGuiElement[]>> Hovering { get; set; } =
            new HashSet<TouchContainer<IGuiElement[]>>(new TouchEqualityComparer());

        public IGuiElement Parent { get; set; }
        public List<IGuiElement> Children { get; set; } = new List<IGuiElement>();
        public List<IInteractionBehavior> Behaviors { get; set; } = new List<IInteractionBehavior>();

        public bool DeleteMe { get; set; }
        public Stopwatch Age { get; set; } = new Stopwatch();
        public Stopwatch Dethklok { get; set; } = new Stopwatch();
        public AttachedValues Value { get; set; } = new AttachedValues();
        public ElementTransformation InteractionTransformation { get; set; } = new ElementTransformation();
        public ElementTransformation DisplayTransformation
        {
            get => _displayTransformation;
            set
            {
                if (_displayTransformation == null)
                {
                    value.OnChange += (sender, args) => InvalidateMatrices();
                    _displayTransformation = value;
                    return;
                }
                if (value.GetHashCode() != _displayTransformation.GetHashCode())
                {
                    value.OnChange += (sender, args) => InvalidateMatrices();
                }
                _displayTransformation = value;
            }
        }
        public bool InteractionMatrixCached { get; set; }
        public bool DisplayMatrixCached { get; set; }
        public Matrix4x4 InteractionMatrix
        {
            get
            {
                if (!InteractionMatrixCached)
                {
                    _interactionMatrix = this.GetInteractionTransform();
                    InteractionMatrixCached = true;
                }
                return _interactionMatrix;
            }
        }
        public Matrix4x4 DisplayMatrix
        {
            get
            {
                if (!DisplayMatrixCached)
                {
                    _displayMatrix = this.GetDisplayTransform();
                    DisplayMatrixCached = true;
                }
                return _displayMatrix;
            }
        }

        public event EventHandler<TouchInteractionEventArgs> OnInteractionBegin;
        public event EventHandler<TouchInteractionEventArgs> OnInteractionEnd;
        public event EventHandler<TouchInteractionEventArgs> OnTouchBegin;
        public event EventHandler<TouchInteractionEventArgs> OnTouchEnd;
        public event EventHandler<TouchInteractionEventArgs> OnHitBegin;
        public event EventHandler<TouchInteractionEventArgs> OnHitEnd;
        public event EventHandler OnInteracting;
        public event EventHandler<ChildrenAddedEventArgs> OnChildrenAdded;
        public event EventHandler OnDeletionStarted;
        public event EventHandler OnDeleting;
        public event EventHandler OnFadedIn;

        public void AddChildren(params IGuiElement[] children)
        {
            Children.AddRange(children);
            foreach (var child in children)
            {
                child.Parent = this;
            }
            OnChildrenAdded?.Invoke(this, new ChildrenAddedEventArgs {Elements = children});
        }

        public abstract bool HitTest(TouchContainer<IGuiElement[]> touch);

        public virtual void ProcessTouch(TouchContainer<IGuiElement[]> touch)
        {
            var hit = Hovering.Contains(touch);
            var eventargs = new TouchInteractionEventArgs { Touch = touch };
            if (!hit)
            {
                if (!Hitting.Contains(touch)) return;
                Hitting.Remove(touch);
                OnHitEnd?.Invoke(this, eventargs);
                return;
            }
            if (!Hitting.Contains(touch))
            {
                OnHitBegin?.Invoke(this, eventargs);
                Hitting.Add(touch);
            }
            FireInteractionTouchBegin(touch);
        }

        public virtual void MainLoop()
        {
            (from touch in Touching where touch.ExpireFrames > Context.ConsiderReleasedAfter select touch).ForEach(FireTouchEnd);

            Hit = Hitting.Count > 0;
            Touched = Touching.Count > 0;

            if (FadeInTime > 0)
            {
                ElementFade = Min(Max(0, (float) Age.Elapsed.TotalSeconds / FadeInTime), 1);
            }
            else
            {
                if(ElementFade < 1) OnFadedIn?.Invoke(this, EventArgs.Empty);
                ElementFade = 1;
            }
            if (Age.Elapsed.TotalSeconds >= FadeInTime) OnFadedIn?.Invoke(this, EventArgs.Empty);

            if (FadeOutTime > 0)
            {
                ElementFade *= Min(Max(0, 1 - (float)Dethklok.Elapsed.TotalSeconds / FadeOutTime), 1);
                if (Dethklok.Elapsed.TotalSeconds > FadeOutTime)
                {
                    ElementFade = 0;
                    if(!DeleteMe) OnDeleting?.Invoke(this, EventArgs.Empty);
                    DeleteMe = true;
                }
            }

            if(Touched) OnInteracting?.Invoke(this, EventArgs.Empty);
            foreach (var behavior in Behaviors)
            {
                behavior.AttachedElement = this;
                behavior.Behave();
            }

        }

        public abstract IGuiElement Copy();

        public void InvalidateMatrices()
        {
            InteractionMatrixCached = false;
            DisplayMatrixCached = false;
            foreach (var child in Children)
                child.InvalidateMatrices();
        }

        protected BaseGuiElement()
        {
            DisplayTransformation = new ElementTransformation();
            Age.Start();
        }

        protected void FireInteractionTouchBegin(TouchContainer<IGuiElement[]> touch)
        {
            if(touch.AgeFrames >= Context.ConsiderNewBefore) return;
            if(Touching.Contains(touch)) return;
            var eventargs = new TouchInteractionEventArgs { Touch = touch };
            if (Touching.Count == 0) OnInteractionBegin?.Invoke(this, eventargs);
            OnTouchBegin?.Invoke(this, eventargs);
            Touching.Add(touch);
        }
        protected void FireInteractionEnd(TouchContainer<IGuiElement[]> touch)
        {
            if (Touching.Count == 0) OnInteractionEnd?.Invoke(this, new TouchInteractionEventArgs
            {
                Touch = touch
            });
        }
        protected void FireTouchEnd(TouchContainer<IGuiElement[]> touch)
        {
            if (!Touching.Contains(touch)) return;
            Touching.Remove(touch);
            OnTouchEnd?.Invoke(this, new TouchInteractionEventArgs
            {
                Touch = touch
            });
            FireInteractionEnd(touch);
        }

        public object Clone()
        {
            return Copy();
        }

        public void StartDeletion()
        {
            if (FadeOutTime > 0)
            {
                OnDeletionStarted?.Invoke(this, EventArgs.Empty);
                Dethklok.Start();
            }
            else
            {
                ElementFade = 0;
                OnDeleting?.Invoke(this, EventArgs.Empty);
                DeleteMe = true;
            }
        }
    }
}