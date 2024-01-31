using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIGame.source.States
{
    internal class TransitionDead : Transition
    {
        public TransitionDead(object pOwner) :
            base(pOwner)
        {

        }

        public override bool ToTransition()
        {
            Enemy thisEnemy = (Enemy)GetOwner();
            return thisEnemy.Dead;
        }
    }
}
