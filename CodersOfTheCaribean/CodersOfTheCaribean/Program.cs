using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodersOfTheCaribean
{
    internal class Program
    {
        static void Main(string[] args)
        {

            int turnCounter = 0;
            bool firedLastTurn = false;
            int lastRow = -1;
            int lastCol = -1;

            List<Field> takenFields = new List<Field>();
            List<Field> takenShots = new List<Field>();
            // game loop
            while (true)
            {
                takenFields.Clear();
                takenShots.Clear();
                ++turnCounter;
                int myShipCount = int.Parse(Console.ReadLine()); // the number of remaining ships
                int entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. ships, mines or cannonballs)

                var myShips = Gameboard.Update(entityCount);
                foreach(var ship in myShips)
                {
                    // Write an action using Console.WriteLine()
                    // To debug: Console.Error.WriteLine("Debug messages...");

                    if (firedLastTurn == false)
                    {
                        var closestEnemy = (Ship)Gameboard.AllEntities
                            .Where(x => x.EntityType == EntityType.SHIP && (x as Ship).IsMyShip == false)
                            .OrderBy(x => ship.GetDistanceTo(x))
                            .First();

                        if (ship.GetDistanceTo(closestEnemy) <= 6)
                        {
                            if (!takenShots.Contains(Gameboard.Fields[closestEnemy.Col, closestEnemy.Row]))
                            {
                                Console.WriteLine($"FIRE {closestEnemy.Col} {closestEnemy.Row}");
                                firedLastTurn = true;
                                takenShots.Add(Gameboard.Fields[closestEnemy.Col, closestEnemy.Row]);
                                continue;
                            } else
                            {
                                Console.WriteLine($"FIRE {closestEnemy.CurrentPosition.FieldsTaken[0].Col} {closestEnemy.CurrentPosition.FieldsTaken[0].Row}");
                                firedLastTurn = true;
                                takenShots.Add(closestEnemy.CurrentPosition.FieldsTaken[0]);
                                continue;
                            }
                        }
                    }

                    firedLastTurn = false;
                    var nextField = Gameboard.Fields.Cast<Field>()
                        .Where(x =>!takenFields.Contains(x))
                        .OrderByDescending(x => x.Score - Math.Sqrt(ship.GetDistanceTo(x.Col,x.Row))).First();
                    //var lastField = Gameboard.Fields.Cast<Field>().OrderBy(x => x.Score).First();
                    //Console.Error.WriteLine($"Highest Score: {nextField.Score} Distance: {ship.GetDistanceTo(nextField.Col, nextField.Row)}");
                    //Console.Error.WriteLine($"Lowest Score: {lastField.Score} Distance: {ship.GetDistanceTo(lastField.Col, lastField.Row)}");
                    takenFields.Add(nextField);

                    Console.WriteLine($"MOVE {nextField.Col} {nextField.Row}");


                    //Console.Error.WriteLine($"ShipPosition: COL {ship.Col} ROW {ship.Row}");
                    //Console.Error.WriteLine($"EnemyPosition: COL {enemy.Col} ROW {enemy.Row}");
                    //Console.Error.WriteLine($"Distance: {ship.GetDistanceTo(enemy)}");

                }
            }
        }

    }





    //USES odd-r coordinates
    abstract class Entity
    {
        internal int EntityId { get; set; }
        internal int Col { get; set; }
        internal int Row { get; set; }
        internal EntityType EntityType { get; set; }

        protected Entity(string[] inputs)
        {
            EntityId = int.Parse(inputs[0]);
            string entityType = inputs[1];
            Col = int.Parse(inputs[2]);
            Row = int.Parse(inputs[3]);
            EntityType = (EntityType)Enum.Parse(typeof(EntityType), entityType);
        }

        static internal Entity Factory(string[] inputs)
        {
            switch (inputs[1])
            {
                case "SHIP": return new Ship(inputs); 
                case "MINE": return new Mine(inputs); 
                case "CANNONBALL": return new CannonBall(inputs); 
                case "BARREL": return new Barrel(inputs); 
                default: throw new NotImplementedException($"unknown entity type: {inputs[1]}");
            }
        }

        internal Tuple<int, int, int> GetCubeCoordinates()
        {
            var x = Col - (Row - (Row & 1)) / 2;
            var z = Row;
            var y = -x - z;
            return new Tuple<int, int, int>(x, y, z);
        }

        private static Tuple<int, int> CubeCoordinatesToOffsetCoordinates(Tuple<int, int, int> cube)
        {
            var col = cube.Item1 + (cube.Item3 - (cube.Item3 & 1)) / 2;
            var row = cube.Item3;
            return new Tuple<int, int>(col, row);
        }

        private static float cubeDistance(Tuple<int, int, int> a, Tuple<int, int, int> b)
        {
            return (Math.Abs(a.Item1 - b.Item1) + Math.Abs(a.Item2 - b.Item2) + Math.Abs(a.Item3 - b.Item3)) / 2;
        }

        internal float GetDistanceTo(Entity entity)
        {
            float result = cubeDistance(GetCubeCoordinates(), entity.GetCubeCoordinates());

            //Console.Error.WriteLine(result);
            return result;
        }

        internal float GetDistanceTo(int col, int row)
        {
            var x = col - (row - (row & 1)) / 2;
            var z = row;
            var y = -x - z;
            return cubeDistance(GetCubeCoordinates(), new Tuple<int, int, int>(x, y, z));
        }


    }

    internal class CannonBall : Entity
    {
        internal int ShipEntityId { get; set; }

        internal int ImpactInXTurns { get; set; }

        internal Field AimedAtField { get; set; }

        internal CannonBall(string[] inputs) : base(inputs)
        {
            int arg1 = int.Parse(inputs[4]);
            int arg2 = int.Parse(inputs[5]);
            ShipEntityId = arg1;
            ImpactInXTurns = arg2;
            AimedAtField = Gameboard.Fields[Col, Row];
        }
    }

    internal class Mine : Entity
    {
        internal Mine(string[] inputs) : base(inputs) { }
    }


    internal class Ship : Entity
    {
        internal class ShipPosition
        {
            internal Field[] FieldsTaken = new Field[3];

            internal ShipPosition(int col, int row, Rotation shipRotation)
            {
                FieldsTaken[1] = Gameboard.Fields[col, row];
                #region PositionCalculation
                bool isEvenLine = row % 2 == 0;

                switch (shipRotation)
                {
                    case Rotation.East:
                        FieldsTaken[0] = Gameboard.Fields[col + 1, row];
                        FieldsTaken[2] = Gameboard.Fields[col > 0 ? col - 1 : col, row];
                        break;                   
                    case Rotation.NorthEast:
                        if (isEvenLine)
                        {
                            FieldsTaken[0] = Gameboard.Fields[col    , row > 0 ? row - 1 : row];
                            FieldsTaken[2] = Gameboard.Fields[col > 0 ? col - 1 : col, row + 1];
                        } else
                        {
                            FieldsTaken[0] = Gameboard.Fields[col + 1, row > 0 ? row - 1 : row];
                            FieldsTaken[2] = Gameboard.Fields[col    , row + 1];
                        }
                        break;
                    case Rotation.NorthWest:
                        if (isEvenLine)
                        {
                            FieldsTaken[0] = Gameboard.Fields[col > 0 ? col - 1 : col, row > 0 ? row - 1 : row];
                            FieldsTaken[2] = Gameboard.Fields[col    , row + 1];
                        }
                        else
                        {
                            FieldsTaken[0] = Gameboard.Fields[col    , row > 0 ? row - 1 : row];
                            FieldsTaken[2] = Gameboard.Fields[col + 1, row + 1];
                        }
                        break;
                    case Rotation.West:
                        FieldsTaken[0] = Gameboard.Fields[col > 0 ? col - 1 : col, row];
                        FieldsTaken[2] = Gameboard.Fields[col + 1, row];
                        break;
                    case Rotation.SouthWest:
                        if (isEvenLine)
                        {
                            FieldsTaken[0] = Gameboard.Fields[col > 0 ? col - 1 : col, row + 1];
                            FieldsTaken[2] = Gameboard.Fields[col, row > 0 ?  row - 1 : row];
                        }
                        else
                        {
                            FieldsTaken[0] = Gameboard.Fields[col, row + 1];
                            FieldsTaken[2] = Gameboard.Fields[col + 1, row - 1];
                        }
                        break;
                    case Rotation.SouthEast:
                        if (isEvenLine)
                        {
                            FieldsTaken[0] = Gameboard.Fields[col > 0 ? col - 1 : col, row > 0 ? row - 1 : row];
                            FieldsTaken[2] = Gameboard.Fields[col, row + 1];
                        }
                        else
                        {
                            FieldsTaken[0] = Gameboard.Fields[col, row > 0 ? row - 1 : row];
                            FieldsTaken[2] = Gameboard.Fields[col + 1, row + 1];
                        }
                        break;
                    default: throw new NotImplementedException("Invalid Rotation");
                }

                #endregion
            }
        }

        private ShipPosition _shipPosition;
        internal ShipPosition CurrentPosition
        {
            get
            {
                if (_shipPosition == null)
                    _shipPosition = new ShipPosition(Col, Row, CurrentRotation);
                return _shipPosition;
            }
        }

        //internal ShipPosition GetNextPositionForCourse(int row, int col)
        //{

        //}

        internal Rotation CurrentRotation { get; set; }

        internal int Speed { get; set; }

        internal int StockOfRum { get; set; }

        internal bool IsMyShip { get; set; }

        internal Ship(string[] inputs) : base(inputs)
        {
            int arg1 = int.Parse(inputs[4]);
            int arg2 = int.Parse(inputs[5]);
            int arg3 = int.Parse(inputs[6]);
            int arg4 = int.Parse(inputs[7]);
            CurrentRotation = (Rotation)arg1;
            Speed = arg2;
            StockOfRum = arg3;
            IsMyShip = arg4 == 1 ? true : false;
        }

        internal enum Rotation
        {
            East = 0,
            NorthEast = 1,
            NorthWest = 2,
            West = 3,
            SouthWest = 4,
            SouthEast = 5
        }

        //internal 

        //internal Tuple<int,int> GetNextPosition 
    }

    internal class Barrel : Entity
    {
        internal int AmountOfRum { get; set; }

        internal Barrel(string[] inputs) : base(inputs)
        {
            int arg1 = int.Parse(inputs[4]);
            AmountOfRum = arg1;
        }
    }

    enum EntityType
    {
        SHIP,
        BARREL,
        MINE,
        CANNONBALL
    }

    internal class Field
    {
        internal int Col { get; set; }
        internal int Row { get; set; }
        internal Entity EntityOnField { get; set; }

        internal int Score { get; set; }
    }

    static class Gameboard
    {
        internal const int MAXHEIGHT = 20;
        internal const int MAXWIDTH = 22;


        static internal Field[,] Fields;

        static internal List<Entity> AllEntities = new List<Entity>();
        static Gameboard()
        {
            Fields = new Field[MAXWIDTH + 1, MAXHEIGHT + 1];
            for (int row = 0; row <= MAXHEIGHT; row++)
            {
                for (int col = 0; col <= MAXWIDTH; col++)
                {
                    Fields[col, row] = new Field
                    {
                        Col = col,
                        Row = row
                    };
                }
            }
        }

        internal static List<Ship> Update(int entityCount)
        {
            AllEntities.Clear();
            for (int row = 0; row <= MAXHEIGHT; row++)
            {
                for (int col = 0; col <= MAXWIDTH; col++)
                {
                    Fields[col, row].Score = 0;
                }
            }
            List<Ship> playerShips = new List<Ship>();
            for (int i = 0; i < entityCount; i++)
            {
                string[] inputs = Console.ReadLine().Split(' ');
                var entity = Entity.Factory(inputs);
                Fields[entity.Col, entity.Row].EntityOnField = entity;

                if (entity.EntityType == EntityType.SHIP && (entity as Ship).IsMyShip)
                    playerShips.Add(entity as Ship);

                switch(entity.EntityType)
                {
                    case EntityType.BARREL:
                        Fields[entity.Col, entity.Row].Score += (entity as Barrel).AmountOfRum;
                        addAdjacent(entity.Col, entity.Row, 6);
                        addFarAdjacent(entity.Col, entity.Row, 3);
                        break;
                    case EntityType.CANNONBALL:
                        Fields[entity.Col, entity.Row].Score -= 100;
                        addAdjacent(entity.Col, entity.Row, -50);
                        addFarAdjacent(entity.Col, entity.Row, -25);
                        break;
                    case EntityType.MINE:
                        Fields[entity.Col, entity.Row].Score -= 100;
                        addAdjacent(entity.Col, entity.Row, -50);
                        addFarAdjacent(entity.Col, entity.Row, -5);
                        break;
                    case EntityType.SHIP:
                        if (!(entity as Ship).IsMyShip)
                        {
                            Fields[entity.Col, entity.Row].Score += 5;
                            addAdjacent(entity.Col, entity.Row, +3);
                            addFarAdjacent(entity.Col, entity.Row, +1);
                        }
                        break;
                }

                AllEntities.Add(entity);
            }

            CalculateFieldValues();

            return playerShips;
        }

        private static void addFarAdjacent(int col, int row, int v)
        {
            if (row % 2 == 0)
            {
                safeAdd(col-2, row, v);
                safeAdd(col-2, row+1, v);
                safeAdd(col-1, row+2, v);
                safeAdd(col, row+2, v);
                safeAdd(col+1, row+2, v);
                safeAdd(col+1, row+1, v);
                safeAdd(col+2, row, v);
                safeAdd(col+1, row-1, v);
                safeAdd(col+1, row-2, v);
                safeAdd(col, row-2, v);
                safeAdd(col-1, row-2, v);
                safeAdd(col-2, row-1, v);

            }
            else
            {
                safeAdd(col - 2, row, v);
                safeAdd(col - 1, row + 1, v);
                safeAdd(col- 1, row+2, v);
                safeAdd(col, row+2, v);
                safeAdd(col+1, row+2, v);
                safeAdd(col+2, row+1, v);
                safeAdd(col+2, row, v);
                safeAdd(col+2, row-1, v);
                safeAdd(col+1, row-2, v);
                safeAdd(col, row-2, v);
                safeAdd(col-1, row-2, v);
                safeAdd(col-1, row-1, v);
            }
        }

        private static void addAdjacent(int col, int row, int v)
        {
            if (row % 2 == 0)
            {
                safeAdd(col-1, row-1, v);
                safeAdd(col-1, row, v);
                safeAdd(col-1, row+1, v);
                safeAdd(col, row+1, v);
                safeAdd(col+1, row, v);
                safeAdd(col, row-1, v);
            } else
            {
                safeAdd(col, row-1, v);
                safeAdd(col-1, row, v);
                safeAdd(col, row+1, v);
                safeAdd(col+1, row+1, v);
                safeAdd(col+1, row, v);
                safeAdd(col+1, row-1, v);
            }
        }

        private static void safeAdd(int col, int row, int v)
        {
            //Console.Error.WriteLine($"SAFEADD {col} {row}");
            if (col > 0 && row > 0 && col <= MAXWIDTH && row <= MAXHEIGHT)
                Fields[col, row].Score += v;
        }

        private static void CalculateFieldValues()
        {

        }

    }

}


