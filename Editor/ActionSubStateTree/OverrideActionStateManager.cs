using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace TW.ActionStates
{
	public class OverrideActionStateManager : IDisposable
	{
		static OverrideActionStateManager m_Instance;
		readonly List<ActionSubState> m_assets = new();

		internal static OverrideActionStateManager instance
		{
			get
			{
				if (m_Instance == null)
				{
					m_Instance = new OverrideActionStateManager();
					m_Instance.Refresh();
				}

				return m_Instance;
			}

			set { m_Instance = value; }
		}

		internal OverrideActionStateManager()
		{
			ActionSubState.OnEnableCallback += Register;
		}

		public static IEnumerable<ActionSubState> assets
		{
			get
			{
				foreach (var asset in instance.m_assets)
				{
					if (asset != null)
						yield return asset;
				}
			}
		}

		public static ActionSubState CreateSignalAssetInstance(string path)
		{
			var newSignal = ScriptableObject.CreateInstance<ActionSubState>();
			newSignal.name = Path.GetFileNameWithoutExtension(path);

			var asset = AssetDatabase.LoadMainAssetAtPath(path) as ActionSubState;
			if (asset != null)
			{
				//TimelineUndo.PushUndo(asset, Styles.UndoCreateSignalAsset);
				EditorUtility.CopySerialized(newSignal, asset);
				Object.DestroyImmediate(newSignal);
				return asset;
			}

			AssetDatabase.CreateAsset(newSignal, path);
			return newSignal;
		}

		public void Dispose()
		{
			ActionSubState.OnEnableCallback -= Register;
		}

		void Register(ActionState a)
		{
			m_assets.Add(a as ActionSubState);
		}

		void Refresh()
		{
			var guids = AssetDatabase.FindAssets($"t:{nameof(ActionSubState)}");
			foreach (var g in guids)
			{
				var path = AssetDatabase.GUIDToAssetPath(g);
				var asset = AssetDatabase.LoadAssetAtPath<ActionSubState>(path);
				m_assets.Add(asset);
			}
		}
	}
}