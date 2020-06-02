using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOfKutulu
{
    public static class Gameboard
    {
        static Gameboard()
        {
            Entities = new List<Entity>();
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
}
