﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using md.stdl.Mathematics;

namespace md.stdl.Interaction.Notui
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
    /// Notui Context to manage GuiElements and Touches
    /// </summary>
    public class NotuiContext
    {
        /// <summary>
        /// Use PLINQ or not?
        /// </summary>
        public bool UseParallel { get; set; } = true;
        /// <summary>
        /// Consider touches to be new before the age of this amount of frames
        /// </summary>
        public int ConsiderNewBefore { get; set; } = 1;
        /// <summary>
        /// Ignore and delete touches older than this amount of frames
        /// </summary>
        public int ConsiderReleasedAfter { get; set; } = 1;
        /// <summary>
        /// To consider a touch minimum this amount of force have to be applied
        /// </summary>
        public float MinimumForce { get; set; } = -1.0f;

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
        /// Inverse of view transform
        /// </summary>
        public Matrix4x4 ViewInverse { get; private set; } = Matrix4x4.Identity;
        /// <summary>
        /// Inverse of projection transform combined with aspect ratio
        /// </summary>
        public Matrix4x4 ProjectionWithAspectRatioInverse { get; private set; } = Matrix4x4.Identity;
        /// <summary>
        /// Projection transform combined with aspect ratio
        /// </summary>
        public Matrix4x4 ProjectionWithAspectRatio { get; private set; } = Matrix4x4.Identity;
        /// <summary>
        /// Camera Position in world
        /// </summary>
        public Vector3 ViewPosition { get; private set; } = Vector3.Zero;
        /// <summary>
        /// Camera view direction in world
        /// </summary>
        public Vector3 ViewDirection { get; private set; } = Vector3.UnitZ;
        /// <summary>
        /// Camera view orientation in world
        /// </summary>
        public Quaternion ViewOrientation { get; private set; } = Quaternion.Identity;
        /// <summary>
        /// Delta time between mainloop calls in seconds
        /// </summary>
        /// <remarks>
        /// This is provided by the implementer in the Mainloop args
        /// </remarks>
        public float DeltaTime { get; private set; } = 0;

        /// <summary>
        /// All the touches in this context
        /// </summary>
        public ConcurrentDictionary<int, TouchContainer<NotuiElement[]>> Touches { get; } =
            new ConcurrentDictionary<int, TouchContainer<NotuiElement[]>>();

        /// <summary>
        /// Elements in this context without a parent (or Root elements)
        /// </summary>
        public Dictionary<string, NotuiElement> RootElements { get; } = new Dictionary<string, NotuiElement>();

        /// <summary>
        /// All the elements in this context including the children of the root elements recursively
        /// </summary>
        public List<NotuiElement> FlatElements { get; } = new List<NotuiElement>();

        /// <summary>
        /// Call this function every frame in your own main loop
        /// </summary>
        /// <param name="inputTouches">List of touch prototypes present this frame</param>
        /// <param name="deltaT">Delta time in seconds (so 0.001 is 1 ms)</param>
        public void Mainloop(List<TouchPrototype> inputTouches, float deltaT)
        {
            // Calculating globals
            Matrix4x4.Invert(AspectRatio, out var invasp);
            //Matrix4x4.Invert(Projection, out var invproj);
            Matrix4x4.Invert(View, out var invview);
            var aspproj = Projection * invasp;
            Matrix4x4.Invert(aspproj, out var invaspproj);

            ViewInverse = invview;
            ProjectionWithAspectRatio = aspproj;
            ProjectionWithAspectRatioInverse = invaspproj;
            DeltaTime = deltaT;

            Matrix4x4.Decompose(invview, out var vscale, out var vquat, out var vpos);
            ViewOrientation = vquat;
            ViewPosition = vpos;
            ViewDirection = Vector3.Normalize(Vector3.TransformNormal(Vector3.UnitZ, View));

            // Removing expired touches
            var removabletouches = (from touch in Touches.Values
                where touch.ExpireFrames > ConsiderReleasedAfter
                select touch.Id).ToArray();
            foreach (var tid in removabletouches)
            {
                Touches.TryRemove(tid, out var dummy);
            }

            // Touches mainloop and reset their hits
            Touches.Values.ForEach(touch =>
            {
                touch.Mainloop();
                touch.AttachedObject = null;
            });

            // Scan through elements if any of them wants to be killed or if there are new ones
            bool rebuild = false;
            if (_elementsDeleted)
            {
                foreach (var element in FlatElements)
                {
                    if (!element.DeleteMe) continue;
                    if (element.Parent == null) RootElements.Remove(element.Id);
                    else element.Parent.Children.Remove(element.Id);
                }
                rebuild = true;
                _elementsDeleted = false;
            }
            if (_elementsUpdated)
            {
                rebuild = true;
                _elementsUpdated = false;
            }
            if(rebuild) BuildFlatList();

            // Process input touches
            foreach (var touch in inputTouches)
            {
                TouchContainer<NotuiElement[]> tt;
                if (Touches.ContainsKey(touch.Id))
                {
                    tt = Touches[touch.Id];
                }
                else
                {
                    tt = new TouchContainer<NotuiElement[]>(touch.Id) { Force = touch.Force };
                    Touches.TryAdd(tt.Id, tt);
                }
                tt.Update(touch.Point, deltaT);
            }

            // preparing elements for hittest
            foreach (var element in FlatElements)
            {
                //Matrix4x4.Decompose(element.DisplayMatrix, out var aelscale, out var aelrot, out var aelpos);
                //var elpos = Vector4.Transform(new Vector4(aelpos, 1), View * aspproj);
                //element.Depth = elpos.Z / elpos.W;
                element.Hovering.Clear();
            }

            // look at which touches hit which element
            void ProcessTouches(TouchContainer<NotuiElement[]> touch)
            {
                // Transform touches into world
                Coordinates.GetPointWorldPosDir(touch.Point, invaspproj, invview, out var tpw, out var tpd);
                touch.WorldPosition = tpw;
                touch.ViewDir = tpd;

                // get hitting intersections and order them from closest to furthest
                var intersections = FlatElements.Select(el =>
                    {
                        var intersection = el.HitTest(touch);
                        if (intersection != null) intersection.Element = el;
                        return intersection;
                    })
                    .Where(insec => insec != null)
                    .Where(insec => insec.Element.Active)
                    .OrderBy(insec =>
                    {
                        var screenpos = Vector4.Transform(new Vector4(insec.WorldSpace, 1), View * aspproj);
                        return screenpos.Z / screenpos.W;
                    });

                // Sift through ordered intersection list until the furthest non-transparent element
                // or in other words ignore all intersected elements which are further away from the closest non-transparent element
                var passedintersections = GetTopInteractableElements(intersections);

                // Add the touch and the corresponding intersection point to the interacting elements
                // and attach those elements to the touch too.
                touch.AttachedObject = passedintersections.Select(insec =>
                {
                    insec.Element.Hovering.TryAdd(touch, insec);
                    return insec.Element;
                }).ToArray();

            }
            if(UseParallel) Touches.Values.AsParallel().ForAll(ProcessTouches);
            else Touches.Values.ForEach(ProcessTouches);

            // Do element logic
            void ProcessElements(NotuiElement el)
            {
                foreach (var touch in Touches.Values)
                {
                    el.ProcessTouch(touch);
                }
                el.MainLoop();
            }
            if(UseParallel) FlatElements.AsParallel().ForAll(ProcessElements);
            else FlatElements.ForEach(ProcessElements);
        }
        
        /// <summary>
        /// Instantiate new elements and update existing elements from the input prototypes. Optionally start the deletion of elements which are not present in the input array.
        /// </summary>
        /// <param name="removeNotPresent">When true elements will be deleted if their prototype with the same ID is not found in the input array</param>
        /// <param name="elements">Input prototypes</param>
        /// <returns>List of the newly instantiated elements</returns>
        public List<NotuiElement> AddOrUpdateElements(bool removeNotPresent, params ElementPrototype[] elements)
        {
            var newelements = new List<NotuiElement>();
            if (removeNotPresent)
            {
                var removables = (from el in RootElements.Values where elements.All(c => c.Id != el.Id) select el).ToArray();
                foreach (var el in removables)
                {
                    el.StartDeletion();
                }
            }

            foreach (var el in elements)
            {
                if (RootElements.ContainsKey(el.Id))
                    RootElements[el.Id].UpdateFrom(el);
                else
                {
                    var elinst = el.Instantiate(this);
                    RootElements.Add(el.Id, elinst);
                    newelements.Add(elinst);
                }
            }
            _elementsUpdated = true;
            return newelements;
        }

        private void BuildFlatList()
        {
            foreach (var element in FlatElements)
            {
                element.OnDeleting -= OnElementDeletion;
                element.OnChildrenUpdated -= OnElementUpdate;
            }
            FlatElements.Clear();
            foreach (var element in RootElements.Values)
            {
                element.FlattenElements(FlatElements);
            }

            foreach (var element in FlatElements)
            {
                element.OnDeleting += OnElementDeletion;
                element.OnChildrenUpdated += OnElementUpdate;
            }
        }

        private bool _elementsDeleted;
        private bool _elementsUpdated;

        private void OnElementUpdate(object sender, ChildrenUpdatedEventArgs childrenAddedEventArgs)
        {
            _elementsUpdated = true;
        }

        private void OnElementDeletion(object sender, EventArgs eventArgs)
        {
            _elementsDeleted = true;
        }

        private static IEnumerable<IntersectionPoint> GetTopInteractableElements(IEnumerable<IntersectionPoint> orderedhitinsecs)
        {
            if (orderedhitinsecs == null) yield break;

            foreach (var insec in orderedhitinsecs)
            {
                yield return insec;
                if (insec.Element.Transparent) continue;
                yield break;
            }
        }
    }
}
