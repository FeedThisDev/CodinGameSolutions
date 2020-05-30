using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using static Gameboard;

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

            //Console.WriteLine("WAIT"); // MOVE <x> <y> | WAIT

        }
    }
}

public static class PlayerLogic
{
    public enum Actions
    {
        MOVE,
        WAIT,
        PLAN,
        LIGHT,
        YELL
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

        var bestField = Gameboard.Fields.Cast<Gameboard.Field>()
            .Where(x => !x.IsWall)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => player.GetDistanceTo(x))
            .First();

        DoMove(bestField);
    }

    private static void CheckYells()
    {
        foreach(var yell in Entities.Where(x => 
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
        if(X >=0 && Y >= 0 && X < WIDTH && Y < HEIGHT)
            Field = Gameboard.Fields[X, Y];
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
                return new Wanderer(inputs, true);

            default:
                throw new NotImplementedException();
        }
    }

    public bool Equals(Field field)
    {
        throw new Exception("Can't process equals on Entity and Field");
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
            return (Explorer)Gameboard.Entities.Single(x => x.ID == TargetedExplorerID);
        }
    }

    public State CurrentState { get; private set; }
    public bool IsSlasher { get; private set; }
    public Wanderer(string[] inputs, bool isSlasher = false) : base(inputs)
    {
        Type = EntityType.WANDERER;
        _time = int.Parse(inputs[4]);
        CurrentState = (State)int.Parse(inputs[5]);
        TargetedExplorerID = int.Parse(inputs[6]);
        if (isSlasher)
        {
            IsSlasher = true;
        }
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

        internal IEnumerable<Field> GetNeighouringFields(int maxDistance = 1)
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

    public static Explorer MyExplorer { get; private set; }

    public static List<Entity> Entities { get; private set; }

    public static int WIDTH { get; private set; }
    public static int HEIGHT { get; private set; }

    public static int SanityLossLonely { get; private set; }

    public static int SanityLossGroup { get; private set; }

    public static int WandererSpawnTime { get; private set; }

    public static int WandererLifeTime { get; private set; }

    public static Field[,] Fields { get; private set; }

    public static void GAMEINIT()
    {
        WIDTH = int.Parse(Console.ReadLine());
        HEIGHT = int.Parse(Console.ReadLine());
        Fields = new Field[WIDTH, HEIGHT];

        for (int row = 0; row < HEIGHT; row++)
        {
            string line = Console.ReadLine();
            for (int col = 0; col < line.Length; col++)
            {
                char symbol = line[col];
                Fields[col, row] = new Field(col, row, symbol);
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
                int score = -10;
                if (mob.IsSlasher)
                    score = -25;
                if ((mob.CurrentState == Wanderer.State.SPAWNING && mob.TimeTillSpawn <= 2) || mob.CurrentState == Wanderer.State.WANDERING)
                {
                    entity.Field.AddScore(score);
                    IEnumerable<Field> neighbouring = entity.Field.GetNeighouringFields(1);
                    foreach (var field in neighbouring)
                    {
                        field.AddScore(score);
                    }
                }
            }
            if (entity.Type == Entity.EntityType.SLASHER)
            {
                var mob = (Wanderer)entity;
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
                    field.AddScore(SanityLossLonely - SanityLossGroup);
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

    public static IEnumerable<Explorer> GetOtherExplorers()
    {
        return Entities
            .Where(x => x.Type == Entity.EntityType.EXPLORER && ((Explorer)x).IsPlayer == false)
            .Cast<Explorer>();
    }

    public static IEnumerable<Explorer> GetOtherExplorersByDistance(Entity otherEntity)
    {
        return Entities
            .Where(x => x.Type == Entity.EntityType.EXPLORER && ((Explorer)x).IsPlayer == false)
            .OrderBy(x => otherEntity.GetDistanceTo(x))
            .Cast<Explorer>();
    }

    internal static IEnumerable<Effect> GetMyEffects()
    {
        return Entities.Where(x => (x.Type == Entity.EntityType.EFFECT_LIGHT
                                    || x.Type == Entity.EntityType.EFFECT_PLAN)
                                    && ((Effect)x).CasterId == MyExplorer.ID)
            .Cast<Effect>();
    }

    internal static IEnumerable<Wanderer> GetActiveEnemies()
    {
        return Entities.Where(x =>
        x.Type == Entity.EntityType.WANDERER
        && ((Wanderer)x).CurrentState == Wanderer.State.WANDERING)
            .Cast<Wanderer>();
    }
}