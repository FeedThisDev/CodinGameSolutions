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

            Console.WriteLine("WAIT"); // MOVE <x> <y> | WAIT

        }
    }
}

public static class PlayerLogic
{    public enum Actions
    {
        MOVE,
        WAIT
    }
    public static void DoTurn()
    {
        var player = Gameboard.MyExplorer;
        var bestField = Gameboard.Fields.Cast<Gameboard.Field>().Where(x => !x.IsWall)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => player.GetDistanceTo(x))
            .First();

        Console.WriteLine($"{Actions.MOVE} {bestField}");
    }

    private static void Wait()
    {
        Console.WriteLine(Actions.WAIT);
    }

    private static void DoMove(Explorer clostestPlayer)
    {
        Console.WriteLine($"{Actions.MOVE} {clostestPlayer}");
    }
}

public abstract class Entity
{
    public enum EntityType
    {
        WANDERER,
        EXPLORER
    }

    public EntityType Type { get; protected set; }

    public Gameboard.Field Field { get; private set; }

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

        Field = Gameboard.Fields[X, Y];
    }

    public static Entity CreateEntityFromString(string entityDescription, bool isPlayer = false)
    {

        string[] inputs = entityDescription.Split(' ');
        var type  = (EntityType)Enum.Parse(typeof(EntityType), inputs[0]);
        switch (type)
        {
            case EntityType.EXPLORER:
                return new Explorer(inputs, isPlayer);

            case EntityType.WANDERER:
                return new Wanderer(inputs);

            default:
                throw new NotImplementedException();
        }
    }

    public int GetDistanceTo(Entity otherEntity)
    {
        return Math.Abs(this.X - otherEntity.X) + Math.Abs(this.Y - otherEntity.Y);
    }

    public int GetDistanceTo(Gameboard.Field otherField)
    {
        return Math.Abs(this.X - otherField.X) + Math.Abs(this.Y - otherField.Y);
    }

    public override string ToString()
    {
        return $"{X} {Y}";
    }   
}

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

public class Wanderer : Entity
{
    public enum State
    {
        SPAWNING,
        WANDERING
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
            return (Explorer)Gameboard.Entities.Single(x => x.ID == TargetedExplorerID);
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

public static class Gameboard
{
    static Gameboard()
    {
        Entities = new List<Entity>();
    }

    public class Field
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public int Score { get; set; }

        public int Col { get { return X; } }

        public int Row { get { return Y; } }

        public bool IsWall { get; private set; }
        public bool IsSpwan { get; private set; }


        public Field(int x, int y, bool isWall, bool isSpawn)
        {
            X = x;
            Y = y;
            IsWall = isWall;
            IsSpwan = isSpawn;
        }

        internal void AddScore(int toAdd)
        {
            Score += toAdd;
        }


        internal IEnumerable<Field> GetNeighouringFields(int maxDistance = 1)
        {
            List<Field> result = new List<Field>();

            for (int col = X - maxDistance; col < X + maxDistance; col++)
            {
                for (int row = Y - maxDistance; row < Y + maxDistance; row++)
                {
                    if (col < 0 || row < 0 || col > Gameboard.Width || row > Gameboard.Height)
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

        public int GetDistanceTo(Entity otherEntity)
        {
            return Math.Abs(this.X - otherEntity.X) + Math.Abs(this.Y - otherEntity.Y);
        }

        public int GetDistanceTo(Field otherField)
        {
            return Math.Abs(this.X - otherField.X) + Math.Abs(this.Y - otherField.Y);
        }

    }

    public static Explorer MyExplorer { get; private set; }

    public static List<Entity> Entities { get; private set; }

    public static int Width { get; private set; }
    public static int Height { get; private set; }

    public static int SanityLossLonely { get; private set; }

    public static int SanityLossGroup { get; private set; }

    public static int WandererSpawnTime { get; private set; }

    public static int WandererLifeTime { get; private set; }

    public static Field[,] Fields { get; private set; }

    public static void GAMEINIT()
    {
        Width = int.Parse(Console.ReadLine());
        Height = int.Parse(Console.ReadLine());
        Fields = new Field[Width, Height];

        for (int row = 0; row < Height; row++)
        {
            string line = Console.ReadLine();
            for (int col = 0; col < line.Length; col++)
            {
                char symbol = line[col];
                Fields[col, row] = new Field(col, row, symbol == '#', symbol == 'w');
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
        Entities.Clear();
        int entityCount = int.Parse(Console.ReadLine()); // the first given entity corresponds to your explorer

        MyExplorer = (Explorer)Entity.CreateEntityFromString(Console.ReadLine(), true);
        Entities.Add(MyExplorer);

        for (int i = 1; i < entityCount; i++)
        {
            var entity = Entity.CreateEntityFromString(Console.ReadLine());
            Entities.Add(entity);
        }

        CalculateScore();
    }

    private static void CalculateScore()
    {
        foreach (var entity in Entities.Where(x => x.ID != MyExplorer.ID))
        {
            if (entity.Type == Entity.EntityType.WANDERER)
            {
                var mob = (Wanderer)entity;
                if ((mob.CurrentState == Wanderer.State.SPAWNING && mob.TimeTillSpawn <= 2) || mob.CurrentState == Wanderer.State.WANDERING)
                {
                    entity.Field.AddScore(-20);
                    IEnumerable<Field> neighbouring = entity.Field.GetNeighouringFields(1);
                    foreach (var field in neighbouring)
                    {
                        field.AddScore(-10);
                    }
                }
            }
            if (entity.Type == Entity.EntityType.EXPLORER)
            {
                foreach (var field in entity.Field.GetNeighouringFields(2))
                {
                    field.AddScore(SanityLossLonely - SanityLossGroup);
                }
            }
        }
    }


    public static IEnumerable<Explorer> GetOtherExplorersByDistance(Entity otherEntity)
    {
        return Entities
            .Where(x => x.Type == Entity.EntityType.EXPLORER && ((Explorer)x).IsPlayer == false)
            .OrderBy(x => otherEntity.GetDistanceTo(x))
            .Cast<Explorer>();
    }
}