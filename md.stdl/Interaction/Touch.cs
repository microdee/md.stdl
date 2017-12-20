using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VVVV.Utils.Animation;

namespace md.stdl.Interaction
{
    public class TouchContainer
    {
        private Vector2 PrevPoint;

        public Vector2 Point { get; protected set; }
        public Vector2 Velocity { get; protected set; }
        public Vector2 NormalizedVelocity { get; protected set; }
        public int ID { get; protected set; }
        public Stopwatch Age { get; protected set; }
        public int ExpireFrames { get; protected set; }
        public int AgeFrames { get; protected set; }

        public TouchContainer(int id)
        {
            Age = new Stopwatch();
            ID = id;
            Age.Start();
        }

        public virtual void FrameLoop()
        {
            AgeFrames++;
            ExpireFrames++;
        }
        public virtual void Update(Vector2 point, float deltaft)
        {
            
            if (AgeFrames <= 1)
            {
                Point = point;
                PrevPoint = point;
                Velocity = Vector2.Zero;
                NormalizedVelocity = Vector2.Zero;
            }
            else
            {
                Velocity = point - PrevPoint;
                NormalizedVelocity = Velocity / deltaft;
                PrevPoint = Point;
                Point = point;
            }
            ExpireFrames = 0;
        }
    }

    public class FilteredTouch : TouchContainer
    {
        public Vector2 PrevPointFiltered { get; private set; }
        public Vector2 PointFiltered { get; private set; }
        public Vector2 VelocityFiltered { get; private set; }
        public Vector2 NormalizedVelocityFiltered { get; private set; }

        private OneEuroFilter FilterX;
        private OneEuroFilter FilterY;

        public double FilterMinCutoff
        {
            get => FilterX.MinCutoff;
            set
            {
                FilterX.MinCutoff = value;
                FilterY.MinCutoff = value;
            }
        }
        public double FilterBeta
        {
            get => FilterX.Beta;
            set
            {
                FilterX.Beta = value;
                FilterY.Beta = value;
            }
        }
        public double FilterCutoffDerivative
        {
            get => FilterX.CutoffDerivative;
            set
            {
                FilterX.CutoffDerivative = value;
                FilterY.CutoffDerivative = value;
            }
        }

        public FilteredTouch(int id, double fmincutoff, double fbeta) : base(id)
        {
            FilterX = new OneEuroFilter(fmincutoff, fbeta);
            FilterY = new OneEuroFilter(fmincutoff, fbeta);
        }

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
}
