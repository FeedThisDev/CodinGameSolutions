using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoCaM
{
    public class Config
    {
        //Card Score
        public const int AttackMultiplier = 10;
        public const int DefenseMultiplier = 10;
        public const int SkillMultiplier = 10;
        public const int OpponentLifeMultiplier = 10;
        public const int PlayerLifeMultiplier = 10;
        public const int HandCardsMultiplier = 15;
        public const int ManaCostMultiplier = 10; 

        //Board Score
        public const int MoreCreaturesThanEnemyMultiplier = 10;
        public const int BoardClearedBonus = 50;
        public const int GameWon = 10000000;
        public const int ImDeadInOneRound = 50;
    }
}
