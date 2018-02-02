using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace md.stdl.Mathematics
{
    public static class Intersections
    {
        /// <summary>
        /// Intersection test with ray and plane via offset and normal
        /// </summary>
        /// <param name="rayOrigin">Ray origin in world</param>
        /// <param name="rayDir">Normalized direction of the ray</param>
        /// <param name="planeCenter">Plane offset in world</param>
        /// <param name="planeNorm">Plane normal in world</param>
        /// <param name="isPoint">The intersection point in world space. 0 if no intersection.</param>
        /// <returns>Intersection happens or not</returns>
        public static bool PlaneRay(Vector3 rayOrigin, Vector3 rayDir, Vector3 planeCenter, Vector3 planeNorm, out Vector3 isPoint)
        {
            var rDotn = Vector3.Dot(Vector3.Normalize(rayDir), planeNorm);

            //parallel to plane or pointing away from plane?
            if (rDotn < 0.0000001)
            {
                isPoint = Vector3.Zero;
                return false;
            }

            var s = Vector3.Dot(planeNorm, planeCenter - rayOrigin) / rDotn;

            isPoint = rayOrigin + s * Vector3.Normalize(rayDir);

            return true;
        }

        /// <summary>
        /// Intersection test with ray and plane via plane transform
        /// </summary>
        /// <param name="rayOrigin">Ray origin in world</param>
        /// <param name="rayDir">Normalized direction of the ray</param>
        /// <param name="planeTr">Plane transformation. The XY plane is used which is looking at the Z+ forward camera (normal(0,0,-1))</param>
        /// <param name="isPoint">The intersection point in world space. 0 if no intersection.</param>
        /// <param name="pointOnPlane">The point in the plane's original space</param>
        /// <returns>Intersection happens or not</returns>
        public static bool PlaneRay(Vector3 rayOrigin, Vector3 rayDir, Matrix4x4 planeTr, out Vector3 isPoint, out Vector3 pointOnPlane)
        {
            var norm = Vector3.TransformNormal(new Vector3(0, 0, -1), planeTr);
            var pos = planeTr.Translation;
            var ishit = PlaneRay(rayOrigin, rayDir, pos, norm, out var wpoint);
            if (ishit)
            {
                isPoint = wpoint;
                Matrix4x4.Invert(planeTr, out var invPlaneTr);
                pointOnPlane = Vector3.Transform(wpoint, invPlaneTr);
                return true;
            }
            isPoint = Vector3.Zero;
            pointOnPlane = Vector3.Zero;
            return false;
        }
    }
}
