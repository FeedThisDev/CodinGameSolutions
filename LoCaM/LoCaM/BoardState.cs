using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
