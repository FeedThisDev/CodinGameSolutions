using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Survive the wrath of Kutulu
 * Coded fearlessly by JohnnyYuge & nmahoude (ok we might have been a bit scared by the old god...but don't say anything)
 **/

namespace CodeOfKutulu
{
    class Program
    {
        static void Main(string[] args)
        {
            Gameboard.GAMEINIT();

            // game loop
            while (true)
            {
                Gameboard.TURNINIT();

                PlayerLogic.DoTurn();

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                //Console.WriteLine("WAIT"); // MOVE <x> <y> | WAIT

            }
        }
    }



}