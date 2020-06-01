// {
using System;
using System.Collections.Generic;
using System.Linq;

namespace SearchAlgo
{
    public class SearchAlgoComparison
    {
        struct Cell
        {
            public int x, y, d;
            public char c;
            public bool visited;
        }

        class Maze
        {
            public Cell[][] grid;
            public int width, height;

            public bool isInGrid(int x, int y)
            {
                return x >= 0 && x < width && y >= 0 && y < height;
            }
        }

        static Maze mapToMaze(string[] map)
        {
            Cell[][] grid = new Cell[map.Length][];
            for (int x = 0; x < map.Length; ++x)
            {
                var s = map[x];
                grid[x] = new Cell[s.Length];
                for (int y = 0; y < s.Length; ++y)
                {
                    var c = s[y] == '#' ? '#' : '.';
                    grid[x][y] = new Cell() { x = x, y = y, c = c, visited = false, d = 0 };
                }
            }

            Maze maze0 = new Maze();
            maze0.grid = grid;
            maze0.width = grid.Length;
            maze0.height = grid[0].Length;

            return maze0;
        }

        static void breadthFirst_search(string[] map, int xi, int yi, int xf, int yf)
        {
            Maze maze = mapToMaze(map);
            if (!maze.isInGrid(xi, yi) || !maze.isInGrid(xf, yf))
            {
                Console.WriteLine("Out of Bounds Coords");
                return;
            }

            var cell_i = maze.grid[xi][yi];
            var cell_f = maze.grid[xf][yf];

            if (cell_i.c != '.' || cell_f.c != '.')
            {
                Console.WriteLine("Bad Cells");
                return;
            }

            bfs(maze, cell_i, cell_f);
        }

        static void bfs(Maze maze, Cell cell_i, Cell cell_f)
        {
            cell_i.visited = true;
            maze.grid[cell_i.x][cell_i.y] = cell_i;
            Queue<Cell> q = new Queue<Cell>();
            q.Enqueue(cell_i);

            int step = 0;

            while (q.Count != 0)
            {
                ++step;
                var c0 = q.Dequeue();

                // can add here some jobs and operations with the current cell c0
                // for more complex search system

                for (int k = 0; k < 4; ++k)
                {
                    int x0 = c0.x;
                    int y0 = c0.y;

                    if (k == 0) x0 = c0.x + 1;
                    else if (k == 1) x0 = c0.x - 1;
                    else if (k == 2) y0 = c0.y + 1;
                    else y0 = c0.y - 1;

                    if (!maze.isInGrid(x0, y0)) continue;

                    var c1 = maze.grid[x0][y0];
                    if (c1.visited || c1.c != '.') continue;

                    c1.visited = true;
                    c1.d = c0.d + 1;
                    q.Enqueue(c1);
                    maze.grid[x0][y0] = c1;

                    if (c1.x == cell_f.x && c1.y == cell_f.y)
                    {
                        cell_f.d = c1.d;
                        Console.WriteLine($"BFS  search; Distance ({cell_i.x,2} {cell_i.y,2}) to ({cell_f.x,2} {cell_f.y,2}) = {cell_f.d,3}; Total step:{step,3}");
                        return;
                    }
                }
            }
        }

        static void beam_search(string[] map, int beamSize, int xi, int yi, int xf, int yf)
        {
            Maze maze = mapToMaze(map);
            if (!maze.isInGrid(xi, yi) || !maze.isInGrid(xf, yf))
            {
                Console.WriteLine("Out of Bounds Coords");
                return;
            }

            var cell_i = maze.grid[xi][yi];
            var cell_f = maze.grid[xf][yf];

            if (cell_i.c != '.' || cell_f.c != '.')
            {
                Console.WriteLine("Bad Cells");
                return;
            }

            bms(maze, beamSize, cell_i, cell_f);
        }

        static void bms(Maze maze, int beamSize, Cell cell_i, Cell cell_f)
        {
            Func<Cell, int> heuristic = ca => Math.Abs(ca.x - cell_f.x) + Math.Abs(ca.y - cell_f.y); /// manhattan distance

            cell_i.visited = true;
            maze.grid[cell_i.x][cell_i.y] = cell_i;
            List<Cell> beam = new List<Cell>(); // Can be fixed size for more speed
            List<Cell> set = new List<Cell>(); // Maximum size < beamSize x maxNeighbors
            beam.Add(cell_i);

            int step = 0;

            while (beam.Count != 0)
            {
                set.Clear();

                foreach (var c0 in beam)
                {
                    ++step;

                    // can add here some jobs and operations with the current cell c0
                    // for more complex search system

                    for (int k = 0; k < 4; ++k)
                    {
                        int x0 = c0.x;
                        int y0 = c0.y;

                        if (k == 0) x0 = c0.x + 1;
                        else if (k == 1) x0 = c0.x - 1;
                        else if (k == 2) y0 = c0.y + 1;
                        else y0 = c0.y - 1;

                        if (!maze.isInGrid(x0, y0)) continue;

                        var c1 = maze.grid[x0][y0];
                        if (c1.visited || c1.c != '.') continue;

                        c1.visited = true;
                        c1.d = c0.d + 1;
                        maze.grid[x0][y0] = c1;
                        set.Add(c1);

                        if (c1.x == cell_f.x && c1.y == cell_f.y)
                        {
                            cell_f.d = c1.d;
                            Console.WriteLine($"Beam search; Distance ({cell_i.x,2} {cell_i.y,2}) to ({cell_f.x,2} {cell_f.y,2}) = {cell_f.d,3}; Total step:{step,3}; beamSize:{beamSize}");
                            return;
                        }
                    }
                }

                beam.Clear();
                beam.AddRange(set.OrderBy(heuristic).Take(beamSize)); // Sort by heuristic and restrict size
            }

            Console.WriteLine($"Beam search; Distance ({cell_i.x,2} {cell_i.y,2}) to ({cell_f.x,2} {cell_f.y,2}) => Not found; beamSize:{beamSize}");
        }
        // }

        // be careful, the below mazes representations are reversed
        static string[] mapEmpty =
        {
            ".....",
            ".....",
            ".....",
            ".....",
            "....."
        };

        static string[] mapSimple =
        {
            "...#......",
            "...#......",
            "...#......",
            "...#..###.",
            ".###..#...",
            "......#...",
            "..#####...",
            "..#.......",
            "..#......."
        };

        static String[] mapComplex =
        {
            "######################",
            "#.......######.......#",
            "#.###.#........#.###.#",
            "#.#.#.##########.#.#.#",
            "#.#.#.###.##.###.#.#.#",
            "#....................#",
            "#.#.#.##########.#.#.#",
            "#.#.#............#.#.#",
            "#.#.###.######.###.#.#",
            "#.#.###.######.###.#.#",
            "#.#.#............#.#.#",
            "#.#.#.##########.#.#.#",
            "#....................#",
            "#.#.#.###.##.###.#.#.#",
            "#.#.#.##########.#.#.#",
            "#.###.#........#.###.#",
            "#.......######.......#",
            "######################"
        };

        public static void run_comparison()
        {
            // ( 0  0) to ( 4  4) 
            breadthFirst_search(mapEmpty, 0, 0, 4, 4);
            beam_search(mapEmpty, 1, 0, 0, 4, 4); // beamSize = 1
            Console.WriteLine();

            // ( 2  2) to ( 0  4)
            breadthFirst_search(mapSimple, 2, 2, 0, 4);
            beam_search(mapSimple, 2, 2, 2, 0, 4); // beamSize = 2
            beam_search(mapSimple, 3, 2, 2, 0, 4); // beamSize = 3
            Console.WriteLine();

            // ( 3  2) to ( 8  7)
            breadthFirst_search(mapSimple, 3, 2, 8, 7);
            beam_search(mapSimple, 2, 3, 2, 8, 7); // beamSize = 2
            beam_search(mapSimple, 3, 3, 2, 8, 7); // beamSize = 3
            Console.WriteLine();

            // ( 2 10) to (15 10)
            breadthFirst_search(mapComplex, 2, 10, 15, 10);
            beam_search(mapComplex, 2, 2, 10, 15, 10); // beamSize = 2
            Console.WriteLine();

        }
    }
}
