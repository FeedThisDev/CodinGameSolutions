using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LoCaM.Card;

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
