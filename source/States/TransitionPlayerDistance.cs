using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace AIGame.source.States
{
    internal class TransitionPlayerDistance : Transition
    {
        Player player;
        float distance;
        bool lessThan;

        public TransitionPlayerDistance(object pOwner, Player pPlayer, float pDistance, bool pLessThan) :
            base(pOwner)
        {
            player = pPlayer;
            distance = pDistance;
            lessThan = pLessThan;
        }

        public override bool ToTransition()
        {
            Entity thisEntity = (Entity)GetOwner();
            float distanceToPlayer = (thisEntity.position - player.position).Length();
            if (lessThan)
            {
                return distanceToPlayer < distance;
            }
            else
            {
                return distance < distanceToPlayer;
            }
        }
    }
}
