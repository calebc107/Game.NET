using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameNET
{
    /// <summary>
    /// Sound to be played
    /// </summary>
    public class Sound : IExpansion
    {
        /// <summary>
        /// Volume of sound, [0,1]
        /// </summary>
        public double volume = 1;
        bool dynamic = false;
        private SourceVoice sourceVoice;
        private string filename;
        private double repeatAt;

        /// <summary>
        /// Create non-repeating sound
        /// </summary>
        /// <param name="filename">Path to WAV file</param>
        public Sound(string filename) : this(filename, -1, true) { }

        /// <summary>
        /// Create repeating sound
        /// </summary>
        /// <param name="filename">Path to WAV file</param>
        /// <param name="repeatAt">Second to repeat at</param>
        /// <param name="dynamic">Should this sound be processed as a stereo sound based on location to camera</param>
        public Sound(string filename, double repeatAt, bool dynamic)
        {
            this.filename = filename;
            this.repeatAt = repeatAt;
            this.dynamic = dynamic;
        }

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
        [HandleProcessCorruptedStateExceptions]
        public void OnCreate()
        {
            sourceVoice = new SourceVoice(filename, (float)repeatAt);
        }

        /// <summary>
        /// Plays the sound
        /// </summary>
        public void Play()
        {
            sourceVoice.Play();
        }
        
        /// <summary>
        /// calculates stereo volumes based on location in relation to camera
        /// </summary>
        void CalculateVolumes()
        {
            var basevol = volume;
            if (dynamic&&!Engine.abort)
            {
                var dist = (parent.pos - Engine.camera.pos).ToVector().r;
                basevol = volume * (1 - dist / (Engine.xres / 2));
                double pan = (parent.pos - Engine.camera.pos).x / (Engine.xres / 2);
                //TODO:reimplement in D3DInterop
                //sourceVoice.SetChannelVolumes(2, new float[] { (float)(1 - pan), (float)(1 + pan) });
            }
            if (sourceVoice != null&&!Engine.abort)
                try
                {
                    //sourceVoice.SetVolume((float)basevol);
                }
                catch { }
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
            CalculateVolumes();
        }

        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public void Stop()
        {
            sourceVoice.Stop();
        }

        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public void onDestroy()
        {
            sourceVoice.Dispose();
        }

        /// <summary>
        /// From IExpansion.cs
        /// </summary>
        public void onRender()
        {
        }
    }
}
