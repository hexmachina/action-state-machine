using UnityEngine;
using UnityEngine.Events;

namespace TW.ActionStates
{
	public class ActionSubStateListener : MonoBehaviour
	{
		[SerializeField] ActionSubState targetState;

		private ActionSubState _subState;
		public UnityEvent OnStateEnter = new();
		public UnityEvent OnStateExit = new();
		public void OnOverrideChanged(ActionSubState stateOverride)
		{
			if (_subState != targetState && targetState == stateOverride)
			{
				OnStateEnter.Invoke();
			}
			else if (_subState == targetState && targetState != stateOverride)
			{
				OnStateExit.Invoke();
			}

			_subState = stateOverride;
		}

	}
}