using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace GameNET
{
    /// <summary>
    /// The whole thing
    /// </summary>
    public static class Engine
    {
        /// <summary>
        /// List of all objects being run and rendered by the engine
        /// </summary>
        static public List<GameObject> objects = new List<GameObject>();

        /// <summary>
        /// List of all expansions currently attached to objects
        /// </summary>
        static public List<IExpansion> expansions = new List<IExpansion>();

        /// <summary>
        /// Current fps target
        /// </summary>
        public static double targetfps = 60;
        private static bool resize;

        /// <summary>
        /// width of the screen, in pixels
        /// </summary>
        public static int xres = 500;

        /// <summary>
        /// Height of the screen, in pixels
        /// </summary>
        public static int yres = 500;

        /// <summary>
        /// represents the position of the camera, which can move
        /// </summary>
        public static Camera camera = new Camera();
        static Bitmap bgPtr;
        static int bgW;
        static int bgH;

        static MainWindow mainwindow;

        //performance
        static Thread gameLoop;
        static Thread renderLoop;
        static Thread consoleLoop;
        static bool render = true;
        static bool doLogic = true;

        /// <summary>
        /// If set to true, stops the gameloop and render loop and quits the game
        /// </summary>
        public static bool abort = false;

        /// <summary>
        /// If set to true, pauses the game
        /// </summary>
        public static bool paused = false;

        /// <summary>
        /// for debugging, draws a box around gameobjects
        /// </summary>
        public static bool showBounds = false;

        /// <summary>
        /// current actual fps of game
        /// </summary>
        public static double fps = 0;

        /// <summary>
        /// current actual fps of game
        /// </summary>
        public static double tps = 0;

        /// <summary>
        /// Current time, in game ticks, of game
        /// </summary>
        public static double tick = 0;
        static long lastlogic = 0;
        static long lastrender = 0;
        static Queue<float> tickavg = new Queue<float>(60);
        static Queue<float> favg = new Queue<float>(60);
        static Stopwatch timer = new Stopwatch();

        /// <summary>
        /// Queue of actions to be taken on the GameLoop thread ath the start of the next tick
        /// </summary>
        public static Queue<Action> onCreateQueue = new Queue<Action>();

        /// <summary>
        /// Contains a list of all input buttons/etc
        /// </summary>
        static public List<ControlCode> keys = new List<ControlCode>();

        /// <summary>
        /// Cursor for the mouse. Can be set to have a custom cursor over the game window
        /// </summary>
        static public Cursor mouse = new Cursor();

        /// <summary>
        /// Create game window and start the engine 
        /// </summary>
        static public void Start()
        {
            gameLoop = new Thread(new ThreadStart(GameLoop))
            {
                Name = "GameLoop",
                Priority = ThreadPriority.Highest
            };
            gameLoop.Start();

            timer.Start();
        }

        /// <summary>
        /// Initialize the game engine. Game will not start until Engine.Start() is called
        /// </summary>
        /// <param name="fps">Target FPS. Cannot be higher than the monitor's refresh rate</param>
        /// <param name="xres">X resolution of game window</param>
        /// <param name="yres">Y resolution of game window</param>
        public static void Init(double fps, int xres, int yres)
        {
            targetfps = fps;
            Engine.xres = xres;
            Engine.yres = yres;
            for (int i = 0; i < 60; i++)
            {
                tickavg.Enqueue(60);
                favg.Enqueue(60);
            }

            mainwindow = new MainWindow(xres, yres);
            renderLoop = new Thread(new ThreadStart(Initgraphics))
            {
                Name = "RenderLoop",
                Priority = ThreadPriority.Highest,
                IsBackground = true
            };
            renderLoop.Start();

            consoleLoop = new Thread(new ThreadStart(GameConsole.ReadFromConsole))
            {
                Name = "ConsoleLoop",
                IsBackground = true
            };
            consoleLoop.Start();

            while (!RenderTarget.started)
                Thread.Sleep(10);
        }

        /// <summary>
        /// Pause the game
        /// </summary>
        public static void Pause()
        {
            paused = true;
        }

        /// <summary>
        /// Unpause the game
        /// </summary>
        public static void Unpause()
        {
            paused = false;
        }

        /// <summary>
        /// Stop the engine and quit the application
        /// </summary>
        public static void Stop()
        {
            abort = true;
            doLogic = true;
            render = true;
        }

        /// <summary>
        /// Pause the game and show a message
        /// </summary>
        /// <param name="message"></param>
        public static void ShowMessage(string message)
        {
            Pause();
            MessageBox.Show(message);
            Unpause();
        }

        /// <summary>
        /// Set the background image of the gamse
        /// </summary>
        /// <param name="bitmap">Bitmap of background</param>
        /// <param name="width">Width of bitmap, used for tiling</param>
        /// <param name="height">Height of bitmap, used for tiling</param>
        public static void SetBg(Bitmap bitmap, int width, int height)
        {
            bgPtr = bitmap;
            bgW = width;
            bgH = height;
        }

        /// <summary>
        /// Executes logic for all gameobjects ant their expansion
        /// </summary>
        static private void GameLoop()
        {
            //gameloop init
            while (!abort)
            {
                SpinWait.SpinUntil(() => doLogic);
                doLogic = false;
                expansions.Clear();
                long start = timer.ElapsedMilliseconds;

                lock (onCreateQueue)
                    while (onCreateQueue.Count > 0)
                    {
                        var action = onCreateQueue.Dequeue();
                        if (action != null)
                            action();
                    }

                //object logic
                var actives = 0;
                for (int i = 0; i < objects.Count(); i++)
                {
                    var objecti = objects[i];
                    if (objecti != null&&objecti.active)
                    {
                        objecti.Step();
                        expansions.AddRange(objecti.GetAllExpansions());
                        actives++;
                    }
                }

                //run all expansions prestep, based on priority
                for (int h = 4; h >= 0; h--)
                    for (int i = 0; i < expansions.Count; i++)
                        if (expansions[i] != null && expansions[i].Priority == h)
                            expansions[i].PreStep();

                //run all expansions, based on priority
                for (int h = 4; h >= 0; h--)
                    for (int i = 0; i < expansions.Count; i++)
                        if (expansions[i] != null && expansions[i].Priority == h)
                            try
                            {
                                expansions[i].Step();
                            }
                            catch (NullReferenceException) { }
                
                render = true;
                GameConsole.Read();

                //time control
                tick++;
                tickavg.Dequeue();
                var delta = timer.ElapsedMilliseconds - lastlogic;
                if (delta > 0)
                    tickavg.Enqueue(1000 / (delta));
                else
                    tickavg.Enqueue((float)targetfps);

                try
                {
                    tps = tickavg.Average();
                    fps = favg.Average();
                }
                catch { }
                Console.Title = "FPS: " + fps.ToString("#.0") + " | TPS: " + tps.ToString("#.0") + " | Tick: " + tick;
                lastlogic = timer.ElapsedMilliseconds;

                if (tps > targetfps)
                    drift--;
                else
                    drift++;

                var sleeps = (1000 / targetfps) - ((timer.ElapsedMilliseconds - start));
                if (sleeps > 0)
                    Wait(sleeps);
            }
        }

        /// <summary>
        /// Executes drawing instructions for all objects, background, and any expansions
        /// </summary>
        static private void Render()
        {
            long start = timer.ElapsedMilliseconds;
            while (paused)
            {
                Thread.Sleep(100);
                return;
            }
            SpinWait.SpinUntil(() => render);
            render = false;
            camera.CalculateCoords();
            if (resize)
            {
                D3DInterop.Init(mainwindow.Handle, xres, yres, (int)targetfps);
                resize = false;
                return;
            }
            RenderTarget.BeginDraw();

            //draw background
            RenderTarget.Clear();
            if (bgPtr != null && bgPtr != null)
            {
                Point bottomleft = camera.pos - new Point(xres / 2, yres / 2);
                bottomleft = new Point((int)(bottomleft.x / bgW) * bgW, (int)(bottomleft.y / bgH) * bgH);
                for (int j = (int)bottomleft.x - 2 * (int)bgW; j < bottomleft.x + xres + 2 * (int)bgW; j += (int)bgW)
                    for (int k = (int)bottomleft.y - 2 * (int)bgH; k < bottomleft.y + yres + 2 * (int)bgH; k += (int)bgH)
                    {
                        var pos = new Point(j, k);
                        var rectangle = camera.Global2LocalCoords(pos);//.Global2LocalRectangle(pos, (int)bg.Size.Width, (int)bg.Size.Height);
                        RenderTarget.DrawBitmap(bgPtr, (int)rectangle.x, (int)rectangle.y, 0);
                    }
            }

            //render sprites
            for (int j = 0; j < 5; j++)
            {
                lock (objects)
                    for (int i = 0; i < objects.Count(); i++)
                    {
                        var objecti = objects[i];
                        if (objecti == null||!objecti.active)
                            continue;
                        var rectangle = camera.Global2LocalCoords(objecti.pos);
                        bool offscreen = (rectangle.x < -xres || rectangle.x > xres * 2 || rectangle.y < -yres || rectangle.y > yres * 2);

                        if (!offscreen && objecti.layer == j && objecti != null && objecti.sprite != null)
                        {
                            var sprite = objecti.sprite;

                            var rotation = objecti.θ;
                            RenderTarget.DrawBitmap(sprite, (int)rectangle.x, (int)rectangle.y, (int)objecti.θ);
                            var expansions = objecti.GetAllExpansions();

                            //run all expansions render, based on priority
                            for (int h = 4; h >= 0; h--)
                                for (int k = 0; k < expansions.Count; k++)
                                    if (expansions[k] != null && expansions[k].Priority == h)
                                        expansions[k].onRender();
                        }
                    }
            }

            //render cursor
            if (mouse.cursorIcon != null)
                RenderTarget.DrawBitmap(mouse.cursorIcon, (int)mouse.location.x, (int)mouse.location.y, 0);

            RenderTarget.EndDraw();
            RenderTarget.Present();
            doLogic = true;

            //time control
            favg.Dequeue();
            var delta = (timer.ElapsedMilliseconds - lastrender);
            if (delta > 0)
                favg.Enqueue(1000 / delta);
            else
                favg.Enqueue((float)targetfps);
            lastrender = timer.ElapsedMilliseconds;
            var sleeps = (1000 / targetfps) - ((timer.ElapsedMilliseconds - start));
        }

        /// <summary>
        /// Add a new Game object to the game engine
        /// </summary>
        /// <param name="gameObject">Object to add</param>
        /// <param name="x">x coordinate to initially place the object at</param>
        /// <param name="y">y coordinate to initially place the object at</param>
        static public void AddObject(GameObject gameObject, int x, int y)
        {
            gameObject.pos.x = x;
            gameObject.pos.y = y;
            gameObject.OnCreate();
            objects.Add(gameObject);
            Console.WriteLine("Added " + gameObject.GetType().Name + " at " + x + "," + y);
        }

        /// <summary>
        /// remove an object from the game engine
        /// </summary>
        /// <param name="gameObject">object to remove</param>
        static public void DestroyObject(GameObject gameObject)
        {lock (objects)
            objects.Remove(gameObject);
            gameObject.OnDestroy();
            gameObject.Destroy();

            Console.WriteLine("Deleted " + gameObject.GetType().Name);
        }

        static int drift = 0;

        /// <summary>
        /// wait for the specified amount of milliseconds
        /// </summary>
        /// <param name="ms"></param>
        public static void Wait(double ms)
        {
            var durationTicks = ms / (1000 + drift) * Stopwatch.Frequency;
            var sw = Stopwatch.StartNew();
            SpinWait.SpinUntil(() => (sw.ElapsedTicks >= durationTicks));
        }

        /// <summary>
        /// Initialize Direct3D and game window
        /// </summary>
        static void Initgraphics()
        {
            mainwindow = new MainWindow(xres, yres);
            RenderTarget.Init(mainwindow.Handle, xres, yres, (int)targetfps);
            mainwindow.Run(Render);
        }

        /// <summary>
        /// Resizes the current game window
        /// </summary>
        /// <param name="x">New x resolution</param>
        /// <param name="y">New y resolution</param>
        /// <param name="fps">New target fps</param>
        /// <param name="fullscreen">Set fullscreen property</param>
        public static void Resize(int x, int y, int fps, bool fullscreen)
        {
            D3DInterop.Resize(x, y, fps, fullscreen);
        }

        /// <summary>
        /// free as many managed resources as possible and deconstruct the renderer
        /// </summary>
        static void DisposeOfEverything()
        {
            for (int i = 0; i < objects.Count; i++)
            {
                DestroyObject(objects[i]);
            }
            mainwindow.Dispose();
            D3DInterop.DeInit();
        }
    }

    /// <summary>
    /// Object that specefies the coordinates to render from
    /// </summary>
    public class Camera
    {
        GameObject target;

        /// <summary>
        /// When calculating position changes, divide that change into x pieces
        /// </summary>
        public double smoothness = 1;

        /// <summary>
        /// Global coordinates for camera location
        /// </summary>
        public Point pos = new Point(0, 0);

        /// <summary>
        /// Object that specefies the coordinates to render from
        /// </summary>
        public Camera()
        {

        }

        /// <summary>
        /// Object that specefies the coordinates to render from
        /// </summary>
        /// <param name="target">GameObject for the camera to follow</param>
        /// <param name="smoothness">Specefies the smoothness of the camera. Set to 1 for no smothness</param>
        public Camera(GameObject target, double smoothness)
        {
            this.target = target;
            this.smoothness = smoothness;
        }

        /// <summary>
        /// Set location based on target location and smoothness
        /// </summary>
        public void CalculateCoords()
        {
            if (target != null)
            {
                pos.x += (target.pos.x - pos.x) / smoothness;
                pos.y += (target.pos.y - pos.y) / smoothness;
            }
        }

        /// <summary>
        /// Converts a global point to a local screen-space point, based on this camera's location and the specified coordinate
        /// </summary>
        /// <param name="global">Global point to find the local points for</param>
        /// <returns></returns>
        public Point Global2LocalCoords(Point global)
        {
            var xpos = (global.x - pos.x + Engine.xres / 2);
            var ypos = (Engine.yres - (global.y - pos.y + Engine.yres / 2)); //flip y because screen-space y coordinates are reversed
            return new Point(xpos, ypos);
        }
    }
}