using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace TW.ActionStates
{
	static class OverrideActionStateListFactory
	{
		public static OverrideActionStateTreeView CreateSignalInspectorList(TreeViewState state, ActionHeader header, ActionStateController target, SerializedObject so)
		{
			return new OverrideActionStateTreeView(state, header, target, so);
		}

		public static ActionHeader CreateHeader(MultiColumnHeaderState state, int columnHeight)
		{
			var header = new ActionHeader(state) { height = columnHeight };
			header.ResizeToFit();
			return header;
		}

		public static MultiColumnHeaderState CreateHeaderState()
		{
			return new MultiColumnHeaderState(OverrideActionStateTreeView.GetColumns());
		}

		public static TreeViewState CreateViewState()
		{
			return new TreeViewState();
		}
	}
}