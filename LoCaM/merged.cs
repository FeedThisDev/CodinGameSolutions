using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LoCaM.Config;
using static LoCaM.Card;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LoCaM
{
    public class BoardState : ITarget
    {
        public string ActionString { get; private set; }
        public int OpponentHealth { get; private set; }
        public int PlayerHealth { get; private set; }

        public int AmountOfHandCards { get; private set; }
        public int AmountOfDeckCards { get; private set; }
        public List<Card> PlayerHandCards { get; private set; }

        public List<Card> PlayerBoardCards { get; private set; }

        public List<Card> OpponentBoardCards { get; private set; }
        public int PlayerMana { get; private set; }

        static int[] specialCards = { 153, 154, 156, 160 };


        public BoardState Clone()
        {
            BoardState clone = new BoardState()
            {
                AmountOfHandCards = this.AmountOfHandCards,
                AmountOfDeckCards = this.AmountOfDeckCards,
                OpponentBoardCards = this.OpponentBoardCards.Clone(),
                OpponentHealth = this.OpponentHealth,
                PlayerBoardCards = this.PlayerBoardCards.Clone(),
                PlayerHandCards = this.PlayerHandCards.Clone(),
                PlayerHealth = this.PlayerHealth,
                PlayerMana = this.PlayerMana,
                ActionString = (string)this.ActionString.Clone()
            };
            return clone;
        }

        private BoardState()
        {
            PlayerBoardCards = new List<Card>();
            OpponentBoardCards = new List<Card>();
            PlayerHandCards = new List<Card>();
            ActionString = String.Empty;
        }

        public static BoardState ParseInputs()
        {
            BoardState boardState = new BoardState();
            string[] inputs;
            for (int i = 0; i < 2; i++)
            {
#if STUDIO
                inputs = "30 7 30 5 1".Split(' ');
#else
                inputs = Console.ReadLine().Split(' ');
#endif
                if (i == 0)
                {
                    boardState.PlayerHealth = int.Parse(inputs[0]);
                    boardState.PlayerMana = int.Parse(inputs[1]);
                    boardState.AmountOfDeckCards = int.Parse(inputs[2]);
                    int rune = int.Parse(inputs[3]);
                    int playerDraw = int.Parse(inputs[4]);

                }
                else
                {
                    boardState.OpponentHealth = int.Parse(inputs[0]);
                }
            }
#if STUDIO
            inputs = "30 1".Split(' ');
#else
            inputs = Console.ReadLine().Split(' ');
#endif

            int opponentHand = int.Parse(inputs[0]);
            int opponentActions = int.Parse(inputs[1]);
            for (int i = 0; i < opponentActions; i++)
            {
#if STUDIO
#else
                string cardNumberAndAction = Console.ReadLine();
#endif
            }

#if STUDIO

            AddCreature(boardState.PlayerHandCards);
            AddCreature(boardState.PlayerHandCards);
            //AddItem(boardState.PlayerHandCards);

            AddCreature(boardState.PlayerBoardCards);
            AddCreature(boardState.PlayerBoardCards);
            AddCreature(boardState.PlayerBoardCards);
            AddCreature(boardState.PlayerBoardCards);

            AddStrongCreature(boardState.OpponentBoardCards);
            AddStrongCreature(boardState.OpponentBoardCards);
            //AddCreature(boardState.OpponentBoardCards);
            //AddCreature(boardState.OpponentBoardCards);
            //AddCreature(boardState.OpponentBoardCards);
            //AddCreature(boardState.OpponentBoardCards);

#else
            int cardCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < cardCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');

                Card card = new Card(inputs);

                switch (card.Location)
                {
                    case Card.Locations.OppononentsBoardSide:
                        boardState.OpponentBoardCards.Add(card);
                        break;
                    case Card.Locations.PlayerBoardSide:
                        boardState.PlayerBoardCards.Add(card);
                        break;
                    case Card.Locations.PlayerHand:
                        boardState.PlayerHandCards.Add(card);
                        break;
                    default: throw new NotImplementedException("invalid card location ");
                }
            }            
#endif

            return boardState;
        }

#if STUDIO
        private static void AddItem(List<Card> destination)
        {
            var inputs = (++instanceCounter + " " + instanceCounter + " 0 2 3 0 0 WCDGBL 0 0 1").Split(' ');
            var card = new Card(inputs);
            destination.Add(card);
        }

        static int instanceCounter = 0;
        private static void AddCreature(List<Card> destination)
        {
            var inputs = (++instanceCounter + " " + instanceCounter + " 1 0 3 4 4 - 0 0 0").Split(' ');
            var card = new Card(inputs);
            destination.Add(card);
        }

        private static void AddStrongCreature(List<Card> destination)
        {
            var inputs = (++instanceCounter + " " + instanceCounter + " 1 0 4 9 9 - 0 0 0").Split(' ');
            var card = new Card(inputs);

            destination.Add(card);
        }


#endif
        internal IEnumerable<BoardState> GetAttackMutations(IEnumerable<Card> possibleAttackers)
        {
            List<BoardState> result = new List<BoardState>();

            foreach (var card in possibleAttackers)
            {
                var mutations = this.SimAttack(card);
                if (mutations == null || mutations.Count() == 0)
                    continue;

                result.AddRange(mutations);

                foreach (var mutation in mutations)
                {
                    var otherPossibleAttackers = mutation.PlayerBoardCards
                                                 .Where(creature => creature.CanAttack
                                                                   && creature.InstanceId != card.InstanceId);

                    result.AddRange(mutation.GetAttackMutations(otherPossibleAttackers));
                }
            }

            return result;
        }

        internal double GetScore()
        {
            double positiveScore = PlayerBoardCards.Sum(x => x.GetScore());
            positiveScore += PlayerHealth * Config.PlayerLifeMultiplier;
            positiveScore += PlayerBoardCards.Count * Config.HandCardsMultiplier + AmountOfHandCards * Config.HandCardsMultiplier;

            if (OpponentHealth <= 0)
                positiveScore += Config.GameWon;

            if (PlayerBoardCards.Count > OpponentBoardCards.Count)
                positiveScore += Config.MoreCreaturesThanEnemyMultiplier;

            if (OpponentBoardCards.Count == 0)
                positiveScore += Config.BoardClearedBonus;

            double negativeScore = OpponentBoardCards.Sum(x => x.GetScore());
            negativeScore += OpponentHealth * Config.OpponentLifeMultiplier;

            return positiveScore - negativeScore;
        }

        private IEnumerable<BoardState> SimAttack(Card card)
        {
            List<BoardState> boardStates = new List<BoardState>();

            foreach (var target in GetTargets(card))
            {
                var newMutation = SimAttack(card, target);
                if (newMutation != null)
                    boardStates.Add(newMutation);
            }

            return boardStates;
        }

        private BoardState SimAttack(Card cardReference, ITarget iTarget)
        {
            var clone = this.Clone();

            Card card = clone.GetCard(cardReference.InstanceId);
            card.HasAttacked = true;

            if (iTarget.GetInstanceID() == -1)
            {
                clone.OpponentHealth -= cardReference.Attack;
                clone.ActionString += $"ATTACK {cardReference.InstanceId} -1;";
                if (cardReference.CardAbilities.Contains(Card.Abilities.Drain))
                    clone.PlayerHealth += cardReference.Attack;
                return clone;
            }

            Card target = clone.GetCard(iTarget.GetInstanceID());

            if (card.CardAbilities.Contains(Card.Abilities.Ward))
            {
                if (target.Attack > 0)
                {
                    card.CardAbilities.Remove(Card.Abilities.Ward);
                }

                if (target.CardAbilities.Contains(Card.Abilities.Ward))
                {
                    target.CardAbilities.Remove(Card.Abilities.Ward);
                }
                else
                {
                    //target doesn't have ward, I had
                    if (card.CardAbilities.Contains(Card.Abilities.Drain))
                    {
                        int dmg = Math.Max(card.Attack, target.Defense);
                        clone.PlayerHealth += dmg;
                    }

                    target.Defense -= card.Attack;

                    if (card.CardAbilities.Contains(Card.Abilities.Lethal))
                        target.Defense = -1;
                }
            }
            else if (target.CardAbilities.Contains(Card.Abilities.Ward))
            {
                //target had wand, attacker didn't
                target.CardAbilities.Remove(Card.Abilities.Ward);

                if (target.CardAbilities.Contains(Card.Abilities.Drain))
                {
                    int dmgDealt = Math.Max(target.Attack, card.Defense);
                    clone.OpponentHealth += dmgDealt;
                }

                if (target.CardAbilities.Contains(Card.Abilities.Lethal))
                    card.Defense = -1;

                card.Defense -= target.Attack;
            }
            else
            {
                //target doesn't have ward, me neither
                if (target.CardAbilities.Contains(Card.Abilities.Lethal))
                {
                    card.Defense = -1;
                }
                if (card.CardAbilities.Contains(Card.Abilities.Lethal))
                {
                    target.Defense = -1;
                }
                if (card.CardAbilities.Contains(Card.Abilities.Breakthrough) && card.Attack > target.Defense)
                {
                    clone.OpponentHealth -= (card.Attack - target.Defense);
                }
                if (card.CardAbilities.Contains(Card.Abilities.Drain))
                {
                    int dmg = Math.Max(target.Defense, card.Attack);
                    clone.PlayerHealth += dmg;
                }
                if (target.CardAbilities.Contains(Card.Abilities.Drain))
                {
                    int dmg = Math.Max(target.Attack, card.Defense);
                    clone.OpponentHealth += dmg;
                }

                target.Defense -= card.Attack;
                card.Defense -= target.Attack;
            }

            if (card.Defense <= 0)
                clone.PlayerBoardCards.Remove(card);
            if (target.Defense <= 0)
                clone.OpponentBoardCards.Remove(target);

            clone.ActionString += $"ATTACK {card.InstanceId} {target.InstanceId};";
            return clone;
        }

        internal IEnumerable<BoardState> GetUseItemMutations(IEnumerable<Card> itemCards, List<int> excludeList)
        {
            List<BoardState> result = new List<BoardState>();

            foreach (var card in itemCards)
            {
                var mutations = this.SimUse(card);
                if (mutations == null || mutations.Count == 0)
                    continue;
                excludeList.Add(card.InstanceId);
                result.AddRange(mutations);

                foreach (var mutation in mutations)
                {
                    var otherItemCards = mutation.PlayerHandCards
                                                 .Where(item => item.CardType != Card.CardTypes.Creature
                                                            && excludeList.Contains(item.InstanceId)  ////unnesseccary
                                                            && item.Cost <= mutation.PlayerMana);
                    result.AddRange(mutation.GetUseItemMutations(otherItemCards, excludeList));
                }
            }

            return result;
        }

        internal IEnumerable<BoardState> GetSummonMutations(IEnumerable<Card> mobs, List<int> excludeList, Card.Abilities? extraAbility = null)
        {
            List<BoardState> result = new List<BoardState>();

            foreach (var card in mobs)
            {
                var mutation = this.SimSummon(card);
                if (mutation == null)
                    continue;
                result.Add(mutation);
                excludeList.Add(card.InstanceId);

                var othermobs = mutation.PlayerHandCards
                               .Where(x => x.CardType == Card.CardTypes.Creature
                                               && x.Cost <= mutation.PlayerMana
                                               && !excludeList.Contains(x.InstanceId)
                                               );
                if (extraAbility.HasValue)
                    othermobs = othermobs.Where(x => x.CardAbilities.Contains(extraAbility.Value));

                result.AddRange(mutation.GetSummonMutations(othermobs, excludeList, extraAbility));
            }

            return result;
        }

        internal List<BoardState> SimUse(Card card)
        {
            List<BoardState> boardStates = new List<BoardState>();

            foreach (var target in GetTargets(card))
            {
                var newMutation = SimUse(card, target);
                if (newMutation != null)
                    boardStates.Add(newMutation);
            }

            return boardStates;
        }

        private BoardState SimUse(Card card, ITarget iTarget)
        {
            var clone = CommonAction(card);
            if (clone == null)
                return null;

            if (iTarget.GetInstanceID() == -1)
            {
                clone.OpponentHealth += card.Defense;
                clone.ActionString += $"USE {card.InstanceId} -1;";
                return clone;
            }
            Card target;

            target = clone.GetCard(iTarget.GetInstanceID());
            target.Attack += card.Attack;
            target.Defense += card.Defense;

            if (card.CardType == Card.CardTypes.BlueItem || card.CardType == Card.CardTypes.RedItem)
            {
                foreach (var ability in card.CardAbilities)
                {
                    if (target.CardAbilities.Contains(ability))
                        target.CardAbilities.Remove(ability);
                }
            }
            else if (card.CardType == Card.CardTypes.GreenItem)
            {
                foreach (var ability in card.CardAbilities)
                {
                    if (!target.CardAbilities.Contains(ability))
                        target.CardAbilities.Add(ability);
                }
            }
            else
            {
                throw new InvalidOperationException("Can't use creature as item");
            }

            if (target.Defense <= 0)
                OpponentBoardCards.Remove(target);

            clone.ActionString += $"USE {card.InstanceId} {target.InstanceId};";
            return clone;
        }

        private Card GetCard(int instanceID)
        {
            var card = OpponentBoardCards.SingleOrDefault(x => x.InstanceId == instanceID);
            if (card != null)
                return card;

            card = PlayerBoardCards.SingleOrDefault(x => x.InstanceId == instanceID);
            if (card != null)
                return card;

            throw new KeyNotFoundException();
        }

        public BoardState SimSummon(Card card)
        {

            if (card.CardType != Card.CardTypes.Creature)
                return null; // throw new InvalidOperationException("can't summon a spell");

            if (PlayerBoardCards.Count > 5)
                return null;//throw new InvalidOperationException("no space on board to summon creature");

            var clone = CommonAction(card);
            if (clone == null)
                return null;

            clone.PlayerBoardCards.Add(card);
            clone.ActionString += $"SUMMON {card.InstanceId};";
            return clone;
        }

        private BoardState CommonAction(Card card)
        {
            if (card.Cost > PlayerMana)
                return null;//throw new InvalidOperationException("not enough mana");

            BoardState clone = this.Clone();
            //this card is played so reduce amount of cards
            clone.AmountOfHandCards--;

            //draw card effect of card
            for (int i = 0; i < card.CardDraw && clone.AmountOfDeckCards > 0; i++)
            {
                clone.AmountOfDeckCards--;
                clone.AmountOfHandCards++;
                if (clone.AmountOfHandCards > 8)
                    clone.AmountOfHandCards = 8;
            }

            clone.PlayerHealth += card.MyHealthChange;
            clone.OpponentHealth += card.OpponentHealthChange;
            clone.PlayerMana -= card.Cost;
            clone.PlayerHandCards.Remove(card);

            return clone;
        }

        private IEnumerable<ITarget> GetTargets(Card card)
        {
            switch (card.CardType)
            {
                case Card.CardTypes.Creature:
                    var guards = OpponentBoardCards.Where(x => x.CardAbilities.Contains(Card.Abilities.Guard)).Select(x => (ITarget)x);
                    if (guards != null && guards.Count() > 0)
                        return guards;
                    var targets = OpponentBoardCards.Select(x => (ITarget)x).ToList();
                    targets.Add(this);
                    return targets;

                case Card.CardTypes.GreenItem:
                    return PlayerBoardCards.Select(x => (ITarget)x);

                case Card.CardTypes.RedItem:
                    return OpponentBoardCards.Select(x => (ITarget)x);

                case Card.CardTypes.BlueItem:
                    List<ITarget> result = new List<ITarget>();
                    result.Add(this);
                    if (!specialCards.Contains(card.CardNumber)) //no creature as target
                    {
                        result.AddRange(OpponentBoardCards.Select(x => (ITarget)x));
                    }
                    return result;

                default: throw new NotImplementedException($"Card Type  {card.CardType} not implemented");
            }
        }

        public int GetInstanceID()
        {
            return -1;
        }

        //public BoardState ApplyCardEffect(Card cardToApply)
        //{
        //    BoardState clone = this.Clone();
        //    clone.OpponentHealth -= cardToApply.OpponentHealthChange;
        //    clone.OpponentHealth -= cardToApply.Attack;
        //    return clone;
        //}
    }
}

namespace LoCaM
{
    public class Card : IEquatable<Card>, ITarget
    {
        public enum CardTypes
        {
            Creature = 0,
            GreenItem = 1,
            RedItem = 2,
            BlueItem = 3
        }

        public enum Locations
        {
            PlayerHand = 0,
            PlayerBoardSide = 1,
            OppononentsBoardSide = -1
        }

        public enum Abilities
        {
            Breakthrough,
            Charge,
            Guard,
            Ward,
            Lethal,
            Drain
        }

        public int CardNumber { get; private set; }
        public int InstanceId { get; private set; }
        public Locations Location { get; private set; }
        public CardTypes CardType { get; private set; }
        public int Cost { get; private set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public List<Abilities> CardAbilities { get; private set; }
        public int MyHealthChange { get; private set; }
        public int OpponentHealthChange { get; private set; }
        public int CardDraw { get; private set; }

        private Card() { }

        public Card(string[] inputs)
        {
            CardNumber = int.Parse(inputs[0]);
            InstanceId = int.Parse(inputs[1]);
            Location = (Locations)Enum.Parse(typeof(Locations), inputs[2]);
            CardType = (CardTypes)int.Parse(inputs[3]);
            Cost = int.Parse(inputs[4]);
            Attack = int.Parse(inputs[5]);
            Defense = int.Parse(inputs[6]);
            ParseAbilities(inputs[7]);
            MyHealthChange = int.Parse(inputs[8]);
            OpponentHealthChange = int.Parse(inputs[9]);
            CardDraw = int.Parse(inputs[10]);
        }

        private void ParseAbilities(string abilityString)
        {
            CardAbilities = new List<Abilities>();
            if (abilityString.Contains("B"))
                CardAbilities.Add(Abilities.Breakthrough);
            if (abilityString.Contains("C"))
                CardAbilities.Add(Abilities.Charge);
            if (abilityString.Contains("G"))
                CardAbilities.Add(Abilities.Guard);
            if (abilityString.Contains("L"))
                CardAbilities.Add(Abilities.Lethal);
            if (abilityString.Contains("W"))
                CardAbilities.Add(Abilities.Ward);
            if (abilityString.Contains("D"))
                CardAbilities.Add(Abilities.Drain);
        }

        public override bool Equals(object obj)
        {
            return obj is Card card &&
                   InstanceId == card.InstanceId;
        }

        public bool Equals(Card other)
        {
            return InstanceId == other.InstanceId;
        }

        public override int GetHashCode()
        {
            return -676353417 + InstanceId.GetHashCode();
        }

        public double GetScore()
        {
            return (AttackMultiplier * Attack
                + DefenseMultiplier * Defense
                + SkillMultiplier * CardAbilities.Count
                + OpponentLifeMultiplier * OpponentHealthChange
                + PlayerLifeMultiplier * MyHealthChange
                + CardDraw * HandCardsMultiplier
                );
        }

        public double GetDraftScore()
        {
            return (AttackMultiplier * Attack
                + DefenseMultiplier * Defense
                + SkillMultiplier * CardAbilities.Count
                + OpponentLifeMultiplier * OpponentHealthChange
                + PlayerLifeMultiplier * MyHealthChange
                + CardDraw * HandCardsMultiplier
                ) / ((Cost == 0 ? 0.9 : Cost ) * ManaCostMultiplier);
        }

        public Card Clone()
        {
            Card clone = new Card()
            {
                Attack = this.Attack,
                CardAbilities = this.CardAbilities.Clone(),
                CardDraw = this.CardDraw,
                CardNumber = this.CardNumber,
                CardType = this.CardType,
                Cost = this.Cost,
                Defense = this.Defense,
                InstanceId = this.InstanceId,
                Location = this.Location,
                MyHealthChange = this.MyHealthChange,
                OpponentHealthChange = this.OpponentHealthChange,
                HasAttacked = this.HasAttacked
            };
            return clone;
        }

        public int GetInstanceID()
        {
            return this.InstanceId;
        }

        public bool HasAttacked = false;

        public bool CanAttack
        {
            get
            {
                if (Attack <= 0)
                    return false;

                if (HasAttacked)
                    return false;

                if (Location == Locations.PlayerBoardSide)
                    return true;
                if (Location == Locations.PlayerHand && CardAbilities.Contains(Abilities.Charge))
                    return true;
                return false;
            }
        }

    }
}

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

namespace LoCaM
{
    public static class Externsions
    {
        public static List<Card> Clone(this List<Card> list)
        {
            List<Card> clone = new List<Card>();
            foreach(var card in list)
            {
                clone.Add(card.Clone());
            }
            return clone;
        }

        public static IEnumerable<Card> Clone(this IEnumerable<Card> list)
        {
            List<Card> clone = new List<Card>();
            foreach (var card in list)
            {
                clone.Add(card.Clone());
            }
            return clone;
        }

        public static List<Abilities> Clone(this List<Abilities> list)
        {
            List<Abilities> clone = new List<Abilities>();

            foreach(var ability in list)
            {
                clone.Add(ability);
            }

            return clone;
        }

    }
}

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

namespace LoCaM
{
    interface ITarget
    {
        int GetInstanceID();
    }
}

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.

// The following GUID is for the ID of the typelib if this project is exposed to COM

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
