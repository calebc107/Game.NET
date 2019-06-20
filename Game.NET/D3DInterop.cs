using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameNET
{
    static class D3DInterop
    {
        [DllImport("D3DInterop")]
        extern public static void Init(IntPtr handle, int x, int y, int fps);

        [DllImport("D3DInterop")]
        extern public static void DeInit();

        [DllImport("D3DInterop")]
        extern public static void BeginDraw();

        [DllImport("D3DInterop")]
        extern public static void EndDraw(bool lighting);

        [DllImport("D3DInterop")]
        extern public static void DrawBitmap(IntPtr intPtr, int x, int y, int rot);

        [DllImport("D3DInterop")]
        extern public static void DrawLight(IntPtr intPtr, int x, int y);

        [DllImport("D3DInterop")]
        extern public static void Clear();

        [DllImport("D3DInterop")]
        extern public static void Present();

        [DllImport("D3DInterop")]
        extern public static IntPtr LoadBitmapFile(StringBuilder filepath);

        [DllImport("D3DInterop")]
        extern public static void DisposeBitmap(IntPtr bitmapPtr);

        [DllImport("D3DInterop")]
        extern public static IntPtr CreateBrush(float r, float g, float b);

        [DllImport("D3DInterop")]
        extern public static void RtDrawText(StringBuilder sstring, int length, StringBuilder fontname, float size, int x, int y, IntPtr brush);

        [DllImport("D3DInterop")]
        extern public static IntPtr LoadWAVFile(StringBuilder path, float repeatAt, ref IntPtr bufferloc, ref IntPtr xaudiobufloc);

        [DllImport("D3DInterop")]
        extern public static void PlaySourceVoice(IntPtr voidSourceVoice, IntPtr buffer);

        [DllImport("D3DInterop")]
        extern public static void DisposeSourceVoice(IntPtr ptr, IntPtr bufferloc, IntPtr xaudiobufloc);

        [DllImport("D3DInterop")]
        extern public static void StopSourceVoice(IntPtr voidSourceVoice);

        [DllImport("D3DInterop")]
        extern public static void Resize(int w, int h, int fps, bool fullscreen);

        [DllImport("D3DInterop")]
        extern public static IntPtr BitmapFromGDI(int width, int height, byte[] stream, int stride);
    }

    /// <summary>
    /// The renderer that draws to the screen
    /// </summary>
    public static class RenderTarget
    {
        /// <summary>
        /// indicates if the renderer has been initialized
        /// </summary>
        public static bool started = false;

        /// <summary>
        /// Initializes the renderer
        /// </summary>
        /// <param name="handle">Window handle that the renderer will draw to</param>
        /// <param name="x">X resolution</param>
        /// <param name="y">Y resolution</param>
        /// <param name="fps">Target fps</param>
        public static void Init(IntPtr handle, int x, int y, int fps)
        {
            D3DInterop.Init(handle, x, y, fps);
            started = true;
        }

        /// <summary>
        /// Prepares the renderer for drawing instructions
        /// </summary>
        public static void BeginDraw()
        {
            D3DInterop.BeginDraw();
        }

        /// <summary>
        /// Draw the specified bitmap on screen at the local coordinates
        /// </summary>
        /// <param name="bitmap">Bitmap to draw</param>
        /// <param name="x">Local screen-space x-coordinate to draw at</param>
        /// <param name="y">Local screen-space y-coordinate to draw at</param>
        /// <param name="rot">Rotation of bitmap</param>
        public static void DrawBitmap(Bitmap bitmap, int x, int y, int rot)
        {
            D3DInterop.DrawBitmap(bitmap.location, x, y, rot);
        }

        /// <summary>
        /// Prepares renderer for presenting the drawn image
        /// </summary>
        public static void EndDraw()
        {
            D3DInterop.EndDraw(Light.active);
        }

        /// <summary>
        /// Reset frame
        /// </summary>
        public static void Clear()
        {
            D3DInterop.Clear();
        }

        /// <summary>
        /// Shows the drawn frame (Call after EndDraw())
        /// </summary>
        public static void Present()
        {
            D3DInterop.Present();
        }

        /// <summary>
        /// Draw a string of text onscreen
        /// </summary>
        /// <param name="text">The text to draw</param>
        /// <param name="fontname">Name of font to use</param>
        /// <param name="size">Pt size of font</param>
        /// <param name="x">Local screen-space x-coordinate to draw at</param>
        /// <param name="y">Local screen-space y-coordinate to draw at</param>
        /// <param name="brush"></param>
        public static void DrawText(string text, string fontname, int size, int x, int y, Brush brush)
        {
            StringBuilder sb = new StringBuilder(text);
            StringBuilder fn = new StringBuilder(fontname);
            D3DInterop.RtDrawText(sb, text.Length, fn, size, x, y, brush.location);
        }

    }

    /// <summary>
    /// Bitmap class that works with the renderer better that the ones in System.Drawing
    /// </summary>
    public class Bitmap : IDisposable
    {
        /// <summary>
        /// A pointer to this bitmap's data in memory.
        /// </summary>
        public IntPtr location;

        bool disposed = false;

        /// <summary>
        /// Loads a bitmap from a file. PNG file format is the only tested type of image
        /// </summary>
        /// <param name="filepath">Path to image file</param>
        /// <returns>New Bitmap object</returns>
        public static Bitmap fromFile(string filepath)
        {
            var str = new StringBuilder(filepath);
            var intptr = D3DInterop.LoadBitmapFile(str);
            var bitmap = new Bitmap();
            bitmap.location = intptr;
            str.Clear();
            return bitmap;
        }

        /// <summary>
        /// Converts from a System.Drawing.Bitmap
        /// </summary>
        /// <param name="bitmap">System.Drawing.Bitmap object to convert from</param>
        /// <returns>New Bitmap object</returns>
        public static Bitmap FromGDI(System.Drawing.Bitmap bitmap)
        {
            //from https://github.com/sharpdx/SharpDX-Samples/blob/master/Desktop/Direct2D1/BitmapApp/Program.cs
            {
                var sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var size = new Size(bitmap.Width, bitmap.Height);

                // Transform pixels from BGRA to RGBA
                int stride = bitmap.Width * sizeof(int);
                using (var tempStream = new MemoryStream(bitmap.Height * stride))
                {
                    // Lock System.Drawing.Bitmap
                    var bitmapData = bitmap.LockBits(sourceArea, ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);

                    // Convert all pixels 
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        int offset = bitmapData.Stride * y;
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
                            byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
                            byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
                            byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);
                            int rgba = R | (G << 8) | (B << 16) | (A << 24);
                            byte[] bytes = BitConverter.GetBytes(rgba);
                            for (int i = 0; i < bytes.Length; i++)
                            {
                                tempStream.WriteByte(bytes[i]);
                            }
                        }
                    }
                    bitmap.UnlockBits(bitmapData);
                    tempStream.Position = 0;

                    Bitmap result = new Bitmap();
                    result.location = D3DInterop.BitmapFromGDI(size.Width, size.Height, tempStream.GetBuffer(), stride);
                    return result;
                }
            }
        }

        /// <summary>
        /// Deletes releases unmanaged memory for bitmap
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                D3DInterop.DisposeBitmap(location);
                disposed = true;
            }
        }
    }

    /// <summary>
    /// Object that represents a Direct3D brush
    /// </summary>
    public class Brush : IDisposable
    {
        /// <summary>
        /// Memory location of brush object
        /// </summary>
        public IntPtr location;

        /// <summary>
        /// creates a solid brush with the specified color
        /// </summary>
        /// <param name="r">Red</param>
        /// <param name="g">Green</param>
        /// <param name="b">Blue</param>
        public Brush(float r, float g, float b)
        {
            location = D3DInterop.CreateBrush(r, g, b);
        }

        /// <summary>
        /// releases unmanaged memory for brush
        /// </summary>
        public void Dispose()
        {
            D3DInterop.DisposeBitmap(location);
        }
    }

    /// <summary>
    /// Xaudio SourceVoice
    /// </summary>
    public class SourceVoice : IDisposable
    {
        /// <summary>
        /// location of sourcevoice
        /// </summary>
        IntPtr location;

        /// <summary>
        /// location of rawbufferdata to load audio from
        /// </summary>
        public IntPtr bufferloc;

        /// <summary>
        /// location of Xaudio buffer location
        /// </summary>
        IntPtr xaudiobufloc;

        /// <summary>
        /// Creates a new SourceVoice form the specified Wav File
        /// </summary>
        /// <param name="filename">path to WAV file</param>
        /// <param name="repeatAt">Seconds at which to loop the audio file</param>
        public SourceVoice(string filename, float repeatAt)
        {
            location = D3DInterop.LoadWAVFile(new StringBuilder(filename), repeatAt, ref bufferloc, ref xaudiobufloc);
        }

        /// <summary>
        /// releases unmanaged memory for SourceVoice
        /// </summary>
        public void Dispose()
        {
            D3DInterop.DisposeSourceVoice(location, bufferloc, xaudiobufloc);
        }

        /// <summary>
        /// play the sound, repeats at the previously specified time
        /// </summary>
        public void Play()
        {
            D3DInterop.PlaySourceVoice(location, xaudiobufloc);
        }


        /// <summary>
        /// stops the sound if if is currently playing
        /// </summary>
        public void Stop()
        {
            D3DInterop.StopSourceVoice(location);
        }

    }
}
