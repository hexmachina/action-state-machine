using System;
using System.Collections.Generic;
using UnityEngine;

namespace TW.ActionStates
{
	[CreateAssetMenu(menuName = "Data/Action State/Action State")]

	public class ActionState : ScriptableObject
	{
		[SerializeField] protected List<ActionSignal> _signals = new();
		public List<ActionSignal> Signals => _signals;

		public static event Action<ActionState> OnEnableCallback;

		void OnEnable()
		{
			if (OnEnableCallback != null)
			{
				OnEnableCallback(this);
			}

		}
	}
}