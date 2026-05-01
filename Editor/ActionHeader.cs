using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace TW.ActionStates
{

	public class ActionHeader : MultiColumnHeader
	{
		public ActionHeader(MultiColumnHeaderState state) : base(state) { }

		protected override void AddColumnHeaderContextMenuItems(GenericMenu menu)
		{
			menu.AddItem(EditorGUIUtility.TrTextContent("Resize to Fit"), false, ResizeToFit);
		}
	}
}