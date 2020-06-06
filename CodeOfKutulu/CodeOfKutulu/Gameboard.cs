using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
