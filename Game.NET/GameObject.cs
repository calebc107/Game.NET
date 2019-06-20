using System;
using System.Collections.Generic;

namespace GameNET
{
    /// <summary>
    /// an object to be tracked and rendered by the game engine
    /// </summary>
    public abstract class GameObject
    {
        //transform information
        /// <summary>
        /// image representation of the object
        /// </summary>
        public Bitmap sprite;// = new IntPtr();

        /// <summary>
        /// Layer/order to draw object in
        /// </summary>
        public int layer = 0;

        /// <summary>
        /// GameObjects will only run logic if active is true, use to disable logic to control performance for farther away objects
        /// </summary>
        public bool active = true;

        /// <summary>
        /// Global coordinates for the location of the object
        /// </summary>
        public Point pos = new Point(0, 0);

        /// <summary>
        /// Rotation of object
        /// </summary>
        public double θ = 0;

        /// <summary>
        /// List of expansions that this object has
        /// </summary>
        public List<IExpansion> expansions = new List<IExpansion>();

        /// <summary>
        /// Add a new expansion
        /// </summary>
        /// <param name="expansion">Expansion to add</param>
        public void AddExpansion(IExpansion expansion)
        {
            expansion.parent = this;
            expansion.OnCreate();
            expansions.Add(expansion);
        }

        /// <summary>
        /// Remove expansion from object
        /// </summary>
        /// <param name="expansion">Expansion to remove</param>
        public void RemoveExpansion(IExpansion expansion)
        {
            expansion.onDestroy();
            expansions.Remove(expansion);
        }

        /// <summary>
        /// returns all expansions of this object
        /// </summary>
        /// <returns></returns>
        public List<IExpansion> GetAllExpansions()
        {
            return expansions;
        }

        /// <summary>
        /// returns the expantion of the specefied type
        /// </summary>
        /// <typeparam name="T">Type of expansion to return</typeparam>
        /// <returns>expansion of type T</returns>
        public T GetExpansionOfType<T>()
        {
            for (int i = 0; i < expansions.Count; i++)
            {
                if (expansions[i] is T)
                    return (T)expansions[i];
            }
            return default(T);
        }

        /// <summary>
        /// convert global coordinates to local coordinates
        /// </summary>
        /// <param name="global">Globla coordinates</param>
        /// <returns>Local coordinates</returns>
        public Point global2Local(Point global)
        {
            if (θ != 0)
            {
                var relativeLocal = (global - pos).ToVector();
                var reletiveVector = new Vector(Vector.Verify(relativeLocal.θ - θ), relativeLocal.r);
                var localPoint = reletiveVector.ToPoint();
                return localPoint;
            }
            else
                return global - pos;
        }

        /// <summary>
        /// convert local coordinates to global coordinates
        /// </summary>
        /// <param name="local">local coordinates relative to object</param>
        /// <returns>Global coordinates</returns>
        public Point local2Global(Point local)
        {
            if (θ != 0)
            {
                var localVector = local.ToVector();
                var reletiveLocalVector = new Vector(Vector.Verify(localVector.θ + θ), localVector.r);
                var global = reletiveLocalVector.ToPoint() + pos;
                return global;
            }
            else
                return local + pos;
        }

        /// <summary>
        /// method to be called with every frame
        /// </summary>
        abstract public void Step();

        /// <summary>
        /// method that is called on object creation
        /// </summary>
        abstract public void OnCreate();

        /// <summary>
        /// method that is called when the gameObject is destroyed
        /// </summary>
        abstract public void OnDestroy();

        /// <summary>
        /// Runs OnDestroy for self and expansions
        /// </summary>
        public void Destroy()
        {
            OnDestroy();
            for (int i = 0; i < expansions.Count; i++)
            {
                if (expansions[i] != null)
                    expansions[i].onDestroy();
            }
        }
    }
}
