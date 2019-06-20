using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameNET
{
    /// <summary>
    /// Collider with a custom shape
    /// </summary>
    public class PolygonCollider : Collider
    {
        /// <summary>
        /// Create new polygoncollider
        /// </summary>
        /// <param name="vertecies">List of local points to be used as collider vertecies</param>
        /// <returns></returns>
        public static List<Point> Init(List<Point> vertecies)
        {
            vertecies.Sort(new SortbyAngle());
            return vertecies;
        }

        /// <summary>
        /// Create new polygoncollider
        /// </summary>
        /// <param name="vertecies">List of local points to be used as collider vertecies</param>
        /// <returns></returns>
        public PolygonCollider(List<Point> vertecies) : base(vertecies)
        {
            FindMax();
        }

        /// <summary>
        /// find the slope that is closest to the local point, and return the correction
        /// </summary>
        /// <param name="point">Local point</param>
        /// <returns>Correction</returns>
        public override Vector GetCorrectionVector(Point point)
        {
            var localPoint = point - parent.pos;
            var angle = localPoint.ToVector().θ;
            for (int i = 1; i < vertecies.Count; i++)
            {
                if (vertecies[i].ToVector().θ > localPoint.ToVector().θ)
                {
                    var slope = (vertecies[i].y - vertecies[i - 1].y) / (vertecies[i].x - vertecies[i - 1].x);
                    var slope2 = -1 / slope;
                    var yIntercept = -(slope * vertecies[i].x - vertecies[i].y);
                    var yIntercept2 = -(slope2 * localPoint.x - localPoint.y);
                    var x = (yIntercept2-yIntercept) / (slope - slope2);
                    var point2 = new Point(x, slope * x + yIntercept);
                    var angle2 = Vector.Verify(Math.Atan(slope) / Math.PI * 180 + 90);
                    var dist = Point.Distance(point2, localPoint);
                        return new Vector(angle2, dist);
                }
            }
            return new Vector(0, -1);
        }
    }
    
}
