using System.Collections.Generic;
using UnityEngine;

namespace TW.ActionStates
{
	[CreateAssetMenu(menuName = "Data/Action State/Action Sub State")]
	public class ActionSubState : ActionState, ISerializationCallbackReceiver
	{
		[System.Serializable]
		public struct EnabledActionSignal
		{
			public bool enable;
			public ActionSignal signal;
		}

		[SerializeField] private List<bool> _values = new();

		public Dictionary<ActionSignal, bool> _signalValues = new();
		public IReadOnlyDictionary<ActionSignal, bool> SignalValues => _signalValues;
		public IReadOnlyList<bool> Values => _values;


		public void OnBeforeSerialize()
		{

		}

		public void OnAfterDeserialize()
		{
			_signalValues.Clear();
			for (int i = 0; i < _signals.Count; i++)
			{
				_signalValues.Add(_signals[i], _values[i]);
			}
		}
	}
}
