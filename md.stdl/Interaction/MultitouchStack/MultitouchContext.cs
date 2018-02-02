﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using md.stdl.Mathematics;

namespace md.stdl.Interaction.MultitouchStack
{
    /// <summary>
    /// Stateless prototype of a TouchContainer
    /// </summary>
    public struct TouchPrototype
    {
        public Vector2 Point;
        public int Id;
        public float Force;
    }

    /// <summary>
    /// Context to manage GuiElements and Touches
    /// </summary>
    public class MultitouchContext
    {
        /// <summary>
        /// Consider touches to be new before the age of this amount of frames
        /// </summary>
        public int ConsiderNewBefore { get; set; } = 1;
        /// <summary>
        /// Ignore and delete touches older than this amount of frames
        /// </summary>
        public int ConsiderReleasedAfter { get; set; } = 1;

        /// <summary>
        /// Optional camera view matrix
        /// </summary>
        public Matrix4x4 View { get; set; } = Matrix4x4.Identity;
        /// <summary>
        /// Optional camera projection matrix
        /// </summary>
        public Matrix4x4 Projection { get; set; } = Matrix4x4.Identity;
        /// <summary>
        /// Very optional aspect ratio correction matrix a'la vvvv
        /// </summary>
        public Matrix4x4 AspectRatio { get; set; } = Matrix4x4.Identity;

        /// <summary>
        /// All the touches in this context
        /// </summary>
        public Dictionary<int, TouchContainer<IGuiElement[]>> Touches { get; } =
            new Dictionary<int, TouchContainer<IGuiElement[]>>();

        /// <summary>
        /// Elements in this context without a parent (or Root elements)
        /// </summary>
        public List<IGuiElement> Elements { get; } = new List<IGuiElement>();

        /// <summary>
        /// All the elements in this context including the children of the root elements recursively
        /// </summary>
        public List<IGuiElement> FlatElementList { get; } = new List<IGuiElement>();

        public MultitouchContext()
        {
        }

        public void Mainloop(List<TouchPrototype> inputTouches, float deltaT)
        {
            // Calculating globals
            Matrix4x4.Invert(AspectRatio, out var invasp);
            Matrix4x4.Invert(Projection, out var invproj);
            Matrix4x4.Invert(View, out var invview);
            var aspproj = Projection * invasp;
            Matrix4x4.Invert(aspproj, out var invaspproj);

            // Removing expired touches
            (from touch in Touches.Values
                where touch.ExpireFrames > ConsiderReleasedAfter
                select touch.Id)
                .ForEach(tid => Touches.Remove(tid));

            // Touches mainloop and reset their hits
            Touches.Values.ForEach(touch =>
            {
                touch.Mainloop();
                touch.AttachedObject = null;
            });

            // Scan through elements if any of them wants to be killed or if there are new ones
            bool rebuild = false;
            if (_elementDeleted)
            {
                foreach (var element in FlatElementList)
                {
                    if (!element.DeleteMe) continue;
                    if (element.Parent == null) Elements.Remove(element);
                    else element.Parent.Children.Remove(element);
                }
                rebuild = true;
                _elementDeleted = false;
            }
            if (_elementAdded)
            {
                rebuild = true;
                _elementAdded = false;
            }
            if(rebuild) BuildFlatList();

            // Process input touches
            foreach (var touch in inputTouches)
            {
                TouchContainer<IGuiElement[]> tt;
                if (Touches.ContainsKey(touch.Id))
                {
                    tt = Touches[touch.Id];
                }
                else
                {
                    tt = new TouchContainer<IGuiElement[]>(touch.Id) { Force = touch.Force };
                    Touches.Add(tt.Id, tt);
                }
                tt.Update(touch.Point, deltaT);

                // Transform touches into world
                var tpw = Vector4.Transform(new Vector4(tt.Point, 0, 1), invaspproj * invview);
                var tpdw = Vector4.Transform(new Vector4(tt.Point, 1, 1), invaspproj * invview);
                tt.WorldPosition = tpw.xyz() / tpw.W;
                tt.ViewDir = Vector3.Normalize(tpdw.xyz() / tpdw.W - tpw.xyz() / tpw.W);
            }

            // preparing elements for hittest
            foreach (var element in FlatElementList)
            {
                var elpos = Vector4.Transform(new Vector4(element.DisplayTransformation.Position, 1), View * aspproj);
                element.Depth = elpos.Z / elpos.W;
                element.Hovering.Clear();
            }

            // look at which touches hit which element
            Touches.Values.ForEach(touch =>
            {
                var hitelements = GetTopInteractableElements(from element in FlatElementList
                    where element.HitTest(touch)
                    orderby element.Depth
                    select element).ToArray();

                touch.AttachedObject = hitelements;

                foreach (var element in hitelements)
                {
                    element.Hovering.Add(touch);
                }
            });

            // Do element logic in parallel
            FlatElementList.AsParallel().ForEach(el =>
            {
                foreach (var touch in Touches.Values)
                {
                    el.ProcessTouch(touch);
                }
                el.MainLoop();
            });
        }

        public void AddElements(params IGuiElement[] elements)
        {
            Elements.AddRange(elements);
            _elementAdded = true;
        }

        private void BuildFlatList()
        {
            foreach (var element in FlatElementList)
            {
                element.OnDeleting -= OnElementDeletion;
                element.OnChildrenAdded -= OnElementAddition;
            }
            FlatElementList.Clear();
            foreach (var element in Elements)
            {
                element.FlattenElements(FlatElementList);
            }

            foreach (var element in FlatElementList)
            {
                element.OnDeleting += OnElementDeletion;
                element.OnChildrenAdded += OnElementAddition;
            }
        }

        private bool _elementDeleted;
        private bool _elementAdded;

        private void OnElementAddition(object sender, ChildrenAddedEventArgs childrenAddedEventArgs)
        {
            _elementAdded = true;
        }

        private void OnElementDeletion(object sender, EventArgs eventArgs)
        {
            _elementDeleted = true;
        }

        private static IEnumerable<IGuiElement> GetTopInteractableElements(IEnumerable<IGuiElement> orderedhitelements)
        {
            if (orderedhitelements == null) yield break;

            foreach (var element in orderedhitelements)
            {
                yield return element;
                if (element.Transparent) continue;
                yield break;
            }
        }
    }
}