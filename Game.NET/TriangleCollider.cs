using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameNET
{
    /// <summary>
    /// Colider for a triangle
    /// </summary>
    public class TriangleCollider : Collider
    {
        static List<Point> toVertecies(int width, int height, TriangleType type)
        {
            List<Point> verts = new List<Point>();
            switch (type)
            {
                case TriangleType.BottomLeft:
                    verts.Add(new Point(-width / 2, height / 2));
                    verts.Add(new Point(-width / 2, -height / 2));
                    verts.Add(new Point(width / 2, -height / 2));
                    break;

                case TriangleType.BottomRight:
                    verts.Add(new Point(width / 2, height / 2));
                    verts.Add(new Point(-width / 2, -height / 2));
                    verts.Add(new Point(width / 2, -height / 2));
                    break;

                case TriangleType.UpperLeft:
                    verts.Add(new Point(width / 2, height / 2));
                    verts.Add(new Point(-width / 2, height / 2));
                    verts.Add(new Point(-width / 2, -height / 2));
                    break;

                case TriangleType.UpperRight:
                    verts.Add(new Point(width / 2, height / 2));
                    verts.Add(new Point(-width / 2, height / 2));
                    verts.Add(new Point(width / 2, -height / 2));
                    break;
            }
            return verts;
        }

        TriangleType type;
        double m;
        double b = 0;
        int width;
        int height;

        /// <summary>
        /// Creates a new TriangleCollider
        /// </summary>
        /// <param name="width">Width of triangle</param>
        /// <param name="height">Height of triangle</param>
        /// <param name="type">Type of Triangle</param>
        public TriangleCollider(int width, int height, TriangleType type) : base(toVertecies(width, height, type))
        {
            this.type = type;
            this.width = width;
            this.height = height;

            //mx+b
            m = height / width;
            if (type == TriangleType.BottomLeft || type == TriangleType.UpperRight)
            {
                m *= -1;
                b = height;
            }
        }

        /// <summary>
        /// From Collider.cs
        /// </summary>
        /// <param name="localPoint"></param>
        /// <returns></returns>
        public override Vector GetCorrectionVector(Point localPoint)
        {
            localPoint = localPoint + new Point(width / 2, height / 2);

            var heightAtX = m * localPoint.x + b;
            if (localPoint.x > width || localPoint.x < 0 || localPoint.y < 0 || localPoint.y > height)
                return new Vector(0, -1);
            if (localPoint.y > heightAtX && (type == TriangleType.BottomLeft || type == TriangleType.BottomRight))
                return new Vector(0, -1);
            else if (localPoint.y < heightAtX && (type == TriangleType.UpperLeft || type == TriangleType.UpperRight))
                return new Vector(0, -1);

            var mi = -1 / m; //m'
            var bi = -mi * localPoint.x + localPoint.y; //b'

            var xIntersect = (bi - b) / (m - mi);
            var yintersect = m * xIntersect + b;

            var edgepoint = new Point(xIntersect, yintersect);
            var correction = (edgepoint - localPoint).ToVector();
            return correction;
        }
    }

    /// <summary>
    /// Specefies which corner the 90-degree angle is
    /// </summary>
    public enum TriangleType
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        BottomRight,
        BottomLeft,
        UpperRight,
        UpperLeft
    }
}
