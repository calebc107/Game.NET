using System;
using GameNET;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;

namespace GameNET
{
    static class GameConsole
    {
        static string input = "";

        /// <summary>
        /// Loop to read from console as long as engine is running
        /// </summary>
        public static void ReadFromConsole()
        {
            while (!Engine.abort)
            {
                if (Console.KeyAvailable)
                    input = Console.ReadLine();
                else
                    Thread.Sleep(100);
            }
        }

        /// <summary>
        /// take action on any input, if there is any
        /// </summary>
        public static void Read()
        {
            if (!string.IsNullOrEmpty(input))
            {
                Parse(input.Split(' '));
                input = "";
            }

        }

        /// <summary>
        /// Execute command
        /// </summary>
        /// <param name="input"></param>
        public static void Parse(string[] input)
        {
            Console.WriteLine();
            GameConsole.input = "";
            try
            {
                switch (input[0])
                {
                    //delete {gameobject} [count] - delete an object
                    case "delete":
                        if (input.Length == 2)
                            if ('0' < input[1][0] && input[1][0] <= '9') //parse as int
                                DeleteObjects(int.Parse(input[1]));
                            else
                                DeleteObjects(input[1]);
                        break;

                    //fps {new target fps} - set new fps
                    case "fps":
                        SetFPS(int.Parse(input[1]));
                        break;
                    //new {gameobject name} {x} {y} {count}
                    case "new":
                        CreateObject(input[1], int.Parse(input[2]), int.Parse(input[3]), int.Parse(input[4]));
                        break;
                    // gc - Force a garbage Collection
                    case "gc":
                        ForceGC();
                        break;
                    //status - pring game engine and memory status
                    case "status":
                        PrintStatus();
                        break;

                    //light - enable or disable lighting system
                    case "light":
                        Light.active = !Light.active;
                        Console.WriteLine("Lighting " + (Light.active ? "on" : "off"));
                        break;
                    //showbounds - show boxes around game objects
                    case "showbounds":
                        Engine.showBounds = !Engine.showBounds;
                        Console.WriteLine("Borders " + (Engine.showBounds ? "on" : "off"));
                        break;
                    case "exit":
                    case "quit":
                        Engine.Stop();
                        break;

                    default:
                        Console.WriteLine("Unrecognized Command");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void DeleteObjects(int index)
        {
            Engine.DestroyObject(Engine.objects[index]);
        }

        public static void DeleteObjects(string objectType)
        {
            Engine.onCreateQueue.Enqueue(delegate
            {
                lock (Engine.objects)
                    for (int i = 0; i < Engine.objects.Count; i++)
                    {
                        if (Engine.objects[i].GetType().Name.ToLower() == objectType.ToLower())
                            Engine.DestroyObject(Engine.objects[i]);
                    }
            });
        }

        public static void SetFPS(int newfps)
        {
            Engine.targetfps = newfps;
        }

        public static void CreateObject(string stype, int x, int y, int quantity)
        {
            var assembly = Assembly.GetEntryAssembly();
            Type type = null;
            foreach (var type2 in assembly.GetTypes())
            {
                if (type2.Name.ToLower() == stype.ToLower())
                    type = type2;
            }
            for (int i = 0; i < quantity; i++)
            {
                object newob = type.GetConstructors()[0].Invoke(new object[] { });
                Engine.AddObject((GameObject)newob, x, y);
            }
        }

        public static void ForceGC()
        {
            long original = GC.GetTotalMemory(false);
            GC.Collect(int.MaxValue, GCCollectionMode.Forced, true, true);
            long now = GC.GetTotalMemory(false);
            var freed = ((original - now) / 1000000.0).ToString("0.##");
            Console.WriteLine("Collected " + freed + "MB of garbage");
        }

        public static void PrintStatus()
        {
            var actives = 0;
            foreach (var item in Engine.objects)
                if (item.active)
                    actives++;
            string status = "STATUS:"
                + "\n" + actives + "/" + Engine.objects.Count + " Objects, " + Engine.expansions.Count + " Expansions"
                + "\nMEMORY: " + (GC.GetTotalMemory(false) / 1000000).ToString("0.##") + "MB USED"
                + "\nFPS: " + Engine.fps + " TPS: " + Engine.tps;
            Console.WriteLine(status);
        }
    }
}
