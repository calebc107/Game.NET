using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameNET
{
    /// <summary>
    /// represents x and y coordinates
    /// </summary>
    public class Point
    {
        /// <summary>
        /// x coordinate
        /// </summary>
        public double x;

        /// <summary>
        /// y coordinate
        /// </summary>
        public double y;

        /// <summary>
        /// represents x and y coordinates
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// used to calculate the distance between two points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.x - p2.x, 2) + Math.Pow(p1.y - p2.y, 2));
        }

        /// <summary>
        /// converts this x,y coordinate into a vector
        /// </summary>
        /// <returns>Vector representation of this point</returns>
        public Vector ToVector()
        {
            double θ = Math.Atan(y / x) * 180 / Math.PI;
            double r = Math.Sqrt(x * x + y * y);

            //make adjustments for quadrants
            if (y >= 0)
            {
                if (x >= 0) θ += 0;
                else θ += 180;
            }
            else
            {
                if (x < 0) θ += 180;
                else θ += 360;
            }
            if (double.IsNaN(θ))
                θ = 90;
            return new Vector(θ, r);
        }

        /// <summary>
        /// adds 2 points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static Point operator +(Point p1, Point p2)
        {
            return new Point(p1.x + p2.x, p1.y + p2.y);
        }

        /// <summary>
        /// adds a vector to a point
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Point operator +(Point p1, Vector v1)
        {
            return p1 + v1.ToPoint();
        }

        /// <summary>
        /// represents object as "(x,y)"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "(" + x + "," + y + ")";
        }

        /// <summary>
        /// subtracts 2 points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static Point operator -(Point p1, Point p2)
        {
            return new Point(p1.x - p2.x, p1.y - p2.y);
        }

        /// <summary>
        /// divides a point
        /// </summary>
        /// <param name="p1">Point to divide</param>
        /// <param name="div">Divisor</param>
        /// <returns></returns>
        public static Point operator /(Point p1, int div)
        {
            return new Point(p1.x / div, p1.y / div);
        }
    }

    /// <summary>
    /// Class that represents polar coordinates
    /// </summary>
    public class Vector
    {
        /// <summary>
        /// multipication constant for conversion
        /// </summary>
        public const double Rad2Deg = 180 / Math.PI;

        /// <summary>
        /// multipication constant for conversion
        /// </summary>
        public const double Deg2Rad = 1 / Rad2Deg;
        /// <summary>
        /// angle of vector
        /// </summary>
        public double θ;

        /// <summary>
        /// distance of vector
        /// </summary>
        public double r;

        /// <summary>
        /// Represents a direction and distance
        /// </summary>
        /// <param name="θ">angle of vector in degrees</param>
        /// <param name="r">distance of vector in pixels</param>
        public Vector(double θ, double r)
        {
            this.θ = θ;
            this.r = r;
        }

        /// <summary>
        /// Converts vector into x,y coordinate
        /// </summary>
        /// <returns></returns>
        public Point ToPoint()
        {
            var x = r * Math.Cos(θ / 180 * Math.PI);
            var y = r * Math.Sin(θ / 180 * Math.PI);
            return new Point(x, y);
        }

        /// <summary>
        /// changed the direction of this vector, but retains the same distance
        /// </summary>
        /// <param name="θ"></param>
        /// <returns></returns>
        public static double Flip(double θ)
        {
            return Verify(θ + 180);
        }

        /// <summary>
        /// find the acute difference between two ancles
        /// </summary>
        /// <param name="angle1"></param>
        /// <param name="angle2"></param>
        /// <returns></returns>
        public static double Difference(double angle1, double angle2)
        {
            var r1 = angle1 - angle2;
            var r2 = angle1 + 360 - angle2;
            if (Math.Abs(r1) < Math.Abs(r2))
                return r1;
            else
                return r2;
        }

        /// <summary>
        /// flips the specefied angle to its polar opposite
        /// </summary>
        /// <returns>The flipped Vector</returns>
        public Vector Flip()
        {
            return new Vector(Verify(θ + 180), r);
        }

        /// <summary>
        /// adds or subtracts 360 so that angle is between 0 and 360 degrees
        /// </summary>
        /// <param name="angle"></param>
        /// <returns>an angle between 0 and 360</returns>
        public static double Verify(double angle)
        {
            while (angle < 0 || angle >= 360)
            {
                if (angle < 0)
                    angle += 360;
                else
                    angle -= 360;
            }
            return angle;
        }

        /// <summary>
        /// adds 2 vectors
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static Vector operator +(Vector p1, Vector p2)
        {

            return (p1.ToPoint() + p2.ToPoint()).ToVector();
        }

        /// <summary>
        /// subtracts 2 vectors
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static Vector operator -(Vector p1, Vector p2)
        {
            return (p1.ToPoint() - p2.ToPoint()).ToVector();
        }

        /// <summary>
        /// divides two vectors
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static Vector operator /(Vector p1, double n)
        {
            if (n < 0)
                p1 = p1.Flip();
            return new Vector(p1.θ, p1.r / Math.Abs(n));
        }

        /// <summary>
        /// multiplies two vectors
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static Vector operator *(Vector p1, double n)
        {
            if (n < 0)
                p1 = p1.Flip();
            return new Vector(p1.θ, p1.r * Math.Abs(n));
        }

        /// <summary>
        /// Represents vector as (θ,r)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "(θ=" + θ + ", r=" + r + ")";
        }
    }
}
