using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LoCaM.Config;

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
