using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceRect = md.stdl.RectPack.RectXYWH;

#pragma warning disable CS1591

namespace md.stdl.RectPack
{
    public class EmptySpaceList
    {
        public List<SpaceRect> EmptySpaces { get; }

        public EmptySpaceList(int cap = 16384)
        {
            EmptySpaces = new List<SpaceRect>(cap);
        }

        public SpaceRect Add(SpaceRect r)
        {
            EmptySpaces.Add(r);
            return r;
        }

        public SpaceRect Remove(int i)
        {
            var pop = EmptySpaces[i];
            EmptySpaces.RemoveAt(i);
            return pop;
        }

        public void Reset() => EmptySpaces.Clear();

        public SpaceRect this[int i]
        {
            get => EmptySpaces[i];
            set => EmptySpaces[i] = value;
        }

        public int Count => EmptySpaces.Count;
    }
}
