using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameNET
{
    /// <summary>
    /// Collider with a circle shape
    /// </summary>
    public class CircleCollider : Collider
    {
        double radius;

        /// <summary>
        /// Creates a collider with a circle shape
        /// </summary>
        /// <param name="radius">the radius of the circle in pixels</param>
        public CircleCollider (double radius) : base(new List<Point>())
        {
            this.radius = radius;
            for (int i = 0; i < 360; i+=5)
            {
                vertecies.Add(new Vector(i, radius).ToPoint());
            }
            FindMax();
        }

        /// <summary>
        /// Gets a vector that specefies how to get the point out of this collider's bounds
        /// </summary>
        /// <param name="localPoint">the local point to find a vector for</param>
        /// <returns>A vector that can be added to point to get a point outside this colliders bounds</returns>
        public override Vector GetCorrectionVector(Point localPoint)
        {
            var angle = localPoint.ToVector().θ;
            var r = radius-localPoint.ToVector().r;
            return new Vector(angle, r);
        }
    }
}
