using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

#pragma warning disable CS1591

namespace md.stdl.RectPack
{
    public interface IAttachment
    {
        object Attachment { get; set; }
    }
    public interface IRxy
    {
        float X { get; set; }
        float Y { get; set; }

        Vector2 Position { get; }
    }
    public interface IRwh
    {
        float Width { get; set; }
        float Height { get; set; }

        Vector2 Size { get; }

        float LongerSide { get; }
        float ShorterSide { get; }

        float Area { get; }
        float Perimeter { get; }
        float PathologicalMult { get; }
    }

    public struct RectWH : IRwh, IAttachment
    {
        public float Width { get; set; }
        public float Height { get; set; }

        public object Attachment { get; set; }

        public Vector2 Size => new Vector2(Width, Height);

        public RectWH Flip()
        {
            return new RectWH
            {
                Width = Height,
                Height = Width
            };
        }

        public RectWH(float w, float h, object a = null)
        {
            Width = w;
            Height = h;
            Attachment = a;
        }

        public RectWH(Vector2 v, object a = null)
        {
            Width = v.X;
            Height = v.Y;
            Attachment = a;
        }

        public float LongerSide => Max(Width, Height);
        public float ShorterSide => Max(Width, Height);
        
        public float Area => Width * Height;
        public float Perimeter => Width * 2 + Height * 2;
        public float PathologicalMult => LongerSide / ShorterSide * Area;

        public TOut ExpandWith<TOut, TIn>(TIn r)
            where TIn: struct, IRwh, IRxy
            where TOut: struct, IRwh
        {
            return new TOut
            {
                Width = Max(Width, r.X + r.Width),
                Height = Max(Height, r.Y + r.Height)
            };
        }
    }

    public struct RectXYWH : IRxy, IRwh, IAttachment
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public object Attachment { get; set; }

        public Vector2 Size => new Vector2(Width, Height);
        public Vector2 Position => new Vector2(X, Y);

        public float LongerSide => Max(Width, Height);
        public float ShorterSide => Max(Width, Height);

        public float Area => Width * Height;
        public float Perimeter => Width * 2 + Height * 2;
        public float PathologicalMult => LongerSide / ShorterSide * Area;

        public RectXYWH(float x, float y, float w, float h, object a = null)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
            Attachment = a;
        }

        public RectXYWH(RectWH r, object a = null)
        {
            Width = r.Width;
            Height = r.Height;
            X = 0.0f;
            Y = 0.0f;
            Attachment = a ?? r.Attachment;
        }

        public RectXYWH(Vector4 v, object a = null)
        {
            Width = v.Z;
            Height = v.W;
            X = v.X;
            Y = v.Y;
            Attachment = a;
        }

        public RectWH GetWH() => new RectWH { Width = Width, Height = Height };
    }

    public struct RectFlipXYWH : IRxy, IRwh, IAttachment
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Flipped { get; set; }

        public object Attachment { get; set; }

        public Vector4 AsVector => new Vector4(X, Y, Width, Height);
        public Vector2 Size => new Vector2(Width, Height);
        public Vector2 Position => new Vector2(X, Y);

        public float LongerSide => Max(Width, Height);
        public float ShorterSide => Max(Width, Height);

        public float Area => Width * Height;
        public float Perimeter => Width * 2 + Height * 2;
        public float PathologicalMult => LongerSide / ShorterSide * Area;

        public RectFlipXYWH(float x, float y, float w, float h, bool f, object a = null)
        {
            X = x;
            Y = y;
            Width = f ? h : w;
            Height = f ? w : h;
            Flipped = f;
            Attachment = a;
        }

        public RectFlipXYWH(RectWH r, object a = null)
        {
            Width = r.Width;
            Height = r.Height;
            X = 0.0f;
            Y = 0.0f;
            Flipped = false;
            Attachment = a ?? r.Attachment;
        }
        public RectFlipXYWH(RectXYWH r, object a = null)
        {
            Width = r.Width;
            Height = r.Height;
            X = r.X;
            Y = r.Y;
            Flipped = false;
            Attachment = a ?? r.Attachment;
        }
        public RectFlipXYWH(Vector4 v, object a = null)
        {
            Width = v.Z;
            Height = v.W;
            X = v.X;
            Y = v.Y;
            Flipped = false;
            Attachment = a;
        }

        public RectWH GetWH() => new RectWH { Width = Width, Height = Height };
        public RectXYWH GetXYWH() => new RectXYWH { Width = Width, Height = Height, X = X, Y = Y };
    }
}
