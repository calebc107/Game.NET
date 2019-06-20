using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace GameNET
{
    /// <summary>
    /// collider that uses a set of heights to determine vertical collision vectors
    /// </summary>
    public class TerrainCollider : Collider
    {
        static TerrainPointSorter sorter = new TerrainPointSorter();
        static double[] bounds;
        private static List<Point> heightmap;

        static List<Point> Init(List<Point> heights, int smoothing)
        {
            List<Point> polypoints = new List<Point>();
            bounds = calculateBounds(heights);
            var height = -bounds[2] + bounds[3];
            var width = -bounds[0] + bounds[1];
            heights.Sort(sorter);

            for (int i = 1; i <= width; i++)
            {
                int j = 0;
                for (j = 0; j < heights.Count - 1; j++)
                {
                    if (i < heights[j].x)
                        break;
                }
                var v1 = heights[j - 1];
                var v2 = heights[j];

                // m=(y2-y1)/(x2-x1)
                var slope = (v2.y - v1.y) / (v2.x - v1.x);
                if (double.IsNaN(slope))
                    slope = 0;
                else if (double.IsPositiveInfinity(slope))
                    slope = 1;
                else if (double.IsNegativeInfinity(slope))
                    slope = -1;
                // y=mx+b
                var y = slope * (i - v1.x) + v1.y;
                polypoints.Add(new Point(i, y));
            }

            Point midpoint = new Point(bounds[0] + width / 2, bounds[2] + height / 2);
            for (int i = 0; i < polypoints.Count; i++)
            {
                //center all of it
                polypoints[i] -= midpoint;
            }

            //smooth
            for (int i = 0; i < smoothing; i++)
            {
                for (int j = 1; j < polypoints.Count - 1; j++)
                {
                    polypoints[j].y = ((polypoints[j - 1].y + polypoints[j + 1].y) / 2);
                }
            }
            heightmap = polypoints;
            List<Point> vertecies = new List<Point>() { heightmap[0] };
            for (int i = 1; i < heightmap.Count - 1; i++)
            {
                //find all points where they are higher than both adjacent points
                if (heightmap[i - 1].y < heightmap[i].y && heightmap[i + 1].y < heightmap[i].y)
                    vertecies.Add(heightmap[i]);
            }
            return vertecies;
        }

        /// <summary>
        /// Creates and smoothes heights for collider
        /// </summary>
        /// <param name="heights">List of points to be used as terrain vectors</param>
        /// <param name="smoothing">iterations to smooth between points</param>
        public TerrainCollider(List<Point> heights, int smoothing) : base(Init(heights, smoothing))
        {
            FindMax();
        }

        /// <summary>
        /// Finds correction by comparing point to heights table
        /// </summary>
        /// <param name="localPoint">point to correct</param>
        /// <returns>correction</returns>
        public override Vector GetCorrectionVector(Point localPoint)
        {
            int x = (int)(localPoint.x + bounds[4] / 2);
            var y = -1.0;
            if (x >= 0 && x < heightmap.Count - 2)
            {
                y = heightmap[x].y;
                return new Vector(90, (y - localPoint.y));
            }
            else return new Vector(90, -1);
        }

        static double[] calculateBounds(List<Point> vertecies)
        {
            double[] bounds = new double[] { double.MaxValue, double.MinValue, double.MaxValue, double.MinValue, 0, 0 };
            for (int i = 0; i < vertecies.Count; i++)
            {
                var vert = vertecies[i];
                if (vert.x < bounds[0])
                    bounds[0] = vert.x;
                if (vert.x > bounds[1])
                    bounds[1] = vert.x;
                if (vert.y < bounds[2])
                    bounds[2] = vert.y;
                if (vert.y > bounds[3])
                    bounds[3] = vert.y;
            }
            bounds[4] = -bounds[0] + bounds[1];
            bounds[5] = -bounds[2] + bounds[3];
            return bounds;
        }
        
        /// <summary>
        /// Will create a sprite based on a texture and the shape of this terrain object
        /// </summary>
        /// <param name="TextureFilename"></param>
        public void RenderSprite(string TextureFilename)
        {
            var width = bounds[4];
            var height = bounds[5];
            System.Drawing.Bitmap lsprite = new System.Drawing.Bitmap((int)(width), (int)(height));
            var canvas = Graphics.FromImage(lsprite);
            var points = new PointF[heightmap.Count + 2];
            for (int i = 0; i < heightmap.Count; i++)
            {
                var localx = heightmap[i].x + width / 2;
                var localy = lsprite.Height - (heightmap[i].y + height / 2);
                points[i] = new PointF((float)(localx), (float)(localy));
            }
            var bruh = new TextureBrush(Image.FromFile(TextureFilename));
            points[points.Length - 2] = new PointF(lsprite.Width, lsprite.Height);
            points[points.Length - 1] = new PointF(0f, lsprite.Height);

            canvas.FillPolygon(bruh, points);
            parent.sprite = Bitmap.FromGDI(lsprite);
        }

        class TerrainPointSorter : IComparer<Point>
        {
            public int Compare(Point p1, Point p2)
            {
                return p1.x.CompareTo(p2.x);
            }
        }
    }
}
