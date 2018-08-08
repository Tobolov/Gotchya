﻿using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsAPI;

namespace Gotchya
{
    class Program
    {
        static Keys DETONATOR_OFF = Keys.T;
        static int DETONATOR_DELAY = 5;
        static int MOUSE_SHAKE_DELAY = 0;
        static int MOUSE_SHAKE_TIME = 4;
        static int WINDOW_SHAKE_DELAY = 2;
        static int WINDOW_SHAKE_TIME = 4;
        static int WINDOW_NAME_DELAY = 2;
        static int WINDOW_NAME_TIME = 1;
        static int WINDOW_NAME_LENGTH = 70;
        static int SCREEN_BLEED_DELAY = 20;
        static int SCREEN_BLEED_FRAMES = 150;

        private static IKeyboardMouseEvents GlobalHook;

        static void Main(string[] args) {
            Thread.Sleep(DETONATOR_DELAY * 1000);
            ApplicationContext msgLoop = new ApplicationContext();
            GlobalHook = Hook.GlobalEvents();
            GlobalHook.MouseMove += GlobalHook_MouseMove;
            GlobalHook.KeyUp += GlobalHook_KeyUp;
            Application.Run(msgLoop);
            Console.WriteLine("DETONATOR PRIMED");
            while (true) {
                Console.ReadKey();
                Console.WriteLine("KEY READ");
            }
        }

        private static void SpeedTest() {
            Stopwatch s = new Stopwatch();
            s.Start();
            ScreenBleedHoleOptimised.Run(100);
            s.Stop();
            Console.WriteLine("ScreenBleedHoleOptimised: " + s.ElapsedMilliseconds);
            s.Reset();
            s.Start();
            ScreenBleedHoleOld.Run(100);
            s.Stop();
            Console.WriteLine("ScreenBleedHoleOld: " + s.ElapsedMilliseconds);
            s.Reset();
            s.Start();
            ScreenBleedHoleStep.Run(100);
            s.Stop();
            Console.WriteLine("ScreenBleedHoleStep: " + s.ElapsedMilliseconds);
            Console.Read();
        }

        private static void GlobalHook_KeyUp(object sender, KeyEventArgs e) {
            Console.WriteLine("DETONATOR DETECTED KEY " + e.KeyCode.ToString());
            if (e.KeyCode == DETONATOR_OFF) {
                Quit();
            } else {
                MainSequence();
            }
        }

        private static void GlobalHook_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e) {
            Console.WriteLine("DETONATOR DETECTED MOUSE MOVEMENT");
            GlobalHook.MouseMove -= GlobalHook_MouseMove;
            MainSequence();
        }

        static void MainSequence() {
            Console.WriteLine("MAIN SEQUENECE STARTED");
            List<string> windowTitles = Function.GetWindowTitles();

            //Mouse Shake
            Task mouseShakeTask = Task.Run(() => {
                Thread.Sleep(MOUSE_SHAKE_DELAY * 1000);
                Function.ShakeMouse(MOUSE_SHAKE_TIME * 1000, 1, 35);
            });

            //Window Shake
            List<Task> windowShakeTasks = new List<Task>();
            foreach (String s in windowTitles) {
                Task windowShakeInstance = Task.Run(() => {
                    Thread.Sleep(WINDOW_SHAKE_DELAY * 1000);
                    Function.WindowShaker(s, WINDOW_SHAKE_TIME * 1000, 1, 35);
                });
                windowShakeTasks.Add(windowShakeInstance);
            }

            //Window Titles
            List<Task> windowNameTasks = new List<Task>();
            foreach (String s in windowTitles) {
                Task windowNameInstance = Task.Run(() => {
                    Thread.Sleep(WINDOW_NAME_DELAY * 1000);
                    Function.ScrambleWindowTitle(s, WINDOW_NAME_TIME * 1000, WINDOW_NAME_LENGTH);
                });
                windowNameTasks.Add(windowNameInstance);
            }

            //Screen Bleed
            Task screenBleedTask = Task.Run(() => {
                Thread.Sleep(SCREEN_BLEED_DELAY * 1000);
                ScreenBleedHoleOptimised.Run(SCREEN_BLEED_FRAMES);
            });

            mouseShakeTask.Wait();
            Task.WaitAll(windowShakeTasks.ToArray());
            Task.WaitAll(windowNameTasks.ToArray());
            screenBleedTask.Wait();

            Quit();
        }

        private static void Quit() {
            Desktop.ShowTaskBar();
            GlobalHook.KeyUp -= GlobalHook_KeyUp;
            GlobalHook.MouseMove -= GlobalHook_MouseMove;
            GlobalHook.Dispose();
            Environment.Exit(0);
        }
    }
}