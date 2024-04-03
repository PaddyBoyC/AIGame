using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIGame.source.StateMachineNS;

namespace AIGame.source.States
{
    internal class TransitionAnimationComplete : Transition
    {
        Animation animation;

        public TransitionAnimationComplete(object pOwner, Animation pAnimation) :
            base(pOwner)
        {
            animation = pAnimation;
        }

        public override bool ToTransition()
        {
            return animation.AtEnd();
        }
    }
}
