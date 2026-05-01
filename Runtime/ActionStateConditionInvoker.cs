using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TW.ActionStates
{
	[RequireComponent(typeof(ActionStateController))]
	public class ActionStateConditionInvoker : MonoBehaviour
	{
		[System.Serializable]
		public class SignalCondition
		{
			public ActionSignal signal;
			public UnityEvent<bool> onSignalFound = new();
		}
		[SerializeField] private ActionStateController _controller;

		public List<SignalCondition> signalConditions = new();


		private void OnEnable()
		{

			if (_controller)
			{
				_controller.onProfileStateChanged += OnProfileStateChanged;
			}
		}

		private void OnProfileStateChanged(ActionStateProfile obj)
		{
			var state = obj.DefaultState;
			for (int i = 0; i < signalConditions.Count; i++)
			{
				signalConditions[i].onSignalFound.Invoke(state.Signals.Contains(signalConditions[i].signal));
			}
		}

		private void OnDisable()
		{
			if (_controller)
			{
				_controller.onProfileStateChanged -= OnProfileStateChanged;
			}
		}

	}
}