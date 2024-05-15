using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIGame.source.Entities;
using AIGame.source.StateMachineNS;

namespace AIGame.source.States
{
    internal class StateEnemyAttack : State
    {
        public StateEnemyAttack(object pOwner):
            base(pOwner) { }

        public override void OnUpdate()
        {
            ((Enemy)GetOwner()).UpdateAttacking();
        }
    }
}
