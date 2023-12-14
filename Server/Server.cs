// @File: Server.cs
// @Created: 2021/04/18
// @Last Modified: 2021/04/19
// @Author: Keming Chen, Yifei Sun

using System;
using ServerController;

namespace Server
{
    /// <summary>
    ///     This class is to hold the ServerController in the terminal.
    /// </summary>
    internal class Server
    {
        // Server Controller.
        private static Controller controller;

        /// <summary>
        ///     Main.
        /// </summary>
        /// <param name="args">Arguments</param>
        private static void Main(string[] args)
        {
            controller = new Controller(Console.WriteLine);
            Console.ReadLine();
        }
    }
}