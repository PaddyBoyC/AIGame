using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIGame.source.StateMachineNS
{
    internal abstract class Transition
    {
        object owner;

        public Transition(object pOwner)
        {
            owner = pOwner;
        }

        public abstract bool ToTransition();

        public object GetOwner()
        {
            return owner;
        }
    }
}
