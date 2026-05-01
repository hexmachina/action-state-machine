using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Events;

namespace TW.ActionStates
{
	[CustomEditor(typeof(ActionStateController))]
	public class ActionStateControllerEditor : Editor
	{
		const int k_DefaultTreeviewHeaderHeight = 20;

		ActionStateController m_Target;

		[SerializeField] TreeViewState m_TreeState;
		[SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
		internal ActionSignalTreeView m_SignalTreeView;

		[SerializeField] TreeViewState m_ActionStateTreeState;
		[SerializeField] MultiColumnHeaderState m_ActionStateMultiColumnHeaderState;
		internal ActionStateTreeView m_StateTreeView;
		[SerializeField] TreeViewState m_TreeOverrideState;
		[SerializeField] MultiColumnHeaderState m_OverrideMultiColumnHeaderState;
		internal OverrideActionStateTreeView m_OverrideStateTreeView;
		SerializedProperty generateProfileProp;
		SerializedProperty profileProp;
		SerializedProperty signalProp;
		SerializedProperty statesProp;
		SerializedProperty overrideStatesProp;
		SerializedProperty stateProp;
		SerializedProperty generateDefaultstateProp;

		public List<ActionSubState> stateOverrides = new();
		SerializedProperty stateOverridesProp;

		bool hide;
		private void OnEnable()
		{
			m_Target = target as ActionStateController;
			InitSignalTreeView(serializedObject);
			InitStateTreeView(serializedObject);
			InitOverrideStateTreeView(serializedObject);
			generateProfileProp = serializedObject.FindProperty("_generateProfile");
			hide = generateProfileProp.boolValue;
			profileProp = serializedObject.FindProperty("_profile");
			generateDefaultstateProp = serializedObject.FindProperty("_generateDefaultState");
			signalProp = serializedObject.FindProperty("_signals");
			statesProp = serializedObject.FindProperty("states");
			overrideStatesProp = serializedObject.FindProperty("stateOverrides");
			stateProp = serializedObject.FindProperty("_state");

			Undo.undoRedoPerformed += OnUndoRedo;

		}

		private void OnDisable()
		{
			Undo.undoRedoPerformed -= OnUndoRedo;

		}

		void OnUndoRedo()
		{
			m_SignalTreeView.dirty = true;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(generateProfileProp);
			if (EditorGUI.EndChangeCheck())
			{
				hide = generateProfileProp.boolValue;
			}
			if (hide)
			{
				EditorGUILayout.PropertyField(generateDefaultstateProp);

			}
			EditorGUI.BeginDisabledGroup(hide);
			EditorGUILayout.PropertyField(profileProp);
			EditorGUI.EndDisabledGroup();
			EditorGUI.BeginDisabledGroup(true);
			if (Application.isPlaying)
			{
				ActionState obj = null;

				if (m_Target.Profile)
				{
					obj = m_Target.Profile.State;
					stateOverrides = new(m_Target.Profile.SubStateStack);
					var so = new SerializedObject(this);
					stateOverridesProp = so.FindProperty(nameof(stateOverrides));
					EditorGUILayout.ObjectField("State", obj, typeof(ActionState), false);
					EditorGUILayout.PropertyField(stateOverridesProp, new GUIContent("SubState"));
				}

			}
			EditorGUI.EndDisabledGroup();

			statesProp.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(statesProp.isExpanded, new GUIContent("Action States"));
			if (statesProp.isExpanded)
			{
				using (var changeCheck = new EditorGUI.ChangeCheckScope())
				{
					m_StateTreeView.RefreshIfDirty();
					//DrawEmitterControls(); // Draws buttons coming from the Context (SignalEmitter)

					//EditorGUILayout.Space();
					m_StateTreeView.Draw();

					DrawStateAddRemoveButtons();
					//if (signalEmitterContext == null)

					if (changeCheck.changed)
					{
						serializedObject.ApplyModifiedProperties();
						m_StateTreeView.dirty = true;
					}
				}
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
			overrideStatesProp.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(overrideStatesProp.isExpanded, new GUIContent("Action SubStates"));
			if (overrideStatesProp.isExpanded)
			{
				using (var changeCheck = new EditorGUI.ChangeCheckScope())
				{
					m_OverrideStateTreeView.RefreshIfDirty();
					//DrawEmitterControls(); // Draws buttons coming from the Context (SignalEmitter)

					//EditorGUILayout.Space();
					m_OverrideStateTreeView.Draw();

					DrawOverrideStateAddRemoveButtons();
					//if (signalEmitterContext == null)

					if (changeCheck.changed)
					{
						serializedObject.ApplyModifiedProperties();
						m_OverrideStateTreeView.dirty = true;
					}
				}
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			signalProp.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(signalProp.isExpanded, new GUIContent("Action Signals"));
			if (signalProp.isExpanded)
			{
				using (var changeCheck = new EditorGUI.ChangeCheckScope())
				{
					m_SignalTreeView.RefreshIfDirty();
					//DrawEmitterControls(); // Draws buttons coming from the Context (SignalEmitter)

					//EditorGUILayout.Space();
					m_SignalTreeView.Draw();

					DrawAddRemoveButtons();
					//if (signalEmitterContext == null)

					if (changeCheck.changed)
					{
						serializedObject.ApplyModifiedProperties();
						m_SignalTreeView.dirty = true;
					}
				}
			}
			EditorGUILayout.EndFoldoutHeaderGroup();

			if (serializedObject.hasModifiedProperties)
			{
				serializedObject.ApplyModifiedProperties();

			}
		}

		internal void SetAssetContext(ActionSignal asset)
		{
			m_SignalTreeView.SetSignalContext(asset);
		}

		void DrawAddRemoveButtons()
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				if (GUILayout.Button(EditorGUIUtility.TrTextContent("Add Reaction")))
				{
					Undo.RegisterCompleteObjectUndo(m_Target, L10n.Tr("Add Signal Receiver Reaction"));
					m_Target.AddEmptyReaction(new UnityEvent());
				}
				GUILayout.Space(18.0f);
			}
		}

		void DrawStateAddRemoveButtons()
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				if (GUILayout.Button(EditorGUIUtility.TrTextContent("Add State Event")))
				{
					Undo.RegisterCompleteObjectUndo(m_Target, L10n.Tr("Add Signal Receiver Reaction"));
					m_Target.AddEmptyStateReaction(new UnityEvent(), new UnityEvent());
				}
				GUILayout.Space(18.0f);
			}
		}

		void DrawOverrideStateAddRemoveButtons()
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				if (GUILayout.Button(EditorGUIUtility.TrTextContent("Add SubState Event")))
				{
					Undo.RegisterCompleteObjectUndo(m_Target, L10n.Tr("Add Signal Receiver Reaction"));
					m_Target.AddEmptyOverrideStateReaction(new UnityEvent(), new UnityEvent());
				}
				GUILayout.Space(18.0f);
			}
		}

		void InitSignalTreeView(SerializedObject so)
		{
			m_TreeState = ActionSignalListFactory.CreateViewState();
			m_MultiColumnHeaderState = ActionSignalListFactory.CreateHeaderState();
			var header = ActionSignalListFactory.CreateHeader(m_MultiColumnHeaderState, k_DefaultTreeviewHeaderHeight);

			//var context = signalEmitterContext;
			m_SignalTreeView = ActionSignalListFactory.CreateSignalInspectorList(m_TreeState, header, m_Target, so);
			//m_TreeView.readonlySignals = context != null;

			//if (context != null)
			//m_TreeView.SetSignalContext(context.asset);
		}

		void InitStateTreeView(SerializedObject so)
		{
			m_ActionStateTreeState = ActionStateListFactory.CreateViewState();
			m_ActionStateMultiColumnHeaderState = ActionStateListFactory.CreateHeaderState();
			var header = ActionStateListFactory.CreateHeader(m_ActionStateMultiColumnHeaderState, k_DefaultTreeviewHeaderHeight);

			//var context = signalEmitterContext;
			m_StateTreeView = ActionStateListFactory.CreateSignalInspectorList(m_ActionStateTreeState, header, m_Target, so);
			//m_TreeView.readonlySignals = context != null;

			//if (context != null)
			//m_TreeView.SetSignalContext(context.asset);
		}

		void InitOverrideStateTreeView(SerializedObject so)
		{
			m_TreeOverrideState = OverrideActionStateListFactory.CreateViewState();
			m_ActionStateMultiColumnHeaderState = OverrideActionStateListFactory.CreateHeaderState();
			var header = OverrideActionStateListFactory.CreateHeader(m_ActionStateMultiColumnHeaderState, k_DefaultTreeviewHeaderHeight);

			//var context = signalEmitterContext;
			m_OverrideStateTreeView = OverrideActionStateListFactory.CreateSignalInspectorList(m_TreeOverrideState, header, m_Target, so);
			//m_TreeView.readonlySignals = context != null;

			//if (context != null)
			//m_TreeView.SetSignalContext(context.asset);
		}
	}
}
