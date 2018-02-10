using System;
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
        /// All the touches in this context
        /// </summary>
        public ConcurrentDictionary<int, TouchContainer<NotuiElement[]>> Touches { get; } =
            new ConcurrentDictionary<int, TouchContainer<NotuiElement[]>>();

        /// <summary>
        /// Elements in this context without a parent (or Root elements)
        /// </summary>
        public Dictionary<string, NotuiElement> Elements { get; } = new Dictionary<string, NotuiElement>();

        /// <summary>
        /// All the elements in this context including the children of the root elements recursively
        /// </summary>
        public Dictionary<string, NotuiElement> FlatElementList { get; } = new Dictionary<string, NotuiElement>();

        public NotuiContext()
        {
        }

        public void Mainloop(List<TouchPrototype> inputTouches, float deltaT)
        {
            // Calculating globals
            Matrix4x4.Invert(AspectRatio, out var invasp);
            //Matrix4x4.Invert(Projection, out var invproj);
            Matrix4x4.Invert(View, out var invview);
            var aspproj = Projection * invasp;
            Matrix4x4.Invert(aspproj, out var invaspproj);

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
                foreach (var element in FlatElementList.Values)
                {
                    if (!element.DeleteMe) continue;
                    if (element.Parent == null) Elements.Remove(element.Id);
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
            foreach (var element in FlatElementList.Values)
            {
                var elpos = Vector4.Transform(new Vector4(element.DisplayTransformation.Position, 1), View * aspproj);
                element.Depth = elpos.Z / elpos.W;
                element.Hovering.Clear();
            }

            // look at which touches hit which element
            Touches.Values.AsParallel().ForAll(touch =>
            {
                // Transform touches into world
                var tpw = Vector4.Transform(new Vector4(touch.Point, 0, 1), invaspproj * invview);
                var tpdw = Vector4.Transform(new Vector4(touch.Point, 1, 1), invaspproj * invview);
                touch.WorldPosition = tpw.xyz() / tpw.W;
                touch.ViewDir = Vector3.Normalize(tpdw.xyz() / tpdw.W - tpw.xyz() / tpw.W);

                // get hitting intersections and order them from closest to furthest
                var intersections = FlatElementList.Values.Select(el =>
                    {
                        var intersection = el.HitTest(touch);
                        if (intersection != null) intersection.Element = el;
                        return intersection;
                    })
                    .Where(insec => insec != null)
                    .Where(insec => insec.Element.Active)
                    .OrderBy(insec => insec.Element.Depth);

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
                
            });

            // Do element logic in parallel
            FlatElementList.Values.AsParallel().ForAll(el =>
            {
                foreach (var touch in Touches.Values)
                {
                    el.ProcessTouch(touch);
                }
                el.MainLoop();
            });
        }

        public void AddOrUpdateElements(bool removeNotPresent, bool updateTransformOfRemovable, params ElementPrototype[] elements)
        {

            if (removeNotPresent)
            {
                var removables = (from el in FlatElementList.Values where elements.All(c => c.Id != el.Id) select el).ToArray();
                foreach (var el in removables)
                {
                    el.StartDeletion();
                }
            }

            foreach (var el in elements)
            {
                if (FlatElementList.ContainsKey(el.Id))
                    FlatElementList[el.Id].UpdateFrom(el);
                else
                {
                    if (el.Parent == null)
                    {
                        Elements.Add(el.Id, el.Instantiate(this));
                        continue;
                    }
                    if (FlatElementList.ContainsKey(el.Parent.Id))
                    {
                        FlatElementList[el.Parent.Id].UpdateChildren(false, el);
                    }
                }
            }
            _elementsUpdated = true;
        }

        private void BuildFlatList()
        {
            foreach (var element in FlatElementList.Values)
            {
                element.OnDeleting -= OnElementDeletion;
                element.OnChildrenUpdated -= OnElementUpdate;
            }
            FlatElementList.Clear();
            foreach (var element in Elements.Values)
            {
                element.FlattenElements(FlatElementList);
            }

            foreach (var element in FlatElementList.Values)
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
