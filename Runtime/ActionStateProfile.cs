using System;
using System.Collections.Generic;
using UnityEngine;

namespace TW.ActionStates
{
	[CreateAssetMenu(menuName = "Data/Action State/Profile")]
	public class ActionStateProfile : ScriptableObject
	{
		[SerializeField] ActionState _defaultState;

		[NonSerialized] private readonly List<ActionStateController> _controllers = new();
		[NonSerialized] private readonly List<ActionSubState> _subStateStack = new();

		[NonSerialized] private ActionState _state;
		[NonSerialized] private ActionState _lastState;

		public event Action<ActionState, ActionState> OnStateChanged;
		public event Action OnSubStateChanged;
		public event Action<ActionSubState> OnSubStateAdded;
		public event Action<ActionSubState> OnSubStateRemoved;

		public ActionState DefaultState { get => _defaultState; set => _defaultState = value; }
		public ActionState State
		{
			get
			{
				if (_state == null)
				{
					_state = _defaultState;
				}
				return _state;
			}
			set
			{
				if (value == _state)
					return;
				if (!_state)
				{
					_lastState = DefaultState;
				}
				else
				{
					_lastState = _state;
				}
				_state = value;
				OnStateChanged?.Invoke(_state, _lastState);
			}
		}

		public ActionState LastState => _lastState;

		public IReadOnlyList<ActionSubState> SubStateStack => _subStateStack;

		public void Register(ActionStateController controller)
		{
			if (!_state)
			{
				_state = _defaultState;
			}
			if (_controllers.Contains(controller))
			{
				return;
			}

			_controllers.Add(controller);
		}

		public void Unregister(ActionStateController controller)
		{
			if (!_controllers.Contains(controller))
			{
				return;
			}

			_controllers.Remove(controller);
		}

		public void BroadcastStateChange(ActionState state, ActionStateController controller)
		{
			foreach (var item in _controllers)
			{
				if (item != controller)
				{
					item.RecieveStateChange(state);
				}
			}
		}

		public void SetConditionedDefault()
		{
			_lastState = _state;
			_state = _defaultState;
			OnStateChanged?.Invoke(_state, _lastState);
		}

		public void RestoreLastState()
		{
			if (!_lastState || _lastState == _state)
			{
				return;
			}

			var temp = _state;
			_state = _lastState;
			_lastState = _defaultState;
			OnStateChanged?.Invoke(_state, temp);
		}

		public void ClearSubStateStack()
		{
			if (_subStateStack.Count == 0)
			{
				return;
			}
			foreach (var item in _subStateStack)
			{
				OnSubStateRemoved?.Invoke(item);
			}
			_subStateStack.Clear();
			OnSubStateChanged?.Invoke();
		}

		public void ReplaceAllSubStates(ActionSubState subState)
		{
			ClearSubStateStack();
			AddSubState(subState);
		}

		public void AddSubState(ActionSubState subState)
		{
			if (subState == null || _subStateStack.Contains(subState))
			{
				return;
			}
			_subStateStack.Add(subState);
			OnSubStateAdded?.Invoke(subState);
			OnSubStateChanged?.Invoke();

		}

		public void RemoveSubState(ActionSubState subState)
		{
			if (!_subStateStack.Contains(subState))
			{
				return;
			}
			_subStateStack.Remove(subState);
			OnSubStateRemoved?.Invoke(subState);
			OnSubStateChanged?.Invoke();

		}

		public bool TryGetSubStateValueBySignal(ActionSignal signal, out bool value)
		{
			value = false;
			foreach (var subState in _subStateStack)
			{
				if (subState.SignalValues.TryGetValue(signal, out value))
				{
					return true;
				}
			}
			return false;
		}

		public static ActionStateProfile GenerateProfile(ActionState defaultState, string name = "Profile")
		{
			var prof = CreateInstance<ActionStateProfile>();
			prof.DefaultState = defaultState;
			prof.name = name;
			return prof;
		}
	}
}