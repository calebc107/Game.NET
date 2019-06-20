namespace GameNET
{
    /// <summary>
    /// Draws a light over the current object if lighting engine is enabled
    /// </summary>
    public class Light : IExpansion
    {
        Bitmap bitmap;
        /// <summary>
        /// //if lighing engine is enabled
        /// </summary>
        public static bool active = false;

        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public GameObject parent { get; set; }
        
        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public int Priority { get { return 0; } }
        
        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public Light(string filename)
        {
            bitmap = Bitmap.fromFile(filename);
        }
        
        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public void OnCreate()
        {

        }
        
        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public void onDestroy()
        {
            bitmap.Dispose();
        }
        
        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public void PreStep()
        {
        }
        
        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public void Step()
        {
        }

        /// <summary>
        /// draw light to screen
        /// </summary>
        public void onRender()
        {
            var loc = Engine.camera.Global2LocalCoords(parent.pos);
            D3DInterop.DrawLight(bitmap.location, (int)loc.x, (int)loc.y);
        }
    }
}
