using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoCaM
{

    /**
     * Auto-generated code below aims at helping you parse
     * the standard input according to the problem statement.
     **/
    class Game
    {
        /// <summary>
        /// Guard Picklist
        /// </summary>
        //static int[] Picklist = { 49, 40, 99, 108, 128, 97, 106, 103, 53, 105, 101, 104, 113, 100, 121, 112, 48, 102, 107, 139, 115, 74, 118, 137, 151, 67, 131, 110, 133, 142, 129, 116, 152, 126, 148, 62, 111, 63, 80, 114, 55, 92, 91, 143, 98, 94, 93, 95, 96, 87, 64, 13, 1, 2, 3, 4, 5, 6, 7, 8, 9, 56, 10, 11, 12, 122, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 41, 42, 43, 44, 45, 46, 47, 50, 51, 52, 54, 57, 58, 59, 60, 61, 65, 66, 68, 69, 70, 71, 72, 73, 75, 76, 77, 78, 79, 81, 82, 83, 84, 85, 86, 88, 89, 90, 109, 117, 119, 120, 123, 124, 125, 127, 130, 132, 134, 135, 136, 140, 138, 141, 144, 145, 146, 147, 149, 150, 153, 154, 155, 156, 157, 158, 159, 160 };
        enum GamePhases
        {
            Draft,
            Battle
        }
              
        static int TurnCounter { get; set; }

        static GamePhases GamePhase { get; set; }

        static BoardState RealBoardState { get; set; }

        static Game()
        {
#if STUDIO
            TurnCounter = 32;
#else
            TurnCounter = 0;
#endif
            GamePhase = GamePhases.Draft;
        }

        static void Main(string[] args)
        {
            // game loop
            while (true)
            {
                RealBoardState = BoardState.ParseInputs();

                if(TurnCounter++ >= 30)
                {
                    GamePhase = GamePhases.Battle;
                }

                switch (GamePhase)
                {
                    case GamePhases.Draft: DoDraft(); break;
                    case GamePhases.Battle: DoBattle(); break;
                }
            }
        }

        private static void DoBattle()
        {
            BoardState initialState = RealBoardState.Clone();

            var mobsWithCharge = initialState.PlayerHandCards
                                            .Where(card => card.CardType == Card.CardTypes.Creature
                                                            && card.Cost <= initialState.PlayerMana
                                                            && (card.CardAbilities.Contains(Card.Abilities.Charge)));

            var mutations = new List<BoardState>();
            mutations.Add(initialState);
            List<int> excludeList = new List<int>();
            mutations.AddRange(initialState.GetSummonMutations(mobsWithCharge, excludeList, Card.Abilities.Charge));
            
            List<BoardState> mutations2 = new List<BoardState>();
            mutations2.AddRange(mutations);
            foreach (var mutation in mutations)
            {
                var itemCards = mutation.PlayerHandCards
                                                .Where(card => card.CardType != Card.CardTypes.Creature
                                                                && card.Cost <= mutation.PlayerMana);

                excludeList.Clear();
                mutations2.AddRange(mutation.GetUseItemMutations(itemCards, excludeList));              
            }
            Console.Error.WriteLine($"Mutations2: {mutations2.Count()}");

            List<BoardState> mutations3 = new List<BoardState>();
            mutations3.AddRange(mutations2);
            foreach(var mutation in mutations2)
            {
                var possibleAttackers = mutation.PlayerBoardCards.Where(x => x.CanAttack);
                mutations3.AddRange(mutation.GetAttackMutations(possibleAttackers));
            }
            Console.Error.WriteLine($"Mutations3: {mutations3.Count()}");

            var bestAttacks = mutations3.OrderByDescending(x => x.GetScore());
            

            List<BoardState> mutations4 = new List<BoardState>();
            foreach (var mutation in mutations3)
            {
                mutations4.Add(mutation);

                var mobs = mutation.PlayerHandCards
                                               .Where(card => card.CardType == Card.CardTypes.Creature
                                                               && card.Cost <= initialState.PlayerMana);
                excludeList.Clear();
                mutations4.AddRange(mutation.GetSummonMutations(mobs, excludeList));
            }

            var best = mutations4.OrderByDescending(x => x.GetScore()).First();
            Console.Error.WriteLine($"Found {mutations4.Count()} mutation, picked best with {best.GetScore()}");
#if STUDIO
            foreach (var mutation in mutations4.Take(200))
                Console.Error.WriteLine(mutation.ActionString);
#endif

            if (String.IsNullOrWhiteSpace(best.ActionString))
            {
                Console.Error.WriteLine("WARNING: FOUND NO ACTION");
                Console.WriteLine("PASS");
                return;
            }
            Console.WriteLine(best.ActionString);
            //test
        }


        private static void DoDraft()
        {
            var indexToCard = RealBoardState.PlayerHandCards.Select((card, index) => new { card, index }).OrderByDescending(x => x.card.GetDraftScore()).First();
            foreach (var card in RealBoardState.PlayerHandCards)
                Console.Error.WriteLine($"{card.InstanceId}: {card.GetDraftScore()}");

            Console.WriteLine($"PICK {indexToCard.index}");
        }

    }
}
