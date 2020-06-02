using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOfKutulu
{

    public abstract class Effect : Entity
    {
        public int TimeToFadeOut { get; private set; }
        public int CasterId { get; private set; }

        public virtual int Radius { get; protected set; }

        public Effect(string[] inputs) : base(inputs)
        {
            TimeToFadeOut = int.Parse(inputs[4]);
            CasterId = int.Parse(inputs[5]);
        }
    }



    public class Plan : Effect
    {
        public Plan(string[] inputs) : base(inputs)
        {
            Radius = 2;
            Type = EntityType.EFFECT_PLAN;
        }
    }

    public class Shelter : Effect
    {
        public int RemainingEnergy { get { return TimeToFadeOut; } }
        public Shelter(string[] inputs) : base(inputs)
        {
            Radius = 0;
            Type = EntityType.EFFECT_SHELTER;
        }
    }

    public class Yell : Effect
    {
        public int YelledPlayerID { get; private set; }
        public Yell(string[] inputs) : base(inputs)
        {
            Radius = 1;
            Type = EntityType.EFFECT_YELL;
            YelledPlayerID = int.Parse(inputs[6]);
        }
    }


    public class Light : Effect
    {
        public Light(string[] inputs) : base(inputs)
        {
            Radius = 5;
            Type = EntityType.EFFECT_LIGHT;
        }
    }

}
