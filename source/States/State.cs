using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIGame.source.States
{
    public class State
    {
        object owner;
        
        public State(object pOwner)
        {
            owner = pOwner;
        }

        public virtual void OnUpdate()
        {

        }

        protected object GetOwner()
        {
            return owner;
        }
    }
}
