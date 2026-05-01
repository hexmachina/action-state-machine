using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace TW.ActionStates
{
	public class ActionSignalManager : IDisposable
	{
		static ActionSignalManager m_Instance;
		readonly List<ActionSignal> m_assets = new List<ActionSignal>();

		internal static ActionSignalManager instance
		{
			get
			{
				if (m_Instance == null)
				{
					m_Instance = new ActionSignalManager();
					m_Instance.Refresh();
				}

				return m_Instance;
			}

			set { m_Instance = value; }
		}

		internal ActionSignalManager()
		{
			ActionSignal.OnEnableCallback += Register;
		}

		public static IEnumerable<ActionSignal> assets
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

		public static ActionSignal CreateSignalAssetInstance(string path)
		{
			var newSignal = ScriptableObject.CreateInstance<ActionSignal>();
			newSignal.name = Path.GetFileNameWithoutExtension(path);

			var asset = AssetDatabase.LoadMainAssetAtPath(path) as ActionSignal;
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
			ActionSignal.OnEnableCallback += Register;
		}

		void Register(ActionSignal a)
		{
			m_assets.Add(a);
		}

		void Refresh()
		{
			var guids = AssetDatabase.FindAssets("t:ActionSignal");
			foreach (var g in guids)
			{
				var path = AssetDatabase.GUIDToAssetPath(g);
				var asset = AssetDatabase.LoadAssetAtPath<ActionSignal>(path);
				m_assets.Add(asset);
			}
		}
	}
}