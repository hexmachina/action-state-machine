using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace TW.ActionStates
{
	static class ActionStateListFactory
	{
		public static ActionStateTreeView CreateSignalInspectorList(TreeViewState state, ActionHeader header, ActionStateController target, SerializedObject so)
		{
			return new ActionStateTreeView(state, header, target, so);
		}

		public static ActionHeader CreateHeader(MultiColumnHeaderState state, int columnHeight)
		{
			var header = new ActionHeader(state) { height = columnHeight };
			header.ResizeToFit();
			return header;
		}

		public static MultiColumnHeaderState CreateHeaderState()
		{
			return new MultiColumnHeaderState(ActionStateTreeView.GetColumns());
		}

		public static TreeViewState CreateViewState()
		{
			return new TreeViewState();
		}
	}
}