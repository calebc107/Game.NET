using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameNET
{
    /// <summary>
    /// This expansion allows for gameobjects to be animated by placing multiple frames in a specified folder
    /// </summary>
    public class AnimatedSprite : IExpansion
    {
        int currentindex = 0;
        int delay = -1;
        int start = 0;
        int end = 0;
        int repeat = 0;
        double last = 0;


        Bitmap[] frames;

        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public GameObject parent { get; set; }

        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public int Priority { get { return 0; } }

        /// <summary>
        /// Creates a new AnimatedSprite and loads all images in order from the specified directory
        /// </summary>
        /// <param name="path">Path to the directory containing images </param>
        public AnimatedSprite(string path)
        {
            var filenames = Directory.GetFiles(path);
            frames = new Bitmap[filenames.Length];
            for (int i = 0; i < filenames.Length; i++)
            {
                    frames[i] = Bitmap.fromFile(filenames[i]);
            }
        }

        /// <summary>
        /// Sets the currently playing frames
        /// </summary>
        /// <param name="start">Frame index to start this animation loop</param>
        /// <param name="end">Frame index to end this animation loop</param>
        /// <param name="delay">Game ticks to wait before moving to the next frame</param>
        /// <param name="repeat">Number of times to repeat animation loop, set to -1 for infinite</param>
        public void Play(int start, int end, int delay, int repeat)
        {
            currentindex = start;
            this.start = start;
            this.end = end;
            this.delay = delay;
            this.repeat = repeat;
        }

        /// <summary>
        /// Looks at the parameters of the defined 
        /// </summary>
        public void Step()
        {
            if (delay > 0 && delay < (Engine.tick - last))
            {
                last = Engine.tick;
                currentindex++;
                if (currentindex > end)
                {
                    currentindex = start;
                    repeat--;
                }
                if (repeat == 0)
                    delay = -1;
                parent.sprite = frames[currentindex];
            }
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
        public void PreStep()
        {
        }

        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public void onDestroy()
        {
            parent.sprite = null;
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i].Dispose();
                frames[i] = null;
            }
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
