using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameNET
{
    /// <summary>
    /// Class that helps with input-related functions
    /// </summary>
    public static class Input
    {
        static int[] pairs = new int[14];

        /// <summary>
        /// converts windows keys into internal control codes
        /// </summary>
        /// <param name="keycode">keycode to get the input code for</param>
        /// <returns>inpt control code</returns>
        public static ControlCode GetControlCode(int keycode)
        {
            for (int i = 0; i < pairs.Length; i++)
                if (keycode == pairs[i])
                    return (ControlCode)i;
            return ControlCode.None;
        }
        /// <summary>
        /// sets an accosiation between a keycode and a contol code
        /// </summary>
        /// <param name="keycode">Windows keycode</param>
        /// <param name="controlCode">Nebula Game Engine control code</param>
        public static void SetControl(int keycode, ControlCode controlCode)
        {
            pairs[(int)controlCode] = keycode;
        }
    }

    /// <summary>
    /// Represents the mouse on screen
    /// </summary>
    public class Cursor
    {
        /// <summary>
        /// Set the image to be used as the cursor
        /// </summary>
        /// <param name="path">Path to image file</param>
        public void setIcon(string path)
        {
            cursorIcon = Bitmap.fromFile(path);
            visible = true;
        }

        /// <summary>
        /// Get global coordinates of cursor
        /// </summary>
        /// <returns>Global coordinates</returns>
        public Point toGlobal()
        {
            return Engine.camera.pos + new Point(location.x, Engine.yres - location.y)-new Point(Engine.xres/2,Engine.yres/2);
        }

        /// <summary>
        /// image to be used as the cursor
        /// </summary>
        public Bitmap cursorIcon;

        /// <summary>
        /// current local coordinates of cursor on window
        /// </summary>
        public Point location = new Point(0, 0);

        /// <summary>
        /// If true, draw the cursor on screen
        /// </summary>
        public bool visible = false;

        /// <summary>
        /// True if the left click is pressed this tick
        /// </summary>
        public bool lClick;

        /// <summary>
        /// True if the right click is pressed this tick
        /// </summary>
        public bool rClick;
    }

#pragma warning disable 1591
    public enum ControlCode
    {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 3,
        Right = 4,
        Primary = 5,
        Primary2 = 6,
        Secondary = 7,
        Secondary2 = 8,
        Start = 9,
        Select = 10,
        L1 = 11,
        L2 = 12,
        R1 = 13,
        R2 = 14
    }
}