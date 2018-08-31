using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using md.stdl.Interfaces;
using md.stdl.Mathematics;
using SpaceRect = md.stdl.RectPack.RectXYWH;

#pragma warning disable CS1591

namespace md.stdl.RectPack
{
    public class CreatedSplits : ICloneable<CreatedSplits>
    {
        public int Count;
        public SpaceRect[] Spaces { get; set; } = new SpaceRect[0];

        public static CreatedSplits Failed => new CreatedSplits { Count = -1 };
        public static CreatedSplits None => new CreatedSplits();

        public CreatedSplits() { Count = 0; }

        public CreatedSplits(params SpaceRect[] spaces)
        {
            Spaces = spaces;
            Count = Spaces.Length;
        }

        public bool BetterThan(CreatedSplits other)
        {
            return Count < other.Count;
        }

        public static implicit operator bool(CreatedSplits cs)
        {
            return cs.Count > -1;
        }

        public static CreatedSplits InsertAndSplit(RectWH im, SpaceRect sp)
        {
            var freeW = sp.Width - im.Width;
            var freeH = sp.Height - im.Height;
            if (freeW < 0 || freeH < 0)
                return CreatedSplits.Failed;
            if (freeW.Eq() && freeH.Eq())
                return CreatedSplits.None;

            if (freeW > 0.0f && freeH.Eq())
            {
                var r = sp;
                r.X += im.Width;
                r.Width -= im.Width;
                return new CreatedSplits(r);
            }

            if (freeW.Eq() && freeH > 0)
            {
                var r = sp;
                r.Y += im.Height;
                r.Height -= im.Height;
                return new CreatedSplits(r);
            }

            if (freeW > freeH)
            {
                var bs = new SpaceRect(sp.X + im.Width, sp.Y, freeW, sp.Height);
                var ls = new SpaceRect(sp.X, sp.Y + im.Height, im.Width, freeH);
                return new CreatedSplits(bs, ls);
            }

            var biggersplit = new SpaceRect(sp.X, sp.Y + im.Height, sp.Width, freeH);
            var lessersplit = new SpaceRect(sp.X + im.Width, sp.Y, freeW, im.Height);
            return new CreatedSplits(biggersplit, lessersplit);
        }

        public CreatedSplits Copy()
        {
            return new CreatedSplits
            {
                Spaces = Spaces.ToArray(),
                Count = Count
            };
        }

        public object Clone()
        {
            return Copy();
        }
    }
}
