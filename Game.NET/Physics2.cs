using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameNET
{
    /// <summary>
    /// Expansion that applies physics to gameobjects
    /// </summary>
    public class Physics2 : IExpansion
    {
        private double t = 1 / Engine.targetfps;
        double scale = 100; //100 pixels = 1m
        bool dynamic = true;
        double thresh = 1E-10;
        double maxVelocity = 20;
        Collider parentCollider;

        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public GameObject parent { get; set; }

        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public int Priority { get { return 0; } }

        /// <summary>
        /// mass of object
        /// </summary>
        public double Mass { get; set; }

        /// <summary>
        /// Gravity to be applied to object
        /// </summary>
        public Vector Gravity { get; set; }

        /// <summary>
        /// current objects accelleration
        /// </summary>
        public Vector Acceleration { get; set; }

        /// <summary>
        /// current velocity
        /// </summary>
        public Vector Velocity { get; set; }

        /// <summary>
        /// current angular velocity
        /// </summary>
        public double AVelocity { get; set; }

        /// <summary>
        /// Constructs new Physics2 object
        /// </summary>
        /// <param name="mass">Mass of object</param>
        /// <param name="dynamic">Whether or not this object should move</param>
        public Physics2(double mass, bool dynamic)
        {
            this.dynamic = dynamic;
            if (!dynamic)
                Mass = double.MaxValue;
            else
                Mass = mass;
        }

        /// <summary>
        /// create new physics object
        /// </summary>
        public void OnCreate()
        {
            parentCollider = parent.GetExpansionOfType<Collider>();
            Gravity = new Vector(270, 9.8);
            Velocity = new Vector(0, 0);
        }


        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public void onDestroy()
        {

        }

        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public void PreStep()
        {
            alreadyProcessed.Clear();
        }

        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public void Step()
        {
            if (dynamic)
            {
                Velocity += Gravity * t * Mass;
                parent.pos += Velocity * t * scale;
            }
            ProcessCollisions();
            if (Velocity.r > maxVelocity)
                Velocity.r = maxVelocity;
        }

        List<GameObject> alreadyProcessed = new List<GameObject>();

        /// <summary>
        /// Process all colisions and try to resolve them
        /// </summary>
        void ProcessCollisions()
        {
            parentCollider.Step(true);
            for (int i = 0; i < parentCollider.others.Count; i++) //for all colliding objects
            {
                //calculate correction
                var col = parentCollider.others[i];
                var othercollider = col.other.GetExpansionOfType<Collider>();
                var otherPhys = col.other.GetExpansionOfType<Physics2>();
                Point p = FindAvgLocalPoint(othercollider);
                Point myGlobal = parent.local2Global(p);
                Point otherlocal = othercollider.parent.global2Local(myGlobal);
                Vector correction = othercollider.GetCorrectionVector(otherlocal);
                correction.θ = Vector.Verify(othercollider.parent.θ + correction.θ);

                //if the correction is large enough
                if (otherPhys != null && correction.r > thresh)
                {
                    var othernewVelocity = otherPhys.Velocity;
                    if (!otherPhys.alreadyProcessed.Contains(parent)
                        && otherPhys.dynamic
                        && otherPhys.Move(correction.Flip(), 0, true)) //try to move other object if it hasnt already
                    {
                        var otherCol = new Collision(parent, Vector.Flip(col.direction));
                        othernewVelocity = CalcCollisionVelocity(otherPhys, this, otherCol);
                        //parentCollider.Step(true);
                        otherPhys.alreadyProcessed.Add(parent); //correct the velocity
                    }

                    var newVelocity = Velocity;
                    if (!alreadyProcessed.Contains(otherPhys.parent) && Move(correction, 0, true)) //try to move self
                    {
                        newVelocity = CalcCollisionVelocity(this, otherPhys, col);
                        //CalcCollisionVelocity(this, otherPhys, col);
                        correctAngVelocity(p, correction);
                        alreadyProcessed.Add(otherPhys.parent);
                    }

                    if (othernewVelocity.r > thresh)
                        otherPhys.Velocity = othernewVelocity;
                    if (newVelocity.r > thresh)
                        Velocity = newVelocity;
                }
            }

            //apply friction
            for (int i = 0; i < parentCollider.others.Count; i++)
            {
                var diff = Vector.Difference(parentCollider.others[i].direction, Gravity.θ);
                if (Math.Abs(diff) < 80 && dynamic)
                {
                    Velocity /= 1.05;
                    break;
                }
            }
        }

        //calculates velocity for myPhysics
        static Vector CalcCollisionVelocity(Physics2 myPhysics, Physics2 otherPhysics, Collision col)
        {
            var v1 = myPhysics.Velocity.r;
            var v2 = otherPhysics.Velocity.r;
            var colAngle = col.direction * Vector.Deg2Rad;
            var ang1 = myPhysics.Velocity.θ * Vector.Deg2Rad;
            var ang2 = otherPhysics.Velocity.θ * Vector.Deg2Rad;
            var m1 = myPhysics.Mass;
            var m2 = otherPhysics.Mass;
            if (!otherPhysics.dynamic)
            {
                m2 = m1 * 2;
                v2 = 0;
                ang2 = 0;
            }

            //formulas from https://en.wikipedia.org/wiki/Elastic_collision
            var factor = ((v1 * Math.Cos(ang1 - colAngle) * (m1 - m2) + v2 * 2 * m2 * Math.Cos(ang2 - colAngle)) / (m1 + m2));
            var vfx = factor * Math.Cos(colAngle) - v1 * Math.Sin(ang1 - colAngle) * Math.Sin(colAngle);
            var vfy = factor * Math.Sin(colAngle) + v1 * Math.Sin(ang1 - colAngle) * Math.Cos(colAngle);

            var newVelocity = new Point(vfx, vfy).ToVector();
            return newVelocity;
        }
        
        /// <summary>
        /// correct angular velocity
        /// </summary>
        /// <param name="localCollisionPoint">local point to process</param>
        /// <param name="correction">correction at that point</param>
        public void correctAngVelocity(Point localCollisionPoint, Vector correction)
        {
            //AVelocity = Vector.Difference(localCollisionPoint.ToVector().θ, correction.Flip().θ) * t * -1;
            var pointToCenterAngle = (parent.pos - parent.local2Global(localCollisionPoint)).ToVector().θ;
            if (pointToCenterAngle - correction.θ > 180)
                correction.θ += 360;
            var AVcorrection = (Vector.Difference(pointToCenterAngle, correction.θ)) * t * .1;
            AVelocity += AVcorrection;
        }

        /// <summary>
        /// move this object and any others needed 
        /// </summary>
        /// <param name="correction">directions and madnatude of movement</param>
        /// <param name="n">recursive depth</param>
        /// <param name="move">Actually move the object</param>
        /// <returns></returns>
        private bool Move(Vector correction, int n, bool move)
        {
            var parentcollider = parent.GetExpansionOfType<Collider>();
            if (!dynamic || n > 100) //do not recurse more than 100 objects
                return false;
            for (int i = 0; i < parentcollider.others.Count; i++)
            {
                var col = parentcollider.others[i];
                double diff = Vector.Difference(col.direction, correction.θ);
                if (Math.Abs(diff) < 90)
                {
                    var otherphys = col.other.GetExpansionOfType<Physics2>();
                    if (otherphys != null && !otherphys.Move(correction, n + 1, move))
                        return false;
                }
            }
            //move object
            if (move && correction.r > thresh)
                parent.pos += correction * t * scale;
            return true;
        }

        Point FindAvgLocalPoint(Collider other)
        {
            return parentCollider.findAvgCollisionPoint(other);
        }

        //if we need to change a variable, recalculate all related variables
        void UpdateVars(PhyVar type, Vector value)
        {
            switch (type)
            {
                case PhyVar.a:
                    Acceleration = value;
                    break;
                case PhyVar.p:
                    //Momentum = value;
                    //Velocity = Momentum / Mass;
                    //Acceleration = Velocity / t;
                    break;
                case PhyVar.v:
                    Velocity = value;
                    Acceleration = Velocity / t;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// From Iexpansion.cs
        /// </summary>
        public void onRender()
        {
        }
    }
    enum PhyVar
    {
        v, a, p
    }
}
