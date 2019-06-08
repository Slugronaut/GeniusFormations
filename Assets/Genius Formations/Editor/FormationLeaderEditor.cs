using UnityEditor;
using GeniusFormations;
using UnityEngine.AI;
using UnityEngine;

namespace GeniusFormationsEditor
{
    [CustomEditor(typeof(FormationLeader))]
    [CanEditMultipleObjects]
    public class FormationLeaderEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            var leader = target as FormationLeader;

            GUILayout.Space(15);
            EditorGUILayout.ObjectField(serializedObject.FindProperty("Agent"), typeof(NavMeshAgent));
            leader.Formation = EditorGUILayout.ObjectField(new GUIContent("Formation", "The formation asset that defines the local-space positions of the formation followers, relative to the leader."),
                                                           leader.Formation, typeof(FormationAsset), false) as FormationAsset;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("NavAgentDefaultSpeed"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("WaitForGroup"));
            
            
            if (leader.WaitForGroup)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxDrift"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxSpeedCompensaton"));
                EditorGUI.indentLevel--;
            }
            GUILayout.Space(15);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnAllAgentsInPosition"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnLeaderInPosition"));

            serializedObject.ApplyModifiedProperties();
        }
}
}
