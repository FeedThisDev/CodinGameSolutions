using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    const int HEIGHT = 18;
    const int WIDTH = 40;
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int TX = int.Parse(inputs[0]);
        int TY = int.Parse(inputs[1]);
        // game loop
        while (true)
        {
            bool striked = false;
            List<Point> mobs = new List<Point>();
            inputs = Console.ReadLine().Split(' ');
            int H = int.Parse(inputs[0]); // the remaining number of hammer strikes.
            int N = int.Parse(inputs[1]); // the number of giants which are still present on the map.
            for (int i = 0; i < N; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int X = int.Parse(inputs[0]);
                int Y = int.Parse(inputs[1]);
                mobs.Add(new Point(X, Y));
            }

            if (AllEnenmiesWithinRange(TX, TY, mobs))
            {
                Console.Error.WriteLine("KILL THEM ALL!");
                Console.WriteLine("STRIKE");
                continue;
            }

            int dx = (mobs.Max(mob => mob.X) - mobs.Min(mob => mob.X)) / 2;
            int dy = (mobs.Max(mob => mob.Y) - mobs.Min(mob => mob.Y)) / 2;
            int centerx = mobs.Min(mob => mob.X) + dx;
            int centery = mobs.Min(mob => mob.Y) + dy;

            if (InDanger(TX, TY, mobs))
            {
                Console.Error.WriteLine("DANGER! Can I flee?");
                if (Flee(ref TX, ref TY, centerx, centery, mobs))
                    continue;

                Console.Error.WriteLine("LAST RESORT!");
                Console.WriteLine("STRIKE");
                continue;
            }
            else
            {
                Console.Error.WriteLine($"MOVE TO CENTER {centerx} {centery}");
                if (DoMove(ref TX, ref TY, centerx, centery))
                    continue;
            }

            // The movement or action to be carried out: WAIT STRIKE N NE E SE S SW W or N
            Console.Error.WriteLine($"Nothing to do");
            Console.WriteLine("WAIT");
        }
    }

    private static bool Flee(ref int tX, ref int tY, int centerx, int centery, List<Point> mobs)
    {
        List<Point> escapePoints = new List<Point>();
        for (int x = tX - 1; x <= tX + 1; x++)
        {
            for (int y = tY - 1; y <= tY + 1; y++)
            {
                if (x == tX && y == tY)
                    continue;
                if (x < 0 || x >= WIDTH || y < 0 || y >= HEIGHT)
                    continue;

                if (!InDanger(x, y, mobs))
                {
                    escapePoints.Add(new Point(x, y));
                }
            }
        }

        Console.Error.WriteLine($"Center: {centerx} {((mobs.Max(mob => mob.Y) - mobs.Min(mob => mob.Y)) / 2) + mobs.Min(mob => mob.Y)}");


        foreach (var point in escapePoints)
        {
            Console.Error.WriteLine($"Found Escape Point: {point.X} {point.Y}");
        }

        var escapePoint = escapePoints.OrderBy(point => GetDistance(centerx, centery, point.X, point.Y)).FirstOrDefault();
        if(escapePoint == null)
            return false;

        DoMove(ref tX, ref tY, escapePoint.X, escapePoint.Y);
        return true;
    }

    private static double GetDistance(int x1, int y1, int x2, int y2)
    {
        return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
    }

    private static bool InDanger(int tX, int tY, List<Point> mobs)
    {
        int minX = tX - 1;
        int minY = tY - 1;
        int maxX = tX + 1;
        int maxY = tY + 1;

        foreach (var mob in mobs)
        {
            if (mob.X >= minX && mob.X <= maxX && mob.Y >= minY && mob.Y <= maxY)
                return true;
        }
        return false;
    }

    private static bool AllEnenmiesWithinRange(int tX, int tY, List<Point> mobs)
    {
        int minX = tX - 4;
        int minY = tY - 4;
        int maxX = tX + 4;
        int maxY = tY + 4;

        foreach (var mob in mobs)
        {
            if (mob.X < minX || mob.X > maxX)
                return false;
            if (mob.Y < minY || mob.Y > maxY)
                return false;
        }
        return true;
    }

    private static bool DoMove(ref int tX, ref int tY, int targetX, int targetY)
    {
        if (tX == targetX && tY == targetY)
            return false;

        string command = string.Empty;
        // Write an action using Console.WriteLine()
        // To debug: Console.Error.WriteLine("Debug messages...");
        if (targetY < tY)
        {
            command += "N";
            tY--;
        }
        if (targetY > tY)
        {
            command += "S";
            tY++;
        }

        if (targetX > tX)
        {
            command += "E";
            tX++;
        }
        if (targetX < tX)
        {
            tX--;
            command += "W";
        }
        Console.WriteLine(command);
        return true;
    }


    public static Point GetCentroid(List<Point> poly)
    {
        double accumulatedArea = 0.0f;
        double centerX = 0.0f;
        double centerY = 0.0f;

        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
        {
            double temp = poly[i].X * poly[j].Y - poly[j].X * poly[i].Y;
            accumulatedArea += temp;
            centerX += (poly[i].X + poly[j].X) * temp;
            centerY += (poly[i].Y + poly[j].Y) * temp;
        }

        if (Math.Abs(accumulatedArea) < 1E-7f)
            return null;  // Avoid division by zero

        accumulatedArea *= 3f;
        return new Point((int)(centerX / accumulatedArea), (int)(centerY / accumulatedArea));
    }
}



public class Point
{
    public int X { get; set; }
    public int Y { get; set; }
    public Point() { }
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }


}