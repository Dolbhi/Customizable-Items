using UnityEngine;

namespace ColbyDoan
{
	[System.Serializable]
	public class StateMachine
	{
		[SerializeField]
		string currentStateName = "";

		public IState CurrentState { get; private set; }

		public void ChangeState(IState newState)
		{
			CurrentState.Exit();
			CurrentState = newState;
			CurrentState.Enter();
			currentStateName = CurrentState.ToString();
		}

		public void Initialize(IState startState)
		{
			CurrentState = startState;
			CurrentState.Enter();
			currentStateName = CurrentState.ToString();
		}
	}

	public class State<TContainer> : IState
	{
		protected TContainer container;
		protected StateMachine sm;

		public virtual void Init(TContainer _container, StateMachine _sm)
		{
			container = _container;
			sm = _sm;
		}

		public virtual void Enter() { if (container == null) Debug.LogError("State has not been initialized"); }
		public virtual void Exit() { }
		public virtual void Update() { }
	}

	public interface IState
	{
		void Enter();
		void Exit();
		void Update();
	}
}