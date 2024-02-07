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
        bool facingMatters;

        public TransitionPlayerDistance(object pOwner, Player pPlayer, float pDistance, bool pLessThan, bool pFacingMatters) :
            base(pOwner)
        {
            player = pPlayer;
            distance = pDistance;
            lessThan = pLessThan;
            facingMatters = pFacingMatters; 
        }

        public override bool ToTransition()
        {
            Enemy thisEnemy = GetOwner() as Enemy;
            if (thisEnemy != null && facingMatters && thisEnemy.IsFacingRight == thisEnemy.position.X > player.position.X)
            {
                return false;
            }
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
