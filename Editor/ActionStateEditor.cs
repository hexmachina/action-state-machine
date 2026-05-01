using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace TW.ActionStates
{
	[CustomEditor(typeof(ActionState))]
	public class ActionStateEditor : Editor
	{
		private ReorderableList reorderableList;

		SerializedProperty signalsProp;
		ActionState m_Target;
		private void OnEnable()
		{
			m_Target = target as ActionState;
			signalsProp = serializedObject.FindProperty("_signals");
			reorderableList = new ReorderableList(serializedObject, signalsProp, false, true, true, true);
			reorderableList.elementHeight = GetDefaultSpaceBetweenElements();
			reorderableList.drawHeaderCallback += OnDrawReorderListHeader;
			reorderableList.drawElementCallback += OnDrawReorderListElement;
			reorderableList.onAddDropdownCallback += OnReorderListAddDropdown;
			reorderableList.onRemoveCallback += OnRemove;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			reorderableList.DoLayoutList();

			if (serializedObject.hasModifiedProperties)
			{
				serializedObject.ApplyModifiedProperties();
			}
		}

		private void OnDrawReorderListHeader(Rect rect)
		{
			EditorGUI.LabelField(rect, "Signals");
		}

		private void OnDrawReorderListElement(Rect rect, int index, bool isActive, bool isFocused)
		{


			SerializedProperty iteratorProp = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
			var name = iteratorProp.displayName;
			if (iteratorProp.objectReferenceValue)
			{
				name = iteratorProp.objectReferenceValue.name;
			}
			EditorGUI.LabelField(rect, name);
		}


		private void OnReorderListAddDropdown(Rect buttonRect, ReorderableList list)
		{
			GenericMenu menu = new GenericMenu();
			var showTypes = AvailableSignalAssets();

			for (int i = 0; i < showTypes.Count; i++)
			{
				var type = showTypes[i];

				string actionName = showTypes[i].name;


				// TODO: if it is "unique", we must check what we already have to be on or not
				menu.AddItem(new GUIContent(actionName), false, OnAddItemFromDropdown, (object)type);
			}

			menu.ShowAsContext();
		}

		private void OnAddItemFromDropdown(object obj)
		{
			var settingsType = obj as ActionSignal;

			int last = reorderableList.serializedProperty.arraySize;
			reorderableList.serializedProperty.InsertArrayElementAtIndex(last);
			//reordList.
			SerializedProperty lastProp = reorderableList.serializedProperty.GetArrayElementAtIndex(last);
			//lastProp.managedReferenceValue = Activator.CreateInstance(settingsType);
			lastProp.objectReferenceValue = settingsType;
			reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
		}

		private float GetDefaultSpaceBetweenElements()
		{
			return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
		}

		List<ActionSignal> AvailableSignalAssets()
		{
			var ret = ActionSignalManager.assets.Except(m_Target.Signals);
			return ret.ToList();
		}

		private void OnRemove(ReorderableList list)
		{
			var index = list.index;
			reorderableList.serializedProperty.DeleteArrayElementAtIndex(index);

		}
	}
}