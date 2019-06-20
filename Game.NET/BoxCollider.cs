using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameNET
{
    /// <summary>
    /// Collider with a rectangle shape
    /// </summary>
    public class BoxCollider : Collider
    {
        double width;
        double height;
        double q1;
        double q2;
        double q3;
        double q4;

        /// <summary>
        /// Collider with a rectangle shape
        /// </summary>
        /// <param name="width">Width of the collider in pixels</param>
        /// <param name="height">Height of the collider in pixels</param>
        public BoxCollider(double width, double height) : base(new List<Point>())
        {
            this.width = width;
            this.height = height;

            //add corners
            vertecies.Add(new Point(width / 2, height / 2));
            vertecies.Add(new Point(width / -2, height / 2));
            vertecies.Add(new Point(width / -2, height / -2));
            vertecies.Add(new Point(width / 2, height / -2));
            //add midpoints
            vertecies.Add((vertecies[0] + vertecies[1]) / 2);
            vertecies.Add((vertecies[1] + vertecies[2]) / 2);
            vertecies.Add((vertecies[2] + vertecies[3]) / 2);
            vertecies.Add((vertecies[3] + vertecies[0]) / 2);

            FindMax();

            var offset = new Point(0, 0);
            q1 = (vertecies[0] - offset).ToVector().θ;
            q2 = (vertecies[1] - offset).ToVector().θ;
            q3 = (vertecies[2] + offset).ToVector().θ;
            q4 = (vertecies[3] + offset).ToVector().θ;
        }

        /// <summary>
        /// Gets a vector that specefies how to get the point out of this collider's bounds
        /// </summary>
        /// <param name="localpoint">the loacl point to find a vector for</param>
        /// <returns>A vector that can be added to point to get a point outside this colliders bounds</returns>
        public override Vector GetCorrectionVector(Point localpoint)
        {
            var angle = (localpoint).ToVector().θ;
            if (angle < q1 || angle > q4)
                return new Vector(0, Math.Abs(width / 2) - Math.Abs(localpoint.x));
            if (angle >= q1 && angle <= q2)
                return new Vector(90, Math.Abs(height / 2) - Math.Abs(localpoint.y));
            if (angle > q2 && angle < q3)
                return new Vector(180, Math.Abs(width / 2) - Math.Abs(localpoint.x));
            if (angle >= q3 && angle <= q4)
                return new Vector(270, Math.Abs(height / 2) - Math.Abs(localpoint.y));
            throw new Exception("angle not found");
        }
    }
}
