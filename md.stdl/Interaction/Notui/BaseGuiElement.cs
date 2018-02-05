using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace md.stdl.Interaction.Notui
{
    /// <inheritdoc cref="IGuiElement" />
    /// <summary>
    /// Simple element base implementing some useful management functions
    /// </summary>
    /// <typeparam name="TElement">The type of the element inheriting this class</typeparam>
    public abstract class BaseGuiElement<TElement> : IGuiElement where TElement : IGuiElement, new()
    {
        private Matrix4x4 _interactionMatrix;
        private Matrix4x4 _displayMatrix;
        private ElementTransformation _displayTransformation;

        public string Name { get; set; }
        public Guid Id { get; set; } = new Guid();
        public NotuiContext Context { get; set; }
        public bool Active { get; set; }
        public bool Transparent { get; set; }

        public bool Hit { get; set; }
        public bool Touched { get; set; }
        public float Depth { get; set; }
        public float FadeOutTime { get; set; }
        public float FadeInTime { get; set; }
        public float ElementFade { get; set; }

        public ConcurrentDictionary<TouchContainer<IGuiElement[]>, IntersectionPoint> Touching { get; set; } =
            new ConcurrentDictionary<TouchContainer<IGuiElement[]>, IntersectionPoint>(new TouchEqualityComparer());
        public ConcurrentDictionary<TouchContainer<IGuiElement[]>, IntersectionPoint> Hitting { get; set; } =
            new ConcurrentDictionary<TouchContainer<IGuiElement[]>, IntersectionPoint>(new TouchEqualityComparer());
        public ConcurrentDictionary<TouchContainer<IGuiElement[]>, IntersectionPoint> Hovering { get; set; } =
            new ConcurrentDictionary<TouchContainer<IGuiElement[]>, IntersectionPoint>(new TouchEqualityComparer());

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

        public void AddOrUpdateChildren(bool removeNotPresent, params IGuiElement[] children)
        {
            var newchildren = from child in children where Children.All(c => c.Id != child.Id) && child.Id != Id select child;
            var existingchildren = from child in children where Children.Any(c => c.Id == child.Id) && child.Id != Id select child;

            if (removeNotPresent)
            {
                var removablechildren = from child in Children where children.All(c => c.Id != child.Id) select child;
                foreach (var element in removablechildren)
                {
                    element.StartDeletion();
                }
            }

            foreach (var child in existingchildren)
            {
                var existingchild = Children.First(c => c.Id == child.Id);
                child.UpdateTo(existingchild);
            }
            foreach (var child in newchildren)
            {
                child.Parent = this;
                Children.Add(child);
            }
            OnChildrenAdded?.Invoke(this, new ChildrenAddedEventArgs {Elements = children});
        }

        public abstract IntersectionPoint HitTest(TouchContainer<IGuiElement[]> touch);

        public virtual void ProcessTouch(TouchContainer<IGuiElement[]> touch)
        {
            var hit = Hovering.ContainsKey(touch);
            var eventargs = new TouchInteractionEventArgs
            {
                Touch = touch,
                IntersectionPoint = Hovering[touch]
            };
            if (hit && Touching.ContainsKey(touch)) Touching[touch] = Hovering[touch];
            if (!hit)
            {
                if (!Hitting.ContainsKey(touch)) return;
                Hitting.TryRemove(touch, out var dummy);
                OnHitEnd?.Invoke(this, eventargs);
                if (Touching.ContainsKey(touch)) Touching[touch] = null;
                return;
            }
            if (!Hitting.ContainsKey(touch))
            {
                OnHitBegin?.Invoke(this, eventargs);
                Hitting.TryAdd(touch, Hovering[touch]);
            }
            FireInteractionTouchBegin(touch);
        }

        public virtual void MainLoop()
        {
            (from touch in Touching.Keys where touch.ExpireFrames > Context.ConsiderReleasedAfter select touch).ForEach(FireTouchEnd);

            Hit = Hitting.Count > 0;
            Touched = Touching.Count > 0;

            if (FadeInTime > 0)
            {
                ElementFade = Math.Min(Math.Max(0, (float) Age.Elapsed.TotalSeconds / FadeInTime), 1);
            }
            else
            {
                if(ElementFade < 1) OnFadedIn?.Invoke(this, EventArgs.Empty);
                ElementFade = 1;
            }
            if (Age.Elapsed.TotalSeconds >= FadeInTime) OnFadedIn?.Invoke(this, EventArgs.Empty);

            if (FadeOutTime > 0)
            {
                ElementFade *= Math.Min(Math.Max(0, 1 - (float)Dethklok.Elapsed.TotalSeconds / FadeOutTime), 1);
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

        public virtual IGuiElement Copy()
        {
            var res = new TElement();
            this.CopyTo(res);
            return res;
        }

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
            if(Touching.ContainsKey(touch)) return;
            var eventargs = new TouchInteractionEventArgs { Touch = touch };
            if (Touching.Count == 0) OnInteractionBegin?.Invoke(this, eventargs);
            OnTouchBegin?.Invoke(this, eventargs);
            Touching.TryAdd(touch, Hovering[touch]);
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
            if (!Touching.ContainsKey(touch)) return;
            Touching.TryRemove(touch, out var dummy);
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
            foreach (var child in Children)
            {
                child.StartDeletion();
            }
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
