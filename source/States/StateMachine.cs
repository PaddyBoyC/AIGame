using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIGame.source.States
{
    internal class StateMachine
    {
        State currentState;

        Dictionary<State, List<(Transition, State)>> stateTransitions = new ();

        public StateMachine(State currentState, Dictionary<State, List<(Transition, State)>> stateTransitions)
        {
            this.currentState = currentState;
            this.stateTransitions = stateTransitions;
        }

        void SetState(State newState)
        {
            currentState = newState;
        }

        public State GetState => currentState;

        public void Update()
        {
            if (stateTransitions.ContainsKey(currentState))
            {
                foreach (var transition in stateTransitions[currentState])
                {
                    if (transition.Item1.ToTransition())
                    {
                        SetState(transition.Item2);
                    }
                }
            }
            currentState.OnUpdate();
        }
    }
}
