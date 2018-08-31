using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceRect = md.stdl.RectPack.RectXYWH;

#pragma warning disable CS1591

namespace md.stdl.RectPack
{
    public class EmptySpaces
    {
        public EmptySpaceList Spaces { get; }
        public RectWH CurrentAABB { get; private set; }
        public bool AllowFlip;

        public EmptySpaces(RectWH r, int spacescap = 16384)
        {
            Spaces = new EmptySpaceList(spacescap);
            Reset(r);
        }

        public void Reset(RectWH r)
        {
            CurrentAABB = new RectWH();
            Spaces.Reset();
            Spaces.Add(new SpaceRect(r));
        }

        public bool Insert(RectWH imagerect, out RectFlipXYWH outresult, Action<SpaceRect> reportCandidateEmptySpace = null)
        {
            for (int i = Spaces.Count - 1; i >= 0; i--)
            {
                var candidateSpace = Spaces[i];
                reportCandidateEmptySpace?.Invoke(candidateSpace);

                RectFlipXYWH AcceptResult(CreatedSplits splits, bool flippingNecessary)
                {
                    Spaces.Remove(i);

                    for (int s = 0; s < splits.Count; s++)
                    {
                        Spaces.Add(splits.Spaces[s]);
                    }

                    var result = new RectFlipXYWH(
                        candidateSpace.X,
                        candidateSpace.Y,
                        imagerect.Width,
                        imagerect.Height,
                        AllowFlip && flippingNecessary,
                        imagerect.Attachment
                    );
                    CurrentAABB = CurrentAABB.ExpandWith<RectWH, RectFlipXYWH>(result);
                    return result;
                }

                CreatedSplits TryToInsert(RectWH img) => CreatedSplits.InsertAndSplit(img, candidateSpace);
                if (AllowFlip)
                {
                    var normal = TryToInsert(imagerect);
                    var flipped = TryToInsert(imagerect.Flip());
                    if (normal && flipped)
                    {
                        outresult = flipped.BetterThan(normal) ?
                            AcceptResult(flipped, true) :
                            AcceptResult(normal, false);
                        return true;
                    }

                    if (normal)
                    {
                        outresult = AcceptResult(normal, false);
                        return true;
                    }
                    if (flipped)
                    {
                        outresult = AcceptResult(flipped, true);
                        return true;
                    }
                }
                else
                {
                    var normal = TryToInsert(imagerect);
                    if (normal)
                    {
                        outresult = AcceptResult(normal, false);
                        return true;
                    }
                }
            }
            outresult = new RectFlipXYWH();
            return false;
        }
    }
}
