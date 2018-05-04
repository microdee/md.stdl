using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using md.stdl.Interfaces;
using md.stdl.Mathematics;
using VVVV.Utils.Animation;

namespace md.stdl.Interaction
{
    /// <summary>
    /// Equality comparer for touches.
    /// </summary>
    public class TouchEqualityComparer : IEqualityComparer<TouchContainer>
    {
#pragma warning disable CS1591
        public bool Equals(TouchContainer x, TouchContainer y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Id == y.Id;
        }

        public int GetHashCode(TouchContainer obj)
        {
            return obj.Id;
        }
#pragma warning restore CS1591
    }
    /// <inheritdoc />
    /// <summary>
    /// Minimal multitouch touch container
    /// </summary>
    public class TouchContainer : IMainlooping
    {
        private Vector2 _prevPoint;

        /// <summary>
        /// Current position in screen or projection space
        /// </summary>
        public Vector2 Point { get; protected set; }

        /// <summary>
        /// Optional world position in 3D space
        /// </summary>
        public Vector3 WorldPosition { get; set; }

        /// <summary>
        /// Optional computed view direction in world
        /// </summary>
        public Vector3 ViewDir { get; set; }

        /// <summary>
        /// Force of touch, manager should set it.
        /// </summary>
        public float Force { get; set; }

        /// <summary>
        /// Attached object.
        /// </summary>
        public object CustomAttachedObject { get; set; }

        /// <summary>
        /// Touch point difference between updates
        /// </summary>
        public Vector2 Velocity { get; protected set; }

        /// <summary>
        /// Touch point speed (distance/seconds)
        /// </summary>
        public Vector2 NormalizedVelocity { get; protected set; }

        /// <summary>
        /// Unique ID
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Age since creation
        /// </summary>
        public Stopwatch Age { get; protected set; }

        /// <summary>
        /// Number of frames since touch was released
        /// </summary>
        public int ExpireFrames { get; protected set; }

        /// <summary>
        /// Number of frames since the touch was created
        /// </summary>
        public int AgeFrames { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Provide a unique ID</param>
        public TouchContainer(int id)
        {
            Age = new Stopwatch();
            Id = id;
            Age.Start();
        }

        public event EventHandler OnMainLoopBegin;
        public event EventHandler OnMainLoopEnd;
        public virtual void Mainloop(float deltatime)
        {
            OnMainLoopBegin?.Invoke(this, EventArgs.Empty);
            AgeFrames++;
            ExpireFrames++;
            OnMainLoopEnd?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Call every frame when the touch is present
        /// </summary>
        /// <param name="point">The new touch coordinates</param>
        /// <param name="deltatime">Delta time of a frame (seconds are recommended)</param>
        public virtual void Update(Vector2 point, float deltatime)
        {
            
            if (AgeFrames <= 1)
            {
                Point = point;
                _prevPoint = point;
                Velocity = Vector2.Zero;
                NormalizedVelocity = Vector2.Zero;
            }
            else
            {
                Velocity = point - _prevPoint;
                NormalizedVelocity = Velocity / deltatime;
                _prevPoint = Point;
                Point = point;
            }
            ExpireFrames = 0;
        }

        /// <summary>
        /// Touch equality only depends on its ID. This make it easier to manage them into Dictionaries
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case null:
                    return false;
                case TouchContainer touch:
                    return Id == touch.Id;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Touch equality only depends on its ID. This make it easier to manage them into Dictionaries
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id;
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Generic touch container where you can set attached object type
    /// </summary>
    /// <typeparam name="T">Type of attached object</typeparam>
    public class TouchContainer<T> : TouchContainer
    {
        /// <summary>
        /// Strongly typed attached object
        /// </summary>
        public T AttachedObject
        {
            get => (T) CustomAttachedObject;
            set => CustomAttachedObject = value;
        }
        public TouchContainer(int id) : base(id) { }
    }

    /// <summary>
    /// A touch with 1€ filtering
    /// </summary>
    public class FilteredTouch : TouchContainer
    {
        /// <summary>
        /// Previous point with temporal smoothing
        /// </summary>
        public Vector2 PrevPointFiltered { get; private set; }

        /// <summary>
        /// Touch point with temporal smoothing
        /// </summary>
        public Vector2 PointFiltered { get; private set; }

        /// <summary>
        /// Frame difference of filtered touch point
        /// </summary>
        public Vector2 VelocityFiltered { get; private set; }

        /// <summary>
        /// Filtered touch point speed (distance/seconds)
        /// </summary>
        public Vector2 NormalizedVelocityFiltered { get; private set; }

        private OneEuroFilter FilterX;
        private OneEuroFilter FilterY;

        /// <summary>
        /// Minimum cutoff for 1€ filter
        /// </summary>
        public double FilterMinCutoff
        {
            get => FilterX.MinCutoff;
            set
            {
                FilterX.MinCutoff = value;
                FilterY.MinCutoff = value;
            }
        }

        /// <summary>
        /// Beta of 1€ filter
        /// </summary>
        public double FilterBeta
        {
            get => FilterX.Beta;
            set
            {
                FilterX.Beta = value;
                FilterY.Beta = value;
            }
        }

        /// <summary>
        /// Cutoff derivative for 1€ filter
        /// </summary>
        public double FilterCutoffDerivative
        {
            get => FilterX.CutoffDerivative;
            set
            {
                FilterX.CutoffDerivative = value;
                FilterY.CutoffDerivative = value;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Filtered constructor with options for 1€ filter
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fmincutoff"></param>
        /// <param name="fbeta"></param>
        public FilteredTouch(int id, double fmincutoff, double fbeta) : base(id)
        {
            FilterX = new OneEuroFilter(fmincutoff, fbeta);
            FilterY = new OneEuroFilter(fmincutoff, fbeta);
        }

        /// <inheritdoc />
        public override void Update(Vector2 point, float deltaft)
        {
            base.Update(point, deltaft);

            var rate = 1 / deltaft;

            if (AgeFrames <= 1)
            {
                PrevPointFiltered = point;
                VelocityFiltered = Vector2.Zero;
            }
            else
            {
                VelocityFiltered = PointFiltered - PrevPointFiltered;
                PrevPointFiltered = PointFiltered;
            }

            NormalizedVelocityFiltered = VelocityFiltered * rate;

            PointFiltered = new Vector2(
                (float)FilterX.Filter(point.X, rate),
                (float)FilterY.Filter(point.Y, rate)
            );
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Generic filtered touch container where you can set attached object type
    /// </summary>
    /// <typeparam name="T">Type of attached object</typeparam>
    public class FilteredTouch<T> : FilteredTouch
    {
        /// <summary>
        /// Strongly typed attached object
        /// </summary>
        public T AttachedObject
        {
            get => (T)CustomAttachedObject;
            set => CustomAttachedObject = value;
        }

        public FilteredTouch(int id, double fmincutoff, double fbeta) : base(id, fmincutoff, fbeta) { }
    }
}
