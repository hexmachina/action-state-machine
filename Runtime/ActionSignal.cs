using System;
using UnityEngine;

namespace TW.ActionStates
{
	[CreateAssetMenu(fileName = "New Action Signal", menuName = "Data/ActionStates/Action Signal")]
	public class ActionSignal : ScriptableObject
	{

		public static event Action<ActionSignal> OnEnableCallback;

		void OnEnable()
		{
			if (OnEnableCallback != null)
				OnEnableCallback(this);

		}

	}
}