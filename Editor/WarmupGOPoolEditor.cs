using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace LiteNinja.Pooling.Editor
{
    [CustomEditor(typeof(WarmupGOPool))]
    public class WarmupGOPoolEditor : UnityEditor.Editor
    {
        private ReorderableList list;

        private void OnEnable()
        {
            list = new ReorderableList(serializedObject, serializedObject.FindProperty("items"), 
                true, true, true, true);

            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                const int numberRectWidth = 100;

                var item = list.serializedProperty.GetArrayElementAtIndex(index);

                var prefabRect = rect;
                prefabRect.width -= numberRectWidth + 3;
                var amountRect = rect;
                amountRect.width = numberRectWidth;
                amountRect.x = prefabRect.x + prefabRect.width + 3;

                EditorGUI.PropertyField(prefabRect, item.FindPropertyRelative("prefab"), GUIContent.none);
                EditorGUI.PropertyField(amountRect, item.FindPropertyRelative("amount"), GUIContent.none);
            };

            list.drawHeaderCallback = (rect) => { GUI.Label(rect, "Prefabs"); };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("createOnAwake"));
            EditorGUILayout.Space(8);
            list.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}