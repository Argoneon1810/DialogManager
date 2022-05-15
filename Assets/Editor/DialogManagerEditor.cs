using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace DialogManager {
    [CustomEditor(typeof(DialogManager))]
    public class DialogManagerEditor : Editor {
        DialogManager script;
        AnimBool showFadeMargin;

        void OnEnable() {
            showFadeMargin = new AnimBool(false);
            showFadeMargin.valueChanged.AddListener(Repaint);
        }

        public override void OnInspectorGUI() {
            if(script == null) script = (DialogManager) target;

            showFadeMargin.target  = EditorGUILayout.Foldout(showFadeMargin.target, "Name Tag Margin/Padding Control");
            if(EditorGUILayout.BeginFadeGroup(showFadeMargin.faded)) {
                ++EditorGUI.indentLevel;
                script.bApplyMarginToNameTag = EditorGUILayout.Toggle("Apply Margin", script.bApplyMarginToNameTag);
                script.bApplyPaddingToNameTag = EditorGUILayout.Toggle("Apply Padding", script.bApplyPaddingToNameTag);
                --EditorGUI.indentLevel;
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Target Canvas to Draw Dialog");
                script.targetCanvasToDrawDialog = EditorGUILayout.ObjectField(script.targetCanvasToDrawDialog, typeof(Canvas), true) as Canvas;
            EditorGUILayout.EndHorizontal();

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("backgrounds"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dialogs"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
