using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeOfKutulu
{

    public class Wanderer : Entity
    {
        public enum State
        {
            SPAWNING,
            WANDERING,
            STALKING,
            RUSHING,
            STUNNED
        }
        private int _time;
        public int TimeTillSpawn
        {
            get
            {
                return _time;
            }
        }

        public int TimeBeforeRecall
        {
            get
            {
                return _time;
            }
        }

        public int TargetedExplorerID { get; private set; }

        public Explorer TargetedExplorer
        {
            get
            {
                if (TargetedExplorerID == -1)
                    return null;
                return (Explorer)Gameboard.Entities.Single(x => x.ID == TargetedExplorerID);
            }
        }

        public State CurrentState { get; private set; }
        public bool IsSlasher { get; private set; }
        public Wanderer(string[] inputs, bool isSlasher = false) : base(inputs)
        {
            Type = EntityType.WANDERER;
            _time = int.Parse(inputs[4]);
            CurrentState = (State)int.Parse(inputs[5]);
            TargetedExplorerID = int.Parse(inputs[6]);
            if (isSlasher)
            {
                IsSlasher = true;
            }
        }
    }
}
