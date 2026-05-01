using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace TW.ActionStates
{
	public class ActionStateManager : IDisposable
	{
		static ActionStateManager m_Instance;
		readonly List<ActionState> m_assets = new List<ActionState>();

		internal static ActionStateManager instance
		{
			get
			{
				if (m_Instance == null)
				{
					m_Instance = new ActionStateManager();
					m_Instance.Refresh();
				}

				return m_Instance;
			}

			set { m_Instance = value; }
		}

		internal ActionStateManager()
		{
			ActionState.OnEnableCallback += Register;
		}

		public static IEnumerable<ActionState> assets
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

		public static ActionState CreateSignalAssetInstance(string path)
		{
			var newSignal = ScriptableObject.CreateInstance<ActionState>();
			newSignal.name = Path.GetFileNameWithoutExtension(path);

			var asset = AssetDatabase.LoadMainAssetAtPath(path) as ActionState;
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
			ActionState.OnEnableCallback -= Register;
		}

		void Register(ActionState a)
		{
			m_assets.Add(a);
		}

		void Refresh()
		{
			var guids = AssetDatabase.FindAssets("t:ActionState");
			foreach (var g in guids)
			{
				var path = AssetDatabase.GUIDToAssetPath(g);
				var asset = AssetDatabase.LoadAssetAtPath<ActionState>(path);
				m_assets.Add(asset);
			}
		}
	}
}