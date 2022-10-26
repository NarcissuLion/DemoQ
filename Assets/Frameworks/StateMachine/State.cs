using Framework.Utils;
using Framework.FMath;

namespace Framework.SM
{
    public abstract class State
    {
        protected StateMachine m_machine;
        public State(StateMachine machine, ITuple instParams = null)
        {
            m_machine = machine;
        }

        public abstract string Name { get; }

        public abstract void Dispose();

        public abstract void Enter(ITuple enterParams, string preStateName);

        public abstract void Exit(string nextStateName);

        public abstract void Update(float deltaTime);
        public abstract void FixedTick(Fix64 deltaTime);
        public abstract void LateUpdate(float deltaTime);
    }
}
