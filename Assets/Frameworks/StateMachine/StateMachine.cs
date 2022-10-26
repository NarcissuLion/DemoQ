using Framework.Log;
using Framework.Utils;
using Framework.FMath;
using System.Collections.Generic;
using System;

namespace Framework.SM
{
    public class StateMachine
    {
        private Dictionary<string, State> m_states = new Dictionary<string, State>();
        private State m_currState;
        private object m_owner;
        public object owner { get { return m_owner; } }

        public StateMachine(object owner)
        {
            m_owner = owner;
        }

        public string currStateName
        {
            get { return m_currState == null ? string.Empty : m_currState.Name; }
        }

        public State currState
        {
            get { return m_currState; }
        }

        private State FindState(string stateName)
        {
            State state;
            if (!m_states.TryGetValue(stateName, out state))
            {
                Logger.LogError("StateMachine->RemoveState can't find state : " + stateName);
            }
            return state;
        }
        
        public void AddState<T>(ITuple instParams = null) where T : State
        {
            T state = (T)Activator.CreateInstance(typeof(T), this, instParams);
            if (instParams != null) instParams.Close();
            if (m_states.ContainsKey(state.Name))
            {
                throw new System.Exception("StateMachine->AddState state already added : " + state.Name);
            }
            m_states.Add(state.Name, state);
        }

        public void RemoveState(string stateName)
        {
            RemoveState(FindState(stateName));
        }
        private void RemoveState(State state)
        {
            if (state == null) return;
            if (m_currState == state)
            {
                Logger.LogError("StateMachine->RemoveState can't remove current state : " + state.Name);
            }
            else if (m_states.ContainsKey(state.Name))
            {
                m_states.Remove(state.Name);
            }
        }

        public void ChangeState(string stateName, ITuple enterParams = null, bool reEnterSelf = true)
        {
            State nextState = FindState(stateName);

            if (nextState == null)
            {
                Logger.LogError("Can't find state : " + stateName);
                return;
            }

            string preStateName = m_currState == null ? string.Empty : m_currState.Name;
            string nextStateName = nextState == null ? string.Empty : nextState.Name;

            if (!reEnterSelf && preStateName == nextStateName) return;

            if (m_currState != null)
            {
                m_currState.Exit(nextStateName);
            }
            m_currState = nextState;
            if (m_currState != null)
            {
                m_currState.Enter(enterParams, preStateName);
            }
            if (enterParams != null) enterParams.Close();
        }

        public void Update(float deltaTime)
        {
            if (m_currState != null)
            {
                m_currState.Update(deltaTime);
            }
        }

        public void FixedTick(Fix64 deltaTime)
        {
            if (m_currState != null)
            {
                m_currState.FixedTick(deltaTime);
            }
        }

        public void LateUpdate(float deltaTime)
        {
            if (m_currState != null)
            {
                m_currState.LateUpdate(deltaTime);
            }
        }

        public void Dispose()
        {
            if (m_currState != null)
            {
                m_currState.Exit(string.Empty);
                m_currState = null;
            }
            var e = m_states.GetEnumerator();
            while (e.MoveNext())
            {
                e.Current.Value.Dispose();
            }
            m_states.Clear();
        }
    }
}
