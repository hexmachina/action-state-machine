using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace TW.ActionStates
{
	public class ActionSignalTreeView : TreeView
	{
		public bool dirty { private get; set; }

		SerializedProperty signals { get; set; }
		SerializedProperty eventsPositive { get; set; }
		SerializedProperty eventsNegative { get; set; }

		readonly ActionStateController m_Target;


		ActionSignal signalAssetContext { get; set; }
		public bool readonlySignals { get; set; }

		const float k_VerticalPadding = 5;
		const float k_HorizontalPadding = 5;

		public ActionSignalTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, ActionStateController receiver, SerializedObject serializedObject)
			: base(state, multiColumnHeader)
		{
			m_Target = receiver;
			useScrollView = true;
			SetSerializedProperties(serializedObject);
			getNewSelectionOverride = (item, selection, shift) => new List<int>(); // Disable Selection
		}

		public void SetSignalContext(ActionSignal assetContext = null)
		{
			signalAssetContext = assetContext;
			dirty = true;
		}

		void SetSerializedProperties(SerializedObject serializedObject)
		{
			signals = serializedObject.FindProperty("_signals");
			eventsPositive = serializedObject.FindProperty("onPositiveSignal");
			eventsNegative = serializedObject.FindProperty("onNegativeSignal");
			Reload();
		}

		public void Draw()
		{
			var rect = EditorGUILayout.GetControlRect(true, GetTotalHeight());
			OnGUI(rect);
		}

		float GetTotalHeight()
		{
			var height = 0.0f;
			foreach (var item in GetRows())
			{
				var signalListItem = item as ActionSignalItem;
				height += signalListItem.GetHeight() + k_VerticalPadding;
			}

			var scrollbarPadding = showingHorizontalScrollBar ? GUI.skin.horizontalScrollbar.fixedHeight : k_VerticalPadding;
			return height + multiColumnHeader.height + scrollbarPadding;
		}

		public void RefreshIfDirty()
		{
			var signalsListSizeHasChanged = signals.arraySize != GetRows().Count;
			if (dirty || signalsListSizeHasChanged)
				Reload();
			dirty = false;
		}

		public static MultiColumnHeaderState.Column[] GetColumns()
		{
			return new[]
			{
				new MultiColumnHeaderState.Column
				{
					headerContent = EditorGUIUtility.TrTextContent("Signal"),
					contextMenuText = "",
					headerTextAlignment = TextAlignment.Center,
					width = 50, minWidth = 50,
					autoResize = true,
					allowToggleVisibility = false,
					canSort = false
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = EditorGUIUtility.TrTextContent("Reaction"),
					contextMenuText = "",
					headerTextAlignment = TextAlignment.Center,
					width = 120, minWidth = 120,
					autoResize = true,
					allowToggleVisibility = false,
					canSort = false
				}
			};
		}

		protected override TreeViewItem BuildRoot()
		{
			var root = new TreeViewItem(-1, -1) { children = new List<TreeViewItem>() };

			var matchingId = signalAssetContext != null && readonlySignals ? FindIdForSignal(signals, signalAssetContext) : -1;
			if (matchingId >= 0)
				AddItem(root, matchingId);

			for (var i = 0; i < signals.arraySize; ++i)
			{
				if (i == matchingId) continue;
				AddItem(root, i, !readonlySignals);
			}

			return root;
		}

		protected override void RowGUI(RowGUIArgs args)
		{
			var item = (ActionSignalItem)args.item;
			for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
			{
				var rect = args.GetCellRect(i);
				rect.y += k_VerticalPadding;
				item.Draw(rect, args.GetColumn(i), args.row, k_HorizontalPadding, m_Target);
			}
		}

		protected override float GetCustomRowHeight(int row, TreeViewItem treeItem)
		{
			var item = treeItem as ActionSignalItem;
			return item.GetHeight() + k_VerticalPadding;
		}

		void AddItem(TreeViewItem root, int id, bool enabled = true)
		{
			var signal = signals.GetArrayElementAtIndex(id);
			var evt = eventsPositive.GetArrayElementAtIndex(id);
			var evtNeg = eventsNegative.GetArrayElementAtIndex(id);
			root.children.Add(new ActionSignalItem(signal, evt, evtNeg, id, readonlySignals, enabled, this));
		}

		static int FindIdForSignal(SerializedProperty signals, ActionSignal signalToFind)
		{
			for (var i = 0; i < signals.arraySize; ++i)
			{
				//signal in the receiver that matches the current signal asset will be displayed first
				var serializedProperty = signals.GetArrayElementAtIndex(i);
				var signalReferenceValue = serializedProperty.objectReferenceValue;
				var signalToFindRefValue = signalToFind;
				if (signalReferenceValue != null && signalReferenceValue == signalToFindRefValue)
					return i;
			}
			return -1;
		}
	}
}