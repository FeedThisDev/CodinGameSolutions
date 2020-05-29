using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;

namespace CodersOfTheCaribean
{
    public class Program
    {
        #region DEBUGSTUFF
        static Dictionary<string, Stopwatch> swDict = new Dictionary<string, Stopwatch>();
        const LogLevel CurrentLogLevel = LogLevel.INFO;
        public enum LogLevel
        {
            DISABLED = 0,
            ERROR = 1,
            WARN = 3,
            INFO = 4,
            TRACE = 5,
        }


        public static void Log(string log, LogLevel logLevel)
        {
            if (logLevel <= CurrentLogLevel)
            {
                Console.Error.WriteLine(log);
                Console.Error.Flush();
            }
        }
        public static void StartFunction(string functionName)
        {
            Stopwatch sw;
            if (!swDict.TryGetValue(functionName, out sw))
                swDict.Add(functionName, new Stopwatch());

            swDict[functionName].Restart();
        }
        public static void StopFunction(string functionName, LogLevel logLevel)
        {
            swDict[functionName].Stop();
            if (logLevel <= CurrentLogLevel)
                Console.Error.WriteLine($"{functionName} took {swDict[functionName].ElapsedMilliseconds} ms");
        }
        #endregion

        static void Main(string[] args)
        {
            Random randGen = new Random();
            int turnCounter = 0;

            List<Field> takenFieldDestinations = new List<Field>(); // don't send several ships to the same Field
            List<Field> takenShotDestinations = new List<Field>();  // don't shoot at the same field as other ships

            // game loop
            while (true)
            {
                StartFunction("MainLoop");
                takenFieldDestinations.Clear();
                takenShotDestinations = GetShotsInTheAir();
                ++turnCounter;
                int myShipCount = int.Parse(Console.ReadLine()); // the number of remaining ships
                int entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. ships, mines or cannonballs)

                Gameboard.UpdateBoard(entityCount);

                foreach (var ship in Gameboard.GetPlayerShips()
                                              .OrderBy(x => x.EntityId))
                {
                    StartFunction("ShipLogic");

                    //if (shipCount++ > 0)
                    //{
                    //    Console.WriteLine("WAIT");
                    //    break;
                    //}

                    var barrels = Gameboard.GetBarrels();

                    #region FireShot?
                    if (ship.HasFiredLastRound == false && !barrels.Any())
                    {
                        var closestEnemy = Gameboard.GetEnemyShipsByDistance(ship).First();
                        var distance = ship.GetDistanceTo(closestEnemy);

                        if (distance <= 12)
                        {
                            Field shootAtField = GetTarget(takenShotDestinations, closestEnemy, distance);
                            if (ship.GetDistanceTo(shootAtField.Col, shootAtField.Row) < 10)
                            {
                                Console.WriteLine($"FIRE {shootAtField}");
                                ship.HasFiredLastRound = true;
                                continue;
                            }
                        }
                    }
                    else
                    {
                        ship.HasFiredLastRound = false;
                    }
                    #endregion FireShot?

                    #region PlaceMine?
                    ////Place Mine?
                    //int rand = randGen.Next(3) + 1;
                    //if (ship.Speed > 0 && rand % 2 == 0)
                    //{
                    //    Field fieldBehindShip = ship.GetFieldBehind();
                    //    if (fieldBehindShip.EntityOnField?.EntityType != Entity.EntityTypeEnum.SHIP || (fieldBehindShip.EntityOnField as Ship).IsMyShip == false)
                    //    {
                    //        Console.WriteLine("MINE");
                    //        continue;
                    //    }
                    //}
                    #endregion PlaceMine?

                    Field nextField;
                    
                    if (barrels.Any(x => !takenFieldDestinations.Contains(Gameboard.Fields[x.Col,x.Row])))
                    {
                        nextField = barrels.Select(x => Gameboard.Fields[x.Col,x.Row])
                           .Where(x => !takenFieldDestinations.Contains(x))
                           .OrderBy(x => ship.GetDistanceTo(x.Col, x.Row))
                           .ThenByDescending(x => (x.EntityOnField as Barrel).AmountOfRum)
                           .First();
                    } 
                    else
                    {
                        var closestEnemy = Gameboard.GetEnemyShipsByDistance(ship).First();
                        nextField = Gameboard.Fields[closestEnemy.Col, closestEnemy.Row];
                    }

                    takenFieldDestinations.Add(nextField);

                    Log($"TargetDestination: {nextField.ToString()}", LogLevel.WARN);

                    var getNextStep = FindOptimalPath(ship, nextField);

                    Console.WriteLine($"MOVE {getNextStep.ToString()}");

                    StopFunction("ShipLogic", LogLevel.TRACE);
                }
                Console.Out.Flush();
                Console.Error.Flush();


                StopFunction("MainLoop", LogLevel.INFO);
            }
        }

        private static Field FindOptimalPath(Ship ship, Field targetField)
        {
            Field startField = Gameboard.Fields[ship.Col, ship.Row];

            Log($"StartField {startField}", LogLevel.WARN);
            Log($"TargetField {targetField}", LogLevel.WARN);

            AStarSearch search = new AStarSearch(startField, targetField);

            //Field lastCurrent = null;
            //var current = search.cameFrom[targetField];
            //while(current != null)
            //{
            //    lastCurrent = current.CloneLocation();
            //    current = search.cameFrom[current];
            //}

            //return lastCurrent;

            foreach (var kvp in search.cameFrom)
            {
                if (kvp.Value != null)
                    Log($"FROM {kvp.Key.ToString()} TO {kvp.Value.ToString()}", LogLevel.WARN);
                else
                    Log($"FROM {kvp.Key.ToString()}", LogLevel.WARN);
            }

            Log($"SearchLookup {search.cameFrom[Gameboard.Fields[ship.Col, ship.Row]]}", LogLevel.WARN);
            var gotoNextField = search.cameFrom[Gameboard.Fields[ship.Col, ship.Row]] ?? targetField;
            Log($"FROM {gotoNextField}", LogLevel.WARN);
            return gotoNextField;

            //List<Field> path = new List<Field>();
            //var current = search.cameFrom[startField];
            //while(current != null)
            //{
            //    path.Add(current);
            //    current = search.cameFrom[current];
            //}            

            //Log($"Path.Length {path.Count()}", LogLevel.WARN);

            //for (int i = 0; i < path.Count(); i++)
            //    Log($"Path[{i}]: {path[i].ToString()}", LogLevel.WARN);

            //var gotoField = path.FirstOrDefault(x => !x.Equals(startField));
            //if(gotoField == null)
            //    gotoField = Gameboard.Fields[0,0];
            //return gotoField;
        }

        private static IEnumerable<Field> getBlockedFields(Ship ship)
        {
            var mines = Gameboard.GetMines();
            var otherShips = Gameboard.GetShips().Where(x => x.EntityId != ship.EntityId);
            var cannonBalls = Gameboard.GetCannonballs();


            return mines.Cast<Entity>()
                .Concat(otherShips.Cast<Entity>())
                .Concat(cannonBalls.Cast<Entity>())
                .Select(x => Gameboard.Fields[x.Col, x.Row]);

        }

        private static List<Field> GetShotsInTheAir()
        {
            StartFunction("GetShotsInTheAir");
            List<Field> result = new List<Field>();

            foreach (var shot in Gameboard.GetCannonballs())
            {
                result.Add(Gameboard.Fields[shot.Col, shot.Row]);
            }
            StopFunction("GetShotsInTheAir", Program.LogLevel.TRACE);
            return result;
        }

        private static Field GetTarget(List<Field> takenShots, Ship closestEnemy, float distance)
        {
            StartFunction("GetTarget");
            int roundsTillImpact = (int)Math.Round(1 + distance / 3, 0);

            Field fieldWhereIsShipGonnaBe = GetDesination(closestEnemy, roundsTillImpact);

            while (takenShots.Contains(fieldWhereIsShipGonnaBe))
            {
                fieldWhereIsShipGonnaBe = fieldWhereIsShipGonnaBe.RandMutate();
            }
            takenShots.Add(fieldWhereIsShipGonnaBe);

            StopFunction("GetTarget", Program.LogLevel.TRACE);
            return fieldWhereIsShipGonnaBe;
        }

        private static Field GetDesination(Ship closestEnemy, int roundsTillImpact)
        {
            StartFunction("GetDesination");
            while (--roundsTillImpact >= 0)
            {
                closestEnemy = closestEnemy.GetShipAfterSimulatedMove();
            }

            StopFunction("GetDesination", Program.LogLevel.TRACE);
            return Gameboard.Fields[closestEnemy.Col, closestEnemy.Row];
        }
    }





    //USES odd-r coordinates
    public abstract class Entity
    {
        public enum EntityTypeEnum
        {
            SHIP,
            BARREL,
            MINE,
            CANNONBALL
        }
        public int EntityId { get; set; }
        public int Col { get; set; }
        public int Row { get; set; }
        public EntityTypeEnum EntityType { get; set; }

        protected Entity(string[] inputs)
        {
            EntityId = int.Parse(inputs[0]);
            string entityType = inputs[1];
            Col = int.Parse(inputs[2]);
            Row = int.Parse(inputs[3]);
            EntityType = (EntityTypeEnum)Enum.Parse(typeof(EntityTypeEnum), entityType);
        }

        protected Entity()
        {
        }

        public override string ToString()
        {
            return $"{Col} {Row}";
        }

        static public Entity Factory(string[] inputs)
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


        private static int cubeDistance(Tuple<int, int, int> a, Tuple<int, int, int> b)
        {
            return (Math.Abs(a.Item1 - b.Item1) + Math.Abs(a.Item2 - b.Item2) + Math.Abs(a.Item3 - b.Item3)) / 2;
        }
        public Tuple<int, int, int> GetCubeCoordinates()
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


        public int GetDistanceTo(Entity entity)
        {
            int result = cubeDistance(GetCubeCoordinates(), entity.GetCubeCoordinates());

            //Console.Error.WriteLine(result);
            return result;
        }

        public int GetDistanceTo(int col, int row)
        {
            var x = col - (row - (row & 1)) / 2;
            var z = row;
            var y = -x - z;
            return cubeDistance(GetCubeCoordinates(), new Tuple<int, int, int>(x, y, z));
        }
    }

    public class CannonBall : Entity
    {
        // the ship that shot this
        public int ShipEntityId { get; set; }

        public int ImpactInXTurns { get; set; }

        public Field AimedAtField { get; set; }

        public CannonBall(string[] inputs) : base(inputs)
        {
            int arg1 = int.Parse(inputs[4]);
            int arg2 = int.Parse(inputs[5]);
            ShipEntityId = arg1;
            ImpactInXTurns = arg2;
            AimedAtField = Gameboard.Fields[Col, Row];
        }
    }

    public class Mine : Entity
    {
        public Mine(string[] inputs) : base(inputs) { }
    }


    public class Ship : Entity
    {
        public Ship GetShipAfterSimulatedMove()
        {
            Ship result = (Ship)this.MemberwiseClone();
            int col = result.Col;
            int row = result.Row;
            if (Speed == 0)
            {
                return result;
            }
            if (Speed == 1)
            {
                if (row % 2 == 0)
                {
                    switch (result.CurrentRotation)
                    {
                        case Rotation.East: col++; break;
                        case Rotation.NorthEast: row--; break;
                        case Rotation.NorthWest: row--; col--; break;
                        case Rotation.SouthEast: row++; break;
                        case Rotation.SouthWest: row++; col--; break;
                        case Rotation.West: col--; break;
                    }
                }
                else
                {
                    switch (result.CurrentRotation)
                    {
                        case Rotation.East: col++; break;
                        case Rotation.NorthEast: col++; row--; break;
                        case Rotation.NorthWest: row--; break;
                        case Rotation.SouthEast: row++; col++; break;
                        case Rotation.SouthWest: row++; break;
                        case Rotation.West: col--; break;
                    }
                }

            }
            else if (Speed == 2)
            {
                switch (CurrentRotation)
                {
                    case Rotation.East: col += 2; break;
                    case Rotation.NorthEast: col += 1; row -= 2; break;
                    case Rotation.NorthWest: col -= 1; row -= 2; break;
                    case Rotation.West: col -= 2; break;
                    case Rotation.SouthWest: col -= 1; row += 2; break;
                    case Rotation.SouthEast: col += 1; row += 2; break;
                }
            }

            //sanitize result
            if (col < 0)
                col = 0;
            if (row < 0)
                row = 0;
            if (row > Gameboard.MAXHEIGHT)
                row = Gameboard.MAXHEIGHT;
            if (col > Gameboard.MAXWIDTH)
                col = Gameboard.MAXWIDTH;

            result.Col = col;
            result.Row = row;

            return result;
        }

        public Field GetFieldBehind()
        {
            int row = Row;
            int col = Col;


            switch (CurrentRotation)
            {
                case Rotation.East: col += 2; break;
                case Rotation.NorthEast: col += 1; row -= 2; break;
                case Rotation.NorthWest: col -= 1; row -= 2; break;
                case Rotation.West: col -= 2; break;
                case Rotation.SouthWest: col -= 1; row += 2; break;
                case Rotation.SouthEast: col += 1; row += 2; break;
            }

            //sanitize result
            if (col < 0)
                col = 0;
            if (row < 0)
                row = 0;
            if (row > Gameboard.MAXHEIGHT)
                row = Gameboard.MAXHEIGHT;
            if (col > Gameboard.MAXWIDTH)
                col = Gameboard.MAXWIDTH;

            return Gameboard.Fields[col, row];
        }

        public Field[] GetFieldsCovered()
        {
            Field[] fieldsCovered = new Field[3];

            #region PositionCalculation
            fieldsCovered[1] = Gameboard.Fields[Col, Row];
            int row = Row;
            int col = Col;
            bool isEvenLine = row % 2 == 0;

            switch (CurrentRotation)
            {
                case Rotation.East:
                    fieldsCovered[0] = Gameboard.Fields[col + 1, row];
                    fieldsCovered[2] = Gameboard.Fields[col > 0 ? col - 1 : col, row];
                    break;
                case Rotation.NorthEast:
                    if (isEvenLine)
                    {
                        fieldsCovered[0] = Gameboard.Fields[col, row > 0 ? row - 1 : row];
                        fieldsCovered[2] = Gameboard.Fields[col > 0 ? col - 1 : col, row + 1];
                    }
                    else
                    {
                        fieldsCovered[0] = Gameboard.Fields[col + 1, row > 0 ? row - 1 : row];
                        fieldsCovered[2] = Gameboard.Fields[col, row + 1];
                    }
                    break;
                case Rotation.NorthWest:
                    if (isEvenLine)
                    {
                        fieldsCovered[0] = Gameboard.Fields[col > 0 ? col - 1 : col, row > 0 ? row - 1 : row];
                        fieldsCovered[2] = Gameboard.Fields[col, row + 1];
                    }
                    else
                    {
                        fieldsCovered[0] = Gameboard.Fields[col, row > 0 ? row - 1 : row];
                        fieldsCovered[2] = Gameboard.Fields[col + 1, row + 1];
                    }
                    break;
                case Rotation.West:
                    fieldsCovered[0] = Gameboard.Fields[col > 0 ? col - 1 : col, row];
                    fieldsCovered[2] = Gameboard.Fields[col + 1, row];
                    break;
                case Rotation.SouthWest:
                    if (isEvenLine)
                    {
                        fieldsCovered[0] = Gameboard.Fields[col > 0 ? col - 1 : col, row + 1];
                        fieldsCovered[2] = Gameboard.Fields[col, row > 0 ? row - 1 : row];
                    }
                    else
                    {
                        fieldsCovered[0] = Gameboard.Fields[col, row + 1];
                        fieldsCovered[2] = Gameboard.Fields[col + 1, row > 0 ? row - 1 : row];
                    }
                    break;
                case Rotation.SouthEast:
                    if (isEvenLine)
                    {
                        fieldsCovered[0] = Gameboard.Fields[col, row + 1];
                        fieldsCovered[2] = Gameboard.Fields[col > 0 ? col - 1 : col, row > 0 ? row - 1 : row];
                    }
                    else
                    {
                        fieldsCovered[0] = Gameboard.Fields[col + 1, row + 1];
                        fieldsCovered[2] = Gameboard.Fields[col, row > 0 ? row - 1 : row];
                    }
                    break;
                default: throw new NotImplementedException("Invalid Rotation");
            }

            #endregion PositionCalculation

            return fieldsCovered;
        }

        public Rotation CurrentRotation { get; private set; }

        public int Speed { get; private set; }

        public bool HasFiredLastRound { get; set; }

        public int StockOfRum { get; private set; }

        public bool IsMyShip { get; private set; }

        public Ship(string[] inputs) : base(inputs)
        {
            int arg1 = int.Parse(inputs[4]);
            int arg2 = int.Parse(inputs[5]);
            int arg3 = int.Parse(inputs[6]);
            int arg4 = int.Parse(inputs[7]);
            CurrentRotation = (Rotation)arg1;
            Speed = arg2;
            StockOfRum = arg3;
            IsMyShip = arg4 == 1 ? true : false;
            HasFiredLastRound = true; //in the first round we want to move so that we are not sitting ducks
        }

        //TODO
        public Ship(int col, int row, int speed, Rotation randDirection) : base()
        {
            Col = col;
            Row = row;
            this.Speed = speed;
            this.CurrentRotation = randDirection;
        }

        public enum Rotation
        {
            East = 0,
            NorthEast = 1,
            NorthWest = 2,
            West = 3,
            SouthWest = 4,
            SouthEast = 5
        }
    }

    public class Barrel : Entity
    {
        public int AmountOfRum { get; set; }

        public Barrel(string[] inputs) : base(inputs)
        {
            int arg1 = int.Parse(inputs[4]);
            AmountOfRum = arg1;
        }
    }

    public class Field
    {
        private Random rndGen = new Random();
        public int Col { get; set; }
        public int Row { get; set; }
        public Entity EntityOnField { get; set; }

        public int Cost { get; set; }
        
        public Field Parent { get; set; }


        //TODO
        public Field RandMutate()
        {
            Field result = (Field)this.MemberwiseClone();
            var randDirection = (Ship.Rotation)rndGen.Next(Enum.GetNames(typeof(Ship.Rotation)).Length);
            var ship = new Ship(Col, Row, 1, randDirection);
            ship = ship.GetShipAfterSimulatedMove();
            result.Col = ship.Col;
            result.Row = ship.Row;
            return result;
        }

        public IEnumerable<Field> GetNeighbours()
        {
            List<Field> neighbours = new List<Field>();
            int col = Col;
            int row = Row;
            if (row % 2 == 0)
            {
                safeAdd(neighbours, col - 1, row - 1);
                safeAdd(neighbours, col - 1, row);
                safeAdd(neighbours, col - 1, row + 1);
                safeAdd(neighbours, col, row + 1);
                safeAdd(neighbours, col + 1, row);
                safeAdd(neighbours, col, row - 1);
            }
            else
            {
                safeAdd(neighbours, col, row - 1);
                safeAdd(neighbours, col - 1, row);
                safeAdd(neighbours, col, row + 1);
                safeAdd(neighbours, col + 1, row + 1);
                safeAdd(neighbours, col + 1, row);
                safeAdd(neighbours, col + 1, row - 1);
            }

            return neighbours;
        }

        public int GetDistanceTo(Field field)
        {
            int result = cubeDistance(GetCubeCoordinates(), field.GetCubeCoordinates());

            //Console.Error.WriteLine(result);
            return result;
        }

        private static int cubeDistance(Tuple<int, int, int> a, Tuple<int, int, int> b)
        {
            return (Math.Abs(a.Item1 - b.Item1) + Math.Abs(a.Item2 - b.Item2) + Math.Abs(a.Item3 - b.Item3)) / 2;
        }
        public Tuple<int, int, int> GetCubeCoordinates()
        {
            var x = Col - (Row - (Row & 1)) / 2;
            var z = Row;
            var y = -x - z;
            return new Tuple<int, int, int>(x, y, z);
        }

        private void safeAdd(List<Field> neighbours, int col, int row)
        {
            if (col >= 0 && row >= 0 && col <= Gameboard.MAXWIDTH && row <= Gameboard.MAXHEIGHT)
                neighbours.Add(Gameboard.Fields[col, row]);
        }

        public override string ToString()
        {
            return $"{Col} {Row}";
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Field objAsField = obj as Field;
            if (objAsField == null) return false;
            else return Equals(objAsField);
        }
        public override int GetHashCode()
        {
            return (Col << 8) + Row;
        }
        public bool Equals(Field other)
        {
            if (other == null) return false;
            return this.Col == other.Col && this.Row == other.Row;
        }

        internal Field CloneLocation()
        {
            var b = new Field();
            b.Row = this.Row;
            b.Col = this.Col;
            return b;
        }
    }

    static class Gameboard
    {
        public const int MAXHEIGHT = 20;
        public const int MAXWIDTH = 22;


        static public Field[,] Fields;

        private static List<Entity> _allEntities = new List<Entity>();
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

        public static IEnumerable<CannonBall> GetCannonballs()
        {
            return _allEntities.Where(x => x.EntityType == Entity.EntityTypeEnum.CANNONBALL).Select(x => (CannonBall)x);
        }

        public static void UpdateBoard(int entityCount)
        {
            Dictionary<int, Ship> oldShips = getShipDictionary();

            Program.StartFunction("Gameboard.Update");
            _allEntities.Clear();
            for (int row = 0; row <= MAXHEIGHT; row++)
            {
                for (int col = 0; col <= MAXWIDTH; col++)
                {
                    Fields[col, row].Cost = 100;
                }
            }

            for (int i = 0; i < entityCount; i++)
            {
                string[] inputs = Console.ReadLine().Split(' ');
                var entity = Entity.Factory(inputs);
                Fields[entity.Col, entity.Row].EntityOnField = entity;

                switch (entity.EntityType)
                {
                    case Entity.EntityTypeEnum.BARREL:
                        Fields[entity.Col, entity.Row].Cost -= (entity as Barrel).AmountOfRum;
                        break;
                    case Entity.EntityTypeEnum.CANNONBALL:
                        Fields[entity.Col, entity.Row].Cost += 1000;
                        break;
                    case Entity.EntityTypeEnum.MINE:
                        Fields[entity.Col, entity.Row].Cost += 1000;
                        break;
                    case Entity.EntityTypeEnum.SHIP:
                        if (!(entity as Ship).IsMyShip)
                        {
                            //ENEMY
                            Fields[entity.Col, entity.Row].Cost += 10000;
                        }
                        else
                        {
                            //MY SHIP - lower score than enemy cause we don't want to block us
                            Fields[entity.Col, entity.Row].Cost += 10000;

                            //Set previous values
                            if (oldShips.ContainsKey(entity.EntityId))
                                (entity as Ship).HasFiredLastRound = oldShips[entity.EntityId].HasFiredLastRound;
                        }
                        break;
                }

                _allEntities.Add(entity);
            }

            Program.StopFunction("Gameboard.Update", Program.LogLevel.TRACE);
        }

        public static IEnumerable<Ship> GetPlayerShips()
        {
            return _allEntities.Where(x => x.EntityType == Entity.EntityTypeEnum.SHIP && (x as Ship).IsMyShip).Select(x => (Ship)x);
        }

        private static Dictionary<int, Ship> getShipDictionary()
        {
            return GetPlayerShips().ToDictionary(x => x.EntityId, x => x);
        }

        public static IEnumerable<Ship> GetEnemyShipsByDistance(Ship playerShip)
        {
            return _allEntities
                       .Where(x => x.EntityType == Entity.EntityTypeEnum.SHIP && (x as Ship).IsMyShip == false)
                       .OrderBy(x => playerShip.GetDistanceTo(x))
                       .Select(x => (Ship)x);
        }

        public static IEnumerable<Mine> GetMines()
        {
            return _allEntities.Where(x => x.EntityType == Entity.EntityTypeEnum.MINE).Select(x => (Mine)x);
        }
        public static IEnumerable<Ship> GetShips()
        {
            return _allEntities.Where(x => x.EntityType == Entity.EntityTypeEnum.SHIP).Select(x => (Ship)x);
        }

        public static IEnumerable<Barrel> GetBarrels()
        {
            return _allEntities.Where(x => x.EntityType == Entity.EntityTypeEnum.BARREL).Select(x => (Barrel)x);
        }


        //public static Field[] AStar(Field start, Field goal)
        //{
        //    var frontier = new PriorityQueue<Field>();
        //    frontier.Enqueue(start, 0);
        //    var came_from = new Dictionary<Field, Field>();
        //    var cost_so_far = new Dictionary<Field, int>();
        //    came_from[start] = null;
        //    cost_so_far[start] = 0;

        //    while (frontier.Count != 0)
        //    {
        //        Program.Log("TEST", Program.LogLevel.WARN);
        //        var current = (Field)frontier.Dequeue();

        //        if (current == goal)
        //            break;

        //        foreach (var next in current.GetNeighbours())
        //        {
        //            var new_cost = cost_so_far[current] + next.Cost;
        //            if (!cost_so_far.ContainsKey(next) || new_cost < cost_so_far[next])
        //            {
        //                cost_so_far[next] = new_cost;
        //                int priority = new_cost;
        //                frontier.Enqueue(next, priority);
        //                came_from[next] = current;
        //            }
        //        }
        //    }
        //    return came_from.Keys.ToArray();
        //}

    }

    public class AStarSearch
    {
        public Dictionary<Field, Field> cameFrom
            = new Dictionary<Field, Field>();
        public Dictionary<Field, double> costSoFar
            = new Dictionary<Field, double>();

        public ArrayList Path = new ArrayList();

        // Note: a generic version of A* would abstract over Location and
        // also Heuristic
        static public double Heuristic(Field a, Field b)
        {
            return a.GetDistanceTo(b);
        }

        public AStarSearch(Field start, Field goal)
        {
            var frontier = new PriorityQueue<Field>();
            frontier.Enqueue(start, 0);

            cameFrom[start] = null;
            costSoFar[start] = 0;
            Path.Add(start);

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current.Equals(goal))
                {
                    break;
                }

                foreach (var next in current.GetNeighbours())
                {
                    double newCost = costSoFar[current] + next.Cost;

                    if (!costSoFar.ContainsKey(next)
                        || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        double priority = Heuristic(next, goal);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;                        
                    }
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



}


