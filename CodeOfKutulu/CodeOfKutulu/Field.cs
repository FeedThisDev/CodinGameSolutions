using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CodeOfKutulu.Gameboard;

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
}
