using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOfKutulu
{
    public class Explorer : Entity
    {
        public int Sanity { get; private set; }

        public bool IsPlayer { get; private set; }
        public Explorer(string[] inputs, bool isPlayer = false) : base(inputs)
        {
            Type = EntityType.EXPLORER;
            Sanity = int.Parse(inputs[4]);
            int param1 = int.Parse(inputs[5]);
            int param2 = int.Parse(inputs[6]);
            IsPlayer = isPlayer;
        }
    }
}
