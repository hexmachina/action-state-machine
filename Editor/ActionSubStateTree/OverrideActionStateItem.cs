using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;


namespace TW.ActionStates
{
	public class OverrideActionStateItem : TreeViewItem
	{
		static readonly UnityEventDrawer k_EvtDrawer = new UnityEventDrawer();
		readonly SerializedProperty m_Asset;
		readonly SerializedProperty m_Evt;
		readonly SerializedProperty m_EvtNeg;
		readonly OverrideActionStateTreeView m_TreeView;

		//ActionState signalAsset { get; set; }
		public ActionSubState signalAsset
		{
			get { return m_CurrentReceiver.GetOverrideStateAssetAtIndex(m_CurrentRowIdx); }
			set
			{
				Undo.RegisterCompleteObjectUndo(m_CurrentReceiver, L10n.Tr("Create New Signal Asset"));
				m_CurrentReceiver.ChangeOverrideStateAtIndex(m_CurrentRowIdx, value);
			}
		}

		int m_CurrentRowIdx;
		ActionStateController m_CurrentReceiver;

		internal readonly bool enabled;
		internal readonly bool readonlySignal;


		internal const string SignalName = "SignalName";
		internal const string SignalNameReadOnly = "SignalNameReadOnly";
		internal const string SignalOptions = "SignalOptions";

		const string k_SignalExtension = "asset";

		public OverrideActionStateItem(SerializedProperty signalAsset, SerializedProperty eventPositive, SerializedProperty eventNegative, int id, bool readonlySignal, bool enabled, OverrideActionStateTreeView treeView)
			: base(id, 0)
		{
			m_Asset = signalAsset;
			m_Evt = eventPositive;
			m_EvtNeg = eventNegative;
			this.enabled = enabled;
			this.readonlySignal = readonlySignal;
			m_TreeView = treeView;
		}

		void DrawOptionsButton(Rect rect, int rowIdx, ActionStateController target)
		{
			GUI.SetNextControlName(SignalOptions);
			if (EditorGUI.DropdownButton(rect, EditorGUIUtility.IconContent("SettingsIcon"), FocusType.Passive, GUIStyle.none))
			{
				var menu = new GenericMenu();
				menu.AddItem(new GUIContent(L10n.Tr("Duplicate")), false, () =>
				{
					Undo.RegisterCompleteObjectUndo(target, "Duplicate Row");
					var evtCloner = ScriptableObject.CreateInstance<UnityEventCloner>();
					evtCloner.evt = target.GetOverrideStateEnterAtIndex(rowIdx);
					var clone = Object.Instantiate(evtCloner);
					var evtCloner1 = ScriptableObject.CreateInstance<UnityEventCloner>();
					evtCloner1.evt = target.GetOverrideStateEnterAtIndex(rowIdx);
					var clone1 = Object.Instantiate(evtCloner1);
					target.AddEmptyOverrideStateReaction(clone.evt, clone1.evt);
					m_TreeView.dirty = true;
				});
				menu.AddItem(new GUIContent(L10n.Tr("Delete")), false, () =>
				{
					Undo.RegisterCompleteObjectUndo(target, "Delete Row");
					target.RemoveOverrideStateAtIndex(rowIdx);
					m_TreeView.dirty = true;
				});
				menu.ShowAsContext();
			}
		}

		public float GetHeight()
		{
			var height = k_EvtDrawer.GetPropertyHeight(m_Evt, GUIContent.none);
			height += k_EvtDrawer.GetPropertyHeight(m_EvtNeg, GUIContent.none);
			return height;
		}

		public void Draw(Rect rect, int colIdx, int rowIdx, float padding, ActionStateController target)
		{
			switch (colIdx)
			{
				case 0:
					DrawSignalNameColumn(rect, padding, target, rowIdx);
					break;
				case 1:
					DrawReactionColumn(rect, rowIdx);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		void DrawSignalNameColumn(Rect rect, float padding, ActionStateController target, int rowIdx)
		{
			using (new EditorGUI.DisabledScope(!enabled))
			{
				if (!readonlySignal)
				{
					m_CurrentRowIdx = rowIdx;
					m_CurrentReceiver = target;

					rect.x += padding;
					rect.width -= padding;
					rect.height = EditorGUIUtility.singleLineHeight;
					GUI.SetNextControlName(SignalName);
					DrawSignalNames(rect, GUIContent.none, false);
				}
				else
				{
					GUI.SetNextControlName(SignalNameReadOnly);
					var signalAsset = m_Asset.objectReferenceValue;
					GUI.Label(rect,
						signalAsset != null
						? new GUIContent(signalAsset.name)
						//? EditorGUIUtility.TempContent(signalAsset.name)
						: EditorGUIUtility.TrTextContent("None"));
				}
			}
		}

		void DrawReactionColumn(Rect rect, int rowIdx)
		{
			if (!readonlySignal)
			{
				//var optionButtonSize = GetOptionButtonSize();
				var optionButtonSize = new Vector2(14, 14);
				rect.width -= optionButtonSize.x;

				var optionButtonRect = new Rect
				{
					x = rect.xMax,
					y = rect.y,
					width = optionButtonSize.x,
					height = optionButtonSize.y
				};
				DrawOptionsButton(optionButtonRect, rowIdx, m_CurrentReceiver);
			}

			using (new EditorGUI.DisabledScope(!enabled))
			{
				//var nameAsString = m_Asset.objectReferenceValue == null ? "Null" : m_Asset.objectReferenceValue.name;
				using (var change = new EditorGUI.ChangeCheckScope())
				{
					EditorGUI.PropertyField(rect, m_Evt, new GUIContent("On Enter"));
					//EditorGUI.PropertyField(rect, m_Evt);
					if (change.changed)
						m_TreeView.dirty = true;
				}

				using (var change = new EditorGUI.ChangeCheckScope())
				{
					rect.y += k_EvtDrawer.GetPropertyHeight(m_Evt, GUIContent.none);
					EditorGUI.PropertyField(rect, m_EvtNeg, new GUIContent("On Exit"));
					//EditorGUI.PropertyField(rect, m_Evt);
					if (change.changed)
						m_TreeView.dirty = true;
				}
			}


		}

		class UnityEventCloner : ScriptableObject
		{
			public UnityEvent evt;
		}

		public void DrawSignalNames(Rect position, GUIContent label, bool multipleValues)
		{
			var assets = AvailableSignalAssets(signalAsset).ToList();
			var index = assets.IndexOf(signalAsset);

			var availableNames = new List<string>();
			availableNames.Add("None");

			availableNames.AddRange(assets.Select(x => x.name));
			availableNames.Add("Create Signal…");

			var curValue = index + 1;
			var selected = EditorGUI.Popup(position, label.text, curValue, availableNames.ToArray());
			//EditorGUI.Popup()
			if (selected != curValue)
			{
				var noneEntryIdx = 0;
				if (selected == noneEntryIdx) // None
					signalAsset = null;
				else if (selected == availableNames.Count - 1) // "Create New Asset"
				{
					var path = GetNewSignalPath();
					if (!string.IsNullOrEmpty(path))
						CreateNewSignalAsset(path);
					GUIUtility.ExitGUI();
				}
				else
					signalAsset = assets[selected - 1];
			}
			//using (new GUIMixedValueScope(multipleValues))
			//{
			//}
		}

		public static string GetNewSignalPath()
		{
			return EditorUtility.SaveFilePanelInProject(
				"Create Action State",
				"New Signal",
				k_SignalExtension,
				"Create Action State");
		}

		IEnumerable<ActionSubState> AvailableSignalAssets(ActionSubState signalAsset)
		{
			var ret = OverrideActionStateManager.assets.Except(m_CurrentReceiver.GetRegisteredOverrideStates());
			return signalAsset == null ? ret : ret.Union(new List<ActionSubState> { signalAsset }).ToList();
		}

		void CreateNewSignalAsset(string path)
		{
			var newSignalAsset = OverrideActionStateManager.CreateSignalAssetInstance(path);
			m_CurrentReceiver.ChangeOverrideStateAtIndex(m_CurrentRowIdx, newSignalAsset);
		}
	}
}