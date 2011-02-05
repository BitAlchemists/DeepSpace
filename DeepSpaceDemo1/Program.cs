using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeepSpaceDemo1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to DeepSpaceDemo1");
            bool keepRunning = true;
            while (keepRunning)
            {
                // User pressed ESC?
                while (Console.KeyAvailable)
                {
                    ConsoleKeyInfo info = Console.ReadKey();
                    if (info.Key == ConsoleKey.Escape)
                        keepRunning = false;
                }
            }
            Console.WriteLine("That's it, i'm going home!");
            Console.ReadLine();
        }
    }
}
