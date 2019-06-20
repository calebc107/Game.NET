using GameNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameNET
{
    /// <summary>
    /// Abstract class that
    /// </summary>
    public abstract class Collider : IExpansion
    {
        /// <summary>
        /// Collection of all vertecies in this collider/polygon
        /// </summary>
        public List<Point> vertecies = new List<Point>();


        /// <summary>
        /// Contains all other collisions this collider is having
        /// </summary>
        public List<Collision> others = new List<Collision>();

        double maxRange = 0;

        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public int Priority { get { return 1; } }

        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public GameObject parent { get; set; }

        /// <summary>
        /// Expansion that enables the game engine to getect collision between two game objects
        /// </summary>
        /// <param name="vertecies">Collection of all vertecies in this collider/polygon</param>
        public Collider(List<Point> vertecies)
        {
            this.vertecies = vertecies;
            this.vertecies.Sort(new SortbyAngle());
            FindMax();
        }

        /// <summary>
        /// Sets the max range of the collider by analyzing all verticies
        /// </summary>
        public void FindMax()
        {
            for (int i = 0; i < vertecies.Count; i++)
            {
                var current = vertecies[i].ToVector().r;
                if (current > maxRange)
                    maxRange = current;
            }
        }

        /// <summary>
        /// Comparer that sorts all verticies by their angle from the center point
        /// </summary>
        public class SortbyAngle : IComparer<Point>
        {
            /// <summary>
            /// Compares angle of points x and y
            /// </summary>
            /// <param name="x">First point</param>
            /// <param name="y">Second point</param>
            /// <returns>Comparison between the angles of the two points</returns>
            public int Compare(Point x, Point y)
            {
                return x.ToVector().θ.CompareTo(y.ToVector().θ);
            }
        }

        /// <summary>
        /// Gets a vector that specefies how to get the point out of this collider's bounds
        /// </summary>
        /// <param name="localPoint">the local point to find a vector for</param>
        /// <returns>A vector that can be added to point to get a point outside this colliders bounds</returns>
        public abstract Vector GetCorrectionVector(Point localPoint);

        /// <summary>
        /// Returns a collision object if this collider collides with other
        /// </summary>
        /// <param name="other">object to check collision against</param>
        /// <returns>A collision if there is one, Collision.Null if not</returns>
        Collision CollidesWith(Collider other)
        {
            var avg = findAvgCollisionPoint(other);
            if (avg.x != 0 || avg.y != 0)
            {
                Point myGlobal = parent.local2Global(avg);
                Point otherlocal = other.parent.global2Local(myGlobal);
                Vector correction = other.GetCorrectionVector(otherlocal) / 2;
                correction.θ = Vector.Verify(correction.θ + other.parent.θ);
                if (correction != null && correction.r >= 0)
                {
                    return new Collision(other.parent, correction.Flip().θ);
                }
            }
            return Collision.Null;
        }


        /// <summary>
        /// Finds the average collision point between this object and the other collider
        /// </summary>
        /// <param name="other">The other collider to find collision between</param>
        /// <returns>Average collision point in local coordinates</returns>
        public Point findAvgCollisionPoint(Collider other)
        {
            //average it out
            var avg = new Point(0, 0);
            var div = 0;
            for (int i = 0; i < vertecies.Count; i++)
            {
                var myglobal = parent.local2Global(vertecies[i]);
                var otherlocal = other.parent.global2Local(myglobal);
                var correction = other.GetCorrectionVector(otherlocal);
                if (correction != null && correction.r >= 0)
                {
                    try
                    {
                        avg += vertecies[i];
                        div++;
                    }
                    catch (ArgumentOutOfRangeException) { }
                }
            }
            if (div == 0)
                div = 1;
            avg = avg / div;
            return avg;
        }

        Point lastPos = new Point(0, 0);

        /// <summary>
        /// Run logic every tick, but do not force it
        /// </summary>
        public void Step()
        {
            Step(false);
        }

        /// <summary>
        /// Runs all logic for the collider plugin. Will not be forced if ther is a physics expansion present, because it will be run ahain for a physics expansion
        /// </summary>
        /// <param name="force">Disregard presense of physics expansion</param>
        public void Step(bool force)
        {
            //check for other collisions
            var myParent = parent;
            if (myParent is null)
                return;
            bool moved = (myParent.pos != lastPos);
            //bool hasPhys = myParent.expansions[(int)ExpansionTypes.Physics2] != null;
            if (!moved && !force)
                return;
            for (int i = 0; i < Engine.expansions.Count; i++)
            {
                Collider other = null;
                try
                {
                    var exp = Engine.expansions[i];
                    if(exp is Collider)
                    other = (Collider)Engine.expansions[i];
                }
                catch (ArgumentOutOfRangeException) { }

                if (other != null && other != this)
                {
                    var otherParent = other.parent;
                    if (otherParent is null)
                        continue;
                    var distancex = Math.Abs(myParent.pos.x - otherParent.pos.x);
                    var distancey = Math.Abs(myParent.pos.y - otherParent.pos.y);
                    bool inrange = (distancex < (maxRange + other.maxRange) * 1.1) && (distancey < (maxRange + other.maxRange) * 1.1);
                    bool othermoved = (otherParent.pos != other.lastPos);
                    if (inrange && (moved || othermoved || force))
                    {
                        var collision = CollidesWith(other);
                        if (collision != Collision.Null && others.FindIndex(item => item.other == collision.other) < 0)
                        {
                            others.Add(collision);
                            ((ICollidable)parent).OnCollide(collision);
                        }
                        if (collision != Collision.Null && other.others.FindIndex(item => item.other == myParent) < 0)
                        {
                            //var othercol = other.CollidesWith(this);
                            var othercol = new Collision(myParent, Vector.Flip(collision.direction));
                            other.others.Add(othercol);
                            ((ICollidable)other.parent).OnCollide(othercol);
                        }
                    }
                }
            }
            lastPos = myParent.pos;
        }

        /// <summary>
        /// Clear list of others before main logic
        /// </summary>
        public void PreStep()
        {
            others.Clear();
        }

        /// <summary>
        /// Assert that the parent gameobject implemments ICollidable
        /// </summary>
        public void OnCreate()
        {
            if ((parent as ICollidable) == null)
                throw new Exception("parent must implement ICollidable!");
        }

        /// <summary>
        /// Clear arrays for GC
        /// </summary>
        public void onDestroy()
        {
            vertecies.Clear();
            others.Clear();
            parent = null;
        }


        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public void onRender()
        {
        }
    }
}

/// <summary>
/// Represents a collision with another object
/// </summary>
public class Collision
{
    /// <summary>
    /// Reference to GameObject that is being collided with
    /// </summary>
    public GameObject other;

    /// <summary>
    /// Direction to other
    /// </summary>
    public double direction;

    /// <summary>
    /// Creates a reference to GameObject that is being collided with
    /// </summary>
    /// <param name="other">GameObject that is being collided with</param>
    /// <param name="direction">Direction to other</param>
    public Collision(GameObject other, double direction)
    {
        this.other = other;
        this.direction = direction;
    }

    /// <summary>
    /// Null collision
    /// </summary>
    public static Collision Null = new Collision(null, 0);
}

/// <summary>
/// GameObjects must implement this in order to use colliders. It defines steps to be taken when collision occurs
/// </summary>
public interface ICollidable
{
    /// <summary>
    /// Method to be called when two objects collide
    /// </summary>
    /// <param name="collision">Object representation of the current collision</param>
    void OnCollide(Collision collision);
}
