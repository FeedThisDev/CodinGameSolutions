using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CodeOfKutulu.Gameboard;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CodeOfKutulu
{
    // A* needs only a WeightedGraph and a location type L, and does *not*
    // have to be a grid. However, in the example code I am using a grid.
    public interface WeightedGraph<L>
    {
        double Cost(Location a, Location b);
        IEnumerable<Location> Neighbors(Location id);
    }


    public struct Location
    {
        // Implementation notes: I am using the default Equals but it can
        // be slow. You'll probably want to override both Equals and
        // GetHashCode in a real project.

        public readonly int x, y;
        public Location(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }


    public class SquareGrid : WeightedGraph<Location>
    {
        // Implementation notes: I made the fields public for convenience,
        // but in a real project you'll probably want to follow standard
        // style and make them private.

        public static readonly Location[] DIRS = new[]
            {
            new Location(1, 0),
            new Location(0, -1),
            new Location(-1, 0),
            new Location(0, 1)
        };

        public int width, height;
        public HashSet<Location> walls = new HashSet<Location>();
        public HashSet<Location> forests = new HashSet<Location>();

        public SquareGrid(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public bool InBounds(Location id)
        {
            return 0 <= id.x && id.x < width
                && 0 <= id.y && id.y < height;
        }

        public bool Passable(Location id)
        {
            return !walls.Contains(id);
        }

        public double Cost(Location a, Location b)
        {
            return forests.Contains(b) ? 5 : 1;
        }

        public IEnumerable<Location> Neighbors(Location id)
        {
            foreach (var dir in DIRS)
            {
                Location next = new Location(id.x + dir.x, id.y + dir.y);
                if (InBounds(next) && Passable(next))
                {
                    yield return next;
                }
            }
        }
    }


    public class PriorityQueue<T>
    {
        // I'm using an unsorted array for this example, but ideally this
        // would be a binary heap. There's an open issue for adding a binary
        // heap to the standard C# library: https://github.com/dotnet/corefx/issues/574
        //
        // Until then, find a binary heap class:
        // * https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
        // * http://visualstudiomagazine.com/articles/2012/11/01/priority-queues-with-c.aspx
        // * http://xfleury.github.io/graphsearch.html
        // * http://stackoverflow.com/questions/102398/priority-queue-in-net

        private List<Tuple<T, double>> elements = new List<Tuple<T, double>>();

        public int Count
        {
            get { return elements.Count; }
        }

        public void Enqueue(T item, double priority)
        {
            elements.Add(Tuple.Create(item, priority));
        }

        public T Dequeue()
        {
            int bestIndex = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Item2 < elements[bestIndex].Item2)
                {
                    bestIndex = i;
                }
            }

            T bestItem = elements[bestIndex].Item1;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }
    }


    /* NOTE about types: in the main article, in the Python code I just
     * use numbers for costs, heuristics, and priorities. In the C++ code
     * I use a typedef for this, because you might want int or double or
     * another type. In this C# code I use double for costs, heuristics,
     * and priorities. You can use an int if you know your values are
     * always integers, and you can use a smaller size number if you know
     * the values are always small. */

    public class AStarSearch
    {
        public Dictionary<Location, Location> cameFrom
            = new Dictionary<Location, Location>();
        public Dictionary<Location, double> costSoFar
            = new Dictionary<Location, double>();

        // Note: a generic version of A* would abstract over Location and
        // also Heuristic
        static public double Heuristic(Location a, Location b)
        {
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }

        public AStarSearch(WeightedGraph<Location> graph, Location start, Location goal)
        {
            var frontier = new PriorityQueue<Location>();
            frontier.Enqueue(start, 0);

            cameFrom[start] = start;
            costSoFar[start] = 0;

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current.Equals(goal))
                {
                    break;
                }

                foreach (var next in graph.Neighbors(current))
                {
                    double newCost = costSoFar[current]
                        + graph.Cost(current, next);
                    if (!costSoFar.ContainsKey(next)
                        || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        double priority = newCost + Heuristic(next, goal);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }
        }
    }

    public class Test
    {
        static void DrawGrid(SquareGrid grid, AStarSearch astar)
        {
            // Print out the cameFrom array
            for (var y = 0; y < 10; y++)
            {
                for (var x = 0; x < 10; x++)
                {
                    Location id = new Location(x, y);
                    Location ptr = id;
                    if (!astar.cameFrom.TryGetValue(id, out ptr))
                    {
                        ptr = id;
                    }
                    if (grid.walls.Contains(id)) { Console.Write("##"); }
                    else if (ptr.x == x + 1) { Console.Write("\u2192 "); }
                    else if (ptr.x == x - 1) { Console.Write("\u2190 "); }
                    else if (ptr.y == y + 1) { Console.Write("\u2193 "); }
                    else if (ptr.y == y - 1) { Console.Write("\u2191 "); }
                    else { Console.Write("* "); }
                }
                Console.WriteLine();
            }
        }

        static void Main()
        {
            // Make "diagram 4" from main article
            var grid = new SquareGrid(10, 10);
            for (var x = 1; x < 4; x++)
            {
                for (var y = 7; y < 9; y++)
                {
                    grid.walls.Add(new Location(x, y));
                }
            }
            grid.forests = new HashSet<Location>
            {
                new Location(3, 4), new Location(3, 5),
                new Location(4, 1), new Location(4, 2),
                new Location(4, 3), new Location(4, 4),
                new Location(4, 5), new Location(4, 6),
                new Location(4, 7), new Location(4, 8),
                new Location(5, 1), new Location(5, 2),
                new Location(5, 3), new Location(5, 4),
                new Location(5, 5), new Location(5, 6),
                new Location(5, 7), new Location(5, 8),
                new Location(6, 2), new Location(6, 3),
                new Location(6, 4), new Location(6, 5),
                new Location(6, 6), new Location(6, 7),
                new Location(7, 3), new Location(7, 4),
                new Location(7, 5)
            };

            // Run A*
            var astar = new AStarSearch(grid, new Location(1, 4),
                                        new Location(8, 5));

            DrawGrid(grid, astar);
        }
    }
}

namespace CodeOfKutulu
{

    public abstract class Effect : Entity
    {
        public int TimeToFadeOut { get; private set; }
        public int CasterId { get; private set; }

        public virtual int Radius { get; protected set; }

        public Effect(string[] inputs) : base(inputs)
        {
            TimeToFadeOut = int.Parse(inputs[4]);
            CasterId = int.Parse(inputs[5]);
        }
    }



    public class Plan : Effect
    {
        public Plan(string[] inputs) : base(inputs)
        {
            Radius = 2;
            Type = EntityType.EFFECT_PLAN;
        }
    }

    public class Shelter : Effect
    {
        public int RemainingEnergy { get { return TimeToFadeOut; } }
        public Shelter(string[] inputs) : base(inputs)
        {
            Radius = 0;
            Type = EntityType.EFFECT_SHELTER;
        }
    }

    public class Yell : Effect
    {
        public int YelledPlayerID { get; private set; }
        public Yell(string[] inputs) : base(inputs)
        {
            Radius = 1;
            Type = EntityType.EFFECT_YELL;
            YelledPlayerID = int.Parse(inputs[6]);
        }
    }


    public class Light : Effect
    {
        public Light(string[] inputs) : base(inputs)
        {
            Radius = 5;
            Type = EntityType.EFFECT_LIGHT;
        }
    }

}

namespace CodeOfKutulu
{

    public abstract class Entity
    {
        public enum EntityType
        {
            WANDERER,
            EXPLORER,
            EFFECT_PLAN,
            EFFECT_LIGHT,
            SLASHER,
            EFFECT_SHELTER,
            EFFECT_YELL,
        }

        public EntityType Type { get; protected set; }
        
        public int ID { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public int Col { get { return X; } }

        public int Row { get { return Y; } }

        protected Entity(string[] inputs)
        {
            ID = int.Parse(inputs[1]);
            X = int.Parse(inputs[2]);
            Y = int.Parse(inputs[3]);
        }

        public static Entity CreateEntityFromString(string entityDescription, bool isPlayer = false)
        {

            string[] inputs = entityDescription.Split(' ');
            var type = (EntityType)Enum.Parse(typeof(EntityType), inputs[0]);
            switch (type)
            {
                case EntityType.EXPLORER:
                    return new Explorer(inputs, isPlayer);

                case EntityType.WANDERER:
                    return new Wanderer(inputs);

                case EntityType.EFFECT_LIGHT:
                    return new Light(inputs);

                case EntityType.EFFECT_PLAN:
                    return new Plan(inputs);

                case EntityType.EFFECT_YELL:
                    return new Yell(inputs);

                case EntityType.EFFECT_SHELTER:
                    return new Shelter(inputs);

                case EntityType.SLASHER:
                    return new Slasher(inputs);

                default:
                    throw new NotImplementedException();
            }
        }

        public bool Equals(Field field)
        {
            throw new Exception("Can't process equals on Entity and Field");
        }

        public override string ToString()
        {
            return $"{X} {Y}";
        }
    }

}

namespace CodeOfKutulu
{
    public class Explorer : Entity
    {
        public int Sanity { get; private set; }

        public bool IsPlayer { get; private set; }
        public Explorer(string[] inputs, bool isPlayer = false) : base(inputs)
        {
            Type = EntityType.EXPLORER;
            Sanity = int.Parse(inputs[4]);
            int param1 = int.Parse(inputs[5]);
            int param2 = int.Parse(inputs[6]);
            IsPlayer = isPlayer;
        }
    }
}

namespace CodeOfKutulu
{
    public class Field
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public int Score { get; set; }

        public int Col { get { return X; } }

        public int Row { get { return Y; } }

        public bool IsWall { get; private set; }
        public bool IsSpwan { get; private set; }
        public bool IsShelter { get; private set; }


        public Field(int x, int y, char symbol)
        {
            X = x;
            Y = y;
            IsWall = symbol == '#';
            IsSpwan = symbol == 'w';
            IsSpwan = symbol == 'U';
        }

        internal void AddScore(int toAdd)
        {
            Score += toAdd;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Field objAsField = obj as Field;
            if (objAsField == null)
                return false;
            else
                return Equals(objAsField);
        }
        public override int GetHashCode()
        {
            return (Col << 8) + Row;
        }
        public bool Equals(Field other)
        {
            if (other == null)
                return false;
            return this.Col == other.Col && this.Row == other.Row;
        }

       

        public int GetDistanceTo(Entity otherEntity)
        {
            return Math.Abs(this.X - otherEntity.X) + Math.Abs(this.Y - otherEntity.Y);
        }

        public int GetDistanceTo(Field otherField)
        {
            return Math.Abs(this.X - otherField.X) + Math.Abs(this.Y - otherField.Y);
        }

        public override string ToString()
        {
            return $"{X} {Y}";
        }

        internal void ResetScore()
        {
            if (IsSpwan)
                Score = -5;
            else
                Score = 0;
        }
    }
}

namespace CodeOfKutulu
{
    public static class Game
    {
        public static int SanityLossLonely { get; private set; }

        public static int SanityLossGroup { get; private set; }

        public static int WandererSpawnTime { get; private set; }

        public static int WandererLifeTime { get; private set; }

        public static Gameboard Gameboard { get; private set; }
        

        public static void GAMEINIT()
        {
            int width = int.Parse(Console.ReadLine());
            int height = int.Parse(Console.ReadLine());

            Gameboard = new Gameboard(width, height);

            for (int row = 0; row < height; row++)
            {
                string line = Console.ReadLine();
                for (int col = 0; col < line.Length; col++)
                {
                    char symbol = line[col];
                    Gameboard.Fields[col, row] = new Field(col, row, symbol);
                }
            }

            string[] inputs = Console.ReadLine().Split(' ');

            SanityLossLonely = int.Parse(inputs[0]); // how much sanity you lose every turn when alone, always 3 until wood 1
            SanityLossGroup = int.Parse(inputs[1]); // how much sanity you lose every turn when near another player, always 1 until wood 1
            WandererSpawnTime = int.Parse(inputs[2]); // how many turns the wanderer take to spawn, always 3 until wood 1
            WandererLifeTime = int.Parse(inputs[3]); // how many turns the wanderer is on map after spawning, always 40 until wood 1
        }

        public static void TURNINIT()
        {
            Gameboard.Entities.Clear();
            int entityCount = int.Parse(Console.ReadLine()); // the first given entity corresponds to your explorer

            Gameboard.MyExplorer = (Explorer)Entity.CreateEntityFromString(Console.ReadLine(), true);
            Gameboard.Entities.Add(Gameboard.MyExplorer);

            for (int i = 1; i < entityCount; i++)
            {
                var entity = Entity.CreateEntityFromString(Console.ReadLine());
                Gameboard.Entities.Add(entity);
            }

            Gameboard.CalculateScoreForEachField();
        }

    }
}

namespace CodeOfKutulu
{
    public class Gameboard
    {
        public Gameboard(int width, int height)
        {
            WIDTH = width;
            HEIGHT = height;
            Fields = new Field[WIDTH, HEIGHT];
        }

        public int WIDTH { get; private set; }
        public int HEIGHT { get; private set; }

        public Explorer MyExplorer { get; set; }

        public List<Entity> Entities { get; private set; }
        
        public Field[,] Fields { get; private set; }

      

        public void CalculateScoreForEachField()
        {
            for (int col = 0; col < WIDTH; col++)
            {
                for (int row = 0; row < HEIGHT; row++)
                {
                    Fields[col, row].ResetScore();
                }
            }

            foreach (var entity in Entities.Where(x => x.ID != MyExplorer.ID))
            {
                if (entity.Type == Entity.EntityType.WANDERER)
                {
                    var mob = (Wanderer)entity;
                    int score = -20;

                    if ((mob.CurrentState == Wanderer.State.SPAWNING && mob.TimeTillSpawn <= 2) || mob.CurrentState == Wanderer.State.WANDERING)
                    {
                        Fields[mob.X, mob.Y].AddScore(score);
                        IEnumerable<Field> neighbouring = GetNeighouringFields(entity, 1);
                        foreach (var field in neighbouring)
                        {
                            field.AddScore(score);
                        }
                    }
                }
                if (entity.Type == Entity.EntityType.SLASHER)
                {
                    var mob = (Slasher)entity;
                    if ((mob.CurrentState == Wanderer.State.SPAWNING && mob.TimeTillSpawn <= 2) || mob.CurrentState == Wanderer.State.WANDERING)
                    {
                        entity.Field.AddScore(-50);
                        IEnumerable<Field> neighbouring = entity.Field.GetNeighouringFields(1);
                        foreach (var field in neighbouring)
                        {
                            field.AddScore(-25);
                        }
                    }
                }
                if (entity.Type == Entity.EntityType.EXPLORER)
                {
                    foreach (var field in entity.Field.GetNeighouringFields(2))
                    {
                        field.AddScore(Game.SanityLossLonely - Game.SanityLossGroup);
                    }
                }
                if (entity.Type == Entity.EntityType.EFFECT_LIGHT)
                {
                    var effect = (Effect)entity;
                    foreach (var field in entity.Field.GetNeighouringFields(effect.Radius))
                    {
                        field.AddScore(1);
                    }
                }
                if (entity.Type == Entity.EntityType.EFFECT_PLAN)
                {
                    var effect = (Effect)entity;
                    foreach (var field in entity.Field.GetNeighouringFields(effect.Radius))
                    {
                        field.AddScore(2);
                    }
                }
                if (entity.Type == Entity.EntityType.EFFECT_SHELTER)
                {
                    var effect = (Shelter)entity;
                    entity.Field.AddScore(effect.RemainingEnergy * 5);
                }
            }

            Console.Error.WriteLine("BEST FIELDS");
            foreach (var field in Fields.Cast<Field>().OrderByDescending(x => x.Score).ThenBy(x => MyExplorer.GetDistanceTo(x)).Take(3))
                Console.Error.WriteLine($"{field}: {field.Score} IsWall: {field.IsWall}");

            //var closestWanderer

        }


        internal IEnumerable<Field> GetNeighouringFields(Entity entityOnField, int maxDistance = 1)
        {
            return GetNeighouringFields(entityOnField.X, entityOnField.Y, maxDistance);
        }

        internal IEnumerable<Field> GetNeighouringFields(Field ofField, int maxDistance = 1)
        {
            return GetNeighouringFields(ofField.X, ofField.Y, maxDistance);
        }

        internal IEnumerable<Field> GetNeighouringFields(int X, int Y, int maxDistance = 1)
        {
            List<Field> result = new List<Field>();

            for (int col = X - maxDistance; col <= X + maxDistance; col++)
            {
                for (int row = Y - maxDistance; row <= Y + maxDistance; row++)
                {
                    if (col < 0 || row < 0 || col >= WIDTH || row >= HEIGHT)
                        continue;

                    if (this.GetDistanceTo(Fields[col, row]) > maxDistance)
                        continue;

                    if (Fields[col, row].IsWall)
                        continue;

                    result.Add(Fields[col, row]);
                }
            }
            return result;
        }

        public static int GetDistanceBetween(Entity entity1, Entity entity2)
        {
            return Math.Abs(entity1.X - entity2.X) + Math.Abs(entity1.Y - entity2.Y);
        }

        public static int GetDistanceBetween(Entity entity1, Field field1)
        {
            return Math.Abs(entity1.X - field1.X) + Math.Abs(entity1.Y - field1.Y);
        }

        public static int GetDistanceBetween(Field field1, Entity entity1)
        {
            return GetDistanceBetween(entity1, field1);
        }

        public static int GetDistanceBetween(Field field1, Field field2)
        {
            return Math.Abs(field1.X - field2.X) + Math.Abs(field1.Y - field2.Y);
        }


        public IEnumerable<Explorer> GetOtherExplorers()
        {
            return Entities
                .Where(x => x.Type == Entity.EntityType.EXPLORER && ((Explorer)x).IsPlayer == false)
                .Cast<Explorer>();
        }

        public IEnumerable<Explorer> GetOtherExplorersByDistance(Entity otherEntity)
        {
            return Entities
                .Where(x => x.Type == Entity.EntityType.EXPLORER && ((Explorer)x).IsPlayer == false)
                .OrderBy(x => GetDistanceBetween(x, otherEntity))
                .Cast<Explorer>();
        }

        internal IEnumerable<Effect> GetMyEffects()
        {
            return Entities.Where(x => (x.Type == Entity.EntityType.EFFECT_LIGHT
                                        || x.Type == Entity.EntityType.EFFECT_PLAN)
                                        && ((Effect)x).CasterId == MyExplorer.ID)
                .Cast<Effect>();

        }

        internal IEnumerable<Wanderer> GetActiveEnemies()
        {
            return Entities.Where(x =>
            x.Type == Entity.EntityType.WANDERER
            && ((Wanderer)x).CurrentState == Wanderer.State.WANDERING)
                .Cast<Wanderer>();
        }
    }
}

namespace CodeOfKutulu
{
    public static class PlayerLogic
    {
        public enum Actions
        {
            MOVE,
            WAIT,
            PLAN,
            LIGHT,
            YELL,
        }

        static int _yellsLeft = 3;
        static List<int> AlreadyYelledPlayerIDs = new List<int>();
        static int _plansLeft = 2;
        static int _lightsLeft = 3;

        public static void DoTurn()
        {
            CheckYells();
            var player = Gameboard.MyExplorer;

            if (ShouldYell())
                DoYell();

            var bestField = Gameboard.Fields.Cast<Field>()
                .Where(x => !x.IsWall)
                .OrderByDescending(x => x.Score)
                .ThenBy(x => player.GetDistanceTo(x))
                .First();

            DoMove(bestField);
        }

        private static void CheckYells()
        {
            foreach (var yell in Entities.Where(x =>
                                         x.Type == Entity.EntityType.EFFECT_YELL
                                         && (x as Yell).CasterId == MyExplorer.ID)
                                        .Cast<Yell>())
            {
                if (!AlreadyYelledPlayerIDs.Contains(yell.YelledPlayerID))
                    AlreadyYelledPlayerIDs.Add(yell.YelledPlayerID);
            }
        }

        private static void DoYell()
        {
            _yellsLeft--;
            Console.WriteLine(Actions.YELL + " - bye,bye looser");
        }

        private static bool ShouldYell()
        {
            if (_yellsLeft > 0 && !GetMyEffects().Any())
            {
                int countPlayersAroundMe = Gameboard.GetOtherExplorers().Where(x => Gameboard.MyExplorer.GetDistanceTo(x) <= 1).Count();
                if (countPlayersAroundMe >= 1)
                {
                    bool newPlayerFound = GetOtherExplorers().Where(x => Gameboard.MyExplorer.GetDistanceTo(x) <= 1 && !AlreadyYelledPlayerIDs.Contains(x.ID)).Any();

                    int countEnemiesAroundMe = Gameboard.GetActiveEnemies().Where(x => MyExplorer.GetDistanceTo(x) <= 3).Count();
                    if (countEnemiesAroundMe >= 1)
                    {
                        return true;
                    }
                }

            }
            return false;
        }

        private static void Wait()
        {
            if (_plansLeft > 0 && !GetMyEffects().Any() && MyExplorer.Sanity <= 200)
            {
                int countPlayersAroundMe = Gameboard.GetOtherExplorers().Where(x => Gameboard.MyExplorer.GetDistanceTo(x) <= 2).Count();
                if (countPlayersAroundMe > 1)
                {
                    CastPlan();
                    return;
                }
            }
            if (_lightsLeft > 0 && !GetMyEffects().Any())
            {
                int countEnemiesAroundMe = Gameboard.GetActiveEnemies().Where(x => MyExplorer.GetDistanceTo(x) <= 5).Count();
                if (countEnemiesAroundMe > 2)
                {
                    CastLight();
                    return;
                }
            }

            Console.WriteLine(Actions.WAIT);
        }

        private static void CastPlan()
        {
            _plansLeft--;
            Console.WriteLine(Actions.PLAN);
        }
        private static void CastLight()
        {
            _lightsLeft--;
            Console.WriteLine(Actions.LIGHT);
        }

        private static void DoMove(Field destination)
        {
            if (MyExplorer.Field.Equals(destination))
            {
                Wait();
                return;
            }

            Console.WriteLine($"{Actions.MOVE} {destination}");
        }
    }

}

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

namespace CodeOfKutulu
{

    public class Slasher : Wanderer
    {
        public new Explorer TargetedExplorer
        {
            get
            {
                if (TargetedExplorerID == -1)
                    return null;
                return (Explorer)Game.Gameboard.Entities.Single(x => x.ID == TargetedExplorerID);
            }
        }

        public Slasher(string[] inputs) : base(inputs)
        {
            Type = EntityType.WANDERER;
        }
    }
}

namespace CodeOfKutulu
{

    public class Wanderer : Entity
    {
        public enum State
        {
            SPAWNING,
            WANDERING,
            STALKING,
            RUSHING,
            STUNNED
        }
        private int _time;
        public int TimeTillSpawn
        {
            get
            {
                return _time;
            }
        }

        public int TimeBeforeRecall
        {
            get
            {
                return _time;
            }
        }

        public int TargetedExplorerID { get; private set; }

        public Explorer TargetedExplorer
        {
            get
            {
                if (TargetedExplorerID == -1)
                    return null;
                return (Explorer) Game.Gameboard.Entities.Single(x => x.ID == TargetedExplorerID);
            }
        }

        public State CurrentState { get; private set; }
        public Wanderer(string[] inputs) : base(inputs)
        {
            Type = EntityType.WANDERER;
            _time = int.Parse(inputs[4]);
            CurrentState = (State)int.Parse(inputs[5]);
            TargetedExplorerID = int.Parse(inputs[6]);           
        }
    }
}

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.

// The following GUID is for the ID of the typelib if this project is exposed to COM

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
