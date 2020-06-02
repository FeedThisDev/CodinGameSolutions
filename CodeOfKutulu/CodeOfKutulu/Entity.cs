using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CodeOfKutulu.Gameboard;

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

        public Field Field { get; private set; }

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
            if (X >= 0 && Y >= 0 && X < WIDTH && Y < HEIGHT)
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

        public int GetDistanceTo(Field otherField)
        {
            return Math.Abs(this.X - otherField.X) + Math.Abs(this.Y - otherField.Y);
        }

        public override string ToString()
        {
            return $"{X} {Y}";
        }
    }

}
