using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TW.ActionStates
{
	[System.Serializable]
	public class UnityActionStateOverrideEvent : UnityEvent<ActionSubState> { }

	[DisallowMultipleComponent]
	public class ActionStateController : MonoBehaviour
	{

		[SerializeField] private bool _generateProfile = false;
		[SerializeField]
		private ActionState _generateDefaultState;

		[SerializeField] private ActionStateProfile _profile;

		public ActionStateProfile Profile
		{
			get => _profile;
			set
			{
				if (value == _profile)
				{
					return;
				}
				if (_profile)
				{

					AddListeners();
				}
				_profile = value;
				if (enabled && _profile)
				{
					RemoveListeners();

				}
			}
		}


		public ActionState State
		{
			get => _profile.State;
			set
			{
				if (value == _profile.State)
				{
					return;
				}
				var old = _profile.State;
				if (_profile)
				{

					_profile.State = value;

				}
				else
				{
					ProcessState(value, old);
				}

			}
		}

		public ActionState LastState => Profile != null ? Profile.LastState : null;

		[SerializeField] private List<ActionState> states = new();
		[SerializeField] private List<UnityEvent> onEnterState = new();
		[SerializeField] private List<UnityEvent> onExitState = new();

		[SerializeField] private List<ActionSubState> stateOverrides = new();
		[SerializeField] private List<UnityEvent> onEnterStateOverride = new();
		[SerializeField] private List<UnityEvent> onExitStateOverride = new();

		[SerializeField] private List<ActionSignal> _signals = new();
		[SerializeField] private List<bool> signalStates = new();
		[SerializeField] private List<UnityEvent> onPositiveSignal = new();
		[SerializeField] private List<UnityEvent> onNegativeSignal = new();

		public event Action<ActionStateProfile> onProfileStateChanged;

		private void Awake()
		{
			Init();
		}

		private void Init()
		{
			if (_profile || !_generateProfile)
			{
				return;
			}

			_profile = ActionStateProfile.GenerateProfile(_generateDefaultState, gameObject.name);

		}

		private void OnEnable()
		{
			if (_profile)
			{
				AddListeners();
			}
		}

		private void OnDisable()
		{

			if (_profile)
			{
				RemoveListeners();
			}
		}

		private void AddListeners()
		{
			_profile.OnStateChanged += OnStateChanged;
			_profile.OnSubStateChanged += OnOverrideChanged;
			_profile.OnSubStateAdded += OnOverrideAdded;
			_profile.OnSubStateRemoved += OnOverrideRemoved;
		}

		private void RemoveListeners()
		{
			_profile.OnStateChanged -= OnStateChanged;
			_profile.OnSubStateChanged -= OnOverrideChanged;
			_profile.OnSubStateAdded -= OnOverrideAdded;
			_profile.OnSubStateRemoved -= OnOverrideRemoved;
		}

		public void SetProfileDefaultState(ActionState newState)
		{
			if (!_profile)
				return;
			_profile.DefaultState = newState;
		}

		public void RecieveStateChange(ActionState newState)
		{
			ProcessState(newState, null);

		}

		public void RestoreLastState()
		{
			if (Profile)
			{
				Profile.RestoreLastState();

			}
		}

		public void SetConditionedDefault()
		{
			if (Profile)
			{
				Profile.SetConditionedDefault();
			}
		}

		public void PassProfileByComponent(Component component)
		{
			if (!_profile)
				return;

			if (component.TryGetComponent(out ActionStateController stateController))
			{
				stateController.Profile = _profile;
			}
		}

		private void OnStateChanged(ActionState newState, ActionState oldState)
		{

			ProcessState(newState, oldState);
			onProfileStateChanged?.Invoke(Profile);

		}

		private void OnOverrideChanged()
		{
			ProcessProfile();
		}

		private void OnOverrideRemoved(ActionSubState obj)
		{
			var index = stateOverrides.IndexOf(obj);
			if (index != -1)
			{
				onExitStateOverride[index].Invoke();
			}
		}

		private void OnOverrideAdded(ActionSubState obj)
		{
			var index = stateOverrides.IndexOf(obj);
			if (index != -1)
			{
				onEnterStateOverride[index].Invoke();
			}
		}

		public void ProcessProfile()
		{
			if (!_profile)
			{
				return;
			}
			for (int i = 0; i < _signals.Count; i++)
			{
				var signal = _signals[i];
				if (_profile.TryGetSubStateValueBySignal(signal, out bool value))
				{
					if (value == signalStates[i])
					{
						continue;
					}
					signalStates[i] = value;
					if (value)
					{
						onPositiveSignal[i].Invoke();
					}
					else
					{
						onNegativeSignal[i].Invoke();
					}
				}
				else if (_profile.State.Signals.Contains(signal))
				{
					if (!signalStates[i])
					{
						signalStates[i] = true;
						onPositiveSignal[i].Invoke();
					}
				}
				else if (signalStates[i])
				{
					signalStates[i] = false;
					onNegativeSignal[i].Invoke();
				}
			}
		}

		public void ClearOverride()
		{
			if (_profile)
				_profile.ClearSubStateStack();

		}

		public void AddOverride(ActionSubState actionStateOverride)
		{
			if (!_profile)
			{
				return;
			}
			_profile.AddSubState(actionStateOverride);
		}

		public void RemoveOverride(ActionSubState actionStateOverride)
		{
			if (!_profile)
			{
				return;
			}
			_profile.RemoveSubState(actionStateOverride);
		}

		private void ProcessState(ActionState newState, ActionState oldState)
		{
			var index = -1;
			if (oldState)
			{
				index = states.IndexOf(oldState);
				if (index != -1)
				{
					onExitState[index].Invoke();
				}
			}
			if (newState)
			{
				//ProcessSignalsOverride(newState.signals);
				ProcessProfile();
				index = states.IndexOf(newState);
				if (index != -1)
				{
					onEnterState[index].Invoke();
				}
			}
		}

		public int AddEmptyStateReaction(UnityEvent reaction, UnityEvent exit)
		{
			//m_Events.Append(null, reaction);
			states.Add(null);
			onEnterState.Add(reaction);
			onExitState.Add(exit);
			return onEnterState.Count - 1;
		}

		public int AddEmptyOverrideStateReaction(UnityEvent reaction, UnityEvent exit)
		{
			//m_Events.Append(null, reaction);
			stateOverrides.Add(null);
			onEnterStateOverride.Add(reaction);
			onExitStateOverride.Add(exit);
			return onEnterStateOverride.Count - 1;
		}


		public void RemoveStateAtIndex(int idx)
		{
			if (idx < 0 || idx > states.Count - 1)
				throw new IndexOutOfRangeException();
			states.RemoveAt(idx);
			onEnterState.RemoveAt(idx);
			onExitState.RemoveAt(idx);
		}

		public void RemoveOverrideStateAtIndex(int idx)
		{
			if (idx < 0 || idx > states.Count - 1)
				throw new IndexOutOfRangeException();
			stateOverrides.RemoveAt(idx);
			onEnterStateOverride.RemoveAt(idx);
			onExitStateOverride.RemoveAt(idx);
		}

		public IEnumerable<ActionState> GetRegisteredStates()
		{
			return states;
		}

		public void ChangeStateAtIndex(int idx, ActionState newKey)
		{
			if (idx < 0 || idx > states.Count - 1)
				throw new IndexOutOfRangeException();

			if (states[idx] == newKey)
				return;
			var alreadyUsed = states.Contains(newKey);
			if (newKey == null || states[idx] == null || !alreadyUsed)
				states[idx] = newKey;

			if (alreadyUsed)
				throw new ArgumentException("SignalAsset already used.");
		}

		public ActionState GetStateAssetAtIndex(int idx)
		{
			if (idx < 0 || idx > states.Count - 1)
				throw new IndexOutOfRangeException();
			return states[idx];
		}

		public IEnumerable<ActionSubState> GetRegisteredOverrideStates()
		{
			return stateOverrides;
		}

		public void RemoveStateOverrideAtIndex(int idx)
		{
			if (idx < 0 || idx > stateOverrides.Count - 1)
				throw new IndexOutOfRangeException();
			stateOverrides.RemoveAt(idx);
			onEnterStateOverride.RemoveAt(idx);
			onExitStateOverride.RemoveAt(idx);
		}

		public void ChangeOverrideStateAtIndex(int idx, ActionSubState newKey)
		{
			if (idx < 0 || idx > stateOverrides.Count - 1)
				throw new IndexOutOfRangeException();

			if (stateOverrides[idx] == newKey)
				return;
			var alreadyUsed = stateOverrides.Contains(newKey);
			if (newKey == null || stateOverrides[idx] == null || !alreadyUsed)
				stateOverrides[idx] = newKey;

			if (alreadyUsed)
				throw new ArgumentException("SignalAsset already used.");
		}

		public ActionSubState GetOverrideStateAssetAtIndex(int idx)
		{
			if (idx < 0 || idx > stateOverrides.Count - 1)
				throw new IndexOutOfRangeException();
			return stateOverrides[idx];
		}

		public IEnumerable<ActionSignal> GetRegisteredSignals()
		{
			return _signals;
		}

		public void AddReaction(ActionSignal asset, UnityEvent reaction)
		{
			if (asset == null)
				throw new ArgumentNullException("asset");

			if (_signals.Contains(asset))
				throw new ArgumentException("SignalAsset already used.");
			//m_Events.Append(asset, reaction);
			_signals.Add(asset);
			onPositiveSignal.Add(reaction);
			onNegativeSignal.Add(new UnityEvent());
			signalStates.Add(true);
		}

		public int AddEmptyReaction(UnityEvent reaction)
		{
			//m_Events.Append(null, reaction);
			_signals.Add(null);
			onPositiveSignal.Add(reaction);
			onNegativeSignal.Add(new UnityEvent());
			signalStates.Add(true);
			return onPositiveSignal.Count - 1;
		}

		public UnityEvent GetStateEnterAtIndex(int idx)
		{
			if (idx < 0 || idx > onEnterState.Count - 1)
				throw new IndexOutOfRangeException();
			return onEnterState[idx];
		}

		public UnityEvent GetOverrideStateEnterAtIndex(int idx)
		{
			if (idx < 0 || idx > onEnterStateOverride.Count - 1)
				throw new IndexOutOfRangeException();
			return onEnterStateOverride[idx];
		}

		public UnityEvent GetStateExitAtIndex(int idx)
		{
			if (idx < 0 || idx > onExitState.Count - 1)
				throw new IndexOutOfRangeException();
			return onExitState[idx];
		}

		public void ChangeSignalAtIndex(int idx, ActionSignal newKey)
		{
			if (idx < 0 || idx > _signals.Count - 1)
				throw new IndexOutOfRangeException();

			if (_signals[idx] == newKey)
				return;
			var alreadyUsed = _signals.Contains(newKey);
			if (newKey == null || _signals[idx] == null || !alreadyUsed)
				_signals[idx] = newKey;

			if (alreadyUsed)
				throw new ArgumentException("SignalAsset already used.");
		}



		public UnityEvent GetReactionAtIndex(int idx)
		{
			if (idx < 0 || idx > onPositiveSignal.Count - 1)
				throw new IndexOutOfRangeException();
			return onPositiveSignal[idx];
		}

		public UnityEvent GetNegativeReactionAtIndex(int idx)
		{
			if (idx < 0 || idx > onNegativeSignal.Count - 1)
				throw new IndexOutOfRangeException();
			return onNegativeSignal[idx];
		}

		public ActionSignal GetSignalAssetAtIndex(int idx)
		{
			if (idx < 0 || idx > _signals.Count - 1)
				throw new IndexOutOfRangeException();
			return _signals[idx];
		}

		public void RemoveAtIndex(int idx)
		{
			if (idx < 0 || idx > _signals.Count - 1)
				throw new IndexOutOfRangeException();
			_signals.RemoveAt(idx);
			onPositiveSignal.RemoveAt(idx);
			onNegativeSignal.RemoveAt(idx);
			signalStates.RemoveAt(idx);
		}

		public void Remove(int idx)
		{
			if (idx != -1)
			{
				_signals.RemoveAt(idx);
				onPositiveSignal.RemoveAt(idx);
				onNegativeSignal.RemoveAt(idx);
				signalStates.RemoveAt(idx);
			}
		}

		public void Remove(ActionSignal key)
		{
			var idx = _signals.IndexOf(key);
			if (idx != -1)
			{
				_signals.RemoveAt(idx);
				onPositiveSignal.RemoveAt(idx);
				onNegativeSignal.RemoveAt(idx);
				signalStates.RemoveAt(idx);
			}
		}

#if UNITY_EDITOR
		private void OnGUI()
		{
			if (UnityEditor.Selection.activeTransform != transform || !_profile)
			{
				return;
			}

			if (_profile.State)
			{
				GUI.Label(new Rect(10, 10, 200, 20), _profile.State.name);
			}
			int count = _profile.SubStateStack.Count;
			if (count > 0)
			{
				float offset = 30;
				for (int i = 0; i < count; i++)
				{
					offset += i * 30;
					GUI.Label(new Rect(20, offset, 200, 20), _profile.SubStateStack[i].name);

				}
			}
		}
#endif
	}


}
