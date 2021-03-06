﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections;

namespace WindowConfiguration
{
    public struct WindowInfo
    {
        public string Process_Name;
        public string Process_Title;
        public int Process_ID;
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;
        public int Width;
        public int Height;
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        
        /// Since the standard C# library omits these window handler functions need to use import statements
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);



        //Struct passed into the window handler section which gives us the window position and size (after calculation)
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [STAThread]
        static void Main()
        {
            // Gets information about all the monitors the current user has
            Console.WriteLine("\n Monitor Information \n");
            foreach (var screen in Screen.AllScreens)
            {
                Console.WriteLine("Device Name: " + screen.DeviceName);
                Console.WriteLine("Bounds: " + screen.Bounds.ToString());
                Console.WriteLine("Type: " + screen.GetType().ToString());
                Console.WriteLine("Working Area: " + screen.WorkingArea.ToString());
                Console.WriteLine("Primary Screen: " + screen.Primary.ToString());
            }
            Console.WriteLine("\n");


            // Some helper variables to get the process information and setting up rect/window handler
            Process[] processes = Process.GetProcesses();
            var count = 0;
            RECT rect = new RECT();
            IntPtr hWnd;

            // Here is how I am querying the database to get all the tuples. NOTE: will be empty initially until something is inserted
            List<WindowInfo> window_lst = new List<WindowInfo>();
            window_lst = SqLiteDataAccess.LoadWindow();
            Console.WriteLine("\nFROM DATABASE\n");
            foreach(var window in window_lst)
            {
                Console.WriteLine("DB Process ID: " + window.Process_ID);
                Console.WriteLine("DB Process Name: " + window.Process_Name);
                Console.WriteLine("DB Process Title: " + window.Process_Title);
                Console.WriteLine("DB Left: " + window.Left);
                Console.WriteLine("DB Right: " + window.Right);
                Console.WriteLine("DB Top: " + window.Top);
                Console.WriteLine("DB Bottom: " + window.Bottom);
                Console.WriteLine("DB Width: " + window.Width);
                Console.WriteLine("DB Height: " + window.Height + "\n");
            }


            // Loop through all the currently running processes and get some diagnostic information for each window. You can also insert into the DB and move the window (uncomment last few statements)
            foreach (var proc in processes)
            {
                if (!string.IsNullOrEmpty(proc.MainWindowTitle))
                {
                    // Print out some diagnostic information
                    WindowInfo window = new WindowInfo();
                    Console.WriteLine(++count + ".");
                    Console.WriteLine("Process Name: " + proc.ProcessName);
                    Console.WriteLine("Process Title: " + proc.MainWindowTitle);
                    Console.WriteLine("Process ID: " + proc.Id);
                    hWnd = FindWindow(null, proc.MainWindowTitle);
                    GetWindowRect(hWnd, out rect);
                    Console.WriteLine("Window Location - Top left (x,y): " + rect.Left + "," + rect.Top);
                    Console.WriteLine("Window Location - Bottom Right (x, y): " + rect.Right + "," + rect.Bottom);
                    Console.WriteLine("Window Width: " + Math.Abs((rect.Left) - (rect.Right)));
                    Console.WriteLine("Window Height: " + Math.Abs((rect.Top) - (rect.Bottom)));
                    Console.WriteLine("\n");

                    // Fill window struct in order to easily send an object to the sql insert function
                    window.Process_Name = proc.ProcessName.ToString();
                    window.Left = rect.Left;
                    window.Right = rect.Right;
                    window.Top = rect.Top;
                    window.Bottom = rect.Bottom;
                    window.Width = Math.Abs((rect.Left) - (rect.Right));
                    window.Height = Math.Abs((rect.Top) - (rect.Bottom));
                    window.Process_Title = proc.MainWindowTitle;
                    window.Process_ID = proc.Id;

                    // Uncomment the line below to insert all window handlers opened into the DB
                    //SqLiteDataAccess.SaveWindow(window);


                    // Uncomment the line below to move window handlers. NOTE: this only works for windows that are not minimized as far as I can tell.
                    // SetWindowPos(hWnd, 0, 0, 0, 500, 500, 0);
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
