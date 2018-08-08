﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsAPI;

namespace Gotchya
{
    class Function
    {
        public static void ShakeMouse(int milliseconds, int offset = 2, int finaloffset = -1) {
            if (finaloffset == -1) finaloffset = offset;
            float m = (finaloffset - offset) / (float)milliseconds;
            float c = finaloffset - m * milliseconds;

            Random r = new Random();
            int terminator = (int)(milliseconds / 10.0);
            for (int i = 0; i < terminator; i++) {
                offset = (int)Math.Floor(m * i + c);
                int currentX = Cursor.Position.X;
                int currentY = Cursor.Position.Y;
                int x = r.Next(currentX - offset, currentX + offset + 1);
                int y = r.Next(currentY - offset, currentY + offset + 1);
                Mouse.Move(x, y);
                System.Threading.Thread.Sleep(10);
            }

        }

        public static void WindowShaker(string title, int milliseconds, int offset = 2, int finaloffset = -1) {
            IntPtr hWnd = Window.Get(title);
            if (hWnd == null) return;

            if (finaloffset == -1) finaloffset = offset;
            float m = (finaloffset - offset) / (float)milliseconds;
            float c = finaloffset - m * milliseconds;

            int terminator = (int)(milliseconds / 10.0);
            Random r = new Random();
            for (int i = 0; i < terminator; i++) {
                offset = (int)Math.Floor(m * i + c);
                int currentX = Window.GetLocation(hWnd).X;
                int currentY = Window.GetLocation(hWnd).Y;
                int x = r.Next(currentX - offset, currentX + offset + 1);
                int y = r.Next(currentY - offset, currentY + offset + 1);
                Window.Move(hWnd, x, y);
                System.Threading.Thread.Sleep(10);
            }
        }

        public static List<string> GetWindowTitles() {
            Process[] processlist = Process.GetProcesses();
            List<string> titles = new List<string>();

            foreach (Process process in processlist) {
                if (!String.IsNullOrEmpty(process.MainWindowTitle)) {
                    titles.Add(process.MainWindowTitle);
                }
            }
            return titles;
        }

        public static void ScrambleWindowTitle(string title, int milliseconds, int length) {
            IntPtr hWnd = Window.Get(title);
            if (hWnd == null) return;

            String initalTitle = Window.GetTitle(hWnd);
            int terminator = (int)(milliseconds / 10.0);
            for (int i = 0; i < terminator; i++) {
                Random r = new Random();
                String s = RandomString(length, r);
                Window.SetTitle(hWnd, s);
                Thread.Sleep(10);
            }
            Window.SetTitle(hWnd, initalTitle);
        }

        private static string RandomString(int length, Random random) {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()~-[]';,./?><:{}\";";
            var stringChars = new char[length];

            for (int i = 0; i < stringChars.Length; i++) {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }
    }
}