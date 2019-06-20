namespace GameNET
{
    /// <summary>
    /// Interface defining common methods between gameObject expansions
    /// </summary>
    public interface IExpansion
    {
        /// <summary>
        /// GameObject that this expanison belongs to
        /// </summary>
        GameObject parent { get; set; }
        /// <summary>
        /// Called when expansion is added to a gameObject
        /// </summary>
        void OnCreate();

        /// <summary>
        /// called once per in-game frame, on the GameLoopThread
        /// </summary>
        void Step();

        /// <summary>
        /// called before Step()
        /// </summary>
        void PreStep();

        /// <summary>
        /// Called when this expansion's parent is destroyed or when expansion is removed
        /// </summary>
        void onDestroy();

        /// <summary>
        /// Called once per in-game frame, but on the rendering thread
        /// </summary>
        void onRender();

        /// <summary>
        /// Used to determine which order expansions are called in. Higher priorities are called before lower ones.
        /// </summary>
        int Priority { get; }
    }
}
