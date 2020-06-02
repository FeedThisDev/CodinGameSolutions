using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CodeOfKutulu.Gameboard;

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
