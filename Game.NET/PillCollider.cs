using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameNET
{
    /// <summary>
    /// Really just 3 colliders in a trenchcoat, upper and lower spheres, and one middle rectanglecollider
    /// </summary>
    public class PillCollider : Collider
    {
        static List<Point> Init(int w, int h)
        {
            List<Point> verts = new List<Point>();

            //add top semicircle
            Point basepoint = new Point(0, h / 2 - w / 2);
            for (int i = 0; i <= 180; i++)
            {
                verts.Add(basepoint + new Vector(i, w / 2));
            }

            //add bottom semicircle
            basepoint = new Point(0, -h / 2 + w / 2);
            for (int i = 180; i <= 360; i++)
            {
                verts.Add(basepoint + new Vector(i, w / 2));
            }
            return verts;
        }

        int width;
        int height;
        Point basepoint;
        Point lowerBasepoint;

        /// <summary>
        /// Construct new pillcollider
        /// </summary>
        /// <param name="width">width of middle</param>
        /// <param name="height">Height as measured from the tips of the round ends of pill shape</param>
        public PillCollider(int width, int height) : base(Init(width, height))
        {
            this.width = width;
            this.height = height;
            basepoint = new Point(0, height / 2 - width / 2);
            lowerBasepoint = new Point(0, -height / 2 + width / 2);
        }

        /// <summary>
        /// From Collider.cs
        /// </summary>
        /// <param name="localPoint"></param>
        /// <returns></returns>
        public override Vector GetCorrectionVector(Point localPoint)
        {
            if (localPoint.y > basepoint.y)
            {
                var angle = (localPoint - basepoint).ToVector().θ;
                var r = (width / 2) - (localPoint - basepoint).ToVector().r;
                return new Vector(angle, r);
            }
            else if (localPoint.y > lowerBasepoint.y)
            {
                if (localPoint.x > 0)
                    return new Vector(0, width / 2 - localPoint.x);
                else
                    return new Vector(180, localPoint.x - width / 2);
            }
            else
            {
                var angle = (localPoint - lowerBasepoint).ToVector().θ;
                var r = (width / 2) - (localPoint - lowerBasepoint).ToVector().r;
                return new Vector(angle, r);
            }
        }
    }
}
