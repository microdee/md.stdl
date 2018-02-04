using System;
using System.Numerics;
using md.stdl.Mathematics;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace md.stdl.Interaction.Notui
{
    public class ElementTransformation : ICopy<ElementTransformation>
    {
        /// <summary>
        /// Position of the element relative to its parent possibly in 3D world
        /// </summary>
        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;
                OnChange?.Invoke(this, EventArgs.Empty);
                InvalidateCache();
            }
        }
        public Vector3 PosVelocity { get; set; }

        /// <summary>
        /// Scale of the element relative to its parent possibly in 3D world
        /// </summary>
        public Vector3 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                OnChange?.Invoke(this, EventArgs.Empty);
                InvalidateCache();
            }
        }

        /// <summary>
        /// Rotation of the element relative to its parent possibly in 3D world
        /// </summary>
        public Quaternion Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                OnChange?.Invoke(this, EventArgs.Empty);
                InvalidateCache();
            }
        }

        public ElementTransformation()
        {
            Position = Vector3.Zero;
            Scale = Vector3.One;
            Rotation = Quaternion.Identity;
            _matrix = Matrix4x4.Identity;
            Cached = true;
        }

        /// <summary>
        /// Copy stateful data from this to another element transform
        /// </summary>
        /// <param name="destination">The receiving end</param>
        public void CopyTo(ElementTransformation destination)
        {
            destination.Position = Position;
            destination.Rotation = Rotation;
            destination.Scale = Scale;
        }

        /// <summary>
        /// Follow reference transform with inertial filtering on position and lowpass on rest
        /// </summary>
        /// <param name="reference">The transformation to follow</param>
        /// <param name="force">The maximum force to use</param>
        /// <param name="alpha">Alpha component of the lowpass filters</param>
        /// <param name="deltaT">Delta time of a hypothetical frame in seconds</param>
        public void FollowWithInertia(ElementTransformation reference, float force, float alpha, float deltaT)
        {
            Filters.Inertial(Position, PosVelocity, reference.Position, force * deltaT, out var pos, out var vel);
            Position = pos;
            PosVelocity = vel;
            Scale = Filters.Lowpass(Scale, reference.Scale, new Vector3(alpha * deltaT));
            Rotation = Filters.Lowpass(Rotation, reference.Rotation, alpha * deltaT);
        }

        /// <summary>
        /// Follow reference transform with lowpass filtering
        /// </summary>
        /// <param name="reference">The transformation to follow</param>
        /// <param name="alpha">Alpha component of the lowpass filters</param>
        /// <param name="deltaT">Delta time of a hypothetical frame in seconds</param>
        public void FollowWithLowpass(ElementTransformation reference, float alpha, float deltaT)
        {
            Position = Filters.Lowpass(Position, reference.Position, new Vector3(alpha * deltaT));
            Scale = Filters.Lowpass(Scale, reference.Scale, new Vector3(alpha * deltaT));
            Rotation = Filters.Lowpass(Rotation, reference.Rotation, alpha * deltaT);
        }

        /// <summary>
        /// Since the last request for the matrix the transformation didn't change.
        /// </summary>
        public bool Cached { get; private set; }

        /// <summary>
        /// Recompute matrix on next request
        /// </summary>
        public void InvalidateCache()
        {
            Cached = false;
        }

        /// <summary>
        /// The actual Matrix transformation
        /// </summary>
        public Matrix4x4 Matrix
        {
            get
            {
                if (Cached) return _matrix;
                _matrix = Matrix4x4.CreateScale(Scale) *
                          Matrix4x4.CreateFromQuaternion(Rotation) *
                          Matrix4x4.CreateTranslation(Position);
                Cached = true;
                return _matrix;
            }
        }

        /// <summary>
        /// Event fired when position, rotation or scale is changed.
        /// </summary>
        /// <remarks>
        /// This will fire on all assignments at position, rotation or scale. Do not do anything expensive here. This is mainly used to invalidate matrix caches on GuiElements
        /// </remarks>
        public event EventHandler OnChange;

        private Matrix4x4 _matrix;
        private Vector3 _position;
        private Vector3 _scale;
        private Quaternion _rotation;

        public object Clone()
        {
            return Copy();
        }

        public ElementTransformation Copy()
        {
            var res = new ElementTransformation();
            CopyTo(res);
            return res;
        }
    }
}
